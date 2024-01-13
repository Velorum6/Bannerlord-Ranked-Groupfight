using DoFAdminTools.repositories;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace DoFAdminTools
{
    public class DoFGameHandler: GameHandler
    {
        public override void OnBeforeSave() { }

        public override void OnAfterSave() { }

        protected override void OnPlayerConnect(VirtualPlayer peer)
        {
            string peerId = peer.Id.ToString();
            
            if (AdminRepository.Instance.IsAdmin(peerId) && peer.Communicator is NetworkCommunicator networkPeer)
            {
                networkPeer.UpdateForJoiningCustomGame(true); // set as admin
                SyncAdminOptionsToPeer(networkPeer);
                
                Helper.Print($"{peer.UserName} joined as admin (ID = {peerId})");
            }
        }

        private void SyncAdminOptionsToPeer(NetworkCommunicator networkPeer)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new MultiplayerOptionsDefault());
            GameNetwork.EndModuleEventAsServer();
            foreach (CustomGameUsableMap usableMap in MultiplayerIntermissionVotingManager.Instance.UsableMaps)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new MultiplayerIntermissionUsableMapAdded(usableMap.map, usableMap.isCompatibleWithAllGameTypes, usableMap.isCompatibleWithAllGameTypes ? 0 : usableMap.compatibleGameTypes.Count, usableMap.compatibleGameTypes));
                GameNetwork.EndModuleEventAsServer();
            }
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new UpdateIntermissionVotingManagerValues());
            GameNetwork.EndModuleEventAsServer();
        }
    }
}