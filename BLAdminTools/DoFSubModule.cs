using System.IO;
using DoFAdminTools.ChatCommands;
using DoFAdminTools.ChatCommands.AdminCommands;
using DoFAdminTools.ChatCommands.AdminCommands.Teleport;
using DoFAdminTools.ChatCommands.PublicCommands;
using DoFAdminTools.Helpers;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;


namespace DoFAdminTools;

public class DoFSubModule: MBSubModuleBase
{
    public static readonly string NativeModulePath =
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../Modules/Native/"));
        
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
        commandHandler.RegisterCommand(new HealCommand());
        commandHandler.RegisterCommand(new RemoveHorsesCommand());
        commandHandler.RegisterCommand(new SlayCommand());

        // Warmup Commands
        commandHandler.RegisterCommand(new ExtendWarmupCommand());
        commandHandler.RegisterCommand(new EndWarmupCommand());

        // Teleport Commands
        commandHandler.RegisterCommand(new MoveCommand());
        commandHandler.RegisterCommand(new TeleportMeToCommand());
        commandHandler.RegisterCommand(new TeleportToMeCommand());
            
    }
}