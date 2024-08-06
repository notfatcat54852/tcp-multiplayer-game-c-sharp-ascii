using System;

using System.Net.Sockets;

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

        ///
        Thread talkToServer = new Thread(TalkToServer);
        talkToServer.Start(client);
        ///
        ///
        Thread listenToServer = new Thread(ListenToServer);
        listenToServer.Start(client);
        ///

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

        while (true)
        {
            string message;
            message = Console.ReadKey(true).KeyChar.ToString().ToUpper();

            // Send a message to the server
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
}
