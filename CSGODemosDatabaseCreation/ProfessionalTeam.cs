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
        public Team m_side { get; set; }

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
            if(name.Equals("CLG", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Counter Logic Gaming";
            }
            else if(name.Equals("Team Dignitas", StringComparison.CurrentCultureIgnoreCase) || name.Equals("dignitas", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Dignitas";
            }
            else if (name.Equals("VexedGaming", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Vexed Gaming";
            }
            else if (name.Equals("Cloud9 CS", StringComparison.CurrentCultureIgnoreCase) || name.Equals("Cloud9 G2A", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Cloud9";
            }
            else if (name.Equals("Team EnVyUs", StringComparison.CurrentCultureIgnoreCase) || name.Equals("ENVYUS", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "EnvyUs";
            }
            else if (name.Equals("fnatic", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Fnatic";
            }
            else if (name.Equals("Na'Vi", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Natus Vincere";
            }
            else if (name.Equals("mousesports", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Mousesports";
            }
            else if (name.Equals("Virtus.Pro", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Virtus.pro";
            }
            else if (name.Equals("Flipsid3 Tactics", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "FlipSid3 Tactics";
            }
            else if (name.Equals("TSM", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Team SoloMid";
            }
            else if (name.Equals("RENEGADES", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Renegades";
            }
            else if (name.Equals("Selfless", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Selfless Gaming";
            }
            else if (name.Equals("EchoFox", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Echo Fox";
            }
            else if (name.Equals("Faze", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Faze Clan";
            }
            else if (name.Equals("NRG", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "NRG eSports";
            }

            if (name.Equals("Conquest", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "OpTiC";
            }
            else if(name.Equals("Team eBettle", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Vexed Gaming";
            }
            else if ((name.Equals("Team SoloMid", StringComparison.CurrentCultureIgnoreCase) && date.CompareTo(new DateTime(2015, 12, 3)) <= 0) ||
                name.Equals("questionmark", StringComparison.CurrentCultureIgnoreCase))
            {
                name = "Astralis";
            }
            else if ((name.Equals("G2 Esports", StringComparison.CurrentCultureIgnoreCase) && date.CompareTo(new DateTime(2016, 1, 20)) <= 0) ||
                (name.Equals("Team Kinguin", StringComparison.CurrentCultureIgnoreCase) && date.CompareTo(new DateTime(2015, 9, 1)) <= 0))
            {
                name = "Faze Clan";
            }
            else if (name.Equals("Tempo Storm", StringComparison.CurrentCultureIgnoreCase) && date.CompareTo(new DateTime(2016, 2, 10)) <= 0)
            {
                name = "ex-Tempo Storm";
            }
            else if (name.Equals("Titan", StringComparison.CurrentCultureIgnoreCase) && date.CompareTo(new DateTime(2016, 2, 1)) <= 0)
            {
                name = "G2 Esports";
            }
            return name;
        }
    }
}
