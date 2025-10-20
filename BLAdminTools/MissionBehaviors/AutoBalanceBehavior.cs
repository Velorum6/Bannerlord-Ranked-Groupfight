using DoFAdminTools.Helpers;
using TaleWorlds.MountAndBlade;
using System.Collections.Generic;
using System.Linq;
using System;
using TaleWorlds.Core;

namespace DoFAdminTools.MissionBehaviors
{
    public class AutoBalanceBehavior : MissionBehavior
    {
        private MultiplayerRoundController roundController;
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void AfterStart()
        {
            roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();
            if (roundController != null)
            {
                roundController.OnRoundEnding += OnRoundEnding;
            }
        }

        private void OnRoundEnding()
        {
                BalanceTeams();
        }

        private void BalanceTeams()
        {
            if (!GameNetwork.IsServer)
            {
                Helper.SendMessageToAllPeers("BalanceTeams: Not a server, exiting.");
                return;
            }

            var allPlayers = GetAllActivePlayers();
            if (allPlayers.Count < 2)
            {
                Helper.SendMessageToAllPeers("BalanceTeams: Not enough players to balance.");
                return;
            }
            // Group players by party, treating non-party players as individual groups
            var playerGroups = allPlayers.GroupBy(p => PartyManager.Instance.GetPlayerParty(p.VirtualPlayer.Id.ToString())?.PartyName ?? p.VirtualPlayer.Id.ToString())
                                         .ToList();

            // Sort groups by total MMR in descending order
            var sortedGroups = playerGroups.OrderByDescending(g => g.Sum(player => GetPlayerMMR(player))).ToList();

            // Calculate total MMR and target MMR for each team
            int totalMMR = sortedGroups.Sum(g => g.Sum(player => GetPlayerMMR(player)));
            int targetMMR = totalMMR / 2;

            var bestTeam1 = new List<NetworkCommunicator>();
            var bestTeam2 = new List<NetworkCommunicator>();
            double bestImbalance = double.MaxValue;

            // Use a dynamic programming approach to find the best distribution
            FindBestDistribution(sortedGroups, 0, new List<NetworkCommunicator>(), new List<NetworkCommunicator>(), ref bestTeam1, ref bestTeam2, ref bestImbalance, targetMMR);

            // Apply the best team configuration found
            foreach (var player in bestTeam1)
            {
                ChangePlayerTeam(player, Mission.Current.AttackerTeam);
                Helper.Print($"Player {player.UserName} moved to AttackerTeam.");
            }
            foreach (var player in bestTeam2)
            {
                ChangePlayerTeam(player, Mission.Current.DefenderTeam);
                Helper.Print($"Player {player.UserName} moved to DefenderTeam.");
            }
        }

        private void FindBestDistribution(List<IGrouping<string, NetworkCommunicator>> groups, int index, List<NetworkCommunicator> currentTeam1, List<NetworkCommunicator> currentTeam2, ref List<NetworkCommunicator> bestTeam1, ref List<NetworkCommunicator> bestTeam2, ref double bestImbalance, int targetMMR)
        {
            if (index == groups.Count)
            {
                double currentImbalance = CalculateImbalance(currentTeam1, currentTeam2);

                // Check if the current imbalance is within the acceptable range
                if (currentImbalance < bestImbalance)
                {
                    bestImbalance = currentImbalance;
                    bestTeam1 = new List<NetworkCommunicator>(currentTeam1);
                    bestTeam2 = new List<NetworkCommunicator>(currentTeam2);
                }

                // Stop if the imbalance is within a threshold that corresponds to win chances of 45%-55%
                if (currentImbalance <= 35) // Adjust this threshold as needed
                {
                    bestTeam1 = new List<NetworkCommunicator>(currentTeam1);
                    bestTeam2 = new List<NetworkCommunicator>(currentTeam2);
                    return;
                }

                return;
            }

            var group = groups[index].ToList();
            currentTeam1.AddRange(group);
            FindBestDistribution(groups, index + 1, currentTeam1, currentTeam2, ref bestTeam1, ref bestTeam2, ref bestImbalance, targetMMR);
            currentTeam1.RemoveRange(currentTeam1.Count - group.Count, group.Count);

            currentTeam2.AddRange(group);
            FindBestDistribution(groups, index + 1, currentTeam1, currentTeam2, ref bestTeam1, ref bestTeam2, ref bestImbalance, targetMMR);
            currentTeam2.RemoveRange(currentTeam2.Count - group.Count, group.Count);
        }

        private double CalculateImbalance(List<NetworkCommunicator> team1, List<NetworkCommunicator> team2)
        {
            int team1MMR = team1.Sum(p => GetPlayerMMR(p));
            int team2MMR = team2.Sum(p => GetPlayerMMR(p));

            // Calculate adjusted MMRs
            int minTeamSize = Math.Min(team1.Count, team2.Count);
            double team1AdjustedMMR = team1MMR / (double)minTeamSize;
            double team2AdjustedMMR = team2MMR / (double)minTeamSize;

            // Calculate imbalance based on adjusted MMRs
            return Math.Abs(team1AdjustedMMR - team2AdjustedMMR);
        }

        private List<NetworkCommunicator> GetAllActivePlayers()
        {
            return GameNetwork.NetworkPeers
                .Where(peer => peer.IsSynchronized &&
                       peer.GetComponent<MissionPeer>()?.Team != null &&
                       peer.GetComponent<MissionPeer>()?.Team != Mission.Current.SpectatorTeam)
                .ToList();
        }

        private int GetPlayerMMR(NetworkCommunicator player)
        {
            const int DEFAULT_MMR = 1000;
            var hitCounter = Mission.Current.GetMissionBehavior<HitCounter>();
            return hitCounter?.GetPlayerMMR(player.VirtualPlayer.Id.ToString()) ?? DEFAULT_MMR;
        }

        private void ChangePlayerTeam(NetworkCommunicator player, Team newTeam)
        {
            var missionPeer = player.GetComponent<MissionPeer>();
            if (missionPeer != null && missionPeer.Team != newTeam)
            {
                missionPeer.Team = newTeam;
            }
        }

        public override void OnRemoveBehavior()
        {
            if (roundController != null)
            {
                roundController.OnRoundEnding -= OnRoundEnding;
            }
            base.OnRemoveBehavior();
        }
    }
} 