using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyUpdaterComponent : MonoBehaviour
    {
        private BotComponent _bot;
        private Dictionary<string, Enemy> Enemies => _bot.EnemyController.Enemies;
        private readonly List<string> _allyIdsToRemove = new List<string>();
        private readonly List<string> _invalidIdsToRemove = new List<string>();

        public void Init(BotComponent bot)
        {
            _bot = bot;
        }

        private void Update()
        {
            if (_bot == null || _bot.EnemyController == null || !_bot.BotActive) {
                return;
            }

            foreach (var kvp in Enemies) {
                string profileId = kvp.Key;
                Enemy enemy = kvp.Value;
                if (!isEnemyValid(profileId, enemy))
                    continue;

                if (isEnemyAlly(profileId, enemy))
                    continue;

                enemy.Update();
                //Logger.LogDebug("update");
                //enemy.Vision.VisionChecker.CheckVision(out _);
            }
            removeInvalid();
            removeAllies();
        }

        private void LateUpdate()
        {
            if (_bot == null || _bot.EnemyController == null || !_bot.BotActive) {
                return;
            }

            foreach (var kvp in Enemies) {
                isEnemyValid(kvp.Key, kvp.Value);
            }
            removeInvalid();
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
            if (_bot.BotOwner.BotsGroup.Allies.Contains(enemy.EnemyPlayer)) {
                if (SAINPlugin.DebugMode)
                    Logger.LogWarning($"{enemy.EnemyPlayer.name} is an ally of {_bot.Player.name} and will be removed from its enemies collection");

                _allyIdsToRemove.Add(id);
                return true;
            }

            return false;
        }

        private void removeInvalid()
        {
            if (_invalidIdsToRemove.Count > 0) {
                foreach (var id in _invalidIdsToRemove) {
                    _bot.EnemyController.RemoveEnemy(id);
                }
                Logger.LogWarning($"Removed {_invalidIdsToRemove.Count} Invalid Enemies");
                _invalidIdsToRemove.Clear();
            }
        }

        private void removeAllies()
        {
            if (_allyIdsToRemove.Count > 0) {
                foreach (var id in _allyIdsToRemove) {
                    _bot.EnemyController.RemoveEnemy(id);
                }

                if (SAINPlugin.DebugMode)
                    Logger.LogWarning($"Removed {_allyIdsToRemove.Count} allies");

                _allyIdsToRemove.Clear();
            }
        }
    }
}