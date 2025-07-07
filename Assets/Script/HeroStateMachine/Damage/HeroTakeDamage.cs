using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroTakeDamage : TakeDamageController
{
    private CinemachineImpulseSource impulseSource;
    protected override void Start()
    {
        this.impulseSource = this.transform.GetComponent<CinemachineImpulseSource>();
    }
    public override void TakeDamage(GameObject target, float getDamageAmount)
    {
        HeroStateMachine hsm = target.GetComponent<HeroStateMachine>();
        this.DamagePop(hsm, getDamageAmount);
        CameraShakeManager.instance.CameraShake(this.impulseSource);
        HealController.Instance.HPBar(hsm);
        hsm.heroPanelHandler.UpdateHeroPanel();
    }
    private void DamagePop(HeroStateMachine hsm, float getDamageAmount)
    {
        bool isCritical = Random.Range(0, 100) < 5;
        if (isCritical) getDamageAmount *= 2;
        DamagePopup.Create(this.transform.Find("Body").position, getDamageAmount, isCritical, false);
        hsm.baseHero.curHP -= getDamageAmount;
    }
    public override void TakeDamage(GameObject target, float getDamageAmount, BaseAttack.Effect attackType1, BaseAttack.Effect attackType2)
    {
        throw new System.NotImplementedException();
    }
}
