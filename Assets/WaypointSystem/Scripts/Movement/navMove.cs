using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace WaypointSystem
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class navMove : MonoBehaviour
    {
        public PathManager pathContainer;

        public bool onStart = false;

        public bool moveToPath = false;

        public bool reverse = false;

        public int startPoint = 0;

        [HideInInspector]
        public int currentPoint = 0;

        public bool closeLoop = false;

        public bool updateRotation = true;

        public enum LoopType
        {
            none,
            loop,
            pingPong,
            random
        }
        public LoopType loopType = LoopType.none;

        [HideInInspector]
        public Transform[] waypoints;

        public UnityEvent movementStart;
        public event Action movementStartEvent;

        public WaypointEvent movementChange;
        public event Action<int> movementChangeEvent;

        public UnityEvent movementEnd;
        public event Action movementEndEvent;

        private bool moveToPathBool;
        private NavMeshAgent agent;
        private System.Random rand = new System.Random();
        private int rndIndex = 0;
        private Coroutine waitRoutine;
        private bool isMoving = false;


        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent && agent.stoppingDistance == 0)
                agent.stoppingDistance = 0.5f;
        }


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

            waypoints = new Transform[pathContainer.waypoints.Length];
            Array.Copy(pathContainer.waypoints, waypoints, pathContainer.waypoints.Length);
            moveToPathBool = moveToPath;

            startPoint = Mathf.Clamp(startPoint, 0, waypoints.Length - 1);
            currentPoint = startPoint;

            Stop();
            StartCoroutine(Move());
        }


        private IEnumerator Move()
        {
            agent.isStopped = false;
            agent.updateRotation = updateRotation;
            isMoving = true;

            if (moveToPath)
            {
                movementStart.Invoke();
                if (movementStartEvent != null)
                    movementStartEvent();

                agent.SetDestination(waypoints[currentPoint].position);
                yield return StartCoroutine(WaitForDestination());
                moveToPathBool = false;
            }

            if (loopType == LoopType.random)
            {
                RandomizeWaypoints();
                StartCoroutine(NextWaypoint());
                yield break;
            }

            movementStart.Invoke();
            if (movementStartEvent != null)
                movementStartEvent();

            if (moveToPath)
                StartCoroutine(NextWaypoint());
            else
                GoToWaypoint(startPoint);
        }


        private IEnumerator NextWaypoint()
        {
            OnWaypointChange(currentPoint);
            yield return new WaitForEndOfFrame();

            while (waitRoutine != null) yield return null;
            Transform next = null;

            if (loopType == LoopType.random)
            {
                rndIndex++;
                currentPoint = int.Parse(waypoints[rndIndex].name.Replace("Waypoint ", ""));
                next = waypoints[rndIndex];
            }
            else if(reverse)
            {
                currentPoint--;
            }
            else
                currentPoint++;

            currentPoint = Mathf.Clamp(currentPoint, 0, waypoints.Length - 1);
            if (next == null) next = waypoints[currentPoint];

            agent.SetDestination(next.position);
            yield return StartCoroutine(WaitForDestination());

            if (loopType != LoopType.random && currentPoint == waypoints.Length - 1
                || rndIndex == waypoints.Length - 1 || reverse && currentPoint == 0)
            {
                OnWaypointChange(currentPoint);
                StartCoroutine(ReachedEnd());
            }
            else
                StartCoroutine(NextWaypoint());
        }


        private IEnumerator WaitForDestination()
        {
            yield return new WaitForEndOfFrame();         
            while (agent.pathPending)
                yield return null;
            yield return new WaitForEndOfFrame();

            float remain = agent.remainingDistance;
            while (remain == Mathf.Infinity || remain - agent.stoppingDistance > float.Epsilon
            || agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                remain = agent.remainingDistance;
                yield return null;
            }
        }


        private void OnWaypointChange(int index)
        {
            movementChange.Invoke(index);
            if (movementChangeEvent != null)
                movementChangeEvent(index);
        }


        private IEnumerator ReachedEnd()
        {
            movementEnd.Invoke();
            if (movementEndEvent != null)
                movementEndEvent();

            switch (loopType)
            {
                case LoopType.none:
                    isMoving = false;
                    yield break;

                case LoopType.loop:
                    int nextPoint = reverse ? waypoints.Length - 1 : 0;

                    if (closeLoop)
                    {
                        moveToPathBool = true;
                        agent.SetDestination(waypoints[nextPoint].position);
                        yield return StartCoroutine(WaitForDestination());
                        moveToPathBool = false;
                    }
                    else
                        agent.Warp(waypoints[nextPoint].position);

                    currentPoint = nextPoint;

                    movementStart.Invoke();
                    if (movementStartEvent != null)
                        movementStartEvent();
                    break;

                case LoopType.pingPong:
                    reverse = !reverse;

                    if(!reverse)
                    {
                        movementStart.Invoke();
                        if (movementStartEvent != null)
                            movementStartEvent();
                    }
                    break;

                case LoopType.random:
                    RandomizeWaypoints();
                    break;
            }

            StartCoroutine(NextWaypoint());
        }


        private void RandomizeWaypoints()
        {
            Array.Copy(pathContainer.waypoints, waypoints, pathContainer.waypoints.Length);
            int n = waypoints.Length;

            while (n > 1)
            {
                int k = rand.Next(n--);
                Transform temp = waypoints[n];
                waypoints[n] = waypoints[k];
                waypoints[k] = temp;
            }

            Transform first = pathContainer.waypoints[currentPoint];
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == first)
                {
                    Transform temp = waypoints[0];
                    waypoints[0] = waypoints[i];
                    waypoints[i] = temp;
                    break;
                }
            }

            rndIndex = 0;
        }


        public void GoToWaypoint(int index)
        {
            Stop();
            isMoving = true;
            currentPoint = index;
            agent.Warp(waypoints[index].position);
            StartCoroutine(NextWaypoint());
        }


        public void Pause(float seconds = 0f)
        {
            if (waitRoutine != null) StopCoroutine(waitRoutine);
            agent.isStopped = true;

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

            agent.isStopped = false;
        }
        
        
        public void Reverse()
        {
            reverse = !reverse;
            
            if(reverse) startPoint = currentPoint - 1;
            else startPoint = currentPoint + 1;
            
            moveToPathBool = true;
            StartMove();
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
            isMoving = false;

            if (agent.enabled)
            {
                agent.isStopped = true;
            }
        }


        public void ResetToStart()
        {
            Stop();
            currentPoint = 0;
            if (pathContainer)
            {
                agent.Warp(pathContainer.waypoints[currentPoint].position);
            }
        }


        public void ChangeSpeed(float value)
        {
            agent.speed = value;
        }


        public bool IsMoving()
        {
            return isMoving;
        }


        public bool IsMovingToPath()
        {
            return IsMoving() && moveToPathBool;
        }


        public bool IsPaused()
        {
            return waitRoutine != null;
        }
    }
}