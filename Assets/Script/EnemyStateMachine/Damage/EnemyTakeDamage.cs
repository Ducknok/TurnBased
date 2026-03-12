using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTakeDamage : TakeDamageController
{
    private float trailDelay;
    private CinemachineImpulseSource impulseSource;
    protected override void Start()
    {
        this.impulseSource = this.transform.GetComponent<CinemachineImpulseSource>();
    }
    public override void TakeDamage(GameObject target, int getDamageAmount)
    {
        throw new System.NotImplementedException();
    }

    public override void TakeDamage(GameObject target, int getDamageAmount, BaseAttack.Effect attackType1, BaseAttack.Effect attackType2)
    {
        CameraShakeManager.instance.CameraShake(impulseSource);
        EnemyStateMachine esm = target.GetComponent<EnemyStateMachine>();
        int finalDamage = this.CheckLock(esm, getDamageAmount, attackType1, attackType2);
        this.DamagePop(esm, finalDamage);
        float ratio = (float)esm.baseEnemy.curHP /  esm.baseEnemy.baseHP;
        if (esm.enemyHPBarFill != null)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(esm.enemyHPBarFill.DOFillAmount(ratio, 0.25f).SetEase(Ease.InOutSine));
            sequence.AppendInterval(this.trailDelay);
            sequence.Play();
        }

        StartCoroutine(esm.ClearEnemyInfo());

        if (esm.baseEnemy.curHP <= 0)
        {
            esm.baseEnemy.curHP = 0;
            esm.curHpNumber.text = esm.baseEnemy.curHP.ToString();
            StartCoroutine(this.DeadSequence(esm));
        }
    }
    private int CheckLock(EnemyStateMachine esm, int getDamageAmount, BaseAttack.Effect attackType1, BaseAttack.Effect attackType2)
    {
        bool hasLocks = esm.activeLocks.Count > 0;
        bool allLocksBroken = false;

        if (hasLocks)
        {
            foreach (var lockSystem in esm.activeLocks)
            {
                lockSystem.TryBreakLock(attackType1, attackType2);
                esm.enemyUI.GrayOutAttackType(attackType1, attackType2);
            }
            allLocksBroken = esm.activeLocks.TrueForAll(lockSystem => lockSystem.IsBroken());
        }

        if (hasLocks && allLocksBroken && !esm.isLockBrokenOnce)
        {
            getDamageAmount = Mathf.CeilToInt(getDamageAmount * 1.5f);
            esm.isLockBrokenOnce = true;
            StartCoroutine(esm.enemyUI.ClearTimerIcon());
            StartCoroutine(esm.enemyUI.ClearAllAttackTypeIcons());
            esm.currentState = EnemyStateMachine.TurnState.WAITING;
        }

        return getDamageAmount; 
    }
    private void DamagePop(EnemyStateMachine esm, int getDamageAmount)
    {
        bool isCritical = Random.Range(0, 100) < 20;
        if (isCritical) getDamageAmount *= 2;

        int finalDamage = Mathf.CeilToInt(getDamageAmount);

        Transform body = esm.transform.Find("Body");
        Vector3 popPos = body != null ? body.position : esm.transform.position;
        DamagePopup.Create(popPos, finalDamage, isCritical, false);

        if (esm.baseEnemy != null)
        {
            esm.baseEnemy.curHP -= finalDamage;
        }

        if (esm.curHpNumber != null)
        {
            esm.curHpNumber.text = esm.baseEnemy.curHP.ToString();
        }

        if (esm.enemyHPBarFill != null && esm.baseEnemy != null)
        {
            esm.enemyHPBarFill.fillAmount = (float)esm.baseEnemy.curHP / esm.baseEnemy.baseHP;
        }
    }
    IEnumerator DeadSequence(EnemyStateMachine esm)
    {
        yield return new WaitForSeconds(1f);
        esm.currentState = EnemyStateMachine.TurnState.DEAD;
    }
    


}
