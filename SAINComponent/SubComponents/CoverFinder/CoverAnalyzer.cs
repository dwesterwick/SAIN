﻿using BepInEx.Logging;
using EFT;

using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace SAIN.SAINComponent.SubComponents.CoverFinder
{
    public class CoverAnalyzer : SAINBase, ISAINClass
    {
        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public CoverAnalyzer(SAINComponentClass botOwner, CoverFinderComponent coverFinder) : base(botOwner)
        {
            Path = new NavMeshPath();
            CoverFinder = coverFinder;
        }

        private readonly CoverFinderComponent CoverFinder; 

        public bool CheckCollider(Collider collider, out CoverPoint newPoint, float minHeight, Vector3 origin, Vector3 target, float minEnemyDist)
        {
            OriginPoint = origin;
            TargetPosition = target;
            MinObstacleHeight = minHeight;
            MinEnemyDist = minEnemyDist;

            const float ExtendLengthThresh = 1.5f;

            newPoint = null;
            if (collider == null || collider.bounds.size.y < MinObstacleHeight || !ColliderDirection(collider))
            {
                return false;
            }

            Vector3 colliderPos = collider.transform.position;

            // The botToCorner from the target to the collider
            Vector3 colliderDir = (colliderPos - TargetPosition).normalized;
            colliderDir.y = 0f;

            if (collider.bounds.size.z > ExtendLengthThresh && collider.bounds.size.x > ExtendLengthThresh)
            {
                colliderDir *= ExtendLengthThresh;
            }

            // a farPoint on opposite side of the target
            Vector3 farPoint = colliderPos + colliderDir;

            // the closest edge to that farPoint
            if (NavMesh.SamplePosition(farPoint, out var hit, 1f, -1))
            {
                Vector3 point = hit.position;
                if (CheckPosition(point) && CheckMainPlayer(point))
                {
                    if (CheckPath(point, out bool isSafe, out NavMeshPath pathToPoint))
                    {
                        newPoint = new CoverPoint(SAIN, point, collider, pathToPoint);
                        newPoint.IsSafePath = isSafe;
                    }
                }
            }

            return newPoint != null;
        }

        private bool CheckMainPlayer(Vector3 point)
        {
            if (SAIN.EnemyController.IsMainPlayerActiveEnemy() == false && SAIN.EnemyController.IsMainPlayerAnEnemy() == true && GameWorldHandler.SAINMainPlayer?.SAINPerson?.Transform != null)
            {
                Vector3 testPoint = point + (Vector3.up * 0.65f);

                Vector3 headPos = GameWorldHandler.SAINMainPlayer.SAINPerson.Transform.Head;

                bool VisibleCheckPass = (VisibilityCheck(testPoint, headPos));

                if (SAINPlugin.LoadedPreset.GlobalSettings.Cover.DebugCoverFinder)
                {
                    if (VisibleCheckPass)
                    {
                        // Main Player does not have vision on coverpoint position
                        Logger.LogWarning("PASS");
                    }
                    else
                    {
                        // Main Player has vision
                        Logger.LogWarning("FAIL");
                    }
                }

                return VisibleCheckPass;
            }
            return true;
        }

        private bool ColliderDirection(Collider collider)
        {
            Vector3 pos = collider.transform.position;
            Vector3 target = TargetPosition;
            Vector3 bot = BotOwner.Position;

            Vector3 directionToTarget = target - bot;
            float targetDist = directionToTarget.magnitude;

            Vector3 directionToCollider = pos - bot;
            float colliderDist = directionToCollider.magnitude;

            float dot = Vector3.Dot(directionToTarget.normalized, directionToCollider.normalized);

            if (dot <= 0.33f)
            {
                return true;
            }
            if (dot <= 0.6f)
            {
                return colliderDist < targetDist * 0.75f;
            }
            if (dot <= 0.8f)
            {
                return colliderDist < targetDist * 0.5f;
            }
            return colliderDist < targetDist * 0.25f;
        }

        private float MinEnemyDist;

        private bool CheckPosition(Vector3 position)
        {
            if (CoverFinder.SpottedPoints.Count > 0)
            {
                foreach (var point in CoverFinder.SpottedPoints)
                {
                    if (!point.IsValidAgain && point.TooClose(position))
                    {
                        return false;
                    }
                }
            }
            if (CheckPositionVsOtherBots(position))
            {
                if ((position - TargetPosition).magnitude > MinEnemyDist)
                {
                    if (VisibilityCheck(position, TargetPosition))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckPath(Vector3 position, out bool isSafe, out NavMeshPath pathToPoint)
        {
            pathToPoint = new NavMeshPath();
            if (NavMesh.CalculatePath(OriginPoint, position, -1, pathToPoint) && pathToPoint.status == NavMeshPathStatus.PathComplete)
            {
                if (PathToEnemy(pathToPoint))
                {
                    isSafe = CheckPathSafety(pathToPoint);
                    return true;
                }
            }

            isSafe = false;
            return false;
        }

        private bool CheckPathSafety(NavMeshPath path)
        {
            Vector3 target;
            if (SAIN.HasEnemy)
            {
                target = SAIN.Enemy.EnemyHeadPosition;
            }
            else
            {
                target = TargetPosition;
            }
            return SAINBotSpaceAwareness.CheckPathSafety(path, target);
        }

        private readonly NavMeshPath Path;

        static bool DebugCoverFinder => SAINPlugin.LoadedPreset.GlobalSettings.Cover.DebugCoverFinder;

        private bool PathToEnemy(NavMeshPath path)
        {
            for (int i = 1; i < path.corners.Length - 1; i++)
            {
                var corner = path.corners[i];
                Vector3 cornerToTarget = TargetPosition - corner;
                Vector3 botToTarget = TargetPosition - OriginPoint;
                Vector3 botToCorner = corner - OriginPoint;

                if (cornerToTarget.magnitude < 0.5f)
                {
                    if (DebugCoverFinder)
                    {
                        //DrawDebugGizmos.Ray(OriginPoint, corner - OriginPoint, Color.red, (corner - OriginPoint).magnitude, 0.05f, true, 30f);
                    }

                    return false;
                }

                if (i == 1)
                {
                    if (Vector3.Dot(botToCorner.normalized, botToTarget.normalized) > 0.5f)
                    {
                        if (DebugCoverFinder)
                        {
                            //DrawDebugGizmos.Ray(corner, cornerToTarget, Color.red, cornerToTarget.magnitude, 0.05f, true, 30f);
                        }
                        return false;
                    }
                }
                else if (i < path.corners.Length - 2)
                {
                    Vector3 cornerB = path.corners[i + 1];
                    Vector3 directionToNextCorner = cornerB - corner;

                    if (Vector3.Dot(cornerToTarget.normalized, directionToNextCorner.normalized) > 0.5f)
                    {
                        if (directionToNextCorner.magnitude > cornerToTarget.magnitude)
                        {
                            if (DebugCoverFinder)
                            {
                                //DrawDebugGizmos.Ray(corner, cornerToTarget, Color.red, cornerToTarget.magnitude, 0.05f, true, 30f);
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool CheckPositionVsOtherBots(Vector3 position)
        {
            if (SAIN.Squad.SquadLocations == null || SAIN.Squad.Members == null || SAIN.Squad.Members.Count < 2)
            {
                return true;
            }

            const float DistanceToBotCoverThresh = 1f;

            foreach (var member in SAIN.Squad.Members.Values)
            {
                if (member != null && member.BotOwner != BotOwner)
                {
                    if (member.Cover.CurrentCoverPoint != null)
                    {
                        if (Vector3.Distance(position, member.Cover.CurrentCoverPoint.Position) < DistanceToBotCoverThresh)
                        {
                            return false;
                        }
                    }
                    if (member.Cover.FallBackPoint != null)
                    {
                        if (Vector3.Distance(position, member.Cover.FallBackPoint.Position) < DistanceToBotCoverThresh)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool VisibilityCheck(Vector3 position, Vector3 target)
        {
            const float offset = 0.15f;

            if (CheckRayCast(position, target))
            {
                Vector3 enemyDirection = target - position;
                enemyDirection = enemyDirection.normalized * offset;

                Quaternion right = Quaternion.Euler(0f, 90f, 0f);
                Vector3 rightPoint = right * enemyDirection;
                rightPoint += position;

                if (CheckRayCast(rightPoint, target))
                {
                    Quaternion left = Quaternion.Euler(0f, -90f, 0f);
                    Vector3 leftPoint = left * enemyDirection;
                    leftPoint += position;

                    if (CheckRayCast(leftPoint, target))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool CheckRayCast(Vector3 point, Vector3 target, float distance = 3f)
        {
            point.y += 0.66f;
            target.y += 0.66f;
            Vector3 direction = target - point;
            return Physics.Raycast(point, direction, distance, LayerMaskClass.HighPolyWithTerrainMask);
        }

        private float MinObstacleHeight;
        private Vector3 OriginPoint;
        private Vector3 TargetPosition;
    }
}