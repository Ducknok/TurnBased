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
    private List<int> currentWeaponSlotIndexes = new List<int>();

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
        if (!this.isEquipMenuOpen || heroSwapButtons.Count == 0 || currentWeaponUIs.Count == 0)
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
            if (currentWeaponIndex >= 0 && currentWeaponIndex < currentWeaponUIs.Count)
            {
                ShowEquipmentOfType(currentWeapon[currentWeaponIndex].itemType);
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
                this.ApplyEquip(currentItemPanelItems[currentItemPanelIndex]);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            this.LoadHeroStat(heroesInCombat[currentSwapIndex]);
            this.ClearEquipmentOfType();
        }
    }

    private void ApplyEquip(EquippableItemSO newItem)
    {
        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        EquippableItemSO oldItem = currentWeapon[currentWeaponIndex];

        if (oldItem == newItem)
        {
            Debug.Log("Hero đã trang bị món này rồi!");
            this.ClearEquipmentOfType();
            return;
        }

        int slotIndex = currentWeaponSlotIndexes[currentWeaponIndex];

        if (slotIndex != -1)
        {
            if (oldItem != null)
            {
                foreach (var mod in oldItem.Modifiers)
                    mod.stat.AffectCharacter(currentHero.gameObject, -mod.val1, -mod.val2);
            }

            if (newItem != null)
            {
                foreach (var mod in newItem.Modifiers)
                    mod.stat.AffectCharacter(currentHero.gameObject, mod.val1, mod.val2);
            }

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
        foreach (Transform child in spacer) Destroy(child.gameObject);

        if (!System.Enum.TryParse(typeStr, out ItemType type)) return;

        var equipItems = hero.agentWeapon.weaponItemSO;

        Dictionary<Rarity, int> ringRarityCount = new Dictionary<Rarity, int>
        {
            { Rarity.Normal, 0 }, { Rarity.Rare, 0 }, { Rarity.Epic, 0 }
        };

        int ringIndexCounter = 1;

        for (int i = 0; i < equipItems.Length; i++)
        {
            var item = equipItems[i];
            if (item == null || item.itemType != type) continue;

            if (item.allowedWeapons != hero.baseHero.heroType && item.allowedWeapons != HeroType.All) continue;

            if (type == ItemType.Ring)
            {
                Rarity rarity = item.rarity;
                int maxAllowed = (rarity == Rarity.Normal) ? 2 : 1;
                if (ringRarityCount[rarity] >= maxAllowed) continue;
                ringRarityCount[rarity]++;
            }
            else
            {
                if (currentWeapon.Any(x => x.Name == item.Name)) continue;
            }

            GameObject newUI = Instantiate(uiPrefab, spacer);
            EquipableItemUI itemUI = newUI.GetComponent<EquipableItemUI>();
            if (itemUI != null) itemUI.Setup(item, this);

            if (type == ItemType.Ring)
            {
                TextMeshProUGUI nameText = newUI.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null) nameText.text = $"[Nhẫn {ringIndexCounter}] " + item.Name;
                ringIndexCounter++;
            }

            uiList.Add(newUI);
            currentWeapon.Add(item);
            currentWeaponSlotIndexes.Add(i);
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
        foreach (Transform child in this.heroSwapButtonSpacer.transform) Destroy(child.gameObject);

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

        currentSwapIndex = heroesInCombat.IndexOf(selectedHero);
        if (currentSwapIndex < 0) currentSwapIndex = 0;
        LoadCurrentHeroUI();
    }

    private void LoadCurrentHeroUI()
    {
        this.currentWeaponUIs.Clear();
        this.currentWeapon.Clear();
        this.currentWeaponSlotIndexes.Clear();

        if (currentSwapIndex < 0 || currentSwapIndex >= heroesInCombat.Count) return;

        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        LoadHero(currentHero);
        LoadHeroStat(currentHero);

        // Load Vũ Khí
        LoadEquipmentUI(currentHero, "Weapon", this.weaponUI, this.weaponUISpacer, this.currentWeaponUIs);

        // Hỗ trợ cả 2 tên đề phòng bạn đặt Enum là Armor hoặc Shield
        if (System.Enum.IsDefined(typeof(ItemType), "Shield"))
            LoadEquipmentUI(currentHero, "Shield", this.weaponUI, this.shieldUISpacer, this.currentWeaponUIs);
        else
            LoadEquipmentUI(currentHero, "Armor", this.weaponUI, this.shieldUISpacer, this.currentWeaponUIs);

        // Load Nhẫn
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
            GameObject weaponUI = currentWeaponUIs[i];
            Image bg = weaponUI.GetComponent<Image>();
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
        this.desText.text = curWeapon.Description;
    }

    private void UpdateItemDetail(EquippableItemSO curWeapon)
    {
        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        string typeName = curWeapon.itemType.ToString();

        // Kiểm tra an toàn loại Item để hiển thị đúng chỉ số tương ứng
        if (typeName == "Weapon")
        {
            this.SetWeaponItemDetail(currentHero, curWeapon);
        }
        else if (typeName == "Armor" || typeName == "Shield")
        {
            this.SetShieldItemDetail(currentHero, curWeapon);
        }
        else if (typeName == "Ring")
        {
            this.SetRingItemDetail(currentHero, curWeapon);
        }
    }

    private void SetWeaponItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        float bonusATK = 0f;
        float bonusMATK = 0f;

        foreach (var mod in curWeapon.Modifiers)
        {
            if (mod.stat != null) { bonusATK += mod.val1; bonusMATK += mod.val2; }
        }

        atkValue.text = FormatStatWithBonus(hero.baseHero.baseATK, bonusATK);
        mAtkValue.text = FormatStatWithBonus(hero.baseHero.baseMATK, bonusMATK);
    }

    private void SetShieldItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        float bonusDEF = 0f;
        float bonusMDEF = 0f;

        // Quy ước của Shield: Val1 = Phóng Thủ Vật Lý (DEF), Val2 = Kháng Phép (MDEF)
        foreach (var mod in curWeapon.Modifiers)
        {
            if (mod.stat != null) { bonusDEF += mod.val1; bonusMDEF += mod.val2; }
        }

        // Tô màu và hiển thị cho dòng DEF / MDEF
        defValue.text = FormatStatWithBonus(hero.baseHero.baseDEF, bonusDEF);
        mDefValue.text = FormatStatWithBonus(hero.baseHero.baseMDEF, bonusMDEF);
    }

    private void SetRingItemDetail(HeroStateMachine hero, EquippableItemSO curWeapon)
    {
        this.LoadHeroStat(hero);
        float bonusATK = 0f;
        float bonusMATK = 0f;

        // Quy ước của Nhẫn: Val1 = Sát Thương (ATK), Val2 = Sức mạnh Phép (MATK)
        foreach (var mod in curWeapon.Modifiers)
        {
            if (mod.stat != null) { bonusATK += mod.val1; bonusMATK += mod.val2; }
        }

        atkValue.text = FormatStatWithBonus(hero.baseHero.baseATK, bonusATK);
        mAtkValue.text = FormatStatWithBonus(hero.baseHero.baseMATK, bonusMATK);
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

    public void ShowEquipmentOfType(ItemType type)
    {
        this.itemPanel.SetActive(true);
        this.isItemPanel = true;

        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        Transform spacer = itemPanel.transform.Find("Scroll View").Find("Viewport").Find("Content");

        foreach (Transform child in spacer) Destroy(child.gameObject);
        itemPanelUIs.Clear();
        currentItemPanelItems.Clear();

        if (spacer != null)
        {
            var equipItems = ItemInventoryController.Instance.GetEquipableItemsByType(type);
            EquippableItemSO itemToReplace = currentWeapon[currentWeaponIndex];

            foreach (var item in equipItems)
            {
                // Cho phép mặc nếu đúng HeroType HOẶC đồ đó dành cho mọi người (HeroType.All)
                if (item.itemType == type && (item.allowedWeapons == currentHero.baseHero.heroType || item.allowedWeapons == HeroType.All))
                {
                    if (type == ItemType.Ring && itemToReplace != null)
                    {
                        if (item.rarity != itemToReplace.rarity) continue;
                    }

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
            if (currentItemPanelItems.Count > 0) this.UpdateItemDetail(currentItemPanelItems[currentItemPanelIndex]);
            UpdateItemPanelSelectionVisual();
        }
    }

    public void ClearEquipmentOfType()
    {
        Transform spacer = itemPanel.transform.Find("Scroll View").Find("Viewport").Find("Content");
        foreach (Transform child in spacer) GameObject.Destroy(child.gameObject);
        this.isItemPanel = false;
        this.itemPanel.SetActive(false);
    }

    private string FormatStatWithBonus(float baseValue, float bonus)
    {
        if (bonus == 0) return baseValue.ToString();

        string sign = bonus > 0 ? "+" : "-";
        string bonusColor = bonus > 0 ? "#58C7E2" : "#FF4C4C";
        float absBonus = Mathf.Abs(bonus);

        return $"<color={bonusColor}>{baseValue}</color> <color={bonusColor}>{sign}{absBonus}</color>";
    }
}