using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoInfo;

namespace CSGODemosDatabaseCreation
{
    class ProfessionalTeam
    {
        private string m_name;
        private Team m_side;

        public ProfessionalTeam(string name, Team side, DateTime date)
        {
            this.SetName(MatchTeamNameWithDate(name, date));
            m_side = side;
        }

        public string GetName()
        {
            return m_name;
        }

        public void SetName(string name)
        {

        }

        public void SwitchSide()
        {
            if(m_side == Team.CounterTerrorist)
            {
                m_side = Team.Terrorist;
            }
            else
            {
                m_side = Team.CounterTerrorist;
            }
        }

        private string MatchTeamNameWithDate(string name, DateTime date)
        {
            //TODO: Check the date of the event and match the name of the team from that
            return name;
        }
    }
}
