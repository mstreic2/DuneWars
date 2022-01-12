using NetworkUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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
    /// Class to control network events and connect cients to the world model via broadcast updates
    /// </summary>
    public class ServerController
    {
        private World theWorld;
        private string startUpInfo;
        private Dictionary<long, SocketState> clients;

        /// <summary>
        /// Constructor for the ServerController called at program inception.
        /// </summary>
        /// <param name="settings">Imported data from the provided XML settings file</param>
        public ServerController(Settings settings)
        {
            theWorld = new World(Settings.UniverseSize);
            clients = new Dictionary<long, SocketState>();

            // Add walls from settings into world
            foreach (Wall wall in settings.walls)
            {
                theWorld.walls[wall.WallId] = wall;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(theWorld.UniverseSize);
            sb.Append("\n");

            foreach (Wall wall in theWorld.walls.Values)
            {
                sb.Append(wall.ToJson());
            }
            startUpInfo = sb.ToString();
        }

        /// <summary>
        /// Initialize the server by setting up the OnNetworkAction of the networking library and port to 11000.
        /// Also create the update thread.
        /// </summary>
        internal void Start()
        {
            Networking.StartServer(NewClientConnects, 11000);
            Thread t = new Thread(Update);
            t.Start();
        }

        /// <summary>
        /// Updates the state of the world to all connected clients according to a stopwatch timer. This timer corresponds to the 
        /// ms per frame as provided by the settings file.
        /// </summary>
        private void Update()
        {
            Stopwatch watch = new Stopwatch();
            string update;

            watch.Start();
            while (true)
            {
                while (watch.ElapsedMilliseconds < Settings.MSPerFrame) { } // Watch constantly spins a busy cycle
                watch.Restart();

                StringBuilder sb = new StringBuilder();

                lock (theWorld)
                {
                    // Update all world objects within a lock section
                    theWorld.Update();
                    foreach (Tank tank in theWorld.tanks.Values)
                    {
                        if (tank.Disconnected)
                        { 
                            tank.Died = true;
                            tank.Hp = 0;
                            theWorld.disconnectedTanks.Push(tank);
                        }
                        sb.Append(tank.ToJson());
                    }
                    foreach (Projectile projectile in theWorld.projectiles.Values)
                    {
                        sb.Append(projectile.ToJson());
                    }
                    foreach (Powerup powerup in theWorld.powerups.Values)
                    {
                        sb.Append(powerup.ToJson());
                    }
                    while (theWorld.beamsJson.TryPop(out string beam))
                    {
                        sb.Append(beam);
                    }
                }

                // Aggregate all changes into a string to send to all connected clients
                update = sb.ToString();
                lock (clients)
                {
                    foreach (SocketState client in clients.Values)
                        Networking.Send(client.TheSocket, update);
                }
            }
        }

        /// <summary>
        /// When a new client connects, prepare to receive their name
        /// </summary>
        /// <param name="client"></param>
        private void NewClientConnects(SocketState client)
        {
            if (client.ErrorOccured)
                return;
            client.OnNetworkAction = RecievePlayerName;
            Networking.GetData(client);
        }

        /// <summary>
        /// Following a successful client connection, receive the player's name
        /// </summary>
        /// <param name="client"></param>
        private void RecievePlayerName(SocketState client)
        {
            // Check for any client-side socket errors
            if (client.ErrorOccured)
                return;

            string name = client.GetData();

            // If the client sends incomplete data, wait for further data
            if (!name.EndsWith("\n"))
            {
                client.GetData();
                return;
            }

            client.RemoveData(0, name.Length);
            name = name.Trim('\n'); // Remove newline from player name

            // Provide the client with startup information about the world, as well as their ID on the server
            Networking.Send(client.TheSocket, client.ID + "\n");
            Networking.Send(client.TheSocket, startUpInfo);

            lock (theWorld)
            {
                theWorld.tanks[(int)client.ID] = new Tank((int)client.ID, name, theWorld.FindEmptyRandomLocation());
            }
            lock (clients)
            {
                clients.Add(client.ID, client);
            }

            // Prepare to receive any client commands
            client.OnNetworkAction = RecieveControlCommands;
            Networking.GetData(client);
        }

        /// <summary>
        /// Receives player inputs for processing within the world to evaluate if they can be executed
        /// </summary>
        /// <param name="client">SocketState for client on this thread</param>
        private void RecieveControlCommands(SocketState client)
        {
            // If the client has an error, remove them from the client dictionary
            if (client.ErrorOccured)
            {
                RemoveClient(client.ID);
                return;
            }

            string totalData = client.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            foreach (string p in parts)
            {
                if (p.Length == 0) // Ignore empty control commands
                    continue;
                if (p[p.Length - 1] != '\n') // Ensure only complete commands are sent to the world
                    break;

                ControlCommand ctrlCmd = JsonConvert.DeserializeObject<ControlCommand>(p);
                lock (theWorld)
                {
                    theWorld.ctrlCmds[(int)client.ID] = ctrlCmd;
                }
                client.RemoveData(0, p.Length); // Clean up the state's message thread
            }

            // Continue the event-loop by passing network activity back to the client
            Networking.GetData(client);
        }

        /// <summary>
        /// Removes a client from the clients dictionary
        /// </summary>
        /// <param name="id">The ID of the client</param>
        private void RemoveClient(long id)
        {
            Console.WriteLine("Client " + id + " disconnected");
            lock (clients)
            {
                lock (theWorld)
                {
                    theWorld.tanks[(int)id].Disconnected = true;                    
                    theWorld.ctrlCmds.Remove((int)id);
                }
                clients.Remove(id);
            }
        }
    }
}
