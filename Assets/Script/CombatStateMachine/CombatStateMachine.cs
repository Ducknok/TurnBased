using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
using Inventory;
using UnityEngine.SceneManagement;

public class CombatStateMachine : DucMonobehaviour
{
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
    public List<HeroStateMachine> heroesToRevive = new List<HeroStateMachine>();
    public enum PlayerGUI
    {
        ACTIVATE,
        WAITING,
        INPUT1,
        DONE,
    }
    public PlayerGUI playerInput;
    public List<GameObject> playerToManage = new List<GameObject>();
    public HandleTurn playerChoice;
    //public GameObject ememySelectPanel;
    [Header("Select enemy")]
    private int selectedEnemyIndex = 0;
    public bool isSelectingEnemy = false; // Biến cờ để kiểm tra trạng thái
    public bool isEnemyActing = false;
    private int maxSelectableEnemies;
    private List<GameObject> selectedEnemies = new List<GameObject>();
    private GameObject enemySelected;
    [Header("Enemy panel")]
    public Transform enemyInfoSpacer;
    public GameObject enemyInfoPanel;
    private EnemyPanelStats enemyStats;
    private List<GameObject> enemyInfoList = new List<GameObject>();
    public List<GameObject> enemiesAttacked = new List<GameObject>(); // Lưu enemy đã tấn công
    [Header("Turn")]
    public bool heroTurn;
    public bool enemyTurn;
    public List<GameObject> heroesDoneTurn = new List<GameObject>();
    // Start is called before the first frame update
    protected override void Start()
    {
        this.combatState = PerformAction.WAIT;

        ButtonController.Instance.actionPanel.SetActive(false);
        ButtonController.Instance.skillPanel.SetActive(false);
        this.heroTurn = true;
        playerInput = PlayerGUI.ACTIVATE;
    }
    protected override void Update()
    {
        this.AreAllHeroesDone();
        if (this.isSelectingEnemy) this.SelectEnemyWithKeyboard();
        this.HandleCombatState();
        this.HandlePlayerInputState();
    }

    protected override void Awake()
    {
        //if (this.enemiesInCombat != null) return;
        this.enemiesInCombat.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        //if (this.playersInCombat != null) return;
        this.playersInCombat.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        this.enemyInfoSpacer = GameObject.Find("BattleCanvas").transform.Find("Panel").transform.Find("EnemyInfoPanel").transform.Find("EnemyInfoPanelSpacer");
        //DontDestroyOnLoad(this.gameObject);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this.enemiesInCombat != null) return;
        this.enemiesInCombat.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        if (this.playersInCombat != null) return;
        this.playersInCombat.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        this.enemyInfoSpacer = GameObject.Find("BattleCanvas").transform.Find("Panel").transform.Find("EnemyInfoPanel").transform.Find("EnemyInfoPanelSpacer");
    }

    public void StartEnemySelection(BaseAttack skillData)
    {
        if (enemiesInCombat.Count == 0) return;
        isSelectingEnemy = true;
        selectedEnemies.Clear();
        selectedEnemyIndex = 0;

        maxSelectableEnemies = skillData.maxEnemyCount;
        HighlightEnemy(); // Highlight ban đầu
    }
    // Update is called once per frame
    
    private void HandleCombatState()
    {
        switch (this.combatState)
        {
            case (PerformAction.WAIT):
                this.CheckWait();
                break;
            case (PerformAction.ENEMYACTION):
                this.CheckEnemyAction();
                break;
            case (PerformAction.PLAYERACTION):
                this.CheckPlayerAction();
                break;
            case (PerformAction.PERFORMACTION):
                break;
            case (PerformAction.CHECKALIVE):
                this.CheckAlive();
                break;
            case (PerformAction.LOSE):
                Debug.Log("Loser");
                break;
            case (PerformAction.WIN):
                Debug.Log("Win");
                CombatController.Instance.CBZ.EndCombat();
                break;
        }
    }
    private void CheckWait()
    {
        if (this.performList.Count > 0)
        {
            if (this.performList[0].Type == "Enemy")
            {
                this.combatState = PerformAction.ENEMYACTION;
            }
            if (this.performList[0].Type == "Hero")
            {
                this.combatState = PerformAction.PLAYERACTION;
            }
        }
    }
    private void CheckEnemyAction()
    {
        if (performList == null || performList.Count == 0)
        {
            Debug.LogWarning("performList is empty!");
            return;
        }

        if (playersInCombat == null || playersInCombat.Count == 0)
        {
            Debug.LogWarning("playersInCombat is empty!");
            return;
        }

        GameObject enemyPerformer = GameObject.Find(performList[0].Attacker);

        if (performList[0].Type == "Enemy")
        {
            EnemyStateMachine esm = enemyPerformer.GetComponent<EnemyStateMachine>();
            if (esm.isLockBrokenOnce)
            {
                esm.CheckCombatState();
                return;
            }
            bool foundTarget = false;
            for (int i = 0; i < playersInCombat.Count; i++)
            {
                if (performList[0].AttackerTarget == playersInCombat[i])
                {
                    esm.playerToAttack = performList[0].AttackerTarget;
                    esm.currentState = EnemyStateMachine.TurnState.ACTION;
                    foundTarget = true;
                    break;
                }
            }

            if (!foundTarget)
            {
                performList[0].AttackerTarget = playersInCombat[Random.Range(0, playersInCombat.Count)];
                esm.playerToAttack = performList[0].AttackerTarget;
                esm.currentState = EnemyStateMachine.TurnState.ACTION;
            }
        }

        combatState = PerformAction.PERFORMACTION;
    }
    private void CheckPlayerAction()
    {
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
    }
    private void CheckAlive()
    {
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
            if (this.AreAllHeroesDone()) this.heroesDoneTurn.Clear();
            if (this.AreAllEnemiesDone() && this.performList.Count == 0)
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
    }
    private void HandlePlayerInputState()
    {
        switch (this.playerInput)
        {
            case (PlayerGUI.ACTIVATE):
                this.CheckActivate();
                break;
            case (PlayerGUI.WAITING):
                ButtonController.Instance.CheckState();
                break;
            case (PlayerGUI.DONE):
                this.PlayerInputDone();
                break;
        }
    }
    private void CheckActivate()
    {
        if (this.playerToManage.Count > 0)
        {
            // Bỏ qua Hero đã thực hiện hành động
            while (this.playerToManage.Count > 0 && this.heroesDoneTurn.Contains(this.playerToManage[0]))
            {
                this.playerToManage.RemoveAt(0);
                ButtonController.Instance.actionPanel.SetActive(false);
            }

            if (playerToManage.Count > 0 && this.heroTurn)
            {
                playerToManage[0].transform.Find("Body").transform.Find("Choose").gameObject.SetActive(true);
                this.playerChoice = new HandleTurn();
                ButtonController.Instance.actionPanel.SetActive(true);
                ButtonController.Instance.CreateButton();
                this.playerInput = PlayerGUI.WAITING;
            }
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
    public void HighlightEnemy()
    {
        // Không có enemy nào trong danh sách
        if (enemiesInCombat == null || enemiesInCombat.Count == 0)
        {
            Debug.LogWarning("Không có enemy nào trong danh sách.");
            return;
        }

        // Nếu chỉ số hiện tại không hợp lệ thì reset về 0
        if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemiesInCombat.Count)
        {
            selectedEnemyIndex = 0;
        }

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
    //--------------------------------------------Hero-----------------------------------------
    //Player input done
    protected void PlayerInputDone()
    {
        this.performList.Add(this.playerChoice);
        this.ClearAttackPanel();
        ButtonController.Instance.ClearAttackTypeInfoPanel();
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
        ButtonController.Instance.actionPanel.SetActive(false);
        foreach (GameObject attackButton in ButtonController.Instance.actionButtons)
        {
            DestroyImmediate(attackButton, true);
        }
        foreach(GameObject skillsButton in ButtonController.Instance.skillsButtons)
        {
            DestroyImmediate(skillsButton, true);
        }
        ButtonController.Instance.skillsButtons.Clear();
        ButtonController.Instance.actionButtons.Clear();
    }
    public bool AreAllHeroesDone()
    {
        return this.heroesDoneTurn.Count == this.playersInCombat.Count;
    }
    public bool AreAllEnemiesDone()
    {
        return this.enemiesAttacked.Count == this.enemiesInCombat.Count;
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
            if (enemySM.timer <= 0)
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
    public void UpdateHeroRevival()
    {
        for (int i = heroesToRevive.Count - 1; i >= 0; i--)
        {
            HeroStateMachine hero = heroesToRevive[i];
            hero.turnsToRevive++;

            if (hero.turnsToRevive >= hero.reviveTurnThreshold)
            {
                hero.ReviveHero();
                heroesToRevive.RemoveAt(i); // Xóa khỏi danh sách
            }
        }
    }

    
}
