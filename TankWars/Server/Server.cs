using System;

/*
* Dune (Tank) Wars
* By: Matthew Streicher and Jim Morgan
* University of Utah
* CS3500 - Software Practice 1
* Fall 2021
*/
namespace TankWars
{
    /// <summary>
    /// Executable for running the TankWars server. Sets up the ServerController and initalizes it.
    /// </summary>
    class Server
    {
        static void Main(string[] args)
        {
            Settings setting = new Settings(@"..\..\..\..\Resources\settings_smallworld.xml");
            ServerController server = new ServerController(setting);
            server.Start();
            Console.WriteLine("Accepting new clients");
            Console.ReadLine();
        }
    }
}
