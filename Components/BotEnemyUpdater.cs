using SAIN.Components.BotController;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Components
{
    public class BotEnemyUpdater : MonoBehaviour
    {
        private BotDictionary _bots;

        private void Awake()
        {
        }

        private void Start()
        {
            _bots = GetComponent<SAINBotController>().Bots;
        }

        private void Update()
        {
            foreach (var bot in _bots.Values) {
                EnemyUpdater updater = bot.EnemyController.EnemyUpdater;
                updater.CheckAllEnemies();
                updater.UpdateAllEnemies();
            }
        }

        private void LateUpdate()
        {
            foreach (var bot in _bots.Values) {
                bot.EnemyController.EnemyUpdater.CheckAllEnemies();
            }
        }
    }
}