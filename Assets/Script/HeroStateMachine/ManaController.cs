using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ManaController : MonoBehaviour
{
    private static ManaController instance;
    public static ManaController Instance => instance;
    private float trailDelay = 0.4f;
    protected void Awake()
    {
        if (instance != null) return;
        instance = this;
    }
    public void ManaBar(HeroStateMachine hsm)
    {
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
            //Debug.Log("Het mana");
        }
    }
    public void RestoreMana(HeroStateMachine hsm)
    {
        hsm.baseHero.curMP += 3f;

        // ??m b?o mana kh?ng v??t qu¨¢ gi¨¢ tr? t?i ?a
        if (hsm.baseHero.curMP > hsm.baseHero.baseMP)
        {
            hsm.baseHero.curMP = hsm.baseHero.baseMP;
        }
        this.UpdateManaBar(hsm);
        
    }
    public void RestoreManaAfterRevive(HeroStateMachine hsm)
    {
        hsm.baseHero.curMP = Mathf.FloorToInt(hsm.baseHero.baseMP * 0.5f); // H?i 50% m¨¢u
        this.UpdateManaBar(hsm);
    }
    public void UpdateManaBar(HeroStateMachine hsm)
    {
        if (hsm.baseHero.curMP > hsm.baseHero.baseMP)
        {
            hsm.baseHero.curMP = hsm.baseHero.baseMP;
        }
        float ratio = hsm.baseHero.curMP / hsm.baseHero.baseMP;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(hsm.heroMPBarFill.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
        sequence.AppendInterval(this.trailDelay);
        sequence.Append(hsm.heroMPBarTrail.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
        sequence.Play();
    }
    public void DescreaseMana(HeroStateMachine hsm)
    {
        hsm.baseHero.curMP -= CombatController.Instance.CBM.performList[0].choosenAttack.attackCost;
        float ratio = hsm.baseHero.curMP / hsm.baseHero.baseMP;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(hsm.heroMPBarFill.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
        sequence.AppendInterval(this.trailDelay);
        sequence.Append(hsm.heroMPBarTrail.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
        sequence.Play();
    }
}
