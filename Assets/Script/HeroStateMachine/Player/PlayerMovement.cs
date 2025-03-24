using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController playerCtrl; 
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected GameObject body { get; private set; }
    [SerializeField] protected Animator anim;
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected Vector2 movement;
    protected void Awake()
    {
        this.body = this.transform.parent.Find("Body").gameObject;
        this.rb = this.body.GetComponent <Rigidbody2D>();
        this.anim = this.body.GetComponent<Animator>();
        this.playerCtrl = GetComponentInParent<PlayerController>();
    }
    protected void Update()
    {
        this.anim.SetFloat("Horizontal", this.movement.x);
        this.anim.SetFloat("Vertical", this.movement.y);
        this.anim.SetFloat("Speed", this.movement.sqrMagnitude);
        if(this.movement != Vector2.zero)
        {
            this.anim.SetFloat("LastHorizontal", this.movement.x);
            this.anim.SetFloat("LastVertical", this.movement.y);
        }
    }
    protected void FixedUpdate()
    {
        if (this.playerCtrl.CombatZone.isInCombat) return;
        this.movement.Set(InputManager.Movement.x, InputManager.Movement.y);
        this.rb.velocity = this.movement * this.moveSpeed;
    }
}
