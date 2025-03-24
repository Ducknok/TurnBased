using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private HeroStateMachine hero;
    [SerializeField] protected Animator anim;

    protected void Start()
    {
        if (this.anim != null) return;
        this.anim = this.transform.GetComponent<Animator>();
        if (this.hero != null) return;
        this.hero = this.transform.parent.GetComponent<HeroStateMachine>();
    }
    public void AnimatedEnd()
    {
        this.anim.ResetTrigger("Attack_1");
    }
}
