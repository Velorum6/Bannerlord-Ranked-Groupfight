using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoFAdminTools.Helpers;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools.MissionBehaviors
{
    public class Multikill : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public bool Enabled { get; set; }

        private Dictionary<string, int> killCounts = new Dictionary<string, int>();

        // MultiplayerRoundController reference
        private MultiplayerRoundController _roundController;

        public Multikill() {
            Enabled = true;
        }

        public override void AfterStart()
        {
            base.AfterStart();

            // Fetch the round controller when the mission starts
            _roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();

            // Subscribe to the necessary events
            if (_roundController != null)
            {
                _roundController.OnRoundStarted += ResetAllKillCounts;
            }

            // Subscribe to player killed event
            MissionPeer.OnPlayerKilled += OnPlayerKilledHandler;

        }

        public override void OnRemoveBehavior()
        {

            MultiplayerRoundController roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();
            
            // Unsubscribe from events when this behavior is removed (end of mission)
            if (roundController != null)
            {
                roundController.OnRoundStarted -= ResetAllKillCounts;
            }

            MissionPeer.OnPlayerKilled -= OnPlayerKilledHandler;

            base.OnRemoveBehavior();
        }

        // Event handler for OnPlayerKilled
        private void OnPlayerKilledHandler(MissionPeer killerPeer, MissionPeer killedPeer)
        {

            string killedPlayerName = killedPeer?.Name ?? "Unknown";
            string killerPlayerName = killerPeer?.Name ?? "Unknown";

            if (killerPeer.Team != killedPeer.Team)
            {
                // Handle kill count and check for multi-kills
                HandleKill(killerPlayerName);
            }

            // Reset the killed player's kill count
            ResetKillCount(killedPlayerName);
        }

        // Method to handle the kill count and detect multi-kills
        private void HandleKill(string killerPlayerName)
        {
            // Increment kill count for the killer
            if (killCounts.ContainsKey(killerPlayerName))
            {
                killCounts[killerPlayerName]++;
            }
            else
            {
                // Initialize count if for some reason it wasn't present
                killCounts[killerPlayerName] = 1;
            }

            // Announce multi-kills based on the kill count
            AnnounceMultiKill(killerPlayerName, killCounts[killerPlayerName]);
        }

        // Method to reset the kill count for a player
        private void ResetKillCount(string playerName)
        {
            if (killCounts.ContainsKey(playerName))
            {
                killCounts[playerName] = 0; // Reset kill count on death
            }
        }
        // Method to reset all the kill counts
        private void ResetAllKillCounts()
        {
            killCounts.Clear();
        }
        // Method to announce multi-kills
        private void AnnounceMultiKill(string playerName, int killCount)
        {
            string multiKillMessage = string.Empty;

            switch (killCount)
            {
                case 1:
                    return;
                case 2:
                    multiKillMessage = $"{playerName} has achieved a Double Kill!";
                    break;
                case 3:
                    multiKillMessage = $"{playerName} has achieved a Triple Kill!";
                    break;
                case 4:
                    multiKillMessage = $"{playerName} has made a QUADRAKILL!";
                    break;
                case 5:
                    multiKillMessage = $"{playerName} DID A PENTAKILL!";
                    break;
                default:
                    // You can add more streak messages for higher kill counts if desired
                    multiKillMessage = $"{playerName} is dominating with {killCount} kills!";
                    break;
            }

            // Send multi-kill notification
            if (!string.IsNullOrEmpty(multiKillMessage))
            {
                Helper.SendMessageToAllPeers(multiKillMessage);
            }
        }

    }
}
