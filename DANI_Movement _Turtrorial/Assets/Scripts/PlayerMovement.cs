using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float moveMultiplier = 10f;
    [SerializeField] float airMultiplier = 0.4f;
    
    [Header("Swinging")]
    public float swingSpeed = 6f;
    public bool swinging;
    

    [Header("Jumping")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    public float jumpForce = 6f;

    [Header("Drag")]
    public float groundDrag = 6f;
    public float airDrag = 2f;

    float horizontalMovement;
    float verticalMovement;
    

    [Header("Sprinting")]
    [SerializeField] public float walkSpeed = 4f;
    [SerializeField] public float runSpeed = 6f;
    [SerializeField] public float accelration = 10f;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    float playerHeight = 2f;
    [SerializeField] bool isGrounded;
    float groundDistance = 0.3f;

    [Header("Gravity")]
    [SerializeField] public float gravityMultiplier = 10f;

    [SerializeField] Transform orientation;

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    Rigidbody rb;

    RaycastHit slopeHit;

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Start() 
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;   
    }

    private void Update() 
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        GravityControlJumpPad();
        MyInput();
        ControlDrag(); 
        ControlSpeed();

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void ControlSpeed()
    {
        if (Input.GetKey(sprintKey) && isGrounded)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, runSpeed, accelration * Time.deltaTime);
        }
        else if(swinging)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, swingSpeed, accelration * Time.deltaTime);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, accelration * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0 , rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void ControlDrag() 
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
        }
    }

    private void FixedUpdate() 
    {
        MovePlayer();
    }


    void MovePlayer()
    {
        if(swinging)
        {
            return;
        }

        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * moveMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * moveMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * moveMultiplier * airMultiplier, ForceMode.Acceleration);
        }

    }

    void GravityControlJumpPad()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, 10))
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }
    }


}
