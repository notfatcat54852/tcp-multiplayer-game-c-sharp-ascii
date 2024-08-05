using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpServer
{
    //create client list with lock for broadcast feature    
    private static List<TcpClient> clients = new List<TcpClient>();
    private static object clientsLock = new object();
    static void Main()
    {
        // Define the server port
        int port = 13000;
        TcpListener server = new TcpListener(IPAddress.Any, port);

        server.Start();
        Console.WriteLine("Server started and waiting for connection...");
        while (true)
        {

            // Accept client connection
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            lock (clientsLock)
            {
                clients.Add(client);
            }

            // Create a thread to handle the client
            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }

        // Keep the server running
        Console.ReadLine();
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;

        // Get the stream to read/write data
        NetworkStream stream = client.GetStream();

        Thread clientThreadListen = new Thread(HandleClientListen);
        clientThreadListen.Start(client);

        //// Read data from the client
        //byte[] buffer = new byte[256];
        //int bytesRead = stream.Read(buffer, 0, buffer.Length);
        //string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        //Console.WriteLine("Received: " + receivedMessage);

        Thread clientThreadTalk = new Thread(HandleClientTalk);
        clientThreadTalk.Start(client);

        //BroadcastLoopTest thread
        Thread broadcastLoopTest = new Thread(BroadcastLoopTest);
        broadcastLoopTest.Start();

        //// Send a response to the client
        //string responseMessage = "Hello from server!";
        //byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
        //stream.Write(responseBytes, 0, responseBytes.Length);

        clientThreadListen.Join();
        clientThreadTalk.Join();
        //for testing the bc test
        broadcastLoopTest.Join();


        // Close the connection
        client.Close();

        lock (clientsLock)
        {
            clients.Remove(client);
        }
    }


    static void HandleClientListen(object obj)
    {
        TcpClient client = (TcpClient)obj;

        // Get the stream to read/write data
        NetworkStream stream = client.GetStream();

        // Read data from the client
        byte[] buffer = new byte[256];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Received: " + receivedMessage);
    }
    static void HandleClientTalk(object obj)
    {
        TcpClient client = (TcpClient)obj;

        // Get the stream to read/write data
        NetworkStream stream = client.GetStream();

        // Send a response to the client
        string responseMessage = "Hello from server!";
        byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
        stream.Write(responseBytes, 0, responseBytes.Length);
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
            foreach (var client in clients)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
        }
    }
}