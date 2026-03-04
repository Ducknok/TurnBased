using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillMenuController : DucMonobehaviour
{
    public bool isSkillMenuOpen = false;

    [Header("Hero")]
    [SerializeField] private Image heroImage;
    [SerializeField] private TextMeshProUGUI hereName;
    [Header("Skill UI")]
    [SerializeField] private GameObject skillUI;
    [SerializeField] private Transform skillUISpacer;
    private List<GameObject> currentSkillUIs = new List<GameObject>();
    private int currentSkillIndex = 0;

    [Header("Skill Detail UI")]
    private List<BaseAttack> currentSkills = new List<BaseAttack>();
    [SerializeField] private GameObject effectSkillSpacer;
    [SerializeField] private TextMeshProUGUI skillDescText;
    [SerializeField] private TextMeshProUGUI skillCostText;

    [Header("Hero Swap")]
    [SerializeField] private Button heroSwapButtonPrefab;
    [SerializeField] private GameObject heroSwapButtonSpacer;
    private List<Button> heroSwapButtons = new List<Button>();
    private int currentSwapIndex = 0;
    private List<HeroStateMachine> heroesInCombat = new List<HeroStateMachine>();

    protected override void Update()
    {

        if (!this.isSkillMenuOpen) return;
        if (currentSkillUIs.Count == 0) return;

        this.SelectHero();
        this.SelectSkill();
    }

    private void SelectHero()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //Debug.LogWarning("Nut Q");
            currentSwapIndex = (currentSwapIndex - 1 + heroSwapButtons.Count) % heroSwapButtons.Count;
            LoadCurrentHeroUI();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            currentSwapIndex = (currentSwapIndex + 1) % heroSwapButtons.Count;
            //Debug.LogWarning("Nut E");
            LoadCurrentHeroUI();
        }
    }
    private void SelectSkill()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSkillIndex = (currentSkillIndex - 1 + currentSkillUIs.Count) % currentSkillUIs.Count;
            //Debug.LogWarning("Nut len");
            UpdateSkillSelectionVisual();
            UpdateSkillDetailUI(currentSkills[currentSkillIndex]);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSkillIndex = (currentSkillIndex + 1) % currentSkillUIs.Count;
            //Debug.LogWarning("Nut xuong");
            UpdateSkillSelectionVisual();
            UpdateSkillDetailUI(currentSkills[currentSkillIndex]);
        }
    }
    private void ClearHero()
    {
        if (heroImage != null) this.heroImage.sprite = null;
        if (hereName != null) this.hereName.text = "";
    }
    public void LoadSkillUI(HeroStateMachine hero)
    {
        foreach (Transform child in this.skillUISpacer)
        {
            Destroy(child.gameObject);
        }

        this.currentSkillUIs.Clear();
        this.currentSkills.Clear(); // đảm bảo bạn có danh sách currentSkills
        this.currentSkillIndex = 0;

        foreach (var skill in hero.baseHero.specialAttacks)
        {
            Debug.Log(skill);
            GameObject newSkillUI = Instantiate(this.skillUI, this.skillUISpacer);
            Image skillImage = newSkillUI.transform.Find("Button").Find("SkillImage").GetComponent<Image>();
            TextMeshProUGUI skillName = newSkillUI.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
            skillImage.sprite = skill.attackImage;
            skillName.text = skill.attackName;

            this.currentSkillUIs.Add(newSkillUI);
            this.currentSkills.Add(skill); // lưu lại skill tương ứng
            
        }
        this.UpdateSkillDetailUI(this.currentSkills[currentSkillIndex]);
        this.UpdateSkillSelectionVisual();
        
    }
    public void LoadHero(HeroStateMachine hero)
    {
        this.ClearHero();
        this.heroImage.sprite = hero.baseHero.heroImage;
        this.hereName.text = hero.baseHero.theName;
    }
    private void UpdateSkillDetailUI(BaseAttack skill)
    {
        // Xóa các icon cũ
        foreach (Transform child in this.effectSkillSpacer.transform)
        {
            Destroy(child.gameObject);
        }
        // Tạo icon mới nếu có effect
        SetAttackTypes(skill.effect1, this.effectSkillSpacer.transform);
        SetAttackTypes(skill.effect2, this.effectSkillSpacer.transform);
        // Cập nhật mô tả và mana cost
        if (skillDescText != null)
            skillDescText.text = skill.attackDescription;

        if (skillCostText != null)
            skillCostText.text = skill.attackCost.ToString();

        
    }
    public void SetAttackTypes(BaseAttack.Effect attackType, Transform spawnPosition)
    {

        // Tải prefab của icon từ Resources
        GameObject attackPrefab = Resources.Load<GameObject>($"AttackTypeIconImage/{attackType}Icon");

        if (attackPrefab != null)
        {
            GameObject attackIcon = Instantiate(attackPrefab); // Tạo icon và đặt vào vị trí
            attackIcon.transform.SetParent(spawnPosition, false);
            attackIcon.name = $"{attackType}Icon"; // Đặt tên để kiểm tra sau này
        }
        else
        {
            //Debug.LogWarning($"Không tìm thấy icon cho {attackType}");
            return;
        }
    }
    private void UpdateSkillSelectionVisual()
    {
        for (int i = 0; i < currentSkillUIs.Count; i++)
        {
            GameObject skillUI = currentSkillUIs[i];
            Image bg = skillUI.GetComponent<Image>();
            if (bg != null)
            {
                // Màu xám nhạt cho skill được chọn, và trong suốt nhẹ cho skill không chọn
                bg.color = (i == currentSkillIndex) ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0f);
            }
        }
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
        }

        // Xác định hero đang được chọn khi vào Skill
        currentSwapIndex = heroesInCombat.IndexOf(selectedHero);
        if (currentSwapIndex < 0) currentSwapIndex = 0; // fallback nếu không tìm thấy
        this.HeroSelected();
    }
    private void LoadCurrentHeroUI()
    {
        if (currentSwapIndex < 0 || currentSwapIndex >= heroesInCombat.Count) return;

        HeroStateMachine currentHero = heroesInCombat[currentSwapIndex];
        this.LoadHero(currentHero);
        this.LoadSkillUI(currentHero);
        this.HeroSelected();
    }
    private void HeroSelected()
    {
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
}
