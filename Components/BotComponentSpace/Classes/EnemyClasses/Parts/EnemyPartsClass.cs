using System.Collections.Generic;
using System.Linq;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyPartsClass : EnemyBase
    {
        public bool LineOfSight {
            get
            {
                foreach (var part in PartsArray) {
                    if (part.LineOfSight) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool CanShoot {
            get
            {
                foreach (var part in PartsArray) {
                    if (part.CanShoot) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsVisible {
            get
            {
                if (!Enemy.Vision.Angles.CanBeSeen) {
                    return false;
                }
                foreach (var part in PartsArray) {
                    if (part.IsVisible) {
                        return true;
                    }
                }
                return false;
            }
        }

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
            //LineOfSight = false;
            //CanShoot = false;
            //IsVisible = false;
            bool canBeSeen = Enemy.Vision.Angles.CanBeSeen;
            foreach (var part in PartsArray) {
                part.UpdateProperties(canBeSeen);
                //if (!Enemy.IsAI && part.LineOfSight)
                //    Logger.LogDebug($"{part.LineOfSight} : {canBeSeen}");
                //if (!LineOfSight && part.LineOfSight) {
                //    //LineOfSight = true;
                //}
                //if (!CanShoot && part.CanShoot) {
                //    CanShoot = true;
                //}
                //if (!IsVisible && part.IsVisible) {
                //    IsVisible = true;
                //}
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