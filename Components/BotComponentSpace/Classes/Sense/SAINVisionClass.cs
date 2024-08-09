using SAIN.SAINComponent.Classes.Sense;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINBotNightVisionClass : BotBase, IBotClass
    {
        private float _nextUpdateTime;
        private float UPDATE_FREQ = 0.5f;
        private BotNightVisionData _botNightVisionData;

        public SAINBotNightVisionClass(BotComponent component) : base(component)
        {
            _botNightVisionData = component.BotOwner.NightVision;
        }

        public void Init()
        {
        }

        public void Update()
        {
            updateBotNightVision();
        }

        public void Dispose()
        {
        }

        private void updateBotNightVision()
        {
            if (!_botNightVisionData.HaveNightVision) {
                return;
            }
            if (_nextUpdateTime < Time.time) {
                _nextUpdateTime = Time.time + UPDATE_FREQ;
                // method_0 tells the bot whether they should toggle nightvision or not
                // will need to make sure this isn't too frequent as the default is 60 second wait between calling this
                // bots might rapidly toggle nvgs
                _botNightVisionData.method_0();
            }
        }
    }

    public class SAINVisionClass : BotBase, IBotClass
    {
        public float TimeLastCheckedLOS { get; set; }
        public float TimeSinceCheckedLOS => Time.time - TimeLastCheckedLOS;
        public FlashLightDazzleClass FlashLightDazzle { get; }
        public SAINBotLookClass BotLook { get; }
        public SAINBotNightVisionClass NightVision { get; }

        public SAINVisionClass(BotComponent component) : base(component)
        {
            FlashLightDazzle = new FlashLightDazzleClass(component);
            BotLook = new SAINBotLookClass(component);
            NightVision = new SAINBotNightVisionClass(component);
        }

        public void Init()
        {
            BotLook.Init();
            NightVision.Init();
        }

        public void Update()
        {
            FlashLightDazzle.CheckIfDazzleApplied(Bot.Enemy);
            NightVision.Update();
        }

        public void Dispose()
        {
            BotLook.Dispose();
            NightVision.Dispose();
        }
    }
}