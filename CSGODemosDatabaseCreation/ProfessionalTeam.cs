using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoInfo;

namespace CSGODemosDatabaseCreation
{
    /// <summary>
    /// Class that represents a team
    /// </summary>
    class ProfessionalTeam
    {
        //Name of the team
        private string m_name;

        //Side the team is on at the moment
        private Team m_side;

        /// <summary>
        /// Constructor for a team
        /// </summary>
        /// <param name="name">Name of the team</param>
        /// <param name="side">Side the team is starting on</param>
        /// <param name="date">Date of the event</param>
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
            m_name = name;
        }

        /// <summary>
        /// Switch the side of the team. If the team is CT, it becomes T, and vice versa.
        /// </summary>
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

        /// <summary>
        /// Function that finds the name of the team today
        /// </summary>
        /// <param name="name">Name of the team on the demo</param>
        /// <param name="date">Date of the tournament</param>
        /// <returns>Returns the name of the team today.</returns>
        private string MatchTeamNameWithDate(string name, DateTime date)
        {
            //TODO: Check the date of the event and match the name of the team from that
            return name;
        }
    }
}
