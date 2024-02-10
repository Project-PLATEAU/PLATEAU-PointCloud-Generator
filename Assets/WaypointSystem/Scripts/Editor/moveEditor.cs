using UnityEditor;
using UnityEngine;

namespace WaypointSystem
{
    public class moveEditor : Editor
    {
        public SerializedObject m_Object;

        public Vector2 scrollPosEvents;
        public bool showEventSetup = false;


        public virtual void OnEnable()
        {
            m_Object = new SerializedObject(target);
        }


        public virtual PathManager GetPathTransform()
        {
            return m_Object.FindProperty("pathContainer").objectReferenceValue as PathManager;
        }


        public override void OnInspectorGUI()
        {
            m_Object.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EventSettings();

            m_Object.ApplyModifiedProperties();
        }


        public virtual void EventSettings()
        {
            if (showEventSetup)
            {
                if (GUILayout.Button("Hide UnityEvents"))
                    showEventSetup = false;

                scrollPosEvents = EditorGUILayout.BeginScrollView(scrollPosEvents, GUILayout.Height(255));

                EditorGUILayout.PropertyField(m_Object.FindProperty("movementStart"));
                EditorGUILayout.PropertyField(m_Object.FindProperty("movementChange"));
                EditorGUILayout.PropertyField(m_Object.FindProperty("movementEnd"));

                EditorGUILayout.EndScrollView();
            }
            else
            {
                if (GUILayout.Button("Show UnityEvents"))
                    showEventSetup = true;
            }
        }
    }
}