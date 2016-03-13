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
                "Team number of rifles", "Enemy team number of rifles", "Team number of AWPs", "Enemy team number of AWPs", "Team number of shotguns",
                "Team number of SMGs", "Enemy team number of SMGs", "Team number of machine guns", "Enemy team number of machine guns",
                "Team number of upgraded pistols", "Enemy team number of upgraded pistols", "Team number of kevlar", "Enemy team number of kevlar",
                "Team number of helmets", "Enemy team number of helmets", "Team number of smoke grenades used", "Enemy team number of smoke grenades used",
                "Team number of flashes used", "Enemy team number of flashes used", "Team number of molotov used", "Enemy team number of molotov used",
                "Team number of HE used", "Enemy team number of HE used", "Team entry kill", "Round end by death", "Bomb exploded", "Bomb was defused",
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
        /// Clear the values of each attribute
        /// </summary>
        public void ClearValues()
        {
            foreach (KeyValuePair<string, RoundAttribute> kvp in m_attributes)
            {
                kvp.Value.m_value = null;
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
                m_value = null;
            }
        }
    }
}
