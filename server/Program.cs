using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpServer
{
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

        // Read data from the client
        byte[] buffer = new byte[256];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Received: " + receivedMessage);

        // Send a response to the client
        string responseMessage = "Hello from server!";
        byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
        stream.Write(responseBytes, 0, responseBytes.Length);

        // Close the connection
        client.Close();
    }
}
