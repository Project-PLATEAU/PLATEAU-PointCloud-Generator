using UnityEditor;
using UnityEngine;

namespace WaypointSystem
{
    public class CreateWPManager : EditorWindow
    {
        [MenuItem("Window/Waypoint System/Waypoint Manager")]

        static void Init()
        {
            WaypointManager wpManager = FindObjectOfType<WaypointManager>(true);

            if (wpManager == null)
            {
                wpManager = new GameObject("WaypointManager").AddComponent<WaypointManager>();
            }

            Selection.activeGameObject = wpManager.gameObject;
        }
    }
}