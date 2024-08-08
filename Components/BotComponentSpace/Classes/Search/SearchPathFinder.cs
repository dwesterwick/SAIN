using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Search
{
    public class SearchPathFinder : BotSubClass<SAINSearchClass>
    {
        public Vector3? FinalDestination => TargetPlace?.Position ?? RandomSearchPoint;
        public EnemyPlace TargetPlace { get; private set; }
        public BotPeekPlan? PeekPoints { get; private set; }
        public bool SearchedTargetPosition { get; private set; }
        public bool FinishedPeeking { get; set; }

        public SearchPathFinder(SAINSearchClass searchClass) : base(searchClass)
        {
        }

        public bool HasPathToSearchTarget(Enemy enemy, out string failReason)
        {
            //if (_nextCheckSearchTime < Time.time || _lastCheckedEnemy != enemy) {
            //    _nextCheckSearchTime = Time.time + 1f;
            //    Vector3? destination = CanPath(enemy, out failReason);
            //    _canStartSearch = destination != null;
            //    _failReason = failReason;
            //    _lastCheckedEnemy = enemy;
            //}
            return CanPath(enemy, out failReason) != null;
        }

        //private Enemy _lastCheckedEnemy;

        private bool checkBotZone(Vector3 target)
        {
            if (Bot.Memory.Location.BotZoneCollider != null) {
                Vector3 closestPointInZone = Bot.Memory.Location.BotZoneCollider.ClosestPointOnBounds(target);
                float distance = (target - closestPointInZone).sqrMagnitude;
                if (distance > 50f * 50f) {
                    return false;
                }
            }
            return true;
        }

        public void UpdateSearchDestination(Enemy enemy)
        {
            if (!SearchedTargetPosition && RandomSearchPoint != null) {
                RandomSearchPoint = null;
            }
            if (TargetPlace == null) {
                FindPath(enemy, out _);
                return;
            }
            checkFinishedSearch(enemy);
            if (SearchedTargetPosition) {
                RandomSearch();
                return;
            }
        }

        private void checkFinishedSearch(Enemy enemy)
        {
            var lastKnown = enemy.KnownPlaces.LastKnownPlace;
            if (lastKnown == null) {
                return;
            }
            if (TargetPlace != lastKnown) {
                FindPath(enemy, out _);
                return;
            }
            if (lastKnown.HasArrivedPersonal || lastKnown.HasArrivedSquad) {
                SearchedTargetPosition = true;
                return;
            }

            var pathToEnemy = enemy.Path.PathToEnemy;
            if (pathToEnemy.corners.Length > 2) {
                return;
            }

            if (!SearchedTargetPosition &&
                lastKnown.DistanceToBot < 0.5f) {
                SearchedTargetPosition = true;
                enemy.KnownPlaces.SetPlaceAsSearched(lastKnown);
                return;
            }

            //var lastCorner = pathToEnemy.LastCorner();
            //if (lastCorner == null) {
            //    Reset();
            //    return;
            //}
            //
            //if ((lastCorner.Value - FinalDestination).sqrMagnitude < 0.5f) {
            //    SearchedTargetPosition = true;
            //    enemy.KnownPlaces.SetPlaceAsSearched(lastKnown);
            //    Reset();
            //    return;
            //}
            //if (!FindPath(enemy, out string failReason)) {
            //    Logger.LogDebug($"Failed to calc path during search for reason: [{failReason}]");
            //    Reset();
            //    return;
            //}
        }

        private void RandomSearch()
        {
            if (RandomSearchPoint != null) {
                float dist = (RandomSearchPoint.Value - Bot.Position).sqrMagnitude;
                if (dist < ComeToRandomDist * ComeToRandomDist || dist > 60f * 60f) {
                    RandomSearchPoint = null;
                }
            }
            if (RandomSearchPoint == null) {
                RandomSearchPoint = GenerateSearchPoint();
            }
        }

        private Vector3? RandomSearchPoint;

        private Vector3 GenerateSearchPoint()
        {
            Vector3 start = Bot.Position;
            float dispersion = 30f;
            for (int i = 0; i < 10; i++) {
                float dispNum = EFTMath.Random(-dispersion, dispersion);
                Vector3 vector = new Vector3(start.x + dispNum, start.y, start.z + dispNum);
                if (NavMesh.SamplePosition(vector, out var hit, 10f, -1)) {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(hit.position, start, -1, path)) {
                        return path.corners[path.corners.Length - 1];
                    }
                }
            }
            return start;
        }

        public void Reset()
        {
            PeekPoints?.DisposeDebug();
            PeekPoints = null;
            TargetPlace = null;
            FinishedPeeking = false;
            SearchedTargetPosition = false;
            RandomSearchPoint = Vector3.zero;
        }

        public Vector3? CanPath(Enemy enemy, out string failReason)
        {
            var path = enemy.Path.PathToEnemy;
            if (path.status == NavMeshPathStatus.PathInvalid) {
                failReason = "pathInvalid";
                return null;
            }

            Vector3? lastCorner = path.LastCorner();
            if (lastCorner == null) {
                failReason = "lastCorner Null";
                return null;
            }

            if ((lastCorner.Value - Bot.Position).sqrMagnitude <= 0.25f) {
                failReason = "tooClose";
                return null;
            }
            failReason = string.Empty;
            return lastCorner.Value;
        }

        public bool FindPath(Enemy enemy, out string failReason)
        {
            Vector3? destination = CanPath(enemy, out failReason);
            if (destination == null) {
                return false;
            }
            BaseClass.Reset();
            PeekPoints = findPeekPosition(enemy);
            TargetPlace = enemy.KnownPlaces.LastKnownPlace;
            failReason = string.Empty;
            return true;
        }

        public void InitPath(Enemy enemy, Vector3 position, EnemyPlace place)
        {
            BaseClass.Reset();
            TargetPlace = place;
            PeekPoints = findPeekPosition(enemy);
        }

        private BotPeekPlan? findPeekPosition(Enemy enemy)
        {
            const float MIN_ANGLE_TO_PEEK = 5f;
            const float CORNER_PEEK_DIST = 3f;
            if (!enemy.Path.EnemyCorners.TryGetValue(ECornerType.Blind, out EnemyCorner blindCorner)) {
                return null;
            }

            Vector3[] pathCorners = enemy.Path.PathToEnemy.corners;
            int count = pathCorners.Length;
            int blindCornerIndex = blindCorner.PathIndex;
            Vector3 blindCornerPosition = blindCorner.GroundPosition;
            Vector3 botPosition = Bot.Position;
            Vector3 blindCornerDir = blindCornerPosition - botPosition;
            Vector3 blindCornerDirNormal = blindCornerDir.normalized;

            Vector3 startPeekPosition = blindCornerPosition - (blindCornerDirNormal * CORNER_PEEK_DIST);

            for (int i = blindCornerIndex; i < count; i++) {
                Vector3 corner = pathCorners[i];
                Vector3 dir = corner - blindCornerPosition;
                Vector3 dirNormal = dir.normalized;
                float signedAngle = findHorizSignedAngle(blindCornerDirNormal, dirNormal);
                if (Mathf.Abs(signedAngle) < MIN_ANGLE_TO_PEEK) {
                    continue;
                }
                Vector3 oppositePoint = blindCornerPosition - (dirNormal * CORNER_PEEK_DIST);
                if (NavMesh.Raycast(blindCornerPosition, oppositePoint, out NavMeshHit hit, -1)) {
                    oppositePoint = hit.position;
                }

                return new BotPeekPlan(startPeekPosition, oppositePoint, corner);
            }
            return null;
        }

        private float findHorizSignedAngle(Vector3 dirA, Vector3 dirB)
        {
            dirA.y = 0;
            dirB.y = 0;
            float signedAngle = Vector3.SignedAngle(dirA, dirB, Vector3.up);
            return signedAngle;
        }

        private void findNewCorners(NavMeshPath path)
        {
            var corners = path.corners;
            int cornerLength = corners.Length;
            newCorners.Clear();

            for (int i = 0; i < cornerLength - 1; i++) {
                Vector3 corner = corners[i];
                if ((corner - corners[i + 1]).sqrMagnitude > 1.5f) {
                    newCorners.Add(corner);
                }
            }

            Vector3? last = corners.LastElement();
            if (last != null)
                newCorners.Add(last.Value);
        }

        private readonly List<Vector3> newCorners = new List<Vector3>();

        private struct peekPositions
        {
            public Vector3 Start;
            public Vector3 End;
            public Vector3 DangerPoint;
        }

        private Vector3 GetPeekStartAndEnd(Vector3 blindCorner, Vector3 dangerPoint, Vector3 dirToBlindCorner, Vector3 dirToBlindDest, out Vector3 peekEnd)
        {
            const float maxMagnitude = 4f;
            const float minMagnitude = 2f;
            const float OppositePointMagnitude = 3f;

            Vector3 directionToStart = BotOwner.Position - blindCorner;

            Vector3 cornerStartDir;
            if (directionToStart.magnitude > maxMagnitude) {
                cornerStartDir = directionToStart.normalized * maxMagnitude;
            }
            else if (directionToStart.magnitude < minMagnitude) {
                cornerStartDir = directionToStart.normalized * minMagnitude;
            }
            else {
                cornerStartDir = Vector3.zero;
            }

            Vector3 PeekStartPosition = blindCorner + cornerStartDir;
            Vector3 directionToDangerPoint = dangerPoint - PeekStartPosition;

            // Rotate to the opposite side depending on the angle of the danger point to the start.
            float signAngle = GetSignedAngle(dirToBlindCorner.normalized, directionToDangerPoint.normalized);
            float rotationAngle = signAngle > 0 ? -90f : 90f;
            Quaternion rotation = Quaternion.Euler(0f, rotationAngle, 0f);

            var direction = rotation * dirToBlindDest.normalized;
            direction *= OppositePointMagnitude;

            CheckForObstacles(PeekStartPosition, direction, out Vector3 result);
            peekEnd = result;
            return PeekStartPosition;
        }

        private float GetSignedAngle(Vector3 dirCenter, Vector3 dirOther)
        {
            return Vector3.SignedAngle(dirCenter, dirOther, Vector3.up);
        }

        private void CheckForObstacles(Vector3 start, Vector3 direction, out Vector3 result)
        {
            if (!NavMesh.SamplePosition(start, out var startHit, 5f, -1)) {
                result = start + direction;
                return;
            }
            direction.y = 0f;
            if (!NavMesh.Raycast(startHit.position, direction, out var rayHit, -1)) {
                result = startHit.position + direction;
                if (NavMesh.SamplePosition(result, out var endHit, 5f, -1)) {
                    result = endHit.position;
                }
                return;
            }
            result = rayHit.position;
        }

        private float _nextCheckFinishTime;
        private const float ComeToRandomDist = 1f;
        private bool _canStartSearch;
        private float _nextCheckSearchTime;
        private float _nextCheckPosTime;
    }
}