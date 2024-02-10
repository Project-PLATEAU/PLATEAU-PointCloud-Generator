using System;
using System.Collections.Generic;
using UnityEngine;
using PclSharp;

/// <summary>
/// Provide functionality to conduct point cloud mapping along all centerlines in OSM.
/// If you play your scene, PointCloudMapper will automatically start mapping.
/// The vehicle keeps warping along centerlines at a interval of <see cref="captureLocationInterval"/> and point cloud data from sensors are captured at every warp point.
/// PCD file will be outputted when you stop your scene or all locations in the route are captured.
/// </summary>
public class AdaPointCloudMapper : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Game object containing sensors to capture pointcloud. It will be warped along centerlines of lanelets.")]
    private GameObject vehicleGameObject;

    [SerializeField]
    [Tooltip("Result PCD file name. On Editor/Windows, it will be saved in Assets/")]
    private string outputPcdFilePath = "output.pcd";

    [SerializeField]
    [Tooltip("World origin in ROS coordinate systems, will be added to every point coordinates")]
    private Vector3 worldOriginROS;

    private List<IMappingSensor> mappingSensors;
    private Queue<Pose> capturePoseQueue;
    private PointCloudOfXYZI mergedPCL;

    public void Start()
    {
        mappingSensors = new List<IMappingSensor>(vehicleGameObject.GetComponentsInChildren<IMappingSensor>());
        if (mappingSensors.Count == 0)
        {
            Debug.LogError($"Found 0 sensors in {vehicleGameObject.name}. Disabling PointCloudMapper!");
            enabled = false;
            return;
        }

        Debug.Log($"Found {mappingSensors.Count} sensors in {vehicleGameObject.name}:");
        foreach (var mappingSensor in mappingSensors)
        {
            Debug.Log($"- IMappingSensor: {mappingSensor.GetSensorName()}");
        }
        
        mergedPCL = new PointCloudOfXYZI();
    }

    public void Update()
    {
        foreach (var sensor in mappingSensors)
        {
            var sensorPCL = sensor.Capture_XYZI_ROS(worldOriginROS);
            foreach (var point in sensorPCL.Points)
            {
                mergedPCL.Add(point);
            }
        }
    }

    public void OnDestroy()
    {
        SavePCL();
    }

    private void SavePCL()
    {
        var path = $"{Application.dataPath}/{outputPcdFilePath}";
        Debug.Log($"Writing PCL data to {path}");
        var writer = new PclSharp.IO.PCDWriter();
        writer.Write(path, mergedPCL);
        Debug.Log("PCL data saved successfully");
    }
}
