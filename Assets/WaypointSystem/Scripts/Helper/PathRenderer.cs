using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaypointSystem
{
    [RequireComponent(typeof(LineRenderer))]
    public class PathRenderer : MonoBehaviour
    {
        public bool onUpdate = false;
        public float spacing = 0.05f;

        private PathManager path;
        private LineRenderer line;
        private Vector3[] points;


        void Start()
        {
            line = GetComponent<LineRenderer>();
            path = GetComponent<PathManager>();
            if (path) StartCoroutine("StartRenderer");
        }


        IEnumerator StartRenderer()
        {
            Render();

            if (onUpdate)
            {
                while (true)
                {
                    yield return null;
                    Render();
                }
            }
        }


        void Render()
        {
            spacing = Mathf.Clamp01(spacing);
            if (spacing == 0) spacing = 0.05f;

            List<Vector3> list = new List<Vector3>();
            list.AddRange(path.GetPathPoints());

            if (path.drawCurved)
            {
                list.Insert(0, list[0]);
                list.Add(list[list.Count - 1]);
                points = list.ToArray();
                DrawCurved();
            }
            else
            {
                points = list.ToArray();
                DrawLinear();
            }
        }


        void DrawCurved()
        {
            int size = Mathf.RoundToInt(1f / spacing) + 1;
            line.positionCount = size;
            float t = 0f;
            int i = 0;

            while (i < size)
            {
                line.SetPosition(i, WaypointManager.GetPoint(points, t));
                t += spacing;
                i++;
            }
        }


        void DrawLinear()
        {
            line.positionCount = points.Length;
            float t = 0f;
            int i = 0;

            while (i < points.Length)
            {
                line.SetPosition(i, points[i]);
                t += spacing;
                i++;
            }
        }
    }
}
