using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyUpdater : BotBase, IBotClass
    {
        private Dictionary<string, Enemy> _enemies;
        private readonly List<string> _allyIdsToRemove = new List<string>();
        private readonly List<string> _invalidIdsToRemove = new List<string>();

        public EnemyUpdater(BotComponent bot) : base(bot)
        {
        }

        public void Init()
        {
            _enemies = Bot.EnemyController.Enemies;
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void CheckAllEnemies()
        {
            foreach (var kvp in _enemies) {
                string profileId = kvp.Key;
                Enemy enemy = kvp.Value;
                if (!isEnemyValid(profileId, enemy)) continue;
                if (isEnemyAlly(profileId, enemy)) continue;
            }
            removeEnemies(_invalidIdsToRemove);
            removeEnemies(_allyIdsToRemove);
        }

        public void UpdateAllEnemies()
        {
            foreach (Enemy enemy in _enemies.Values) {
                enemy.Update();
            }
        }

        private bool isEnemyValid(string id, Enemy enemy)
        {
            if (enemy == null || enemy.CheckValid() == false) {
                _invalidIdsToRemove.Add(id);
                return false;
            }
            return true;
        }

        private bool isEnemyAlly(string id, Enemy enemy)
        {
            if (BotOwner.BotsGroup?.Allies.Contains(enemy.EnemyPlayer) == true) {
                if (SAINPlugin.DebugMode)
                    Logger.LogWarning($"{enemy.EnemyPlayer.name} is an ally of {Player.name} and will be removed from its enemies collection");

                _allyIdsToRemove.Add(id);
                return true;
            }

            return false;
        }

        private void removeEnemies(List<string> idList)
        {
            if (idList.Count > 0) {
                foreach (var id in idList) {
                    Bot.EnemyController.RemoveEnemy(id);
                }
                if (SAINPlugin.DebugMode)
                    Logger.LogWarning($"Removed {idList.Count}");
                idList.Clear();
            }
        }
    }
}