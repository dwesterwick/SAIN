using EFT;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Components
{
    public class BodyPart
    {
        public readonly EBodyPart Type;
        public readonly BifacialTransform Transform;
        public readonly List<BodyPartCollider> Colliders;
        private readonly int _indexMax;

        public BodyPartRaycast GetRaycastToRandomPart(Vector3 origin, float maxRange)
        {
            int index = UnityEngine.Random.Range(0, _indexMax);
            BodyPartCollider collider = GetCollider(ref index);
            return new BodyPartRaycast {
                CastPoint = GetCastPoint(origin, collider),
                PartType = Type,
                ColliderType = collider.BodyPartColliderType
            };
        }

        public BodyPartCollider GetCollider(ref int index)
        {
            if (index > _indexMax) {
                index = 0;
            }
            BodyPartCollider collider = Colliders[index];
            index++;
            return collider;
        }

        public Vector3 GetCastPoint(Vector3 origin, BodyPartCollider collider)
        {
            float size = getColliderMinSize(collider);
            Vector3 random = UnityEngine.Random.insideUnitSphere * size;
            Vector3 result = collider.Collider.ClosestPoint(collider.transform.position + random);
            return result;
        }

        private static float getColliderMinSize(BodyPartCollider collider)
        {
            if (collider.Collider == null) {
                return 0f;
            }
            Vector3 bounds = collider.Collider.bounds.size;
            float lowest = bounds.x;
            if (bounds.y < lowest) {
                lowest = bounds.y;
            }
            if (bounds.z < lowest) {
                lowest = bounds.z;
            }
            return lowest;
        }

        public BodyPart(EBodyPart bodyPart, BifacialTransform transform, List<BodyPartCollider> colliders)
        {
            Type = bodyPart;
            Transform = transform;
            Colliders = colliders;
            _indexMax = colliders.Count - 1;
        }
    }
}