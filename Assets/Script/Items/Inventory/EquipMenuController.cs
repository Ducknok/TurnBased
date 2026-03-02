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

    [Header("Armor")]
    [SerializeField] private Transform armorUISpacer;
    [Header("Ring")]
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
        if (!this.isEquipMenuOpen || heroSwapButtons.Count == 0 || currentWeaponUIs.Count == 0)
        {
            Debug.LogWarning("Equip inventory dang dong");
            return;
        }
        if (isItemPanel)
        {
            HandleItemPanelInput();
            return; // Không xử lý hero hay weapon khi đang mở itemPanel
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
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.currentWeaponIndex = (currentWeaponIndex - 1 + currentWeaponUIs.Count) % currentWeaponUIs.Count;
            //Debug.LogWarning("Nut len");
            this.UpdateWeaponSelectionVisual();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % currentWeaponUIs.Count;
            //Debug.LogWarning("Nut xuong");
            this.UpdateWeaponSelectionVisual();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Giả lập click vào button hiện tại
            if (currentWeaponIndex >= 0 && currentWeaponIndex < currentWeaponUIs.Count)
            {
                if (currentWeaponIndex >= 0 && currentWeaponIndex < currentWeaponUIs.Count)
                {
                    ShowEquipmentOfType(currentWeapon[currentWeaponIndex].itemType);
                }
            }
        }
    }
    private void HandleItemPanelInput()
    {
        if (itemPanelUIs.Count == 0) return;

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
                //itemUI.OnClick(); // Hoặc viết hàm ApplyEquip(item) nếu không dùng click
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            this.LoadHeroStat(heroesInCombat[currentSwapIndex]);
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
        // Xóa UI cũ trong Spacer
        foreach (Transform child in spacer)
        {
            Destroy(child.gameObject);
        }

        // Chuyển string sang Enum ItemType
        if (!System.Enum.TryParse(typeStr, out ItemType type))
        {
            Debug.LogError("Sai kiểu item type: " + typeStr);
            return;
        }

        // Lấy danh sách item đang được trang bị
        var equipItems = hero.agentWeapon.weaponItemSO;

        // Khởi tạo bộ đếm số lượng Ring theo độ hiếm
        Dictionary<Rarity, int> ringRarityCount = new Dictionary<Rarity, int>
        {
            { Rarity.Normal, 0 },
            { Rarity.Rare, 0 },
            { Rarity.Epic, 0 }
        };

        // Duyệt qua các item đã trang bị
        foreach (var item in equipItems)
        {
            if (item == null)
                continue;

            // Loại item không khớp
            if (item.itemType != type)
                continue;

            // Không tương thích với hero
            if (item.allowedWeapons != hero.baseHero.heroType && item.allowedWeapons != HeroType.All)
                continue;

            // Xử lý riêng với Ring: giới hạn số lượng theo độ hiếm
            if (type == ItemType.Ring)
            {
                Rarity rarity = item.rarity;
                int maxAllowed = (rarity == Rarity.Normal) ? 2 : 1;

                if (ringRarityCount[rarity] >= maxAllowed)
                    continue;

                ringRarityCount[rarity]++;
            }
            else
            {
                // Weapon/Armor không được trùng tên
                if (currentWeapon.Any(i => i.Name == item.Name))
                    continue;
            }

            // Tạo UI cho item
            GameObject newUI = Instantiate(uiPrefab, spacer);
            EquipableItemUI itemUI = newUI.GetComponent<EquipableItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(item, this);
            }

            uiList.Add(newUI);     // Thêm vào danh sách UI
            currentWeapon.Add(item);    // Thêm vào danh sách tạm
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
            Destroy(child.gameObject);
        }

        heroSwapButtons.Clear();
        heroesInCombat.Clear();

        foreach (var heroGO in cbm.playersInCombat)
        {
            HeroStateMachine hero = heroGO.GetComponent<HeroStateMachine>();
            heroesInCombat.Add(hero);

            Button newSwapBut = Instantiate(this.heroSwapButtonPrefab, this.heroSwapButtonSpacer.transform);
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

        // 🔥 Xác định hero đang được chọn khi vào Equip
        currentSwapIndex = heroesInCombat.IndexOf(selectedHero);
        if (currentSwapIndex < 0) currentSwapIndex = 0; // fallback nếu không tìm thấy
        LoadCurrentHeroUI();
    }
    private void LoadCurrentHeroUI()
    {
        this.currentWeaponUIs.Clear();
        this.currentWeapon.Clear();
        if (currentSwapIndex < 0 || currentSwapIndex >= heroesInCombat.Count) return;

        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        LoadHero(currentHero);
        LoadHeroStat(currentHero);
        LoadEquipmentUI(currentHero, "Weapon", this.weaponUI, this.weaponUISpacer, this.currentWeaponUIs);
        LoadEquipmentUI(currentHero, "Armor", this.weaponUI, this.armorUISpacer, this.currentWeaponUIs);
        LoadEquipmentUI(currentHero, "Ring", this.weaponUI, this.ringUISpacer, this.currentWeaponUIs);
        this.currentWeaponIndex = 0;
        this.UpdateWeaponSelectionVisual();
        // Cập nhật màu icon của nút chọn hero
        for (int i = 0; i < heroSwapButtons.Count; i++)
        {
            Button btn = heroSwapButtons[i];
            Transform iconTransform = btn.transform.Find("Icon");

            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    // Nếu là hero đang được chọn thì sáng, ngược lại thì tối
                    if (i == currentSwapIndex)
                    {
                        iconImage.color = Color.white; // hoặc new Color(1f, 1f, 1f, 1f)
                    }
                    else
                    {
                        iconImage.color = new Color(1f, 1f, 1f, 0.3f); // alpha thấp để tối đi
                    }
                }
            }

        }
    }
    private void UpdateWeaponSelectionVisual()
    {
        for (int i = 0; i < currentWeaponUIs.Count; i++)
        {
            GameObject weaponUI = currentWeaponUIs[i];
            //Debug.LogError(weaponUI);
            Image bg = weaponUI.GetComponent<Image>();
            if (bg != null)
            {
                // Màu xám nhạt cho skill được chọn, và trong suốt nhẹ cho skill không chọn
                bg.color = (i == currentWeaponIndex) ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0f);
            }
        }
        if (currentWeaponIndex >= 0 && currentWeaponIndex < currentWeapon.Count)
        {
            UpdateWeapontDescription(currentWeapon[currentWeaponIndex]); // 💥 Gọi mô tả
        }
    }
    private void UpdateWeapontDescription(EquippableItemSO curWeapon)
    {
        if (this.desText == null) return;
        this.desText.text = curWeapon.Description;
    }
    private void UpdateItemDetail(EquippableItemSO curWeapon)
    {
        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        switch (curWeapon.itemType) 
        {
            case ItemType.Weapon:
                this.SetWeaponItemDetail(currentHero, curWeapon);
                break;
            case ItemType.Armor:
                this.SetArmorItemDetail(currentHero, curWeapon);
                break;
            case ItemType.Ring:
                this.SetRingItemDetail(currentHero, curWeapon);
                break;
        }
    }
    private void SetWeaponItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        float bonusATK = 0f;
        float bonusMATK = 0f;

        foreach (var mod in curWeapon.Modifiers)
        {
            if (mod.stat != null)
            {
                bonusATK += mod.val1;
                bonusMATK += mod.val2;
            }
        }

        atkValue.text = FormatStatWithBonus(hero.baseHero.baseATK, bonusATK);
        mAtkValue.text = FormatStatWithBonus(hero.baseHero.baseMATK, bonusMATK);
    }
    private void SetArmorItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        float bonusDEF = 0f;
        float bonusMDEF = 0f;

        foreach (var mod in curWeapon.Modifiers)
        {
            if (mod.stat != null)
            {
                bonusDEF += mod.val1;
                bonusMDEF += mod.val2;
            }
        }

        defValue.text = FormatStatWithBonus(hero.baseHero.baseDEF, bonusDEF);
        mDefValue.text = FormatStatWithBonus(hero.baseHero.baseMDEF, bonusMDEF);
    }
    private void SetRingItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        float bonusATK = 0f;
        float bonusMATK = 0f;

        foreach (var mod in curWeapon.Modifiers)
        {
            if (mod.stat != null)
            {
                bonusATK += mod.val1;
                bonusMATK += mod.val2;
            }
        }

        atkValue.text = FormatStatWithBonus(hero.baseHero.baseATK, bonusATK);
        mAtkValue.text = FormatStatWithBonus(hero.baseHero.baseMATK, bonusMATK);
    }
    private void UpdateItemPanelSelectionVisual()
    {
        for (int i = 0; i < itemPanelUIs.Count; i++)
        {
            Image bg = itemPanelUIs[i].GetComponentInParent<Image>();
            if (bg != null)
            {
                bg.color = (i == currentItemPanelIndex) ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0f);
            }
        }

        if (currentItemPanelIndex >= 0 && currentItemPanelIndex < currentItemPanelItems.Count)
        {
            UpdateWeapontDescription(currentItemPanelItems[currentItemPanelIndex]);
        }
    }
    public void ShowEquipmentOfType(ItemType type)
    {
        this.itemPanel.SetActive(true);
        this.isItemPanel = true;

        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];

        Transform spacer = itemPanel.transform.Find("Scroll View").Find("Viewport").Find("Content");

        // Xóa UI cũ
        foreach (Transform child in spacer)
        {
            Destroy(child.gameObject);
        }
        itemPanelUIs.Clear();
        currentItemPanelItems.Clear();

        if (spacer != null)
        {
            var equipItems = ItemInventoryController.Instance.GetEquipableItemsByType(type);
            foreach (var item in equipItems)
            {
                if (item.itemType == type && (item.allowedWeapons == currentHero.baseHero.heroType || item.allowedWeapons == HeroType.All))
                {
                    GameObject newUI = Instantiate(weaponUI, spacer);
                    EquipableItemUI itemUI = newUI.GetComponent<EquipableItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.Setup(item, this);
                        itemPanelUIs.Add(newUI);
                        currentItemPanelItems.Add(item);
                    }
                }
            }

            currentItemPanelIndex = 0;
            this.UpdateItemDetail(currentItemPanelItems[currentItemPanelIndex]);
            UpdateItemPanelSelectionVisual();
        }
    }
    public void ClearEquipmentOfType()
    {
        Transform spacer = itemPanel.transform.Find("Scroll View").Find("Viewport").Find("Content");
        foreach (Transform child in spacer)
        {
            GameObject.Destroy(child.gameObject);
        }
        this.isItemPanel = false;
        this.itemPanel.SetActive(false);
    }
    private string FormatStatWithBonus(float baseValue, float bonus)
    {
        if (bonus == 0)
            return baseValue.ToString();

        string sign = bonus > 0 ? "+" : "-";
        string bonusColor = bonus > 0 ? "#58C7E2" : "#FF4C4C"; // xanh da trời nhạt và đỏ nhạt
        float absBonus = Mathf.Abs(bonus);

        return $"<color={bonusColor}>{baseValue}</color> <color={bonusColor}>{sign}{absBonus}</color>";
    }

}
