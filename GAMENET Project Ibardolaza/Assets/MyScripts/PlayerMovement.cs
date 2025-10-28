using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]

    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    bool readyToJump = true;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    public Transform orientation;
    public Transform playerObj;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        grounded = Physics.Raycast(playerObj.position + Vector3.down * 0.5f, Vector3.down, playerHeight / 2 + 0.5f);
        Debug.DrawRay(playerObj.position + Vector3.down * 0.5f, Vector3.down * 3f, Color.red);

        MyInput();
        SpeedControl();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            CmdSendMessage();
        }

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            RpcJumpMessage();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    [Command]
    void CmdSendMessage()
    {
        Debug.Log("warafen bella why are you crying again i know vamfire right? vampire will feyt to me");
        RpcSendMessage();
    }

    [ClientRpc]
    void RpcSendMessage()
    {
        Debug.Log("edward what are you doing here trusted to protect bella but you did not i will sure you die edward  i will sure you die from now on, she's mine");
    }

    void RpcJumpMessage() // For testing; triggers every jump
    {
        Debug.Log("I have died everyday waiting for you");
    }
}
