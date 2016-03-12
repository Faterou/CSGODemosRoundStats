using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DemoInfo;
using System.IO;

namespace CSGODemosDatabaseCreation
{
    public class ParseDemo
    {
        //The demo parser
        private DemoParser m_parser;

        //Boolean that is true if the match is started and false if not
        private bool m_hasMatchStarted;

        //Name of the map played
        private string m_map;

        //First team
        private ProfessionalTeam m_team;

        //Second team
        private ProfessionalTeam m_enemyTeam;

        //Date of the tournament
        private DateTime m_date;

        /// <summary>
        /// Constructor for a demo parser
        /// </summary>
        public ParseDemo()
        {
            m_parser = null;
            m_hasMatchStarted = false;
            m_date = new DateTime();
            m_team = null;
            m_enemyTeam = null;
            m_map = "";
        }


        /// <summary>
        /// Parses a demo
        /// </summary>
        /// <param name="outputDatabaseFilename">Filename of the output file</param>
        /// <param name="demoFilename">Filename of the demo file</param>
        /// <returns>Returns true if the demo was parsed correctly.</returns>
        public bool ParseADemo(string outputDatabaseFilename, string demoFilename)
        {
            m_date = GetDateOfEvent(demoFilename);

            m_parser = new DemoParser(new FileStream(demoFilename, FileMode.Open));

            m_parser.ParseHeader();

            m_map = m_parser.Map;

            // We subscribe to all the events we need and bind them to a function
            m_parser.MatchStarted += CatchMatchStarted;
            m_parser.FreezetimeEnded += CatchFreezetimeEnded;
            m_parser.RoundStart += CatchRoundStart;
            m_parser.RoundEnd += CatchRoundEnd;
            m_parser.PlayerKilled += CatchPlayerKilled;

            m_parser.ParseToEnd();

            return true;
        }

        /// <summary>
        /// Function that catches a MatchStartedEvent
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchMatchStarted(object sender, MatchStartedEventArgs e)
        {
            m_hasMatchStarted = true;
            m_team = new ProfessionalTeam(((DemoParser)sender).CTClanName, Team.CounterTerrorist, m_date);
            m_enemyTeam = new ProfessionalTeam(((DemoParser)sender).TClanName, Team.Terrorist, m_date);
        }

        /// <summary>
        /// Function that catches a FreezetimeEndedEvent
        /// This is where we calculate the equipment value of each teams
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchFreezetimeEnded(object sender, FreezetimeEndedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Function that catches a RoundStartedEvent
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchRoundStart(object sender, RoundStartedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Function that catches a RoundEndedEvent
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchRoundEnd(object sender, RoundEndedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Function that triggers when a player is killed.
        /// Catches players killed even during warmup and knife rounds
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchPlayerKilled(object sender, PlayerKilledEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Resets the parser
        /// </summary>
        private void resetParser()
        {

        }

        /// <summary>
        /// Gets the date of the event with the filename (the events are hard coded, make sure all your filenames countain the name of the events).
        /// </summary>
        /// <param name="filename">Filename of the demo</param>
        /// <returns>Returns the date of the event</returns>
        private DateTime GetDateOfEvent(string filename)
        {
            if(Regex.Match(filename,"IEMKatowice2016").Success)
            {
                return new DateTime(2016, 3, 2);
            }
            else if(Regex.Match(filename, "GEC2016Finals").Success)
            {
                return new DateTime(2016, 2, 4);
            }
            else if (Regex.Match(filename, "IEMSanJose2015").Success)
            {
                return new DateTime(2015, 11, 21);
            }
            else if (Regex.Match(filename, "MLGColumbus2016MainQualifier").Success)
            {
                return new DateTime(2016, 2, 26);
            }
            else if (Regex.Match(filename, "SLi14Finals").Success)
            {
                return new DateTime(2016, 1, 14);
            }
            else if (Regex.Match(filename, "ESLESEA2Finals").Success)
            {
                return new DateTime(2015, 12, 13);
            }
            else if (Regex.Match(filename, "ESLBarcelonaCSGOInvitational").Success)
            {
                return new DateTime(2016, 19, 2);
            }
            else if (Regex.Match(filename, "DreamHackMastersMalmoNAClosedQualifier").Success)
            {
                return new DateTime(2016, 2, 21);
            }
            else if (Regex.Match(filename, "DHLondon2015").Success)
            {
                return new DateTime(2015, 9, 20);
            }
            else if (Regex.Match(filename, "DHLeipzig2016").Success)
            {
                return new DateTime(2016, 1, 22);
            }
            else if (Regex.Match(filename, "DHCluj2015").Success)
            {
                return new DateTime(2015, 10, 28);
            }
            else if (Regex.Match(filename, "DHCluj2015LANQualifier").Success)
            {
                return new DateTime(2015, 9, 22);
            }
            return new DateTime();
        }
    }
}
