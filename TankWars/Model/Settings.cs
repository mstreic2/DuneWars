using System.Collections.Generic;
using System.Xml;

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
    /// Class for storing and reading in
    /// </summary>
    public class Settings
    {
        public static int UniverseSize { get; internal set; }
        public static int MSPerFrame { get; internal set; }
        public static int ShotCooldown { get; internal set; }
        public static int TankRespawnRate { get; internal set; }
        public static int PowerUpSpawnTimer { get; internal set; } = 1650;
        public static int PowerUpWorldLimit { get; internal set; } = 2;
        public static string Filename { get; internal set; }

        public HashSet<Wall> walls;

        public Settings(string filePath)
        {
            walls = new HashSet<Wall>();
            Filename = filePath;

            ImportSettingsXml();
        }

        /// <summary>
        /// Reads the contents of an XML file and assigns internal settings files, as well as wall locations.
        /// </summary>
        private void ImportSettingsXml()
        {
            using (XmlReader reader = XmlReader.Create(Filename))
            {
                while (reader.Read())
                    if (reader.IsStartElement())
                        switch (reader.Name)
                        {
                            // Overall game settings
                            case "GameSettings":
                                break;
                            case "UniverseSize":
                                UniverseSize = reader.ReadElementContentAsInt();
                                break;
                            case "MSPerFrame":
                                MSPerFrame = reader.ReadElementContentAsInt();
                                break;
                            case "FramesPerShot":
                                ShotCooldown = reader.ReadElementContentAsInt();
                                break;
                            case "RespawnRate":
                                TankRespawnRate = reader.ReadElementContentAsInt();
                                break;
                            case "Wall":
                                while (reader.Name != "x")
                                    reader.Read();
                                int x1 = reader.ReadElementContentAsInt();
                                int y1 = reader.ReadElementContentAsInt();

                                while (reader.Name != "x")
                                    reader.Read();
                                int x2 = reader.ReadElementContentAsInt();
                                int y2 = reader.ReadElementContentAsInt();

                                // Create wall based on points
                                walls.Add(new Wall(new Vector2D(x1, y1), new Vector2D(x2, y2)));
                                break;
                            default:
                                break;
                        }
            }
        }
    }
}
