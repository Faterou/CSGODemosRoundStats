using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGODemosDatabaseCreation
{
    class Logger
    {
        private static Logger m_instance;

        private string m_filename;

        static readonly object m_instanceLock = new object();

        private Logger()
        {
            m_filename = "Logs.txt";

            System.Text.UTF8Encoding utf8NoBom = new System.Text.UTF8Encoding(false);
            using (StreamWriter writer = new StreamWriter(m_filename, false, utf8NoBom))
            {
            }
        }

        public static Logger GetInstance()
        {
            if (m_instance == null)
            {
                lock(m_instanceLock)
                {
                    if(m_instance == null)
                    {
                        m_instance = new Logger();
                    }
                }
            }

            return m_instance;
        }

        public void Log(string toLog)
        {
            System.Text.UTF8Encoding utf8NoBom = new System.Text.UTF8Encoding(false);
            using (StreamWriter writer = new StreamWriter(m_filename, true, utf8NoBom))
            {
                writer.WriteLine(toLog);
            }
            Console.WriteLine(toLog);
        }
    }
}
