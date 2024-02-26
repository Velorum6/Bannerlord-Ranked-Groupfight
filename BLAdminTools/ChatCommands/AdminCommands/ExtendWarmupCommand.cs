using DoFAdminTools.Helpers;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools.ChatCommands.AdminCommands;

public class ExtendWarmupCommand : AdminChatCommand
{
    public override string CommandText => "extendwarmup";

    public override string Description =>
        "Extends the warmup duration to the maximum configured value.";
    public override bool Execute(NetworkCommunicator executor, string args)
    {
        MultiplayerWarmupComponent multiplayerWarmupComponent = Mission.Current.GetMissionBehavior<MultiplayerWarmupComponent>();
        if (multiplayerWarmupComponent == null || !multiplayerWarmupComponent.IsInWarmup)
        {
            Helper.SendMessageToPeer(executor, $"You can only extend the warmup duration during the warmup phase.");
            return true;
        }

        PropertyInfo warmupStateProperty = ReflectionExtensions.GetPropertyInfo(multiplayerWarmupComponent, "WarmupState");
        warmupStateProperty.SetValue(warmupStateProperty, MultiplayerWarmupComponent.WarmupStates.InProgress);
        MultiplayerTimerComponent timerComponent = ReflectionExtensions.GetFieldValue<MultiplayerTimerComponent>(multiplayerWarmupComponent, "_timerComponent");
        timerComponent?.StartTimerAsServer(MultiplayerWarmupComponent.TotalWarmupDuration);
        Helper.SendMessageToAllPeers($"{executor.UserName} extended the warmup duration!");

        return true;
    }
}