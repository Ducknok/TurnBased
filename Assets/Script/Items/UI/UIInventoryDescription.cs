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
        public void SetATKDescription(Image weaponBar, HeroStateMachine hero, EquippableItemSO hoveredWeapon)
        {
            // 1. Lấy chỉ số gốc (curATK, curMATK đã bao gồm đồ đang mặc)
            float currentATK = hero.baseHero.curATK;
            float currentMATK = hero.baseHero.curMATK;

            float projectedATK = currentATK;
            float projectedMATK = currentMATK;
            bool canEquip = false;

            if (hoveredWeapon != null && (hero.baseHero.heroType == hoveredWeapon.allowedWeapons || hoveredWeapon.allowedWeapons == HeroType.All))
            {
                canEquip = true;

                // --- FIX BUG CHÍNH: TRỪ ĐI CHỈ SỐ CỦA VŨ KHÍ CŨ ĐANG MẶC ---
                AgentWeapon agentWeapon = hero.GetComponent<AgentWeapon>();
                if (agentWeapon != null)
                {
                    foreach (var eqItem in agentWeapon.weaponItemSO)
                    {
                        if (eqItem != null && eqItem.itemType == hoveredWeapon.itemType)
                        {
                            foreach (var mod in eqItem.Modifiers)
                            {
                                projectedATK -= mod.val1;
                                projectedMATK -= mod.val2;
                            }
                        }
                    }
                }

                // --- CỘNG CHỈ SỐ CỦA VŨ KHÍ MỚI ---
                foreach (var mod in hoveredWeapon.Modifiers)
                {
                    projectedATK += mod.val1;
                    projectedMATK += mod.val2;
                }
            }

            // 2. Gắn Avatar Khuôn mặt
            Transform heroIconParent = weaponBar.transform.Find("HeroIcon");
            if (heroIconParent != null)
            {
                Image hIcon = heroIconParent.Find("Icon")?.GetComponent<Image>();
                if (hIcon != null && hero.baseHero != null)
                {
                    hIcon.sprite = hero.baseHero.heroImage;
                    hIcon.color = Color.white;
                }
            }

            // BỎ cơ chế ẩn hiện shouldShowStats. Luôn luôn hiển thị!

            // 3. Hiển thị ATK (Có màu sắc)
            Transform atkIcon = weaponBar.transform.Find("ATKBG");
            if (atkIcon != null)
            {
                atkIcon.gameObject.SetActive(true); // Luôn luôn bật
                TextMeshProUGUI atkTxt = atkIcon.Find("ATKText_txt")?.GetComponent<TextMeshProUGUI>();
                if (atkTxt != null) atkTxt.text = FormatStatComparison(currentATK, projectedATK);
            }

            // 4. Hiển thị MATK (Có màu sắc)
            Transform matkIcon = weaponBar.transform.Find("MATKBG");
            if (matkIcon != null)
            {
                matkIcon.gameObject.SetActive(true); // Luôn luôn bật
                TextMeshProUGUI matkTxt = matkIcon.Find("MATKText_txt")?.GetComponent<TextMeshProUGUI>();
                if (matkTxt != null) matkTxt.text = FormatStatComparison(currentMATK, projectedMATK);
            }
        }
        public void SetupShieldHeroBarUI(Image shieldBar, HeroStateMachine hero, EquippableItemSO hoveredShield)
        {
            // 1. Lấy chỉ số gốc (curDEF, curMDEF đã bao gồm đồ đang mặc)
            float currentDEF = hero.baseHero.curDEF;
            float currentMDEF = hero.baseHero.curMDEF;

            float projectedDEF = currentDEF;
            float projectedMDEF = currentMDEF;
            bool canEquip = false;

            // 2. Tính toán sức mạnh thay đổi nếu trang bị khiên này
            if (hoveredShield != null && (hero.baseHero.heroType == hoveredShield.allowedWeapons || hoveredShield.allowedWeapons == HeroType.All))
            {
                canEquip = true;

                // Trừ đi chỉ số của khiên CŨ đang mặc
                AgentWeapon agentWeapon = hero.GetComponent<AgentWeapon>();
                if (agentWeapon != null)
                {
                    foreach (var eqItem in agentWeapon.weaponItemSO)
                    {
                        if (eqItem != null && eqItem.itemType == hoveredShield.itemType)
                        {
                            foreach (var mod in eqItem.Modifiers)
                            {
                                // Val 1 là DEF, Val 2 là MDEF
                                projectedDEF -= mod.val1;
                                projectedMDEF -= mod.val2;
                            }
                        }
                    }
                }

                // Cộng chỉ số của cái khiên MỚI đang trỏ chuột
                foreach (var mod in hoveredShield.Modifiers)
                {
                    projectedDEF += mod.val1;
                    projectedMDEF += mod.val2;
                }
            }

            // 3. Đổ Avatar Hero
            Transform heroIconParent = shieldBar.transform.Find("HeroIcon");
            if (heroIconParent != null)
            {
                Image hIcon = heroIconParent.Find("Icon")?.GetComponent<Image>();
                if (hIcon != null && hero.baseHero != null && hero.baseHero.heroImage != null)
                {
                    hIcon.sprite = hero.baseHero.heroImage;
                    hIcon.color = Color.white;
                }
            }

            // 4. Quyết định xem có hiển thị chỉ số hay không
            bool shouldShowStats = true;
            if (hoveredShield != null && !canEquip)
            {
                // Nếu đang trỏ chuột vào khiên mà Hero này KHÔNG THỂ mặc -> Ẩn luôn nguyên cụm (giống Ninja trong hình)
                shouldShowStats = false;
            }

            // Tạo chữ hiển thị (Đã xóa chữ DEF/MDEF)
            string defTextStr = FormatStatComparison(currentDEF, projectedDEF);
            string mdefTextStr = FormatStatComparison(currentMDEF, projectedMDEF);

            // 5. Đổ dữ liệu vào UI (Theo cấu trúc hình ảnh bạn cung cấp)
            Transform shieldIcon1 = shieldBar.transform.Find("ShieldIcon");
            if (shieldIcon1 != null)
            {
                shieldIcon1.gameObject.SetActive(shouldShowStats); // Tắt/bật cả icon lẫn text
                if (shouldShowStats)
                {
                    TextMeshProUGUI defTxt = shieldIcon1.Find("ShieldInfo_txt")?.GetComponent<TextMeshProUGUI>();
                    if (defTxt != null) defTxt.text = defTextStr;
                }
            }

            Transform shieldIcon2 = shieldBar.transform.Find("ShieldIcon (1)");
            if (shieldIcon2 != null)
            {
                shieldIcon2.gameObject.SetActive(shouldShowStats); // Tắt/bật cả icon lẫn text
                if (shouldShowStats)
                {
                    TextMeshProUGUI mdefTxt = shieldIcon2.Find("ShieldInfo_txt")?.GetComponent<TextMeshProUGUI>();
                    if (mdefTxt != null) mdefTxt.text = mdefTextStr;
                }
            }
        }
        public void SetupRingHeroBarUI(Image ringBar, HeroStateMachine hero)
        {
            // 1. Lấy danh sách các nhẫn Normal đang mặc
            List<EquippableItemSO> equippedRings = new List<EquippableItemSO>();
            AgentWeapon agentWeapon = hero.GetComponent<AgentWeapon>();

            if (agentWeapon != null)
            {
                foreach (var eqItem in agentWeapon.weaponItemSO)
                {
                    if (eqItem != null && eqItem.itemType == ItemType.Ring && eqItem.rarity == Rarity.Normal)
                    {
                        equippedRings.Add(eqItem);
                    }
                }
            }

            // 2. Gán Avatar khuôn mặt Hero vào ô HeroIcon
            Transform heroIconParent = ringBar.transform.Find("HeroIcon");
            if (heroIconParent != null)
            {
                Image hIcon = heroIconParent.Find("Icon")?.GetComponent<Image>();
                if (hIcon != null && hero.baseHero != null && hero.baseHero.heroImage != null)
                {
                    hIcon.sprite = hero.baseHero.heroImage;
                    hIcon.color = Color.white;
                }
            }

            // 3. Đổ dữ liệu vào ô Nhẫn 1 (RingBG1)
            Transform ringBG1 = ringBar.transform.Find("RingBG1");
            if (ringBG1 != null)
            {
                TextMeshProUGUI nameTxt = ringBG1.Find("RingName_txt")?.GetComponent<TextMeshProUGUI>();
                Image iconImg = ringBG1.Find("Icon")?.GetComponent<Image>();

                if (equippedRings.Count > 0)
                {
                    if (nameTxt != null) { nameTxt.text = equippedRings[0].Name.ToUpper(); nameTxt.color = Color.white; }
                    if (iconImg != null) { iconImg.sprite = equippedRings[0].ItemImage; iconImg.color = Color.white; }
                }
                else
                {
                    if (nameTxt != null) { nameTxt.text = "------"; nameTxt.color = new Color(0.5f, 0.5f, 0.5f, 1f); }
                    if (iconImg != null) { iconImg.color = new Color(1f, 1f, 1f, 0f); }
                }
            }

            Transform ringBG2 = ringBar.transform.Find("RingBG2");
            if (ringBG2 != null)
            {
                TextMeshProUGUI nameTxt = ringBG2.Find("RingName_txt")?.GetComponent<TextMeshProUGUI>();
                Image iconImg = ringBG2.Find("Icon")?.GetComponent<Image>();

                if (equippedRings.Count > 1)
                {
                    if (nameTxt != null) { nameTxt.text = equippedRings[1].Name.ToUpper(); nameTxt.color = Color.white; }
                    if (iconImg != null) { iconImg.sprite = equippedRings[1].ItemImage; iconImg.color = Color.white; }
                }
                else
                {
                    if (nameTxt != null) { nameTxt.text = "------"; nameTxt.color = new Color(0.5f, 0.5f, 0.5f, 1f); }
                    if (iconImg != null) { iconImg.color = new Color(1f, 1f, 1f, 0f); }
                }
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
        private string FormatStatComparison(float currentVal, float projectedVal)
        {
            if (currentVal == projectedVal)
            {
                return $"{currentVal} -> {projectedVal}";
            }
            else if (projectedVal > currentVal)
            {
                // Tăng sức mạnh -> Mũi tên và số mới đổi màu Xanh lam dương
                return $"{currentVal} <color=#58C7E2>-> {projectedVal}</color>";
            }
            else
            {
                // Bị giảm sức mạnh -> Mũi tên và số mới đổi màu Đỏ
                return $"{currentVal} <color=#FF4C4C>-> {projectedVal}</color>";
            }
        }
    }
}