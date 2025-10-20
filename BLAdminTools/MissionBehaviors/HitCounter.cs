using System;
using System.Collections.Generic;
using DoFAdminTools.Database;
using DoFAdminTools.Helpers;
using TaleWorlds.MountAndBlade;
using DoFAdminTools.MissionBehaviors;
using System.ComponentModel.Design;
using System.Linq;
using System.Data.SQLite;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace DoFAdminTools.MissionBehaviors
{
    internal class HitCounter : MissionBehavior
    {
        private StatsManager StatsManager;
        private List<TaleWorlds.MountAndBlade.NetworkCommunicator> players; // List of player names
        private int roundId = 0;
        private static int currentMatchId = 0;
        private long roundStartTime;
        private ConcurrentDictionary<string, int> playerMMR = new ConcurrentDictionary<string, int>();
        private const int DEFAULT_MMR = 1000;
        private const int MMR_CHANGE_BASE = 32;

        public bool Enabled { get; set; }

        private string connectionString = "Data Source=damage_stats.db;Version=3;";
        private static readonly object dbLock = new object();

        public HitCounter()
        {
            StatsManager = new StatsManager();
            players = new List<TaleWorlds.MountAndBlade.NetworkCommunicator>();

            MultiplayerRoundController roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();
            roundController.OnRoundEnding += OnRoundEnd;
            roundController.OnPreparationEnded += OnPreparationEnd;

            Enabled = true;
            LoadPlayerMMR();
            // Initialize roundId from the database
            InitializeRoundId();
            DoFSubModule.EnforceMirrorMatchupOnMapChange = true;
        }

        private void InitializeRoundId()
        {
            ExecuteWithRetry(connection =>
            {
                string query = "SELECT COALESCE(MAX(Round_ID), 0) FROM Rounds;";
                using (var command = new SQLiteCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    roundId = Convert.ToInt32(result) + 1;
                }
            });
        }

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
        public void StorePlayerData(
            int roundId, 
            string playerId, 
            string name, 
            int meleeDamage, 
            int rangedDamage, 
            int throwingDamage, 
            int totalDamage, 
            string team, 
            int kicks, 
            int HP,
            int mountedMeleeDamage,
            int mountedRangedDamage,
            int mountedThrowingDamage,
            int friendlyMeleeDamage,
            int friendlyRangedDamage,
            int friendlyThrowingDamage,
            int friendlyMountedMeleeDamage,
            int friendlyMountedRangedDamage,
            int friendlyMountedThrowingDamage,
            string culture,
            int mmr
            )
        {
            // Add null checks and default values
            roundId = roundId == 0 ? 999 : roundId;
            playerId = playerId ?? "Unknown";
            name = name ?? "Unknown";
            meleeDamage = meleeDamage == 0 ? 999 : meleeDamage;
            rangedDamage = rangedDamage == 0 ? 999 : rangedDamage;
            throwingDamage = throwingDamage == 0 ? 999 : throwingDamage;
            totalDamage = totalDamage == 0 ? 999 : totalDamage;
            team = team ?? "Unknown";
            kicks = kicks == 0 ? 999 : kicks;
            HP = HP == 0 ? 999 : HP;
            mountedMeleeDamage = mountedMeleeDamage == 0 ? 999 : mountedMeleeDamage;
            mountedRangedDamage = mountedRangedDamage == 0 ? 999 : mountedRangedDamage;
            mountedThrowingDamage = mountedThrowingDamage == 0 ? 999 : mountedThrowingDamage;
            friendlyMeleeDamage = friendlyMeleeDamage == 0 ? 999 : friendlyMeleeDamage;
            friendlyRangedDamage = friendlyRangedDamage == 0 ? 999 : friendlyRangedDamage;
            friendlyThrowingDamage = friendlyThrowingDamage == 0 ? 999 : friendlyThrowingDamage;
            friendlyMountedMeleeDamage = friendlyMountedMeleeDamage == 0 ? 999 : friendlyMountedMeleeDamage;
            friendlyMountedRangedDamage = friendlyMountedRangedDamage == 0 ? 999 : friendlyMountedRangedDamage;
            friendlyMountedThrowingDamage = friendlyMountedThrowingDamage == 0 ? 999 : friendlyMountedThrowingDamage;
            culture = culture ?? "Unknown";
            mmr = mmr == 0 ? 999 : mmr;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Insert query for Round_Player table
                string insertQuery = "INSERT INTO Round_Player (" +
                    "Round_ID, " +
                    "Player_ID, " +
                    "Name, " +
                    "Melee_Damage, " +
                    "Ranged_Damage, " +
                    "Throwing_Damage, " +
                    "Total_Damage, " +
                    "Team, " +
                    "Kicks, " +
                    "HP, " +
                    "Mounted_Melee_Damage, " +
                    "Mounted_Ranged_Damage, " +
                    "Mounted_Throwing_Damage, " +
                    "Friendly_Melee_Damage, " +
                    "Friendly_Ranged_Damage, " +
                    "Friendly_Throwing_Damage, " +
                    "Friendly_Mounted_Melee_Damage, " +
                    "Friendly_Mounted_Ranged_Damage, " +
                    "Friendly_Mounted_Throwing_Damage, " +
                    "Culture, " +
                    "MMR" +
                    ") VALUES (" +
                    "@roundId, " +
                    "@playerId, " +
                    "@name, " +
                    "@meleeDamage, " +
                    "@rangedDamage, " +
                    "@throwingDamage, " +
                    "@totalDamage, " +
                    "@team, " +
                    "@kicks, " +
                    "@hp, " +
                    "@mountedMeleeDamage, " +
                    "@mountedRangedDamage, " +
                    "@mountedThrowingDamage, " +
                    "@friendlyMeleeDamage, " +
                    "@friendlyRangedDamage, " +
                    "@friendlyThrowingDamage, " +
                    "@friendlyMountedMeleeDamage, " +
                    "@friendlyMountedRangedDamage, " +
                    "@friendlyMountedThrowingDamage, " +
                    "@culture, " +
                    "@mmr);";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    // Parameters for the query
                    command.Parameters.AddWithValue("@roundId", roundId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@meleeDamage", meleeDamage);
                    command.Parameters.AddWithValue("@rangedDamage", rangedDamage);
                    command.Parameters.AddWithValue("@throwingDamage", throwingDamage);
                    command.Parameters.AddWithValue("@mountedMeleeDamage", mountedMeleeDamage);
                    command.Parameters.AddWithValue("@mountedRangedDamage", mountedRangedDamage);
                    command.Parameters.AddWithValue("@mountedThrowingDamage", mountedThrowingDamage);
                    command.Parameters.AddWithValue("@friendlyMeleeDamage", friendlyMeleeDamage);
                    command.Parameters.AddWithValue("@friendlyRangedDamage", friendlyRangedDamage);
                    command.Parameters.AddWithValue("@friendlyThrowingDamage", friendlyThrowingDamage);
                    command.Parameters.AddWithValue("@friendlyMountedMeleeDamage", friendlyMountedMeleeDamage);
                    command.Parameters.AddWithValue("@friendlyMountedRangedDamage", friendlyMountedRangedDamage);
                    command.Parameters.AddWithValue("@friendlyMountedThrowingDamage", friendlyMountedThrowingDamage);
                    command.Parameters.AddWithValue("@totalDamage", totalDamage);
                    // command.Parameters.AddWithValue("@shots", team);
                    // command.Parameters.AddWithValue("@hits", team);
                    // command.Parameters.AddWithValue("@shieldhits", team);
                    // command.Parameters.AddWithValue("@horsehits", team);
                    // command.Parameters.AddWithValue("@headshots", team);
                    command.Parameters.AddWithValue("@kicks", kicks);
                    command.Parameters.AddWithValue("@culture", culture);
                    // command.Parameters.AddWithValue("@class", clase);
                    command.Parameters.AddWithValue("@team", team);
                    command.Parameters.AddWithValue("@hp", HP);
                    command.Parameters.AddWithValue("@mmr", mmr);

                    // Execute the insert command
                    command.ExecuteNonQuery();
                }
            }
        }
        public void OnPreparationEnd()
        {
            try
            {
                players.Clear();
                Dictionary<string, TeamInfo> teamInfos = new Dictionary<string, TeamInfo>();

                foreach (var peer in GameNetwork.NetworkPeers)
                {
                    if (!peer.IsSynchronized)
                        continue;

                    var missionPeer = peer.GetComponent<MissionPeer>();
                    if (missionPeer == null || missionPeer.Team is null || missionPeer.Team == Mission.Current.SpectatorTeam)
                        continue;

                    players.Add(peer);

                    string teamKey = missionPeer.Team.Side.ToString();
                    int playerMMR = this.playerMMR.GetOrAdd(peer.VirtualPlayer.Id.ToString(), DEFAULT_MMR);

                    if (!teamInfos.ContainsKey(teamKey))
                    {
                        teamInfos[teamKey] = new TeamInfo
                        {
                            CultureName = missionPeer.Culture.GetName().ToString(),
                            PrimaryColor = missionPeer.Culture.Color,
                            SecondaryColor = missionPeer.Culture.Color2,
                            AltPrimaryColor = missionPeer.Culture.ClothAlternativeColor,
                            AltSecondaryColor = missionPeer.Culture.ClothAlternativeColor2,
                            SideName = missionPeer.Team.Side.ToString()
                        };
                    }
                    teamInfos[teamKey].PlayerMMRs.Add(playerMMR);
                    teamInfos[teamKey].Players.Add(peer);
                }

                if (teamInfos.Count == 2)
                {
                    var teams = teamInfos.Keys.ToList();
                    TeamInfo team1 = teamInfos[teams[0]];
                    TeamInfo team2 = teamInfos[teams[1]];

                    int totalPlayers = team1.TeamSize + team2.TeamSize;

                    // Calculate total MMR for each team
                    int team1TotalMMR = team1.PlayerMMRs.Sum();
                    int team2TotalMMR = team2.PlayerMMRs.Sum();

                    // Calculate adjusted team MMRs
                    double team1AdjustedMMR = team1TotalMMR / Math.Min(team1.TeamSize, team2.TeamSize);
                    double team2AdjustedMMR = team2TotalMMR / Math.Min(team1.TeamSize, team2.TeamSize);

                    // Calculate expected scores (win probabilities)
                    double team1WinProb = 1 / (1 + Math.Pow(10, (team2AdjustedMMR - team1AdjustedMMR) / 400.0));
                    double team2WinProb = 1 - team1WinProb;

                    string team1Identifier = char.ToUpper(DetermineTeamIdentifier(team1, team2)[0]) + DetermineTeamIdentifier(team1, team2).Substring(1);
                    string team2Identifier = char.ToUpper(DetermineTeamIdentifier(team2, team1)[0]) + DetermineTeamIdentifier(team2, team1).Substring(1);

                    Helper.SendMessageToAllPeers($"{team1Identifier} Team: MMR {team1.AverageMMR:F0} | Players: {team1.TeamSize} | Win Chance: {team1WinProb:P1}");
                    Helper.SendMessageToAllPeers($"{team2Identifier} Team: MMR {team2.AverageMMR:F0} | Players: {team2.TeamSize} | Win Chance: {team2WinProb:P1}");
                }
                else
                {
                    foreach (var team in teamInfos)
                    {
                        Helper.SendMessageToAllPeers($"{team.Value.CultureName} Team: MMR {team.Value.AverageMMR:F0} | Players: {team.Value.TeamSize}");
                    }
                }

                StatsManager.InitializePlayerStats(players);
                roundStartTime = (long)Mission.Current.CurrentTime;
                StatsManager.SetRoundStartTime(roundStartTime);
                roundId++;
            }
            catch 
            {
                Helper.SendMessageToAllPeers("Round starting script didn't work");
            }
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            currentMatchId++;
        }
        // Update stats when an agent is hit
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);

            try { 
            MultiplayerRoundController roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();
            
            if (
                affectorAgent != null && 
                !affectorAgent.IsAIControlled && 
                affectedAgent.IsPlayerControlled && 
                affectorAgent.IsPlayerControlled && 
                roundController.IsRoundInProgress && 
                !attackCollisionData.MissileBlockedWithWeapon &&
                !attackCollisionData.AttackBlockedWithShield
                )
            {
                // Update the attacker's damage dealt
                StatsManager.UpdatePlayerStats(affectorAgent, affectedAgent, blow);
            }
            }
            catch {
                Helper.SendMessageToAllPeers("Error");
            }
        }

        // Display final stats at the end of the round
        public void OnRoundEnd()
        {
            try
            {
                Dictionary<string, PlayerStats> playerStats = StatsManager.GetPlayerStats();
                
                // Send a message with each player's stats
                if ( playerStats.Count <= 5 )
                {
                    foreach (var player in playerStats)
                    {
                        string message = $"{player.Value.Name} | Dmg: {player.Value.DamageDealt} | HP: {player.Value.Health} | Kicks: {player.Value.Kicks}";
                        Helper.SendMessageToAllPeers(message);
                    }
                } 
                else if ( playerStats.Count > 5 ) 
                {
                    var bestPlayers = playerStats
                        .OrderByDescending(entry => entry.Value.DamageDealt)
                        .Take(5)
                        .ToDictionary(pair => pair.Key, pair => pair.Value);

                    foreach (var player in bestPlayers)
                    {

                        string message = $"{player.Value.Name} | Dmg: {player.Value.DamageDealt} | HP: {player.Value.Health} | Kicks: {player.Value.Kicks}";
                        Helper.SendMessageToAllPeers(message);
                    }
                }

                MultiplayerRoundController roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();
                if (roundController == null)
                {
                    Helper.PrintWarning("RoundController is null");
                    return;
                }

                string winningTeam = roundController.RoundWinner.ToString() ?? "Draw";
                UpdateMMR(winningTeam);

                lock (dbLock)
                {
                    ExecuteWithRetry(connection =>
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (var player in playerStats)
                                {
                                    StorePlayerData(
                                        connection,
                                        roundId,
                                        player.Key,
                                        player.Value.Name,
                                        player.Value.meleeDamage,
                                        player.Value.rangedDamage,
                                        player.Value.throwingDamage,
                                        player.Value.DamageDealt,
                                        player.Value.Team,
                                        player.Value.Kicks,
                                        player.Value.Health,
                                        player.Value.mountedMeleeDamage,
                                        player.Value.mountedRangedDamage,
                                        player.Value.mountedThrowingDamage,
                                        player.Value.friendlyMeleeDamage,
                                        player.Value.friendlyRangedDamage,
                                        player.Value.friendlyThrowingDamage,
                                        player.Value.friendlyMountedMeleeDamage,
                                        player.Value.friendlyMountedRangedDamage,
                                        player.Value.friendlyMountedThrowingDamage,
                                        player.Value.Culture,
                                        playerMMR.GetOrAdd(player.Key, DEFAULT_MMR),
                                        player.Value.maceDamage,
                                        player.Value.swordDamage,
                                        player.Value.axeDamage,
                                        player.Value.spearDamage
                                    );
                                }

                                List<KillData> killDataList = StatsManager.GetKillData();
                                foreach (var killData in killDataList)
                                {
                                    StoreKillData(
                                        connection,
                                        roundId,
                                        killData.KillerId,
                                        killData.KilledId,
                                        killData.KillType,
                                        killData.AssistId,
                                        killData.SurvivalTime
                                    );
                                }

                                StoreRoundData(connection);

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                LogError("OnRoundEnd", ex);
                                transaction.Rollback();
                                throw;
                            }
                        }
                    });
                }

                // Reset KillData for the next round
                StatsManager.ClearKillData();

                // Reset player stats at the end of the round (for the next round)
                StatsManager.InitializePlayerStats(players);
            }
            catch (Exception ex)
            {
                LogError("OnRoundEnd", ex);
            }
        }

        private void LogError(string methodName, Exception ex)
        {
            string errorMessage = $"Error in {methodName}: {ex.Message}\nStack Trace: {ex.StackTrace}";
            Helper.PrintWarning(errorMessage);
            // You might also want to log this to a file for later analysis
            // File.AppendAllText("error_log.txt", errorMessage + "\n\n");
        }

        public void StoreKillData(int roundId, string killerId, string killedId, string killType, string assistId, long survivalTime)
        {
            // Add null checks and default values
            roundId = roundId == 0 ? 999 : roundId;
            killerId = killerId ?? "Unknown";
            killedId = killedId ?? "Unknown";
            killType = killType ?? "Unknown";
            assistId = assistId ?? "None";
            survivalTime = survivalTime == 0 ? 999 : survivalTime;

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            const string insertQuery = @"
                INSERT INTO Kills (Round_ID, Killer_ID, Killed_ID, Kill_Type, Assist_ID, Survival_Time)
                VALUES (@roundId, @killerId, @killedId, @killType, @assistId, @survivalTime);";

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@roundId", roundId);
            command.Parameters.AddWithValue("@killerId", killerId);
            command.Parameters.AddWithValue("@killedId", killedId);
            command.Parameters.AddWithValue("@killType", killType);
            command.Parameters.AddWithValue("@assistId", assistId);
            command.Parameters.AddWithValue("@survivalTime", survivalTime);

            command.ExecuteNonQuery();
        }
        public void StoreRoundData()
    {
        MultiplayerRoundController roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();
        
        long roundEndTime = (long)Mission.Current.CurrentTime;
        long roundDuration = roundEndTime - roundStartTime;

        string winnerTeam = roundController?.RoundWinner.ToString() ?? "Draw";

        roundId = roundId == 0 ? 999 : roundId;
        currentMatchId = currentMatchId == 0 ? 999 : currentMatchId;
        roundDuration = roundDuration == 0 ? 999 : roundDuration;

        // Get the current date and time
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        using var connection = new SQLiteConnection(connectionString);
        connection.Open();

        const string insertQuery = @"
            INSERT INTO Rounds (Round_ID, Match_ID, WinnerTeam, Time, Date)
            VALUES (@roundId, @matchId, @winnerTeam, @time, @date);";

        using var command = new SQLiteCommand(insertQuery, connection);
        command.Parameters.AddWithValue("@roundId", roundId);
        command.Parameters.AddWithValue("@matchId", currentMatchId);
        command.Parameters.AddWithValue("@winnerTeam", winnerTeam);
        command.Parameters.AddWithValue("@time", roundDuration);
        command.Parameters.AddWithValue("@date", currentDate); // Add the date parameter
        command.ExecuteNonQuery();
    }
        private void LoadPlayerMMR()
        {
            ExecuteWithRetry(connection =>
            {
                // Create a temporary table with the latest Round_ID for each Player_ID
                string query = @"
                    WITH LatestRounds AS (
                        SELECT Player_ID, MAX(Round_ID) as LastRound
                        FROM Round_Player
                        GROUP BY Player_ID
                    )
                    SELECT rp.Player_ID, rp.MMR
                    FROM Round_Player rp
                    INNER JOIN LatestRounds lr 
                        ON rp.Player_ID = lr.Player_ID 
                        AND rp.Round_ID = lr.LastRound;";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string playerId = reader["Player_ID"].ToString();
                        int mmr = Convert.ToInt32(reader["MMR"]);
                        playerMMR.TryAdd(playerId, mmr);
                    }
                }
            });
        }

        private void UpdateMMR(string winningTeam)
        {
            if (StatsManager == null)
            {
                Helper.PrintWarning("StatsManager is null in UpdateMMR");
                return;
            }

            Dictionary<string, PlayerStats> playerStats = StatsManager.GetPlayerStats();
            if (playerStats == null || playerStats.Count == 0)
            {
                Helper.PrintWarning("No player stats available");
                return;
            }

            var teamInfos = new Dictionary<string, TeamInfo>();

            // First pass: Gather team information based on initial assignments from StatsManager
            foreach (var player in players)
            {
                string playerId = player.VirtualPlayer.Id.ToString();
                
                // Skip if we don't have stats for this player
                if (!playerStats.TryGetValue(playerId, out PlayerStats stats))
                    continue;

                string initialTeam = stats.Team;
                if (!teamInfos.ContainsKey(initialTeam))
                {
                    teamInfos[initialTeam] = new TeamInfo();
                }
                
                int playerMMR = this.playerMMR.GetOrAdd(playerId, DEFAULT_MMR);
                teamInfos[initialTeam].PlayerMMRs.Add(playerMMR);
                teamInfos[initialTeam].Players.Add(player);
            }

            // Rest of the MMR calculation remains the same
            if (teamInfos.Count != 2) return;

            var teams = teamInfos.Keys.ToList();
            TeamInfo team1 = teamInfos[teams[0]];
            TeamInfo team2 = teamInfos[teams[1]];

            // Calculate total MMR for each team
            int team1TotalMMR = team1.PlayerMMRs.Sum();
            int team2TotalMMR = team2.PlayerMMRs.Sum();

            // Calculate adjusted team MMRs
            double team1AdjustedMMR = team1TotalMMR / Math.Min(team1.TeamSize, team2.TeamSize);
            double team2AdjustedMMR = team2TotalMMR / Math.Min(team1.TeamSize, team2.TeamSize);

            // Calculate expected scores
            double team1ExpectedScore = 1 / (1 + Math.Pow(10, (team2AdjustedMMR - team1AdjustedMMR) / 400.0));
            double team2ExpectedScore = 1 - team1ExpectedScore;

            // Calculate per-player MMR change using MMR_CHANGE_BASE
            double actualScore = (winningTeam == teams[0]) ? 1 : 0;
            int baseMMRChange = (int)Math.Round(MMR_CHANGE_BASE * Math.Abs(actualScore - team1ExpectedScore));
            
            // Cap the base MMR change at MMR_CHANGE_BASE
            baseMMRChange = Math.Min(baseMMRChange, MMR_CHANGE_BASE);

            
            if (winningTeam == teams[0])
            {
                // Team 1 won
                int team1MMRChange = baseMMRChange * team1.TeamSize;
                int team2MMRChange = -team1MMRChange;
                
                ApplyMMRChange(team1, team1MMRChange);
                ApplyMMRChange(team2, team2MMRChange);
            }
            else
            {
                // Team 2 won
                int team2MMRChange = baseMMRChange * team2.TeamSize;
                int team1MMRChange = -team2MMRChange;
                
                ApplyMMRChange(team1, team1MMRChange);
                ApplyMMRChange(team2, team2MMRChange);
            }
        }

        private void ApplyMMRChange(TeamInfo team, int totalTeamMMRChange)
        {
            // Calculate per-player MMR change
            int mmrChangePerPlayer = totalTeamMMRChange / team.TeamSize;
            
            // Ensure per-player change doesn't exceed the cap
            if (Math.Abs(mmrChangePerPlayer) > MMR_CHANGE_BASE)
            {
                mmrChangePerPlayer = MMR_CHANGE_BASE * Math.Sign(mmrChangePerPlayer);
            }
            
            // Handle any remainder to ensure total MMR change is exact
            int remainder = totalTeamMMRChange - (mmrChangePerPlayer * team.TeamSize);

            for (int i = 0; i < team.Players.Count; i++)
            {
                var player = team.Players[i];
                string playerId = player.VirtualPlayer.Id.ToString();
                int currentMMR = playerMMR[playerId];

                int playerMMRChange = mmrChangePerPlayer;
                if (i < Math.Abs(remainder))
                {
                    playerMMRChange += Math.Sign(totalTeamMMRChange);
                }

                // Final cap check per player
                if (Math.Abs(playerMMRChange) > MMR_CHANGE_BASE)
                {
                    playerMMRChange = MMR_CHANGE_BASE * Math.Sign(playerMMRChange);
                }

                int newMMR = Math.Max(1, currentMMR + playerMMRChange);
                playerMMR[playerId] = newMMR;

                Helper.SendMessageToPeer(player, $"Old MMR: {currentMMR} | Change: {(playerMMRChange >= 0 ? "+" : "")}{playerMMRChange} | New MMR: {newMMR}");
            }
        }

        // Modify StorePlayerData to accept a SQLiteConnection
        private void StorePlayerData(SQLiteConnection connection, int roundId, string playerId, string name, int meleeDamage, int rangedDamage, int throwingDamage, int totalDamage, string team, int kicks, int HP, int mountedMeleeDamage, int mountedRangedDamage, int mountedThrowingDamage, int friendlyMeleeDamage, int friendlyRangedDamage, int friendlyThrowingDamage, int friendlyMountedMeleeDamage, int friendlyMountedRangedDamage, int friendlyMountedThrowingDamage, string culture, int mmr, int maceDamage, int swordDamage, int axeDamage, int spearDamage)
        {
            // ... (keep existing null checks and default assignments)

            string insertQuery = "INSERT INTO Round_Player (" +
                "Round_ID, " +
                "Player_ID, " +
                "Name, " +
                "Melee_Damage, " +
                "Ranged_Damage, " +
                "Throwing_Damage, " +
                "Total_Damage, " +
                "Team, " +
                "Kicks, " +
                "HP, " +
                "Mounted_Melee_Damage, " +
                "Mounted_Ranged_Damage, " +
                "Mounted_Throwing_Damage, " +
                "Friendly_Melee_Damage, " +
                "Friendly_Ranged_Damage, " +
                "Friendly_Throwing_Damage, " +
                "Friendly_Mounted_Melee_Damage, " +
                "Friendly_Mounted_Ranged_Damage, " +
                "Friendly_Mounted_Throwing_Damage, " +
                "Culture, " +
                "MMR, " +
                "Mace_Damage, " +
                "Sword_Damage, " +
                "Axe_Damage, " +
                "Spear_Damage" +
                ") VALUES (" +
                "@roundId, " +
                "@playerId, " +
                "@name, " +
                "@meleeDamage, " +
                "@rangedDamage, " +
                "@throwingDamage, " +
                "@totalDamage, " +
                "@team, " +
                "@kicks, " +
                "@HP, " +
                "@mountedMeleeDamage, " +
                "@mountedRangedDamage, " +
                "@mountedThrowingDamage, " +
                "@friendlyMeleeDamage, " +
                "@friendlyRangedDamage, " +
                "@friendlyThrowingDamage, " +
                "@friendlyMountedMeleeDamage, " +
                "@friendlyMountedRangedDamage, " +
                "@friendlyMountedThrowingDamage, " +
                "@culture, " +
                "@mmr, " +
                "@maceDamage, " +
                "@swordDamage, " +
                "@axeDamage, " +
                "@spearDamage" +
                ");";

            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@roundId", roundId);
                command.Parameters.AddWithValue("@playerId", playerId);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@meleeDamage", meleeDamage);
                command.Parameters.AddWithValue("@rangedDamage", rangedDamage);
                command.Parameters.AddWithValue("@throwingDamage", throwingDamage);
                command.Parameters.AddWithValue("@totalDamage", totalDamage);
                command.Parameters.AddWithValue("@team", team);
                command.Parameters.AddWithValue("@kicks", kicks);
                command.Parameters.AddWithValue("@HP", HP);
                command.Parameters.AddWithValue("@mountedMeleeDamage", mountedMeleeDamage);
                command.Parameters.AddWithValue("@mountedRangedDamage", mountedRangedDamage);
                command.Parameters.AddWithValue("@mountedThrowingDamage", mountedThrowingDamage);
                command.Parameters.AddWithValue("@friendlyMeleeDamage", friendlyMeleeDamage);
                command.Parameters.AddWithValue("@friendlyRangedDamage", friendlyRangedDamage);
                command.Parameters.AddWithValue("@friendlyThrowingDamage", friendlyThrowingDamage);
                command.Parameters.AddWithValue("@friendlyMountedMeleeDamage", friendlyMountedMeleeDamage);
                command.Parameters.AddWithValue("@friendlyMountedRangedDamage", friendlyMountedRangedDamage);
                command.Parameters.AddWithValue("@friendlyMountedThrowingDamage", friendlyMountedThrowingDamage);
                command.Parameters.AddWithValue("@culture", culture);
                command.Parameters.AddWithValue("@mmr", mmr);
                command.Parameters.AddWithValue("@maceDamage", maceDamage);
                command.Parameters.AddWithValue("@swordDamage", swordDamage);
                command.Parameters.AddWithValue("@axeDamage", axeDamage);
                command.Parameters.AddWithValue("@spearDamage", spearDamage);

                command.ExecuteNonQuery();
            }
        }

        // Modify StoreKillData to accept a SQLiteConnection
        private void StoreKillData(SQLiteConnection connection, int roundId, string killerId, string killedId, string killType, string assistId, long survivalTime)
        {
            // ... (keep existing null checks and default assignments)

            const string insertQuery = @"
                INSERT INTO Kills (Round_ID, Killer_ID, Killed_ID, Kill_Type, Assist_ID, Survival_Time)
                VALUES (@roundId, @killerId, @killedId, @killType, @assistId, @survivalTime);";

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@roundId", roundId);
            command.Parameters.AddWithValue("@killerId", killerId);
            command.Parameters.AddWithValue("@killedId", killedId);
            command.Parameters.AddWithValue("@killType", killType);
            command.Parameters.AddWithValue("@assistId", assistId);
            command.Parameters.AddWithValue("@survivalTime", survivalTime);
            command.ExecuteNonQuery();
        }

        // Modify StoreRoundData to accept a SQLiteConnection
        private void StoreRoundData(SQLiteConnection connection)
        {
            MultiplayerRoundController roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();
            
            long roundEndTime = (long)Mission.Current.CurrentTime;
            long roundDuration = roundEndTime - roundStartTime;

            string winnerTeam = roundController?.RoundWinner.ToString() ?? "Draw";

            roundId = roundId == 0 ? 999 : roundId;
            currentMatchId = currentMatchId == 0 ? 999 : currentMatchId;
            roundDuration = roundDuration == 0 ? 999 : roundDuration;

            // Get the current date and time
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            const string insertQuery = @"
                INSERT INTO Rounds (Round_ID, Match_ID, WinnerTeam, Time, Date)
                VALUES (@roundId, @matchId, @winnerTeam, @time, @date);";

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@roundId", roundId);
            command.Parameters.AddWithValue("@matchId", currentMatchId);
            command.Parameters.AddWithValue("@winnerTeam", winnerTeam);
            command.Parameters.AddWithValue("@time", roundDuration);
            command.Parameters.AddWithValue("@date", currentDate); // Add the date parameter
            command.ExecuteNonQuery();
        }

        private void ExecuteWithRetry(Action<SQLiteConnection> databaseOperation)
        {
            const int maxRetries = 5;
            const int retryDelayMs = 1000;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    lock (dbLock)
                    {
                        using (var connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();
                            connection.DefaultTimeout = 30;
                            databaseOperation(connection);
                            return;
                        }
                    }
                }
                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Locked || ex.ResultCode == SQLiteErrorCode.Busy)
                {
                    if (attempt == maxRetries - 1)
                    {
                        LogError($"Database operation failed after {maxRetries} attempts", ex);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(retryDelayMs);
                    }
                }
                catch (Exception ex)
                {
                    LogError("Database operation", ex);
                    break;
                }
            }
        }

        private string DetermineTeamIdentifier(TeamInfo team, TeamInfo otherTeam)
        {   
            if (team.CultureName != otherTeam.CultureName)
            {
                return team.CultureName;
            }
            else
            {
                if (team.SideName == "Attacker"){
                    string colorName = GetColorName(team.PrimaryColor);
                    return colorName;
                }
                else
                {
                    string colorName = GetColorName(otherTeam.AltPrimaryColor);
                    return colorName;
                }
            }
        }

        private string GetColorName(uint color)
        {
            // Extract RGB values (ignore alpha)
            uint rgb = color & 0x00FFFFFF;

            // Empire colors (5A1D5E, FFD700)
            if (rgb == 0x5A1D5E) return "Purple";  // Empire primary
            if (rgb == 0xFFD700) return "Gold";    // Empire secondary
            if (rgb == 0xFF4500) return "Orange";  // Empire alt1
            if (rgb == 0x808080) return "Gray";    // Empire alt2
            if (rgb == 0x793191) return "Purple";  // Empire banner bg1
            if (rgb == 0xf7bf46) return "Gold";    // Empire banner fg1
            if (rgb == 0xe44434) return "Red";     // Empire banner bg2
            if (rgb == 0xc3c3c3) return "Silver";  // Empire banner fg2

            // Aserai colors (CC8324, 4E1A13)
            if (rgb == 0xCC8324) return "Bronze";  // Aserai primary
            if (rgb == 0x4E1A13) return "Brown";   // Aserai secondary
            if (rgb == 0x4F2212) return "Brown";   // Aserai alt1
            if (rgb == 0x965228) return "Brown";   // Aserai alt2
            if (rgb == 0xA97435) return "Bronze";  // Aserai banner bg1
            if (rgb == 0x41281B) return "Brown";   // Aserai banner fg1/bg2
            
            // Sturgia colors (4682B4, FFFFFF)
            if (rgb == 0x4682B4) return "Blue";    // Sturgia primary
            if (rgb == 0xFFFFFF) return "White";   // Sturgia secondary
            if (rgb == 0x228B22) return "Green";   // Sturgia alt1
            if (rgb == 0x804000) return "Brown";   // Sturgia alt2
            if (rgb == 0x224277) return "Blue";    // Sturgia banner bg1
            if (rgb == 0x284e19) return "Green";   // Sturgia banner bg2
            if (rgb == 0x7f6b60) return "Brown";   // Sturgia banner fg2

            // Vlandia colors (FF0000, FFD700)
            if (rgb == 0xFF0000) return "Red";     // Vlandia primary
            // FFD700 (Gold) already covered in Empire
            if (rgb == 0x00008B) return "Blue";    // Vlandia alt1
            if (rgb == 0xD3D3D3) return "Silver";  // Vlandia alt2
            if (rgb == 0x830808) return "Red";     // Vlandia banner bg1
            if (rgb == 0xf4ca14) return "Gold";    // Vlandia banner fg1
            if (rgb == 0x2c4d86) return "Blue";    // Vlandia banner bg2
            if (rgb == 0xd9d9d9) return "Silver";  // Vlandia banner fg2

            // Battania colors (006400, DAA520)
            if (rgb == 0x006400) return "Green";   // Battania primary
            if (rgb == 0xDAA520) return "Gold";    // Battania secondary
            if (rgb == 0x8A2BE2) return "Purple";  // Battania alt1
            if (rgb == 0xCD853F) return "Bronze";  // Battania alt2
            if (rgb == 0x34671e) return "Green";   // Battania banner bg1
            if (rgb == 0xb57a1e) return "Gold";    // Battania banner fg1
            if (rgb == 0x7739a7) return "Purple";  // Battania banner bg2
            if (rgb == 0x975b43) return "Brown";   // Battania banner fg2

            // Khuzait colors (418174, FFE9D4)
            if (rgb == 0x418174) return "Teal";    // Khuzait primary
            if (rgb == 0xFFE9D4) return "Beige";   // Khuzait secondary
            if (rgb == 0xCCBB89) return "Tan";     // Khuzait alt1
            if (rgb == 0x58888B) return "Teal";    // Khuzait alt2
            if (rgb == 0x5AA4AD) return "Teal";    // Khuzait banner bg1/fg2
            // FFE9D4 (Beige) already covered above

            // Keep existing generic colors
            if (rgb == 0x335F22) return "Green";
            if (rgb == 0xF3F3F3) return "White";
            if (rgb == 0x8D291A) return "Red";
            if (rgb == 0x0b0c11) return "Black";
            if (rgb == 0xc5057c) return "Pink";

            // If no match is found, return the RGB part of the color code as a string
            return $"#{rgb:X6}";
        }

        public int GetPlayerMMR(string playerId)
        {
            return playerMMR.GetOrAdd(playerId, DEFAULT_MMR);
        }
    }

    public class TeamInfo
    {
        public List<int> PlayerMMRs { get; set; } = new List<int>();
        public string CultureName { get; set; }
        public uint PrimaryColor { get; set; }
        public uint SecondaryColor { get; set; }
        public uint AltPrimaryColor { get; set; }
        public uint AltSecondaryColor { get; set; }
        public List<NetworkCommunicator> Players { get; set; } = new List<NetworkCommunicator>();
        public int TeamSize => PlayerMMRs.Count;
        public string SideName { get; set; }
        public double AverageMMR => PlayerMMRs.Count > 0 ? PlayerMMRs.Average() : 0;
    }
}
