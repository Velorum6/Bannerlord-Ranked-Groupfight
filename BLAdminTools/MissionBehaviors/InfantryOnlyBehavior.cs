using TaleWorlds.MountAndBlade;
using TaleWorlds.Core;
using DoFAdminTools.Helpers;
using NetworkMessages.FromServer;

namespace DoFAdminTools.MissionBehaviors
{
    public class InfantryOnlyBehavior : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        private MultiplayerRoundController _roundController;

        public override void AfterStart()
        {
            base.AfterStart();

            _roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();

            if (_roundController != null)
            {
                _roundController.OnRoundStarted += RestrictNonInfantryClasses;
            }

            // Apply restrictions to all current peers
            foreach (var peer in GameNetwork.NetworkPeers)
            {
                ApplyRestrictionsToPeer(peer);
            }

            // Subscribe to the peer synchronized event
            NetworkCommunicator.OnPeerSynchronized += OnPeerSynchronized;
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            if (!GameNetwork.IsServer)
            {
                base.OnAgentBuild(agent, banner);
                return;
            }

            base.OnAgentBuild(agent, banner);
            
            RemoveRestrictedItems(agent, banner);
        }

        private void RemoveRestrictedItems(Agent agent, Banner banner)
        {
            var missionPeer = agent.MissionPeer;
            if (missionPeer == null) return;

            Equipment newEquipment = new Equipment();
            newEquipment.FillFrom(agent.SpawnEquipment);

            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumPrimaryWeaponSlots; i++)
            {
                MissionWeapon mp = agent.Equipment[i];
                if (mp.IsEmpty || mp.Item.Weapons == null)
                {
                    continue;
                }

                if (mp.Item.ItemType == ItemObject.ItemTypeEnum.Arrows || mp.Item.ItemType == ItemObject.ItemTypeEnum.Bolts || mp.Item.ItemType == ItemObject.ItemTypeEnum.Thrown || mp.Item.ItemType == ItemObject.ItemTypeEnum.Polearm || mp.Item.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon || mp.Item.ItemType == ItemObject.ItemTypeEnum.Crossbow || mp.Item.ItemType == ItemObject.ItemTypeEnum.Bow)
                {
                    agent.RemoveEquippedWeapon(i); // Unequip an item if it's a spear, throwing or two handed weapon.
                }
            }
        }

        private void OnPeerSynchronized(NetworkCommunicator peer)
        {
            ApplyRestrictionsToPeer(peer);
        }

        private void RestrictNonInfantryClasses()
        {
            foreach (var peer in GameNetwork.NetworkPeers)
            {
                ApplyRestrictionsToPeer(peer);
            }
        }

        private void ApplyRestrictionsToPeer(NetworkCommunicator peer)
        {
            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new ChangeClassRestrictions(FormationClass.Cavalry, true));
            GameNetwork.EndModuleEventAsServer();

            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new ChangeClassRestrictions(FormationClass.HorseArcher, true));
            GameNetwork.EndModuleEventAsServer();

            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new ChangeClassRestrictions(FormationClass.Ranged, true));
            GameNetwork.EndModuleEventAsServer();
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();

            if (_roundController != null)
            {
                _roundController.OnRoundStarted -= RestrictNonInfantryClasses;
            }

            // Unsubscribe from the peer synchronized event
            NetworkCommunicator.OnPeerSynchronized -= OnPeerSynchronized;
        }

        private void ChangePlayerTeam(NetworkCommunicator player, Team newTeam)
        {
            var missionPeer = player.GetComponent<MissionPeer>();
            if (missionPeer != null && missionPeer.Team != newTeam)
            {
        missionPeer.Team = newTeam;
    }
}

    }
}
