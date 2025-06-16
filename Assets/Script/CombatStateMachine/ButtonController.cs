using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    private CombatController combatCtrl;
    [Header("Player Panel")]
    public Transform actionSpacer;
    public Transform skillSpacer;
    public Transform attackTypeInfoSpacer;
    public GameObject actionButton;
    public GameObject skillsBarButton;
    public GameObject skillsButton;
    public GameObject itemButton;
    public GameObject swapButton;
    public AttackInfoPanel attackInfo;
    public List<GameObject> actionButtons = new List<GameObject>();
    public List<GameObject> skillsButtons = new List<GameObject>();
    private List<GameObject> attackTypeInfoList = new List<GameObject>();
    [Header("Select action")]
    private int selectedIndex = 0; // Lưu index của button đang được chọn
    [Header("Panel")]
    public GameObject actionPanel;
    public GameObject skillPanel;
    public GameObject attackTypeInfoPanel;
    private void Awake()
    {
        this.LoadCBM();
    }
    private void LoadCBM()
    {
        if (this.combatCtrl != null) return;
        this.combatCtrl = FindObjectOfType<CombatController>();
    }
    public void CheckState()
    {
        if (this.combatCtrl.CBM.playerInput == CombatStateMachine.PlayerGUI.WAITING)
        {
            if (skillPanel.activeSelf) // Nếu đang ở skill panel
            {
                if (skillsButtons.Count > 0)
                {
                    // Đảm bảo selectedIndex hợp lệ
                    selectedIndex = Mathf.Clamp(selectedIndex, 0, skillsButtons.Count - 1);

                    if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                    {
                        selectedIndex = (selectedIndex + 1) % skillsButtons.Count;
                        Debug.Log($"Chọn skill index: {selectedIndex}, Tên: {skillsButtons[selectedIndex].GetComponentInChildren<TextMeshProUGUI>().text}");
                        SelectSkillButton(selectedIndex);
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                    {
                        selectedIndex = (selectedIndex - 1 + skillsButtons.Count) % skillsButtons.Count;
                        Debug.Log($"Chọn skill index: {selectedIndex}, Tên: {skillsButtons[selectedIndex].GetComponentInChildren<TextMeshProUGUI>().text}");
                        SelectSkillButton(selectedIndex);
                    }
                    else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        if (skillsButtons[selectedIndex] != null)
                        {
                            var attackButton = skillsButtons[selectedIndex].GetComponent<AttackButton>();
                            if (attackButton != null && attackButton.skillAttackToPerform != null)
                            {
                                Debug.Log($"Kích hoạt skill: {attackButton.skillAttackToPerform.name}");
                                //Debug.Log(selectedIndex);
                            }
                            ExecuteEvents.Execute(skillsButtons[selectedIndex], new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                        }
                        else
                        {
                            Debug.LogWarning("Nút skill tại index " + selectedIndex + " là null!");
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) ||
                             Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                    {
                        skillPanel.SetActive(false);
                        actionPanel.SetActive(true);
                        selectedIndex = 0;
                        SelectButton(selectedIndex);
                    }
                }
                else
                {
                    Debug.LogWarning("Danh sách skillsButtons rỗng!");
                }
            }
            else if (actionButtons.Count > 0) // Nếu đang ở action panel
            {
                // Đảm bảo selectedIndex hợp lệ
                selectedIndex = Mathf.Clamp(selectedIndex, 0, actionButtons.Count - 1);

                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    selectedIndex = (selectedIndex + 1) % actionButtons.Count;
                    SelectButton(selectedIndex);
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    selectedIndex = (selectedIndex - 1 + actionButtons.Count) % actionButtons.Count;
                    SelectButton(selectedIndex);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    if (actionButtons[selectedIndex] != null)
                    {
                        ExecuteEvents.Execute(actionButtons[selectedIndex], new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                        string btnName = actionButtons[selectedIndex].GetComponentInChildren<TextMeshProUGUI>().text;
                        ///Debug.Log($"Kích hoạt action: {btnName}");
                        if (btnName == "Attack")
                        {
                            this.combatCtrl.CBM.HighlightEnemy();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Nút action tại index " + selectedIndex + " là null!");
                    }
                }
            }
        }
    }
    //Attack button (Input)
    public void NormalAttack()
    {
        this.skillPanel.SetActive(false);
        this.combatCtrl.CBM.playerChoice.Attacker = this.combatCtrl.CBM.playerToManage[0].name;
        this.combatCtrl.CBM.playerChoice.AttacksGameObject = this.combatCtrl.CBM.playerToManage[0];
        this.combatCtrl.CBM.playerChoice.Type = "Hero";
        this.combatCtrl.CBM.playerChoice.choosenAttack = this.combatCtrl.CBM.playerToManage[0].GetComponent<HeroStateMachine>().baseHero.normalAttacks[0];
        this.combatCtrl.CBM.isSelectingEnemy = true;
        this.actionPanel.SetActive(false);
    }


    //Skill attack
    public void SkillAttack(SkillBehaviour choosenSkill)
    {
        //Debug.LogError(choosenSkill);
        //Choose skills
        this.combatCtrl.CBM.playerChoice.Attacker = this.combatCtrl.CBM.playerToManage[0].name;
        this.combatCtrl.CBM.playerChoice.AttacksGameObject = this.combatCtrl.CBM.playerToManage[0];
        this.combatCtrl.CBM.playerChoice.Type = "Hero";
        HeroStateMachine hsm = this.combatCtrl.CBM.playerToManage[0].GetComponent<HeroStateMachine>();
        // Check curMP
        if (hsm.baseHero.curMP < choosenSkill.skillData.attackCost)
        {
            Debug.Log("Không đủ mana!");
            return;
        }
        this.combatCtrl.CBM.playerChoice.choosenAttack = choosenSkill.skillData;
        this.skillPanel.SetActive(false);
        this.combatCtrl.CBM.isSelectingEnemy = true;
        this.actionPanel.SetActive(false);
    }
    //Create actionButtons
    public void CreateButton()
    {
        this.CreateAttackButton();
        this.CreateSkillButton();
        this.CreateSwapButton();
        this.CreateItemButton();
        if (this.actionButtons.Count > 0)
        {
            selectedIndex = 0; // Đặt lại index khi tạo mới
            SelectButton(selectedIndex);
        }
    }
    public void CreateAttackButton()
    {
        GameObject attackButton = Instantiate(this.actionButton) as GameObject;
        TextMeshProUGUI attackButtonText = attackButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        HeroStateMachine hero = this.combatCtrl.CBM.playerToManage[0].GetComponent<HeroStateMachine>();
        attackButtonText.text = hero.baseHero.normalAttacks[0].attackName;
        SkillBehaviour skillBehaviour = hero.GetSkillBehaviourForAttack(hero.baseHero.normalAttacks[0]);
        BaseAttack normalAttack = hero.baseHero.normalAttacks[0];
        AttackButton atb = attackButton.GetComponent<AttackButton>();
        if (atb != null)
        {
            atb.skillAttackToPerform = skillBehaviour;
        }
        this.AddTooltipTrigger(attackButton, normalAttack);
        attackButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (atb != null && atb.skillAttackToPerform != null)
            {
                hero.currentAttack = atb.skillAttackToPerform;
                //Debug.LogWarning(heroStateMachine.currentAttack);
            }
        });
        attackButton.GetComponent<Button>().onClick.AddListener(() => this.NormalAttack());
        attackButton.transform.SetParent(this.actionSpacer, false);
        this.actionButtons.Add(attackButton);
    }
    public void CreateSkillButton()
    {
        // Tạo nút skill chính
        GameObject skillButton = Instantiate(this.skillsBarButton) as GameObject;
        TextMeshProUGUI skillButtonText = skillButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        skillButtonText.text = "Skills";

        // Xóa listener cũ và thêm listener mới
        Button buttonComponent = skillButton.GetComponent<Button>();
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(() => this.OpenSkillPanel());

        skillButton.transform.SetParent(this.actionSpacer, false);
        this.actionButtons.Add(skillButton);

        // Kiểm tra dữ liệu nhân vật
        if (this.combatCtrl.CBM.playerToManage != null &&
            this.combatCtrl.CBM.playerToManage.Count > 0 &&
            this.combatCtrl.CBM.playerToManage[0] != null &&
            this.combatCtrl.CBM.playerToManage[0].GetComponent<HeroStateMachine>() != null &&
            this.combatCtrl.CBM.playerToManage[0].GetComponent<HeroStateMachine>().baseHero != null &&
            this.combatCtrl.CBM.playerToManage[0].GetComponent<HeroStateMachine>().baseHero.specialAttacks != null &&
            this.combatCtrl.CBM.playerToManage[0].GetComponent<HeroStateMachine>().baseHero.specialAttacks.Count > 0)
        {
            HeroStateMachine heroStateMachine = this.combatCtrl.CBM.playerToManage[0].GetComponent<HeroStateMachine>();

            // Xóa các nút skill cũ
            foreach (GameObject btn in this.skillsButtons)
            {
                Destroy(btn);
            }
            this.skillsButtons.Clear();

            // Xóa tất cả các con trong skillSpacer
            foreach (Transform child in this.skillSpacer)
            {
                Destroy(child.gameObject);
            }

            // Tạo các nút skill mới
            foreach (BaseAttack baseSkill in heroStateMachine.baseHero.specialAttacks)
            {
                if (baseSkill != null && baseSkill.attackType == BaseAttack.AttackType.SpecialAttack)
                {
                    // Tạo bản sao cục bộ của skill để tránh vấn đề closure
                    BaseAttack currentSkill = baseSkill;
                    SkillBehaviour skillBehaviour = heroStateMachine.GetSkillBehaviourForAttack(currentSkill);

                    if (skillBehaviour != null)
                    {
                        GameObject skillsButton = Instantiate(this.skillsButton) as GameObject;
                        TextMeshProUGUI skillsName = skillsButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI mpCost = skillsButton.transform.Find("MpCost").gameObject.GetComponent<TextMeshProUGUI>();

                        skillsName.text = currentSkill.attackName.ToString();
                        mpCost.text = currentSkill.attackCost.ToString();

                        // Gán skill cho AttackButton
                        AttackButton atb = skillsButton.GetComponent<AttackButton>();
                        if (atb != null)
                        {
                            atb.skillAttackToPerform = skillBehaviour;
                        }

                        // Thêm tooltip
                        this.AddTooltipTrigger(skillsButton, currentSkill);

                        Button skillButtonComponent = skillsButton.GetComponent<Button>();
                        // Xóa listener cũ để tránh chồng chéo
                        skillButtonComponent.onClick.RemoveAllListeners();

                        // Kiểm tra MP để kích hoạt nút
                        if (heroStateMachine.baseHero.curMP < currentSkill.attackCost)
                        {
                            skillsName.color = Color.gray;
                            skillButtonComponent.interactable = false;
                        }
                        else
                        {
                            // Gán listener cho nút
                            AddSkillButtonListener(skillsButton, heroStateMachine, skillBehaviour);
                        }

                        skillsButton.transform.SetParent(this.skillSpacer, false);
                        this.skillsButtons.Add(skillsButton);
                    }
                }
            }
        }
        else
        {
            buttonComponent.interactable = false;
        }
    }

    private void AddSkillButtonListener(GameObject button, HeroStateMachine hero, SkillBehaviour skill)
    {
        Button buttonComponent = button.GetComponent<Button>();
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(() =>
        {
            hero.currentAttack = skill;
            //Debug.Log($"Đã chọn skill: {skill.name} (ID: {skill.GetInstanceID()})"); // Ghi log để kiểm tra
        });
    }
    public void CreateItemButton()
    {
        GameObject itemButton = Instantiate(this.itemButton) as GameObject;
        TextMeshProUGUI itemButtonText = itemButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        itemButtonText.text = "Items";
        //itemButton.GetComponent<Button>().onClick.AddListener(() => NormalAttack());
        itemButton.transform.SetParent(this.actionSpacer, false);
        this.actionButtons.Add(itemButton);
    }
    public void CreateSwapButton()
    {
        GameObject swapButton = Instantiate(this.swapButton) as GameObject;
        TextMeshProUGUI swapButtonText = swapButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        swapButtonText.text = "Swap";
        //itemButton.GetComponent<Button>().onClick.AddListener(() => NormalAttack());
        swapButton.transform.SetParent(this.actionSpacer, false);
        this.actionButtons.Add(swapButton);
    }
    //Create attack type info panel
    public void CreateAttackTypeInfoPanel(BaseAttack chooseSkill)
    {
        GameObject attackTypeInfo = Instantiate(this.attackTypeInfoPanel) as GameObject;
        this.attackInfo = attackTypeInfo.transform.GetComponent<AttackInfoPanel>();
        if (this.attackInfo == null)
        {
            Debug.LogError("AttackInfoPanel component not found on attackTypeInfoPanel prefab!");
            return;
        }

        if (this.attackInfo.attackName == null)
        {
            Debug.LogError("attackInfo.attackName is null – Check if you've assigned it in the AttackInfoPanel component.");
            return;
        }
        this.attackInfo.attackName.text = chooseSkill.attackName;
        this.attackInfo.attackTypeInfo.text = chooseSkill.attackDescription;
        this.SetAttackTypes(chooseSkill.effect1, this.attackInfo.attackTypeIconSpacer.transform);
        this.SetAttackTypes(chooseSkill.effect2, this.attackInfo.attackTypeIconSpacer.transform);
        attackTypeInfo.transform.SetParent(this.attackTypeInfoSpacer, false);
        this.attackTypeInfoList.Add(attackTypeInfo);
    }
    public void ClearAttackTypeInfoPanel()
    {
        foreach (GameObject attackTypeInfo in attackTypeInfoList)
        {
            DestroyImmediate(attackTypeInfo);
        }
        this.attackTypeInfoList.Clear();
    }
    private void AddTooltipTrigger(GameObject button, BaseAttack skill)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        entry.callback.AddListener((eventData) =>
        {
            //Debug.LogWarning(skill);
            ClearAttackTypeInfoPanel();
            CreateAttackTypeInfoPanel(skill);
        });

        trigger.triggers.Add(entry);
    }
    private void SelectSkillButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(skillsButtons[index]);
        EventSystem.current.firstSelectedGameObject = skillsButtons[index];
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
    private void SelectButton(int index)
    {
        //Debug.LogWarning(index);
        //Debug.LogWarning(actionButtons[index]);
        EventSystem.current.SetSelectedGameObject(actionButtons[index]);
    }
    //Open skill panel
    public void OpenSkillPanel()
    {
        this.actionPanel.SetActive(false);
        this.skillPanel.SetActive(true);
        selectedIndex = 0; 
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(this.skillsButtons[selectedIndex]);
        selectedIndex = 0;
        if (this.skillsButtons.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(this.skillsButtons[selectedIndex]);
        }
    }

}
