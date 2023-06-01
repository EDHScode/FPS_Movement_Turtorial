using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad_Boost : MonoBehaviour
{
    public LayerMask WhatIsJumpPadable;
    public float JumpBoostMultiplier = 20f;
    public Rigidbody rb;
    public float raycastDistance = 1.2f;

    // Start is called before the first frame update
    void Start()
    {
        rb.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, Vector3.down, raycastDistance, WhatIsJumpPadable))
        {
            rb.AddForce(Vector3.up * JumpBoostMultiplier, ForceMode.Impulse);
        }

    }

}
