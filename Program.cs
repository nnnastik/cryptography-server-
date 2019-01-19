/*
* Filename: Program.cs
* Author:   Korneva Anastasiia
* Date:     23.02.2018
* This file contains all functionality for server side of chat application.
* It creates connection, keep track of users` connections, recive information from users and broadcasts messages for users and send them message.
*  Reference:
   https://stackoverflow.com/questions/43431196/c-sharp-tcp-ip-simple-chat-with-multiple-clients
*/



using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Reflection;



namespace server
{
    class Program
    {
        const int bufLen = 1024;
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();

        static void Main(string[] args)
        {
            int count = 1;

            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
            ServerSocket.Start();

            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                lock (_lock) list_clients.Add(count, client);
                Console.WriteLine("Someone connected!!");

                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
            }
        }


        /// <summary>
        /// provides connection for client and recive information from clients
        /// </summary>
        /// <param name="o"></param>
        public static void handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client;

            lock (_lock) client = list_clients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[bufLen];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(data);
                Console.WriteLine(data);
            }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }



        /// <summary>
        /// process recived data
        /// </summary>
        /// <param name="data">data recived</param>
        public static void broadcast(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            Console.WriteLine("Chat: " + buffer);
        }
    }
}
