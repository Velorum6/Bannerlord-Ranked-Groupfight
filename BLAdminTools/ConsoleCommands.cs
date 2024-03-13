using System;
using System.IO;
using DoFAdminTools.Helpers;
using DoFAdminTools.Repositories;
using JetBrains.Annotations;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace DoFAdminTools;

public static class ConsoleCommands
{
    private static DoFConfigOptions _configOptions = DoFConfigOptions.Instance;
    private static readonly int MaxAutoMessageLength = 256;
        
    [UsedImplicitly]
    [ConsoleCommandMethod("dat_add_admin",
        "Add the ID of a player to be given admin permissions upon login, without using the admin password")]
    private static void AddAdminCommand(string adminId)
    {
        Helper.Print("Trying to add admin " + adminId);

        var adminRepo = AdminRepository.Instance;

        try
        {
            PlayerId playerId = PlayerId.FromString(adminId);
            // TODO: check if playerId equality checks are fine yet, if so, transform adminRepo to use playerIds
            adminRepo.AddAdmin(adminId); 
        }
        catch (FormatException ex)
        {
            Helper.PrintError($"\tCould not parse {adminId} as a PlayerId, skipping.");
        }
    }

    private static readonly Action<string> HandleConsoleCommand =
        (Action<string>) Delegate.CreateDelegate(typeof(Action<string>),
            typeof(DedicatedServerConsoleCommandManager).GetStaticMethodInfo("HandleConsoleCommand"));

    [UsedImplicitly]
    [ConsoleCommandMethod("dat_include",
        "Include another config file to be parsed as well. Useful for data shared between multiple configurations.")]
    private static void IncludeConfigFileCommand(string configFileName)
    {
        Helper.Print($"Trying to include file {configFileName}");
        string nativeModulePath = DoFSubModule.NativeModulePath;

        string fullTargetPath = Path.GetFullPath(Path.Combine(nativeModulePath, configFileName));

        if (!fullTargetPath.StartsWith(nativeModulePath))
        {
            Helper.PrintError(
                $"\tGiven Path ({configFileName}) leads to location ({fullTargetPath}) outside of your Modules/Native/ directory ({nativeModulePath}), therefore it is not included.");
            return;
        }

        if (!File.Exists(fullTargetPath))
        {
            Helper.PrintError($"\tGiven File ({fullTargetPath}) does not exist.");
            return;
        }

        Helper.Print("\tReading config file " + fullTargetPath);
            
        string[] lines = File.ReadAllLines(fullTargetPath);

        foreach (string currentLine in lines)
        {
            if (!currentLine.StartsWith("#") && !string.IsNullOrWhiteSpace(currentLine))
            {
                HandleConsoleCommand(currentLine);
            }
        }
            
        Helper.Print("\tDone reading config file " + fullTargetPath);
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("dat_set_command_prefix",
        "Set the prefix for ingame chat commands. Note that '/' will not work.")]
    private static void SetCommandPrefixCommand(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            Helper.PrintError("No prefix provided for dat_set_command_prefix");
            return;
        }
            
        prefix = prefix.Trim();

        if (prefix.StartsWith("/"))
        {
            Helper.PrintError("dat_set_command_prefix: Can't set prefix starting with '/'.");
            return;
        }

        _configOptions.CommandPrefix = prefix;
        Helper.Print($"Set command prefix to {prefix}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("dat_set_show_joinleave_messages",
        "[True/False] Set whether to show messages in chat when someone joins or leaves the server.")]
    private static void SetShowJoinLeaveMessagesCommand(string show)
    {
        if (!bool.TryParse(show, out bool showMessages))
        {
            Helper.PrintError($"dat_set_show_joinleave_messages: Could not parse boolean (True/False) from '{show}'");
            return;
        }

        _configOptions.ShowJoinLeaveMessages = showMessages;
            
        Helper.Print($"Set ShowJoinLeaveMessages to {showMessages}");
    }
    
    [UsedImplicitly]
    [ConsoleCommandMethod("dat_set_show_adminpanel_usage",
        "[True/False] Set whether to show messages in chat when an admin changes something in the admin panel.")]
    private static void SetShowAdminPanelUsageMessagesCommand(string show)
    {
        if (!bool.TryParse(show, out bool showMessages))
        {
            Helper.PrintError($"dat_set_show_adminpanel_usage: Could not parse boolean (True/False) from '{show}'");
            return;
        }

        _configOptions.ShowAdminPanelUsageMessages = showMessages;
            
        Helper.Print($"Set ShowAdminPanelUsageMessages to {showMessages}");
    }
    
    [UsedImplicitly]
    [ConsoleCommandMethod("dat_set_prevent_unnecessary_hp_sync",
        "[True/False] [Only for Flag-Domination game modes] Set whether to prevent synchronizing player HP to their respective opponents teams, which could be used by cheating tools.")]
    private static void SetPreventUnnecessaryHpSyncCommand(string shouldPreventStr)
    {
        if (!bool.TryParse(shouldPreventStr, out bool shouldPrevent))
        {
            Helper.PrintError($"dat_set_prevent_unnecessary_hp_sync: Could not parse boolean (True/False) from '{shouldPreventStr}'");
            return;
        }

        _configOptions.PreventHpSyncToEnemies = shouldPrevent;
            
        Helper.Print($"Set PreventHPSyncToEnemies to {shouldPrevent}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("dat_add_automessage", "Adds a new message to the AutoMessage system.")]
    private static void AddAutoMessage(string message)
    {
        string trimmedMessage = message.Trim();
        if (string.IsNullOrWhiteSpace(trimmedMessage))
        {
            Helper.PrintError($"Can't add '{message}'; empty text.");
            return;
        }

        if (trimmedMessage.Length > MaxAutoMessageLength)
        {
            Helper.PrintError($"Can't add '{message}'; too long.");
            return;
        }

        _configOptions.AutoMessages.Add(trimmedMessage);
        Helper.Print($"Added AutoMessage '{trimmedMessage}'");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("dat_set_automessage_interval", 
        "Sets how often (in seconds) automated server messages are sent. Set to a negative value or zero to send no messages.")]
    private static void SetAutoMessageInterval(string intervalStr)
    {
        if (!int.TryParse(intervalStr, out int interval))
        {
            Helper.PrintError($"Could not parse '{intervalStr}' as number for AutoMessageInterval.");
            return;
        }

        _configOptions.AutoMessageInterval = interval;
        
        if (interval <= 0)
            Helper.PrintWarning($"AutoMessageInterval set to {interval}, no messages will be sent.");
        else
            Helper.Print($"AutoMessageInterval set to {interval}.");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("dat_set_and_load_banlist",
        "Set the ban list file, then load all bans contained in the file if it exists.")]
    private static void SetAndLoadBanlistCommand(string banListPath)
    {
        Helper.Print($"Trying to load ban list file {banListPath}");
        string nativeModulePath = DoFSubModule.NativeModulePath;

        string fullTargetPath = Path.GetFullPath(Path.Combine(nativeModulePath, banListPath));

        if (!fullTargetPath.StartsWith(nativeModulePath))
        {
            Helper.PrintError(
                $"\tGiven Path ({banListPath}) leads to location ({fullTargetPath}) outside of your Modules/Native/ directory ({nativeModulePath}), therefore it can not be loaded.");
            return;
        }

        _configOptions.BanListFileName = banListPath;
        Helper.Print($"\tSet BanListFileName to {banListPath}");

        if (!File.Exists(fullTargetPath))
        {
            Helper.PrintWarning($"\tNo ban list file found at path {fullTargetPath}. Path will be used for new bans but no existing bans are loaded right now.");
            return;
        }
            
        Helper.Print("\tLoading ban list from " + fullTargetPath);

        string[] lines = File.ReadAllLines(fullTargetPath);

        foreach (string line in lines)
        {
            var currentLine = line.Trim();
                
            if (currentLine.StartsWith("#"))
                continue;

            int commentSignIndex = currentLine.IndexOf("#", StringComparison.Ordinal);

            if (commentSignIndex != -1)
                currentLine = currentLine.Substring(0, commentSignIndex).TrimEnd();
            try
            {
                PlayerId playerId = PlayerId.FromString(currentLine);
                CustomGameBannedPlayerManager.AddBannedPlayer(playerId, int.MaxValue);
            }
            catch (FormatException ex)
            {
                Helper.PrintError($"\tCould not parse {currentLine} as a PlayerId, skipping.");
            }
        }
            
        Helper.Print("\tDone reading ban list");
    }
}