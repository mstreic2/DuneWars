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
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommand
    {
        [JsonProperty(PropertyName = "moving")]
        public string Moving { get; set; }

        [JsonProperty(PropertyName = "fire")]
        public string Fire { get; set; }

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D Tdir { get; set; }

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
    }
}
