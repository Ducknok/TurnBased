using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealController : Singleton<HealController>
{
    private float trailDelay = 0.4f;
    protected override void Awake()
    {
        base.Awake();
    }
    public void HPBar(HeroStateMachine hsm)
    {
        float ratio = (float)hsm.baseHero.curHP / hsm.baseHero.baseHP;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(hsm.heroPanelHandler.heroHPBarFill.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
        sequence.AppendInterval(this.trailDelay);
        sequence.Append(hsm.heroPanelHandler.heroHPBarTrail.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
        sequence.Play();

        if (hsm.baseHero.curHP <= 0)
        {
            hsm.baseHero.curHP = 0;
            hsm.currentState = HeroStateMachine.TurnState.DEAD;
        }
    }
    public void RestoreHPAfterRevive(HeroStateMachine hsm)
    {
        // T?ng mana hi?n t?i
        hsm.baseHero.curHP = Mathf.FloorToInt(hsm.baseHero.baseHP * 0.5f); // H?i 50% m¨¢u

        // ??m b?o mana kh?ng v??t qu¨¢ gi¨¢ tr? t?i ?a
        if (hsm.baseHero.curHP > hsm.baseHero.baseHP)
        {
            hsm.baseHero.curHP = hsm.baseHero.baseHP;
        }

        // T¨ªnh t? l? mana ?? c?p nh?t thanh UI
        float ratio = (float)hsm.baseHero.curHP / hsm.baseHero.baseHP;

        // T?o Sequence ?? l¨¤m animation
        Sequence sequence = DOTween.Sequence();
        sequence.Append(hsm.heroPanelHandler.heroHPBarFill.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
        sequence.AppendInterval(this.trailDelay);
        sequence.Append(hsm.heroPanelHandler.heroHPBarTrail.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
        sequence.Play();
    }

    
}
