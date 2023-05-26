using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleGrapple : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsSingleGrappelable;
    public PlayerMovement pm;

    [Header("Swinging")]
    [SerializeField] private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    public SpringJoint joint;

    [Header("Air Movement")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float verticalThrustForce;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;
    
    private Vector3 currentGrapplePosition;


    void Update()
    {
        if (Input.GetKeyDown(swingKey))
        {
            StartSwing();
        }
        if (Input.GetKeyUp(swingKey))
        {
            StopSwing();
        }
        if (joint != null)
        {
            AirMovement();
        }

        CheckForSwingPoints();
    }

    private void CheckForSwingPoints()
    {
        if (joint != null)
        {
            return;
        }
        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, out sphereCastHit, maxSwingDistance, whatIsSingleGrappelable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsSingleGrappelable);

        Vector3 realHitPoint;

        // Option 1 - Direct Hit
        if (raycastHit.point != Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }
        // Option 2 - Predicted (using spherecast) hit
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        // Option 3 - Miss
        else
        {
            realHitPoint = Vector3.zero;
        }

        // realHitPoint found
        if(realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }

        //realHitPoint not found
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

    void AirMovement()
    {
        //right
        if(Input.GetKey(KeyCode.D))
        {
            rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        //left
        if(Input.GetKey(KeyCode.A))
        {
            rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        //forward
        if(Input.GetKey(KeyCode.W))
        {
            rb.AddForce(orientation.forward * verticalThrustForce * Time.deltaTime);
        }
        //backward
        if(Input.GetKey(KeyCode.S))
        {
            rb.AddForce(-orientation.forward * verticalThrustForce * Time.deltaTime);
        }
    }

    private void LateUpdate() 
    {
        DrawRope();    
    }

    void DrawRope()
    {
        // if not grappling do not draw the rope
        if (!joint)
        {
            return;
        }

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }

    public void StartSwing()
    {


        // return if predictionHit has not been found
        if (predictionHit.point == Vector3.zero)
        {
            return;
        }

        pm.swinging = true;
        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        // the distance grapple will try to keep from the grapple point
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
        
    }

    public void StopSwing()
    {

        pm.swinging = false;

        lr.positionCount = 0;
        Destroy(joint);
    }
}
