using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleGrapple : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header("References")]
    public List<LineRenderer> lineRenderers;
    public List<Transform> gunTips;
    public Transform cam;
    public Transform player;
    public LayerMask whatIsDoubleGrappelable;
    public PlayerMovement pm;

    [Header("Swinging")]
    private float maxSwingDistance = 25f;
    private List<Vector3> swingPoints;
    private List<SpringJoint> joints;

    private List<bool> grapplesActive;

    [Header("AirMovement")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;

    [Header("Prediction")]
    public List<RaycastHit> predictionHits;
    public List<Transform> predictionPoints;
    public float predictionSphereCastRadius;


    [Header("DualSwinging")]
    public int amountOfSwingPoints = 2;
    public List<Transform> pointAimers;
    private List<bool> swingsActive;

    private void Start()
    {
        ListSetup();
    }

    private void Update()
    {
        MyInput();
        CheckForSwingPoints();
        if (joints[0] != null && joints[1] != null) 
        {
            AirMovement();
        }
        
    }

    private void LateUpdate() 
    {
        DrawRope();    
    }

    void MyInput()
    {
            if (Input.GetKeyDown(swingKey)) StartSwing(0);
            if (Input.GetKeyDown(swingKey)) StartSwing(1);

            if (Input.GetKeyUp(swingKey)) StopSwing(0);
            if (Input.GetKeyUp(swingKey)) StopSwing(1);
    }
    private Vector3 pullPoint;
    private void AirMovement()
    {
        if (swingsActive[0] && !swingsActive[1]) pullPoint = swingPoints[0];
        if (swingsActive[1] && !swingsActive[0]) pullPoint = swingPoints[1];
        // get midpoint if both swing points are active
        if (swingsActive[0] && swingsActive[1])
        {
            Vector3 dirToGrapplePoint1 = swingPoints[1] - swingPoints[0];
            pullPoint = swingPoints[0] + dirToGrapplePoint1 * 0.5f;
        }

        // right
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        // left
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        // forward
        if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);
        // backward
        if (Input.GetKey(KeyCode.S)) rb.AddForce(-orientation.forward * forwardThrustForce * Time.deltaTime);
    }


    void ListSetup()
    {
        predictionHits = new List<RaycastHit>();

        swingPoints = new List<Vector3>();
        joints = new List<SpringJoint>();

        swingsActive = new List<bool>();
        grapplesActive = new List<bool>();
        currentGrapplePositions = new List<Vector3>();

        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            predictionHits.Add(new RaycastHit());
            joints.Add(null);
            swingPoints.Add(Vector3.zero);
            swingsActive.Add(false);
            grapplesActive.Add(false);
            currentGrapplePositions.Add(Vector3.zero);
        }
    }

    private void CheckForSwingPoints()
    {
        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            if (swingsActive[i]) { /* Do Nothing */ }
            else
            {
                RaycastHit sphereCastHit;
                Physics.SphereCast(pointAimers[i].position, predictionSphereCastRadius, pointAimers[i].forward, out sphereCastHit, maxSwingDistance, whatIsDoubleGrappelable);

                RaycastHit raycastHit;
                Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsDoubleGrappelable);

                Vector3 realHitPoint;

                // Option 1 - Direct Hit
                if (raycastHit.point != Vector3.zero)
                    realHitPoint = raycastHit.point;

                // Option 2 - Indirect (predicted) Hit
                else if (sphereCastHit.point != Vector3.zero)
                    realHitPoint = sphereCastHit.point;

                // Option 3 - Miss
                else
                    realHitPoint = Vector3.zero;

                // realHitPoint found
                if (realHitPoint != Vector3.zero)
                {
                    predictionPoints[i].gameObject.SetActive(true);
                    predictionPoints[i].position = realHitPoint;
                }
                // realHitPoint not found
                else
                {
                    predictionPoints[i].gameObject.SetActive(false);
                }

                predictionHits[i] = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
            }
        }
    }

    private void StartSwing(int swingIndex)
    {
        // return if predictionHit not found
        if (predictionHits[swingIndex].point == Vector3.zero) return;

        pm.swinging = true;
        swingsActive[swingIndex] = true;

        swingPoints[swingIndex] = predictionHits[swingIndex].point;
        joints[swingIndex] = player.gameObject.AddComponent<SpringJoint>();
        joints[swingIndex].autoConfigureConnectedAnchor = false;
        joints[swingIndex].connectedAnchor = swingPoints[swingIndex];

        float distanceFromPoint = Vector3.Distance(player.position, swingPoints[swingIndex]);

        // the distance grapple will try to keep from grapple point. 
        joints[swingIndex].maxDistance = distanceFromPoint * 0.8f;
        joints[swingIndex].minDistance = distanceFromPoint * 0.25f;

        // customize values as you like
        joints[swingIndex].spring = 15f;
        joints[swingIndex].damper = 7f;
        joints[swingIndex].massScale = 4.5f;

        lineRenderers[swingIndex].positionCount = 2;
        currentGrapplePositions[swingIndex] = gunTips[swingIndex].position;
    }

     public void StopSwing(int swingIndex)
    {
        pm.swinging = false;

        swingsActive[swingIndex] = false;

        Destroy(joints[swingIndex]);
    }
    private List<Vector3> currentGrapplePositions;
    private void DrawRope()
    {
        if (joints[0] != null && joints[1] != null) 
        {
            for (int i = 0; i < amountOfSwingPoints; i++)
            {
                // if not grappling, don't draw rope
                if (!grapplesActive[i] && !swingsActive[i]) 
                {
                    lineRenderers[i].positionCount = 0;
                }
                else
                {
                    currentGrapplePositions[i] = Vector3.Lerp(currentGrapplePositions[i], swingPoints[i], Time.deltaTime * 8f);

                    lineRenderers[i].SetPosition(0, gunTips[i].position);
                    lineRenderers[i].SetPosition(1, currentGrapplePositions[i]);
                }
            } 
        }
    
    }
}
