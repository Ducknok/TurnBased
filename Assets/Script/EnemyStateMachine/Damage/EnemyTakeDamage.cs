using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTakeDamage : TakeDamageController
{
    private float trailDelay;
    private CinemachineImpulseSource impulseSource;
    protected void Start()
    {
        this.impulseSource = this.transform.GetComponent<CinemachineImpulseSource>();
    }
    public override void TakeDamage(GameObject target, float getDamageAmount)
    {
        throw new System.NotImplementedException();
    }

    public override void TakeDamage(GameObject target, float getDamageAmount, BaseAttack.Effect attackType1, BaseAttack.Effect attackType2)
    {
        CameraShakeManager.instance.CameraShake(impulseSource);
        EnemyStateMachine esm = target.GetComponent<EnemyStateMachine>();
        // Ki?m tra n?u enemy c¨® Lock
        bool hasLocks = esm.activeLocks.Count > 0;
        bool allLocksBroken = false;

        if (hasLocks)
        {
            // Ki?m tra t?t c? Lock hi?n c¨®
            foreach (var lockSystem in esm.activeLocks)
            {
                lockSystem.TryBreakLock(attackType1, attackType2);
                esm.enemyUI.GrayOutAttackType(attackType1, attackType2);
            }

            // N?u t?t c? Lock b? ph¨¢, t?ng s¨¢t th??ng
            allLocksBroken = esm.activeLocks.TrueForAll(lockSystem => lockSystem.IsBroken());
        }

        if (hasLocks && allLocksBroken && !esm.isLockBrokenOnce)
        {
            getDamageAmount *= 1.5f; // T?ng 50% s¨¢t th??ng khi Lock b? ph¨¢
            esm.isLockBrokenOnce = true;
            StartCoroutine(esm.enemyUI.ClearAllAttackTypeIcons());
            // Debug.Log("?? Lock b? ph¨¢! G?y th¨ºm s¨¢t th??ng!");
        }

        // X? l? ch¨ª m?ng
        bool isCritical = Random.Range(0, 100) < 20;
        if (isCritical) getDamageAmount *= 2;

        // Hi?n th? popup s¨¢t th??ng
        DamagePopup.Create(esm.transform.Find("Body").position, getDamageAmount, isCritical, false);
        esm.baseEnemy.curHP -= getDamageAmount;
        esm.curHpNumber.text = esm.baseEnemy.curHP.ToString();

        // C?p nh?t thanh m¨¢u
        float ratio = esm.baseEnemy.curHP /  esm.baseEnemy.baseHP;
        if (esm.enemyHPBarFill != null)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(esm.enemyHPBarFill.DOFillAmount(ratio, 0.25f).SetEase(Ease.InOutSine));
            sequence.AppendInterval(this.trailDelay);
            sequence.Play();
        }

        StartCoroutine(esm.ClearEnemyInfo());

        // Ki?m tra ch?t
        if (esm.baseEnemy.curHP <= 0)
        {
            esm.baseEnemy.curHP = 0;
            esm.curHpNumber.text = esm.baseEnemy.curHP.ToString();
            StartCoroutine(this.DeadSequence(esm));
        }
    }
    IEnumerator DeadSequence(EnemyStateMachine esm)
    {
        yield return new WaitForSeconds(1f);
        esm.currentState = EnemyStateMachine.TurnState.DEAD;
    }
    


}
