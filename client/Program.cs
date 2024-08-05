using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

class TcpClientProgram
{
    static void Main()
    {
        // Define the server address and port
        string server = "127.0.0.1";
        int port = 13000;

        // Connect to the server
        TcpClient client = new TcpClient(server, port);
        Console.WriteLine("Connected to the server!");

        //// Get the stream to read/write data
        //NetworkStream stream = client.GetStream();

        ///
        Thread talkToServer = new Thread(TalkToServer);
        talkToServer.Start(client);
        ///
        ///
        Thread listenToServer = new Thread(ListenToServer);
        listenToServer.Start(client);
        ///

        //// Send a message to the server
        //string message = "Hello from client!";
        //byte[] data = Encoding.ASCII.GetBytes(message);
        //stream.Write(data, 0, data.Length);

        //// Read the response from the server
        //byte[] buffer = new byte[256];
        //int bytesRead = stream.Read(buffer, 0, buffer.Length);
        //string responseMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        //Console.WriteLine("Received: " + responseMessage);

        listenToServer.Join();
        talkToServer.Join();

        Console.ReadLine();

        // Close the connection
        client.Close();
    }

    static void ListenToServer(object obj)
    {
        TcpClient client = (TcpClient)obj;
        // Get the stream to read/write data
        NetworkStream stream = client.GetStream();
        while (true)
        {
            // Read the response from the server
            byte[] buffer = new byte[256];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string responseMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + responseMessage);

        }
    }

    static void TalkToServer(object obj)
    {
        TcpClient client = (TcpClient)obj;
        // Get the stream to read/write data
        NetworkStream stream = client.GetStream();

        // Send a message to the server
        string message = "Hello from client!";
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
}
