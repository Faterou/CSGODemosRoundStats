using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGODemosDatabaseCreation
{
    class Round
    {
        //Dictionary of key: name of attribute and value: Value.
        private Dictionary<string, RoundAttribute> m_attributes;

        private int m_numberOfEnabledAttributes;

        //List of possible attributes
        string[] m_listOfAttributeNames = { "Team name", "Enemy team name", "Map", "Team side", "Team equipment value", "Enemy team equipment value",
                "Team number of rifles", "Enemy team number of rifles", "Team number of AWPs", "Enemy team number of AWPs", "Team number of shotguns", "Enemy team number of shotguns", 
                "Team number of SMGs", "Enemy team number of SMGs", "Team number of machine guns", "Enemy team number of machine guns",
                "Team number of upgraded pistols", "Enemy team number of upgraded pistols", "Team number of kevlar", "Enemy team number of kevlar",
                "Team number of helmets", "Enemy team number of helmets", "Team number of smoke grenades used", "Enemy team number of smoke grenades used",
                "Team number of flashes used", "Enemy team number of flashes used", "Team number of molotov used", "Enemy team number of molotov used",
                "Team number of HE used", "Enemy team number of HE used", "Team entry kill", "Round end by death", "Bomb exploded", "Bomb defused",
                "Time ran out", "Class" };

        /// <summary>
        /// Constructor that enables all of the m_listOfAttributeNames
        /// </summary>
        public Round()
        {
            m_attributes = new Dictionary<string, RoundAttribute>();

            for(int i = 0; i < m_listOfAttributeNames.Length; i++)
            {
                m_attributes.Add(m_listOfAttributeNames[i], new RoundAttribute(true));
            }

            m_numberOfEnabledAttributes = m_attributes.Count;
        }

        /// <summary>
        /// Constructor that enables all the attributes in the attributes parameter
        /// </summary>
        /// <param name="attributes">List of attributes you want to enable</param>
        public Round(string[] attributes)
        {
            m_numberOfEnabledAttributes = 0;
            m_attributes = new Dictionary<string, RoundAttribute>();

            for(int i = 0; i < m_listOfAttributeNames.Length; i++)
            {
                m_attributes.Add(m_listOfAttributeNames[i], new RoundAttribute(false));
            }

            for(int i = 0; i < attributes.Length; i++)
            {
                if(m_attributes.ContainsKey(attributes[i]))
                {
                    m_attributes[attributes[i]].m_isEnabled = true;
                    m_numberOfEnabledAttributes++;
                }
            }
        }

        /// <summary>
        /// Gets an array of strings containing all the attributes that are enabled
        /// </summary>
        /// <returns>Returns the array of strings containing all the attributes that are enabled</returns>
        public string[] GetAttributes()
        {
            string[] attributes = new string[m_numberOfEnabledAttributes];

            int counter = 0;

            foreach(KeyValuePair<string, RoundAttribute> kvp in m_attributes)
            {
                if(kvp.Value.m_isEnabled)
                {
                    attributes[counter] = kvp.Key;
                    counter++;
                }
            }

            return attributes;
        }

        /// <summary>
        /// Activates the attributes specified by attributes' array
        /// </summary>
        /// <param name="attributes">Name attributes to activate</param>
        public void SetAttributes(string[] attributes)
        {
            ClearAttributes();

            for (int i = 0; i < attributes.Length; i++)
            {
                if (m_attributes.ContainsKey(attributes[i]))
                {
                    m_attributes[attributes[i]].m_isEnabled = true;
                }
            }
        }

        /// <summary>
        /// Gets the values of the attributes that are enabled
        /// </summary>
        /// <returns>Returns an array of strings containing the values</returns>
        public string[] GetValues()
        {
            string[] values = new string[m_numberOfEnabledAttributes];

            int counter = 0;

            foreach (KeyValuePair<string, RoundAttribute> kvp in m_attributes)
            {
                if(kvp.Value.m_isEnabled)
                {
                    values[counter] = kvp.Value.m_value;
                    counter++;
                }
            }

            return values;
        }

        /// <summary>
        /// Get the value of an attribute
        /// </summary>
        /// <param name="attribute">Name of the attribute you want to get the value</param>
        /// <returns>Returns the string of the value</returns>
        public string GetValue(string attribute)
        {
            return m_attributes[attribute].m_value;
        }

        /// <summary>
        /// Sets the value of attributeString
        /// </summary>
        /// <param name="attributeString">Attribute's name to set the value of</param>
        /// <param name="value">Value to set</param>
        public void SetValue(string attributeString, string value)
        {
            m_attributes[attributeString].m_value = value;
        }

        /// <summary>
        /// Function that returns a dictionary of attribute-Value that are enabled
        /// </summary>
        /// <returns>Dictionary of attribute values that are enabled</returns>
        public Dictionary<string, string> GetAttributeValuePairs()
        {
            Dictionary<string, string> returnValue = new Dictionary<string, string>();

            foreach(string key in m_attributes.Keys)
            {
                if(m_attributes[key].m_isEnabled)
                {
                    returnValue.Add(key, m_attributes[key].m_value);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Function that reverses a round from the other team's perspective
        /// </summary>
        /// <param name="m_currentRound"></param>
        /// <returns></returns>
        public Round ReverseRound()
        {
            Round newRound = new Round();

            foreach (string att in this.m_attributes.Keys)
            {
                if(this.m_attributes[att].m_isEnabled)
                {
                    if (att == "Team name")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team name"));
                    }
                    else if(att == "Enemy team name")
                    {
                        newRound.SetValue(att, this.GetValue("Team name"));
                    }
                    else if (att == "Map")
                    {
                        newRound.SetValue(att, this.GetValue("Map"));
                    }
                    else if (att == "Team side")
                    {
                        if(this.GetValue("Team side") == "CounterTerrorist")
                        {
                            newRound.SetValue(att, "Terrorist");
                        }
                        else
                        {
                            newRound.SetValue(att, "CounterTerrorist");
                        }
                    }
                    else if (att == "Team equipment value")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team equipment value"));
                    }
                    else if (att == "Enemy team equipment value")
                    {
                        newRound.SetValue(att, this.GetValue("Team equipment value"));
                    }
                    else if (att == "Team number of rifles")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of rifles"));
                    }
                    else if (att == "Enemy team number of rifles")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of rifles"));
                    }
                    else if (att == "Team number of AWPs")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of AWPs"));
                    }
                    else if (att == "Enemy team number of AWPs")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of AWPs"));
                    }
                    else if (att == "Team number of shotguns")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of shotguns"));
                    }
                    else if (att == "Enemy team number of shotguns")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of shotguns"));
                    }
                    else if (att == "Team number of SMGs")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of SMGs"));
                    }
                    else if (att == "Enemy team number of SMGs")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of SMGs"));
                    }
                    else if (att == "Team number of machine guns")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of machine guns"));
                    }
                    else if (att == "Enemy team number of machine guns")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of machine guns"));
                    }
                    else if (att == "Team number of upgraded pistols")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of upgraded pistols"));
                    }
                    else if (att == "Enemy team number of upgraded pistols")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of upgraded pistols"));
                    }
                    else if (att == "Team number of kevlar")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of kevlar"));
                    }
                    else if (att == "Enemy team number of kevlar")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of kevlar"));
                    }
                    else if (att == "Team number of helmets")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of helmets"));
                    }
                    else if (att == "Enemy team number of helmets")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of helmets"));
                    }
                    else if (att == "Team number of smoke grenades used")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of smoke grenades used"));
                    }
                    else if (att == "Enemy team number of smoke grenades used")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of smoke grenades used"));
                    }
                    else if (att == "Team number of flashes used")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of flashes used"));
                    }
                    else if (att == "Enemy team number of flashes used")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of flashes used"));
                    }
                    else if (att == "Team number of molotov used")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of molotov used"));
                    }
                    else if (att == "Enemy team number of molotov used")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of molotov used"));
                    }
                    else if (att == "Team number of HE used")
                    {
                        newRound.SetValue(att, this.GetValue("Enemy team number of HE used"));
                    }
                    else if (att == "Enemy team number of HE used")
                    {
                        newRound.SetValue(att, this.GetValue("Team number of HE used"));
                    }
                    else if (att == "Team entry kill")
                    {
                        if(this.GetValue("Team entry kill") == "Yes")
                        {
                            newRound.SetValue(att, "No");
                        }
                        else
                        {
                            newRound.SetValue(att, "Yes");
                        }
                    }
                    else if (att == "Round end by death")
                    {
                        newRound.SetValue(att, this.GetValue("Round end by death"));
                    }
                    else if (att == "Bomb exploded")
                    {
                        newRound.SetValue(att, this.GetValue("Bomb exploded"));
                    }
                    else if (att == "Bomb defuses")
                    {
                        newRound.SetValue(att, this.GetValue("Bomb defused"));
                    }
                    else if (att == "Time ran out")
                    {
                        newRound.SetValue(att, this.GetValue("Time ran out"));
                    }
                    else if (att == "Class")
                    {
                        if(this.GetValue("Class") == "Win")
                        {
                            newRound.SetValue(att, "Loss");
                        }
                        else
                        {
                            newRound.SetValue(att, "Win");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error when reversing the round????????");
                    }
                }   
            }

            return newRound;
        }

        /// <summary>
        /// Clear the values of each attribute
        /// </summary>
        public void ClearValues()
        {
            foreach (KeyValuePair<string, RoundAttribute> kvp in m_attributes)
            {
                kvp.Value.m_value = "";
            }
        }

        /// <summary>
        /// Disables every attribute.
        /// </summary>
        public void ClearAttributes()
        {
            foreach (KeyValuePair<string, RoundAttribute> ra in m_attributes)
            {
                ra.Value.m_isEnabled = false;
                ra.Value.m_value = null;
            }
            m_numberOfEnabledAttributes = 0;
        }

        private class RoundAttribute
        {
            public bool m_isEnabled { get; set; }
            public string m_value { get; set; }

            public RoundAttribute(bool isEnabled)
            {
                m_isEnabled = isEnabled;
                m_value = "";
            }
        }
    }
}
