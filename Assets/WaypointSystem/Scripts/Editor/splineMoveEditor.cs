using UnityEditor;
using UnityEngine;

namespace WaypointSystem
{
    using DG.Tweening;

    [CustomEditor(typeof(splineMove))]
    public class splineMoveEditor : moveEditor
    {
        public override void OnInspectorGUI()
        {
            m_Object.Update();

            EditorGUILayout.PropertyField(m_Object.FindProperty("pathContainer"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("onStart"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("moveToPath"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("reverse"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("localType"));

            EditorGUILayout.PropertyField(m_Object.FindProperty("startPoint"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("sizeToAdd"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("speed"));

            SerializedProperty timeValue = m_Object.FindProperty("timeValue");
            EditorGUILayout.PropertyField(timeValue);

            SerializedProperty easeType = m_Object.FindProperty("easeType");
            EditorGUILayout.PropertyField(easeType);
            if ((int)Ease.Unset == easeType.enumValueIndex)
                EditorGUILayout.PropertyField(m_Object.FindProperty("animEaseType"));

            SerializedProperty loopType = m_Object.FindProperty("loopType");
            EditorGUILayout.PropertyField(loopType);
            if (loopType.enumValueIndex == 1)
                EditorGUILayout.PropertyField(m_Object.FindProperty("closeLoop"));
            else
                m_Object.FindProperty("closeLoop").boolValue = false;

            SerializedProperty pathType = m_Object.FindProperty("pathType");
            EditorGUILayout.PropertyField(pathType);
            pathType.enumValueIndex = Mathf.Clamp(pathType.enumValueIndex, 0, 1);
            
            SerializedProperty orientToPath = m_Object.FindProperty("pathMode");
            EditorGUILayout.PropertyField(orientToPath);
            if (orientToPath.enumValueIndex > 0)
			{
			    EditorGUILayout.PropertyField(m_Object.FindProperty("lookAhead"));
				EditorGUILayout.PropertyField(m_Object.FindProperty("lockRotation"));
			}
            EditorGUILayout.PropertyField(m_Object.FindProperty("lockPosition"));

			if(orientToPath.enumValueIndex == 0)
			{
	            SerializedProperty waypointRotation = m_Object.FindProperty("waypointRotation");
	            EditorGUILayout.PropertyField(waypointRotation);
	            if(waypointRotation.enumValueIndex > 0)
	            {
	                EditorGUILayout.PropertyField(m_Object.FindProperty("rotationTarget"));
	            }
			}

            EditorGUILayout.Space();
            EventSettings();

            m_Object.ApplyModifiedProperties();
        }


        void OnSceneGUI()
        {
            var path = GetPathTransform();

            if (path == null) return;

            var waypoints = path.waypoints;
            Vector3 wpPos = Vector3.zero;
            float size = 1f;

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
            }
        }


        [DrawGizmo(GizmoType.Active)]
        static void DrawGizmoStartPoint(splineMove script, GizmoType gizmoType)
        {
            if (script.pathContainer == null) return;

            int maxLength = script.pathContainer.GetPathPoints().Length - 1;
            int index = Mathf.Clamp(script.startPoint, 0, maxLength);
            Vector3 position = script.pathContainer.GetPathPoints()[index];
            float size = Mathf.Clamp(HandleUtility.GetHandleSize(position) * 0.1f, 0, 0.3f);
            Gizmos.color = Color.magenta;

            if(!Application.isPlaying)
                Gizmos.DrawSphere(position, size);
        }
    }
}
