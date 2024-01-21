using System;
using System.Collections.Generic;
using System.Linq;
using DoFAdminTools.Helpers;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools.ChatCommands
{
    public class ChatCommandHandler
    {
        public static ChatCommandHandler Instance { get; } = new ChatCommandHandler();

        private readonly Dictionary<string, ChatCommand> _registeredCommands =
            new Dictionary<string, ChatCommand>();

        public List<ChatCommand> Commands => _registeredCommands.Values.ToList();

        public bool RegisterCommand(ChatCommand command)
        {
            if (_registeredCommands.ContainsKey(command.CommandText))
                return false;

            _registeredCommands.Add(command.CommandText, command);

            Helper.Print($"Registered Command {command.CommandText}");
            return true;
        }

        public bool ExecuteCommand(NetworkCommunicator executor, string command)
        {
            if (executor == null || string.IsNullOrWhiteSpace(command))
                return false;

            if (command.Length == 1) // TODO when prefix is configurable, check for configured prefix length
                return false;

            // cut off arguments to find command name only
            int firstWhiteSpaceIndex = command.IndexOf(' ');
            string commandName = firstWhiteSpaceIndex == -1
                ? command.Substring(1)
                : command.Substring(1, firstWhiteSpaceIndex - 1);

            if (!_registeredCommands.TryGetValue(commandName, out ChatCommand chatCommand)
                || chatCommand.CanExecute(executor))
            {
                Helper.SendMessageToPeer(executor,
                    "That command does not exist or you are not allowed to use it right now.");
                return false;
            }

            string args = command.Substring(1 + commandName.Length).Trim();
            return chatCommand.Execute(executor, args);
        }
    }
}