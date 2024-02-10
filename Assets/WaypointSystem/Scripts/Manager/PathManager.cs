using System.Collections.Generic;
using UnityEngine;

namespace WaypointSystem
{
    public class PathManager : MonoBehaviour
    {
        public Transform[] waypoints = new Transform[]{};

        public bool drawCurved = true;
        
        public bool drawDirection = true;

        public Color color1 = new Color(1, 0, 1, 0.5f);

        public Color color2 = new Color(1, 235 / 255f, 4 / 255f, 0.5f);

        public Vector3 size = new Vector3(.7f, .7f, .7f);

        public float radius = .4f;

        public bool skipCustomNames = true;

        public GameObject replaceObject;


        void Awake()
        {
            WaypointManager.AddPath(gameObject);
        }


        public void Create(Transform parent = null)
        {
            if (parent == null)
                parent = transform;

            List<Transform> childs = new List<Transform>();
            foreach(Transform child in parent)
                childs.Add(child);

            Create(childs.ToArray());
        }


        public virtual void Create(Transform[] waypoints, bool makeChildren = false)
        {
            if(waypoints.Length < 2)
            {
                Debug.LogWarning("Not enough waypoints placed - minimum is 2. Cancelling.");
                return;
            }

            if(makeChildren)
            {
                for(int i = 0; i < waypoints.Length; i++)
                    waypoints[i].parent = transform;
            }

            this.waypoints = waypoints;
        }


        void OnDrawGizmos()
        {
            if (waypoints.Length <= 0) return;

            Vector3[] wpPositions = GetPathPoints();

            Vector3 start = wpPositions[0];
            Vector3 end = wpPositions[wpPositions.Length - 1];
            Gizmos.color = color1;
            Gizmos.DrawWireCube(start, size * GetHandleSize(start) * 1.5f);
            Gizmos.DrawWireCube(end, size * GetHandleSize(end) * 1.5f);

            Gizmos.color = color2;
            for (int i = 1; i < wpPositions.Length - 1; i++)
                Gizmos.DrawWireSphere(wpPositions[i], radius * GetHandleSize(wpPositions[i]));

            if (drawCurved && wpPositions.Length >= 2)
                WaypointManager.DrawCurved(wpPositions);
            else
                WaypointManager.DrawStraight(wpPositions);
        }


        public virtual float GetHandleSize(Vector3 pos)
        {
            float handleSize = 1f;
            #if UNITY_EDITOR
                handleSize = UnityEditor.HandleUtility.GetHandleSize(pos) * 0.4f;
                handleSize = Mathf.Clamp(handleSize, 0, 1.2f);
            #endif
            return handleSize;
        }


        public virtual Vector3[] GetPathPoints(bool local = false)
        {
            Vector3[] pathPoints = new Vector3[waypoints.Length];

            if (local)
            {
                for (int i = 0; i < waypoints.Length; i++)
                    pathPoints[i] = waypoints[i].localPosition;
            }
            else
            {
                for (int i = 0; i < waypoints.Length; i++)
                    pathPoints[i] = waypoints[i].position;
            }

            return pathPoints;
        }

        
        public virtual Transform GetWaypoint(int index)
        {
            return waypoints[index];
        }
        

		public virtual int GetWaypointIndex(int pathPoint)
		{
			return pathPoint;
		}


        public virtual int GetPathPointIndex(int waypoint)
        {
            return waypoint;
        }


        public virtual int GetWaypointCount()
		{
			return waypoints.Length;
		}
    }
}


