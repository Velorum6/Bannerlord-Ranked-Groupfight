using DoFAdminTools.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.MultiplayerOptions;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;


namespace DoFAdminTools.ChatCommands.PublicCommands;

public class mapsCommand : ChatCommand
{
    public override string CommandText => "maps";
    public override string Description => "Prints a list of maps available in the server";
   
    public override bool CanExecute(NetworkCommunicator executor) => true;

    public override bool Execute(NetworkCommunicator executor, string args)
    {
        string[] maps = new string[] { "Groupfight", "VDGroupfight", "Current" };

        // Join the maps into a single string with each map on a new line
        string availableMaps = string.Join(" | ", maps);

        // Send the list of available maps to the peer
        Helper.SendMessageToPeer(executor, "Available maps:\n" + availableMaps);

        return true;
    }
}
