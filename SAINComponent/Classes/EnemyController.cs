﻿using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class EnemyController : SAINBase, ISAINClass
    {
        public EnemyController(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            if (ClearEnemyTimer < Time.time)
            {
                ClearEnemyTimer = Time.time + 0.5f;
                ClearEnemies();
            }

            var goalEnemy = BotOwner.Memory.GoalEnemy;
            bool addEnemy = true;

            if (goalEnemy == null)
            {
                addEnemy = false;
            }
            else if (goalEnemy?.Person == null)
            {
                addEnemy = false;
            }
            else
            {
                if (goalEnemy.Person.IsAI && (goalEnemy.Person.AIData?.BotOwner == null || goalEnemy.Person.AIData.BotOwner.BotState != EBotState.Active))
                {
                    addEnemy = false;
                }
                if (goalEnemy.Person.IsAI && goalEnemy.Person.AIData.BotOwner.ProfileId == BotOwner.ProfileId)
                {
                    addEnemy = false;
                }
                if (!goalEnemy.Person.HealthController.IsAlive)
                {
                    addEnemy = false;
                }
            }

            if (addEnemy)
            {
                AddEnemy(goalEnemy.Person);
            }
            else
            {
                Enemy = null;
            }
        }

        public void Dispose()
        {
            Enemies.Clear();
        }

        public bool HasEnemy => Enemy != null && Enemy.Person != null && Enemy.EnemyPlayer != null && (!Enemy.Person.IsAI || Enemy.Person.AIData.BotOwner.BotState == EBotState.Active);

        public EnemyClass Enemy { get; private set; }

        public void ClearEnemy()
        {
            Enemy = null;
        }

        public void AddEnemy(IAIDetails person)
        {
            string id = person.ProfileId;

            // Check if the dictionary contains a previous SAINEnemy
            if (!Enemies.ContainsKey(id))
            {
                Enemies.Add(id, new EnemyClass(SAIN, person));
            }

            var newEnemy = Enemies[id];
            if (Enemy != null && Enemy != newEnemy)
            {
                Enemy?.EnemyVision?.LoseSight();
                Logger.LogWarning("LoseSight");
            }
            Enemy = newEnemy;
        }

        private float ClearEnemyTimer;

        private void ClearEnemies()
        {
            if (Enemies.Count > 0)
            {
                foreach (var keyPair in Enemies)
                {
                    string id = keyPair.Key;
                    EnemyClass enemy = keyPair.Value;
                    // Common checks between PMC and bots
                    if (enemy == null || enemy.EnemyPlayer == null || enemy.EnemyPlayer.HealthController?.IsAlive == false)
                    {
                        EnemyIDsToRemove.Add(id);
                    }
                    // Checks specific to bots
                    else if (enemy.EnemyPlayer.IsAI && (
                        enemy.EnemyPlayer.AIData?.BotOwner == null ||
                        enemy.EnemyPlayer.AIData.BotOwner.ProfileId == BotOwner.ProfileId ||
                        enemy.EnemyPlayer.AIData.BotOwner.BotState != EBotState.Active))
                    {
                        EnemyIDsToRemove.Add(id);
                    }
                }

                foreach (string idToRemove in EnemyIDsToRemove)
                {
                    Enemies.Remove(idToRemove);
                }

                EnemyIDsToRemove.Clear();
            }
        }

        public Dictionary<string, EnemyClass> Enemies { get; private set; } = new Dictionary<string, EnemyClass>();
        public List<Player> VisiblePlayers = new List<Player>();
        public List<string> VisiblePlayerIds = new List<string>();
        private readonly List<string> EnemyIDsToRemove = new List<string>();
    }
}
