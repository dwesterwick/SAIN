using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using static GClass739;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyBodyPart
    {
        public float TimeSeen { get; private set; }
        public bool IsVisible { get; private set; }
        public bool LineOfSight { get; private set; }
        public bool CanShoot { get; private set; }

        public readonly EBodyPart BodyPart;
        public readonly List<BodyPartCollider> Colliders;
        public readonly BifacialTransform Transform;
        public readonly Dictionary<ERaycastCheck, RaycastResult> RaycastResults = new Dictionary<ERaycastCheck, RaycastResult>();

        private BodyPart _playerBodyPart;
        private int _index;
        private readonly Dictionary<EBodyPartColliderType, BodyPartCollider> _colliderDictionary = new Dictionary<EBodyPartColliderType, BodyPartCollider>();

        public PartRaycastResultData UpdateProperties(bool canBeSeen)
        {
            var results = new PartRaycastResultData {
                LineOfSight = RaycastResults[ERaycastCheck.LineofSight].Update(),
                CanShoot = RaycastResults[ERaycastCheck.Vision].Update(),
                IsVisible = RaycastResults[ERaycastCheck.Shoot].Update(),
            };

            LineOfSight = results.LineOfSight.InSight;
            CanShoot = results.CanShoot.InSight;
            IsVisible =
                canBeSeen &&
                LineOfSight &&
                results.IsVisible.InSight;

            if (!IsVisible) {
                TimeSeen = 0f;
            }
            else if (TimeSeen <= 0f) {
                TimeSeen = Time.time;
            }
            return results;
        }

        public EnemyBodyPart(BodyPart part, EBodyPart bodyPart, BifacialTransform transform, List<BodyPartCollider> colliders)
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

            RaycastResults.Add(ERaycastCheck.LineofSight, new RaycastResult(0.25f));
            RaycastResults.Add(ERaycastCheck.Shoot, new RaycastResult(0.25f));
            RaycastResults.Add(ERaycastCheck.Vision, new RaycastResult(0.25f));
        }

        public void SetLineOfSight(Vector3 castPoint, EBodyPartColliderType colliderType, RaycastHit raycastHit, ERaycastCheck type, float time)
        {
            RaycastResults[type].UpdateRaycastHit(castPoint, _colliderDictionary[colliderType], raycastHit, time);
        }

        public BodyPartRaycast GetRaycast(Vector3 origin, float maxRange, ERaycastCheck type)
        {
            BodyPartCollider collider = getCollider(type);
            Vector3 castPoint = getCastPoint(origin, collider);
            return new BodyPartRaycast {
                CastPoint = castPoint,
                PartType = BodyPart,
                ColliderType = collider.BodyPartColliderType
            };
        }

        private BodyPartCollider getCollider(ERaycastCheck type)
        {
            var results = RaycastResults[type];
            if (results.ResultData.InSight) {
                BodyPartCollider lastSuccessPart = results.PointData.LastSuccessBodyPart;
                if (lastSuccessPart != null) {
                    return lastSuccessPart;
                }
            }
            return _playerBodyPart.GetCollider(ref _index);
        }

        private Vector3 getCastPoint(Vector3 origin, BodyPartCollider collider)
        {
            return _playerBodyPart.GetCastPoint(origin, collider);
        }
    }
}