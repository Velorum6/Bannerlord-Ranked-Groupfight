using System;
using System.IO;
using DoFAdminTools.Helpers;
using DoFAdminTools.Repositories;
using JetBrains.Annotations;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools
{
    public class ConsoleCommands
    {
        [UsedImplicitly]
        [ConsoleCommandMethod("dat_add_admin",
            "Add the ID of a player to be given admin permissions upon login, without using the admin password")]
        private static void AddAdminCommand(string adminId)
        {
            Helper.Print("Trying to add admin " + adminId);

            var adminRepo = AdminRepository.Instance;

            // TODO verify the given adminId is an actual playerId
            adminRepo.AddAdmin(adminId);
        }

        private static readonly Action<string> HandleConsoleCommand =
            (Action<string>) Delegate.CreateDelegate(typeof(Action<string>),
                typeof(DedicatedServerConsoleCommandManager).GetStaticMethodInfo("HandleConsoleCommand"));

        private static readonly string NativeModulePath =
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../Modules/Native/"));

        [UsedImplicitly]
        [ConsoleCommandMethod("dat_include",
            "Include another config file to be parsed as well. Useful for data shared between multiple configurations.")]
        private static void IncludeConfigFileCommand(string configFileName)
        {
            Helper.Print($"Trying to include file {configFileName}");

            string fullTargetPath = Path.GetFullPath(Path.Combine(NativeModulePath, configFileName));

            if (!fullTargetPath.StartsWith(NativeModulePath))
            {
                Helper.PrintError(
                    $"\tGiven Path ({configFileName}) leads to location ({fullTargetPath}) outside of your Modules/Native/ directory ({NativeModulePath}), therefore it is not included.");
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
    }
}