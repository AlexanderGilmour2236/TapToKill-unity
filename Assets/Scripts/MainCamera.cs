using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{

    #region Singleton

    private static MainCamera _instance;

    public static MainCamera Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject ins = new GameObject("MainCamera");
                ins.AddComponent<MainCamera>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    #endregion   
    
    public Transform Target;
    public float CameraSpeed;
    public float CameraChangeSizeSpeed;
    public float minCameraSize;
    public float maxCameraSize = 15;
    private Camera _camera;
    
    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    public float CameraSize
    {
        get { return _camera.orthographicSize; }
        set { _camera.orthographicSize = value; }
    }

    void Update()
    {
        transform.position = Vector2.Lerp(transform.position,Target.position, CameraSpeed*Time.deltaTime);
        transform.Translate(0,0,-10);

        // Размер камеры в зависимости от противников
        float maxX, maxY;
        maxX = maxY = 0;

        foreach (Enemy enemy in GameController.Instance.Enemies)
        {
            float magnitude = Mathf.Abs(enemy.transform.position.x - transform.position.x);
            if (magnitude > maxX)
            {
                maxX = magnitude;
            }

            magnitude = Mathf.Abs(enemy.transform.position.y - transform.position.y);
            if (magnitude > maxY)
            {
                maxY = magnitude;
            }
        }

        if (maxX == 0 || maxY == 0)
        {
            CameraSize = Mathf.Lerp(CameraSize, minCameraSize, CameraChangeSizeSpeed*Time.deltaTime);
        }
        else
        {
            if (maxX > maxY)
            {
                CameraSize = Mathf.Lerp(CameraSize, Mathf.Clamp(maxX * 1.3f * Screen.height/Screen.width, minCameraSize,maxCameraSize), CameraChangeSizeSpeed*Time.deltaTime);
            }
            else
            {
                CameraSize = Mathf.Lerp(CameraSize, Mathf.Clamp(maxY * 1.3f, minCameraSize,maxCameraSize), CameraChangeSizeSpeed*Time.deltaTime);
            }
        }
    }
}
