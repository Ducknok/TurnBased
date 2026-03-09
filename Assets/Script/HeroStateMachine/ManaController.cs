using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ManaController : Singleton<ManaController>
{
    private float trailDelay = 0.4f;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ManaBar(HeroStateMachine hsm)
    {
        if (CombatController.Instance.CBM.performList == null || CombatController.Instance.CBM.performList.Count == 0) return;

        if (CombatController.Instance.CBM.performList[0].choosenAttack.attackType == BaseAttack.AttackType.NormalAttack)
        {
            this.RestoreMana(hsm);
        }
        else
        {
            this.DescreaseMana(hsm);
        }

        if (hsm.baseHero.curMP <= 0)
        {
            hsm.baseHero.curMP = 0;
        }
    }

    public void RestoreMana(HeroStateMachine hsm)
    {
        hsm.baseHero.curMP += 3;

        if (hsm.baseHero.curMP > hsm.baseHero.baseMP)
        {
            hsm.baseHero.curMP = hsm.baseHero.baseMP;
        }
        this.UpdateManaBar(hsm);
    }

    public void RestoreManaAfterRevive(HeroStateMachine hsm)
    {
        hsm.baseHero.curMP = Mathf.FloorToInt(hsm.baseHero.baseMP * 0.5f); 
        this.UpdateManaBar(hsm);
    }

    public void DescreaseMana(HeroStateMachine hsm)
    {
        if (CombatController.Instance.CBM.performList == null || CombatController.Instance.CBM.performList.Count == 0) return;

        hsm.baseHero.curMP -= CombatController.Instance.CBM.performList[0].choosenAttack.attackCost;
        if (hsm.baseHero.curMP < 0) hsm.baseHero.curMP = 0;

        this.UpdateManaBar(hsm);
    }

    public void UpdateManaBar(HeroStateMachine hsm)
    {
        if (hsm == null || hsm.heroPanelHandler == null || hsm.heroPanelHandler.heroMPBarFill == null)
        {
            return; 
        }

        if (hsm.baseHero.curMP > hsm.baseHero.baseMP)
        {
            hsm.baseHero.curMP = hsm.baseHero.baseMP;
        }

        float ratio = hsm.baseHero.curMP / hsm.baseHero.baseMP;

        hsm.heroPanelHandler.heroMPBarFill.DOKill();
        hsm.heroPanelHandler.heroMPBarTrail.DOKill();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(hsm.heroPanelHandler.heroMPBarFill.DOFillAmount(ratio, 0.25f).SetEase(Ease.InOutSine));
        sequence.AppendInterval(this.trailDelay);
        sequence.Append(hsm.heroPanelHandler.heroMPBarTrail.DOFillAmount(ratio, 0.3f).SetEase(Ease.InOutSine));

        sequence.SetLink(hsm.heroPanelHandler.heroMPBarFill.gameObject);

        sequence.Play();

        if (hsm.heroPanelHandler.GetComponent<HeroPanelStats>() != null)
        {
            hsm.heroPanelHandler.GetComponent<HeroPanelStats>().heroMP.text = Mathf.Max(0, hsm.baseHero.curMP).ToString();
        }
    }
}