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
                string[] filenames = Directory.GetFiles(directoryPath, "*.dem");
                return filenames;
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("The directory {0} can't be found.", directoryPath);
                return new string[0];
            }
            catch (ArgumentException)
            {
                Console.WriteLine("The path {0} contains invalid characters.", directoryPath);
                return new string[0];
            }
            catch (PathTooLongException)
            {
                Console.WriteLine("The path {0} is too long. Try using a relative path instead.", directoryPath);
                return new string[0];
            }
            catch (IOException)
            {
                Console.WriteLine("The path {0} is a filename. Please provide a directory name with all the .dem files.", directoryPath);
                return new string[0];
            }
        }

        static void Main(string[] args)
        {
            string outputDatabaseFilename = "DemoDatabase.csv";

            ParseDemo demoParser = new ParseDemo();

            foreach(string arg in args)
            {
                foreach (string demoFilename in GetFilenamesInDirectory(arg))
                {
                    demoParser.ParseADemo(outputDatabaseFilename, demoFilename);
                }
            }
        }
    }
}
