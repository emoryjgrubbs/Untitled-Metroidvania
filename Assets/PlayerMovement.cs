using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //movement
    private Vector2 moveInput;
    //jump
    private bool isJumping;
    private bool isJumpCut;
    private bool isJumpFalling;
    private float lastGroundedTime;
    private float lastJumpTime;
    //direction
    private Vector2 faceDirection;
    //assists
    [Range(0.01f, 0.5f)] public float coyoteTime; 
	[Range(0.01f, 0.5f)] public float jumpInputBufferTime;
    //respawn (update this vector when at a checkpoint)
    private Vector2 respawnPosition;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runAccelAmount;
    [SerializeField] private float runDeccelAmount;
    [SerializeField] private float accelInAir;
    [SerializeField] private float deccelInAir;
    [SerializeField] private float jumpForce;
    [Range(0.1f, 0.999f)] public float jumpCut;
    [SerializeField] private float jumpCutGravityMult;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float maxFastFallSpeed;
    [SerializeField] private float gravityScale;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
    [SerializeField] private Vector2 hitBoxSize = new Vector2(1f,1f);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask killPlane;

    // Start is called before the first frame update
    void Start(){
        isJumping = false;
        isJumpCut = false;
        isJumpFalling = false;
        lastGroundedTime = 0;
        lastJumpTime = 0;
        faceDirection.x = 1;    //looking right
        faceDirection.y = 0;    //looking horizontally
        respawnPosition.x = 0f;
        respawnPosition.y = 0f;
    }

    // Update is called once per frame
    void Update(){
        //timers
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        //input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        //movement
        if(Input.GetKeyDown(KeyCode.Space)){    //make compatible with rebinds, do this with "m" or "g" in map script to make it appear on screen
            lastJumpTime = jumpInputBufferTime;
        }
        if(Input.GetKeyUp(KeyCode.Space)){
            if(canJumpCut()){
                isJumpCut = true;
            }
        }
        //collision checks
        if(!isJumping){
            if(Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer)){
                lastGroundedTime = coyoteTime;
            }
        }
        if(isKillPlane()){
            transform.position = respawnPosition;
        }
        //jump checks
        if(isJumping && rb.velocity.y < 0){
            isJumping = false;
            isJumpFalling = true;
        }
        if(lastGroundedTime > 0 && !isJumping){
            isJumpCut = false;
            isJumpFalling = false;
        }
        if(canJump() && lastJumpTime > 0){
            isJumping = true;
            isJumpCut = false;
            isJumpFalling = false;
            jump();
        }
        if(rb.velocity.y <= 0 ){
            isJumpCut = true;
        }
        if(isJumpCut){ 
            if(rb.velocity.y > 0){
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCut);  //cuts upward velocity when player releases jump, allows differnt heights 
            }
            else{
                rb.gravityScale = (gravityScale * jumpCutGravityMult);  //increases fall acceleration when moving down
            }
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));  //makes max fall speed, the max downward speed
        }
        else{
            rb.gravityScale = (gravityScale);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        //add an if(moveInput.y < 0){fastFall = true;}
        //else{fastFall = false;}
        //direction check
        if(moveInput.x * faceDirection.x < 0){
            turn();
        }
        if(moveInput.y == 0f){
            faceDirection.y = 0;    //looking horizontally
            //Debug.Log("test, no facing"); //working thumbs up
        }
        else if(moveInput.y < 0f){ 
            faceDirection.y = -1;   //looking down
            //Debug.Log("test, facing down");
        }
        else{
            faceDirection.y = 1;    //looking up
            //Debug.Log("test, facing up");
        }
        //attack check
        if(Input.GetKeyDown(KeyCode.J)){    //need to make this rebindable, but for current simplicity
            //call attack function, 
                //may need to change this, but we if want one attack per press, this should work
        }
    }

    private void FixedUpdate(){
        run(); 
    }

    private void run(){
        float targetSpeed = moveInput.x * moveSpeed;
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, 1); //1 is lerpAmount
        //acceleration
        float accelRate;
        if (lastGroundedTime > 0){
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount;
        }
		else{
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount * accelInAir : runDeccelAmount * deccelInAir;
        }
        float speedDif = targetSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void jump(){
        float force = jumpForce;
        if(rb.velocity.y < 0){
            force -= rb.velocity.y;
        }
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        isJumping = true;
        lastGroundedTime = 0;
        lastJumpTime = 0;
    }

    private bool canJump(){
        return lastGroundedTime > 0 && !isJumping;
    }
    private bool canJumpCut(){
        return isJumping && rb.velocity.y > 0;
    }

    private bool isGrounded(){
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
    }

    private bool isKillPlane(){
        return Physics2D.OverlapBox(transform.position, hitBoxSize, 0, killPlane);
    }

    private void turn(){
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        faceDirection.x *= -1;  //flips the direction the player is facing  (1 being right, -1 being left)
    }
}
