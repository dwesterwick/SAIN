using SAIN.Components;
using SAIN.Components.BotComponentSpace.Classes.EnemyClasses;
using SAIN.Helpers;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public struct PlaceData
    {
        public Enemy Enemy;
        public bool IsAI;
        public BotComponent Owner;
        public string OwnerID;
    }

    public enum EEnemyPlaceType
    {
        Vision,
        Hearing,
        Flashlight,
        Injury,
    }

    public class EnemyPlace
    {
        public event Action<EnemyPlace> OnPositionUpdated;

        public event Action<EnemyPlace> OnDispose;

        public PlaceData PlaceData { get; }
        public EEnemyPlaceType PlaceType { get; }
        public SAINSoundType? SoundType { get; set; }

        public bool VisibleSourceOnLastUpdate { get; private set; }
        public bool IsDanger { get; set; }

        public bool ShallClear {
            get
            {
                var person = PlaceData.Enemy?.EnemyPerson;
                if (person == null) {
                    return true;
                }
                var activeClass = person.ActivationClass;
                if (!activeClass.Active || !activeClass.IsAlive) {
                    return true;
                }
                if (playerLeftArea) {
                    return true;
                }
                return false;
            }
        }

        private bool playerLeftArea {
            get
            {
                if (_nextCheckLeaveTime < Time.time) {
                    _nextCheckLeaveTime = Time.time + ENEMY_DIST_TO_PLACE_CHECK_FREQ;
                    // If the person this place was created for is AI and left the area, just forget it and move on.
                    float dist = DistanceToEnemy;
                    if (PlaceData.IsAI) {
                        return dist > ENEMY_DIST_TO_PLACE_FOR_LEAVE_AI;
                    }
                    return dist > ENEMY_DIST_TO_PLACE_FOR_LEAVE;
                }
                return false;
            }
        }

        private const float ENEMY_DIST_TO_PLACE_CHECK_FREQ = 10;
        private const float ENEMY_DIST_TO_PLACE_FOR_LEAVE = 150;
        private const float ENEMY_DIST_TO_PLACE_FOR_LEAVE_AI = 100f;

        public Vector3 Position {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                CalcDistances(value);
                _timeLastUpdated = Time.time;
                VisibleSourceOnLastUpdate = PlaceData.Enemy.InLineOfSight;
                OnPositionUpdated?.Invoke(this);
            }
        }

        public float TimeSincePositionUpdated => Time.time - _timeLastUpdated;
        public float DistanceToBot { get; private set; }
        public float DistanceToEnemy { get; private set; }

        public bool HasArrivedPersonal {
            get
            {
                return _hasArrivedPers;
            }
            set
            {
                if (value) {
                    _timeArrivedPers = Time.time;
                    HasSeenPersonal = true;
                }
                _hasArrivedPers = value;
            }
        }

        public bool HasArrivedSquad {
            get
            {
                return _hasArrivedSquad;
            }
            set
            {
                if (value) {
                    _timeArrivedSquad = Time.time;
                }
                _hasArrivedSquad = value;
            }
        }

        public bool HasSeenPersonal {
            get
            {
                return _hasSeenPers;
            }
            set
            {
                if (value) {
                    _timeSeenPers = Time.time;
                }
                _hasSeenPers = value;
            }
        }

        public bool HasSeenSquad {
            get
            {
                return _hasSquadSeen;
            }
            set
            {
                if (value) {
                    _timeSquadSeen = Time.time;
                }
                _hasSquadSeen = value;
            }
        }

        private Vector3 _position;
        private float _nextCheckLeaveTime;
        public float _timeLastUpdated;
        private bool _hasArrivedPers;
        public float _timeArrivedPers;
        private bool _hasArrivedSquad;
        public float _timeArrivedSquad;
        private bool _hasSeenPers;
        public float _timeSeenPers;
        private bool _hasSquadSeen;
        public float _timeSquadSeen;

        private readonly RaycastObject _raycast = new RaycastObject();

        public RaycastHit? BlockedHit { get; private set; }
        private bool _inSightNow;

        public void CalcDistances()
        {
            CalcDistances(Position);
        }

        public void CalcDistances(Vector3 position)
        {
            DistanceToBot = Vector.Distance(position, PlaceData.Owner.Position);
            DistanceToEnemy = Vector.Distance(position, PlaceData.Enemy.EnemyPosition);

            Vector3 eyePos = PlaceData.Owner.Transform.EyePosition;
            Vector3 direction = (position + Vector3.up) - eyePos;
            DistanceData distanceData = new DistanceData {
                Origin = eyePos,
                Direction = direction,
                Distance = Vector.Distance(direction),
            };
            _raycast.Create(distanceData, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public EnemyPlace(PlaceData placeData, Vector3 position, bool isDanger, EEnemyPlaceType placeType, SAINSoundType? soundType)
        {
            PlaceData = placeData;
            VisibleSourceOnLastUpdate = placeData.Enemy.InLineOfSight;
            IsDanger = isDanger;
            PlaceType = placeType;
            SoundType = soundType;

            Position = position;
            initJobs();
        }

        public EnemyPlace(PlaceData placeData, HearingReport report)
        {
            PlaceData = placeData;
            VisibleSourceOnLastUpdate = placeData.Enemy.InLineOfSight;
            IsDanger = report.isDanger;
            PlaceType = report.placeType;
            SoundType = report.soundType;
            Position = report.position;
            initJobs();
        }

        private void initJobs()
        {
            JobManager.Instance.Add(_raycast, EJobType.Raycast);
            _raycast.OnCompleted += raycastComplete;
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            OnDispose?.Invoke(this);
            _raycast.OnCompleted -= raycastComplete;
            JobManager.Instance.Remove(_raycast, EJobType.Raycast);
        }

        public Vector3 GroundedPosition(float range = 2f)
        {
            Vector3 pos = _position;
            if (Physics.Raycast(pos, Vector3.down, out var hit, range, LayerMaskClass.HighPolyWithTerrainMask)) {
                return hit.point;
            }
            return pos + (Vector3.down * range);
        }

        private void raycastComplete(AbstractJobObject _)
        {
            RaycastHit hit = _raycast.Hit;
            _inSightNow = hit.collider == null;
            if (_inSightNow) {
                BlockedHit = null;
            }
            else {
                BlockedHit = hit;
            }
        }

        public bool InLineOfSight(Vector3 origin, LayerMask mask)
        {
            return _inSightNow;
        }
    }
}