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
        private DemoParser m_parser;

        private bool m_hasMatchStarted;
        private string m_map;
        private ProfessionalTeam m_team;
        private ProfessionalTeam m_enemyTeam;
        private DateTime m_date;

        public ParseDemo()
        {
            m_parser = null;
            m_hasMatchStarted = false;
            m_date = new DateTime();
            m_team = null;
            m_enemyTeam = null;
            m_map = "";
        }

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

        private void CatchMatchStarted(object sender, MatchStartedEventArgs e)
        {
            m_hasMatchStarted = true;
            m_team = new ProfessionalTeam(((DemoParser)sender).CTClanName, Team.CounterTerrorist, m_date);
            m_enemyTeam = new ProfessionalTeam(((DemoParser)sender).TClanName, Team.Terrorist, m_date);
        }

        //This is where we calculate the equipment value of each teams
        private void CatchFreezetimeEnded(object sender, FreezetimeEndedEventArgs e)
        {
            return;
        }

        private void CatchRoundStart(object sender, RoundStartedEventArgs e)
        {
            return;
        }

        private void CatchRoundEnd(object sender, RoundEndedEventArgs e)
        {
            return;
        }

        // Catches players killed even during warmup and knife rounds
        private void CatchPlayerKilled(object sender, PlayerKilledEventArgs e)
        {
            return;
        }

        private void resetParser()
        {

        }

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
            return new DateTime();
        }
    }
}
