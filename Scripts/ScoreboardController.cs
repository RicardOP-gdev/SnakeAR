using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class ScoreboardController : MonoBehaviour
{

    public Camera firstPersonCamera;
    private Anchor anchor;
    private DetectedPlane detectedPlane;
    private float yOffset;
    private int score;
    private float timeGame;
    public static string modeGame;
    private string modeGame2;
  


    // Start is called before the first frame update
    void Start()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        timeGame = SceneController.timerGame;
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
        if (detectedPlane == null)
        {
            return;
        }

        // Check for the plane being subsumed.
        // If the plane has been subsumed switch attachment to the subsuming plane.
        while (detectedPlane.SubsumedBy != null)
        {
            detectedPlane = detectedPlane.SubsumedBy;
        }

        // Make the scoreboard face the viewer.
        transform.LookAt(firstPersonCamera.transform);

        // Move the position to stay consistent with the plane.
        transform.position = new Vector3(transform.position.x,
                    detectedPlane.CenterPose.position.y + yOffset, transform.position.z);
    }

    public void SetSelectedPlane(DetectedPlane detectedPlane)
    {
        this.detectedPlane = detectedPlane;
        CreateAnchor();
    }

    void CreateAnchor()
    {
        // Create the position of the anchor by raycasting a point towards
        // the top of the screen.
        Vector2 pos = new Vector2(Screen.width * .5f, Screen.height * .90f);
        Ray ray = firstPersonCamera.ScreenPointToRay(pos);
        Vector3 anchorPosition = ray.GetPoint(5f);

        // Create the anchor at that point.
        if (anchor != null)
        {
            DestroyObject(anchor);
        }
        anchor = detectedPlane.CreateAnchor(
            new Pose(anchorPosition, Quaternion.identity));

        // Attach the scoreboard to the anchor.
        transform.position = anchorPosition;
        transform.SetParent(anchor.transform);

        // Record the y offset from the plane.
        yOffset = transform.position.y - detectedPlane.CenterPose.position.y;

        // Finally, enable the renderers.
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
    }

    public void SetScore(int score)
    {
        GameObject scoreText = GameObject.FindGameObjectWithTag("score"); 
        if (this.score != score)
        {
            scoreText.GetComponent<TextMesh>().text = "Score: " + score;
            this.score = score;
        }
    }

    public void SetTimer(float time)
    {
        GameObject timerText = GameObject.FindGameObjectWithTag("time");
        if (this.timeGame != time)
        {
            timerText.GetComponent<TextMesh>().text = "Time: " + timeGame.ToString("F2");
            this.timeGame = time;

            if (Slithering.bodyParts.Count >= 0 && Slithering.bodyParts.Count <= 9)
            {
                SceneController.refreshTimerGame = 5;
                timerText.GetComponent<TextMesh>().color = Color.green;
            }

            else if (Slithering.bodyParts.Count >= 10)
            {
                SceneController.refreshTimerGame = 3;
                timerText.GetComponent<TextMesh>().color = Color.red;
            }
        }
    }

    public void SetMode(string mode)
    {        
       
        
        
    }
}
