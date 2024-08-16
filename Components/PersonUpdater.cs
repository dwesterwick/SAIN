using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace
{
    public class PersonUpdater : MonoBehaviour
    {
        private PlayerDictionary _alivePlayers;

        private void Awake()
        {
        }

        private void Start()
        {
            _alivePlayers = GetComponent<GameWorldComponent>().PlayerTracker.AlivePlayers;
        }

        private void Update()
        {
            foreach (var player in _alivePlayers.Values) {
                player.Person.Update();
            }
        }

        private void LateUpdate()
        {
            foreach (var player in _alivePlayers.Values) {
                player.Person.LateUpdate();
            }
        }
    }
}