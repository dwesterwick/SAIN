using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class VisionCoverFromEnemy : EnemyBase, IBotEnemyClass
    {
        public bool HasCover => Time.time - _lastHasCoverTime < HAS_COVER_PERIOD;

        private float HAS_COVER_PERIOD = 0.33f;
        private float MAX_CHECK_COVER_RANGE = 100f;
        private float MIN_RATIO_FOR_COVER = 0.4f;
        private float _checkLimbsTime;
        private RaycastBatchJob _limbRaycasts = new RaycastBatchJob(LayerMaskClass.HighPolyWithTerrainMask, new ListCache<RaycastObject>("Raycasts"));
        private readonly List<Vector3> _limbPoints = new List<Vector3>();
        private float _lastHasCoverTime;

        public VisionCoverFromEnemy(Enemy enemy) : base(enemy)
        {
        }

        public void Init()
        {
            //Enemy.Events.OnEnemyKnownChanged.OnToggle += OnEnemyKnownChanged;
        }

        public void Update()
        {
            checkCoverFromEnemy();
        }

        public void Dispose()
        {
            //Enemy.Events.OnEnemyKnownChanged.OnToggle -= OnEnemyKnownChanged;
            _limbRaycasts.Dispose();
        }

        public void OnEnemyKnownChanged(bool known, Enemy enemy)
        {
        }

        private void checkCoverFromEnemy()
        {
            if (!Enemy.EnemyKnown) {
                return;
            }
            if (_checkLimbsTime < Time.time) {
                _checkLimbsTime = Time.time + 0.2f;
                if (Enemy.RealDistance > MAX_CHECK_COVER_RANGE) {
                    //return;
                }

                switch (_limbRaycasts.Status) {
                    case EJobStatus.Ready:
                        break;

                    case EJobStatus.Complete:
                        readResults();
                        break;

                    default:
                        return;
                }
                scheduleRaycasts();
            }
        }

        private void readResults()
        {
            var raycasts = _limbRaycasts.Datas;
            int total = raycasts.Count;
            int blockedSightCount = 0;
            foreach (var raycast in raycasts) {
                if (raycast.Hit.collider != null) {
                    blockedSightCount++;
                }
            }
            float ratio = (float)blockedSightCount / (float)total;
            if (ratio >= MIN_RATIO_FOR_COVER) {
                _lastHasCoverTime = Time.time;
            }
        }

        private void scheduleRaycasts()
        {
            Vector3 origin = EnemyTransform.WeaponFirePort;
            var myParts = PlayerComponent.BodyParts.PartsArray;
            _limbPoints.Clear();
            for (int i = 0; i < myParts.Length; i++) {
                _limbPoints.Add(myParts[i].GetRaycastToRandomPart(origin, float.MaxValue).CastPoint);
            }
            _limbRaycasts.ScheduleRaycastToPoints(_limbPoints.ToArray(), origin);
        }
    }
}