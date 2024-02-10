using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WaypointSystem
{
    [CustomEditor(typeof(WaypointManager))]
    public class WaypointEditor : Editor
    {
        private WaypointManager script;
		public static string pathName = "";
		private bool mode2D = false;

        public static bool placing = false;
        public static GameObject path;
        public static PathManager pathMan;
        private static List<GameObject> wpList = new List<GameObject>();   

        private enum PathType
        {
            standard,
            bezier
        }
        private static PathType pathType = PathType.standard;


        void OnEnable()
        {
            #if !UNITY_2022_2_OR_NEWER
            script = (WaypointManager)target;
            if(script && script.transform.childCount == 0)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.GetComponent<MeshRenderer>().enabled = false;
                obj.GetComponent<Collider>().enabled = false;
                obj.name = "UnityForcesMeToDoThis (bug1394023)";
                obj.transform.localScale = Vector3.zero;
                obj.transform.parent = script.transform;
            }
            #endif
        }


        public void OnSceneGUI()
        {
            if (Event.current.type != EventType.KeyDown || !placing) return;

            if (Event.current.keyCode == script.viewPlacementKey)
            {
                Event.current.Use();
                Vector3 camPos = GetSceneView().camera.transform.position;

                if (pathMan is BezierPathManager)
                    PlaceBezierPoint(camPos);
                else
                    PlaceWaypoint(camPos);

            }
            else if (Event.current.keyCode == script.placementKey)
            {
                Debug.Log("placement key");
                Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hitInfo;

                if (mode2D)
                {
                    Event.current.Use();
                    Vector3 pos2D = worldRay.origin;
                    pos2D.z = 0;

                    if (pathMan is BezierPathManager)
                        PlaceBezierPoint(pos2D);
                    else
                        PlaceWaypoint(pos2D);
                }
                else
                {
                    if (Physics.Raycast(worldRay, out hitInfo))
                    {
                        Event.current.Use();

                        if (pathMan is BezierPathManager)
                            PlaceBezierPoint(hitInfo.point);
                        else
                            PlaceWaypoint(hitInfo.point);
                    }
                    else
                    {
                        Debug.LogWarning("Waypoint Manager: 3D Mode. Trying to place a waypoint but couldn't "
                                         + "find valid target. Have you clicked on a collider?");
                    }
                }
            }
        }


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (WaypointManager)target;
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            SceneView view = GetSceneView();
            mode2D = view.in2DMode;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Enter Path Name: ", GUILayout.Height(15));
            pathName = EditorGUILayout.TextField(pathName, GUILayout.Height(15));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Select Path Type: ", GUILayout.Height(15));
            pathType = (PathType)EditorGUILayout.EnumPopup(pathType);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (mode2D)
                GUILayout.Label("2D Mode Detected.", GUILayout.Height(15));
            else
                GUILayout.Label("3D Mode Detected.", GUILayout.Height(15));
            EditorGUILayout.Space();

            if (!placing && GUILayout.Button("Start Path", GUILayout.Height(40)))
            {
                if (pathName == "")
                {
                    EditorUtility.DisplayDialog("No Path Name", "Please enter a unique name for your path.", "Ok");
                    return;
                }

                if (script.transform.Find(pathName) != null)
                {
                    if(EditorUtility.DisplayDialog("Path Exists Already",
                        "A path with this name exists already.\n\nWould you like to edit it?", "Ok", "Cancel"))
                    {
                        Selection.activeTransform = script.transform.Find(pathName);
                    }
                    return;
                }

                path = new GameObject(pathName);
                path.transform.position = script.gameObject.transform.position;
                path.transform.parent = script.gameObject.transform;
                StartPath();

                placing = true;
                view.Focus();
            }

            GUI.backgroundColor = Color.yellow;

            if (placing && GUILayout.Button("Finish Editing", GUILayout.Height(40)))
            {
				FinishPath();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();
        }


        void OnDisable()
        {
			FinishPath();
        }


        public static void StartPath()
        {
            switch (pathType)
            {
                case PathType.standard:
                    pathMan = path.AddComponent<PathManager>();
                    pathMan.waypoints = new Transform[0];
                    break;
                case PathType.bezier:
                    pathMan = path.AddComponent<BezierPathManager>();
                    BezierPathManager thisPath = pathMan as BezierPathManager;
                    thisPath.showHandles = true;
                    thisPath.bPoints = new List<BezierPoint>();
                    break;
            }
        }


		public static void ContinuePath(PathManager p)
		{
			path = p.gameObject;
			pathMan = p;
			placing = true;

            wpList.Clear();
            if(p is BezierPathManager)
            {
                for (int i = 0; i < (p as BezierPathManager).bPoints.Count; i++)
                    wpList.Add((p as BezierPathManager).bPoints[i].wp.gameObject);
            }
            else
            {
                for (int i = 0; i < p.waypoints.Length; i++)
                    wpList.Add(p.waypoints[i].gameObject);
            }

            GetSceneView().Focus();
        }


        void PlaceWaypoint(Vector3 placePos)
        {
            GameObject wayp = new GameObject("Waypoint");

            Transform[] wpCache = new Transform[pathMan.waypoints.Length];
            System.Array.Copy(pathMan.waypoints, wpCache, pathMan.waypoints.Length);

            pathMan.waypoints = new Transform[pathMan.waypoints.Length + 1];
            System.Array.Copy(wpCache, pathMan.waypoints, wpCache.Length);
            pathMan.waypoints[pathMan.waypoints.Length - 1] = wayp.transform;

            if (wpList.Count == 0)
                pathMan.transform.position = placePos;

            if (mode2D) placePos.z = 0f;
            wayp.transform.position = placePos;
            wayp.transform.rotation = Quaternion.Euler(-90, 0, 0);
            wayp.transform.parent = pathMan.transform;
            wpList.Add(wayp);
            wayp.name = "Waypoint " + (wpList.Count - 1);
        }


        void PlaceBezierPoint(Vector3 placePos)
        {
            BezierPoint newPoint = new BezierPoint();

            Transform wayp = new GameObject("Waypoint").transform;
            newPoint.wp = wayp;

            if (wpList.Count == 0)
                pathMan.transform.position = placePos;

            if (mode2D) placePos.z = 0f;
            wayp.position = placePos;
            wayp.transform.rotation = Quaternion.Euler(-90, 0, 0);
            wayp.parent = pathMan.transform;

            BezierPathManager thisPath = pathMan as BezierPathManager;
            Transform left = new GameObject("Left").transform;
            Transform right = new GameObject("Right").transform;
            left.parent = right.parent = wayp;

            Vector3 handleOffset = new Vector3(2, 0, 0);
            Vector3 targetDir = Vector3.zero;
            int lastIndex = wpList.Count - 1;

            left.position = wayp.position + wayp.rotation * handleOffset;
            right.position = wayp.position + wayp.rotation * -handleOffset;
            newPoint.cp = new[] { left, right };

            if (wpList.Count == 1)
            {
                targetDir = (wayp.position - wpList[0].transform.position).normalized;
                thisPath.bPoints[0].cp[1].localPosition = targetDir * 2;
            }
            else if (wpList.Count >= 1)
            {
                targetDir = (wpList[lastIndex].transform.position - wayp.position);
                wayp.transform.rotation = Quaternion.LookRotation(targetDir) * Quaternion.Euler(0, -90, 0);
            }
            

            if (wpList.Count >= 2)
            {
                BezierPoint lastPoint = thisPath.bPoints[lastIndex];
                targetDir = (wayp.position - wpList[lastIndex].transform.position) +
                                    (wpList[lastIndex - 1].transform.position - wpList[lastIndex].transform.position);

                Quaternion lookRot = Quaternion.LookRotation(targetDir);
                if (mode2D)
                {
                    float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg + 90;
                    lookRot = Quaternion.AngleAxis(angle, Vector3.forward);
                }
                lastPoint.wp.rotation = lookRot;

                Vector3 leftPos = lastPoint.cp[0].position;
                Vector3 preLastPos = wpList[lastIndex - 1].transform.position;

                if (Vector3.Distance(leftPos, preLastPos) > Vector3.Distance(lastPoint.cp[1].position, preLastPos))
                {
                    lastPoint.cp[0].position = lastPoint.cp[1].position;
                    lastPoint.cp[1].position = leftPos;
                }
            }

            thisPath.bPoints.Add(newPoint);
            thisPath.segmentDetail.Add(thisPath.pathDetail);
            wpList.Add(wayp.gameObject);
            wayp.name = "Waypoint " + (wpList.Count - 1);
            thisPath.CalculatePath();
        }


		public static void FinishPath()
		{
			if (!placing) return;

			if (wpList.Count < 2)
			{
				Debug.LogWarning("Not enough waypoints placed. Cancelling.");
				if (path) DestroyImmediate(path);
			}
			
			placing = false;
			wpList.Clear();
			pathName = "";
			Selection.activeGameObject = path;
		}


        public static SceneView GetSceneView()
        {
            EditorWindow window = EditorWindow.mouseOverWindow;

            if (window is SceneView)
                return window as SceneView;

            if (SceneView.lastActiveSceneView == null)
                return SceneView.sceneViews[0] as SceneView;

            return SceneView.lastActiveSceneView;
        }
    }
}
