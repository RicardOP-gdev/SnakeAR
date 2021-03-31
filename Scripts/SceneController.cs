using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using GoogleARCore.Examples.Common;


public class SceneController : MonoBehaviour
{

    public Camera firstPersonCamera;
    public ScoreboardController scoreboard;
    public ScoreboardController timer;
    public SnakeController snakeController;
    public bool stopVisualizePlane;
    public ARCoreSessionConfig SessionConfig;
    public static float timerGame;
    public static float refreshTimerGame = 5;
    public static bool initGame;

    // Start is called before the first frame update
    void Start()
    {
        QuitOnConnectionErrors();
        SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.Horizontal;
    }

    // Update is called once per frame
    void Update()
    {
        //timer.text = timerGame.ToString("#.00");
        // The session status must be Tracking in order to access the Frame.
        if (Session.Status != SessionStatus.Tracking)
        {
            int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
            return;
        }
        Debug.Log(ScoreboardController.modeGame);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ProcessTouches();
        scoreboard.SetScore(snakeController.GetLength());    
        scoreboard.SetMode(ScoreboardController.modeGame);    
        if (initGame)
        {
            timerGame -= Time.deltaTime;
            scoreboard.SetTimer(timerGame);
            if (timerGame <= 0)
            {
                timerGame = 0;
                GameOver(); 
            }
        }   
    }

    void QuitOnConnectionErrors()
    {
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            StartCoroutine(CodelabUtils.ToastAndExit(
                "Camera permission is needed to run this application.", 5));
        }
        else if (Session.Status.IsError())
        {
            // This covers a variety of errors.  See reference for details
            // https://developers.google.com/ar/reference/unity/namespace/GoogleARCore
            StartCoroutine(CodelabUtils.ToastAndExit(
                "ARCore encountered a problem connecting. Please restart the app.", 5));
        }
    }

    void ProcessTouches()
    {
        Touch touch;
        if (Input.touchCount != 1 ||
            (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        TrackableHit hit;
        TrackableHitFlags raycastFilter =
            TrackableHitFlags.PlaneWithinBounds |
            TrackableHitFlags.PlaneWithinPolygon;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            SetSelectedPlane(hit.Trackable as DetectedPlane);
        }
    }

    void SetSelectedPlane(DetectedPlane selectedPlane)
    {  
        if(GameObject.FindGameObjectWithTag("food") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("food"));
        }
        Debug.Log("Selected plane centered at " + selectedPlane.CenterPose.position);
        scoreboard.SetSelectedPlane(selectedPlane);
        snakeController.SetPlane(selectedPlane);
        GetComponent<FoodController>().SetSelectedPlane(selectedPlane);
        GetComponent<FoodController>().SpawnFoodInstance();
        initGame = true;
        timerGame = refreshTimerGame;
        FindObjectOfType<SnakeController>().speed = 20;
        HidePlanes();
    }

   /* public void StopVisualizePlanes(bool showPlane)
    {
        
    } */

    public void GameOver()
    {
        timerGame = 0;
        initGame = false;
        GameObject food = GameObject.FindGameObjectWithTag("food");
        Destroy(food);
        FindObjectOfType<SnakeController>().speed = 0;       
    }   

    public void HidePlanes()
    {
        foreach (GameObject plane in GameObject.FindGameObjectsWithTag("plane"))
        {
            Renderer r = plane.GetComponent<Renderer>();
            DetectedPlaneVisualizer t = plane.GetComponent<DetectedPlaneVisualizer>();
            r.enabled = false;
            t.enabled = false;
            SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.Disabled;
        }
    }
}
