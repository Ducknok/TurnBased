using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using Inventory;

public class CombatStateMachine : MonoBehaviour
{
    public CombatZone combatZone;
    public enum PerformAction
    {
        WAIT,
        PLAYERACTION,
        ENEMYACTION,
        PERFORMACTION,
        CHECKALIVE,
        WIN,
        LOSE
    }

    public PerformAction combatState;

    public List<HandleTurn> performList = new List<HandleTurn>();
    public List<GameObject> playersInCombat = new List<GameObject>();
    public List<GameObject> enemiesInCombat = new List<GameObject>();

    public enum PlayerGUI
    {
        ACTIVATE,
        WAITING,
        INPUT1,
        DONE,
    }

    public PlayerGUI playerInput;

    public List<GameObject> playerToManage = new List<GameObject>();
    private HandleTurn playerChoice;

    [Header("Panel")]
    public GameObject actionPanel;
    public GameObject skillPanel;
    public GameObject attackTypeInfoPanel;
    //public GameObject ememySelectPanel;
    [Header("Select enemy")]
    private int selectedEnemyIndex = 0;
    public bool isSelectingEnemy = false; // Biến cờ để kiểm tra trạng thái
    public bool isEnemyActing = false;
    private GameObject enemySelected;

    [Header("Enemy panel")]
    public Transform enemyInfoSpacer;
    public GameObject enemyInfoPanel;
    private EnemyPanelStats enemyStats;
    private List<GameObject> enemyInfoList = new List<GameObject>();
    public List<GameObject> enemiesAttacked = new List<GameObject>(); // Lưu enemy đã tấn công


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
    private List<GameObject> attackButtons = new List<GameObject>();
    private List<GameObject> skillsButtons = new List<GameObject>();
    private List<GameObject> attackTypeInfoList = new List<GameObject>(); 

    [Header("Select action")]
    private int selectedIndex = 0; // Lưu index của button đang được chọn

    [Header("Turn")]
    public bool heroTurn;
    public bool enemyTurn;
    public List<GameObject> heroesDoneTurn = new List<GameObject>();


    // Start is called before the first frame update
    private void Start()
    {
        this.combatState = PerformAction.WAIT;
        this.combatZone = GameObject.FindObjectOfType<CombatZone>();
        
        this.actionPanel.SetActive(false);
        this.skillPanel.SetActive(false);
        this.heroTurn = true;
        playerInput = PlayerGUI.ACTIVATE;
    }
    private void Awake()
    {
        //if (this.enemiesInCombat != null) return;
        this.enemiesInCombat.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        //if (this.playersInCombat != null) return;
        this.playersInCombat.AddRange(GameObject.FindGameObjectsWithTag("Player"));
    }
    // Update is called once per frame
    [System.Obsolete]
    private void Update()
    {
        this.AreAllHeroesDone();
        if (this.isSelectingEnemy) this.SelectEnemyWithKeyboard();
        switch (this.combatState)
        {
            case (PerformAction.WAIT):
                if(this.performList.Count > 0)
                {
                    if (this.performList[0].Type == "Enemy")
                    {
                        this.combatState = PerformAction.ENEMYACTION;
                    }
                    if(this.performList[0].Type == "Hero")
                    {
                        this.combatState = PerformAction.PLAYERACTION;
                    }
                }
                break;
            case (PerformAction.ENEMYACTION): 
                GameObject enemyPerformer = GameObject.Find(performList[0].Attacker);
                if (this.performList[0].Type == "Enemy")
                {
                    EnemyStateMachine esm = enemyPerformer.GetComponent<EnemyStateMachine>();
                    for (int i = 0; i < this.playersInCombat.Count; i++)
                    {
                        if (this.performList[0].AttackerTarget == this.playersInCombat[i])
                        {
                            esm.playerToAttack = this.performList[0].AttackerTarget;
                            esm.currentState = EnemyStateMachine.TurnState.ACTION;
                            break;
                        }
                        else
                        {
                            this.performList[0].AttackerTarget = this.playersInCombat[Random.Range(0, this.playersInCombat.Count)];
                            esm.playerToAttack = performList[0].AttackerTarget;
                            esm.currentState = EnemyStateMachine.TurnState.ACTION;
                        }
                    }

                }
                
                combatState = PerformAction.PERFORMACTION;
                break;
            case (PerformAction.PLAYERACTION):
                if (performList.Count > 0)
                {
                    GameObject playerPerformer = GameObject.Find(performList[0].Attacker);
                    if (this.performList[0].Type == "Hero")
                    {
                        //Debug.Log("Hero is here to perform");
                        HeroStateMachine hsm = playerPerformer.GetComponent<HeroStateMachine>();
                        hsm.enemyToAttack = this.performList[0].AttackerTarget;
                        hsm.currentState = HeroStateMachine.TurnState.ACTION;
                    }

                    combatState = PerformAction.PERFORMACTION;
                }    
                break;

            case (PerformAction.PERFORMACTION):
                break;

            case (PerformAction.CHECKALIVE):
                
                if (this.playersInCombat.Count < 1)
                {
                    this.combatState = PerformAction.LOSE;
                    //Lose game;
                }
                else if (this.enemiesInCombat.Count < 1)
                {
                    this.combatState = PerformAction.WIN;
                    //Win the battle
                    for (int i = 0; i < playersInCombat.Count; i++)
                    {
                        playersInCombat[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
                    }
                }
                else
                {
                    //call function
                    this.ClearAttackPanel();
                    if(this.AreAllHeroesDone()) this.heroesDoneTurn.Clear();
                    if(this.AreAllEnemiesDone())
                    {
                        Debug.LogWarning("Enemy done");
                        //Nếu tất cả enemy đã tấn công xong, reset Lock & Timer, rồi chuyển lượt cho player

                        this.enemiesAttacked.Clear();
                        this.heroTurn = true;
                        this.enemyTurn = false;
                        foreach (var enemy in this.enemiesInCombat)
                        {
                            EnemyStateMachine esm = enemy.GetComponent<EnemyStateMachine>();
                            esm.timer = Random.Range(1, this.playersInCombat.Count + 1);
                            esm.ChooseAction();
                        }
                    }
                    else
                    {
                        this.heroTurn = true;
                        this.enemyTurn = false;
                    }
                    this.combatState = PerformAction.WAIT;
                    this.playerInput = PlayerGUI.ACTIVATE;
                }
                break;
            case (PerformAction.LOSE):
                Debug.Log("Loser");
                break;
            case (PerformAction.WIN):
                Debug.Log("Win");
                this.combatZone.EndCombat();
                break;
        }
        switch (this.playerInput)
        {
            case (PlayerGUI.ACTIVATE):
                if (this.playerToManage.Count > 0)
                {
                    // Bỏ qua Hero đã thực hiện hành động
                    while (this.playerToManage.Count > 0 && this.heroesDoneTurn.Contains(this.playerToManage[0]))
                    {
                        this.playerToManage.RemoveAt(0);
                        this.actionPanel.SetActive(false);
                    }

                    if (playerToManage.Count > 0)
                    {
                        playerToManage[0].transform.Find("Body").transform.Find("Choose").gameObject.SetActive(true);
                        this.playerChoice = new HandleTurn();
                        this.actionPanel.SetActive(true);
                        this.CreateButton();
                        this.playerInput = PlayerGUI.WAITING;
                    }
                }
                break;
            case (PlayerGUI.WAITING):
                this.CheckState();
                
                break;
            case (PlayerGUI.DONE):
                this.PlayerInputDone();
                break;
        }
    }

    //Collect action
    public void CollectAction(HandleTurn input)
    {
        performList.Add(input);
    }

    //-----------------------------------Enemy------------------------------------
    //Select enemy 
    public void SelectEnemyWithKeyboard()
    {
        if (!isSelectingEnemy || enemiesInCombat.Count == 0) return;

        // Điều khiển chọn kẻ địch bằng phím trái/phải
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            selectedEnemyIndex = (selectedEnemyIndex - 1 + enemiesInCombat.Count) % enemiesInCombat.Count;
            HighlightEnemy();
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            selectedEnemyIndex = (selectedEnemyIndex + 1) % enemiesInCombat.Count;
            HighlightEnemy();
        }

        // Xác nhận chọn kẻ địch bằng phím Enter hoặc Space
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ConfirmEnemySelection();
        }
    }

    // Hiển thị highlight kẻ địch được chọn
    private void HighlightEnemy()
    {
        if (enemySelected != null)
        {
            enemySelected.SetActive(false); // Ẩn highlight kẻ địch trước đó
        }

        GameObject enemy = enemiesInCombat[selectedEnemyIndex];
        //Debug.Log(enemy.transform.GetComponent<EnemyStateMachine>().baseEnemy.theName);
        this.ClearEnemyInfoPanel();
        this.CreateEnemyInfoPanel(enemy);
        enemySelected = enemy.transform.Find("ChooseEnemy").gameObject;
        enemySelected.SetActive(true);
        
        
    }

    // Xác nhận chọn kẻ địch
    private void ConfirmEnemySelection()
    {
        this.playerChoice.AttackerTarget = enemiesInCombat[selectedEnemyIndex];
        this.isSelectingEnemy = false;
        this.playerInput = PlayerGUI.DONE;
        
        StartCoroutine(DeactivateAfterAction());
    }

    //Deactivate after action
    private IEnumerator DeactivateAfterAction()
    {
        yield return new WaitForSeconds(1f); // Thời gian chờ (hoặc sau khi hoàn thành hành động)
        if (enemySelected != null)
        {
            enemySelected.SetActive(false);
        }
    }

    //--------------------------------------------Player-----------------------------------------
    //Attack button (Input)
    public void NormalAttack()
    {
        this.skillPanel.SetActive(false);
        this.playerChoice.Attacker = playerToManage[0].name;
        this.playerChoice.AttacksGameObject = playerToManage[0];
        this.playerChoice.Type = "Hero";
        this.playerChoice.choosenAttack = playerToManage[0].GetComponent<HeroStateMachine>().baseHero.attacks[0];
        this.CreateAttackTypeInfoPanel(this.playerChoice.choosenAttack);
        this.isSelectingEnemy = true;
        this.actionPanel.SetActive(false);
    }

    //Open skill panel
    public void OpenSkillPanel()
    {
        this.actionPanel.SetActive(false);
        this.skillPanel.SetActive(true);
        this.selectedIndex = 0;
        if (this.skillsButtons.Count > 0)
        {

            EventSystem.current.SetSelectedGameObject(this.skillsButtons[selectedIndex]);
        }
    }

    //Skill attack
    public void SkillAttack(BaseAttack choosenSkill)
    {
        //Choose skills
        this.playerChoice.Attacker = this.playerToManage[0].name;
        this.playerChoice.AttacksGameObject = this.playerToManage[0];
        this.playerChoice.Type = "Hero";
        // Check curMP
        if (this.playerToManage[0].GetComponent<HeroStateMachine>().baseHero.curMP < choosenSkill.attackCost)
        {
            Debug.Log("Không đủ mana!");
            return;
        }

        this.playerChoice.choosenAttack = choosenSkill;
        this.CreateAttackTypeInfoPanel(this.playerChoice.choosenAttack);
        this.skillPanel.SetActive(false);
        this.isSelectingEnemy = true;
        this.actionPanel.SetActive(false);
    }


    //Player input done
    [System.Obsolete]
    protected void PlayerInputDone()
    {
        this.performList.Add(this.playerChoice);
        this.ClearAttackPanel();
        this.ClearAttackTypeInfoPanel();
        if (this.playerToManage.Count > 0)
        {
            GameObject currentHero = playerToManage[0];
            currentHero.transform.Find("Body").transform.Find("Choose").gameObject.SetActive(false);
            heroesDoneTurn.Add(currentHero); // Đánh dấu Hero đã hành động
            playerToManage.RemoveAt(0);
        }

        if (this.playerToManage.Count == 0)
        {
            playerInput = PlayerGUI.WAITING;
        }
    }


    //Clear attack panel
    public void ClearAttackPanel()
    {
        this.actionPanel.SetActive(false);
        foreach (GameObject attackButton in attackButtons)
        {
            DestroyImmediate(attackButton, true);
        }
        foreach(GameObject skillsButton in skillsButtons)
        {
            DestroyImmediate(skillsButton, true);
        }
        this.skillsButtons.Clear();
        this.attackButtons.Clear();
    }

    //Create actionButtons
    public void CreateButton()
    {
        this.CreateAttackButton();
        this.CreateSkillButton();
        this.CreateSwapButton();
        this.CreateItemButton();
        if (this.attackButtons.Count > 0)
        {
            selectedIndex = 0; // Đặt lại index khi tạo mới
            SelectButton(selectedIndex);
        }
    }
    public void CreateAttackButton()
    {
        GameObject attackButton = Instantiate(this.actionButton) as GameObject;
        TextMeshProUGUI attackButtonText = attackButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        attackButtonText.text = "Attack";
        attackButton.GetComponent<Button>().onClick.AddListener(() => NormalAttack());
        attackButton.transform.SetParent(this.actionSpacer, false);
        this.attackButtons.Add(attackButton);
    }
    public void CreateSkillButton()
    {
        GameObject skillButton = Instantiate(this.skillsBarButton) as GameObject;
        TextMeshProUGUI skillButtonText = skillButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        skillButtonText.text = "Skills";
        skillButton.GetComponent<Button>().onClick.AddListener(() => OpenSkillPanel());
        skillButton.transform.SetParent(this.actionSpacer, false);
        this.attackButtons.Add(skillButton);

        if (this.playerToManage[0].GetComponent<HeroStateMachine>().baseHero.skills.Count > 0)
        {
            foreach (BaseAttack skill in this.playerToManage[0].GetComponent<HeroStateMachine>().baseHero.skills)
            {
                GameObject skillsButton = Instantiate(this.skillsButton) as GameObject;
                TextMeshProUGUI skillsName = skillsButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI mpCost = skillsButton.transform.Find("MpCost").gameObject.GetComponent<TextMeshProUGUI>();
                skillsName.text = skill.attackName.ToString();
                mpCost.text = skill.attackCost.ToString();
                // Kiểm tra nếu hero không đủ MP
                if (this.playerToManage[0].GetComponent<HeroStateMachine>().baseHero.curMP < skill.attackCost)
                {
                    skillsName.color = Color.gray;
                    skillsButton.GetComponent<Button>().interactable = false; // Vô hiệu hóa nếu không đủ MP
                }
                else
                {
                    AttackButton atb = skillsButton.GetComponent<AttackButton>();
                    atb.skillAttackToPerform = skill;
                }
                skillsButton.transform.SetParent(this.skillSpacer, false);
                this.skillsButtons.Add(skillsButton);
                
            }
        }
        else
        {
            //Dung <Button>().interactable de vo hieu hoa button
            skillButton.GetComponent<Button>().interactable = false;
        }
    }
    public void CreateItemButton()
    {
        GameObject itemButton = Instantiate(this.itemButton) as GameObject;
        TextMeshProUGUI itemButtonText = itemButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        itemButtonText.text = "Items";
        //itemButton.GetComponent<Button>().onClick.AddListener(() => NormalAttack());
        itemButton.transform.SetParent(this.actionSpacer, false);
        this.attackButtons.Add(itemButton);
    }
    public void CreateSwapButton()
    {
        GameObject swapButton = Instantiate(this.swapButton) as GameObject;
        TextMeshProUGUI swapButtonText = swapButton.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
        swapButtonText.text = "Swap";
        //itemButton.GetComponent<Button>().onClick.AddListener(() => NormalAttack());
        swapButton.transform.SetParent(this.actionSpacer, false);
        this.attackButtons.Add(swapButton);
    }
    private void CheckState()
    {
        if (playerInput == PlayerGUI.WAITING)
        {
            if (skillPanel.activeSelf) // Nếu đang ở skill panel
            {
                if (skillsButtons.Count > 0)
                {
                    if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                    {
                        selectedIndex = (selectedIndex + 1) % skillsButtons.Count;
                        Debug.Log(selectedIndex);
                        SelectSkillButton(selectedIndex);
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                    {
                        selectedIndex = (selectedIndex - 1 + skillsButtons.Count) % skillsButtons.Count;
                        SelectSkillButton(selectedIndex);
                    }
                    else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        
                        ExecuteEvents.Execute(skillsButtons[selectedIndex], new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
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
            }
            else if (attackButtons.Count > 0) // Nếu đang ở action panel
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    selectedIndex = (selectedIndex + 1) % attackButtons.Count;
                    SelectButton(selectedIndex);
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    selectedIndex = (selectedIndex - 1 + attackButtons.Count) % attackButtons.Count;
                    SelectButton(selectedIndex);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    ExecuteEvents.Execute(attackButtons[selectedIndex], new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                }
            }
        }
    }

    private void SelectSkillButton(int index)
    {
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(skillsButtons[index]);
        EventSystem.current.firstSelectedGameObject = skillsButtons[index];
    }

    private void SelectButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(attackButtons[index]);
    }

    public bool AreAllHeroesDone()
    {
        foreach (GameObject hero in playersInCombat)
        {
            HeroStateMachine heroSM = hero.GetComponent<HeroStateMachine>();
            if (heroSM != null && heroSM.currentState != HeroStateMachine.TurnState.WAITING && heroSM.currentState != HeroStateMachine.TurnState.DEAD)
            {
                return false; // Còn hero chưa xong lượt
            }
        }
        return true; // Tất cả hero đã xong lượt
    }
    public bool AreAllEnemiesDone()
    {
        return this.enemiesInCombat.TrueForAll(enemy =>
        {
            return this.enemiesAttacked.Contains(enemy);
        });
    }

    //Create attack type info panel
    public void CreateAttackTypeInfoPanel(BaseAttack chooseSkill)
    {
        GameObject attackTypeInfo = Instantiate(this.attackTypeInfoPanel) as GameObject;
        this.attackInfo = attackTypeInfo.transform.GetComponent<AttackInfoPanel>();
        this.attackInfo.attackName.text = chooseSkill.attackName;
        this.attackInfo.attackTypeInfo.text = chooseSkill.attackDescription;
        this.SetAttackTypes(chooseSkill.effect1, this.attackInfo.attackTypeIconSpacer.transform);
        this.SetAttackTypes(chooseSkill.effect2, this.attackInfo.attackTypeIconSpacer.transform);
        //Debug.LogWarning(enemyStat.enemyHPBarFill);
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


    //Create Enemy info panel
    public void CreateEnemyInfoPanel(GameObject enemy)
    {
        //this.ClearEnemyInfoPanel();
        GameObject enemyInfo = Instantiate(this.enemyInfoPanel) as GameObject;
        this.enemyStats = enemyInfo.transform.GetComponent<EnemyPanelStats>();

        EnemyStateMachine etm = enemy.GetComponent<EnemyStateMachine>();
        this.enemyStats.enemyName.text = etm.baseEnemy.theName;
        this.enemyStats.enemyHP.text = etm.baseEnemy.curHP.ToString();
        etm.enemyHPBarFill = this.enemyStats.hpBarFill;
        etm.curHpNumber = this.enemyStats.enemyHP;
        etm.enemyHPBarFill.fillAmount = etm.baseEnemy.curHP / etm.baseEnemy.baseHP;
        //Debug.LogWarning(enemyStat.enemyHPBarFill);
        enemyInfo.transform.SetParent(this.enemyInfoSpacer, false);
        this.enemyInfoList.Add(enemyInfo);
    }
    public void ClearEnemyInfoPanel()
    {
        foreach(GameObject enemyInfo in enemyInfoList)
        {
            DestroyImmediate(enemyInfo);
        }
        this.enemyInfoList.Clear();
    }
    public void UpdateEnemyTimer()
    {
        for (int i = enemiesInCombat.Count - 1; i >= 0; i--)
        {
            var enemySM = enemiesInCombat[i].GetComponent<EnemyStateMachine>();
            // Giảm timer
            enemySM.timer--;
            // Nếu timer đạt 0, thực hiện xử lý
            if (enemySM.timer == 0)
            {
                // Xóa toàn bộ danh sách enemy đã tấn công
                this.enemyTurn = true;
                this.heroTurn = false;
                // Gọi UI cập nhật
                var enemyUI = enemiesInCombat[i].GetComponent<EnemyUI>();
                StartCoroutine(enemyUI.ClearTimerIcon());
                StartCoroutine(enemyUI.ClearAllAttackTypeIcons());
            }
        }
    }


}
