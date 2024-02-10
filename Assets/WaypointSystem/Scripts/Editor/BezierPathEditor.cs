using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WaypointSystem
{
    [CustomEditor(typeof(BezierPathManager))]
    public class BezierPathEditor : Editor
    {
        private BezierPathManager script;
        private bool showDetailSettings = false;
        private Vector2 scrollPosDetail;
        private int activeNode = -1;
        private PathModifierOption editOption = PathModifierOption.SelectModifier;


        public void OnEnable()
        {
            script = (BezierPathManager)target;
            if (script.bPoints.Count == 0) return;

            BezierPoint first = script.bPoints[0];
            first.cp[0].position = first.wp.position;
            BezierPoint last = script.bPoints[script.bPoints.Count - 1];
            last.cp[1].position = last.wp.position;

            script.CalculatePath();
        }


        private void AddWaypointAtIndex(int index)
        {
            BezierPoint point = new BezierPoint();
            Transform wp = new GameObject("Waypoint " + (index + 1)).transform;


            wp.position = script.bPoints[index].wp.position;
            point.wp = wp;
            
            Transform left = new GameObject("Left").transform;
            Transform right = new GameObject("Right").transform;
            left.parent = right.parent = wp;

            left.position = wp.position;
            if(index != 0)
                left.position += new Vector3(2, 0, 0);
            right.position = wp.position;
            if(index + 1 != script.bPoints.Count)
                right.position += new Vector3(-2, 0, 0);

            point.cp = new[] { left, right };
            wp.parent = script.transform;
            wp.SetSiblingIndex(index + 1);
            script.segmentDetail.Insert(index + 1, script.pathDetail);
            script.bPoints.Insert(index + 1, point);
            RenameWaypoints(true);
            activeNode = index + 1;
        }


        private void RemoveWaypointAtIndex(int index)
        {
            activeNode = -1;
            Undo.RecordObject(script, "Remove Waypoint");
            script.segmentDetail.RemoveAt(index - 1);
            Undo.DestroyObjectImmediate(script.bPoints[index].wp.gameObject);
            script.bPoints.RemoveAt(index);
            RenameWaypoints(true);
        }


        public override void OnInspectorGUI()
        {
            if (script.bPoints.Count < 2)
            {
                if (GUILayout.Button("Create Path from Children"))
                {
                    Undo.RecordObject(script, "Create Path");
                    script.Create();
                    SceneView.RepaintAll();
                }

                return;
            }

            script.showHandles = EditorGUILayout.Toggle("Show Handles", script.showHandles);
            script.connectHandles = EditorGUILayout.Toggle("Connect Handles", script.connectHandles);
            script.drawCurved = EditorGUILayout.Toggle("Draw Smooth Lines", script.drawCurved);
            script.drawDirection = EditorGUILayout.Toggle("Draw Direction", script.drawDirection);

            script.color1 = EditorGUILayout.ColorField("Color1", script.color1);
            script.color2 = EditorGUILayout.ColorField("Color2", script.color2);
            script.color3 = EditorGUILayout.ColorField("Color3", script.color3);

            float pathLength = WaypointManager.GetPathLength(script.pathPoints);
            GUILayout.Label("Path Length: " + pathLength);

            float thisDetail = script.pathDetail;
            script.pathDetail = EditorGUILayout.Slider("Path Detail", script.pathDetail, 0.5f, 10);
            script.pathDetail = Mathf.Round(script.pathDetail * 10f) / 10f;
            if (thisDetail != script.pathDetail)
                script.customDetail = false;
            DetailSettings();

            if (GUILayout.Button("Continue Editing"))
            {
                Selection.activeGameObject = (GameObject.FindObjectOfType(typeof(WaypointManager)) as WaypointManager).gameObject;
                WaypointEditor.ContinuePath(script);
            }

            DrawPathOptions();
            EditorGUILayout.Space();

            GUILayout.Label("Waypoints: ", EditorStyles.boldLabel);

            for (int i = 0; i < script.bPoints.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i + ".", GUILayout.Width(20));

                EditorGUILayout.ObjectField(script.bPoints[i].wp, typeof(Transform), true);

                if (i < script.bPoints.Count && GUILayout.Button("+", GUILayout.Width(30f)))
                {
                    AddWaypointAtIndex(i);
                    break;
                }

                if (i > 0 && i < script.bPoints.Count - 1 && GUILayout.Button("-", GUILayout.Width(30f)))
                {
                    RemoveWaypointAtIndex(i);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            if (GUI.changed)
            {
                script.CalculatePath();
                EditorUtility.SetDirty(target);
            }
        }


        private void DetailSettings()
        {
            if (showDetailSettings)
            {
                if (GUILayout.Button("Hide Detail Settings"))
                    showDetailSettings = false;

                GUILayout.Label("Segment Detail:", EditorStyles.boldLabel);
                script.customDetail = EditorGUILayout.Toggle("Enable Custom", script.customDetail);

                EditorGUILayout.BeginHorizontal();
                scrollPosDetail = EditorGUILayout.BeginScrollView(scrollPosDetail, GUILayout.Height(105));

                for (int i = 0; i < script.bPoints.Count - 1; i++)
                {
                    float thisDetail = script.segmentDetail[i];
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(i + "-" + (i + 1) + ".");
                    script.segmentDetail[i] = EditorGUILayout.Slider(script.segmentDetail[i], 0.5f, 10);
                    script.segmentDetail[i] = Mathf.Round(script.segmentDetail[i] * 10f) / 10f;
                    EditorGUILayout.EndHorizontal();
                    if (thisDetail != script.segmentDetail[i])
                        script.customDetail = true;
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Show Detail Settings"))
                    showDetailSettings = true;
            }
        }


        private void ReplaceWaypoints()
        {
            if (script.replaceObject == null)
            {
                Debug.LogWarning("You haven't specified a replace object. Cancelling.");
                return;
            }

            Undo.RecordObject(script, "Replace");

            List<GameObject> toRemove = new List<GameObject>();
            for (int i = 0; i < script.bPoints.Count; i++)
            {
                BezierPoint point = script.bPoints[i];
                Transform curWP = point.wp;
                Transform newCur = ((GameObject)Instantiate(script.replaceObject, curWP.position, Quaternion.identity)).transform;
                Undo.RegisterCreatedObjectUndo(newCur.gameObject, "Replace");
                
                Undo.SetTransformParent(point.cp[0], newCur, "Replace");
                Undo.SetTransformParent(point.cp[1], newCur, "Replace");
                newCur.parent = point.wp.parent;
                
                script.bPoints[i].wp = newCur;
                toRemove.Add(curWP.gameObject);
            }

            for (int i = 0; i < toRemove.Count; i++)
                Undo.DestroyObjectImmediate(toRemove[i]);
        }


        void OnSceneGUI()
        {
            if (script.bPoints.Count == 0) return;
            Vector3 wpPos = Vector3.zero;
            float size = 1f;

            for (int i = 0; i < script.bPoints.Count; i++)
            {
                BezierPoint point = script.bPoints[i];
                if (point == null || !point.wp) continue;
                wpPos = point.wp.position;
                size = HandleUtility.GetHandleSize(wpPos) * 0.4f;

                if (size < 3f)
                {
                    Handles.BeginGUI();
                    var guiPoint = HandleUtility.WorldToGUIPoint(wpPos);
                    var rect = new Rect(guiPoint.x - 50.0f, guiPoint.y - 40, 100, 20);
                    GUI.Box(rect, point.wp.name);
                    Handles.EndGUI(); //end GUI block
                }

                Handles.color = script.color2;
                size = Mathf.Clamp(size, 0, 1.2f);
                
                Handles.FreeMoveHandle(wpPos, Quaternion.identity, size, Vector3.zero, (controlID, position, rotation, hSize, eventType) => 
                {
                    Handles.SphereHandleCap(controlID, position, rotation, hSize, eventType);
                    if(controlID == GUIUtility.hotControl && GUIUtility.hotControl != 0)
                        activeNode = i;
                });
                Handles.RadiusHandle(point.wp.rotation, wpPos, size / 2);
            }
            
            if(activeNode > -1)
            {
                BezierPoint point = script.bPoints[activeNode];
                Handles.color = script.color3;


                Quaternion wpRot = script.bPoints[activeNode].wp.rotation;
                switch(Tools.current)
                {
                    case Tool.Move:
                        for (int i = 0; i <= 1; i++)
                        {
                            if (i == 0 && activeNode == 0) continue;
                            if (i == 1 && activeNode == script.bPoints.Count - 1) continue;

                            size = HandleUtility.GetHandleSize(point.cp[i].position) * 0.25f;
                            size = Mathf.Clamp(size, 0, 0.5f);
                            wpPos = point.cp[i].position;
                            
                            Handles.SphereHandleCap(activeNode, wpPos, Quaternion.identity, size, EventType.Repaint);
                            wpPos = Handles.PositionHandle(wpPos, Quaternion.identity);
                            if (Vector3.Distance(point.cp[i].position, wpPos) > 0.01f)
                            {
                                Undo.RecordObject(point.cp[i].transform, "Move Control Point");
                                PositionOpposite(point, i == 0 ? true : false, wpPos);
                            }
                        }

                        Handles.DrawLine(point.cp[0].position, point.cp[1].position);
                        wpPos = script.bPoints[activeNode].wp.position;

                        if (Tools.pivotRotation == PivotRotation.Global)
                            wpRot = Quaternion.identity;
                            
                        Vector3 newPos = Handles.PositionHandle(wpPos, wpRot);
                        if(wpPos != newPos)
                        {
                            Undo.RecordObject(script.bPoints[activeNode].wp, "Move Handle");
                            script.bPoints[activeNode].wp.position = newPos;
                        }
                        break;

                    case Tool.Rotate:
                        wpPos = script.bPoints[activeNode].wp.position;
                        Quaternion newRot = Handles.RotationHandle(wpRot, wpPos);

                        if (wpRot != newRot) 
                        {
                            Vector3[] globalPos = new Vector3[script.bPoints[activeNode].wp.childCount];
                            for (int i = 0; i < globalPos.Length; i++)
                                globalPos[i] = script.bPoints[activeNode].wp.GetChild(i).position;

                            Undo.RecordObject(script.bPoints[activeNode].wp, "Rotate Handle");
                            script.bPoints[activeNode].wp.rotation = newRot;

                            for (int i = 0; i < globalPos.Length; i++)
                                script.bPoints[activeNode].wp.GetChild(i).position = globalPos[i];
                        }
                        break;
                }
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);

            script.CalculatePath();

            if (!script.showHandles) return;
            Handles.color = script.color2;
            Vector3[] pathPoints = script.pathPoints;
            
            for (int i = 0; i < pathPoints.Length; i++)
            {
                #if UNITY_5_6_OR_NEWER
                Handles.SphereHandleCap(0, pathPoints[i], Quaternion.identity, 
                Mathf.Clamp((HandleUtility.GetHandleSize(pathPoints[i]) * 0.12f), 0, 0.25f), EventType.Repaint);
                #else
                Handles.SphereCap(0, pathPoints[i], Quaternion.identity, 
                Mathf.Clamp((HandleUtility.GetHandleSize(pathPoints[i]) * 0.12f), 0, 0.25f));
                #endif
            }

                
            if(!script.drawDirection) return;
            float lerpVal = 0f;
            
            List<List<Vector3>> segments = new List<List<Vector3>>();
            int curIndex = 0;
            
            for(int i = 0; i < script.bPoints.Count - 1; i++)
            {
                segments.Add(new List<Vector3>());
                for(int j = curIndex; j < pathPoints.Length; j++)
                {
                    if(pathPoints[j] == script.bPoints[i+1].wp.position)
                    {
                        curIndex = j;
                        break;
                    }
                    
                    segments[i].Add(pathPoints[j]);
                }
            }
            
            for(int i = 0; i < segments.Count; i++)
            {   
                for(int j = 0; j < segments[i].Count; j++)
                {
                    size = Mathf.Clamp(HandleUtility.GetHandleSize(segments[i][j]) * 0.4f, 0, 1.2f);
                    lerpVal = j / (float)segments[i].Count;
                    Handles.ArrowHandleCap(0, segments[i][j], Quaternion.Lerp(script.bPoints[i].wp.rotation, script.bPoints[i + 1].wp.rotation, lerpVal), size, EventType.Repaint);
                }
            }
        }


        private void PositionOpposite(BezierPoint point, bool isLeft, Vector3 newPos)
        {
            Vector3 pos = point.wp.position;
            Vector3 toParent = pos - newPos;
            int inIndex = isLeft ? 0 : 1;
            int outIndex = inIndex == 0 ? 1 : 0;

            toParent.Normalize();

            point.cp[inIndex].position = newPos;

            if (toParent != Vector3.zero && script.connectHandles)
            {
                float magnitude = (pos - point.cp[outIndex].position).magnitude;
                point.cp[outIndex].position = pos + toParent * magnitude;
            }
        }


        private void DrawPathOptions()
        {
            editOption = (PathModifierOption)EditorGUILayout.EnumPopup(editOption);

            switch (editOption)
            {
                case PathModifierOption.PlaceToGround:
                    foreach (BezierPoint bp in script.bPoints)
                    {
                        Ray ray = new Ray(bp.wp.position + new Vector3(0, 2f, 0), -Vector3.up);
                        Undo.RecordObject(bp.wp, "Place To Ground");

                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100))
                        {
                            //position waypoint to hit point
                            bp.wp.position = hit.point;
                        }

                        RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, -Vector2.up, 100);
                        if (hit2D)
                        {
                            bp.wp.position = new Vector3(hit2D.point.x, hit2D.point.y, bp.wp.position.z);
                        }
                    }
                    break;

                case PathModifierOption.InvertDirection:
                    Undo.RecordObject(script, "Invert Direction");

                    List<List<Vector3>> waypointCopy = new List<List<Vector3>>();
                    for (int i = 0; i < script.bPoints.Count; i++)
                    {
                        BezierPoint curPoint = script.bPoints[i];
                        waypointCopy.Add(new List<Vector3>() { curPoint.wp.position, curPoint.cp[0].position, curPoint.cp[1].position });
                    }

                    for (int i = 0; i < script.bPoints.Count; i++)
                    {
                        BezierPoint curPoint = script.bPoints[i];
                        curPoint.wp.position = waypointCopy[waypointCopy.Count - 1 - i][0];
                        curPoint.cp[0].position = waypointCopy[waypointCopy.Count - 1 - i][2];
                        curPoint.cp[1].position = waypointCopy[waypointCopy.Count - 1 - i][1];
                    }

                    break;

                case PathModifierOption.RotateWaypointsToPath:
                    Undo.RecordObject(script, "Rotate Waypoints");

                    for (int i = 0; i < script.bPoints.Count; i++)
                    {
                        Vector3[] globalPos = new Vector3[script.bPoints[i].wp.childCount];
                        for (int j = 0; j < globalPos.Length; j++)
                            globalPos[j] = script.bPoints[i].wp.GetChild(j).position;

                        if (i == script.bPoints.Count - 1)
                            script.bPoints[i].wp.rotation = script.bPoints[i - 1].wp.rotation;
                        else
                            script.bPoints[i].wp.LookAt(script.bPoints[i + 1].wp);

                        //restore previous location after rotation
                        for (int j = 0; j < globalPos.Length; j++)
                            script.bPoints[i].wp.GetChild(j).position = globalPos[j];
                    }
                    break;

                case PathModifierOption.RenameWaypoints:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Skip Custom Names?");
                    script.skipCustomNames = EditorGUILayout.Toggle(script.skipCustomNames, GUILayout.Width(20));
                    EditorGUILayout.EndHorizontal();

                    if (!GUILayout.Button("Rename Now"))
                    {
                        return;
                    }

                    RenameWaypoints(script.skipCustomNames);
                    break;

                case PathModifierOption.UpdateFromChildren:
                    Undo.RecordObject(script, "Update Path From Children");
                    script.Create();
                    SceneView.RepaintAll();
                    break;

                case PathModifierOption.ReplaceWaypointObject:
                    script.replaceObject = (GameObject)EditorGUILayout.ObjectField("Replace Object", script.replaceObject, typeof(GameObject), true);

                    if (!GUILayout.Button("Replace Now")) return;
                    else if (script.replaceObject == null)
                    {
                        Debug.LogWarning("No replace object set. Cancelling.");
                        return;
                    }

                    Undo.RegisterFullObjectHierarchyUndo(script.transform, "Replace Object");

                    List<GameObject> toRemove = new List<GameObject>();
                    for (int i = 0; i < script.bPoints.Count; i++)
                    {
                        BezierPoint point = script.bPoints[i];
                        Transform curWP = point.wp;
                        Transform newCur = ((GameObject)Instantiate(script.replaceObject, curWP.position, Quaternion.identity)).transform;

                        Undo.SetTransformParent(point.cp[0], newCur, "Replace Object");
                        Undo.SetTransformParent(point.cp[1], newCur, "Replace Object");
                        newCur.parent = point.wp.parent;

                        script.bPoints[i].wp = newCur;
                        toRemove.Add(curWP.gameObject);
                    }

                    for (int i = 0; i < toRemove.Count; i++)
                        Undo.DestroyObjectImmediate(toRemove[i]);

                    break;
            }

            editOption = PathModifierOption.SelectModifier;
        }


        private void RenameWaypoints(bool skipCustom)
        {
            string wpName = string.Empty;
            string[] nameSplit;

            for (int i = 0; i < script.bPoints.Count; i++)
            {
                wpName = script.bPoints[i].wp.name;
                nameSplit = wpName.Split(' ');

                if (!script.skipCustomNames)
                    wpName = "Waypoint " + i;
                else if (nameSplit.Length == 2 && nameSplit[0] == "Waypoint")
                {
                    int index;
                    if (int.TryParse(nameSplit[1], out index))
                    {
                        wpName = nameSplit[0] + " " + i;
                    }
                }

                script.bPoints[i].wp.name = wpName;
            }
        }
    }
}