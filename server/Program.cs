using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpServer
{
    // Define a class to hold client information
    private class ClientInfo
    {
        public TcpClient Client { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public ClientInfo(TcpClient client, int x, int y)
        {
            Client = client;
            X = x;
            Y = y;
        }
    }

    // Updated list to hold ClientInfo objects
    private static List<ClientInfo> clients = new List<ClientInfo>();
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

            // Assign default positions for this example (0, 0)
            var clientInfo = new ClientInfo(client, 0, 0);

            lock (clientsLock)
            {
                clients.Add(clientInfo);
            }

            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(clientInfo);
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
        ClientInfo clientInfo = (ClientInfo)obj;
        NetworkStream stream = clientInfo.Client.GetStream();

        try
        {
            Thread clientThreadListen = new Thread(HandleClientListen);
            clientThreadListen.Start(clientInfo);

            Thread clientThreadTalk = new Thread(HandleClientTalk);
            clientThreadTalk.Start(clientInfo);

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
                clients.Remove(clientInfo);
            }
            clientInfo.Client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }

    static void HandleClientListen(object obj)
    {
        ClientInfo clientInfo = (ClientInfo)obj;
        TcpClient client = clientInfo.Client;
        NetworkStream stream = client.GetStream();

        // Get client IP address and port
        string clientEndPoint = client.Client.RemoteEndPoint.ToString();

        try
        {
            byte[] buffer = new byte[256];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received from {clientEndPoint}: {receivedMessage}");
                Console.WriteLine($"posx: {clientInfo.X} posy: {clientInfo.Y}");

                // BroadcastMessage(receivedMessage);
                // update client position
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Listen error from {clientEndPoint}: " + ex.Message);
        }
    }

    static void HandleClientTalk(object obj)
    {
        ClientInfo clientInfo = (ClientInfo)obj;
        TcpClient client = clientInfo.Client;
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
            List<ClientInfo> disconnectedClients = new List<ClientInfo>();

            foreach (var clientInfo in clients)
            {
                try
                {
                    NetworkStream stream = clientInfo.Client.GetStream();
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
                catch (Exception)
                {
                    disconnectedClients.Add(clientInfo);
                }
            }

            foreach (var clientInfo in disconnectedClients)
            {
                clients.Remove(clientInfo);
                clientInfo.Client.Close();
                Console.WriteLine("Removed disconnected client.");
            }
        }
    }
}
