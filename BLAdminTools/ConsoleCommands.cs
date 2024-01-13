using DoFAdminTools.repositories;
using JetBrains.Annotations;
using TaleWorlds.MountAndBlade;

namespace DoFAdminTools
{
    public class ConsoleCommands
    {
        [UsedImplicitly]
        [ConsoleCommandMethod("dat_add_admin", "Add the ID of a player to be given admin permissions upon login, without using the admin password")]
        private static void TestCommand(string adminId)
        {
            Helper.Print("Trying to add admin " + adminId);

            var adminRepo = AdminRepository.Instance;

            // TODO verify the given adminId is an actual playerId
            adminRepo.AddAdmin(adminId);
        }
    }
}