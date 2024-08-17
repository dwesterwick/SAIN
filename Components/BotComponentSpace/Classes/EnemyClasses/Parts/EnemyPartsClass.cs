using EFT;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyPartsClass : EnemyBase
    {
        public bool LineOfSight { get; private set; }
        public bool CanShoot { get; private set; }
        public bool IsVisible { get; private set; }
        public Dictionary<EBodyPart, EnemyBodyPart> Parts { get; } = new Dictionary<EBodyPart, EnemyBodyPart>();
        public EnemyBodyPart[] PartsArray { get; }

        public EnemyPartsClass(Enemy enemy) : base(enemy)
        {
            createPartDatas(enemy.Player.PlayerBones);
            PartsArray = Parts.Values.ToArray();
        }

        public void Update()
        {
            updateStatus();
        }

        private void updateStatus()
        {
            LineOfSight = false;
            CanShoot = false;
            IsVisible = false;
            bool canBeSeen = Enemy.Vision.Angles.CanBeSeen;
            foreach (var part in Parts.Values) {
                PartRaycastResultData results = part.UpdateProperties(canBeSeen);
                if (!LineOfSight) {
                    LineOfSight = results.LineOfSight.InSight;
                }
                if (!CanShoot) {
                    CanShoot = results.CanShoot.InSight;
                }
                if (!IsVisible) {
                    IsVisible = results.IsVisible.InSight;
                }
            }
        }

        private void createPartDatas(PlayerBones bones)
        {
            var parts = Enemy.EnemyPlayerComponent.BodyParts.Parts;
            foreach (var bodyPart in parts) {
                Parts.Add(bodyPart.Key, new EnemyBodyPart(bodyPart.Value, bodyPart.Key, bodyPart.Value.Transform, bodyPart.Value.Colliders));
            }
        }
    }
}