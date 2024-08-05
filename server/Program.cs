using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpServer
{
    private static List<TcpClient> clients = new List<TcpClient>();
    private static object clientsLock = new object();
    private static bool isBroadcastLoopRunning = false;

    static void Main()
    {
        int port = 13000;
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine("Server started and waiting for connection...");

        // Start the broadcast loop once
        StartBroadcastLoop();

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            lock (clientsLock)
            {
                clients.Add(client);
            }

            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    static void StartBroadcastLoop()
    {
        lock (clientsLock)
        {
            if (!isBroadcastLoopRunning)
            {
                isBroadcastLoopRunning = true;
                Thread broadcastLoopTest = new Thread(BroadcastLoopTest);
                broadcastLoopTest.Start();
            }
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        try
        {
            Thread clientThreadListen = new Thread(HandleClientListen);
            clientThreadListen.Start(client);

            Thread clientThreadTalk = new Thread(HandleClientTalk);
            clientThreadTalk.Start(client);

            clientThreadListen.Join();
            clientThreadTalk.Join();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Client error: " + ex.Message);
        }
        finally
        {
            lock (clientsLock)
            {
                clients.Remove(client);
            }
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }

    static void HandleClientListen(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        try
        {
            byte[] buffer = new byte[256];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + receivedMessage);

                //BroadcastMessage(receivedMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Listen error: " + ex.Message);
        }
    }

    static void HandleClientTalk(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        try
        {
            string responseMessage = "Hello from server!";
            byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
            stream.Write(responseBytes, 0, responseBytes.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Talk error: " + ex.Message);
        }
    }

    static void BroadcastLoopTest()
    {
        int count = 0;
        while (true)
        {
            count++;
            BroadcastMessage("test" + count);
            Thread.Sleep(1000);
        }
    }

    static void BroadcastMessage(string message)
    {
        lock (clientsLock)
        {
            byte[] responseBytes = Encoding.ASCII.GetBytes(message);
            List<TcpClient> disconnectedClients = new List<TcpClient>();

            foreach (var client in clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
                catch (Exception)
                {
                    disconnectedClients.Add(client);
                }
            }

            foreach (var client in disconnectedClients)
            {
                clients.Remove(client);
                client.Close();
                Console.WriteLine("Removed disconnected client.");
            }
        }
    }
}
