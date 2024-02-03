﻿using NetworkMessages.FromServer;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools.Helpers;

public static class Helper
{
    private const string Prefix = "[DAT] ";
    private const string WarningPrefix = Prefix + "[WARN] ";
    private const string ErrorPrefix = "[ERROR] ";

    public static void Print(string message) => 
        Debug.Print(Prefix + message, 0, Debug.DebugColor.DarkGreen);
        
        
    public static void PrintWarning(string message) => 
        Debug.Print(WarningPrefix + message, 0, Debug.DebugColor.DarkYellow);
        
        
    public static void PrintError(string message) => 
        Debug.Print(ErrorPrefix + message, 0, Debug.DebugColor.DarkRed);
        
    public static void SendMessageToAllPeers(string message)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new ServerMessage(Prefix + message));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients);
    }

    public static void SendMessageToPeer(NetworkCommunicator peer, string message)
    {
        GameNetwork.BeginModuleEventAsServer(peer);
        GameNetwork.WriteMessage(new ServerMessage(Prefix + message));
        GameNetwork.EndModuleEventAsServer();
    }
}