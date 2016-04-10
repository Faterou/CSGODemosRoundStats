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

        //The current rounds of the match
        private Match m_match;

        //True if the sides have to be switched, false otherwise
        private bool m_sideSwitch;

        //Association of a player with his total cash spent at the beginning of the round (used to get the guns bought after free time end)
        private Dictionary<Player, int> m_totalCashSpent;

        //Association of a player and if he has a molotov at the moment (used to get the number of molotov used)
        private Dictionary<Player, bool> m_hasMolotov;

        //The first player has already been killed
        private bool m_firstPlayerKilled;

        //True when the round is rolling (after freezetime)
        private bool m_roundRolling;

        //True when the round is paused (something happened unusual)
        private bool m_isPaused;

        //Number of rounds played
        private int m_roundsPlayed;

        //Attributes
        private string[] m_attributes;

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
            m_currentRound = null;
            m_sideSwitch = false;
            m_totalCashSpent = new Dictionary<Player, int>();
            m_roundRolling = false;
            m_hasMolotov = new Dictionary<Player, bool>();
            m_firstPlayerKilled = false;
            m_isPaused = true;
            m_match = new Match();
            m_roundsPlayed = 0;
        }


        /// <summary>
        /// Parses a demo
        /// </summary>
        /// <param name="outputDatabaseFilename">Filename of the output file</param>
        /// <param name="demoFilename">Filename of the demo file</param>
        /// <returns>Returns true if the demo was parsed correctly.</returns>
        public bool ParseADemo(string outputDatabaseFilename, string demoFilename, string[] attributes)
        {
            m_date = GetDateOfEvent(demoFilename);

            m_attributes = attributes;

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
            m_parser.PlayerTeam += CatchPlayerTeam;
            m_parser.RoundOfficiallyEnd += CatchRoundOfficiallyEnd;

            m_currentRound = new Round(m_attributes);

            m_printer = new RoundPrinter(m_currentRound.GetAttributes(), outputDatabaseFilename);

            m_parser.ParseToEnd();

            foreach(Round r in m_match.GetAllRounds())
            {
                m_printer.PrintRound(r.GetValues());
                m_printer.PrintRound(r.ReverseRound().GetValues());
            }

            resetParser();

            return true;
        }

        /// <summary>
        /// Catches when a someone chooses a team
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">args</param>
        private void CatchPlayerTeam(object sender, PlayerTeamEventArgs e)
        {
            if(m_hasMatchStarted && e.NewTeam != Team.Spectate)
            {
                m_totalCashSpent = new Dictionary<Player, int>();
                m_hasMolotov = new Dictionary<Player, bool>();

                foreach (Player p in ((DemoParser)sender).PlayingParticipants)
                {
                    m_totalCashSpent.Add(p, 0);
                    m_hasMolotov.Add(p, false);
                }
            }
        }

        /// <summary>
        /// Function that catches a MatchStartedEvent. MatchRoundStart for the first round happens before this event.
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchMatchStarted(object sender, MatchStartedEventArgs e)
        {
            m_hasMatchStarted = true;
            m_isPaused = false;
            m_team = new ProfessionalTeam(((DemoParser)sender).CTClanName, Team.CounterTerrorist, m_date);
            m_enemyTeam = new ProfessionalTeam(((DemoParser)sender).TClanName, Team.Terrorist, m_date);

            m_totalCashSpent = new Dictionary<Player, int>();
            m_hasMolotov = new Dictionary<Player, bool>();

            foreach (Player p in ((DemoParser)sender).PlayingParticipants)
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
                m_totalCashSpent = new Dictionary<Player, int>();
                m_hasMolotov = new Dictionary<Player, bool>();

                foreach (Player p in ((DemoParser)sender).PlayingParticipants)
                {
                    m_totalCashSpent.Add(p, 0);
                    m_hasMolotov.Add(p, false);
                }

                if (m_roundsPlayed != m_parser.CTScore + m_parser.TScore)
                {
                    m_match.RemoveLastRound();
                    m_roundsPlayed = m_parser.CTScore + m_parser.TScore;
                }

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
            if(m_isPaused)
            {
                m_isPaused = false;
            }

            if(m_hasMatchStarted)
            {
                if (m_sideSwitch)
                {
                    m_team.SwitchSide();
                    m_enemyTeam.SwitchSide();
                    m_sideSwitch = false;
                }
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
            m_firstPlayerKilled = false;
            foreach(Player p in m_hasMolotov.Keys.ToList()) {
                m_hasMolotov[p] = false;
            }

            if(m_hasMatchStarted)
            {
                if (Regex.Match(e.Message, "CTs_Win").Success)
                {
                    m_currentRound.SetValue("Win condition", "Death");

                    if (m_team.m_side == Team.CounterTerrorist)
                    {
                        m_currentRound.SetValue("Class", "Win");
                    }
                    else
                    {
                        m_currentRound.SetValue("Class", "Loss");
                    }
                }
                else if (Regex.Match(e.Message, "Bomb_Defused").Success)
                {
                    m_currentRound.SetValue("Win condition", "Bomb defused");

                    if (m_team.m_side == Team.CounterTerrorist)
                    {
                        m_currentRound.SetValue("Class", "Win");
                    }
                    else
                    {
                        m_currentRound.SetValue("Class", "Loss");
                    }
                }
                else if (Regex.Match(e.Message, "Terrorists_Win").Success)
                {
                    m_currentRound.SetValue("Win condition", "Death");

                    if (m_team.m_side == Team.CounterTerrorist)
                    {
                        m_currentRound.SetValue("Class", "Loss");
                    }
                    else
                    {
                        m_currentRound.SetValue("Class", "Win");
                    }
                }
                else if(Regex.Match(e.Message, "Target_Bombed").Success)
                {
                    m_currentRound.SetValue("Win condition", "Bomb exploded");

                    if (m_team.m_side == Team.CounterTerrorist)
                    {
                        m_currentRound.SetValue("Class", "Loss");
                    }
                    else
                    {
                        m_currentRound.SetValue("Class", "Win");
                    }
                }
                else
                {
                    m_currentRound.SetValue("Win condition", "Time ran out");

                    if (m_team.m_side == Team.CounterTerrorist)
                    {
                        m_currentRound.SetValue("Class", "Win");
                    }
                    else
                    {
                        m_currentRound.SetValue("Class", "Loss");
                    }
                }

                m_match.AddRound(m_currentRound);
                m_currentRound = new Round(m_attributes);
            }
        }

        /// <summary>
        /// Catches when a round offically ends (just before the next round starts)
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">args</param>
        private void CatchRoundOfficiallyEnd(object sender, RoundOfficiallyEndedEventArgs e)
        {
            m_roundsPlayed++;
        }

        /// <summary>
        /// Function that triggers when a player is killed.
        /// Catches players killed even during warmup and knife rounds
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">Args</param>
        private void CatchPlayerKilled(object sender, PlayerKilledEventArgs e)
        {
            if(m_hasMatchStarted)
            {
                if(!m_firstPlayerKilled)
                {
                    if((e.Killer != null && e.Killer.Team == m_team.m_side) || (e.Victim.Team != m_team.m_side))
                    {
                        m_currentRound.SetValue("Team entry kill", "Yes");
                    }
                    m_firstPlayerKilled = true;
                }
            }
        }

        /// <summary>
        /// Function that triggers when a smoke is thrown
        /// It adds 1 to the attribute number of smoke grenades used
        /// </summary>
        /// <param name="sender">the parser</param>
        /// <param name="e">args</param>
        private void CatchSmokeNadeStarted(object sender, SmokeEventArgs e)
        {
            if(m_hasMatchStarted)
            {
                if (e.ThrownBy.Team == m_team.m_side)
                {
                    try
                    {
                        m_currentRound.SetValue("Team number of smoke grenades used", (Int32.Parse(m_currentRound.GetValue("Team number of smoke grenades used")) + 1).ToString());
                    }
                    catch(FormatException)
                    {

                    }
                }
                else
                {
                    try
                    {
                        m_currentRound.SetValue("Enemy team number of smoke grenades used", (Int32.Parse(m_currentRound.GetValue("Enemy team number of smoke grenades used")) + 1).ToString());
                    }
                    catch (FormatException)
                    {

                    }
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
            if (m_hasMatchStarted)
            {
                if (e.ThrownBy.Team == m_team.m_side)
                {
                    try
                    {
                        m_currentRound.SetValue("Team number of flashes used", (Int32.Parse(m_currentRound.GetValue("Team number of flashes used")) + 1).ToString());
                    }
                    catch(FormatException)
                    {

                    }
                }
                else
                {
                    try
                    {
                        m_currentRound.SetValue("Enemy team number of flashes used", (Int32.Parse(m_currentRound.GetValue("Enemy team number of flashes used")) + 1).ToString());
                    }
                    catch(FormatException)
                    {

                    }
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
            if(m_hasMatchStarted)
            {
                if (e.ThrownBy.Team == m_team.m_side)
                {
                    try
                    {
                        m_currentRound.SetValue("Team number of HE used", (Int32.Parse(m_currentRound.GetValue("Team number of HE used")) + 1).ToString());
                    }
                    catch (FormatException)
                    {

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

                    }
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
            if(m_hasMatchStarted)
            {
                if (p.Team == m_team.m_side)
                {
                    try
                    {
                        m_currentRound.SetValue("Team number of molotov used", (Int32.Parse(m_currentRound.GetValue("Team number of molotov used")) + 1).ToString());
                    }
                    catch (FormatException)
                    {

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

                    }
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

        /// <summary>
        /// Catches the end of a tick
        /// </summary>
        /// <param name="sender">The parser</param>
        /// <param name="e">args</param>
        private void CatchTickDone(object sender, TickDoneEventArgs e)
        {
            if(m_roundRolling)
            {
                foreach(Player p in ((DemoParser)sender).PlayingParticipants)
                {
                    try
                    {
                        if (p.AdditionaInformations.TotalCashSpent > m_totalCashSpent[p])
                        {
                            RecordEquipment();
                        }

                        if ((p.Weapons.Where(weapon => weapon.Weapon == EquipmentElement.Molotov || weapon.Weapon == EquipmentElement.Incendiary)).Count() > 0)
                        {
                            m_hasMolotov[p] = true;
                        }
                        else if (m_hasMolotov[p])
                        {
                            if (p.IsAlive)
                            {
                                CatchFireNadeStarted(p);
                            }
                            m_hasMolotov[p] = false;
                        }
                    }
                    catch(KeyNotFoundException)
                    {
                        pause();
                    }
                }
            }
        }

        /// <summary>
        /// Resets the parser
        /// </summary>
        private void resetParser()
        {
            m_hasMatchStarted = false;
            m_date = new DateTime();
            m_team = null;
            m_enemyTeam = null;
            m_map = "";
            m_printer = null;
            m_currentRound = null;
            m_sideSwitch = false;
            m_totalCashSpent = new Dictionary<Player, int>();
            m_roundRolling = false;
            m_hasMolotov = new Dictionary<Player, bool>();
            m_firstPlayerKilled = false;
            m_isPaused = true;
            m_match = new Match();
            m_roundsPlayed = 0;
        }

        /// <summary>
        /// Puts the parser on "pause"
        /// </summary>
        private void pause()
        {
            m_currentRound = new Round(m_attributes);
            m_roundRolling = false;
            m_isPaused = true;
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

                    if (p.HasHelmet)
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

                    if (p.HasHelmet)
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
            m_currentRound.SetValue("Team number of smoke grenades used", "0");
            m_currentRound.SetValue("Enemy team number of smoke grenades used", "0");
            m_currentRound.SetValue("Team number of flashes used", "0");
            m_currentRound.SetValue("Enemy team number of flashes used", "0");
            m_currentRound.SetValue("Team number of molotov used", "0");
            m_currentRound.SetValue("Enemy team number of molotov used", "0");
            m_currentRound.SetValue("Team number of HE used", "0");
            m_currentRound.SetValue("Enemy team number of HE used", "0");
            m_currentRound.SetValue("Team entry kill", "No");
        }

        /// <summary>
        /// Gets the date of the event with the filename (the events are hard coded, make sure all your filenames countain the name of the events).
        /// </summary>
        /// <param name="filename">Filename of the demo</param>
        /// <returns>Returns the date of the event</returns>
        private DateTime GetDateOfEvent(string filename)
        {
            //Works1
            if(Regex.Match(filename,"IEMKatowice2016").Success)
            {
                return new DateTime(2016, 3, 2);
            }
            //Doesn't work
            else if(Regex.Match(filename, "GEC2016Finals").Success)
            {
                return new DateTime(2016, 2, 4);
            }
            //Works1
            else if (Regex.Match(filename, "IEMSanJose2015").Success)
            {
                return new DateTime(2015, 11, 21);
            }
            //Works1
            else if (Regex.Match(filename, "MLGColumbus2016MainQualifier").Success)
            {
                return new DateTime(2016, 2, 26);
            }
            //Doesn't work, doesn't seem to be any catchmatchstarted event
            else if (Regex.Match(filename, "SLi14Finals").Success)
            {
                return new DateTime(2016, 1, 14);
            }
            //Works1
            else if (Regex.Match(filename, "ESLESEA2Finals").Success)
            {
                return new DateTime(2015, 12, 13);
            }
            //Works1
            else if (Regex.Match(filename, "ESLBarcelonaCSGOInvitational").Success)
            {
                return new DateTime(2016, 2, 19);
            }
            //Works1
            else if (Regex.Match(filename, "DreamHackMastersMalmoNAClosedQualifier").Success)
            {
                return new DateTime(2016, 2, 21);
            }
            //Doesn't work, no matchstarted event
            else if (Regex.Match(filename, "DHLondon2015").Success)
            {
                return new DateTime(2015, 9, 20);
            }
            //Doesn't work, no match started event
            else if (Regex.Match(filename, "DHLeipzig2016").Success)
            {
                return new DateTime(2016, 1, 22);
            }
            //Works1
            else if (Regex.Match(filename, "DHCluj2015").Success)
            {
                return new DateTime(2015, 10, 28);
            }
            //Doesn't work, no match started event
            else if (Regex.Match(filename, "DHCluj2015LANQualifier").Success)
            {
                return new DateTime(2015, 9, 22);
            }
            //Works1
            else if (Regex.Match(filename, "ESLESEADubai").Success)
            {
                return new DateTime(2015, 9, 10);
            }
            //Works1
            else if (Regex.Match(filename, "ESLOneCologne2015").Success)
            {
                return new DateTime(2015, 8, 20);
            }
            //Works1
            else if(Regex.Match(filename, "MLGColumbus2016").Success)
            {
                return new DateTime(2016, 4, 1);
            }
            return new DateTime();
        }
    }
}
