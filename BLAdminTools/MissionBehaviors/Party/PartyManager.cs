using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools.MissionBehaviors;

public class PartyManager
{
    private static PartyManager _instance;
    public static PartyManager Instance => _instance ??= new PartyManager();

    public Dictionary<string, Party> _parties = new Dictionary<string, Party>();
    public Dictionary<string, string> _playerPartyMap = new Dictionary<string, string>();

    public Party GetPlayerParty(string playerId)
    {
        return _playerPartyMap.TryGetValue(playerId, out string partyName) ? _parties[partyName] : null;
    }

    public bool CreateParty(string leaderId, string partyName)
    {
        if (_parties.ContainsKey(partyName) || _playerPartyMap.ContainsKey(leaderId))
            return false;

        var party = new Party(leaderId, partyName);
        _parties[partyName] = party;
        _playerPartyMap[leaderId] = partyName;
        return true;
    }

    public bool JoinParty(string playerId, string partyName)
    {
        if (!_parties.TryGetValue(partyName, out Party party) || _playerPartyMap.ContainsKey(playerId))
            return false;

        if (party.AddMember(playerId))
        {
            _playerPartyMap[playerId] = partyName;
            return true;
        }
        return false;
    }

    public bool LeaveParty(string playerId)
    {
        if (!_playerPartyMap.TryGetValue(playerId, out string partyName))
            return false;

        var party = _parties[partyName];
        if (party.IsLeader(playerId))
        {
            // Disband party if leader leaves
            foreach (var member in party.PartyMembers)
            {
                _playerPartyMap.Remove(member);
            }
            _parties.Remove(partyName);
        }
        else
        {
            party.RemoveMember(playerId);
            _playerPartyMap.Remove(playerId);
        }
        return true;
    }

    public IEnumerable<Party> GetAllParties() => _parties.Values;

    public IEnumerable<NetworkCommunicator> GetPartyMemberPeers(string partyName)
    {
        if (!_parties.TryGetValue(partyName, out Party party))
            return Enumerable.Empty<NetworkCommunicator>();

        return party.PartyMembers
            .Select(playerId => GameNetwork.NetworkPeers
                .FirstOrDefault(peer => peer.VirtualPlayer.Id.ToString() == playerId))
            .Where(peer => peer != null);
    }
} 