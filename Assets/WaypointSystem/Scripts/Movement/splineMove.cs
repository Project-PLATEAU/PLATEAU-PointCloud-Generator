using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace WaypointSystem
{
    using DG.Tweening;
    using DG.Tweening.Core;
    using DG.Tweening.Plugins.Options;

    public class splineMove : MonoBehaviour
    {
        public PathManager pathContainer;

        public bool onStart = false;

        public bool moveToPath = false;

        public bool reverse = false;

        public int startPoint = 0;

        [HideInInspector]
        public int currentPoint = 0;

        public bool closeLoop = false;

        public LocalType localType = LocalType.none;
        public enum LocalType
        {
            none,
            toPath,
            toObject
        }

        public float lookAhead = 0;

        public float sizeToAdd = 0;

        public TimeValue timeValue = TimeValue.speed;
        public enum TimeValue
        {
            time,
            speed
        }

        public float speed = 5;

        public AnimationCurve animEaseType;

        public LoopType loopType = LoopType.none;
        public enum LoopType
        {
            none,
            loop,
            pingPong,
            random,
            yoyo
        }

        [HideInInspector]
        public Vector3[] waypoints;

        public PathType pathType = PathType.CatmullRom;

        public PathMode pathMode = PathMode.Full3D;

        public Ease easeType = Ease.Linear;

        public AxisConstraint lockPosition = AxisConstraint.None;

        public AxisConstraint lockRotation = AxisConstraint.None;

		public RotationType waypointRotation = RotationType.none;
        public enum RotationType
        {
            none,
            all
			/*
            x,
            y,
            z
            */
        }

        public Transform rotationTarget;

        public UnityEvent movementStart = new UnityEvent();
        public event Action movementStartEvent;

        public WaypointEvent movementChange = new WaypointEvent();
        public event Action<int> movementChangeEvent;

        public UnityEvent movementEnd = new UnityEvent();
        public event Action movementEndEvent;

        [HideInInspector]
        public Tweener tween;
        private bool moveToPathBool;
        private Vector3[] wpPos;
        private float originSpeed;
        private Quaternion originRot;
        private System.Random rand = new System.Random();
        private int[] rndArray;
        private Coroutine waitRoutine;


        void Start()
        {
            if (onStart)
                StartMove();
        }


        public void StartMove()
        {
            if (pathContainer == null)
            {
                Debug.LogWarning(gameObject.name + " has no path! Please set Path Container.");
                return;
            }

            waypoints = pathContainer.GetPathPoints(localType != LocalType.none);
            originSpeed = speed;
            originRot = transform.rotation;
            moveToPathBool = moveToPath;

            startPoint = Mathf.Clamp(startPoint, 0, waypoints.Length - 1);
            int index = startPoint;
            if (reverse)
            {
                Array.Reverse(waypoints);
                index = waypoints.Length - 1 - index;
            }
            Initialize(index);

            Stop();
            CreateTween();
        }


        private void Initialize(int startAt = 0)
        {
            if (!moveToPathBool) startAt = 0;
            wpPos = new Vector3[waypoints.Length - startAt];
            for (int i = 0; i < wpPos.Length; i++)
                wpPos[i] = waypoints[i + startAt] + new Vector3(0, sizeToAdd, 0);

            if (localType == LocalType.toObject)
            {
                for (int i = 0; i < wpPos.Length; i++)
                    wpPos[i] = transform.position + wpPos[i];
            }
        }


        private void CreateTween()
        {

            TweenParams parms = new TweenParams();
            if (timeValue == TimeValue.speed)
                parms.SetSpeedBased();
            if (loopType == LoopType.yoyo)
                parms.SetLoops(-1, DG.Tweening.LoopType.Yoyo);

            if (easeType == Ease.Unset)
                parms.SetEase(animEaseType);
            else
                parms.SetEase(easeType);

            if (moveToPathBool)
                parms.OnWaypointChange(OnWaypointReached);
            else
            {
                if (loopType == LoopType.random)
                    RandomizeWaypoints();
                else if (loopType == LoopType.yoyo)
                    parms.OnStepComplete(ReachedEnd);

                Vector3 startPos = wpPos[0];
                if (localType == LocalType.toPath) 
                    startPos = pathContainer.transform.TransformPoint(startPos);
                transform.position = startPos;

                parms.OnWaypointChange(OnWaypointChange);
                parms.OnComplete(ReachedEnd);
            }

            if (pathMode == PathMode.Ignore && waypointRotation != RotationType.none)
            {
                if (rotationTarget == null)
                    rotationTarget = transform;
                parms.OnUpdate(OnWaypointRotation);
            }

            if (localType == LocalType.toPath)
            {
                tween = transform.DOLocalPath(wpPos, originSpeed, pathType, pathMode)
                                 .SetAs(parms)
                                 .SetOptions(closeLoop, lockPosition, lockRotation)
                                 .SetLookAt(lookAhead);
            }
            else
            {
                tween = transform.DOPath(wpPos, originSpeed, pathType, pathMode)
                                 .SetAs(parms)
                                 .SetOptions(closeLoop, lockPosition, lockRotation)
                                 .SetLookAt(lookAhead);
            }

            tween.Play();
            if (!moveToPathBool && startPoint > 0)
            {
                GoToWaypoint(startPoint);
                startPoint = 0;
            }

            if (originSpeed != speed)
                ChangeSpeed(speed);

            movementStart.Invoke();
            if (movementStartEvent != null)
                movementStartEvent();
        }


        private void OnWaypointReached(int index)
        {
            if (index <= 0) return;

            Stop();
            moveToPathBool = false;
            Initialize();
            CreateTween();
        }


        private void OnWaypointChange(int index)
        {
            index = pathContainer.GetWaypointIndex(index);

            if (index == -1) return;
            if (loopType != LoopType.yoyo && reverse)
                index = pathContainer.GetWaypointCount() - 1 - index;
            if (loopType == LoopType.random)
                index = rndArray[index];

            currentPoint = index;

            if (loopType == LoopType.random && index == rndArray[rndArray.Length - 1])
                return;

            if (startPoint > 0 && pathContainer.GetWaypointIndex(startPoint) != index)
                return;

            movementChange.Invoke(index);
            if (movementChangeEvent != null)
                movementChangeEvent(index);
        }


        private void OnWaypointRotation()
        {
            int lookPoint = currentPoint;
            lookPoint = Mathf.Clamp(pathContainer.GetWaypointIndex(currentPoint), 0, pathContainer.GetWaypointCount());

            if (!tween.IsInitialized() || tween.IsComplete())
            {
                ApplyWaypointRotation(pathContainer.GetWaypoint(lookPoint).rotation);
                return;
            }

            TweenerCore<Vector3, DG.Tweening.Plugins.Core.PathCore.Path, PathOptions> tweenPath = tween as TweenerCore<Vector3, DG.Tweening.Plugins.Core.PathCore.Path, PathOptions>;
            float currentDist = tweenPath.PathLength() * tweenPath.ElapsedPercentage();
            float pathLength = 0f;
            float currentPerc = 0f;
            int targetPoint = currentPoint;

            if (moveToPathBool)
            {
                pathLength = tweenPath.changeValue.wpLengths[1];
                currentPerc = currentDist / pathLength;
                ApplyWaypointRotation(Quaternion.Lerp(originRot, pathContainer.GetWaypoint(currentPoint).rotation, currentPerc));
                return;
            }

            if (pathContainer is BezierPathManager)
            {
                BezierPathManager bPath = pathContainer as BezierPathManager;
                int curPoint = currentPoint;

                if (reverse)
                {
                    targetPoint = bPath.GetWaypointCount() - 2 - (waypoints.Length - currentPoint - 1);
                    curPoint = (bPath.bPoints.Count - 2) - targetPoint;
                }

                int prevPoints = (int)(curPoint * bPath.pathDetail * 10);

                if (bPath.customDetail)
                {
                    prevPoints = 0;
                    for (int i = 0; i < targetPoint; i++)
                        prevPoints += (int)(bPath.segmentDetail[i] * 10);
                }

                if (reverse)
                {
                    for (int i = 0; i <= curPoint * 10; i++)
                        currentDist -= tweenPath.changeValue.wpLengths[i];
                }
                else
                {
                    for (int i = 0; i <= prevPoints; i++)
                        currentDist -= tweenPath.changeValue.wpLengths[i];
                }

                if (bPath.customDetail)
                {
                    for (int i = prevPoints + 1; i <= prevPoints + bPath.segmentDetail[currentPoint] * 10; i++)
                        pathLength += tweenPath.changeValue.wpLengths[i];
                }
                else
                {
                    for (int i = prevPoints + 1; i <= prevPoints + 10; i++)
                        pathLength += tweenPath.changeValue.wpLengths[i];
                }
            }
            else
            {
                if(reverse) targetPoint = waypoints.Length - currentPoint - 1;

                for (int i = 0; i <= targetPoint; i++)
                    currentDist -= tweenPath.changeValue.wpLengths[i];
				
                pathLength = tweenPath.changeValue.wpLengths[targetPoint + 1];
            }

            currentPerc = currentDist / pathLength;
            if (pathContainer is BezierPathManager)
            {
                lookPoint = targetPoint;
                if (reverse) lookPoint++;
            }

            currentPerc = Mathf.Clamp01(currentPerc);
            ApplyWaypointRotation(Quaternion.Lerp(pathContainer.GetWaypoint(lookPoint).rotation, pathContainer.GetWaypoint(reverse ? lookPoint - 1 : lookPoint + 1).rotation, currentPerc));
        }


        private void ApplyWaypointRotation(Quaternion rotation)
        {
			rotationTarget.rotation = rotation;
        }


        private void ReachedEnd()
        {
            movementEnd.Invoke();
            if (movementEndEvent != null)
                movementEndEvent();

            switch (loopType)
            {
                case LoopType.none:
                    tween = null;
                    return;

                case LoopType.loop:
                    currentPoint = 0;
                    CreateTween();
                    break;

                case LoopType.pingPong:
                    reverse = !reverse;
                    Array.Reverse(waypoints);
                    Initialize();

                    CreateTween();
                    break;

                case LoopType.yoyo:
                    reverse = !reverse;
                    break;

                case LoopType.random:
                    RandomizeWaypoints();
                    CreateTween();
                    break;
            }
        }


        private void RandomizeWaypoints()
        {
            Initialize();
            rndArray = new int[wpPos.Length];
            for (int i = 0; i < rndArray.Length; i++)
            {
                rndArray[i] = i;
            }

            int n = wpPos.Length;
            while (n > 1)
            {
                int k = rand.Next(n--);
                Vector3 temp = wpPos[n];
                wpPos[n] = wpPos[k];
                wpPos[k] = temp;

                int tmpI = rndArray[n];
                rndArray[n] = rndArray[k];
                rndArray[k] = tmpI;
            }

            Vector3 first = wpPos[0];
            int rndFirst = rndArray[0];
            for (int i = 0; i < wpPos.Length; i++)
            {
                if (rndArray[i] == currentPoint)
                {
                    rndArray[i] = rndFirst;
                    wpPos[0] = wpPos[i];
                    wpPos[i] = first;
                }
            }
            rndArray[0] = currentPoint;
        }


        public void GoToWaypoint(int index)
        {
            if (tween == null)
                return;

            if (reverse) index = waypoints.Length - 1 - index;

            tween.ForceInit();
            tween.GotoWaypoint(index, true);
        }


        public void Pause(float seconds = 0f)
        {
            if(waitRoutine != null) StopCoroutine(waitRoutine);
            if (tween != null) tween.Pause();

            if (seconds > 0)
                waitRoutine = StartCoroutine(Wait(seconds));
        }


        private IEnumerator Wait(float secs = 0f)
        {
            yield return new WaitForSeconds(secs);
            Resume();
        }


        public void Resume()
        {
            if (waitRoutine != null)
            {
                StopCoroutine(waitRoutine);
                waitRoutine = null;
            }

            if (tween != null)
                tween.Play();
        }


        public void Reverse()
        {
            reverse = !reverse;
            float timeRemaining = 0f;
            if(tween != null)
                timeRemaining = 1 - tween.ElapsedPercentage(false);
            
            startPoint = waypoints.Length - 1 - currentPoint;
            StartMove();
            tween.ForceInit();
            tween.fullPosition = tween.Duration(false) * timeRemaining;
        }

        public void SetPath(PathManager newPath)
        {
            Stop();
            pathContainer = newPath;
            StartMove();
        }


        public void Stop()
        {
            StopAllCoroutines();
            waitRoutine = null;

            if (tween != null)
                tween.Kill();
            tween = null;
        }


        public void ResetToStart()
        {
            Stop();
            currentPoint = 0;
            if (pathContainer)
            {
                transform.position = pathContainer.waypoints[currentPoint].position + new Vector3(0, sizeToAdd, 0);
            }
        }


        public void ChangeSpeed(float value)
        {
            float newValue;
            if (timeValue == TimeValue.speed)
                newValue = value / originSpeed;
            else
                newValue = originSpeed / value;

            speed = value;
            if (tween != null)
                tween.timeScale = newValue;
        }


        public bool IsMoving()
        {
            return tween != null;
        }


        public bool IsMovingToPath()
        {
            return IsMoving() && moveToPathBool;
        }

        public bool IsPaused()
        {
            return waitRoutine != null || tween != null && !tween.IsPlaying();
        }


        void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}