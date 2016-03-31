using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGODemosDatabaseCreation
{
    class Match
    {
        private List<Round> rounds;

        public Match()
        {
            rounds = new List<Round>();
        }

        public void AddRound(Round round)
        {
            bool letsadd = true;

            foreach(string s in round.GetValues())
            {
                if(s == "")
                {
                    letsadd = false;
                    break;
                }
            }

            if(letsadd)
            {
                rounds.Add(round);
            }
        }

        public List<Round> GetAllRounds()
        {
            return rounds;
        }

        public void RemoveLastRound()
        {
            if(rounds.Count > 0)
            {
                rounds.RemoveAt(rounds.Count - 1);
            }
        }
    }
}
