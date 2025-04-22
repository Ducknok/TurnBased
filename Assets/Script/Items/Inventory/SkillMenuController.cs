using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillMenuController : MonoBehaviour
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

    private void Update()
    {

        if (!this.isSkillMenuOpen || currentSkillUIs.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSkillIndex = (currentSkillIndex - 1 + currentSkillUIs.Count) % currentSkillUIs.Count;
            UpdateSkillSelectionVisual();
            UpdateSkillDetailUI(currentSkills[currentSkillIndex]);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSkillIndex = (currentSkillIndex + 1) % currentSkillUIs.Count;
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

        currentSkillUIs.Clear();
        currentSkills.Clear(); // đảm bảo bạn có danh sách currentSkills
        currentSkillIndex = 0;

        foreach (var skill in hero.baseHero.skills)
        {
            GameObject newSkillUI = Instantiate(this.skillUI, this.skillUISpacer);
            Image skillImage = newSkillUI.transform.Find("Button").Find("SkillImage").GetComponent<Image>();
            TextMeshProUGUI skillName = newSkillUI.transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
            skillImage.sprite = skill.attackImage;
            skillName.text = skill.attackName;

            currentSkillUIs.Add(newSkillUI);
            currentSkills.Add(skill); // lưu lại skill tương ứng
        }

        this.UpdateSkillSelectionVisual();
        this.UpdateSkillDetailUI(currentSkills[currentSkillIndex]);

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

        // Cập nhật mô tả và mana cost
        if (skillDescText != null)
            skillDescText.text = skill.attackDescription;

        if (skillCostText != null)
            skillCostText.text = skill.attackCost.ToString();

        // Tạo icon mới nếu có effect
        SetAttackTypes(skill.effect1, this.effectSkillSpacer.transform);
        SetAttackTypes(skill.effect2, this.effectSkillSpacer.transform);
    }

    public void SetAttackTypes(BaseAttack.Effect attackType, Transform spawnPosition)
    {
        // Kiểm tra xem attackType đã tồn tại chưa
        foreach (Transform child in spawnPosition)
        {
            if (child.name == $"{attackType}Icon") // So sánh với tên icon
            {
                //Debug.Log($"Attack type {attackType} đã tồn tại, không tạo mới.");
                return; // Không tạo lại nếu đã có
            }
        }

        // Tải prefab của icon từ Resources
        GameObject attackPrefab = Resources.Load<GameObject>($"AttackTypeIconImage/{attackType}Icon");

        if (attackPrefab != null)
        {
            GameObject attackIcon = Instantiate(attackPrefab); // Tạo icon và đặt vào vị trí
            attackIcon.transform.SetParent(spawnPosition);
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
                bg.color = (i == currentSkillIndex) ? new Color(0, 0, 0, 0) : new Color(1f, 1f, 1f, 0.2f);
            }
        }
    }
}
