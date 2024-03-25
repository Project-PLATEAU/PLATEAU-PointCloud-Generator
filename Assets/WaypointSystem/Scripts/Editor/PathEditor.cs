using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WaypointSystem
{
    [CustomEditor(typeof(PathManager))]
    public class PathEditor : Editor
    {
        private SerializedObject m_Object;
        private SerializedProperty m_Waypoint;
        private SerializedProperty m_WaypointsCount;
        private SerializedProperty m_Check1;
        private SerializedProperty m_Check2;
        private SerializedProperty m_Color1;
        private SerializedProperty m_Color2;
        private SerializedProperty m_SkipNames;
        private SerializedProperty m_WaypointPref;

        private static string wpArraySize = "waypoints.Array.size";
        private static string wpArrayData = "waypoints.Array.data[{0}]";
        private List<int> activeNode = new List<int>();
        private PathModifierOption editOption = PathModifierOption.SelectModifier;
        private double lastMouseClickTime = 1;
        private bool isControlPressed = false;

        public void OnEnable()
        {
            m_Object = new SerializedObject(target);

            m_Check1 = m_Object.FindProperty("drawCurved");
            m_Check2 = m_Object.FindProperty("drawDirection");
            m_Color1 = m_Object.FindProperty("color1");
            m_Color2 = m_Object.FindProperty("color2");
            m_SkipNames = m_Object.FindProperty("skipCustomNames");
            m_WaypointPref = m_Object.FindProperty("replaceObject");

            m_WaypointsCount = m_Object.FindProperty(wpArraySize);
            isControlPressed = false;
            activeNode.Clear();
        }


        private Transform[] GetWaypointArray()
        {
            var arrayCount = m_Object.FindProperty(wpArraySize).intValue;
            var transformArray = new Transform[arrayCount];
            for (var i = 0; i < arrayCount; i++)
            {
                transformArray[i] = m_Object.FindProperty(string.Format(wpArrayData, i)).objectReferenceValue as Transform;
            }
            return transformArray;
        }


        private void SetWaypoint(int index, Transform waypoint)
        {
            activeNode.Clear();
            
            m_Object.FindProperty(string.Format(wpArrayData, index)).objectReferenceValue = waypoint;
        }


        private Transform GetWaypointAtIndex(int index)
        {
            return m_Object.FindProperty(string.Format(wpArrayData, index)).objectReferenceValue as Transform;
        }


        private void RemoveWaypointAtIndex(int index)
        {
            Undo.DestroyObjectImmediate(GetWaypointAtIndex(index).gameObject);

            for (int i = index; i < m_WaypointsCount.intValue - 1; i++)
                SetWaypoint(i, GetWaypointAtIndex(i + 1));

            m_WaypointsCount.intValue--;
			RenameWaypoints(GetWaypointArray(), true);
        }


        private void AddWaypointAtIndex(int index)
        {
            m_WaypointsCount.intValue++;

            for (int i = m_WaypointsCount.intValue - 1; i > index; i--)
                SetWaypoint(i, GetWaypointAtIndex(i - 1));

            GameObject wp = new GameObject("Waypoint " + (index + 1));
            Undo.RegisterCreatedObjectUndo(wp, "Created WP");

            wp.transform.position = GetWaypointAtIndex(index).position;
			wp.transform.SetParent(GetWaypointAtIndex(index).parent);
			wp.transform.SetSiblingIndex(index + 1);
            SetWaypoint(index + 1, wp.transform);
			RenameWaypoints(GetWaypointArray(), true);
            activeNode.Clear();
            activeNode.Add(index + 1);
        }


        public override void OnInspectorGUI()
        {
            m_Object.Update();

            var waypoints = GetWaypointArray();

            if (m_WaypointsCount.intValue < 2)
            {
                if (GUILayout.Button("Create Path from Children"))
                {
                    Undo.RecordObjects(waypoints, "Create Path");
                    (m_Object.targetObject as PathManager).Create();
                    SceneView.RepaintAll();
                }

                return;
            }

            m_Check1.boolValue = EditorGUILayout.Toggle("Draw Smooth Lines", m_Check1.boolValue);
            m_Check2.boolValue = EditorGUILayout.Toggle("Draw Direction", m_Check2.boolValue);

            EditorGUILayout.PropertyField(m_Color1);
            EditorGUILayout.PropertyField(m_Color2);

            Vector3[] wpPositions = new Vector3[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
                wpPositions[i] = waypoints[i].position;
                
            float pathLength = WaypointManager.GetPathLength(wpPositions);
            GUILayout.Label("Path Length: " + pathLength);

			if (GUILayout.Button("Continue Editing")) 
			{
				Selection.activeGameObject = (GameObject.FindObjectOfType(typeof(WaypointManager)) as WaypointManager).gameObject;
				WaypointEditor.ContinuePath(m_Object.targetObject as PathManager);
			}

			DrawPathOptions();
			EditorGUILayout.Space();

            GUILayout.Label("Waypoints: ", EditorStyles.boldLabel);

            for (int i = 0; i < waypoints.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i + ".", GUILayout.Width(20));
                EditorGUILayout.ObjectField(waypoints[i], typeof(Transform), true);

                if (i < waypoints.Length && GUILayout.Button("+", GUILayout.Width(30f)))
                {
                    AddWaypointAtIndex(i);
                    break;
                }

                if (i > 0 && i < waypoints.Length - 1 && GUILayout.Button("-", GUILayout.Width(30f)))
                {
                    RemoveWaypointAtIndex(i);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            m_Object.ApplyModifiedProperties();
        }


        void OnSceneGUI()
        {
            var waypoints = GetWaypointArray();
            if (waypoints.Length == 0) return;
            Vector3 wpPos = Vector3.zero;
            float size = 1f;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.modifiers == EventModifiers.Control)
                isControlPressed = true;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (!waypoints[i]) continue;
                wpPos = waypoints[i].position;
                
                size = HandleUtility.GetHandleSize(wpPos) * 0.4f;

                if (size < 3f)
                {
                    Handles.BeginGUI();
                    var guiPoint = HandleUtility.WorldToGUIPoint(wpPos);
                    var rect = new Rect(guiPoint.x - 50.0f, guiPoint.y - 40, 100, 20);
                    GUI.Box(rect, waypoints[i].name);
                    Handles.EndGUI(); //end GUI block
                }

                Handles.color = m_Color2.colorValue;
                size = Mathf.Clamp(size, 0, 1.2f);

                Handles.FreeMoveHandle(wpPos, Quaternion.identity, size, Vector3.zero, (controlID, position, rotation, hSize, eventType) => 
                {
                    Handles.SphereHandleCap(controlID, position, rotation, hSize, eventType);
                    if (Event.current.type == EventType.Layout && GUIUtility.hotControl != 0 && controlID == GUIUtility.hotControl)
                    {
                        if (EditorApplication.timeSinceStartup - lastMouseClickTime < 0.5f)
                            return;

                        if (isControlPressed)
                        {
                            if (activeNode.Contains(i))
                                activeNode.Remove(i);
                            else
                                activeNode.Add(i);
                        }
                        else
                        {
                            activeNode.Clear();
                            if (!activeNode.Contains(i))
                                activeNode.Add(i);
                        }

                        lastMouseClickTime = EditorApplication.timeSinceStartup;
                        isControlPressed = false;
                    }
                });
                Handles.RadiusHandle(waypoints[i].rotation, wpPos, size / 2);
            }

            for (int i = 0; i < activeNode.Count; i++)
            {
                wpPos = waypoints[activeNode[i]].position;
                Quaternion wpRot = waypoints[activeNode[i]].rotation;
                switch(Tools.current)
                {
                    case Tool.Move:
                        if(Tools.pivotRotation == PivotRotation.Global)
                            wpRot = Quaternion.identity;
                            
                        Vector3 newPos = Handles.PositionHandle(wpPos, wpRot);
                        Vector3 offset = newPos - wpPos;

                        if(offset != Vector3.zero)
                        {
                            Undo.RecordObjects(waypoints, "Move Handle");

                            for(int j = 0; j < activeNode.Count; j++)
                                waypoints[activeNode[j]].position += offset;
                        }
                        break;

                    case Tool.Rotate:
                        Quaternion newRot = Handles.RotationHandle(wpRot, wpPos);

                        if(wpRot != newRot) 
                        {
                            Undo.RecordObject(waypoints[activeNode[i]], "Rotate Handle");
                            waypoints[activeNode[i]].rotation = newRot;
                        }
                        break;
                }
            }
            
            if(!m_Check2.boolValue) return;
            Vector3[] pathPoints = new Vector3[waypoints.Length];           
            for(int i = 0; i < pathPoints.Length; i++)
                pathPoints[i] = waypoints[i].position;
            
            List<List<Vector3>> segments = new List<List<Vector3>>();
            int curIndex = 0;
            float lerpVal = 0f;
            
            switch(m_Check1.boolValue)
            {
                case true:
                    pathPoints = WaypointManager.GetCurved(pathPoints);
                    int detail = Mathf.FloorToInt((pathPoints.Length - 1f) / (waypoints.Length - 1f));
                    
                    for(int i = 0; i < waypoints.Length - 1; i++)
                    {
                        float dist = Mathf.Infinity;
                        segments.Add(new List<Vector3>());
                        
                        for(int j = curIndex; j < pathPoints.Length; j++)
                        {
                            segments[i].Add(pathPoints[j]);
                            
                            if(j >= (i+1) * detail)
                            {
                                float pointDist = Vector3.Distance(waypoints[i].position, pathPoints[j]);
                                if(pointDist < dist)
                                   dist = pointDist;
                                else
                                {
                                   curIndex = j + 1;
                                   break;
                                }
                            }
                        }
                    }
                    break;
                 
                case false:
                    int lerpMax = 16;
                    for(int i = 0; i < waypoints.Length - 1; i++)
                    {
                        segments.Add(new List<Vector3>());
                        for(int j = 0; j < lerpMax; j++)
                        {
                            segments[i].Add(Vector3.Lerp(pathPoints[i], pathPoints[i+1], j / (float)lerpMax));
                        }
                    }
                    break;
            }

            for(int i = 0; i < segments.Count; i++)
            {
                for(int j = 0; j < segments[i].Count; j++)
                {
                    size = Mathf.Clamp(HandleUtility.GetHandleSize(segments[i][j]) * 0.4f, 0, 1.2f);
                    lerpVal = j / (float)segments[i].Count;
                    Handles.ArrowHandleCap(0, segments[i][j], Quaternion.Lerp(waypoints[i].rotation, waypoints[i + 1].rotation, lerpVal), size, EventType.Repaint);
                }
            }
        }


		private void DrawPathOptions()
		{
			Transform[] waypoints = GetWaypointArray();
			editOption = (PathModifierOption)EditorGUILayout.EnumPopup(editOption);
			
			switch (editOption) 
			{
				case PathModifierOption.PlaceToGround:
					foreach (Transform trans in waypoints) 
					{
						Ray ray = new Ray (trans.position + new Vector3 (0, 2f, 0), -Vector3.up);
						Undo.RecordObject (trans, "Place To Ground");
						
						RaycastHit hit;
						if (Physics.Raycast (ray, out hit, 100)) {
							trans.position = hit.point;
						}
						
						RaycastHit2D hit2D = Physics2D.Raycast (ray.origin, -Vector2.up, 100);
						if (hit2D) {
							trans.position = new Vector3 (hit2D.point.x, hit2D.point.y, trans.position.z);
						}
					}
					break;
				
				case PathModifierOption.InvertDirection:
					Undo.RecordObjects(waypoints, "Invert Direction");
					
					Vector3[] waypointCopy = new Vector3[waypoints.Length];
					for (int i = 0; i < waypoints.Length; i++)
						waypointCopy[i] = waypoints[i].position;
					
					for (int i = 0; i < waypoints.Length; i++)
						waypoints[i].position = waypointCopy[waypointCopy.Length - 1 - i];
					
					break;
				
				case PathModifierOption.RotateWaypointsToPath:
					Undo.RecordObjects(waypoints, "Rotate Waypoints");
					
					for(int i = 0; i < waypoints.Length - 1; i++)
						waypoints[i].LookAt(waypoints[i+1]);
					
					waypoints[waypoints.Length - 1].rotation = waypoints[waypoints.Length - 2].rotation;
					break;
				
				case PathModifierOption.RenameWaypoints:
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("Skip Custom Names?");
					m_SkipNames.boolValue = EditorGUILayout.Toggle(m_SkipNames.boolValue, GUILayout.Width(20));
					EditorGUILayout.EndHorizontal();	
					
					if(!GUILayout.Button("Rename Now"))
					{
						return;
					}

					RenameWaypoints(waypoints, m_SkipNames.boolValue);
					break;
				
				case PathModifierOption.UpdateFromChildren:
					Undo.RecordObjects(waypoints, "Update Path From Children");
					(m_Object.targetObject as PathManager).Create();
					SceneView.RepaintAll();
					break;
				
				case PathModifierOption.ReplaceWaypointObject:
					EditorGUILayout.PropertyField(m_WaypointPref);
					
					if (!GUILayout.Button("Replace Now")) return;
					else if (m_WaypointPref == null || m_WaypointPref.objectReferenceValue == null)
					{
						Debug.LogWarning("No replace object set. Cancelling.");
						return;
					}
					
					var waypointPrefab = m_WaypointPref.objectReferenceValue as GameObject;
					var path = GetWaypointAtIndex(0).parent;
                    Undo.RegisterFullObjectHierarchyUndo(path, "Replace Object");

                    for (int i = 0; i < m_WaypointsCount.intValue; i++)
					{
						Transform curWP = GetWaypointAtIndex(i);
						Transform newCur = ((GameObject)Instantiate(waypointPrefab, curWP.position, Quaternion.identity)).transform;
						
						newCur.parent = path;
						SetWaypoint(i, newCur);
						
						Undo.DestroyObjectImmediate(curWP.gameObject);
					}
					break;
			}
			
			editOption = PathModifierOption.SelectModifier;
		}


		private void RenameWaypoints(Transform[] waypoints, bool skipCustom)
		{
			string wpName = string.Empty;
			string[] nameSplit;
			for (int i = 0; i < waypoints.Length; i++)
			{
				wpName = waypoints[i].name;
				nameSplit = wpName.Split(' ');
				
				if(!skipCustom)
					wpName = "Waypoint " + i;
				else if (nameSplit.Length == 2 && nameSplit[0] == "Waypoint")
				{
					int index;
					if (int.TryParse(nameSplit[1], out index))
					{
						wpName = nameSplit[0] + " " + i;
					}
				}
				
				waypoints[i].name = wpName;
			}
		}
	}

    enum PathModifierOption
    {
        SelectModifier,
        PlaceToGround,
        InvertDirection,
        RotateWaypointsToPath,
        RenameWaypoints,
        UpdateFromChildren,
        ReplaceWaypointObject
    }
}