using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyBodyPart
    {
        public float TimeSeen { get; private set; }
        public bool IsVisible => RaycastResults[ERaycastCheck.Vision].ResultData.InSight;
        public bool LineOfSight => RaycastResults[ERaycastCheck.LineofSight].ResultData.InSight;
        public bool CanShoot => RaycastResults[ERaycastCheck.Shoot].ResultData.InSight;

        public readonly EBodyPart BodyPart;
        public readonly List<BodyPartCollider> Colliders;
        public readonly BifacialTransform Transform;
        public readonly Dictionary<ERaycastCheck, RaycastResult> RaycastResults = new Dictionary<ERaycastCheck, RaycastResult>();

        private BodyPart _playerBodyPart;
        private readonly Dictionary<EBodyPartColliderType, BodyPartCollider> _colliderDictionary = new Dictionary<EBodyPartColliderType, BodyPartCollider>();

        public void UpdateProperties(bool canBeSeen)
        {
            if (!IsVisible) {
                TimeSeen = 0f;
            }
            else if (TimeSeen <= 0f) {
                TimeSeen = Time.time;
            }
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

            RaycastResults.Add(ERaycastCheck.LineofSight, new RaycastResult(0.5f));
            RaycastResults.Add(ERaycastCheck.Shoot, new RaycastResult(0.5f));
            RaycastResults.Add(ERaycastCheck.Vision, new RaycastResult(0.5f));
        }

        public void SetLineOfSight(Vector3 castPoint, EBodyPartColliderType colliderType, RaycastHit raycastHit, ERaycastCheck type, float time)
        {
            if (!_colliderDictionary.TryGetValue(colliderType, out BodyPartCollider bodyPartCollider)) {
                Logger.LogError($"[{colliderType}] not in collider Dictionary!");
                return;
            }
            if (!RaycastResults.TryGetValue(type, out RaycastResult raycastResult)) {
                Logger.LogError($"[{type}] not in Raycast Results Dictionary!");
                return;
            }
            if (type == ERaycastCheck.LineofSight && raycastHit.collider == null) {
                Logger.LogDebug($"SetLineOfSight hit collider null");
            }
            raycastResult.UpdateRaycastHit(castPoint, bodyPartCollider, raycastHit, time);
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
            RaycastResult results = RaycastResults[type];
            if (results.ResultData.InSight) {
                BodyPartCollider lastSuccessPart = results.PointData.LastSuccessBodyPart;
                if (lastSuccessPart != null) {
                    return lastSuccessPart;
                }
            }
            return _playerBodyPart.GetCollider(ref results.BodyPartIndex);
        }

        private Vector3 getCastPoint(Vector3 origin, BodyPartCollider collider)
        {
            return _playerBodyPart.GetCastPoint(origin, collider);
        }
    }
}