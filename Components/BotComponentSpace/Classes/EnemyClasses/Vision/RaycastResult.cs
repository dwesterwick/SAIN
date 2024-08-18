using UnityEngine;
using UnityEngine.UI;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public struct PartRaycastResultData
    {
        public RaycastResultData LineOfSight;
        public RaycastResultData CanShoot;
        public RaycastResultData IsVisible;
    }

    public class RaycastResultPointData
    {
        public RaycastHit LastRaycastHit;
        public BodyPartCollider LastSuccessBodyPart;
        public Vector3? LastSuccessPoint;
        public Vector3? LastSuccessOffset;
    }

    public class RaycastResultData
    {
        public float LastCheckTime;
        public float LastSuccessTime;

        public bool InSight => TimeSinceSuccess <= SIGHT_PERIOD_SEC;
        public bool CheckedRecently => TimeSinceCheck <= CHECKED_PERIOD_SEC;
        public float TimeSinceCheck => Time.time - LastCheckTime;
        public float TimeSinceSuccess => Time.time - LastSuccessTime;

        private readonly float SIGHT_PERIOD_SEC = 0.5f;
        private readonly float CHECKED_PERIOD_SEC = 2f;

        public RaycastResultData(float sightPeriodSec)
        {
            SIGHT_PERIOD_SEC = sightPeriodSec;
        }
    }

    public class RaycastResult
    {
        public RaycastResultData ResultData { get; }
        public RaycastResultPointData PointData { get; }

        public int BodyPartIndex;
        private float _nextLogTime;

        public RaycastResult(float sightPeriodSec)
        {
            ResultData = new RaycastResultData(sightPeriodSec);
            PointData = new RaycastResultPointData();
        }

        public void UpdateRaycastHit(Vector3 castPoint, BodyPartCollider bodyPartCollider, RaycastHit raycastHit, float time)
        {
            var pointData = PointData;
            ResultData.LastCheckTime = time;
            pointData.LastRaycastHit = raycastHit;
            bool lineOfSight = raycastHit.collider == null;
            if (lineOfSight) {
                pointData.LastSuccessBodyPart = bodyPartCollider;
                pointData.LastSuccessPoint = castPoint;
                pointData.LastSuccessOffset = castPoint - bodyPartCollider.transform.position;
                ResultData.LastSuccessTime = time;
            }
            else {
                pointData.LastSuccessBodyPart = null;
            }
            if (_nextLogTime < time) {
                _nextLogTime = time + 1f;
                Logger.LogDebug($"LOS? [{lineOfSight}] : Last Success [{ResultData.LastSuccessTime}] TimeSince: [{Time.time - ResultData.LastSuccessTime}]");
            }
        }
    }
}