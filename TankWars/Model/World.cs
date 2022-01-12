using System;
using System.Collections.Generic;

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
    /// A class to store world model objects for the game model.
    /// </summary>
    public class World
    {
        public Dictionary<int, Powerup> powerups;
        public Dictionary<int, Projectile> projectiles;
        public Dictionary<int, Tank> tanks;
        public Dictionary<int, Wall> walls;
        public Dictionary<int, ControlCommand> ctrlCmds;

        public Stack<Beam> beams;
        public Stack<string> beamsJson;
        public Stack<Projectile> deadProjectiles;
        public Stack<Powerup> deadPowerups;
        public Stack<Tank> disconnectedTanks;

        public int PowerUpTimer;

        public int UniverseSize { get; private set; }

        /// <summary>
        /// Constructor for the world class.
        /// </summary>
        /// <param name="_size">A variable passed obtained from the server used to generate a game world.</param>
        public World(int _size)
        {
            powerups = new Dictionary<int, Powerup>();
            projectiles = new Dictionary<int, Projectile>();
            tanks = new Dictionary<int, Tank>();
            walls = new Dictionary<int, Wall>();
            ctrlCmds = new Dictionary<int, ControlCommand>();
            beams = new Stack<Beam>();
            UniverseSize = _size;

            deadPowerups = new Stack<Powerup>();
            deadProjectiles = new Stack<Projectile>();
            disconnectedTanks = new Stack<Tank>();
            beamsJson = new Stack<string>();

            PowerUpTimer = 0;
        }

        /// <summary>
        /// Finds an empty random loaction in the world to spawn an object.
        /// </summary>
        /// <returns></returns>
        public Vector2D FindEmptyRandomLocation()
        {
            Vector2D randomLocation = GenerateRandomCoords();

            while (WallCollisionChecker(randomLocation))
            {
                randomLocation = GenerateRandomCoords();
            }

            foreach (Powerup powerup in powerups.Values)
            {
                if (powerup.Location == randomLocation)
                    randomLocation = GenerateRandomCoords();
            }
            return randomLocation;
        }

        /// <summary>
        /// Returns false if no collision occurs.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool WallCollisionChecker(Vector2D position)
        {
            foreach (Wall wall in walls.Values)
            {
                if (wall.WallCollision(position, Tank.TANK_WIDTH / 2 + Wall.WALL_WIDTH / 2))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Generated the random coordinates within the bounds of the universe.
        /// </summary>
        /// <returns></returns>
        private Vector2D GenerateRandomCoords()
        {
            Random random = new Random();

            int randX = random.Next(-UniverseSize / 2 + Tank.TANK_WIDTH, UniverseSize / 2 - Tank.TANK_WIDTH);
            int randY = random.Next(-UniverseSize / 2 + Tank.TANK_WIDTH, UniverseSize / 2 - Tank.TANK_WIDTH);
            return new Vector2D(randX, randY);
        }

        /// <summary>
        /// Checks for if object passes world boundaries. Teleports object to opposite side of axis.
        /// </summary>
        /// <param name="o"></param>
        private void CheckWorldEdgeCollision(object o)
        {
            // Set universe size boundaries
            double left = -UniverseSize / 2;
            double top = -UniverseSize / 2;
            double right = UniverseSize / 2;
            double bottom = UniverseSize / 2;

            if (o is Tank)
            {
                Tank tank = o as Tank;
                double x = tank.Location.GetX();
                double y = tank.Location.GetY();

                if (x > right || x < left)
                {
                    tank.Location = new Vector2D(-x, y);
                }
                else if (y < top || y > bottom)
                {
                    tank.Location = new Vector2D(x, -y);
                }
            }
            else if (o is Projectile)
            {
                Projectile projectile = o as Projectile;
                double x = projectile.Location.GetX();
                double y = projectile.Location.GetY();

                if (x > right || x < left || y < top || y > bottom)
                {
                    projectile.Died = true;
                    deadProjectiles.Push(projectile);
                }
            }
        }

        /// <summary>
        /// Timers for tanks next chance to fire a projectile and a timer for the tanks hot streak.
        /// </summary>
        /// <param name="tank"></param>
        private void TankTimers(Tank tank)
        {
            //Frame and hotstreak counter must always decrement, where appropriate
            if (tank.FramesUntilNextShot > 0)
            {
                // Hotstreak counter
                if (tank.HotStreak)
                {
                    if (tank.HotStreakTimer > 0)
                        tank.HotStreakTimer--;
                    else
                        tank.EnginePower = 3;
                }
                tank.FramesUntilNextShot--;
            }
        }
        /// <summary>
        /// Increments tanks score if it destroys another tank. Checks if the killer's score is a multiple of 5.
        /// Hot streak is activated if so.
        /// </summary>
        /// <param name="killerId"></param>
        private void ScoreChecker(int killerId)
        {
            Tank killer = tanks[killerId];
            killer.Score++;
            if (killer.Score % 5 == 0)
            {
                killer.HotStreak = true;
                killer.EnginePower = 6;
                killer.HotStreakTimer = 3600;
            }
        }
        /// <summary>
        /// Updates control commands from user
        /// </summary>
        private void UpdateCtrlCommands()
        {
            // Process control commands and check against collision mechanics
            foreach (KeyValuePair<int, ControlCommand> ctrlCmd in ctrlCmds)
            {

                Tank tank = tanks[ctrlCmd.Key];
                if (tank.Hp == 0)
                {
                    tank.Velocity = new Vector2D(0, 0);
                    continue;
                }

                // Check tank's movement
                switch (ctrlCmd.Value.Moving)
                {
                    case "up":
                        tank.Velocity = new Vector2D(0, -1);
                        tank.BodyDirection = new Vector2D(0, -1);
                        break;
                    case "down":
                        tank.Velocity = new Vector2D(0, 1);
                        tank.BodyDirection = new Vector2D(0, 1);
                        break;
                    case "left":
                        tank.Velocity = new Vector2D(-1, 0);
                        tank.BodyDirection = new Vector2D(-1, 0);
                        break;
                    case "right":
                        tank.Velocity = new Vector2D(1, 0);
                        tank.BodyDirection = new Vector2D(1, 0);
                        break;
                    default:
                        tank.Velocity = new Vector2D(0, 0);
                        break;
                }
                tank.Velocity *= tank.EnginePower;

                ctrlCmd.Value.Tdir.Normalize();

                // Set tank's turret direction
                tank.TurretDirection = ctrlCmd.Value.Tdir;

                // Check tank against its shot timer
                if (tank.FramesUntilNextShot > 0)
                    continue;

                switch (ctrlCmd.Value.Fire)
                {

                    // Fire the main gun
                    case "main":

                        // Create new projectile with initial parameters
                        Projectile proj = new Projectile(tank.Location, tank.TurretDirection, tank.TankId);
                        projectiles.Add(proj.Id, proj);

                        // Set tank's projectile cooldown
                        tank.FramesUntilNextShot = tank.ShotCooldown;
                        break;

                    // Fire the beam weapon
                    case "alt":
                        if (tank.NumBeams > 0)
                        {
                            beams.Push(new Beam(tank.Location, tank.TurretDirection, tank.TankId));
                            tank.FramesUntilNextShot = tank.ShotCooldown;
                            tank.NumBeams--;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// Updates world objects.
        /// </summary>
        public void Update()
        {
            // Bring out your dead powerups
            while (deadPowerups.Count > 0)
            {
                powerups.Remove(deadPowerups.Pop().Id);
            }

            // Spawn powerups
            if (PowerUpTimer == 0 && powerups.Count < 2)
            {
                Powerup powerUp = new Powerup(FindEmptyRandomLocation());
                powerups.Add(powerUp.Id, powerUp);
                PowerUpTimer = 1650;
            }
            else
            {
                PowerUpTimer--;
            }

            // Bring out your dead projectiles
            while (deadProjectiles.Count > 0)
            {
                projectiles.Remove(deadProjectiles.Pop().Id);
            }

            UpdateCtrlCommands();

            // Update each tank
            foreach (Tank tank in tanks.Values)
            {
                // Check tank's timers for hotstreak and shot cooldown
                TankTimers(tank);

                // Handle tank death flag and respawn timer
                if (tank.Died)
                {
                    tank.FramesUntilRespawn = Settings.TankRespawnRate;
                    tank.Died = false;
                }

                if (tank.Hp == 0)
                {
                    if (tank.FramesUntilRespawn == 0)
                    {
                        tank.Hp = Tank.MAX_HP;
                        tank.Location = FindEmptyRandomLocation();
                    }
                    else
                        tank.FramesUntilRespawn--;
                }

                if (tank.Velocity.Length() == 0)
                    continue;

                foreach (Wall wall in walls.Values)
                {
                    if (wall.WallObjectCollision(tank))
                    {
                        tank.Velocity = new Vector2D(0, 0);
                        break;
                    }
                }

                foreach (Powerup powerup in powerups.Values)
                {
                    if (powerup.PowerupTankCollision(tank))
                    {
                        tank.NumBeams++;
                        powerup.Died = true;
                        deadPowerups.Push(powerup);
                    }
                }

                CheckWorldEdgeCollision(tank);
                tank.Location += tank.Velocity;
            }


            // Update each projectile's position
            foreach (Projectile projectile in projectiles.Values)
            {
                foreach (Wall wall in walls.Values)
                {
                    if (wall.WallObjectCollision(projectile))
                    {
                        projectile.Died = true;
                        deadProjectiles.Push(projectile);
                        break;
                    }
                }
                foreach (Tank tank in tanks.Values)
                {
                    if (tank.Hp == 0)
                        break;
                    if (tank.TankObjectCollision(projectile))
                    {
                        projectile.Died = true;
                        tank.Hp--;
                        if (tank.Hp == 0)
                        {
                            tank.Died = true;
                            ScoreChecker(projectile.Owner);
                        }
                        deadProjectiles.Push(projectile);
                        break;
                    }
                }
                CheckWorldEdgeCollision(projectile);
                projectile.Location += projectile.Velocity;
            }
            while (beams.Count > 0)
            {
                foreach (Tank tank in tanks.Values)
                {
                    if (tank.TankObjectCollision(beams.Peek()))
                    {
                        tank.Hp = 0;
                        tank.Died = true;

                        ScoreChecker(beams.Peek().Owner);
                    }
                }
                beamsJson.Push(beams.Pop().ToJson());
            }

            //dead tank removal
            while (disconnectedTanks.Count > 0)
            {
                tanks.Remove(disconnectedTanks.Pop().TankId);
            }
        }
    }
}
