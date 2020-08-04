using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D groundCollider;
    public BoxCollider2D topCollider;
    public BoxCollider2D topDetector;
    private Animator anim;
    public float runSpeed = 10f;
    public float crouchSpeed = 5f;
    public float jumpForce;
    public int jumpCombo = 2;
    public bool jumping, idling, falling, running, jumped, crouching;

    public const float RUNTHRESHOLD = 0.2f;
    public const float JUMPTHRESHOLD = 1f;

    private int cherry;
    public Text cherryDisplay;
    public float jumpAttack = 50f;

    // public Transform groundCheck;
    public LayerMask ground;
    float horizontalMove, verticalMove;

    public bool jumpButton, onGround;
    public int jumpAvailable;
    public bool isHurt;
    public float hurtForce = 30;
    public float hurtDelay = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCollider = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        DetectJumpButton();
        DetectMovement();

        updateGround();

        if (!isHurt)
        {
            Jump();
            Crouch();
            GroundMovement();
        }
        else
        {
            Invoke("ResetHurtTime", hurtDelay);
        }

    }
    void FixedUpdate()
    {

    }

    private void LateUpdate()
    {

        //update player state and animation state
        if (!isHurt)
        {
            UpdateState(horizontalMove, jumpButton);
        }
        UpdateAnimState();
    }

    void updateGround()
    {
        //groundCheck = cd.IsTouchingLayers(ground);
        //onGround = Physics2D.OverlapCircle(groundCheck.position, 0.1f, ground);

        onGround = groundCollider.IsTouchingLayers(ground);
    }


    void DetectJumpButton()
    {
        if (Input.GetButtonDown("Jump") && jumpAvailable > 0)
        {
            jumpButton = true;
        }
    }

    void DetectMovement()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal");
        verticalMove = Input.GetAxisRaw("Vertical");
    }

    void GroundMovement()
    {
        float moveSpeed = runSpeed;
        if (crouching) moveSpeed = crouchSpeed;
        rb.velocity = new Vector2(horizontalMove * moveSpeed, rb.velocity.y);

        if (horizontalMove != 0)
        {
            transform.localScale = new Vector3(horizontalMove, transform.localScale.y, transform.localScale.z);
        }
    }

    void Crouch()
    {

        if (crouching)
        {
            topCollider.enabled = false;
        }
        else
        {
            topCollider.enabled = true;
        }
    }

    void Jump()
    {
        bool firstJump = jumpButton && onGround;
        bool secondJump = jumpButton && !onGround && jumpAvailable > 0;
        if (onGround)
        {
            jumpAvailable = jumpCombo;
        }
        if (firstJump || secondJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpAvailable -= 1;
            jumpButton = false;
            jumped = true;
        }
    }

    void UpdateState(float horizontalMove, bool jumpButton)
    {
        if (jumped) // jump 
        {
            jumping = true;
            falling = false;
            jumped = false;
            crouching = false;
        }

        if (rb.velocity.y < 0 && !onGround) // fall
        {
            jumping = false;
            falling = true;
        }

        if (onGround)
        {
            // if (jumpAvailable <= jumpCombo - 1) jumping = false;
            falling = false;
            if (verticalMove < 0)
            {
                if (crouching == false && Mathf.Abs(rb.velocity.x) > crouchSpeed)
                {
                    float maxSpeed = Mathf.Abs(rb.velocity.x) / rb.velocity.x * crouchSpeed;
                    rb.velocity = new Vector2(maxSpeed, rb.velocity.y);
                }
                if (crouching == false && Mathf.Abs(rb.velocity.y) > crouchSpeed)
                {
                    float maxSpeed = Mathf.Abs(rb.velocity.y) / rb.velocity.y * crouchSpeed;
                    rb.velocity = new Vector2(rb.velocity.x, maxSpeed);
                }
                crouching = true;
                jumping = false;
            }
            else if (crouching == true && !topDetector.IsTouchingLayers(ground))
            {
                crouching = false;
            }
        }
        else if (!topDetector.IsTouchingLayers(ground) && Mathf.Abs(rb.velocity.y) >= JUMPTHRESHOLD)
        {
            crouching = false;
        }
        if (Mathf.Abs(horizontalMove) > 0.2) // whether running
        {
            running = true;
        }
        else
        {
            running = false;
        }
        if (!running && !crouching && !jumping && !falling) // whether idling
        {
            idling = true;
        }
        else
        {
            idling = false;
        }
    }

    void UpdateAnimState()
    {
        anim.SetBool("jumping", jumping);
        anim.SetBool("falling", falling);
        anim.SetBool("idling", idling);
        anim.SetBool("running", running);
        anim.SetBool("crouching", crouching);
        anim.SetBool("onGround", onGround);
        anim.SetBool("isHurt", isHurt);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "CollectionItem")
        {
            if (other.name == "cherry") cherry += 1;
            cherryDisplay.text = cherry.ToString();
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        bool hitGroundCollider = (other.otherCollider == groundCollider);
        bool hitTopCollider = (other.otherCollider == topCollider);
        if (other.gameObject.tag == "Enemy")
        {
            if (falling && hitGroundCollider) // kick enemy
            {
                // jump attack
                EnemyAI enemy = other.gameObject.GetComponent<EnemyAI>();

                enemy.AttackOn(jumpAttack);
                // jump
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpAvailable = 2;
                jumpButton = false;
                jumped = true;
            }
            if (hitTopCollider || (hitGroundCollider && crouching)) // hurt by enemy
            {
                //push effect
                float xDifference = rb.position.x - other.rigidbody.position.x;
                rb.velocity = new Vector2(hurtForce * (xDifference / Mathf.Abs(xDifference)), rb.velocity.y);

                //hurt time
                isHurt = true;

                //reset status
                running = false;
                jumping = false;
                falling = false;
            }
        }
    }

    void ResetHurtTime()
    {
        isHurt = false;
        CancelInvoke();
    }
}

