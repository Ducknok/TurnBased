using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
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
    private List<EquippableItemSO> currentWeapon = new List<EquippableItemSO>();

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

        this.SelectHero();
        this.SelectWeapon();
    }
    public void SelectHero()
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
    private void SelectWeapon()
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
    public void LoadEquipmentUI(HeroStateMachine hero, string type, GameObject uiPrefab, Transform spacer, List<GameObject> uiList)
    {
        foreach (Transform child in spacer)
        {
            Destroy(child.gameObject);
        }

        

        AgentWeapon agentWeapon = hero.GetComponent<AgentWeapon>();
        foreach (var item in agentWeapon.weaponItemSO)
        {
            if (item.itemType.ToString() == type)
            {
                GameObject newUI = Instantiate(uiPrefab, spacer);
                Image itemImage = newUI.transform.Find("Button").Find("WeaponImage").GetComponent<Image>();
                TextMeshProUGUI itemName = newUI.transform.Find("WeaponName").GetComponent<TextMeshProUGUI>();

                itemImage.sprite = item.ItemImage;
                itemName.text = item.Name;

                uiList.Add(newUI);
                this.currentWeapon.Add(item);
            }
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
            UpdateWeapontDetail(currentWeapon[currentWeaponIndex]); // 💥 Gọi mô tả
        }
    }
    private void UpdateWeapontDetail(EquippableItemSO curWeapon)
    {
        if (this.desText == null) return;
        this.desText.text = curWeapon.Description;
    }
}
