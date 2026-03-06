using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Inventory.Model;

//View (V) in MVC
namespace Inventory.UI
{
    public class UIInventoryDescription : DucMonobehaviour
    {
        [Header("Item")]
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI receiveEffect;
        [SerializeField] private TextMeshProUGUI description;
        private float trailDelay = 0.4f;

        protected override void Awake()
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
        public void SetHeroBarDescription(Image image, HeroStateMachine hero) 
        {
            Image heroButton = image.transform.Find("HeroIcon").Find("Icon").GetComponent<Image>();
            TextMeshProUGUI curHP = image.transform.Find("HP").Find("CurHPNumber").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI curMP = image.transform.Find("MP").Find("CurMPNumber").GetComponent<TextMeshProUGUI>();
            Image hpBarFill = image.transform.Find("HPBar").Find("Fill").GetComponent<Image>();
            Image mpBarFill = image.transform.Find("MPBar").Find("Fill").GetComponent<Image>();

            heroButton.sprite = hero.baseHero.heroImage;
            curHP.text = $"{hero.baseHero.curHP}/{hero.baseHero.baseHP}";
            curMP.text = $"{hero.baseHero.curMP}/{hero.baseHero.baseMP}";
            hpBarFill.fillAmount = 1f;
            mpBarFill.fillAmount = 1f;
            this.UpdateHPBar(hpBarFill, hero);
            this.UpdateMPBar(mpBarFill, hero);

        }
        public void SetHeroUIDescription(GameObject image, HeroStateMachine hero)
        {
            Image heroButton = image.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI name = image.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI hp = image.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI mp = image.transform.Find("MPText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI hpValue = image.transform.Find("HPText").Find("HPNumber").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI mpValue = image.transform.Find("MPText").Find("MPNumber").GetComponent<TextMeshProUGUI>();

            if (hero == null || hero.baseHero == null)
            {
                // Nếu không có hero -> để trống hoặc ẩn nội dung
                heroButton.gameObject.SetActive(false);
                name.text = "";
                hp.text = "";
                mp.text = "";
                hpValue.text = "";
                mpValue.text = "";
                return;
            }

            // Có hero -> hiển thị như bình thường
            heroButton.sprite = hero.baseHero.heroImage;
            name.text = hero.baseHero.theName;
            hpValue.text = $"{hero.baseHero.curHP} / {hero.baseHero.baseHP}";
            mpValue.text = $"{hero.baseHero.curMP} / {hero.baseHero.baseMP}";
        }
        public void SetATKDescription(Image image, HeroStateMachine hero, ItemSO item)
        {
            Image heroBar = image.transform.Find("HeroIcon/Icon").GetComponent<Image>();
            TextMeshProUGUI atk = image.transform.Find("ATKBG/ATKText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI matk = image.transform.Find("MATKBG/MATKText").GetComponent<TextMeshProUGUI>();

            heroBar.sprite = hero.baseHero.heroImage;

            // Nếu item null hoặc không phải EquippableItemSO → chỉ hiện icon, ẩn text
            if (item is EquippableItemSO eqItem && eqItem.allowedWeapons == hero.baseHero.heroType)
            {
                float bonusATK = 0f;
                float bonusMATK = 0f;

                foreach (var mod in eqItem.Modifiers)
                {
                    if (mod.stat != null)
                    {
                        bonusATK += mod.val1;
                        bonusMATK += mod.val2;
                    }
                }

                atk.text = $"{hero.baseHero.baseATK} -> {hero.baseHero.baseATK + bonusATK}";
                matk.text = $"{hero.baseHero.baseMATK} -> {hero.baseHero.baseMATK + bonusMATK}";
                atk.transform.parent.gameObject.SetActive(true);
                matk.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                atk.transform.parent.gameObject.SetActive(false);  // Ẩn ATKBG
                matk.transform.parent.gameObject.SetActive(false); // Ẩn MATKBG
            }
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