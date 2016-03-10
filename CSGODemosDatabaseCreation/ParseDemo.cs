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
        }

        public bool ParseADemo(string outputDatabaseFilename, string demoFilename)
        {
            m_date = GetDateOfEvent(demoFilename);

            m_parser = new DemoParser(new FileStream(demoFilename, FileMode.Open));

            m_parser.ParseHeader();

            m_parser.MatchStarted += CatchMatchStarted;

            m_parser.ParseToEnd();

            return true;
        }

        private void CatchMatchStarted(object sender, MatchStartedEventArgs e)
        {
            m_hasMatchStarted = true;
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
            return new DateTime();
        }
    }
}
