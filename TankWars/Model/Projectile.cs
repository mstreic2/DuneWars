using Newtonsoft.Json;
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
    /// Class for player-initiated attacks against other player or AI tanks.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        public static int projId = 0;

        [JsonProperty(PropertyName = "proj")]
        public int Id { get; private set; } = 0;

        [JsonProperty(PropertyName = "loc")]
        public Vector2D Location { get; internal set; }

        [JsonProperty(PropertyName = "dir")]
        public Vector2D Direction { get; internal set; }

        [JsonProperty(PropertyName = "died")]
        public bool Died { get; internal set; }

        [JsonProperty(PropertyName = "owner")]
        public int Owner { get; internal set; }

        public static double Speed { get; internal set; } = 25.0;
        public Vector2D Velocity { get; internal set; }

        public Projectile()
        { 
        }

        /// <summary>
        /// Projectile constructor using server-provided variables
        /// </summary>
        /// <param name="loc">Projectile coordinates</param>
        /// <param name="dir">Projectile direction</param>
        /// <param name="owner">Player ID who fired the projectile</param>
        public Projectile(Vector2D loc, Vector2D dir, int owner)
        {
            Id = projId++;
            Location = loc;
            Direction = dir;
            Died = false;
            Owner = owner;
            Velocity = dir * Speed;
        }
        /// <summary>
        /// Serializes a projectile to a JSON object to be sent to the client.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
