using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Search;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAIN.Layers.Combat.Solo
{
    internal class SearchAction : CombatAction, ISAINAction
    {
        private Enemy _searchTarget => Search.TargetEnemy;
        private float _nextCheckWeaponTime;
        private float _nextUpdateSearchTime;
        private bool _haveTalked = false;
        private bool _sprintEnabled = false;
        private float _sprintTimer = 0f;
        private SAINSearchClass Search => Bot.Search;

        public override void Start()
        {
            Search.Start();
            Toggle(true);
        }

        public override void Stop()
        {
            Search.Stop();
            Toggle(false);
            BotOwner.Mover?.MovementResume();
            _haveTalked = false;
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update()
        {
            var enemy = Search.TargetEnemy;
            if (enemy == null) return;

            bool isBeingStealthy = enemy.Hearing.EnemyHeardFromPeace;
            if (isBeingStealthy) {
                _sprintEnabled = false;
            }
            else {
                checkShouldSprint(enemy);
                talk();
            }

            steer(enemy);

            if (_nextUpdateSearchTime < Time.time) {
                _nextUpdateSearchTime = Time.time + 0.1f;
                Search.Search(_sprintEnabled, enemy);
            }

            if (!_sprintEnabled) {
                Shoot.CheckAimAndFire();
                if (!isBeingStealthy)
                    checkWeapon();
            }
        }

        private void talk()
        {
            EnemyPlace target = Search.PathFinder.TargetPlace;
            if (target == null) {
                return;
            }

            // Scavs will speak out and be more vocal
            if (!_haveTalked &&
                Bot.Info.Profile.IsScav &&
                target.DistanceToBot < 50f) {
                _haveTalked = true;
                if (EFTMath.RandomBool(40)) {
                    Bot.Talk.Say(EPhraseTrigger.OnMutter, ETagStatus.Aware, true);
                }
            }
        }

        private void checkWeapon()
        {
            if (_nextCheckWeaponTime < Time.time) {
                _nextCheckWeaponTime = Time.time + 180f * Random.Range(0.5f, 1.5f);
                if (_searchTarget.TimeSinceLastKnownUpdated > 30f) {
                    if (EFTMath.RandomBool())
                        Bot.Player.HandsController.FirearmsAnimator.CheckAmmo();
                    else
                        Bot.Player.HandsController.FirearmsAnimator.CheckChamber();
                }
            }
        }

        private void checkShouldSprint(Enemy enemy)
        {
            //  || Search.CurrentState == ESearchMove.MoveToDangerPoint
            if (Search.CurrentState == ESearchMove.MoveToEndPeek || Search.CurrentState == ESearchMove.Wait) {
                _sprintEnabled = false;
                return;
            }

            //  || Bot.Enemy?.InLineOfSight == true
            if (enemy.IsVisible) {
                _sprintEnabled = false;
                return;
            }

            if (Bot.Decision.CurrentSquadDecision == ESquadDecision.Help) {
                _sprintEnabled = true;
                return;
            }

            var persSettings = Bot.Info.PersonalitySettings;
            float chance = persSettings.Search.SprintWhileSearchChance;
            if (_sprintTimer < Time.time && chance > 0) {
                float myPower = Bot.Info.Profile.PowerLevel;
                if (enemy.EnemyPlayer != null && enemy.EnemyPlayer.AIData.PowerOfEquipment < myPower * 0.5f) {
                    chance = 100f;
                }

                _sprintEnabled = EFTMath.RandomBool(chance);
                float timeAdd;
                if (_sprintEnabled) {
                    timeAdd = 4f * Random.Range(0.5f, 2.00f);
                }
                else {
                    timeAdd = 4f * Random.Range(0.5f, 1.5f);
                }
                _sprintTimer = Time.time + timeAdd;
            }
        }

        private void steer(Enemy enemy)
        {
            if (!Bot.Steering.SteerByPriority(enemy, false)) {
                Bot.Steering.LookToMovingDirection();
            }
        }

        public SearchAction(BotOwner bot) : base(bot, "Search")
        {
        }
    }
}