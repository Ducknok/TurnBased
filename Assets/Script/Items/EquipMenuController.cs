using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipMenuController : MonoBehaviour
{
    [Header("Hero")]
    [SerializeField] private Image heroImage;
    [SerializeField] private TextMeshProUGUI hereName;
    [Header("Weapon UI")]
    [SerializeField] private GameObject weaponUI;
    [SerializeField] private Button weaponBut;
    [SerializeField] private Image weaponImage;
    [SerializeField] private TextMeshProUGUI weaponDes;

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
    //TODO: Shield UI here
    //TODO: Ring UI here

    private void Update()
    {
        if (heroSwapButtons.Count == 0) return;

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
    private void ClearWeaponUI()
    {
        if(weaponImage != null) this.weaponImage.sprite = null;
        if(weaponDes != null) this.weaponDes.text = "";
    }
    private void ClearHero()
    {
        if (heroImage != null) this.heroImage.sprite = null;
        if (hereName != null) this.hereName.text = "";
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
    public void LoadWeaponUI(HeroStateMachine hero)
    {
        this.ClearWeaponUI();
        AgentWeapon agentWeapon = hero.transform.GetComponent<AgentWeapon>();
        this.weaponImage.sprite = agentWeapon.weaponItemSO.ItemImage;
        this.weaponDes.text = agentWeapon.weaponItemSO.Description;
    }
    public void LoadHero(HeroStateMachine hero)
    {
        this.ClearHero();
        this.heroImage.sprite = hero.baseHero.heroImage;
        this.hereName.text = hero.baseHero.theName;
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
    public void CreateHeroSwapButton(CombatStateMachine cbm)
    {
        // Clear cũ
        foreach (Transform child in this.heroSwapButtonSpacer.transform)
        {
            Destroy(child.gameObject);
        }

        heroSwapButtons.Clear();
        heroesInCombat.Clear();

        // Tạo mới
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

            // Optional: Gán onClick nếu muốn chọn bằng chuột
            HeroStateMachine capturedHero = hero;
            newSwapBut.onClick.AddListener(() =>
            {
                currentSwapIndex = heroesInCombat.IndexOf(capturedHero);
                LoadCurrentHeroUI();
            });
        }

        // Load UI hero đầu tiên
        currentSwapIndex = 0;
        LoadCurrentHeroUI();
    }

    private void LoadCurrentHeroUI()
    {
        if (currentSwapIndex < 0 || currentSwapIndex >= heroesInCombat.Count) return;

        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        LoadHero(currentHero);
        LoadHeroStat(currentHero);
        LoadWeaponUI(currentHero);
    }
}
