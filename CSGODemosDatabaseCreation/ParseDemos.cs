using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CSGODemosDatabaseCreation
{
    class ParseDemos
    {
        private static string[] GetFilenamesInDirectory(string directoryPath)
        {
            try
            {
                string[] filenames = Directory.GetFiles(directoryPath, "*.dem", SearchOption.AllDirectories);
                return filenames;
            }
            catch (DirectoryNotFoundException)
            {
                Logger.GetInstance().Log(String.Format("The directory {0} can't be found.",directoryPath));
                return new string[0];
            }
            catch (ArgumentException)
            {
                Logger.GetInstance().Log(String.Format("The path {0} contains invalid characters.", directoryPath));
                return new string[0];
            }
            catch (PathTooLongException)
            {
                Logger.GetInstance().Log(String.Format("The path {0} is too long. Try using a relative path instead.", directoryPath));
                return new string[0];
            }
            catch (IOException)
            {
                Logger.GetInstance().Log(String.Format("The path {0} is a filename. Please provide a directory name with all the .dem files.", directoryPath));
                return new string[0];
            }
        }

        static void Main(string[] args)
        {
            string outputDatabaseFilename = "DemoDatabase.csv";

            ParseDemo demoParser = new ParseDemo();

            string[] listOfAttributeNames = {
                "Team name", "Enemy team name", "Map", "Team side", "Team equipment value", "Enemy team equipment value",
                "Team number of rifles", "Enemy team number of rifles", "Team number of AWPs", "Enemy team number of AWPs", "Team number of shotguns", "Enemy team number of shotguns", 
                "Team number of SMGs", "Enemy team number of SMGs", "Team number of machine guns", "Enemy team number of machine guns",
                "Team number of upgraded pistols", "Enemy team number of upgraded pistols", "Team number of kevlar", "Enemy team number of kevlar",
                "Team number of helmets", "Enemy team number of helmets", "Team number of smoke grenades used", "Enemy team number of smoke grenades used",
                "Team number of flashes used", "Enemy team number of flashes used", "Team number of molotov used", "Enemy team number of molotov used",
                "Team number of HE used", "Enemy team number of HE used", "Team entry kill",  "Class" };

            System.Text.UTF8Encoding utf8NoBom = new System.Text.UTF8Encoding(false);
            using (StreamWriter writer = new StreamWriter(outputDatabaseFilename, false, utf8NoBom))
            {
                for (int i = 0; i < listOfAttributeNames.Length - 1; i++)
                {
                    writer.Write(listOfAttributeNames[i] + ",");
                }
                writer.Write(listOfAttributeNames[listOfAttributeNames.Length - 1]);
                writer.WriteLine();
            }

            foreach (string arg in args)
            {
                foreach (string demoFilename in GetFilenamesInDirectory(arg))
                {
                    try
                    {
                        Logger.GetInstance().Log(String.Format("Parsing {0}", demoFilename));
                        demoParser.ParseADemo(outputDatabaseFilename, demoFilename, listOfAttributeNames);
                    }
                    catch(Exception e)
                    {
                        Logger.GetInstance().Log(String.Format("The file {0} wasn't parsed properly.", demoFilename));
                        Logger.GetInstance().Log(e.Message);
                    }
                }
            }
        }
    }
}
