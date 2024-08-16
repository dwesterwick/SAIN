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
        public bool LineOfSight => TimeSinceInLineOfSight < LINEOFSIGHT_TIME;
        public float TimeSinceInLineOfSight => Time.time - _timeLastInSight;
        public Vector3 LastSuccessLookPosition { get; private set; }
        public bool CanShoot => TimeSinceCanShoot < CANSHOOT_TIME;
        public float TimeSinceCanShoot => Time.time - _timeLastCanShoot;
        public Vector3 LastSuccessShootPosition { get; private set; }
        public Dictionary<EBodyPart, EnemyPartDataClass> Parts { get; } = new Dictionary<EBodyPart, EnemyPartDataClass>();
        public EnemyPartDataClass[] PartsArray { get; private set; }

        private float _timeLastInSight;
        private float _timeLastCanShoot;
        private int _index;
        private readonly int _indexMax;

        private const float LINEOFSIGHT_TIME = 0.25f;
        private const float CANSHOOT_TIME = 0.25f;

        public EnemyPartsClass(Enemy enemy) : base(enemy)
        {
            createPartDatas(enemy.Player.PlayerBones);
            PartsArray = Parts.Values.ToArray();
            _indexMax = Parts.Count;
        }

        public void Update()
        {
            updateParts();
        }

        private void updateParts()
        {
            foreach (var part in Parts.Values) {
                part.Update(Enemy);
                float lastCanShootTime = part.RaycastResults[ERaycastCheck.Shoot].LastSuccessTime;
                if (lastCanShootTime > _timeLastCanShoot) {
                    _timeLastCanShoot = lastCanShootTime;
                }

                float lastCanSeeTime = part.RaycastResults[ERaycastCheck.LineofSight].LastSuccessTime;
                if (lastCanSeeTime > _timeLastInSight) {
                    _timeLastInSight = lastCanSeeTime;
                }
            }
        }

        public EnemyPartDataClass GetNextPart()
        {
            EnemyPartDataClass result = null;
            EBodyPart epart = (EBodyPart)_index;
            if (!Parts.TryGetValue(epart, out result)) {
                _index = 0;
                result = Parts[EBodyPart.Chest];
            }

            _index++;
            if (_index > _indexMax) {
                _index = 0;
            }

            if (result == null) {
                result = Parts.PickRandom().Value;
            }
            return result;
        }

        private void createPartDatas(PlayerBones bones)
        {
            var parts = Enemy.EnemyPlayerComponent.BodyParts.Parts;
            foreach (var bodyPart in parts) {
                Parts.Add(bodyPart.Key, new EnemyPartDataClass(bodyPart.Value, bodyPart.Key, bodyPart.Value.Transform, bodyPart.Value.Colliders));
            }
        }
    }
}