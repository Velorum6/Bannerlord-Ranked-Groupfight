using DoFAdminTools.Helpers;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools.ChatCommands.AdminCommands;

public class EndWarmupCommand : AdminChatCommand
{
    public override string CommandText => "endwarmup";

    public override string Description =>
        "Sets the warmup duration to 30 seconds.";
    public override bool Execute(NetworkCommunicator executor, string args)
    {
        MultiplayerWarmupComponent multiplayerWarmupComponent = Mission.Current.GetMissionBehavior<MultiplayerWarmupComponent>();
        if (multiplayerWarmupComponent == null || !multiplayerWarmupComponent.IsInWarmup)
        {
            Helper.SendMessageToPeer(executor, $"You can only end the warmup during the warmup phase.");
            return true;
        }
        multiplayerWarmupComponent.EndWarmupProgress();
        Helper.SendMessageToAllPeers($"{executor.UserName} reduced the warmup duration.");

        return true;
    }
}