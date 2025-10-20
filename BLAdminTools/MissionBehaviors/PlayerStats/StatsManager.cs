using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;
using DoFAdminTools.Helpers;

namespace DoFAdminTools.Database
{
    public class StatsManager
    {
        private Dictionary<string, PlayerStats> playerStats = new Dictionary<string, PlayerStats>();
        private List<KillData> killDataList = new List<KillData>();
        private long roundStartTime;

        public void InitializePlayerStats(List<NetworkCommunicator> players)
        {
            playerStats.Clear();
            try 
            { 
                foreach (var player in players)
                {
                    var missionPeer = player.GetComponent<MissionPeer>();
                    
                    playerStats[player.VirtualPlayer.Id.ToString()] = new PlayerStats 
                    { 
                        Name = missionPeer.Name, 
                        Health = 100,
                        DamageDealt = 0,
                        Kicks = 0,
                        Team = missionPeer.Team.Side.ToString(),
                        Culture = missionPeer.Culture.Name.ToString(),
                        meleeDamage = 0,
                        rangedDamage = 0,
                        throwingDamage = 0,
                        mountedMeleeDamage = 0,
                        mountedRangedDamage = 0,
                        mountedThrowingDamage = 0,
                        friendlyMeleeDamage = 0,
                        friendlyRangedDamage = 0,
                        friendlyThrowingDamage = 0,
                        friendlyMountedMeleeDamage = 0,
                        friendlyMountedRangedDamage = 0,
                        friendlyMountedThrowingDamage = 0,
                        shots = 0,
                        hitShots = 0,
                        shieldHits = 0,
                        headShots = 0,
                        horseHits = 0,
                        maceDamage = 0,
                        swordDamage = 0,
                        axeDamage = 0,
                        spearDamage = 0,
                        DamageReceived = new Dictionary<string, int>()
                    };
                }
            } 
            catch (Exception ex) 
            {
                Helper.SendMessageToAllPeers("Error initializing player stats: " + ex.Message);
            }
        }

        public void UpdatePlayerStats(Agent affectorAgent, Agent affectedAgent, Blow blow)
        {
            string affectorId = affectorAgent.MissionPeer.GetNetworkPeer().VirtualPlayer.Id.ToString();
            string affectedId = affectedAgent.MissionPeer.GetNetworkPeer().VirtualPlayer.Id.ToString();
            
            int actualDamageDealt = Math.Min(blow.InflictedDamage, playerStats[affectedId].Health);
            
            if (!playerStats[affectedId].DamageReceived.ContainsKey(affectorId))
            {
                playerStats[affectedId].DamageReceived[affectorId] = 0;
            }
            playerStats[affectedId].DamageReceived[affectorId] += actualDamageDealt;

            if (blow.InflictedDamage >= playerStats[affectedId].Health)
            {
                HandleKill(affectorAgent, affectedAgent, blow);
            }

            playerStats[affectedId].Health = Math.Max(0, playerStats[affectedId].Health - actualDamageDealt);
            
            if (affectorAgent.Team.Side == affectedAgent.Team.Side)
            {
                playerStats[affectorId].DamageDealt -= actualDamageDealt;
            }
            else
            {
                playerStats[affectorId].DamageDealt += actualDamageDealt;
            }
            
            if (blow.AttackType.ToString() == "Kick")
            {
                playerStats[affectorId].Kicks++;
            }

            bool isFriendly = affectorAgent.Team.Side == affectedAgent.Team.Side;
            bool isMounted = affectorAgent.HasMount;
            string weaponClass = blow.WeaponRecord.WeaponClass.ToString();

            UpdateDamageStats(affectorId, actualDamageDealt, isFriendly, isMounted, weaponClass);
        }

        private void UpdateDamageStats(string affectorId, int damage, bool isFriendly, bool isMounted, string weaponClass)
        {
            var stats = playerStats[affectorId];

            string damageType = GetDamageType(weaponClass);
            string weaponType = GetWeaponType(weaponClass);
            string mountedPrefix = isMounted ? "Mounted" : "";
            string friendlyPrefix = isFriendly ? "Friendly" : "";

            string propertyName = $"{friendlyPrefix}{mountedPrefix}{damageType}Damage";
            propertyName = char.ToLower(propertyName[0]) + propertyName.Substring(1);

            var property = typeof(PlayerStats).GetProperty(propertyName);
            if (property != null)
            {
                int currentValue = (int)property.GetValue(stats);
                property.SetValue(stats, currentValue + damage);
            }
            else
            {
                Helper.PrintError($"Property not found: {propertyName}");
            }

            // Update specific weapon damage
            if (!string.IsNullOrEmpty(weaponType))
            {
                string weaponPropertyName = $"{weaponType}Damage";
                weaponPropertyName = char.ToLower(weaponPropertyName[0]) + weaponPropertyName.Substring(1);
                var weaponProperty = typeof(PlayerStats).GetProperty(weaponPropertyName);
                if (weaponProperty != null)
                {
                    int currentWeaponValue = (int)weaponProperty.GetValue(stats);
                    weaponProperty.SetValue(stats, currentWeaponValue + damage);
                }
                else
                {
                    Helper.PrintError($"Weapon property not found: {weaponPropertyName}");
                }
            }
        }

        private string GetDamageType(string weaponClass)
        {
            switch (weaponClass)
            {
                case "Arrow":
                case "Bolt":
                    return "Ranged";
                case "Javelin":
                case "ThrowingAxe":
                    return "Throwing";
                default:
                    return "Melee";
            }
        }

        private string GetWeaponType(string weaponClass)
        {
            switch (weaponClass)
            {
                case "OneHandedMace":
                case "TwoHandedMace":
                    return "Mace";
                case "OneHandedSword":
                case "TwoHandedSword":
                    return "Sword";
                case "OneHandedAxe":
                case "TwoHandedAxe":
                    return "Axe";
                case "OneHandedPolearm":
                case "TwoHandedPolearm":
                    return "Spear";
                default:
                    return "";
            }
        }

        public void SetRoundStartTime(long startTime)
        {
            roundStartTime = startTime;
        }

        private void HandleKill(Agent killerAgent, Agent killedAgent, Blow blow)
        {
            string killerId = killerAgent.MissionPeer.GetNetworkPeer().VirtualPlayer.Id.ToString();
            string killedId = killedAgent.MissionPeer.GetNetworkPeer().VirtualPlayer.Id.ToString();

            string killType = GetKillType(blow);

            string assistId = playerStats[killedId].DamageReceived
                .Where(kvp => kvp.Key != killerId && kvp.Value >= 50)
                .Select(kvp => kvp.Key)
                .FirstOrDefault() ?? "Solokill";

            long currentTime = (long)Mission.Current.CurrentTime;
            long survivalTime = currentTime - roundStartTime;

            KillData killData = new KillData
            {
                KillerId = killerId,
                KilledId = killedId,
                KillType = killType,
                AssistId = assistId,
                SurvivalTime = survivalTime
            };
            
            killDataList.Add(killData);
        }

        private string GetKillType(Blow blow)
        {
            string weaponClass = blow.WeaponRecord.WeaponClass.ToString();

            if (weaponClass == "Arrow" || weaponClass == "Bolt")
            {
                return "ranged";
            }
            else if (weaponClass == "Javelin" || weaponClass == "ThrowingAxe")
            {
                return "throwing";
            }
            else
            {
                return "melee";
            }
        }

        public Dictionary<string, PlayerStats> GetPlayerStats()
        {
            return playerStats;
        }

        public List<KillData> GetKillData()
        {
            return killDataList;
        }

        public void ClearKillData()
        {
            killDataList.Clear();
        }
    }

    public class KillData
    {
        public string KillerId { get; set; }
        public string KilledId { get; set; }
        public string KillType { get; set; }
        public string AssistId { get; set; }
        public long SurvivalTime { get; set; }
    }

    public class PlayerStats
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int DamageDealt { get; set; }
    public int Kicks { get; set; }
    public string Culture { get; set; }
    public string Team { get; set; }
    public string Class { get; set; }
    public int meleeDamage { get; set; }
    public int rangedDamage { get; set; }
    public int throwingDamage { get; set; }
    public int mountedMeleeDamage { get; set; }
    public int mountedRangedDamage { get; set; }
    public int mountedThrowingDamage { get; set; }
    public int friendlyMeleeDamage { get; set; }
    public int friendlyRangedDamage { get; set; }
    public int friendlyThrowingDamage { get; set; }
    public int friendlyMountedMeleeDamage { get; set; }
    public int friendlyMountedRangedDamage { get; set; }
    public int friendlyMountedThrowingDamage { get; set; }
    public int shots { get; set; }
    public int hitShots { get; set; }
    public int shieldHits { get; set; }
    public int headShots { get; set; }
    public int horseHits { get; set; }
    public int maceDamage  { get; set; }
    public int swordDamage { get; set; }
    public int axeDamage { get; set; }
    public int spearDamage { get; set; }
    // New property to track damage received from other players
    public Dictionary<string, int> DamageReceived { get; set; } = new Dictionary<string, int>();
}
}
