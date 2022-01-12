using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

/*
* Dune (Tank) Wars
* By: Matthew Streicher and Jim Morgan
* University of Utah
* CS3500 - Software Practice 1
* Fall 2021
*/
namespace TankWars
{
    public class GameController
    {
        private World theWorld;
        private SocketState server;
        private ControlCommand ctrlCommand;

        public int playerID;
        private string playerName;

        public delegate void ErrorOccurredHandler(string errorMessage);
        public delegate void SetupHandler();
        public delegate void ServerUpdateHandler();
        public delegate void BeamAnimationHandler(Beam beam);
        public delegate void TankDeathAnimationHandler(Tank tank);

        public event ServerUpdateHandler UpdateArrived;
        public event ErrorOccurredHandler ErrorOccurred;
        public event SetupHandler ReceivedSetupInfo;
        public event BeamAnimationHandler BeamAnimation;
        public event TankDeathAnimationHandler TankDeath;

        public int WorldSize { get; internal set; }

        public GameController()
        {
            ctrlCommand = new ControlCommand();
            server = null;
        }
        public World GetWorld()
        {
            return theWorld;
        }

        public void Connect(string IpAddress, string playerName)
        {
            try
            {
                Networking.ConnectToServer(OnConnect, IpAddress, 11000);
            }
            catch (System.Exception)
            {
                ErrorOccurred("Unable to connect to server");
            }
            this.playerName = playerName;
        }

        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccured)
            {
                ErrorOccurred(state.ErrorMessage);
                return;
            }

            try
            {
                Networking.Send(state.TheSocket, playerName + "\n");
                server = state;
                state.OnNetworkAction = ReceiveStartupInfo;

                Networking.GetData(state);
            }
            catch (System.Exception)
            {
                ErrorOccurred(state.ErrorMessage);
                return;
            }
        }

        private void ReceiveStartupInfo(SocketState state)
        {
            if (state.ErrorOccured)
            {
                ErrorOccurred(state.ErrorMessage);
                return;
            }

            string data = state.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            if (parts.Length < 2 || !parts[1].EndsWith("\n"))
            {
                Networking.GetData(state);
                return;
            }

            // Set up world and player ID
            int.TryParse(parts[0], out playerID);
            int.TryParse(parts[1], out int worldSize);
            state.RemoveData(0, parts[0].Length + parts[1].Length);

            WorldSize = worldSize;

            theWorld = new World(worldSize);

            state.OnNetworkAction = ReceiveJson;

            ReceivedSetupInfo();

            try
            {
                Networking.GetData(state);
            }
            catch (System.Exception)
            {
                ErrorOccurred("Disconnected from Server upon receive");
                return;
            }
        }

        private void ReceiveJson(SocketState state)
        {
            string data = state.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            if (state.ErrorOccured)
            {
                ErrorOccurred(state.ErrorMessage);
                return;
            }

            lock (theWorld)
            {
                foreach (string part in parts)
                {
                    if (part.Length == 0)
                        continue;
                    if (part[part.Length - 1] != '\n')
                        break;

                    // Handle the data received
                    JObject obj = JObject.Parse(part);
                    TryParseJson(obj, part);
                    state.RemoveData(0, part.Length);
                }

            }
            try
            {
                ProcessInputs();
                UpdateArrived();
                Networking.GetData(state);
            }
            catch (System.Exception)
            {
                ErrorOccurred("Disconnected from Server");
                return;
            }
        }

        /// <summary>
        /// Private Helper that deserializes different objects from the server. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="part"></param>
        private void TryParseJson(JObject obj, string part)
        {
            JToken token = obj["wall"];
            if (token != null)
            {
                Wall wall = JsonConvert.DeserializeObject<Wall>(part);
                theWorld.walls[wall.WallId] = wall;
                return;
            }
            token = obj["tank"];
            if (token != null)
            {
                Tank tank = JsonConvert.DeserializeObject<Tank>(part);
                if (tank.Died)
                    TankDeath(tank);
                else
                    theWorld.tanks[tank.TankId] = tank;
                return;
            }
            token = obj["proj"];
            if (token != null)
            {
                Projectile proj = JsonConvert.DeserializeObject<Projectile>(part);
                if (proj.Died)
                    theWorld.projectiles.Remove(proj.Id);
                else
                    theWorld.projectiles[proj.Id] = proj;
                return;
            }
            token = obj["power"];
            if (token != null)
            {
                Powerup power = JsonConvert.DeserializeObject<Powerup>(part);
                if (power.Died)
                    theWorld.powerups.Remove(power.Id);
                else
                    theWorld.powerups[power.Id] = power;
                return;
            }
            token = obj["beam"];
            if (token != null)
            {
                Beam beam = JsonConvert.DeserializeObject<Beam>(part);
                BeamAnimation(beam);
                return;
            }
        }

        private void ProcessInputs()
        {
            string message = JsonConvert.SerializeObject(ctrlCommand);
            Networking.Send(server.TheSocket, message + "\n");
        }

        public void MouseMove(Vector2D cursorLoc)
        {
            ctrlCommand.Tdir = cursorLoc;

        }

        public void HandleMoveRequest(string direction)
        {
            ctrlCommand.Moving = direction;
        }

        public void CancelMoveRequest()
        {
            ctrlCommand.Moving = "none";
        }

        public void HandleMouseRequest(string fireProj)
        {
            ctrlCommand.Fire = fireProj;
        }

        public void CancelMouseRequest()
        {
            ctrlCommand.Fire = "none";
        }

       /* [JsonObject(MemberSerialization.OptIn)]
        public class ControlCommand
        {
            [JsonProperty(PropertyName = "moving")]
            public string Moving { get; internal set; }

            [JsonProperty(PropertyName = "fire")]
            public string Fire { get; internal set; }

            [JsonProperty(PropertyName = "tdir")]
            public Vector2D Tdir { get; internal set; }

            public ControlCommand()
            {
                Tdir = new Vector2D(0, -1);
                Clear();
            }

            internal void Clear()
            {
                Moving = "none";
                Fire = "none";
            }
        }*/
    }
}
