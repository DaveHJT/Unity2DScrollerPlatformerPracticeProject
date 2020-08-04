using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D cd;
    public Rigidbody2D rbPlayer;
    public float healthPoints;
    public bool isDead = false;
    public bool idling;
    public float facingDirection;
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        cd = gameObject.GetComponent<Collider2D>();
        rb.drag = 1;
        rb.mass = 1;
        idling = true;
    }

    // Update is called once per frame
    protected virtual void Update()
    {

        UpdateStatus();
        UpdateAnimState();
    }

    protected virtual void UpdateAnimState()
    {
        anim.SetBool("idling", idling);
        anim.SetBool("isDead", isDead);
    }
    protected virtual void UpdateStatus()
    {
        // update facing direction
        facingDirection = -1 * transform.localScale.x / Mathf.Abs(transform.localScale.x);

        if (healthPoints <= 0) // check death
        {
            isDead = true;
            CancelInvoke();
            cd.enabled = false;
            rb.Sleep();
        }
    }

    public virtual void AttackOn(float damage)
    {
        healthPoints -= damage;
    }

    public virtual void Kill()
    {
        Destroy(gameObject);
    }
}
