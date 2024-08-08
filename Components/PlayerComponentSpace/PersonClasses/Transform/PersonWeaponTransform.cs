using EFT;
using HarmonyLib;
using SAIN.Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace.PersonClasses
{
    public class PersonWeaponTransform : PersonSubClass
    {
        public Vector3 FirePort { get; private set; }
        public Vector3 PointDirection { get; private set; }
        public Vector3 Root { get; private set; }
        public bool WeaponAimBlocked { get; private set; }
        public bool WeaponPointBlocked { get; private set; }

        public Player.FirearmController FirearmController {
            get
            {
                if (_fireArmController == null) {
                    _fireArmController = (Player.HandsController as Player.FirearmController);
                }
                return _fireArmController;
            }
        }

        private Player.FirearmController _fireArmController;
        private readonly BifacialTransform _weaponRootTransform;

        public void Update()
        {
            Root = _weaponRootTransform.position;
            getWeaponTransforms();
        }

        private void getWeaponTransforms()
        {
            var controller = FirearmController;
            if (controller != null) {
                WeaponAimBlocked = checkAimBlocked(controller);
                WeaponPointBlocked = checkOverlapAmount(controller);

                var currentFirePort = controller.CurrentFireport;
                if (currentFirePort != null) {
                    Vector3 firePort = currentFirePort.position;
                    Vector3 pointDir = currentFirePort.Original.TransformDirection(Player.LocalShotDirection);
                    controller.AdjustShotVectors(ref firePort, ref pointDir);
                    FirePort = firePort;
                    PointDirection = pointDir;
                    return;
                }
            }

            // we failed to get fireport info, set the positions to a fallback
            FirePort = Root;
            PointDirection = Player.LookDirection;
        }

        private bool checkAimBlocked(Player.FirearmController controller)
        {
            return controller.IsOverlap && (bool)_overlapField.GetValue(controller);
        }

        private bool checkOverlapAmount(Player.FirearmController controller)
        {
            return controller.IsOverlap && controller.OverlapValue > 0.4f;
        }

        public PersonWeaponTransform(PersonClass person, PlayerData playerData) : base(person, playerData)
        {
            _weaponRootTransform = playerData.Player.WeaponRoot;
        }

        static PersonWeaponTransform()
        {
            _overlapField = AccessTools.Field(typeof(Player.FirearmController), "AimingInterruptedByOverlap");
        }

        private static FieldInfo _overlapField;
    }
}