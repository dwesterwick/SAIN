using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyPartDataClass
    {
        public float TimeSeen { get; private set; }
        public bool IsVisible { get; private set; }
        public bool LineOfSight => RaycastResults[ERaycastCheck.LineofSight].InSight;
        public bool CanShoot => RaycastResults[ERaycastCheck.Shoot].InSight;
        public float TimeSinceLastVisionCheck => RaycastResults[ERaycastCheck.LineofSight].TimeSinceChecked;
        public float TimeSinceLastVisionSuccess => RaycastResults[ERaycastCheck.LineofSight].TimeSinceSuccess;

        public readonly Dictionary<ERaycastCheck, RaycastResult> RaycastResults = new Dictionary<ERaycastCheck, RaycastResult>();
        private readonly Dictionary<EBodyPartColliderType, BodyPartCollider> _colliderDictionary = new Dictionary<EBodyPartColliderType, BodyPartCollider>();

        public readonly EBodyPart BodyPart;
        public readonly List<BodyPartCollider> Colliders;
        public readonly BifacialTransform Transform;

        private BodyPart _playerBodyPart;
        private int _index;
        private BodyPartCollider _lastSuccessPart;

        public void Update(Enemy enemy)
        {
            IsVisible =
                enemy.Vision.Angles.CanBeSeen &&
                RaycastResults[ERaycastCheck.Vision].InSight &&
                RaycastResults[ERaycastCheck.LineofSight].InSight;

            if (!IsVisible) {
                TimeSeen = 0f;
                return;
            }
            if (TimeSeen <= 0f) {
                TimeSeen = Time.time;
            }
        }

        public EnemyPartDataClass(BodyPart part, EBodyPart bodyPart, BifacialTransform transform, List<BodyPartCollider> colliders)
        {
            _playerBodyPart = part;
            BodyPart = bodyPart;
            Transform = transform;
            Colliders = colliders;

            foreach (BodyPartCollider collider in colliders) {
                if (!_colliderDictionary.ContainsKey(collider.BodyPartColliderType)) {
                    _colliderDictionary.Add(collider.BodyPartColliderType, collider);
                }
            }

            RaycastResults.Add(ERaycastCheck.LineofSight, new RaycastResult());
            RaycastResults.Add(ERaycastCheck.Shoot, new RaycastResult());
            RaycastResults.Add(ERaycastCheck.Vision, new RaycastResult());
        }

        public void SetLineOfSight(Vector3 castPoint, EBodyPartColliderType colliderType, RaycastHit raycastHit, ERaycastCheck type, float time)
        {
            BodyPartCollider collider = _colliderDictionary[colliderType];
            RaycastResults[type].Update(castPoint, collider, raycastHit, time);
            if (raycastHit.collider == null) {
                _lastSuccessPart = collider;
            }
        }

        public BodyPartRaycast GetRaycast(Vector3 origin, float maxRange)
        {
            BodyPartCollider collider = getCollider();
            Vector3 castPoint = getCastPoint(origin, collider);

            return new BodyPartRaycast {
                CastPoint = castPoint,
                PartType = BodyPart,
                ColliderType = collider.BodyPartColliderType
            };
        }

        private BodyPartCollider getCollider()
        {
            if (LineOfSight && _lastSuccessPart != null) {
                return _lastSuccessPart;
            }
            else {
                _lastSuccessPart = null;
            }
            return _playerBodyPart.GetCollider(ref _index);
        }

        private Vector3 getCastPoint(Vector3 origin, BodyPartCollider collider)
        {
            return _playerBodyPart.GetCastPoint(origin, collider);
        }
    }
}