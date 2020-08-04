using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomExtensions;
public class EagleAI : EnemyAI
{
    public Transform leftBound, rightBound;
    public float sightDistance = 30f;
    public bool attacking, returning, aiming, gliding;
    private LayerMask visibleLayers = (1 << 8) | (1 << 9) | (1 << 10);
    public bool attackAvailable = false;

    public float patrolBaseSpeed = 3;
    public float patrolBaseAcc = 9;
    public float flyingTime = 0;
    public float accTime = 1;
    public float arriveDistance = 1;
    public Vector2 targetPosition, returnPosition, aimDirection;

    private Vector2 offset = new Vector2(1.02f, -0.5f);

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0;
        rb.drag = 3;
        transform.DetachChildren();
        healthPoints = 50;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        flyingTimer();


        behaviourAI();
    }

    void flyingTimer()
    {
        if (rb.velocity.x == 0) flyingTime = 0;
        flyingTime += Time.deltaTime;
    }

    void behaviourAI()
    {
        if (idling) patrol();
        if (attacking) diveAtTarget();
        if (returning) returnToPatrol();
    }

    void returnToPatrol()
    {
        // calculate direction of returning
        aimDirection = returnPosition - transform.position.toVector2();
        aimDirection /= aimDirection.magnitude;
        float patrolSpeed = patrolBaseSpeed + getPatrolSpeed(transform.position.toVector2(), returnPosition);
        rb.velocity = new Vector2(aimDirection.x * patrolSpeed, aimDirection.y * patrolSpeed);
    }

    void aimTarget(Rigidbody2D target, bool changeReturn)
    {
        // get target position
        targetPosition = target.transform.position.toVector2();
        returnPosition = new Vector2(transform.position.x, leftBound.position.y);
    }

    void diveAtTarget()
    {
        float maxSpeed = (patrolBaseAcc);
        float diveSpeed = Mathf.Min(flyingTime, accTime) / accTime * maxSpeed;
        // calculate direction of diving
        aimDirection = targetPosition - transform.position.toVector2();
        aimDirection /= aimDirection.magnitude;
        rb.velocity = new Vector2(aimDirection.x * diveSpeed, aimDirection.y * diveSpeed);
    }

    float getPatrolSpeed(Vector2 from, Vector2 to)
    {
        float range = Vector2.Distance(from, to);
        Vector2 midPoint = Vector2.Lerp(from, to, 0.5f);
        float distanceToMid = Vector2.Distance(transform.position.toVector2(), midPoint);
        float distanceToBound = Mathf.Abs(range / 2 - distanceToMid);
        float patrolSpeed = patrolBaseAcc * (distanceToBound / range) + patrolBaseSpeed;

        return patrolSpeed;
    }
    void patrol()
    {
        float patrolSpeed = getPatrolSpeed(leftBound.position.toVector2(), rightBound.position.toVector2());

        if (isInBound(rb))
        {
            rb.velocity = new Vector2(patrolSpeed * facingDirection, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(patrolBaseSpeed * facingDirection, rb.velocity.y);
            // U-turn
            bool wrongDirection = (transform.position.x < leftBound.position.x && facingDirection < 0) || (transform.position.x > rightBound.position.x && facingDirection > 0);
            if (wrongDirection)
            {
                transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
    }

    bool isInSight(Rigidbody2D enemy)
    {
        Vector2 eyePosition = new Vector2(rb.position.x + facingDirection * offset.x, rb.position.y + offset.y);
        Vector2 heading = enemy.position - eyePosition;
        Vector2 direction = heading / heading.magnitude;
        Debug.DrawRay(eyePosition, direction * sightDistance, Color.red);
        RaycastHit2D rayHit = Physics2D.Raycast(eyePosition, direction, sightDistance, visibleLayers);

        return rayHit.rigidbody && rayHit.rigidbody.Equals(enemy);
    }

    bool isInBound(Rigidbody2D entity)
    {
        float xPosition = entity.transform.position.x;
        return (xPosition >= leftBound.position.x && xPosition <= rightBound.position.x);
    }

    protected override void UpdateAnimState()
    {
        anim.SetBool("gliding", gliding);
        anim.SetBool("isDead", isDead);
    }

    protected override void UpdateStatus()
    {
        if (rb.velocity.y < 0) { gliding = true; } else { gliding = false; }
        attackAvailable = isInSight(rbPlayer) && isInBound(rbPlayer);
        if (idling && attackAvailable && isInBound(rb))
        {
            attacking = true;
            idling = false;
            aimTarget(rbPlayer, true);
        }

        if (attacking && (!isInBound(rb) || Vector2.Distance(targetPosition, transform.position) <= arriveDistance || cd.IsTouchingLayers(visibleLayers)))
        {
            if (!attackAvailable)
            {
                attacking = false;
                returning = true;
                // turn arround
                transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            else
            {
                returning = false;
                attacking = true;
                aimTarget(rbPlayer, false);
            }

        }
        if (attacking && rb.velocity.magnitude == 0)
        {
            attacking = false;
            returning = true;
            // turn arround
            transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);

        }
        if (returning && Vector2.Distance(returnPosition, transform.position) <= arriveDistance)
        {
            returning = false;
            idling = true;
        }
        else if (returning && attackAvailable)
        {
            attacking = true;
            returning = false;
            aimTarget(rbPlayer, false);

        }

        base.UpdateStatus();
    }
    public override void Kill()
    {
        base.Kill();
        Destroy(leftBound.gameObject);
        Destroy(rightBound.gameObject);
    }

}
