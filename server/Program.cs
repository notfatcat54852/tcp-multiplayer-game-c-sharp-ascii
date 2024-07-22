using System;
using System.Threading;

class Program
{
    static void Main()
    {
        string message = "hello world";

        // Create and start a thread with a method that takes a generic parameter
        Thread thread = new Thread(() => PrintMessage(message));
        thread.Start();

        // Wait for the thread to finish
        thread.Join();
    }

    // Method that the thread will execute
    static void PrintMessage(string message)
    {
        Console.WriteLine(message);
    }
}
