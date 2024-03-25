using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaypointSystem
{
    public class BezierPathManager : PathManager
    {
        public Vector3[] pathPoints = new Vector3[]{};

        public List<BezierPoint> bPoints = new List<BezierPoint>();

        public bool showHandles = true;

        public bool connectHandles = true;

        public Color color3 = new Color(108 / 255f, 151 / 255f, 1, 1);

        public float pathDetail = 1;

        public bool customDetail = false;

        public List<float> segmentDetail = new List<float>();


        void Awake()
        {
            WaypointManager.AddPath(gameObject);

            if (bPoints == null || bPoints.Count == 0)
                return;

            CalculatePath();
        }


        public override void Create(Transform[] waypoints, bool makeChildren = false)
        {
            if (waypoints.Length < 2)
            {
                Debug.LogWarning("Not enough waypoints placed - minimum is 2. Cancelling.");
                return;
            }

            if (makeChildren)
            {
                for (int i = 0; i < waypoints.Length; i++)
                    waypoints[i].parent = transform;
            }

            bPoints.Clear();
            for(int i = 0; i < waypoints.Length; i++)
            {
                BezierPoint point = new BezierPoint();
                point.wp = waypoints[i];
                point.cp = new Transform[2];
                point.cp[0] = point.wp.GetChild(0);
                point.cp[1] = point.wp.GetChild(1);
                bPoints.Add(point);
            }

            CalculatePath();
        }


        void OnDrawGizmos()
        {
            if (bPoints.Count <= 0) return;

            Vector3 start = bPoints[0].wp.position;
            Vector3 end = bPoints[bPoints.Count - 1].wp.position;
            Gizmos.color = color1;
            Gizmos.DrawWireCube(start, size * GetHandleSize(start) * 1.5f);
            Gizmos.DrawWireCube(end, size * GetHandleSize(end) * 1.5f);

            Gizmos.color = color2;
            for (int i = 1; i < bPoints.Count - 1; i++)
                Gizmos.DrawWireSphere(bPoints[i].wp.position, radius * GetHandleSize(bPoints[i].wp.position));

            if (drawCurved && bPoints.Count >= 2)
                WaypointManager.DrawCurved(pathPoints);
            else
                WaypointManager.DrawStraight(pathPoints);
        }


        public override Vector3[] GetPathPoints(bool local = false)
        {
            Vector3[] copy = new Vector3[pathPoints.Length];

            if(local)
            {
                for (int i = 0; i < copy.Length; i++)
                    copy[i] = transform.InverseTransformPoint(pathPoints[i]);
            }
            else
                System.Array.Copy(pathPoints, copy, pathPoints.Length);

            return copy;
        }


        public override int GetWaypointCount()
		{
			return bPoints.Count;
		}
        
        
        public override Transform GetWaypoint(int index)
        {
            return bPoints[index].wp;
        }


        public override int GetWaypointIndex(int pathPoint)
		{
            int index = -1;
            int summedPoints = 0;
            int defaultPoints = 10;

            for(int i = 0; i < segmentDetail.Count; i++)
            {
                if(pathPoint == summedPoints)
                {
                    index = i;
                    break;
                }

                if (customDetail) summedPoints += Mathf.CeilToInt(segmentDetail[i] * defaultPoints);
                else summedPoints += Mathf.CeilToInt(pathDetail * defaultPoints);
            }

            return index;
        }


        public override int GetPathPointIndex(int waypoint)
        {
            int summedPoints = 0;
            int defaultPoints = 10;

            for (int i = 0; i < segmentDetail.Count; i++)
            {
                if (i == waypoint)
                    break;

                if (customDetail) summedPoints += Mathf.CeilToInt(segmentDetail[i] * defaultPoints);
                else summedPoints += Mathf.CeilToInt(pathDetail * defaultPoints);
            }

            return summedPoints;
        }


        public void CalculatePath()
        {
            List<Vector3> temp = new List<Vector3>();
            List<Transform> tempWps = new List<Transform>();

            for (int i = 0; i < bPoints.Count - 1; i++)
            {
                BezierPoint bp = bPoints[i];
                float detail = pathDetail;
                if (customDetail)
                    detail = segmentDetail[i];
                temp.AddRange(GetPoints(bp.wp.position,
                                bp.cp[1].position,
                                bPoints[i + 1].cp[0].position,
                                bPoints[i + 1].wp.position,
                                detail));

                tempWps.Add(bp.wp);
            }
            tempWps.Add(bPoints[bPoints.Count - 1].wp);

            pathPoints = temp.Distinct().ToArray();
            waypoints = tempWps.ToArray();
        }


        private List<Vector3> GetPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float detail)
        {
            List<Vector3> segmentPoints = new List<Vector3>();
            float iterations = detail * 10f;
            for (int n = 0; n <= iterations; n++)
            {
                float i = (float)n / iterations;
                float rest = (1f - i);
                Vector3 newPos = Vector3.zero;
                newPos += p0 * rest * rest * rest;
                newPos += p1 * i * 3f * rest * rest;
                newPos += p2 * 3f * i * i * rest;
                newPos += p3 * i * i * i;
                segmentPoints.Add(newPos);
            }
            return segmentPoints;
        }
    }


    [System.Serializable]
    public class BezierPoint
    {
        public Transform wp = null;

        public Transform[] cp = new Transform[2];
    }
}