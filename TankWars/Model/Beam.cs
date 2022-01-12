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
    /// A special attack availabe to players through powerups
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        private static int beamId = 0;

        [JsonProperty(PropertyName = "beam")]
        public int Id { get; private set; } = 0;
        [JsonProperty(PropertyName = "org")]
        public Vector2D Origin { get; private set; }
        [JsonProperty(PropertyName = "dir")]
        public Vector2D Direction { get; private set; }
        [JsonProperty(PropertyName = "owner")]
        public int Owner { get; private set; }

        public Beam()
        { 
        }

        /// <summary>
        /// Constructor for beam class
        /// </summary>
        /// <param name="origin">The coordinates of the beam's origin</param>
        /// <param name="dir">The direction in degrees from Up the beam fires in</param>
        /// <param name="owner">The player whose tank fired the beam</param>
        public Beam(Vector2D origin, Vector2D dir, int owner)
        {
            Id = beamId++;
            Origin = origin;
            Direction = dir;
            Owner = owner;
        }
        /// <summary>
        /// Serializes a Beam as a JSON object to be sent to the client.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
