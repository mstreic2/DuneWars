using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

/*
 * Dune (Tank) Wars
 * By: Matthew Streicher and Jim Morgan
 * University of Utah
 * CS3500 - Software Practice 1
 * Fall 2021
 */

namespace TankWars
{
    public class DrawingPanel : Panel
    {
        private static object PLAYER_INFO_LOCK;

        private readonly World theWorld;
        readonly GameController controller;
        private Dictionary<int, BeamAnimation> animatedBeams;
        private Dictionary<int, BeamAnimation> animatedBeamsCopy;
        private Dictionary<int, TankDeathAnimation> animatedTankDeaths;
        private Dictionary<int, TankDeathAnimation> animatedTankDeathsCopy;

        private const int TANK_WIDTH = 60;
        private const int TURRET_WIDTH = 50;
        private const int PROJ_WIDTH = 30;
        private const int POWER_WIDTH = 40;
        private const int WALL_WIDTH = 50;

        readonly Image wallImage;
        readonly Image projectileImage;
        readonly Image beamImage;
        readonly Image tankDeathImage;
        readonly Image duneBackground;
        readonly Image powerupImage;

        // A class to hold all player-associated color and image data for tanks and turrets
        private static class PlayerInfo
        {
            // Temp class variables to allow for extraceting player info on objects
            static Image tankImage;
            static Image turretImage;

            // Declare tank images
            static readonly Image pinkTank;
            static readonly Image redTank;
            static readonly Image orangeTank;
            static readonly Image yellowTank;
            static readonly Image greenTank;
            static readonly Image tealTank;
            static readonly Image blueTank;
            static readonly Image purpleTank;

            // Declare turret images
            static readonly Image pinkTurret;
            static readonly Image redTurret;
            static readonly Image orangeTurret;
            static readonly Image yellowTurret;
            static readonly Image greenTurret;
            static readonly Image tealTurret;
            static readonly Image blueTurret;
            static readonly Image purpleTurret;

            /// <summary>
            /// Pre-defined colors for players as they join the game.
            /// </summary>
            enum PlayerColor
            {
                PINK = 0,
                RED = 1,
                ORANGE = 2,
                YELLOW = 3,
                GREEN = 4,
                TEAL = 5,
                BLUE = 6,
                PURPLE = 7,
            }

            /// <summary>
            /// Constructor for static PlayerInfo class
            /// </summary>
            static PlayerInfo()
            {
                try
                {
                    // Initialize tank images
                    pinkTank = Image.FromFile(@"..\..\..\Resources\Images\PinkTank.png");
                    redTank = Image.FromFile(@"..\..\..\Resources\Images\RedTank.png");
                    orangeTank = Image.FromFile(@"..\..\..\Resources\Images\OrangeTank.png");
                    yellowTank = Image.FromFile(@"..\..\..\Resources\Images\YellowTank.png");
                    greenTank = Image.FromFile(@"..\..\..\Resources\Images\GreenTank.png");
                    tealTank = Image.FromFile(@"..\..\..\Resources\Images\TealTank.png");
                    blueTank = Image.FromFile(@"..\..\..\Resources\Images\BlueTank.png");
                    purpleTank = Image.FromFile(@"..\..\..\Resources\Images\PurpleTank.png");

                    // Initialize turret images
                    redTurret = Image.FromFile(@"..\..\..\Resources\Images\RedTurret.png");
                    orangeTurret = Image.FromFile(@"..\..\..\Resources\Images\OrangeTurret.png");
                    yellowTurret = Image.FromFile(@"..\..\..\Resources\Images\YellowTurret.png");
                    greenTurret = Image.FromFile(@"..\..\..\Resources\Images\GreenTurret.png");
                    blueTurret = Image.FromFile(@"..\..\..\Resources\Images\BlueTurret.png");
                    purpleTurret = Image.FromFile(@"..\..\..\Resources\Images\PurpleTurret.png");
                    pinkTurret = Image.FromFile(@"..\..\..\Resources\Images\PinkTurret.png");
                    tealTurret = Image.FromFile(@"..\..\..\Resources\Images\TealTurret.png");
                }
                catch (Exception)
                {
                    MessageBox.Show("Image file(s) not found");
                    Application.Exit();
                }
            }

            /// <summary>
            /// Gets the tank image information 
            /// </summary>
            /// <param name="tank">Player ID requesting tank information</param>
            /// <returns></returns>
            public static Image GetTankImage(Tank tank)
            {
                SetPlayerInfo(tank);
                return tankImage;
            }

            /// <summary>
            /// Gets the turret image information 
            /// </summary>
            /// <param name="tank">Player ID requesting tank information</param>
            /// <returns></returns>
            public static Image GetTurretImage(Tank tank)
            {
                SetPlayerInfo(tank);
                return turretImage;
            }

            /// <summary>
            /// Sets the player color based on their ID
            /// </summary>
            /// <param name="tank">Player ID</param>
            private static void SetPlayerInfo(Tank tank)
            {
                PlayerColor color = (PlayerColor)(tank.TankId % 8);

                switch (color)
                {
                    case PlayerColor.BLUE:
                        tankImage = blueTank;
                        turretImage = blueTurret;
                        break;
                    case PlayerColor.PURPLE:
                        tankImage = purpleTank;
                        turretImage = purpleTurret;
                        break;
                    case PlayerColor.PINK:
                        tankImage = pinkTank;
                        turretImage = pinkTurret;
                        break;
                    case PlayerColor.GREEN:
                        tankImage = greenTank;
                        turretImage = greenTurret;
                        break;
                    case PlayerColor.ORANGE:
                        tankImage = orangeTank;
                        turretImage = orangeTurret;
                        break;
                    case PlayerColor.RED:
                        tankImage = redTank;
                        turretImage = redTurret;
                        break;
                    case PlayerColor.YELLOW:
                        tankImage = yellowTank;
                        turretImage = yellowTurret;
                        break;
                    case PlayerColor.TEAL:
                        tankImage = tealTank;
                        turretImage = tealTurret;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Beam animation object wrapper for beams
        /// </summary>
        internal class BeamAnimation
        {
            public Beam beam;
            public int currentFrame;
            public bool removeAnimation;

            public const int TOTAL_ROWS = 8;
            public const int TOTAL_COLS = 1;
            public const int TOTAL_FRAMES = 8;

            // The x coordinate of the source rectangle for the sprite sheet
            public int XCoord
            {
                get { return currentFrame % TOTAL_ROWS * 64; }
            }

            // The y coordinate of the source rectangle for the sprite sheet
            public int YCoord
            {
                get { return 0; }
            }

            public BeamAnimation(Beam beam)
            {
                currentFrame = 0;
                removeAnimation = false;
                this.beam = beam;
            }

            public int SelectFrame()
            {
                return currentFrame * 64;
            }
        }

        /// <summary>
        /// Tank death animation object for tank deaths
        /// </summary>
        internal class TankDeathAnimation
        {
            public Tank tank;
            public int currentFrame;
            public bool removeAnimation;

            public const int TOTAL_ROWS = 4;
            public const int TOTAL_COLS = 4;
            public const int TOTAL_FRAMES = 16;

            public int XCoord
            {
                get { return 64 * currentFrame % 4; }
            }

            public int YCoord
            {
                get { return currentFrame / 4 * 64; }
            }

            public TankDeathAnimation(Tank tank)
            {
                currentFrame = 0;
                removeAnimation = false;
                this.tank = tank;
            }
        }

        /// <summary>
        /// Creates a tank death animation object wrapper based on a server-provided tank death or disconnect
        /// </summary>
        /// <param name="tank"></param>
        private void CreateTankDeathAnimation(Tank tank)
        {
            if (tank.Disconnected)
            {
                theWorld.tanks.Remove(tank.TankId);
            }
            animatedTankDeaths.Add(tank.TankId, new TankDeathAnimation(tank));
        }

        private void CreateBeamAnimation(Beam beam)
        {
            animatedBeams.Add(beam.Id, new BeamAnimation(beam));
        }

        public DrawingPanel(GameController controller)
        {
            DoubleBuffered = true;
            this.controller = controller;
            theWorld = controller.GetWorld();
            PLAYER_INFO_LOCK = new Object();

            animatedBeams = new Dictionary<int, BeamAnimation>();
            animatedTankDeaths = new Dictionary<int, TankDeathAnimation>();

            try
            {
                wallImage = Image.FromFile(@"..\..\..\Resources\Images\Wall.png");
                duneBackground = Image.FromFile(@"..\..\..\Resources\Images\Sand2.png");
                beamImage = Image.FromFile(@"..\..\..\Resources\Images\Beam.png");
                projectileImage = Image.FromFile(@"..\..\..\Resources\Images\Projectile.png");
                powerupImage = Image.FromFile(@"..\..\..\Resources\Images\PowerUp.png");
                tankDeathImage = Image.FromFile(@"..\..\..\Resources\Images\Death.png");
            }
            catch (Exception)
            {
                MessageBox.Show("Image file(s) not found");
                Application.Exit();
            }

            // Set up listeners
            controller.TankDeath += CreateTankDeathAnimation;
            controller.BeamAnimation += CreateBeamAnimation;
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Draws a tank
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            Image tankImage = null;

            lock (PLAYER_INFO_LOCK)
            {
                tankImage = PlayerInfo.GetTankImage(tank);
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(tankImage, new Rectangle(-(TANK_WIDTH / 2), -(TANK_WIDTH / 2), TANK_WIDTH, TANK_WIDTH));
        }

        private void HealthBarDrawer(object o, PaintEventArgs e)
        {
            double healthBarWidth;
            Brush healthBrush;

            Tank tank = o as Tank;
            Font font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
            string nameAndScore = tank.Name + ": " + tank.Score;

            switch (tank.Hp)
            {
                case 3:
                    healthBrush = Brushes.Green;
                    healthBarWidth = TANK_WIDTH - 20;
                    break;
                case 2:
                    healthBrush = Brushes.Yellow;
                    healthBarWidth = (TANK_WIDTH - 20) * 0.6;
                    break;
                default:
                    healthBrush = Brushes.Red;
                    healthBarWidth = (TANK_WIDTH - 20) * 0.3;
                    break;
            }

            e.Graphics.FillRectangle(healthBrush, -(TANK_WIDTH / 2) + 10, -(TANK_WIDTH / 2) - 10, (int)healthBarWidth, 5);
            e.Graphics.DrawString(nameAndScore, font, Brushes.White, -(TANK_WIDTH / 2) - 5, (TANK_WIDTH / 2));
        }

        /// <summary>
        /// Draws a turret
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            Image turretImage;

            lock (PLAYER_INFO_LOCK)
            {
                turretImage = PlayerInfo.GetTurretImage(tank);
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(turretImage, new Rectangle(-(TURRET_WIDTH / 2), -(TURRET_WIDTH / 2), TURRET_WIDTH, TURRET_WIDTH));
        }

        /// <summary>
        /// Draws a projectile
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(projectileImage, new Rectangle(-(PROJ_WIDTH / 2), -(PROJ_WIDTH / 2), PROJ_WIDTH, PROJ_WIDTH));
        }

        /// <summary>
        /// Draws powerups
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(powerupImage, new Rectangle(-(POWER_WIDTH / 2), -(POWER_WIDTH / 2), POWER_WIDTH, POWER_WIDTH));
        }

        private void AnimatedBeamDrawer(object o, PaintEventArgs e)
        {
            BeamAnimation beamAnimation = o as BeamAnimation;
            int beamLength = 2 * theWorld.UniverseSize;

            Rectangle destinationRectangle = new Rectangle(-(TANK_WIDTH / 2), -beamLength, TANK_WIDTH, beamLength);
            Rectangle sourceRectangle = new Rectangle(new Point(beamAnimation.XCoord, 0), new Size(64, 64));

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(beamImage, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        private void TankDeathDrawer(object o, PaintEventArgs e)
        {
            TankDeathAnimation tankDeathAnimation = o as TankDeathAnimation;

            Rectangle destinationRectangle = new Rectangle(-(TANK_WIDTH / 2), -(TANK_WIDTH / 2), TANK_WIDTH, TANK_WIDTH);
            Rectangle sourceRectangle = new Rectangle(new Point(tankDeathAnimation.XCoord, tankDeathAnimation.YCoord), new Size(64, 64));

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(tankDeathImage, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Draws walls
        /// </summary>
        /// <param name="wall"></param>
        private void WallDrawer(Wall wall, PaintEventArgs e)
        {
            Point origin;

            double width = wall.Point2.GetX() - wall.Point1.GetX();
            double height = wall.Point2.GetY() - wall.Point1.GetY();

            // Determine leftmost or topmost point for rectangle origin and offset to account for center coordinate placement
            if (width < 0 || height < 0)
                origin = new Point((int)wall.Point2.GetX(), (int)wall.Point2.GetY());
            else
                origin = new Point((int)wall.Point1.GetX(), (int)wall.Point1.GetY());
            origin.Offset(-WALL_WIDTH / 2, -WALL_WIDTH / 2);

            // Set height and width for linear block chains
            if (height == 0 && width == 0) // For single blocks
            {
                height = WALL_WIDTH;
                width = WALL_WIDTH;
            }
            else if (height == 0) // For long wall blocks
            {
                height = WALL_WIDTH;
                width = Math.Abs(width) + WALL_WIDTH;
            }
            else if (width == 0) // For tall wall blocks
            {
                height = Math.Abs(height) + WALL_WIDTH;
                width = WALL_WIDTH;
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            using (TextureBrush wallBrush = new TextureBrush(wallImage))
            {
                wallBrush.ScaleTransform(0.78125f, 0.78125f); // 0.78125 is the ratio of the 64x64 image to a size 50 square
                e.Graphics.FillRectangle(wallBrush, new Rectangle(origin, new Size((int)width, (int)height)));
            }
        }

        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {
            double playerX, playerY;

            lock (theWorld)
            {
                // Check if player tank is in tanks dictionary
                if (theWorld.tanks.TryGetValue(controller.playerID, out Tank playerTank))
                {
                    // Assign tank's coords
                    playerX = playerTank.Location.GetX();
                    playerY = playerTank.Location.GetY();
                }
                else
                {
                    return;
                }

                // Set up View window
                int viewSize = Size.Width;
                e.Graphics.TranslateTransform((int)(-playerX + (viewSize / 2)), (int)(-playerY + (viewSize / 2)));

                // Set up world background
                BackColor = Color.Black;
                e.Graphics.DrawImage(duneBackground, new Rectangle(new Point((-controller.WorldSize) / 2, (-controller.WorldSize) / 2), new Size(controller.WorldSize, controller.WorldSize)));

                // Draw walls
                foreach (Wall wall in theWorld.walls.Values)
                {
                    WallDrawer(wall, e);
                }
                // Draw the players' tanks
                foreach (Tank tank in theWorld.tanks.Values)
                {
                    if (tank.Hp > 0 || !tank.Disconnected)
                    {
                        DrawObjectWithTransform(e, tank, tank.Location.GetX(), tank.Location.GetY(), tank.BodyDirection.ToAngle(), TankDrawer);
                        DrawObjectWithTransform(e, tank, tank.Location.GetX(), tank.Location.GetY(), tank.TurretDirection.ToAngle(), TurretDrawer);
                        DrawObjectWithTransform(e, tank, tank.Location.GetX(), tank.Location.GetY(), 0, HealthBarDrawer);
                    }
                }

                // Draw all projectiles
                foreach (Projectile proj in theWorld.projectiles.Values)
                {
                    DrawObjectWithTransform(e, proj, proj.Location.GetX(), proj.Location.GetY(), proj.Direction.ToAngle(), ProjectileDrawer);
                }

                // Draw powerups
                foreach (Powerup pow in theWorld.powerups.Values)
                {
                    DrawObjectWithTransform(e, pow, pow.Location.GetX(), pow.Location.GetY(), 0, PowerupDrawer);
                }

                // Animate fired beams
                animatedBeamsCopy = new Dictionary<int, BeamAnimation>();
                foreach (BeamAnimation beamAnimation in animatedBeams.Values)
                {
                    DrawObjectWithTransform(e, beamAnimation, beamAnimation.beam.Origin.GetX(), beamAnimation.beam.Origin.GetY(), beamAnimation.beam.Direction.ToAngle(), AnimatedBeamDrawer);
                    beamAnimation.currentFrame++;

                    if (beamAnimation.currentFrame < BeamAnimation.TOTAL_FRAMES - 1)
                        animatedBeamsCopy.Add(beamAnimation.beam.Id, beamAnimation);
                }
                animatedBeams = animatedBeamsCopy;

                // Animate tank deaths
                animatedTankDeathsCopy = new Dictionary<int, TankDeathAnimation>();
                foreach (TankDeathAnimation tankDeathAnimation in animatedTankDeaths.Values)
                {
                    DrawObjectWithTransform(e, tankDeathAnimation, tankDeathAnimation.tank.Location.GetX(), tankDeathAnimation.tank.Location.GetY(),
                                    tankDeathAnimation.tank.BodyDirection.ToAngle(), TankDeathDrawer);
                    tankDeathAnimation.currentFrame++;

                    if (tankDeathAnimation.currentFrame < TankDeathAnimation.TOTAL_FRAMES - 1)
                        animatedTankDeathsCopy.Add(tankDeathAnimation.tank.TankId, tankDeathAnimation);
                }
                animatedTankDeaths = animatedTankDeathsCopy;
            }
            base.OnPaint(e);
        }
    }
}

