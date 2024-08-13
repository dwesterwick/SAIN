using EFT;
using SAIN.Components;
using SAIN.Components.BotComponentSpace.Classes.EnemyClasses;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
                    float dist = DistanceToEnemyRealPosition;
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
        private const float ENEMY_DIST_UPDATE_FREQ = 0.25f;

        public EnemyPlace(PlaceData placeData, Vector3 position, bool isDanger, EEnemyPlaceType placeType, SAINSoundType? soundType)
        {
            PlaceData = placeData;
            VisibleSourceOnLastUpdate = placeData.Enemy.InLineOfSight;
            IsDanger = isDanger;
            PlaceType = placeType;
            SoundType = soundType;

            _position = position;
            updateJobs(position);
            _timeLastUpdated = Time.time;
            initJobs();
        }

        public EnemyPlace(PlaceData placeData, HearingReport report)
        {
            PlaceData = placeData;
            VisibleSourceOnLastUpdate = placeData.Enemy.InLineOfSight;
            IsDanger = report.isDanger;
            PlaceType = report.placeType;
            SoundType = report.soundType;

            _position = report.position;
            updateJobs(report.position);
            _timeLastUpdated = Time.time;
            initJobs();
        }

        private void initJobs()
        {
            JobManager.Add(_raycast, EJobType.Raycast);
            JobManager.Add(_enemyDistance, EJobType.Distance);
            JobManager.Add(_botDistance, EJobType.Distance);
            _raycast.DistanceData = _botDistance;
            _raycast.OnCompleted += raycastComplete;
        }

        public void Update()
        {
            updateJobs(_position);
        }

        public void Dispose()
        {
            OnDispose?.Invoke(this);
            JobManager.Remove(_raycast, EJobType.Raycast);
            JobManager.Remove(_enemyDistance, EJobType.Distance);
            JobManager.Remove(_botDistance, EJobType.Distance);
        }

        public Vector3 GroundedPosition(float range = 2f)
        {
            Vector3 pos = _position;
            if (Physics.Raycast(pos, Vector3.down, out var hit, range, LayerMaskClass.HighPolyWithTerrainMask)) {
                return hit.point;
            }
            return pos + (Vector3.down * range);
        }

        public Vector3 Position {
            get
            {
                return _position;
            }
            set
            {
                checkNewValue(value, _position);
                _position = value;
                _timeLastUpdated = Time.time;
                VisibleSourceOnLastUpdate = PlaceData.Enemy.InLineOfSight;
                OnPositionUpdated?.Invoke(this);
            }
        }

        private void checkNewValue(Vector3 value, Vector3 oldValue)
        {
            if ((value - oldValue).sqrMagnitude > ENEMY_DIST_RECHECK_MIN_SQRMAG)
                updateJobs(value);
        }

        private const float ENEMY_DIST_RECHECK_MIN_SQRMAG = 0.25f;

        public float TimeSincePositionUpdated => Time.time - _timeLastUpdated;
        public float DistanceToBot => _botDistance.Distance;
        public float DistanceToEnemyRealPosition => _enemyDistance.Distance;

        private void updateJobs(Vector3 position)
        {
            switch (_enemyDistance.Status) {
                case EJobStatus.Ready:
                case EJobStatus.Complete:
                    _enemyDistance.UpdateData(PlaceData.Enemy.EnemyTransform.Position, position);
                    break;

                default:
                    break;
            }
            switch (_botDistance.Status) {
                case EJobStatus.Ready:
                case EJobStatus.Complete:
                    _botDistance.UpdateData(PlaceData.Owner.Position, position);
                    break;

                default:
                    break;
            }
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

        private readonly RaycastObject _raycast = new RaycastObject {
            LayerMask = LayerMaskClass.HighPolyWithTerrainMask,
        };

        private readonly DistanceObject _botDistance = new DistanceObject();
        private readonly DistanceObject _enemyDistance = new DistanceObject();
        public RaycastHit? BlockedHit { get; private set; }
        private bool _inSightNow;

        public bool InLineOfSight(Vector3 origin, LayerMask mask)
        {
            return _inSightNow;
        }
    }
}