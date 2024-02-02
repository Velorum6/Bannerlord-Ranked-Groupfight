using DoFAdminTools.Helpers;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools.ChatCommands.AdminCommands
{
    public class RemoveHorsesCommand : AdminChatCommand
    {
        public override string CommandText => "removeHorses";

        public override string Description =>
            "Removes all unmounted horses.";

        public override bool Execute(NetworkCommunicator executor, string args)
        {

            if (Mission.Current.MountsWithoutRiders.Count > 0)
            {
                foreach (var pair in Mission.Current.MountsWithoutRiders)
                {
                    pair.Key?.FadeOut(true, true);
                }
                Helper.SendMessageToAllPeers($"{executor.UserName} removed all stray horses.");
                return true;
            }

            Helper.SendMessageToPeer(executor, "No unmounted horses were found.");
            return false;
        }
    }
}