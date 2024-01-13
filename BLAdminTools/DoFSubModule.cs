using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;


namespace DoFAdminTools
{
    public class DoFSubModule: MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            
            AddConsoleCommands();
            Helper.Print("Loaded.");
        }
        
        public override void OnMultiplayerGameStart(Game game, object starterObject)
        {
            base.OnMultiplayerGameStart(game, starterObject);
            
            Helper.Print("Adding GameHandler.");
            game.AddGameHandler<DoFGameHandler>();
        }

        private void AddConsoleCommands() => DedicatedServerConsoleCommandManager.AddType(typeof(ConsoleCommands));
    }
}