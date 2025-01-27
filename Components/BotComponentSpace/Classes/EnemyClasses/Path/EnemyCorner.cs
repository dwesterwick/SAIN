﻿using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyCorner
    {
        public EnemyCorner()
        {
        }

        public EnemyCorner(Vector3 groundPoint, float signedAngle, int pathIndex)
        {
            UpdateData(groundPoint, signedAngle, pathIndex);
        }

        public void UpdateData(Vector3 groundPoint, float signedAngle, int pathIndex)
        {
            GroundPosition = groundPoint;
            SignedAngleToTarget = signedAngle;
            PathIndex = pathIndex;
        }

        public int PathIndex { get; private set; }
        public Vector3 GroundPosition { get; private set; }
        public float SignedAngleToTarget { get; private set; }

        public Vector3 EyeLevelCorner(Vector3 eyePos, Vector3 botPosition)
        {
            return CornerHelpers.EyeLevelCorner(eyePos, botPosition, GroundPosition);
        }

        public Vector3 PointPastCorner(Vector3 eyePos, Vector3 botPosition)
        {
            return CornerHelpers.PointPastEyeLevelCorner(eyePos, botPosition, GroundPosition); ;
        }
    }
}