﻿using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes;
using System.Collections;
using System.Text;

namespace SAIN.Layers
{
    public abstract class SAINAction : CustomLogic
    {
        public static string Name { get; private set; }

        public SAINAction(BotOwner botOwner, string name) : base(botOwner)
        {
            Name = name;
            Bot = botOwner.GetComponent<BotComponent>();
            Shoot = new ShootClass(botOwner);
        }

        public virtual IEnumerator ActionCoroutine()
        {
            while (Active)
            {
                yield return null;
            }
        }

        public readonly BotComponent Bot;

        public readonly ShootClass Shoot;

        private bool _actionActive;
        public bool Active => Bot?.BotActive == true && _actionActive;

        public void ToggleAction(bool value)
        {
            _actionActive = value;
            switch (value)
            {
                case true:
                    Bot.CoroutineManager.Add(ActionCoroutine());
                    BotOwner.PatrollingData?.Pause();
                    break;

                case false:
                    BotOwner.PatrollingData?.Unpause();
                    Bot.CoroutineManager.Remove(ActionCoroutine());
                    break;
            }
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            DebugOverlay.AddBaseInfo(Bot, BotOwner, stringBuilder);
        }
    }
}
