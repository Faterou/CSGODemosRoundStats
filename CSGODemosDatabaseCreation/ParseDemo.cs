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

        //The current round stats
        private Round m_currentRound;

        //True if the sides have to be switched, false otherwise
        private bool m_sideSwitch;

        //Association of a player with his total cash spent at the beginning of the round (used to get the guns bought after free time end)
        private Dictionary<Player, int> m_totalCashSpent;

        //Association of a player and if he has a molotov at the moment (used to get the number of molotov used)
        private Dictionary<Player, bool> m_hasMolotov;

        //True when the round is rolling (after freezetime)
        private bool m_roundRolling;

        //Writer
        private RoundPrinter m_printer;

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
            m_printer = null;
            m_currentRound = new Round();
            m_sideSwitch = false;
            m_totalCashSpent = new Dictionary<Player, int>();
            m_roundRolling = false;
            m_hasMolotov = new Dictionary<Player, bool>();
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
            m_parser.SmokeNadeStarted += CatchSmokeNadeStarted;
            m_parser.FlashNadeExploded += CatchFlashNadeExploded;
            m_parser.ExplosiveNadeExploded += CatchExplosiveNadeExploded;
            m_parser.LastRoundHalf += CatchLastRoundHalf;
            m_parser.RoundEnd += CatchRoundEnd;
            m_parser.TickDone += CatchTickDone;

            string[] attributes = {"Team name", "Enemy team name", "Map", "Team side", "Team equipment value", "Enemy team equipment value",
                "Team number of rifles", "Enemy team number of rifles", "Team number of AWPs", "Enemy team number of AWPs", "Team number of shotguns", "Enemy team number of shotguns",
                "Team number of SMGs", "Enemy team number of SMGs", "Team number of machine guns", "Enemy team number of machine guns",
                "Team number of upgraded pistols", "Enemy team number of upgraded pistols", "Team number of kevlar", "Enemy team number of kevlar",
                "Team number of helmets", "Enemy team number of helmets", "Team number of smoke grenades used", "Enemy team number of smoke grenades used",
                "Team number of flashes used", "Enemy team number of flashes used", "Team number of molotov used", "Enemy team number of molotov used",
                "Team number of HE used", "Enemy team number of HE used", "Team entry kill", "Round end by death", "Bomb exploded", "Bomb was defused",
                "Time ran out", "Class" };

            m_printer = new RoundPrinter(attributes, "OutputDatabaseFile.csv");

            m_parser.ParseToEnd();

            return true;
        }

        /// <summary>
        /// Function that catches a MatchStartedEvent. MatchRoundStart for the first round happens before this event.
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchMatchStarted(object sender, MatchStartedEventArgs e)
        {
            m_hasMatchStarted = true;
            m_team = new ProfessionalTeam(((DemoParser)sender).CTClanName, Team.CounterTerrorist, m_date);
            m_enemyTeam = new ProfessionalTeam(((DemoParser)sender).TClanName, Team.Terrorist, m_date);

            foreach(Player p in ((DemoParser)sender).PlayingParticipants)
            {
                m_totalCashSpent.Add(p, 0);
                m_hasMolotov.Add(p, false);
            }
        }

        /// <summary>
        /// Function that catches a FreezetimeEndedEvent
        /// This is where we calculate the equipment value of each teams
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchFreezetimeEnded(object sender, FreezetimeEndedEventArgs e)
        {
            if (m_hasMatchStarted)
            {
                RecordEquipment();
                m_roundRolling = true;
            }
        }

        /// <summary>
        /// Function that catches a RoundStartedEvent
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchRoundStart(object sender, RoundStartedEventArgs e)
        {
            if(m_hasMatchStarted)
            {
                if(m_printer.PrintRound(m_currentRound.GetValues()) && m_printer.PrintRound(m_currentRound.ReverseRound().GetValues()))
                {
                    m_currentRound.ClearValues();
                }
                else
                {
                    Console.WriteLine("WHOOPS PROBLEM WHEN PRINTING");
                }
            }

            if(m_sideSwitch)
            {
                m_team.SwitchSide();
                m_enemyTeam.SwitchSide();
                m_sideSwitch = false;
            }
        }

        /// <summary>
        /// Function that catches a RoundEndedEvent
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchRoundEnd(object sender, RoundEndedEventArgs e)
        {
            m_roundRolling = false;
            foreach(Player p in m_hasMolotov.Keys.ToList()) {
                m_hasMolotov[p] = false;
            }
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
        /// Function that triggers when a smoke is thrown
        /// It adds 1 to the attribute number of smoke grenades used
        /// </summary>
        /// <param name="sender">the parser</param>
        /// <param name="e">args</param>
        private void CatchSmokeNadeStarted(object sender, SmokeEventArgs e)
        {
            if(e.ThrownBy.Team == m_team.m_side)
            {
                try
                {
                    m_currentRound.SetValue("Team number of smoke grenades used", (Int32.Parse(m_currentRound.GetValue("Team number of smoke grenades used")) + 1).ToString());
                }
                catch(FormatException)
                {
                    m_currentRound.SetValue("Team number of smoke grenades used", "0");
                }
            }
            else
            {
                try
                {
                    m_currentRound.SetValue("Enemy team number of smoke grenades used", (Int32.Parse(m_currentRound.GetValue("Enemy team number of smoke grenades used")) + 1).ToString());
                }
                catch(FormatException)
                {
                    m_currentRound.SetValue("Enemy team number of smoke grenades used", "0");
                }
            }
        }

        /// <summary>
        /// Triggers when a flash explodes
        /// It adds 1 to the attribute number of flashes used
        /// </summary>
        /// <param name="sender">the parser</param>
        /// <param name="e">args</param>
        private void CatchFlashNadeExploded(object sender, FlashEventArgs e)
        {
            if (e.ThrownBy.Team == m_team.m_side)
            {
                try
                {
                    m_currentRound.SetValue("Team number of flashes used", (Int32.Parse(m_currentRound.GetValue("Team number of flashes used")) + 1).ToString());
                }
                catch (FormatException)
                {
                    m_currentRound.SetValue("Team number of flashes used", "0");
                }
            }
            else
            {
                try
                {
                    m_currentRound.SetValue("Enemy team number of flashes used", (Int32.Parse(m_currentRound.GetValue("Enemy team number of flashes used")) + 1).ToString());
                }
                catch (FormatException)
                {
                    m_currentRound.SetValue("Enemy team number of flashes used", "0");
                }
            }
        }

        /// <summary>
        /// Triggers when an HE explodes
        /// It adds 1 to the attribute number of HE used
        /// </summary>
        /// <param name="sender">the parser</param>
        /// <param name="e">args</param>
        private void CatchExplosiveNadeExploded(object sender, GrenadeEventArgs e)
        {
            if (e.ThrownBy.Team == m_team.m_side)
            {
                try
                {
                    m_currentRound.SetValue("Team number of HE used", (Int32.Parse(m_currentRound.GetValue("Team number of HE used")) + 1).ToString());
                }
                catch (FormatException)
                {
                    m_currentRound.SetValue("Team number of HE used", "0");
                }
            }
            else
            {
                try
                {
                    m_currentRound.SetValue("Enemy team number of HE used", (Int32.Parse(m_currentRound.GetValue("Enemy team number of HE used")) + 1).ToString());
                }
                catch (FormatException)
                {
                    m_currentRound.SetValue("Enemy team number of HE used", "0");
                }
            }
        }

        /// <summary>
        /// Triggers when a molotov is started
        /// It adds 1 to the attribute number of HE used
        /// </summary>
        /// <param name="p">the player who threw the molotov</param>
        private void CatchFireNadeStarted(Player p)
        {
            if (p.Team == m_team.m_side)
            {
                try
                {
                    m_currentRound.SetValue("Team number of molotov used", (Int32.Parse(m_currentRound.GetValue("Team number of molotov used")) + 1).ToString());
                }
                catch (FormatException)
                {
                    m_currentRound.SetValue("Team number of molotov used", "0");
                }
            }
            else
            {
                try
                {
                    m_currentRound.SetValue("Enemy team number of molotov used", (Int32.Parse(m_currentRound.GetValue("Enemy team number of molotov used")) + 1).ToString());
                }
                catch (FormatException)
                {
                    m_currentRound.SetValue("Enemy team number of molotov used", "0");
                }
            }
        }

        /// <summary>
        /// Catches the beginning of the last round of the half after the RoundStart event
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchLastRoundHalf(object sender, LastRoundHalfEventArgs e)
        {
            m_sideSwitch = true;
        }

        private void CatchTickDone(object sender, TickDoneEventArgs e)
        {
            if(m_roundRolling)
            {
                foreach(Player p in ((DemoParser)sender).PlayingParticipants)
                {
                    if(p.AdditionaInformations.TotalCashSpent > m_totalCashSpent[p])
                    {
                        RecordEquipment();
                    }

                    if((p.Weapons.Where(weapon => weapon.Weapon == EquipmentElement.Molotov || weapon.Weapon == EquipmentElement.Incendiary)).Count() > 0)
                    {
                        m_hasMolotov[p] = true;
                    }
                    else if(m_hasMolotov[p])
                    {
                        if(p.IsAlive)
                        {
                            CatchFireNadeStarted(p);
                        }
                        m_hasMolotov[p] = false;
                    }
                }
            }
        }

        /// <summary>
        /// Resets the parser
        /// </summary>
        private void resetParser()
        {

        }

        


        /// <summary>
        /// Records the equipment values and the weapons bought
        /// </summary>
        private void RecordEquipment()
        {
            int teamEquipmentValue = 0;
            int enemyTeamEquipmentValue = 0;

            int teamNumberOfRifles = 0;
            int enemyTeamNumberOfRifles = 0;

            int teamNumberOfAWPs = 0;
            int enemyTeamNumberOfAWPs = 0;

            int teamNumberOfShotguns = 0;
            int enemyTeamNumberOfShotguns = 0;

            int teamNumberOfMachineGuns = 0;
            int enemyTeamNumberOfMachineGuns = 0;

            int teamNumberOfSMGs = 0;
            int enemyTeamNumberOfSMGs = 0;

            int teamNumberOfUpgradedPistols = 0;
            int enemyTeamNumberOfUpgradedPistols = 0;

            int teamNumberOfKevlar = 0;
            int enemyTeamNumberOfKevlar = 0;

            int teamNumberOfHelmets = 0;
            int enemyTeamNumberOfHelmets = 0;

            foreach (Player p in m_parser.PlayingParticipants)
            {
                if (p.Team == m_team.m_side)
                {
                    //Let's record the equipment value
                    teamEquipmentValue += p.CurrentEquipmentValue;
                    m_totalCashSpent[p] = p.AdditionaInformations.TotalCashSpent;

                    //Let's record the weapons
                    foreach (Equipment equipment in p.Weapons)
                    {
                        if (equipment.Class == EquipmentClass.Rifle && equipment.Weapon != EquipmentElement.AWP)
                        {
                            teamNumberOfRifles++;
                        }
                        else if (equipment.Class == EquipmentClass.Rifle && equipment.Weapon == EquipmentElement.AWP)
                        {
                            teamNumberOfAWPs++;
                        }
                        else if (equipment.Class == EquipmentClass.Heavy && equipment.Weapon != EquipmentElement.M249 && equipment.Weapon != EquipmentElement.Negev)
                        {
                            teamNumberOfShotguns++;
                        }
                        else if (equipment.Class == EquipmentClass.Heavy && (equipment.Weapon == EquipmentElement.M249 || equipment.Weapon == EquipmentElement.Negev))
                        {
                            teamNumberOfMachineGuns++;
                        }
                        else if (equipment.Class == EquipmentClass.SMG)
                        {
                            teamNumberOfSMGs++;
                        }
                        else if (equipment.Class == EquipmentClass.Pistol && equipment.Weapon != EquipmentElement.USP && equipment.Weapon != EquipmentElement.Glock && equipment.Weapon != EquipmentElement.P2000)
                        {
                            teamNumberOfUpgradedPistols++;
                        }
                    }

                    //Let's record the armor
                    if (p.Armor > 0)
                    {
                        teamNumberOfKevlar++;
                    }
                    else if (p.HasHelmet)
                    {
                        teamNumberOfHelmets++;
                    }
                }
                else
                {
                    enemyTeamEquipmentValue += p.CurrentEquipmentValue;
                    m_totalCashSpent[p] = p.AdditionaInformations.TotalCashSpent;

                    foreach (Equipment equipment in p.Weapons)
                    {
                        if (equipment.Class == EquipmentClass.Rifle && equipment.Weapon != EquipmentElement.AWP)
                        {
                            enemyTeamNumberOfRifles++;
                        }
                        else if (equipment.Class == EquipmentClass.Rifle && equipment.Weapon == EquipmentElement.AWP)
                        {
                            enemyTeamNumberOfAWPs++;
                        }
                        else if (equipment.Class == EquipmentClass.Heavy && equipment.Weapon != EquipmentElement.M249 && equipment.Weapon != EquipmentElement.Negev)
                        {
                            enemyTeamNumberOfShotguns++;
                        }
                        else if (equipment.Class == EquipmentClass.Heavy && (equipment.Weapon == EquipmentElement.M249 || equipment.Weapon == EquipmentElement.Negev))
                        {
                            enemyTeamNumberOfMachineGuns++;
                        }
                        else if (equipment.Class == EquipmentClass.SMG)
                        {
                            enemyTeamNumberOfSMGs++;
                        }
                        else if (equipment.Class == EquipmentClass.Pistol && equipment.Weapon != EquipmentElement.USP && equipment.Weapon != EquipmentElement.Glock && equipment.Weapon != EquipmentElement.P2000)
                        {
                            enemyTeamNumberOfUpgradedPistols++;
                        }
                    }

                    if (p.Armor > 0)
                    {
                        enemyTeamNumberOfKevlar++;
                    }
                    else if (p.HasHelmet)
                    {
                        enemyTeamNumberOfHelmets++;
                    }
                }
            }
            m_currentRound.SetValue("Team name", m_team.GetName());
            m_currentRound.SetValue("Enemy team name", m_enemyTeam.GetName());
            m_currentRound.SetValue("Team side", m_team.m_side.ToString());
            m_currentRound.SetValue("Map", m_map);

            m_currentRound.SetValue("Team equipment value", teamEquipmentValue.ToString());
            m_currentRound.SetValue("Enemy team equipment value", enemyTeamEquipmentValue.ToString());
            m_currentRound.SetValue("Team number of rifles", teamNumberOfRifles.ToString());
            m_currentRound.SetValue("Enemy team number of rifles", enemyTeamNumberOfRifles.ToString());
            m_currentRound.SetValue("Team number of AWPs", teamNumberOfAWPs.ToString());
            m_currentRound.SetValue("Enemy team number of AWPs", enemyTeamNumberOfAWPs.ToString());
            m_currentRound.SetValue("Team number of shotguns", teamNumberOfShotguns.ToString());
            m_currentRound.SetValue("Enemy team number of shotguns", enemyTeamNumberOfShotguns.ToString());
            m_currentRound.SetValue("Team number of SMGs", teamNumberOfSMGs.ToString());
            m_currentRound.SetValue("Enemy team number of SMGs", enemyTeamNumberOfSMGs.ToString());
            m_currentRound.SetValue("Team number of machine guns", teamNumberOfMachineGuns.ToString());
            m_currentRound.SetValue("Enemy team number of machine guns", enemyTeamNumberOfMachineGuns.ToString());
            m_currentRound.SetValue("Team number of upgraded pistols", teamNumberOfUpgradedPistols.ToString());
            m_currentRound.SetValue("Enemy team number of upgraded pistols", enemyTeamNumberOfUpgradedPistols.ToString());
            m_currentRound.SetValue("Team number of kevlar", teamNumberOfKevlar.ToString());
            m_currentRound.SetValue("Enemy team number of kevlar", enemyTeamNumberOfKevlar.ToString());
            m_currentRound.SetValue("Team number of helmets", teamNumberOfHelmets.ToString());
            m_currentRound.SetValue("Enemy team number of helmets", enemyTeamNumberOfHelmets.ToString());
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
            else if (Regex.Match(filename, "ESLESEADubai").Success)
            {
                return new DateTime(2015, 9, 10);
            }
            else if (Regex.Match(filename, "ESLOneCologne2015").Success)
            {
                return new DateTime(2015, 8, 20);
            }
            return new DateTime();
        }
    }
}
