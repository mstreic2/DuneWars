using TankWars;
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
    /// Wall class to describe barriers to tank and projectile movement.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        private static int wallId = 0;

        [JsonProperty(PropertyName = "wall")]
        public int WallId { get; private set; } = 0;

        [JsonProperty(PropertyName = "p1")]
        public Vector2D Point1 { get; private set;}

        [JsonProperty(PropertyName = "p2")]
        public Vector2D Point2 { get; private set;}

        double top, bottom, left, right;
        internal const int WALL_WIDTH = 50;

        public Wall()
        {
        }

        /// <summary>
        /// Parameterized wall constructor.
        /// </summary>
        /// <param name="wallId"></param>
        /// <param name="p1">First wall terminus</param>
        /// <param name="p2">Second wall terminus</param>
        public Wall(Vector2D p1, Vector2D p2)
        {
            WallId = wallId++;
            Point1 = p1;
            Point2 = p2;
        }
        /// <summary>
        /// Determines if an object has collided with a wall.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool WallObjectCollision(Object o)
        {
            double buffer = WALL_WIDTH / 2;
            Vector2D newObjLoc;

            if (o is Tank)
            {
                Tank obj = o as Tank;
                buffer += Tank.TANK_WIDTH / 2;
                newObjLoc = obj.Location + obj.Velocity;
            }
            else if (o is Projectile)
            {
                Projectile obj = o as Projectile;
                newObjLoc = obj.Location + obj.Velocity;
            }
            else if (o is Powerup)
            {
                Powerup obj = o as Powerup;
                newObjLoc = obj.Location;
            }
            else
            {
                return false;
            }

            return WallCollision(newObjLoc, buffer);
        }

        public bool WallCollision(Vector2D location, double buffer)
        {
            // Set wall collision boundaries
            left = Math.Min(Point1.GetX(), Point2.GetX()) - buffer;
            right = Math.Max(Point1.GetX(), Point2.GetX()) + buffer;
            top = Math.Min(Point1.GetY(), Point2.GetY()) - buffer;
            bottom = Math.Max(Point1.GetY(), Point2.GetY()) + buffer;

            // Check object position against buffered wall boundaries
            return left < location.GetX()
                && location.GetX() < right
                && top < location.GetY()
                && location.GetY() < bottom;
        }
        /// <summary>
        /// Serializes a wall as a JSON object to be sent to the client.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
