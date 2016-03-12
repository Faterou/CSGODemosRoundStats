using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGODemosDatabaseCreation
{
    /// <summary>
    /// Class that prints a given round somewhere
    /// </summary>
    class RoundPrinter
    {
        //Array containing the name of all the attributes
        private string[] m_attributes;

        //Writer
        private TextWriter m_writer;

        /// <summary>
        /// Constructor for a RoundPrinter
        /// </summary>
        /// <param name="attributes">String array that contains the name of all the attributes in order.</param>
        /// <param name="outputDatabseFilename">Filename of the output file where we want to print the rounds.</param>
        public RoundPrinter(string[] attributes, string outputDatabaseFilename)
        {
            m_attributes = attributes;
            m_writer = new StreamWriter(outputDatabaseFilename, false, Encoding.UTF8);
        }

        /// <summary>
        /// Function that prints in the round in the output file
        /// </summary>
        /// <param name="round">String array that contains the values of every attribute in order.</param>
        /// <returns>Returns true if everything went correctly, false if not.</returns>
        public bool PrintRound(string[] round)
        {
            if(round.Length == m_attributes.Length)
            {
                for(int i = 0; i < round.Length; i++)
                {
                    m_writer.Write(round[i] + " ");
                }
                m_writer.WriteLine();

                return true;
            }
            return false;
        }
    }
}
