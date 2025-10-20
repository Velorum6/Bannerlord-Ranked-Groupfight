using System.IO;
using DoFAdminTools.ChatCommands;
using DoFAdminTools.ChatCommands.AdminCommands;
using DoFAdminTools.ChatCommands.AdminCommands.Teleport;
using DoFAdminTools.ChatCommands.PublicCommands;
using DoFAdminTools.Helpers;
using DoFAdminTools.MissionBehaviors;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using TaleWorlds.MountAndBlade.ListedServer;
using NetworkMessages.FromServer;
namespace DoFAdminTools;

public class DoFSubModule: MBSubModuleBase
{
    public static bool EnforceMirrorMatchupOnMapChange = true;
    protected override void OnApplicationTick(float dt)
{
    base.OnApplicationTick(dt);

    if (EnforceMirrorMatchupOnMapChange && DedicatedCustomServerSubModule.Instance.ServerSideIntermissionManager != null && DedicatedCustomServerSubModule.Instance.ServerSideIntermissionManager!.AutomatedBattleState == AutomatedBattleState.CountingForNextBattle)    {
        // When the "StartingNextBattle" state was reached the vote is already over which means that Culture1 is automatically the one with the most votes. Therefore we can set culture2 = culture1 to enforce a mirror matchup.
        MultiplayerOptions.OptionType.CultureTeam2.SetValue(MultiplayerOptions.OptionType.CultureTeam1.GetValueText(MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions), MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions);
        MultiplayerOptions.OptionType.CultureTeam2.SetValue(MultiplayerOptions.OptionType.CultureTeam1.GetValueText());
        Helper.Print("Enforcing Mirror Matchup on Map Change");
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new MultiplayerOptionsInitial());
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients);
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new MultiplayerOptionsImmediate());
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients);

        EnforceMirrorMatchupOnMapChange = false; // This needs to be set to true again as soon as the new map was loaded.
    }
}

    public static readonly string NativeModulePath =
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../Modules/Native/"));

    private AutoMessageHandler _autoMessageHandler;

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

    public override void OnMissionBehaviorInitialize(Mission mission)
    {
        base.OnMissionBehaviorInitialize(mission);

        if (_autoMessageHandler == null || !_autoMessageHandler.Enabled)
            _autoMessageHandler = new AutoMessageHandler();
        
        if (!DoFConfigOptions.Instance.PreventHpSyncToEnemies)
            return;
        
        // Disable HP sync between enemies for all Flag Domination Gamemodes (Skirmish/Battle/Captain).
        MissionMultiplayerGameModeBase gamemode = mission.GetMissionBehavior<MissionMultiplayerGameModeBase>();
        if (gamemode is MissionMultiplayerFlagDomination && mission.GetMissionBehavior<HpSyncAntiCheat>() == null)
        {
            mission.AddMissionBehavior(new HpSyncAntiCheat());
        }
        
        mission.AddMissionBehavior(new HitCounter());

        mission.AddMissionBehavior(new Multikill());

        mission.AddMissionBehavior(new InfantryOnlyBehavior());

        mission.AddMissionBehavior(new AutoBalanceBehavior());
    }

    private void AddConsoleCommands() => DedicatedServerConsoleCommandManager.AddType(typeof(ConsoleCommands));
        
    private void RegisterChatCommands()
    {
        ChatCommandHandler commandHandler = ChatCommandHandler.Instance;

        commandHandler.RegisterCommand(new PlayerInfoCommand());
        commandHandler.RegisterCommand(new HealCommand());
        commandHandler.RegisterCommand(new RemoveHorsesCommand());
        commandHandler.RegisterCommand(new SlayCommand());
        commandHandler.RegisterCommand(new ListSpectatorsCommand());

        // Velorum Commands
        commandHandler.RegisterCommand(new mapsCommand());
        commandHandler.RegisterCommand(new SetMapCommand());
        commandHandler.RegisterCommand(new PartyCommand());

        // Warmup Commands
        commandHandler.RegisterCommand(new ExtendWarmupCommand());
        commandHandler.RegisterCommand(new EndWarmupCommand());

        // Teleport Commands
        commandHandler.RegisterCommand(new MoveCommand());
        commandHandler.RegisterCommand(new TeleportMeToCommand());
        commandHandler.RegisterCommand(new TeleportToMeCommand());
        
        
        // the help command should always be registered last, as it only shows commands registered *before* it.
        commandHandler.RegisterCommand(new HelpCommand());
    }
}