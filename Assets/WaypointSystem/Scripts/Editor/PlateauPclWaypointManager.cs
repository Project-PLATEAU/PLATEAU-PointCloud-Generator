using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR


namespace WaypointSystem
{
    public class PlateauPclWaypointManager : EditorWindow
    {
        private GameObject waypointManagerObject;

        private WaypointManager script;

        private GameObject vehicleObject;
        private splineMove splineMoveScript;

        private string pathName;

        [MenuItem("PLATEAU PCL Generator/Waypoint設定", false, 2)]
        static void Init()
        {
            PlateauPclWaypointManager window = (PlateauPclWaypointManager)EditorWindow.GetWindow(typeof(PlateauPclWaypointManager));
            window.Show(); 
        }

        private void CreateGUI() {
            waypointManagerObject = GameObject.Find("WaypointManager");
            script = waypointManagerObject.GetComponent<WaypointManager>();
        }

        private void OnGUI() {
            GUILayout.Label("Waypoint設定", EditorStyles.boldLabel);
            pathName = EditorGUILayout.TextField("ルート名", pathName);
            GUILayout.Label("");
            if(!WaypointEditor.placing && GUILayout.Button("パス作成開始", GUILayout.Height(30))){
                WaypointEditor.pathName = pathName;
                if (WaypointEditor.pathName == "")
                {
                    EditorUtility.DisplayDialog("エラー ルート名なし", "有効なルート名を入力してください", "Ok");
                    return;
                }

                if (script.transform.Find(WaypointEditor.pathName) != null)
                {
                    if(EditorUtility.DisplayDialog("Path Exists Already",
                        "A path with this name exists already.\n\nWould you like to edit it?", "Ok", "Cancel"))
                    {
                        Selection.activeTransform = script.transform.Find(WaypointEditor.pathName);
                    }
                    return;
                }

                //create a new container transform which will hold all new waypoints
                WaypointEditor.path = new GameObject(WaypointEditor.pathName);
                //reset position and parent container gameobject to this manager gameobject
                WaypointEditor.path.transform.position = script.gameObject.transform.position;
                WaypointEditor.path.transform.parent = script.gameObject.transform;
                WaypointEditor.StartPath();

                // //we passed all prior checks, toggle waypoint placement
                WaypointEditor.placing = true;

                Selection.activeGameObject = waypointManagerObject;

                SceneView view = WaypointEditor.GetSceneView();

                view.Focus();
            }

            GUI.backgroundColor = Color.yellow;

            //finish path button
            if (WaypointEditor.placing && GUILayout.Button("パス編集を終了", GUILayout.Height(30)))
            {
				WaypointEditor.FinishPath();

                // vehicleObject = GameObject.Find("Vehicle");
                // splineMoveScript = vehicleObject.GetComponent<splineMove>();
                // splineMoveScript.pathContainer = GameObject.Find(WaypointEditor.pathName).GetComponent<PathManager>();
            }

            GUI.backgroundColor = Color.white;

            GUILayout.Label("");

            if (GUILayout.Button("パスを設定", GUILayout.Height(30)))
            {
				vehicleObject = GameObject.Find("Vehicle");
                splineMoveScript = vehicleObject.GetComponent<splineMove>();
                splineMoveScript.pathContainer = GameObject.Find(pathName).GetComponent<PathManager>();


            }


        }
    }
}

#endif