using DoFAdminTools.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.MultiplayerOptions;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace DoFAdminTools.ChatCommands.AdminCommands;
public class SetMapCommand : AdminChatCommand
{
    public override string CommandText => "setmap";

    public override string Description => "Changes the map and the team factions. !setmap <map name> <team1 faction> <team2 faction>";

    public override bool CanExecute(NetworkCommunicator executor) => true;

    bool ArgValid(Tuple<bool, string> args, NetworkCommunicator executor, string messagePrefix = "")
    {

        if (!args.Item1)
        {
            Helper.SendMessageToPeer(executor, messagePrefix + args.Item2);
            // GameNetwork.BeginModuleEventAsServer(networkPeer);
            // GameNetwork.WriteMessage(new ServerMessage(messagePrefix + args.Item2));
            // GameNetwork.EndModuleEventAsServer();
            return false;
        }
        return true;
    }
    // Function to get all available factions
    public List<string> GetAllFactions()
    {
        return MultiplayerOptions.Instance.GetMultiplayerOptionsList(OptionType.CultureTeam1);
    }

    // Function to calculate Levenshtein Distance
    int LevenshteinDistance(string s, string t)
    {
        int[,] d = new int[s.Length + 1, t.Length + 1];

        if (s.Length == 0) return t.Length;
        if (t.Length == 0) return s.Length;

        for (int i = 0; i <= s.Length; i++)
            d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++)
            d[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[s.Length, t.Length];
    }
    public override bool Execute(NetworkCommunicator executor, string args)
    {
        if (!executor.IsAdmin)
        {
            Helper.SendMessageToPeer(executor, "Only admins can use this command.");
            return true;
        };

        string[] stringArgs = args.Trim().Split();

        if (stringArgs.Length != 3)
        {
            Helper.SendMessageToPeer(executor, "Invalid number of arguments");
            // GameNetwork.BeginModuleEventAsServer(executor);
            // GameNetwork.WriteMessage(new ServerMessage("Invalid number of arguments"));
            // GameNetwork.EndModuleEventAsServer();
            return true;
        }

        string userInput = stringArgs[0].ToLower();

        TaleWorlds.MountAndBlade.MultiplayerOptions.OptionType currentmap = MultiplayerOptions.OptionType.Map;

        // List of maps and their respective real names
        Dictionary<string, string> maps = new Dictionary<string, string>()
        {
            { "Groupfight", "mp_groupfight" },
            { "VDGroupfight", "VD_Groupfight_01" },
            { "current", $"{currentmap.GetValueText()}" }
        };
      
        // Convert all keys in the dictionary to lowercase for case-insensitive matching
        Dictionary<string, string> mapsLower = maps.ToDictionary(m => m.Key.ToLower(), m => m.Value);

        string finalmapname = "";
        string finalmapcode = ""; 
  
        // Try to find the exact match (case-insensitive)
        if (mapsLower.ContainsKey(userInput))
        {
            finalmapcode = mapsLower[userInput];
            finalmapname = userInput;
        }
        else
        {
            // Check if the input is long enough for partial matching (set a minimum length)
            const int minimumPartialMatchLength = 1;

            if (userInput.Length >= minimumPartialMatchLength)
            {
                // Try to find partial matches (substring)
                var partialMatches = mapsLower.Keys.Where(map => map.Contains(userInput)).ToList();

                if (partialMatches.Count == 1)
                {
                    // Only one match, accept it
                    finalmapcode = mapsLower[partialMatches[0]];
                    finalmapname = partialMatches[0];
                }
                else if (partialMatches.Count > 1)
                {
                    // Multiple matches, inform the user
                    string matchingMaps = string.Join(", ", partialMatches);
                    Helper.SendMessageToPeer(executor, $"Multiple maps match '{stringArgs[0]}': {matchingMaps}. Please be more specific.");
                    return true;
                }
            }

            if (string.IsNullOrEmpty(finalmapname))
            {
                // If no partial match, use fuzzy matching to find the closest match
                string closestMap = FindClosestMatch(userInput, mapsLower.Keys.ToList());

                if (closestMap != null)
                {
                    finalmapcode = mapsLower[closestMap];
                    finalmapname = closestMap;
                }
                else
                {
                    Helper.SendMessageToPeer(executor, $"Map '{stringArgs[0]}' not found, check command !maps for the available maps.");
                    return true;
                }
            }
        }

        // Proceed with changing the map to finalmapcode here

        // Fuzzy matching using Levenshtein distance
        string FindClosestMatch(string input, List<string> mapNames)
        {
            const int threshold = 3; // Adjust for leniency

            string closestMatch = null;
            int smallestDistance = int.MaxValue;

            foreach (var map in mapNames)
            {
                int distance = LevenshteinDistance(input, map);
                if (distance < smallestDistance && distance <= threshold)
                {
                    smallestDistance = distance;
                    closestMatch = map;
                }
            }

            return closestMatch;
        }

        List<string> availableFactions = GetAllFactions();

        Tuple<bool,string> GetClosestFaction(string inputFaction)
        {
            // Normalize the input faction (lowercase to ignore case differences)
            inputFaction = inputFaction.ToLower();
            
            string closestFaction = null;

            var partialMatches = availableFactions.Where(faction => faction.Contains(inputFaction)).ToList();

            if (partialMatches.Count == 1)
            {
                // Only one match, accept it
                closestFaction = partialMatches[0];
                return new Tuple<bool, string>(true, closestFaction);

            } else if (partialMatches.Count > 1)
            {
                // Multiple matches, inform the user
                string matchingFactions = string.Join(", ", partialMatches);
                Helper.SendMessageToPeer(executor, $"Multiple maps match '{stringArgs[0]}': {matchingFactions}. Please be more specific.");
                return new Tuple<bool, string>(false, "Multiple maps matching");
            } else { 

                // If no exact prefix match, find the closest one using Levenshtein distance
                int shortestDistance = int.MaxValue;

                foreach (var faction in availableFactions)
                {
                    int distance = LevenshteinDistance(inputFaction, faction.ToLower());
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestFaction = faction;
                    }
                }

                // You can set a threshold to allow only reasonable typos (e.g., distance <= 3)
                if (shortestDistance <= 3)
                {
                    return new Tuple<bool, string>(true, closestFaction);
                } else
                {
                    Helper.SendMessageToPeer(executor, $"Couldn't find the exact faction ");
                    return new Tuple<bool, string>(false, "Not found");
                }
            }
        }

        string faction1SearchString = stringArgs[1];
        Tuple<bool, string> faction1SearchResult = GetClosestFaction(faction1SearchString);
        if (!ArgValid(faction1SearchResult, executor, "Faction1: "))
        {
            return true;
        }

        string faction2SearchString = stringArgs[2];
        Tuple<bool, string> faction2SearchResult = GetClosestFaction(faction2SearchString);
        if (!ArgValid(faction2SearchResult, executor, "Faction2: "))
        {
            return true;
        }

        // All arguments are good, change the map and the factions
        string mapName = finalmapcode;
        string faction1 = faction1SearchResult.Item2;
        string faction2 = faction2SearchResult.Item2;

        Helper.SendMessageToAllPeers("Map: " + finalmapname);
        // GameNetwork.BeginBroadcastModuleEvent();
        // GameNetwork.WriteMessage(new ServerMessage("Map: " + mapName));
        // GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        Helper.SendMessageToAllPeers("Faction1: " + faction1);
        // GameNetwork.BeginBroadcastModuleEvent();
        // GameNetwork.WriteMessage(new ServerMessage("Faction1: " + faction1));
        // GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        Helper.SendMessageToAllPeers("Faction1: " + faction2);
        // GameNetwork.BeginBroadcastModuleEvent();
        // GameNetwork.WriteMessage(new ServerMessage("Faction2: " + faction2));
        // GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);


        MultiplayerOptions.OptionType.Map.SetValue(mapName, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions);
        MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam1, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(faction1);
        MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.CultureTeam2, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(faction2);
        MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
        MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;
        DedicatedCustomServerSubModule.Instance.ServerSideIntermissionManager.EndMission();

        return true;
    }
}