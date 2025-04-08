using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

//View (V) in MVC
namespace Inventory.UI
{
    public class UIInventoryDescription : MonoBehaviour
    {
        [Header("Item")]
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI receiveEffect;
        [SerializeField] private TextMeshProUGUI description;
        private float trailDelay = 0.4f;

        public void Awake()
        {
            ResetDescription();
        }
        public void ResetDescription()
        {
            this.itemImage.gameObject.SetActive(false);
            this.title.text = "";
            this.receiveEffect.text = "";
            this.description.text = "";
        }
        public void SetItemDescription(Sprite sprite, string itemName, string itemEffect, string itemDescription)
        {
            this.itemImage.gameObject.SetActive(true);
            this.itemImage.sprite = sprite;
            this.title.text = itemName;
            this.receiveEffect.text = itemEffect;
            this.description.text = itemDescription;
        }
        public void SetHeroDescription(Image image, HeroStateMachine hero) 
        {
            Image heroButton = image.transform.Find("HeroIcon").Find("Icon").GetComponent<Image>();
            TextMeshProUGUI curHP = image.transform.Find("HP").Find("CurHPNumber").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI maxHP = image.transform.Find("HP").Find("MaxHPNumber").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI curMP = image.transform.Find("MP").Find("CurMPNumber").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI maxMP = image.transform.Find("MP").Find("MaxMPNumber").GetComponent<TextMeshProUGUI>();
            Image hpBarFill = image.transform.Find("HPBar").Find("Fill").GetComponent<Image>();
            Image mpBarFill = image.transform.Find("MPBar").Find("Fill").GetComponent<Image>();

            heroButton.sprite = hero.baseHero.heroImage;
            curHP.text = hero.baseHero.curHP.ToString();
            maxHP.text = hero.baseHero.baseHP.ToString();
            curMP.text = hero.baseHero.curMP.ToString();
            maxMP.text = hero.baseHero.baseMP.ToString();
            hpBarFill.fillAmount = 1f;
            mpBarFill.fillAmount = 1f;
            this.UpdateHPBar(hpBarFill, hero);
            this.UpdateMPBar(mpBarFill, hero);

        }
        public void UpdateHPBar(Image hpBar, HeroStateMachine hero)
        {
            float ratio = hero.baseHero.curHP / hero.baseHero.baseHP;


            Sequence sequence = DOTween.Sequence();
            sequence.Append(hpBar.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
            sequence.AppendInterval(this.trailDelay);
            sequence.Append(hpBar.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
            sequence.Play();

            if (hero.baseHero.curHP <= 0)
            {
                hero.baseHero.curHP = 0;
            }
        }
        public void UpdateMPBar(Image mpBar, HeroStateMachine hero)
        {
            float ratio = hero.baseHero.curMP / hero.baseHero.baseMP;


            Sequence sequence = DOTween.Sequence();
            sequence.Append(mpBar.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
            sequence.AppendInterval(this.trailDelay);
            sequence.Append(mpBar.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
            sequence.Play();

            if (hero.baseHero.curMP <= 0)
            {
                hero.baseHero.baseMP = 0;
            }
        }
    }
}