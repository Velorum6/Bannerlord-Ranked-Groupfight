using DoFAdminTools.ChatCommands;
using DoFAdminTools.Helpers;
using DoFAdminTools.Repositories;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using PlayerMessageAll = NetworkMessages.FromClient.PlayerMessageAll;
using PlayerMessageTeam = NetworkMessages.FromClient.PlayerMessageTeam;

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
        
        protected override void OnGameNetworkBegin()
        {
            base.OnGameNetworkBegin();
            Helper.Print("Registering Chat handlers...");
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }

        protected override void OnGameNetworkEnd()
        {
            base.OnGameNetworkEnd();
            Helper.Print("Unregistering Chat handlers...");
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
        
        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            if (!GameNetwork.IsServer) 
                return;
            
            GameNetwork.NetworkMessageHandlerRegisterer handlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            handlerRegisterer.Register<PlayerMessageAll>(HandleClientEventPlayerMessageAll);
            handlerRegisterer.Register<PlayerMessageTeam>(HandleClientEventPlayerMessageTeam);
        }

        private bool HandleClientEventPlayerMessageAll(NetworkCommunicator peer, PlayerMessageAll message)
        {
            return HandleChatMessage(peer, message.Message);
        }
        
        private bool HandleClientEventPlayerMessageTeam(NetworkCommunicator peer, PlayerMessageTeam message)
        {
            return HandleChatMessage(peer, message.Message);
        }

        private bool HandleChatMessage(NetworkCommunicator sender, string message)
        {
            if (!message.StartsWith("!")) // TODO use configurable prefix
                return true; // don't hide, show in chat

            ChatCommandHandler.Instance.ExecuteCommand(sender, message);
            
            return false; // "hide" message from other MessageHandlers, making it not show up in chat for players
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