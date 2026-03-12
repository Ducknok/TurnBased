using Inventory;
using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipMenuController : DucMonobehaviour
{
    public bool isEquipMenuOpen = false;
    [Header("Hero")]
    [SerializeField] private Image heroImage;
    [SerializeField] private TextMeshProUGUI heroName;
    [SerializeField] private TextMeshProUGUI heroType;

    [Header("Weapon UI")]
    [SerializeField] private GameObject weaponUI;
    [SerializeField] private Transform weaponUISpacer;
    private List<GameObject> currentWeaponUIs = new List<GameObject>();
    private int currentWeaponIndex = 0;

    public List<EquippableItemSO> currentWeapon = new List<EquippableItemSO>();
    private List<string> currentWeaponTypeStrings = new List<string>();
    private List<int> currentWeaponSlotIndexes = new List<int>();
    private List<Rarity?> currentWeaponSlotRarities = new List<Rarity?>(); // Lưu trữ độ hiếm cần thiết cho từng slot

    [Header("Shield & Ring UI")]
    [SerializeField] private Transform shieldUISpacer;
    [SerializeField] private Transform ringUISpacer;

    [Header("Hero Stat")]
    [SerializeField] private TextMeshProUGUI hpValue;
    [SerializeField] private TextMeshProUGUI mpValue;
    [SerializeField] private TextMeshProUGUI atkValue;
    [SerializeField] private TextMeshProUGUI defValue;
    [SerializeField] private TextMeshProUGUI mAtkValue;
    [SerializeField] private TextMeshProUGUI mDefValue;

    [Header("Hero Swap")]
    [SerializeField] private Button heroSwapButtonPrefab;
    [SerializeField] private GameObject heroSwapButtonSpacer;
    private List<Button> heroSwapButtons = new List<Button>();
    private int currentSwapIndex = 0;
    private List<HeroStateMachine> heroesInCombat = new List<HeroStateMachine>();

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI desText;

    [Header("ItemPanel")]
    [SerializeField] public bool isItemPanel;
    [SerializeField] private GameObject itemPanel;
    [SerializeField] private GameObject spacer;
    private List<GameObject> itemPanelUIs = new List<GameObject>();
    private int currentItemPanelIndex = 0;
    private List<EquippableItemSO> currentItemPanelItems = new List<EquippableItemSO>();

    protected override void Update()
    {
        this.CheckState();
    }
    public override void CheckState()
    {
        base.CheckState();
        if (!this.isEquipMenuOpen || heroSwapButtons.Count == 0)
        {
            return;
        }
        if (isItemPanel)
        {
            HandleItemPanelInput();
            return;
        }

        this.HandleSelectHero();
        this.HandleSelectWeapon();
    }
    public void HandleSelectHero()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentSwapIndex = (currentSwapIndex - 1 + heroSwapButtons.Count) % heroSwapButtons.Count;
            LoadCurrentHeroUI();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            currentSwapIndex = (currentSwapIndex + 1) % heroSwapButtons.Count;
            LoadCurrentHeroUI();
        }
    }
    private void HandleSelectWeapon()
    {
        if (currentWeaponUIs.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.currentWeaponIndex = (currentWeaponIndex - 1 + currentWeaponUIs.Count) % currentWeaponUIs.Count;
            this.UpdateWeaponSelectionVisual();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % currentWeaponUIs.Count;
            this.UpdateWeaponSelectionVisual();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (currentWeaponIndex >= 0 && currentWeaponIndex < currentWeaponTypeStrings.Count)
            {
                ShowEquipmentOfType(currentWeaponTypeStrings[currentWeaponIndex]);
            }
        }
    }
    private void HandleItemPanelInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            this.LoadHeroStat(heroesInCombat[currentSwapIndex]);
            this.ClearEquipmentOfType();
            return;
        }

        if (itemPanelUIs.Count == 0)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                this.LoadHeroStat(heroesInCombat[currentSwapIndex]);
                this.ClearEquipmentOfType();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentItemPanelIndex = (currentItemPanelIndex - 1 + itemPanelUIs.Count) % itemPanelUIs.Count;
            UpdateItemPanelSelectionVisual();
            this.UpdateItemDetail(currentItemPanelItems[currentItemPanelIndex]);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentItemPanelIndex = (currentItemPanelIndex + 1) % itemPanelUIs.Count;
            UpdateItemPanelSelectionVisual();
            this.UpdateItemDetail(currentItemPanelItems[currentItemPanelIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            var itemUI = itemPanelUIs[currentItemPanelIndex].GetComponent<EquipableItemUI>();
            if (itemUI != null)
            {
                this.ApplyEquip(currentItemPanelItems[currentItemPanelIndex]);
            }
        }
    }

    private void ApplyEquip(EquippableItemSO newItem)
    {
        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        EquippableItemSO oldItem = currentWeapon[currentWeaponIndex];

        if (oldItem == newItem && newItem != null)
        {
            this.LoadCurrentHeroUI();
            this.ClearEquipmentOfType();
            return;
        }

        int slotIndex = currentWeaponSlotIndexes[currentWeaponIndex];

        // BẢO VỆ TUYỆT ĐỐI: Nếu mảng đồ của Hero bị đầy, ép nới rộng mảng để nhét đồ mới vào
        if (slotIndex == -1)
        {
            for (int i = 0; i < currentHero.agentWeapon.weaponItemSO.Length; i++)
            {
                if (currentHero.agentWeapon.weaponItemSO[i] == null)
                {
                    slotIndex = i;
                    break;
                }
            }

            // Nếu quét xong vẫn không có ô trống -> Nới rộng mảng
            if (slotIndex == -1)
            {
                List<EquippableItemSO> expandedList = currentHero.agentWeapon.weaponItemSO.ToList();
                expandedList.Add(null);
                currentHero.agentWeapon.weaponItemSO = expandedList.ToArray();
                slotIndex = expandedList.Count - 1;
            }
        }

        if (slotIndex != -1)
        {
            currentHero.agentWeapon.SetWeapon(slotIndex, newItem);

            var field = typeof(ItemInventoryController).GetField("inventoryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                InventorySO invData = field.GetValue(ItemInventoryController.Instance) as InventorySO;
                if (invData != null) invData.RemoveItem(newItem, 1);
            }

            this.LoadCurrentHeroUI();
            this.ClearEquipmentOfType();
        }
    }

    private void ClearHero()
    {
        if (heroImage != null) this.heroImage.sprite = null;
        if (heroName != null) this.heroName.text = "";
        if (heroType != null) this.heroType.text = "";
    }
    private void ClearHeroStat()
    {
        if (hpValue != null) this.hpValue.text = "";
        if (mpValue != null) this.mpValue.text = "";
        if (atkValue != null) this.atkValue.text = "";
        if (defValue != null) this.defValue.text = "";
        if (mAtkValue != null) this.mAtkValue.text = "";
        if (mDefValue != null) this.mDefValue.text = "";
    }

    public void LoadEquipmentUI(HeroStateMachine hero, string typeStr, GameObject uiPrefab, Transform spacer, List<GameObject> uiList)
    {
        // 1. Dọn dẹp Spacer nhưng bảo vệ bản gốc
        foreach (Transform child in spacer)
        {
            if (child.gameObject == uiPrefab) child.gameObject.SetActive(false);
            else Destroy(child.gameObject);
        }

        // 2. Xác định cấu trúc Slot
        bool isRing = typeStr.ToLower().Contains("ring");
        int slotsToDraw = isRing ? 3 : 1;

        // 3. Phân loại đồ đang mặc để chuẩn bị gán vào vị trí cố định
        var equippedList = hero.agentWeapon.weaponItemSO;
        var silverRingsMatched = new List<(EquippableItemSO item, int idx)>();
        var goldRingsMatched = new List<(EquippableItemSO item, int idx)>();
        var otherMatched = new List<(EquippableItemSO item, int idx)>();

        for (int i = 0; i < equippedList.Length; i++)
        {
            var item = equippedList[i];
            if (item == null) continue;

            string itemTypeStr = item.itemType.ToString().ToLower();
            string targetTypeStr = typeStr.ToLower();
            bool isMatch = itemTypeStr == targetTypeStr || ( itemTypeStr == "Armor");

            if (isMatch)
            {
                if (isRing)
                {
                    // Tách riêng Bạc (Normal) và Vàng (Rare/Epic)
                    if (item.rarity == Rarity.Normal) silverRingsMatched.Add((item, i));
                    else goldRingsMatched.Add((item, i));
                }
                else
                {
                    otherMatched.Add((item, i));
                }
            }
        }

        // 4. TIẾN HÀNH VẼ ĐỦ SỐ SLOT THEO ĐÚNG VỊ TRÍ CỐ ĐỊNH
        for (int i = 0; i < slotsToDraw; i++)
        {
            GameObject newUI = Instantiate(uiPrefab, spacer);
            newUI.SetActive(true);
            uiList.Add(newUI);

            EquipableItemUI itemUIComp = newUI.GetComponent<EquipableItemUI>();

            EquippableItemSO slotItem = null;
            int originalIdx = -1;
            string slotDisplayName = "------";
            Rarity? reqRarity = null; // Ghi nhớ slot này yêu cầu Rarity gì

            // LOGIC KHÓA CỨNG VỊ TRÍ
            if (isRing)
            {
                if (i == 0) // Vị trí 1: Bắt buộc là Nhẫn Bạc 1
                {
                    reqRarity = Rarity.Normal;
                    if (silverRingsMatched.Count > 0) { slotItem = silverRingsMatched[0].item; originalIdx = silverRingsMatched[0].idx; }
                    slotDisplayName = (slotItem != null) ? $"{slotItem.Name}" : "------";
                }
                else if (i == 1) // Vị trí 2: Bắt buộc là Nhẫn Bạc 2
                {
                    reqRarity = Rarity.Normal;
                    if (silverRingsMatched.Count > 1) { slotItem = silverRingsMatched[1].item; originalIdx = silverRingsMatched[1].idx; }
                    slotDisplayName = (slotItem != null) ? $"{slotItem.Name}" : "------";
                }
                else if (i == 2) // Vị trí 3: Bắt buộc là Nhẫn Vàng (Rare/Epic)
                {
                    reqRarity = Rarity.Epic; // Dùng Epic đại diện cho Vàng
                    if (goldRingsMatched.Count > 0) { slotItem = goldRingsMatched[0].item; originalIdx = goldRingsMatched[0].idx; }
                    slotDisplayName = (slotItem != null) ? $"{slotItem.Name}" : "------";
                }
            }
            else // Vũ khí hoặc Khiên
            {
                if (otherMatched.Count > 0) { slotItem = otherMatched[0].item; originalIdx = otherMatched[0].idx; }
                slotDisplayName = (slotItem != null) ? slotItem.Name : "------";
            }

            // ==========================================
            // FIX: DỌN DẸP SẠCH SẼ MỌI TEXT RÁC Ở Ô TRỐNG
            // ==========================================
            TextMeshProUGUI[] texts = newUI.GetComponentsInChildren<TextMeshProUGUI>(true);

            if (slotItem != null)
            {
                if (itemUIComp != null)
                {
                    itemUIComp.enabled = true; // Bật script lên lại nếu lỡ copy từ ô trống
                    itemUIComp.Setup(slotItem, this);
                }
                // Tìm đúng cái Text chứa Tên để đè chữ lên
                foreach (var t in texts)
                {
                    if (t.gameObject.name.ToLower().Contains("name") || t.gameObject.name.ToLower().Contains("txt") || texts.Length == 1)
                    {
                        t.text = slotDisplayName;
                        t.color = Color.white;
                        break;
                    }
                }
            }
            else
            {
                // XỬ LÝ TRẮNG TRƠN CHO Ô TRỐNG (Weapon, Armor, Ring đều dùng chung)
                if (itemUIComp != null) itemUIComp.enabled = false;

                foreach (var t in texts)
                {
                    string tName = t.gameObject.name.ToLower();
                    if (tName.Contains("name") || tName.Contains("txt") || texts.Length == 1)
                    {
                        t.text = slotDisplayName;
                        t.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                    }
                    else
                    {
                        t.text = ""; // Xóa trắng các dòng Level, Quantity, v.v.
                    }
                }

                // Làm tàng hình toàn bộ Icon của Prefab gốc
                Image[] allImages = newUI.GetComponentsInChildren<Image>(true);
                foreach (var img in allImages)
                {
                    if (img.gameObject == newUI) continue; // Chừa cái nền Background lại
                    string n = img.gameObject.name.ToLower();
                    if (!n.Contains("bg") && !n.Contains("select") && !n.Contains("choose") && !n.Contains("background"))
                    {
                        img.color = new Color(1, 1, 1, 0); // Trong suốt
                    }
                }
            }

            currentWeapon.Add(slotItem);
            currentWeaponSlotIndexes.Add(originalIdx);
            currentWeaponTypeStrings.Add(typeStr);
            currentWeaponSlotRarities.Add(reqRarity);
        }
    }

    public void LoadHero(HeroStateMachine hero)
    {
        this.ClearHero();
        this.heroImage.sprite = hero.baseHero.heroImage;
        this.heroName.text = hero.baseHero.theName;
        this.heroType.text = hero.baseHero.heroType.ToString() + "/" + hero.baseHero.elemental.ToString();
    }

    public void LoadHeroStat(HeroStateMachine hero)
    {
        this.ClearHeroStat();
        this.hpValue.text = $"{hero.baseHero.curHP}/{hero.baseHero.baseHP}";
        this.mpValue.text = $"{hero.baseHero.curMP}/{hero.baseHero.baseMP}";
        this.atkValue.text = hero.baseHero.curATK.ToString();
        this.defValue.text = hero.baseHero.curDEF.ToString();
        this.mAtkValue.text = hero.baseHero.curMATK.ToString();
        this.mDefValue.text = hero.baseHero.curMDEF.ToString();
    }

    public void CreateHeroSwapButton(CombatStateMachine cbm, HeroStateMachine selectedHero)
    {
        foreach (Transform child in this.heroSwapButtonSpacer.transform)
        {
            if (child.gameObject == heroSwapButtonPrefab.gameObject) child.gameObject.SetActive(false);
            else Destroy(child.gameObject);
        }

        heroSwapButtons.Clear();
        heroesInCombat.Clear();

        foreach (var heroGO in cbm.playersInCombat)
        {
            HeroStateMachine hero = heroGO.GetComponent<HeroStateMachine>();
            heroesInCombat.Add(hero);

            Button newSwapBut = Instantiate(this.heroSwapButtonPrefab, this.heroSwapButtonSpacer.transform);
            newSwapBut.gameObject.SetActive(true);
            heroSwapButtons.Add(newSwapBut);

            Transform iconTransform = newSwapBut.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image heroIcon = iconTransform.GetComponent<Image>();
                heroIcon.sprite = hero.baseHero.heroImage;
            }

            HeroStateMachine capturedHero = hero;
            newSwapBut.onClick.AddListener(() =>
            {
                currentSwapIndex = heroesInCombat.IndexOf(capturedHero);
                LoadCurrentHeroUI();
            });
        }

        currentSwapIndex = heroesInCombat.IndexOf(selectedHero);
        if (currentSwapIndex < 0) currentSwapIndex = 0;
        LoadCurrentHeroUI();
    }

    private void LoadCurrentHeroUI()
    {
        this.currentWeaponUIs.Clear();
        this.currentWeapon.Clear();
        this.currentWeaponTypeStrings.Clear();
        this.currentWeaponSlotIndexes.Clear();
        this.currentWeaponSlotRarities.Clear();

        if (currentSwapIndex < 0 || currentSwapIndex >= heroesInCombat.Count) return;

        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        LoadHero(currentHero);
        LoadHeroStat(currentHero);

        LoadEquipmentUI(currentHero, "Weapon", this.weaponUI, this.weaponUISpacer, this.currentWeaponUIs);
        LoadEquipmentUI(currentHero, "Armor", this.weaponUI, this.shieldUISpacer, this.currentWeaponUIs);
        LoadEquipmentUI(currentHero, "Ring", this.weaponUI, this.ringUISpacer, this.currentWeaponUIs);

        this.currentWeaponIndex = 0;
        this.UpdateWeaponSelectionVisual();

        for (int i = 0; i < heroSwapButtons.Count; i++)
        {
            Button btn = heroSwapButtons[i];
            Transform iconTransform = btn.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null) iconImage.color = (i == currentSwapIndex) ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }

    private void UpdateWeaponSelectionVisual()
    {
        for (int i = 0; i < currentWeaponUIs.Count; i++)
        {
            GameObject weaponUIObj = currentWeaponUIs[i];
            Image bg = weaponUIObj.GetComponent<Image>();
            if (bg != null) bg.color = (i == currentWeaponIndex) ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0f);
        }

        if (currentWeaponIndex >= 0 && currentWeaponIndex < currentWeapon.Count)
        {
            UpdateWeapontDescription(currentWeapon[currentWeaponIndex]);
        }
    }

    private void UpdateWeapontDescription(EquippableItemSO curWeapon)
    {
        if (this.desText == null) return;
        this.desText.text = curWeapon != null ? curWeapon.Description : "Empty. <color=#58C7E2>[Enter]</color> to equip new item.";
    }

    private void UpdateItemDetail(EquippableItemSO curWeapon)
    {
        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        if (curWeapon == null)
        {
            this.LoadHeroStat(currentHero);
            return;
        }

        string typeName = curWeapon.itemType.ToString();
        if (typeName == "Weapon") this.SetWeaponItemDetail(currentHero, curWeapon);
        else if (typeName == "Armor") this.SetShieldItemDetail(currentHero, curWeapon);
        else if (typeName == "Ring") this.SetRingItemDetail(currentHero, curWeapon);
    }

    private void SetWeaponItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        EquippableItemSO oldItem = currentWeapon[currentWeaponIndex];

        float oldBonusATK = 0f; float oldBonusMATK = 0f;
        if (oldItem != null && oldItem.Modifiers != null)
        {
            foreach (var mod in oldItem.Modifiers)
                if (mod.stat != null) { oldBonusATK += mod.val1; oldBonusMATK += mod.val2; }
        }

        float newBonusATK = 0f; float newBonusMATK = 0f;
        if (curWeapon != null && curWeapon.Modifiers != null)
        {
            foreach (var mod in curWeapon.Modifiers)
                if (mod.stat != null) { newBonusATK += mod.val1; newBonusMATK += mod.val2; }
        }

        float deltaATK = newBonusATK - oldBonusATK;
        float deltaMATK = newBonusMATK - oldBonusMATK;

        atkValue.text = FormatStatWithBonus(hero.baseHero.baseATK, deltaATK);
        mAtkValue.text = FormatStatWithBonus(hero.baseHero.baseMATK, deltaMATK);
    }

    private void SetShieldItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        EquippableItemSO oldItem = currentWeapon[currentWeaponIndex];

        float oldBonusDEF = 0f; float oldBonusMDEF = 0f;
        if (oldItem != null && oldItem.Modifiers != null)
        {
            foreach (var mod in oldItem.Modifiers)
                if (mod.stat != null) { oldBonusDEF += mod.val1; oldBonusMDEF += mod.val2; }
        }

        float newBonusDEF = 0f; float newBonusMDEF = 0f;
        if (curWeapon != null && curWeapon.Modifiers != null)
        {
            foreach (var mod in curWeapon.Modifiers)
                if (mod.stat != null) { newBonusDEF += mod.val1; newBonusMDEF += mod.val2; }
        }

        float deltaDEF = newBonusDEF - oldBonusDEF;
        float deltaMDEF = newBonusMDEF - oldBonusMDEF;

        defValue.text = FormatStatWithBonus(hero.baseHero.baseDEF, deltaDEF);
        mDefValue.text = FormatStatWithBonus(hero.baseHero.baseMDEF, deltaMDEF);
    }

    private void SetRingItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        EquippableItemSO oldItem = currentWeapon[currentWeaponIndex];

        float oldBonusATK = 0f; float oldBonusMATK = 0f;
        if (oldItem != null && oldItem.Modifiers != null)
        {
            foreach (var mod in oldItem.Modifiers)
                if (mod.stat != null) { oldBonusATK += mod.val1; oldBonusMATK += mod.val2; }
        }

        float newBonusATK = 0f; float newBonusMATK = 0f;
        if (curWeapon != null && curWeapon.Modifiers != null)
        {
            foreach (var mod in curWeapon.Modifiers)
                if (mod.stat != null) { newBonusATK += mod.val1; newBonusMATK += mod.val2; }
        }

        float deltaATK = newBonusATK - oldBonusATK;
        float deltaMATK = newBonusMATK - oldBonusMATK;

        atkValue.text = FormatStatWithBonus(hero.baseHero.baseATK, deltaATK);
        mAtkValue.text = FormatStatWithBonus(hero.baseHero.baseMATK, deltaMATK);
    }

    private void UpdateItemPanelSelectionVisual()
    {
        for (int i = 0; i < itemPanelUIs.Count; i++)
        {
            Image bg = itemPanelUIs[i].GetComponentInParent<Image>();
            if (bg != null) bg.color = (i == currentItemPanelIndex) ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0f);
        }

        if (currentItemPanelIndex >= 0 && currentItemPanelIndex < currentItemPanelItems.Count)
        {
            UpdateWeapontDescription(currentItemPanelItems[currentItemPanelIndex]);
        }
    }

    public void ShowEquipmentOfType(string typeStr)
    {
        // FIX: Cố gắng chuyển đổi chuỗi sang Enum an toàn. Hỗ trợ trường hợp game bạn dùng "Armor" thay cho "Shield"
        ItemType type = default;
        bool foundType = System.Enum.TryParse(typeStr, true, out type);

        if (!foundType && typeStr.ToLower() == "Armor") foundType = System.Enum.TryParse("Armor", true, out type);

        this.itemPanel.SetActive(true);
        this.isItemPanel = true;

        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        Transform spacerContent = itemPanel.transform.Find("Scroll View").Find("Viewport").Find("Content");

        foreach (Transform child in spacerContent)
        {
            if (child.gameObject == weaponUI) child.gameObject.SetActive(false);
            else Destroy(child.gameObject);
        }

        itemPanelUIs.Clear();
        currentItemPanelItems.Clear();

        if (spacerContent != null)
        {
            var equipItems = ItemInventoryController.Instance.GetEquipableItemsByType(type);
            Rarity? reqRarity = currentWeaponSlotRarities[currentWeaponIndex];

            foreach (var item in equipItems)
            {
                // Cho phép hiển thị nếu đúng loại đồ (Hoặc hệ thống coi Khiên và Áo giáp là một)
                bool isMatch = item.itemType == type;
                if (!isMatch && type.ToString().ToLower() == "Armor") isMatch = true;

                if (isMatch && (item.allowedWeapons == currentHero.baseHero.heroType || item.allowedWeapons == HeroType.All))
                {
                    // FIX: Nếu là ô Nhẫn, ép hiển thị đúng loại Nhẫn (Vàng/Bạc) theo Slot
                    if (typeStr.ToLower().Contains("ring") && reqRarity != null)
                    {
                        if (reqRarity == Rarity.Normal && item.rarity != Rarity.Normal) continue;
                        if (reqRarity != Rarity.Normal && item.rarity == Rarity.Normal) continue;
                    }

                    GameObject newUI = Instantiate(weaponUI, spacerContent);
                    newUI.SetActive(true);

                    EquipableItemUI itemUI = newUI.GetComponent<EquipableItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.enabled = true; // Bật lại Component lỡ prefab bị tắt
                        itemUI.Setup(item, this);
                        itemPanelUIs.Add(newUI);
                        currentItemPanelItems.Add(item);
                    }
                }
            }

            currentItemPanelIndex = 0;
            if (currentItemPanelItems.Count > 0)
                this.UpdateItemDetail(currentItemPanelItems[currentItemPanelIndex]);
            else
                this.UpdateWeapontDescription(null);

            UpdateItemPanelSelectionVisual();
        }
    }

    public void ClearEquipmentOfType()
    {
        Transform spacerContent = itemPanel.transform.Find("Scroll View").Find("Viewport").Find("Content");
        if (spacerContent != null)
        {
            foreach (Transform child in spacerContent)
            {
                if (child.gameObject == weaponUI) child.gameObject.SetActive(false);
                else Destroy(child.gameObject);
            }
        }

        this.isItemPanel = false;
        this.itemPanel.SetActive(false);
    }

    private string FormatStatWithBonus(float baseValue, float bonus)
    {
        if (bonus == 0) return baseValue.ToString();
        string sign = bonus > 0 ? "+" : "-";
        string bonusColor = bonus > 0 ? "#58C7E2" : "#FF4C4C";
        return $"{baseValue} <color={bonusColor}>{sign}{Mathf.Abs(bonus)}</color>";
    }
}