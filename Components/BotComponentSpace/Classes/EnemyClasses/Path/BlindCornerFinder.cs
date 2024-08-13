using SAIN.Components;
using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using SAIN.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class BlindCornerFinder : EnemyBase
    {
        private Coroutine _blindCornerLoop;
        private EnemyCorner _blindCorner = new EnemyCorner();
        private RaycastBatchData _raycasts = new RaycastBatchData(LayerMaskClass.HighPolyWithTerrainMask, new ListCache<RaycastObject>("BlindCornerRaycasts"));
        private BiDirectionObject _biDirectionData = new BiDirectionObject();

        private readonly List<Vector3> _raycastPoints = new List<Vector3>();
        private readonly List<Vector3> _corners = new List<Vector3>();
        private readonly List<Vector3> _segments = new List<Vector3>();

        private Vector3 HEIGHT_OFFSET = Vector3.up * HEIGHT;
        private Vector3 HEIGHT_OFFSET_HALF = Vector3.up * HEIGHT_HALF;

        private static readonly LayerMask _mask = LayerMaskClass.HighPolyWithTerrainMask;
        private const float HEIGHT = 1.6f;
        private const float HEIGHT_HALF = HEIGHT / 2f;
        private const float SEGMENT_LENGTH = 1f;
        private const float SEGMENT_LENGTH_SQR = SEGMENT_LENGTH * SEGMENT_LENGTH;

        private float _nextLogTime;

        public BlindCornerFinder(Enemy enemy) : base(enemy)
        {
            //createDebug();
            JobManager.BiDirections.Add(_biDirectionData);
        }

        public void Init()
        {
            _blindCornerLoop = Bot.StartCoroutine(FindBlindCorner3Loop());
            SubscribeToDispose(Dispose);
        }

        public void Dispose()
        {
            if (_blindCornerLoop != null) {
                Bot.StopCoroutine(_blindCornerLoop);
                _blindCornerLoop = null;
            }
            //foreach (var debug in _debugObjects) {
            //    GameObject.Destroy(debug);
            //}
            JobManager.BiDirections.Remove(_biDirectionData);
            _raycasts.Dispose();
        }

        public void ClearBlindCorner()
        {
            Enemy.Path.EnemyCorners.Remove(ECornerType.Blind);
        }

        private void createDebug()
        {
            //float size = 0.015f;
            //Color color = Color.white;
            //for (int i = 0; i < _debugObjects.Length; i++) {
            //    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    sphere.GetComponent<Renderer>().material.color = color;
            //    sphere.GetComponent<Collider>().enabled = false;
            //    sphere.transform.position = new Vector3(0, -1000, 0);
            //    sphere.transform.localScale = new Vector3(size, size, size);
            //    _debugObjects[i] = sphere;
            //}
        }

        private void findCastPoints(Vector3[] corners, List<Vector3> castPoints)
        {
            const int STEPCOUNT = 5;
            castPoints.Clear();
            int count = corners.Length;
            Vector3 characterHeight = Vector3.up * 1.65f;
            Vector3 step = characterHeight * (1 / STEPCOUNT);

            for (int i = count - 1; i > 0; i--) {
                Vector3 corner = corners[i];
                Vector3 nextCorner = corners[i - 1];

                _segments.Clear();
                findSegmentsBetweenCorner(corner, nextCorner, _segments);

                Logger.LogInfo($"segments count {_segments.Count}");

                for (int j = 0; j < _segments.Count; j++) {
                    Vector3 segment = _segments[j];
                    castPoints.Add(segment);
                    for (int h = 0; h < STEPCOUNT; h++) {
                        segment += step;
                        castPoints.Add(segment);
                    }
                }
            }
        }

        //private readonly GameObject[] _debugObjects = new GameObject[2000];

        public IEnumerator FindBlindCorner2(Vector3[] corners, Vector3 enemyPosition)
        {
            _corners.Clear();
            _corners.AddRange(corners);
            int count = _corners.Count;

            if (count <= 2) {
                ClearBlindCorner();
                yield break;
            }

            //const int MAX_RAYCASTS_PERFRAME = 100;
            int blindCornerIndex = count - 1;
            Vector3? blindCorner = null;
            //int cornersChecked = 0;

            float startTime = Time.time + 1f;
            _raycastPoints.Clear();
            //int raycasts = 0;
            int pointsPerCorner = 0;
            for (int i = 0; i < count; i++) {
                Vector3 corner = _corners[i];
                blindCornerIndex = i;
                pointsPerCorner = AddSegmentPoints(corner);

                //_segments.Clear();
                //findSegmentsBetweenCorner(corner, nextCorner, _segments);
                //int segmentCount = _segments.Count;
                //for (int j = 0; j < segmentCount; j++) {
                //    raycasts += AddSegmentPoints(_segments[j]);
                //}
                //cornersChecked++;
                //if (raycasts >= MAX_RAYCASTS_PERFRAME) {
                //    raycasts = 0;
                //    _raycasts.ScheduleRaycastToPoints(_points.ToArray(), Bot.Transform.EyePosition);
                //    while (_raycasts.Status != EJobStatus.Complete) {
                //        if (startTime < Time.time) {
                //            Logger.LogError($"Raycast Job is taking too long!");
                //            break;
                //        }
                //        yield return null;
                //    }
                //}
            }

            _raycasts.ScheduleRaycastToPoints(_raycastPoints, Bot.Transform.EyePosition);
            yield return new WaitForEndOfFrame();
            while (_raycasts.Status != EJobStatus.Complete) {
                if (startTime < Time.time) {
                    Logger.LogError($"Raycast Job is taking too long!");
                    break;
                }
                yield return null;
            }

            List<RaycastObject> raycastDatas = _raycasts.Datas;
            int raycastCount = raycastDatas.Count;
            for (int i = raycastCount - 1; i >= 0; i--) {
                RaycastObject raycastData = raycastDatas[i];
                if (raycastData.Hit.collider != null) {
                    blindCornerIndex = i / pointsPerCorner;
                    blindCorner = _corners[blindCornerIndex];
                    break;
                }
            }

            if (blindCorner == null) {
                ClearBlindCorner();
                yield break;
            }

            Vector3 eyePos = Bot.Transform.EyePosition;
            Vector3 blindCornerDir = blindCorner.Value - eyePos;
            blindCornerDir.y = 0;
            Vector3 enemyPosDir = enemyPosition - eyePos;
            enemyPosDir.y = 0;
            BiDirData dirData = new BiDirData(blindCornerDir, enemyPosDir);
            _biDirectionData.UpdateData(dirData);

            startTime = Time.time + 1f;
            yield return new WaitForEndOfFrame();
            while (_biDirectionData.Status != EJobStatus.Complete) {
                if (startTime < Time.time) {
                    Logger.LogError($"Direction Job is taking too long!");
                }
                yield return null;
            }

            float signedAngle = _biDirectionData.Data.SignedAngle;
            _blindCorner.UpdateData(blindCorner.Value, signedAngle, blindCornerIndex);
            Enemy.Path.EnemyCorners.AddOrReplace(ECornerType.Blind, _blindCorner);
            DebugGizmos.Line(blindCorner.Value, eyePos, Color.cyan, 0.1f, true, 1f);
        }

        public IEnumerator FindBlindCorner3Loop()
        {
            var wait = new WaitForSeconds(0.25f);
            var waitActiveEnemy = new WaitForSeconds(0.05f);
            while (true) {
                yield return Enemy.IsCurrentEnemy ? waitActiveEnemy : wait;

                if (!Enemy.EnemyKnown) {
                    ClearBlindCorner();
                    continue;
                }

                NavMeshPath enemyPath = Enemy.Path.PathToEnemy;
                if (enemyPath.status == NavMeshPathStatus.PathInvalid) {
                    continue;
                }
                Vector3[] corners = enemyPath.corners;
                int count = corners.Length;
                if (count <= 2) {
                    ClearBlindCorner();
                    yield break;
                }

                _corners.Clear();
                _corners.AddRange(corners);
                int blindCornerIndex = count - 1;
                Vector3? blindCorner = null;

                float startTime = Time.time + 1f;
                _raycastPoints.Clear();
                int pointsPerCorner = 0;
                for (int i = 0; i < count; i++) {
                    Vector3 corner = _corners[i];
                    pointsPerCorner = AddSegmentPoints(corner);
                }

                _raycasts.ScheduleRaycastToPoints(_raycastPoints, Bot.Transform.EyePosition);

                yield return new WaitForEndOfFrame();
                while (_raycasts.Status != EJobStatus.Complete) {
                    if (startTime < Time.time) {
                        Logger.LogError($"Raycast Job is taking too long!");
                        break;
                    }
                    yield return null;
                }

                Vector3? enemyLastKnown = Enemy.KnownPlaces.LastKnownPosition;
                if (enemyLastKnown == null) {
                    ClearBlindCorner();
                    yield break;
                }

                List<RaycastObject> raycastDatas = _raycasts.Datas;
                int raycastCount = raycastDatas.Count;
                for (int i = raycastCount - 1; i >= 0; i--) {
                    RaycastObject raycastData = raycastDatas[i];
                    if (raycastData.Hit.collider != null) {
                        blindCornerIndex = i / pointsPerCorner;
                        blindCorner = _corners[blindCornerIndex];
                        break;
                    }
                }

                if (blindCorner == null) {
                    ClearBlindCorner();
                    yield break;
                }

                Vector3 eyePos = Bot.Transform.EyePosition;
                Vector3 blindCornerDir = blindCorner.Value - eyePos;
                blindCornerDir.y = 0;
                Vector3 enemyPosDir = enemyLastKnown.Value - eyePos;
                enemyPosDir.y = 0;
                BiDirData dirData = new BiDirData(blindCornerDir, enemyPosDir);
                _biDirectionData.UpdateData(dirData);

                startTime = Time.time + 1f;
                yield return new WaitForEndOfFrame();
                while (_biDirectionData.Status != EJobStatus.Complete) {
                    if (startTime < Time.time) {
                        Logger.LogError($"Direction Job is taking too long!");
                    }
                    yield return null;
                }

                float signedAngle = _biDirectionData.Data.SignedAngle;
                _blindCorner.UpdateData(blindCorner.Value, signedAngle, blindCornerIndex);
                Enemy.Path.EnemyCorners.AddOrReplace(ECornerType.Blind, _blindCorner);
                DebugGizmos.Line(blindCorner.Value, eyePos, Color.cyan, 0.1f, true, 1f);
            }
        }

        private int AddSegmentPoints(Vector3 segment)
        {
            int result = 0;

            _raycastPoints.Add(segment + (Vector3.up * 0.1f));
            result++;
            _raycastPoints.Add(segment + HEIGHT_OFFSET_HALF);
            result++;
            _raycastPoints.Add(segment + HEIGHT_OFFSET);
            result++;

            return result;
        }

        private bool CheckSightAtSegment(Vector3 segment, Vector3 origin, out Vector3 sightPoint)
        {
            Vector3 first = segment + (Vector3.up * 0.1f);
            Vector3 firstDir = first - origin;
            DebugGizmos.Sphere(first, 0.1f, Color.blue, true, 10f);

            if (!Physics.Raycast(origin, firstDir, firstDir.magnitude, LayerMaskClass.HighPolyWithTerrainMaskAI)) {
                sightPoint = segment;
                DebugGizmos.Line(origin, sightPoint, Color.blue, 0.05f, true, 10f, false);
                return true;
            }

            Vector3 second = segment + HEIGHT_OFFSET_HALF;
            Vector3 secondDir = second - origin;
            if (!Physics.Raycast(origin, secondDir, secondDir.magnitude, LayerMaskClass.HighPolyWithTerrainMaskAI)) {
                sightPoint = second;
                DebugGizmos.Line(origin, sightPoint, Color.blue, 0.05f, true, 10f, false);
                return true;
            }

            Vector3 third = segment + HEIGHT_OFFSET;
            Vector3 thirdDir = third - origin;
            if (!Physics.Raycast(origin, thirdDir, thirdDir.magnitude, LayerMaskClass.HighPolyWithTerrainMaskAI)) {
                sightPoint = third;
                DebugGizmos.Line(origin, sightPoint, Color.blue, 0.025f, true, 10f, false);
                return true;
            }

            sightPoint = Vector3.zero;
            return false;
        }

        private void findSegmentsBetweenCorner(Vector3 corner, Vector3 nextCorner, List<Vector3> segmentsList)
        {
            segmentsList.Add(corner);
            Vector3 cornerDirection = (nextCorner - corner);
            float sqrMag = cornerDirection.sqrMagnitude;
            if (sqrMag <= SEGMENT_LENGTH_SQR) {
                return;
            }
            if (sqrMag <= SEGMENT_LENGTH_SQR * 2f) {
                segmentsList.Add(Vector3.Lerp(corner, nextCorner, 0.5f));
                return;
            }
            float segmentLength = sqrMag / SEGMENT_LENGTH_SQR;
            Vector3 segmentDir = cornerDirection.normalized * segmentLength;
            int segmentCount = Mathf.RoundToInt(segmentLength);
            Vector3 segmentPoint = corner;
            for (int i = 0; i < segmentCount; i++) {
                segmentPoint += segmentDir;
                segmentsList.Add(segmentPoint);
            }
        }

        public IEnumerator FindBlindCorner(Vector3[] corners, Vector3 enemyPosition)
        {
            int count = corners.Length;
            if (count <= 1) {
                ClearBlindCorner();
                yield break;
            }

            Stopwatch sw = Stopwatch.StartNew();
            int totalRaycasts = 0;
            const int MAX_CASTS_PER_FRAME = 4;
            const int MAX_ITERATIONS_REAL_CORNER = 15;

            var transform = Bot.Transform;
            Vector3 lookPoint = transform.EyePosition;
            Vector3 lookOffset = lookPoint - Bot.Position;
            float heightOffset = lookOffset.y;

            Vector3 notVisibleCorner = enemyPosition;
            Vector3 lastVisibleCorner = corners[1];
            int index = 1;

            int raycasts = 0;

            // Note: currently this only finds the first corner they can't see past,
            // I should refactor and have it start from the last corner and descend until they CAN see a corner
            if (count > 2) {
                _corners.Clear();
                _corners.AddRange(corners);

                notVisibleCorner = _corners[2];
                lastVisibleCorner = _corners[1];

                for (int i = 1; i < count; i++) {
                    raycasts++;
                    Vector3 checkingCorner = _corners[i];
                    if (rayCastToCorner(checkingCorner, lookPoint, heightOffset)) {
                        index = i - 1;
                        lastVisibleCorner = _corners[i - 1];
                        notVisibleCorner = checkingCorner;
                        break;
                    }
                    if (raycasts >= MAX_CASTS_PER_FRAME) {
                        totalRaycasts += raycasts;
                        raycasts = 0;
                        yield return null;
                    }
                }
                _corners.Clear();
            }
            // end Note

            lastVisibleCorner.y += heightOffset;
            notVisibleCorner.y += heightOffset;

            Vector3 pointPastCorner = RaycastPastCorner(lastVisibleCorner, lookPoint, 0f, 10f);
            raycasts++;

            if (raycasts >= MAX_CASTS_PER_FRAME) {
                totalRaycasts += raycasts;
                raycasts = 0;
                yield return null;
            }

            float sign = Vector.FindFlatSignedAngle(pointPastCorner, notVisibleCorner, lookPoint);
            float angle = sign <= 0 ? -15f : 15f;
            float rotationStep = angle / MAX_ITERATIONS_REAL_CORNER;

            Vector3 blindCorner = lastVisibleCorner;
            Vector3 directionToBlind = lastVisibleCorner - lookPoint;
            float rayMaxDist = (pointPastCorner - lookPoint).magnitude;

            for (int i = 0; i < MAX_ITERATIONS_REAL_CORNER; i++) {
                raycasts++;

                directionToBlind = Vector.Rotate(directionToBlind, 0, rotationStep, 0);

                bool hit = Physics.Raycast(lookPoint, directionToBlind, rayMaxDist, LayerMaskClass.HighPolyWithTerrainMask);
                drawDebug(lookPoint + directionToBlind, lookPoint, hit);

                if (hit) {
                    //Logger.LogDebug($"Angle where LOS broken [{rotationStep * i}] after [{i}] iterations");
                    break;
                }

                blindCorner = lookPoint + directionToBlind;

                if (raycasts >= MAX_CASTS_PER_FRAME) {
                    totalRaycasts += raycasts;
                    raycasts = 0;
                    yield return null;
                }
            }

            blindCorner.y -= heightOffset;
            Enemy.Path.EnemyCorners.AddOrReplace(ECornerType.Blind, new EnemyCorner(blindCorner, angle, index));

            if (raycasts > 0) {
                totalRaycasts += raycasts;
                raycasts = 0;
                yield return null;
            }
            sw.Stop();
            if (_nextLogTime < Time.time) {
                _nextLogTime = Time.time + 5f;
                //float time = (sw.ElapsedMilliseconds / 1000f).Round100();
                //Logger.LogDebug($"Total Raycasts: [{totalRaycasts}] Time To Complete: [{time}] seconds");
            }
        }

        //private Vector3 findDispersionPositionAtNavMesh(Vector3 enemyPosition, float dispersion)
        //{
        //    const int iterations = 50;
        //    const float navSampleRange = 0.5f;
        //    for (int i = 0; i < iterations; i++)
        //    {
        //        Vector3 random = UnityEngine.Random.onUnitSphere * dispersion;
        //        random.y = 0;
        //        Vector3 point = enemyPosition + random;
        //        if (NavMesh.SamplePosition(point, out var hit, navSampleRange, -1))
        //        {
        //            return hit.position;
        //        }
        //    }
        //}

        //private void findBlockedPoint(Vector3[] corners, float segmentLength, Vector3 lastKnownPosition)
        //{
        //    const float reduceOffset = 0.66f;
        //
        //    Vector3? result = null;
        //    var transform = Bot.Transform;
        //    float lengthSqr = segmentLength * segmentLength;
        //    int count = corners.Length;
        //    for (int i = count - 1; i > 0; i--)
        //    {
        //        Vector3 start = corners[i];
        //        Vector3 lookSensor = transform.EyePosition;
        //        Vector3 botPosition = transform.Position;
        //        Vector3 offset = (lookSensor - botPosition);
        //        const int PointsToCheck = 5;
        //        Vector3 pointDir = offset / PointsToCheck;
        //
        //        Vector3 rayCastTarget = start;
        //        rayCastTarget.y += offset.y;
        //        result.Add(start);
        //
        //        Vector3 end = corners[i - 1];
        //        Vector3 direction = end - start;
        //        float sqrMagnitude = direction.sqrMagnitude;
        //
        //        if (sqrMagnitude <= lengthSqr)
        //            continue;
        //
        //        if (sqrMagnitude <= lengthSqr / 2f)
        //        {
        //            Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
        //            result.Add(midPoint);
        //            continue;
        //        }
        //
        //        Vector3 directionNormal = direction.normalized;
        //        Vector3 segment = directionNormal * segmentLength;
        //        float currentLength = 0f;
        //        while (currentLength < lengthSqr)
        //        {
        //
        //        }
        //
        //
        //    }
        //}

        private void drawDebug(Vector3 corner, Vector3 lookPoint, bool hit)
        {
            if (SAINPlugin.DebugMode && SAINPlugin.DebugSettings.Gizmos.DebugDrawBlindCorner) {
                Color color = hit ? Color.red : Color.green;
                float lineWidth = 0.01f;
                float expireTime = 30f;

                //float lowerHeight = (Bot.Position - Bot.Transform.EyePosition).y * 0.8f;
                //corner.y += lowerHeight;
                //lookPoint.y += lowerHeight;

                DebugGizmos.Line(corner, lookPoint, color, lineWidth, true, expireTime, true);
            }
        }

        public static Vector3 RaycastPastCorner(Vector3 corner, Vector3 lookPoint, float addHeight, float addDistance = 2f)
        {
            corner.y += addHeight;
            Vector3 cornerDir = corner - lookPoint;

            Vector3 farPoint;
            if (Physics.Raycast(lookPoint, cornerDir, out var hit, addDistance, _mask)) {
                farPoint = hit.point;
            }
            else {
                farPoint = corner + cornerDir.normalized * addDistance;
            }
            Vector3 midPoint = Vector3.Lerp(farPoint, corner, 0.5f);
            return midPoint;
        }

        private bool rayCastToCorner(Vector3 corner, Vector3 lookPoint, float heightOffset)
        {
            corner.y += heightOffset;
            Vector3 direction = corner - lookPoint;
            return Physics.Raycast(lookPoint, direction, direction.magnitude, _mask);
        }
    }
}