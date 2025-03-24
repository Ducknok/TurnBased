using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattle : MonoBehaviour
{
    [SerializeField] protected PlayerController playerCtrl;
    [SerializeField] protected GameObject Enemy;
    Animator anim;
    private void Awake()
    {
        this.playerCtrl = GetComponentInParent<PlayerController>();
    }

    public void Attack()
    {
        Vector3 attackDir = (this.Enemy.transform.position - this.transform.position).normalized;
    }

    public void PlayerInBattle()
    {
        //Debug.LogWarning(this.playerCtrl.transform.Find("Body").GetComponent<Animator>());
        this.playerCtrl.transform.Find("Body").GetComponent<Animator>().SetBool("IdleBattle", true);
    }

}
