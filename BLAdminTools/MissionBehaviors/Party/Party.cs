using System.Collections.Generic;

namespace DoFAdminTools.MissionBehaviors;

public class Party
{
    public string LeaderId { get; private set; }
    public string PartyName { get; private set; }
    private HashSet<string> Members { get; set; }
    public IReadOnlyCollection<string> PartyMembers => Members;
    public const int MAX_PARTY_SIZE = 4;

    public Party(string leaderId, string partyName)
    {
        LeaderId = leaderId;
        PartyName = partyName;
        Members = new HashSet<string> { leaderId };
    }

    public bool AddMember(string playerId)
    {
        if (Members.Count >= MAX_PARTY_SIZE)
            return false;
            
        return Members.Add(playerId);
    }

    public bool RemoveMember(string playerId)
    {
        if (playerId == LeaderId)
            return false;
            
        return Members.Remove(playerId);
    }

    public bool IsLeader(string playerId) => LeaderId == playerId;
    public bool IsMember(string playerId) => Members.Contains(playerId);
} 