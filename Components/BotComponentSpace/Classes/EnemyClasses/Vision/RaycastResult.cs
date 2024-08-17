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
        public bool InSight { get; private set; }
        public bool CheckedRecently { get; private set; }
        public float TimeSinceCheck { get; private set; }
        public float TimeSinceSuccess { get; private set; }

        private readonly float SIGHT_PERIOD_SEC = 0.25f;
        private readonly float CHECKED_PERIOD_SEC = 2f;

        public void UpdateTimeSince()
        {
            TimeSinceCheck = Time.time - LastCheckTime;
            TimeSinceSuccess = Time.time - LastSuccessTime;
            InSight = TimeSinceSuccess <= SIGHT_PERIOD_SEC;
            CheckedRecently = TimeSinceCheck <= CHECKED_PERIOD_SEC;
        }

        public RaycastResultData(float sightPeriodSec)
        {
            SIGHT_PERIOD_SEC = sightPeriodSec;
        }
    }

    public class RaycastResult
    {
        public RaycastResultData ResultData { get; private set; }
        public RaycastResultPointData PointData { get; private set; }

        public RaycastResult(float sightPeriodSec)
        {
            ResultData = new RaycastResultData(sightPeriodSec);
            PointData = new RaycastResultPointData();
        }

        public RaycastResultData Update()
        {
            return updateProperties();
        }

        private RaycastResultData updateProperties()
        {
            ResultData.UpdateTimeSince();
            return ResultData;
        }

        public void UpdateRaycastHit(Vector3 castPoint, BodyPartCollider bodyPartCollider, RaycastHit raycastHit, float time)
        {
            var pointData = PointData;
            ResultData.LastCheckTime = time;
            pointData.LastRaycastHit = raycastHit;
            if (raycastHit.collider == null) {
                pointData.LastSuccessBodyPart = bodyPartCollider;
                pointData.LastSuccessPoint = castPoint;
                pointData.LastSuccessOffset = castPoint - bodyPartCollider.transform.position;
                ResultData.LastSuccessTime = time;
            }
            else {
                pointData.LastSuccessBodyPart = null;
                pointData.LastSuccessPoint = null;
            }
            updateProperties();
        }
    }
}