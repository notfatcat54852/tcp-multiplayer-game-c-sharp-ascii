using System;
using System.Net.Sockets;

using System.Text;
using System.Threading;

using System.Collections.Generic;


class TcpClientProgram
{
    private static object playersLock = new object();
    private static string playerString;
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
        //Thread draw = new Thread(Draw);
        //draw.Start();
        ////

        listenToServer.Join();
        talkToServer.Join();


        //draw.Join();

        // Close the connection
        client.Close();

        Console.ReadKey();
    }

    static void Draw(string input)
    {
        string[] x = input.Split(' ');
        //Received: p1 0 0
        //3 and 5
        Console.WriteLine("player count: " + x.Length / 3);

        bool temp;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                temp = false;
                for (int h = 0; h < x.Length / 3; h++)
                {
                    if (x[1 + (3 * h)] == j.ToString() && x[2 + (3 * h)] == i.ToString())
                        temp = true;
                }
                if (temp)
                    Console.Write("#");
                else
                    Console.Write("_");
            }
                Console.WriteLine();
        }
        
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

            Draw(responseMessage);
            //UpdatePlayers(responseMessage);
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
