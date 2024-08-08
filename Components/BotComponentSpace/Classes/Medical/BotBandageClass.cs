using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;

namespace SAIN.SAINComponent.Classes
{
    public class BotBandageClass : BotBase, IBotClass
    {
        public BotBandageClass(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            RefreshMeds();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void RefreshMeds()
        {
            Player getPlayer = Player;
            EquipmentSlot[] equipmentSlots = _slots;
            this._medsList.Clear();
            _bandages.Clear();
            getPlayer.InventoryControllerClass.GetAcceptableItemsNonAlloc<MedsClass>(equipmentSlots, this._medsList, null);
            foreach (var med in _medsList) {
                if (med.TryGetItemComponent(out HealthEffectsComponent healthEffectsComponent)) {
                    var damageEffects = healthEffectsComponent.DamageEffects;
                    if (!damageEffects.ContainsKey(EDamageEffectType.HeavyBleeding) &&
                        !damageEffects.ContainsKey(EDamageEffectType.LightBleeding)) {
                        continue;
                    }

                    // Need to check this is what it means for something to heal, aka an ifak or salewa
                    if (healthEffectsComponent.HealthEffects.ContainsKey(EFT.HealthSystem.EHealthFactorType.Health)) {
                        Logger.LogInfo($"{med.Name} has healthEffect and stop bleeds");
                        continue;
                    }
                    _bandages.Add(med);
                    continue;
                }
            }
        }

        public bool WantToOnlyBandage()
        {
            return _bandages.Count > 0;
        }

        public bool IsAlreadyBandage(MedsClass med)
        {
            if (med == null) return false;
            return _bandages.Contains(med);
        }

        public MedsClass GetBandage()
        {
            if (_bandages.Count == 0) {
                return null;
            }
            return _bandages[0];
        }

        public static readonly EquipmentSlot[] _slots = new EquipmentSlot[]
        {
        EquipmentSlot.Pockets,
        EquipmentSlot.TacticalVest,
        };

        private readonly List<MedsClass> _bandages = new List<MedsClass>();

        public bool hasEffectOfType(MedsClass med, EDamageEffectType damageEffectType)
        {
            HealthEffectsComponent healthEffectsComponent;
            return med != null && (med.TryGetItemComponent<HealthEffectsComponent>(out healthEffectsComponent) && healthEffectsComponent.DamageEffects.ContainsKey(damageEffectType));
        }

        private readonly List<MedsClass> _medsList = new List<MedsClass>();
    }
}