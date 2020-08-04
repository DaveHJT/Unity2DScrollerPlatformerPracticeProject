using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogAI : EnemyAI
{

    public float jumpForce = 20f;
    public float attackDelay = 3;
    public bool jumping, falling;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb.mass = 3;
        healthPoints = 100;
        InvokeRepeating("jumpToPlayer", attackDelay, attackDelay);
    }

    void jumpToPlayer()
    {
        float xDifference = rbPlayer.position.x - rb.position.x;
        float yDifference = rbPlayer.position.y - rb.position.y;
        float xDirection = xDifference / Mathf.Abs(xDifference);
        float yDirection = yDifference / Mathf.Abs(yDifference);
        float yForce = jumpForce;
        if (yDirection <= 0.2) yForce /= 2;
        if (idling) { rb.velocity = new Vector2(xDifference, yForce); }

        transform.localScale = new Vector3(-1 * xDirection * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        jumping = true;
    }
    protected override void UpdateStatus()
    {
        if (jumping)
        {
            idling = false;
            falling = false;
        }
        if (rb.velocity.y == 0)
        {
            idling = true;
            jumping = false;
            falling = false;
        }
        else if (rb.velocity.y <= -0.2)
        {
            jumping = false;
            falling = true;
            idling = false;
        }
        base.UpdateStatus();
    }

    protected override void UpdateAnimState()
    {
        anim.SetBool("jumping", jumping);
        anim.SetBool("falling", falling);
        anim.SetBool("idling", idling);
        anim.SetBool("isDead", isDead);
    }
}
