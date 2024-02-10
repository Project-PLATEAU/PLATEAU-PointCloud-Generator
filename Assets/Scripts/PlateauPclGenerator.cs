using UnityEngine;
using UnityEditor;
using RGLUnityPlugin;

public class PlateauPclGenerator : EditorWindow
{
    private GameObject RGLLidar;
    private LidarSensor lidarSensor;

    private int automaticCaptureHz;
    private int horizontalSteps;
    private float minHAngle;
    private float maxHAngle;
    private float maxRange;
    private bool applyGaussianNoise;

    private GameObject vehicle;
    private float vehicleLength;
    private float vehicleHeight;
    private float vehicleWidth;

    private GameObject sensor;
    private float sensorLength;
    private float sensorHeight;
    private float sensorWidth;
    private float sensorTransformX;
    private float sensorTransformY;
    private float sensorTransformZ;
   
    [MenuItem("PLATEAU PCL Generator/各種パラメーター設定", false, 1)]
    static void Init()
    {
        PlateauPclGenerator window = (PlateauPclGenerator)EditorWindow.GetWindow(typeof(PlateauPclGenerator));
        window.Show(); 
    }

    void CreateGUI() {
        RGLLidar = GameObject.Find("GenericRGLLidar");
        lidarSensor = RGLLidar.GetComponent<LidarSensor>();

        automaticCaptureHz = lidarSensor.AutomaticCaptureHz;
        horizontalSteps = lidarSensor.configuration.horizontalSteps;
        minHAngle = lidarSensor.configuration.minHAngle;
        maxHAngle = lidarSensor.configuration.maxHAngle;
        maxRange = lidarSensor.configuration.maxRange;
        applyGaussianNoise = lidarSensor.applyGaussianNoise;

        vehicle = GameObject.Find("Vehicle");
        vehicleLength = vehicle.transform.localScale.x;
        vehicleHeight = vehicle.transform.localScale.y;
        vehicleWidth = vehicle.transform.localScale.z;

        sensor = vehicle.transform.Find("Sensors").gameObject;
        Vector3 parentLossyScale = sensor.transform.parent.lossyScale;
        sensorLength = sensor.transform.localScale.x * parentLossyScale.x;
        sensorHeight = sensor.transform.localScale.y * parentLossyScale.y;
        sensorWidth = sensor.transform.localScale.z * parentLossyScale.z;

        sensorTransformX = sensor.transform.localPosition.x;
        sensorTransformY = sensor.transform.localPosition.y;
        sensorTransformZ = sensor.transform.localPosition.z;
    }

    void OnGUI()
    {
        GUILayout.Label("LiDAR 詳細設定", EditorStyles.boldLabel);

        automaticCaptureHz = (int)EditorGUILayout.Slider ("周波数(Hz)", automaticCaptureHz, 0, 50);
        lidarSensor.AutomaticCaptureHz = automaticCaptureHz;

        horizontalSteps = (int)EditorGUILayout.Slider ("水平ステップ数", horizontalSteps, 1, 3000);
        lidarSensor.configuration.horizontalSteps = horizontalSteps;

        minHAngle = EditorGUILayout.Slider ("最小水平角度", minHAngle, -180, 0);
        lidarSensor.configuration.minHAngle = minHAngle;

        maxHAngle = EditorGUILayout.Slider ("最大水平角度", maxHAngle, 0, 180);
        lidarSensor.configuration.maxHAngle = maxHAngle;

        maxRange = EditorGUILayout.Slider ("飛距離", maxRange, 0, 500);
        lidarSensor.configuration.maxRange = maxRange;
        
        applyGaussianNoise = EditorGUILayout.Toggle ("ガウシアンノイズフィルター", applyGaussianNoise);
        lidarSensor.applyGaussianNoise = applyGaussianNoise;

        GUILayout.Label("");
        GUILayout.Label("車両設定", EditorStyles.boldLabel);

        vehicleWidth = EditorGUILayout.Slider ("全長", vehicleWidth, 1, 5);
        vehicleLength = EditorGUILayout.Slider ("車幅", vehicleLength, 1, 10);
        vehicleHeight = EditorGUILayout.Slider ("車高", vehicleHeight, 1, 10);
        
        
        Vector3 vehicleScale = new Vector3(vehicleLength, vehicleHeight, vehicleWidth);
        vehicle.transform.localScale = vehicleScale;

        GUILayout.Label("");
        GUILayout.Label("LiDAR設定", EditorStyles.boldLabel);


        sensorLength = EditorGUILayout.Slider ("長さ", sensorLength, 0.01f, 1);
        sensorHeight = EditorGUILayout.Slider ("高さ", sensorHeight, 0.01f, 1);
        sensorWidth = EditorGUILayout.Slider ("横幅", sensorWidth, 0.01f, 1);
        Vector3 parentLossyScale = sensor.transform.parent.lossyScale;

        Vector3 sensorScale = 
            new Vector3(sensorLength / parentLossyScale.x, sensorHeight / parentLossyScale.y, sensorWidth / parentLossyScale.z);
        sensor.transform.localScale = sensorScale;

        sensorTransformX = EditorGUILayout.Slider ("取付位置X", sensorTransformX, -0.5f, 0.5f);
        sensorTransformY = EditorGUILayout.Slider ("取付位置Y", sensorTransformY, 0.0f, 3);
        sensorTransformZ = EditorGUILayout.Slider ("取付位置Z", sensorTransformZ, 0.0f, 1);
        sensor.transform.localPosition = new Vector3(sensorTransformX, sensorTransformY, sensorTransformZ);
    }
}