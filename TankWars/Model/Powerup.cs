using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
    /// Class that provides a utility boost to the player whose tank reaches it
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        private static int powerUpId = 0;

        [JsonProperty(PropertyName = "power")]
        public int Id { get; internal set; }
        [JsonProperty(PropertyName = "loc")]
        public Vector2D Location { get; internal set; }
        [JsonProperty(PropertyName = "died")]
        public bool Died { get; internal set; }

        public Powerup()
        { 
        }

        /// <summary>
        /// Constructor for powerup based on server information
        /// </summary>
        /// <param name="power">Powerup ID</param>
        /// <param name="loc">Powerup coordinates</param>
        /// <param name="died">Whether the powerup continues to the next frame</param>
        public Powerup(Vector2D loc)
        {
            Id = powerUpId++;
            Location = loc;
            Died = false;
        }
        /// <summary>
        /// Checks if a tank has collided with a powerup object.
        /// </summary>
        /// <param name="tank"></param>
        /// <returns></returns>
        public bool PowerupTankCollision(Tank tank)
        {
            Vector2D location = Location;
            double left = tank.Location.GetX() - Tank.TANK_WIDTH / 2;
            double right = tank.Location.GetX() + Tank.TANK_WIDTH / 2;
            double top = tank.Location.GetY() - Tank.TANK_WIDTH / 2;
            double bottom = tank.Location.GetY() + Tank.TANK_WIDTH / 2;

            return left < location.GetX()
                && location.GetX() < right
                && top < location.GetY()
                && location.GetY() < bottom;
        }
        /// <summary>
        /// Serializes a powerup to a JSON object to be sent to the client.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
