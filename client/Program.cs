using System;
using System.Net.Sockets;

using System.Text;
using System.Threading;

using System.Collections.Generic;


class TcpClientProgram
{
    //private class ClientInfo
    //{
    //    public TcpClient Client { get; set; }
    //    public int X { get; set; }
    //    public int Y { get; set; }
    //    public int ID { get; set; }

    //    private static int nextId = 1; // Static counter for client IDs

    //    public ClientInfo(TcpClient client, int x, int y, int id)
    //    {
    //        Client = client;
    //        X = x;
    //        Y = y;
    //        ID = id;
    //    }
    //}
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
    //static void Draw()
    //{
    //    string x;
    //    lock (playersLock)
    //    {
    //        x = playerString;
    //    }
    //    if (x[0] != 'p')
    //        return;
    //    string[] x2 = x.Split(' ');
    //    List<Tuple<int, int>> pairs = new List<Tuple<int, int>>();
    //    pairs.Add(Tuple.Create(1, 2));
    //    pairs.Add(Tuple.Create(4, 5));
    //    while (true)
    //    {
    //        for (int i = 0; i < 10; i++)
    //        {
    //            for (int j = 0; j < 10; j++)
    //            {
    //                if (pairs.Contains(Tuple.Create(i, j)))
    //                {
    //                    Console.WriteLine($"#");
    //                }
    //                else
    //                {
    //                    Console.Write(" ");
    //                }
    //            }
    //            Console.WriteLine();
    //        }
    //        Thread.Sleep(1000);
    //    }
    //}
    public static int playerCount = 0;
    public static void UpdatePlayers(string x)
    {
        lock (playersLock)
        {
            playerString = x;
        }
        if (x[0] != 'p')
            return;
        string[] x2 = x.Split(' ');
        playerCount = x2.Length / 3;
        Console.WriteLine(playerCount);

    }
    static void RemoveClientById(int id)
    {
        //lol
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

            UpdatePlayers(responseMessage);
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
