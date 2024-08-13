using EFT.Interactive;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Mover;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class DoorFinder2 : PlayerComponentBase
    {
        private const float DOORS_UPDATE_FREQ = 1f;

        public List<DoorData> CloseDoors { get; } = new List<DoorData>();
        public List<DoorData> AllDoors { get; } = new List<DoorData>();

        private Collider[] _doorColliders = new Collider[300];

        private float _nextUpdateDoorTime;

        public DoorFinder2(PlayerComponent player) : base(player)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            updateCurrentDoors();
        }

        public void Dispose()
        {
        }

        private void updateCurrentDoors()
        {
            if (_nextUpdateDoorTime < Time.time) {
                _nextUpdateDoorTime = Time.time + DOORS_UPDATE_FREQ;
                findDoorsInLayer(LayerMaskClass.DoorLayer);
                if (AllDoors.Count > 0) {
                    Logger.LogInfo("found doors in DoorLayer");
                    return;
                }
                findDoorsInLayer(LayerMaskClass.PlayerStaticDoorMask);
                if (AllDoors.Count > 0) {
                    Logger.LogInfo("found doors in PlayerStaticDoorMask");
                    return;
                }
                findDoorsInLayer(LayerMaskClass.InteractiveMask);
                if (AllDoors.Count > 0) {
                    Logger.LogInfo("found doors in InteractiveMask");
                    return;
                }
                findDoorsInLayer(LayerMaskClass.InteractiveLayer);
                if (AllDoors.Count > 0) {
                    Logger.LogInfo("found doors in InteractiveLayer");
                    return;
                }
            }
        }

        private void findDoorsInLayer(LayerMask layer)
        {
            AllDoors.Clear();
            for (int i = 0; i < _doorColliders.Length; i++) {
                _doorColliders[i] = null;
            }
            int hits = Physics.OverlapSphereNonAlloc(Player.Position, 50, _doorColliders, layer);
            for (int i = 0; i < _doorColliders.Length; i++) {
                Collider collider = _doorColliders[i];
                if (collider == null) continue;
                Door door = collider.GetComponent<Door>();
                if (door == null) continue;
                NavMeshDoorLink link = door.GetComponent<NavMeshDoorLink>();
                if (link == null) continue;
                AllDoors.Add(new DoorData(link));
                Logger.LogInfo("got door");
            }
        }

        private bool isDoorOpenable(Door door)
        {
            if (!door.enabled ||
                !door.gameObject.activeInHierarchy ||
                !door.Operatable) {
                return false;
            }
            if (GlobalSettingsClass.Instance.General.Doors.DisableAllDoors &&
                GameWorldComponent.Instance.Doors.DisableDoor(door)) {
                return false;
            }
            return true;
        }
    }
}