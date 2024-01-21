using DoFAdminTools.ChatCommands;
using DoFAdminTools.ChatCommands.AdminCommands;
using DoFAdminTools.ChatCommands.PublicCommands;
using DoFAdminTools.Helpers;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;


namespace DoFAdminTools
{
    public class DoFSubModule: MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            
            Helper.Print("Loading...");
            
            Helper.Print("Adding Console Commands...");
            AddConsoleCommands();
            
            Helper.Print("Registering Chat Commands...");
            RegisterChatCommands();
            
            Helper.Print("Loaded.");
        }

        public override void OnMultiplayerGameStart(Game game, object starterObject)
        {
            base.OnMultiplayerGameStart(game, starterObject);
            
            Helper.Print("Adding GameHandler.");
            game.AddGameHandler<DoFGameHandler>();
        }

        private void AddConsoleCommands() => DedicatedServerConsoleCommandManager.AddType(typeof(ConsoleCommands));
        
        private void RegisterChatCommands()
        {
            ChatCommandHandler commandHandler = ChatCommandHandler.Instance;

            commandHandler.RegisterCommand(new MeCommand());
            commandHandler.RegisterCommand(new PlayerInfoCommand());
        }
    }
}