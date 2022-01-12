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
    /// Class describing the player tank objects.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        // Tank properties sent to client
        [JsonProperty(PropertyName = "tank")]
        public int TankId { get; internal set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        [JsonProperty(PropertyName = "loc")]
        public Vector2D Location { get; internal set; }

        [JsonProperty(PropertyName = "bdir")]
        public Vector2D BodyDirection { get; internal set; }

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D TurretDirection { get; internal set; }

        [JsonProperty(PropertyName = "score")]
        public int Score { get; internal set; }

        [JsonProperty(PropertyName = "hp")]
        public int Hp { get; set; }

        [JsonProperty(PropertyName = "died")]
        public bool Died { get; set; }

        [JsonProperty(PropertyName = "dc")]
        public bool Disconnected { get; set; }

        [JsonProperty(PropertyName = "join")]
        public bool Joined { get; internal set; }

        // Tank server-side settings
        public int NumBeams { get; internal set; }
        public Vector2D Velocity { get; internal set; }
        public double EnginePower { get; internal set; } = 3;
        public int FramesUntilNextShot { get; internal set; } = 0;
        public int FramesUntilRespawn { get; internal set; } = 0;
        public int ShotCooldown { get; internal set; } = Settings.ShotCooldown;
        public int DiconnectedTimer { get; internal set; } = 1;
        public bool HotStreak { get; internal set; } = false;
        public int HotStreakTimer { get; internal set; } = 3600;

        double top, bottom, left, right;

        public const int MAX_HP = 3; 
        public const int TANK_WIDTH = 60;


        public Tank()
        {
        }

        /// <summary>
        /// Tank constructor with server-provided parameters
        /// </summary>
        /// <param name="tank">Tank ID </param>
        /// <param name="name">Tank name</param>
        /// <param name="loc">Tank coordinates</param>
        /// <param name="bdir">Tank facing relative to screen</param>
        /// <param name="tdir">Turret facing in degrees relative to screen Up</param>
        /// <param name="score">Player score</param>
        /// <param name="hp">Tank health</param>
        /// <param name="died">Tank died flag</param>
        /// <param name="disconnected">Tank disconnected flag</param>
        /// <param name="join">Player joined game and requires new tank</param>
        public Tank(int tankId, string name, Vector2D origin)
        {
            TankId = tankId;
            Name = name;
            Location = origin;
            BodyDirection = new Vector2D(0, -1);
            TurretDirection = new Vector2D(0, -1);
            Score = 0;
            Hp = MAX_HP;
            Died = false;
            Disconnected = false;
            Joined = true;
            Velocity = new Vector2D(0, 0);
            NumBeams = 0;
        }
        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }
        /// <summary>
        /// Determines if a projectile or beam collides with a tank.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool TankObjectCollision(object o)
        {
            if (o is Projectile)
            {
                Projectile projectile = o as Projectile;
                if (projectile.Owner != TankId)
                    return ProjectileCollision(projectile.Location);
            }
            else if (o is Beam)
            {
                Beam beam = o as Beam;
                if (beam.Owner != TankId)
                    return Intersects(beam.Origin, beam.Direction, Location, TANK_WIDTH / 2);
            }
            return false;
        }
        /// <summary>
        /// Helper for determining projectile colliding with a tank.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool ProjectileCollision(Vector2D location)
        {
            // Set tank target
            left = -TANK_WIDTH / 2 + Location.GetX();
            right = TANK_WIDTH / 2 + Location.GetX();
            top = -TANK_WIDTH / 2 + Location.GetY();
            bottom = TANK_WIDTH / 2 + Location.GetY();

            return left < location.GetX()
                && location.GetX() < right
                && top < location.GetY()
                && location.GetY() < bottom;
        }
        /// <summary>
        /// Serializes a tank as a JSON object to be sent to the client.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
