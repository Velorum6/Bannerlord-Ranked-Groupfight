using System;
using System.Linq;
using DoFAdminTools.Helpers;
using DoFAdminTools.MissionBehaviors;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools.ChatCommands.PublicCommands;

public class PartyCommand : ChatCommand
{
    public override string CommandText => "party";
    public override string Description => "Party management command. Use !party help for more information";
    public override bool CanExecute(NetworkCommunicator executor) => true;

    public override bool Execute(NetworkCommunicator executor, string args)
    {
        string[] arguments = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (arguments.Length == 0 || arguments[0].ToLower() == "help")
        {
            ShowHelp(executor);
            return true;
        }

        string playerId = executor.VirtualPlayer.Id.ToString();
        string subCommand = arguments[0].ToLower();

        return subCommand switch
        {
            "create" when arguments.Length >= 2 => CreateParty(executor, arguments[1]),
            "join" when arguments.Length >= 2 => JoinParty(executor, arguments[1]),
            "leave" => LeaveParty(executor),
            "list" => ListParties(executor),
            _ => ShowInvalidCommand(executor)
        };
    }

    private void ShowHelp(NetworkCommunicator executor)
    {
        Helper.SendMessageToPeer(executor, "Party Commands:");
        Helper.SendMessageToPeer(executor, "!party create <name> - Create a new party");
        Helper.SendMessageToPeer(executor, "!party join <name> - Join an existing party");
        Helper.SendMessageToPeer(executor, "!party leave - Leave your current party");
        Helper.SendMessageToPeer(executor, "!party list - List all active parties");
    }

    private bool ShowInvalidCommand(NetworkCommunicator executor)
    {
        Helper.SendMessageToPeer(executor, "Invalid party command. Use !party help for available commands.");
        return false;
    }

    private bool CreateParty(NetworkCommunicator executor, string partyName)
    {
        string playerId = executor.VirtualPlayer.Id.ToString();
        if (PartyManager.Instance.CreateParty(playerId, partyName))
        {
            Helper.SendMessageToPeer(executor, $"You created party: {partyName}");
            return true;
        }
        Helper.SendMessageToPeer(executor, "Could not create party. You might already be in a party or the name is taken.");
        return false;
    }

    private bool JoinParty(NetworkCommunicator executor, string partyName)
    {
        string playerId = executor.VirtualPlayer.Id.ToString();
        if (PartyManager.Instance.JoinParty(playerId, partyName))
        {
            foreach (var peer in PartyManager.Instance.GetPartyMemberPeers(partyName))
            {
                Helper.SendMessageToPeer(peer, $"{executor.UserName} joined the party.");
            }
            return true;
        }
        Helper.SendMessageToPeer(executor, "Could not join party. Party might be full or doesn't exist.");
        return false;
    }

    private bool LeaveParty(NetworkCommunicator executor)
    {
        string playerId = executor.VirtualPlayer.Id.ToString();
        var party = PartyManager.Instance.GetPlayerParty(playerId);
        if (party == null)
        {
            Helper.SendMessageToPeer(executor, "You are not in a party.");
            return false;
        }

        string partyName = party.PartyName;
        bool isLeader = party.IsLeader(playerId);
        var partyMembers = PartyManager.Instance.GetPartyMemberPeers(partyName).ToList();
        
        if (PartyManager.Instance.LeaveParty(playerId))
        {
            if (isLeader)
            {
                foreach (var peer in partyMembers.Where(p => p != executor))
                {
                    Helper.SendMessageToPeer(peer, $"Party {partyName} has been disbanded as the leader left.");
                }
                Helper.SendMessageToPeer(executor, $"You disbanded party {partyName}.");
            }
            else
            {
                foreach (var peer in partyMembers.Where(p => p != executor))
                {
                    Helper.SendMessageToPeer(peer, $"{executor.UserName} left the party.");
                }
                Helper.SendMessageToPeer(executor, "You left the party.");
            }
            return true;
        }
        return false;
    }

    private bool ListParties(NetworkCommunicator executor)
    {
        if (PartyManager.Instance._parties.Count == 0)
        {
            Helper.SendMessageToPeer(executor, "There are no active parties at the moment.");
            return true;
        }

        Helper.SendMessageToPeer(executor, "Active Parties:");
        int partyNumber = 1;
        foreach (var party in PartyManager.Instance._parties.Values)
        {
            var memberNames = party.PartyMembers.Select(playerId => GetUserNameFromId(playerId)).ToList();
            string members = string.Join(", ", memberNames);
            string leaderName = GetUserNameFromId(party.LeaderId);
            Helper.SendMessageToPeer(executor, $"Party {partyNumber} (Leader: {leaderName}): {members}");
            partyNumber++;
        }
        return true;
    }

    private string GetUserNameFromId(string playerId)
    {
        var player = GameNetwork.NetworkPeers.FirstOrDefault(peer => peer.VirtualPlayer.Id.ToString() == playerId);
        return player?.UserName ?? "Unknown";
    }
} 