﻿using System;
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

        //Filename of the output database
        private string m_outputDatabaseFilename;

        /// <summary>
        /// Constructor for a RoundPrinter
        /// Writes the first line in the file consisting of the name of all the attributes separated by \t
        /// </summary>
        /// <param name="attributes">String array that contains the name of all the attributes in order.</param>
        /// <param name="outputDatabseFilename">Filename of the output file where we want to print the rounds.</param>
        public RoundPrinter(string[] attributes, string outputDatabaseFilename)
        {
            m_attributes = attributes;
            m_outputDatabaseFilename = outputDatabaseFilename;
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
                System.Text.UTF8Encoding utf8NoBom = new System.Text.UTF8Encoding(false);
                using(StreamWriter writer = new StreamWriter(m_outputDatabaseFilename, true, utf8NoBom))
                {
                    for (int i = 0; i < round.Length - 1; i++)
                    {
                        if(round[i] == "")
                        {
                            return false;
                        }
                        writer.Write(round[i] + ",");
                    }
                    writer.Write(round[round.Length - 1]);
                    writer.WriteLine();
                }
                return true;
            }
            return false;
        }
    }
}
