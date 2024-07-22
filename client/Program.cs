using System;
using System.Net.Sockets;
using System.Text;

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

        // Get the stream to read/write data
        NetworkStream stream = client.GetStream();

        // Send a message to the server
        string message = "Hello from client!";
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);

        // Read the response from the server
        byte[] buffer = new byte[256];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string responseMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Received: " + responseMessage);

        // Close the connection
        client.Close();
    }
}
