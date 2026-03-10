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

    [Header("Select enemy")]
    private int selectedEnemyIndex = 0;
    public bool isSelectingEnemy = false;
    public bool isEnemyActing = false;
    private int maxSelectableEnemies;
    private List<GameObject> selectedEnemies = new List<GameObject>();

    // ĐÃ SỬA: Biến này giờ sẽ lưu GameObject của QUÁI VẬT, thay vì lưu cái vòng sáng ChooseEnemy
    private GameObject enemySelected;

    [Header("Enemy panel")]
    public Transform enemyInfoSpacer;
    public GameObject enemyInfoPanel;
    private EnemyPanelStats enemyStats;
    private List<GameObject> enemyInfoList = new List<GameObject>();
    public List<GameObject> enemiesAttacked = new List<GameObject>();

    [Header("Turn")]
    public bool heroTurn;
    public bool enemyTurn;
    public List<GameObject> heroesDoneTurn = new List<GameObject>();

    protected override void Awake()
    {
        this.enemiesInCombat.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        this.playersInCombat.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        this.enemyInfoSpacer = GameObject.Find("BattleCanvas").transform.Find("Panel").transform.Find("UIEnemyInfo").transform.Find("EnemyInfoPanelSpacer");
    }

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
        this.enemyInfoSpacer = GameObject.Find("BattleCanvas").transform.Find("Panel").transform.Find("UIEnemyInfoPanel").transform.Find("EnemyInfoPanelSpacer");
    }

    public void StartEnemySelection(BaseAttack skillData)
    {
        if (enemiesInCombat.Count == 0) return;
        isSelectingEnemy = true;
        selectedEnemies.Clear();
        selectedEnemyIndex = 0;

        maxSelectableEnemies = skillData.maxEnemyCount;

        // Đảm bảo tắt highlight của quái vật cũ trước khi chọn mới
        if (enemySelected != null)
        {
            EnemySelected es = enemySelected.GetComponent<EnemySelected>();
            if (es != null) es.HideChooseEnemy();
        }

        HighlightEnemy(); // Highlight ban đầu
    }

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

        if (enemyPerformer == null)
        {
            Debug.LogWarning("Không tìm thấy kẻ địch: " + performList[0].Attacker + ". Lượt đánh sẽ bị hủy bỏ!");
            this.performList.RemoveAt(0);
            this.combatState = PerformAction.WAIT;
            return;
        }
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
            if (playerPerformer == null)
            {
                Debug.LogWarning("Không tìm thấy Hero: " + performList[0].Attacker + ". Lượt đánh bị hủy.");
                this.performList.RemoveAt(0);
                this.combatState = PerformAction.WAIT;
                return;
            }
            if (this.performList[0].Type == "Hero")
            {
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
        }
        else if (this.enemiesInCombat.Count < 1)
        {
            this.combatState = PerformAction.WIN;
            for (int i = 0; i < playersInCombat.Count; i++)
            {
                playersInCombat[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
            }
        }
        else
        {
            this.ClearAttackPanel();
            if (this.AreAllHeroesDone()) this.heroesDoneTurn.Clear();
            if (this.AreAllEnemiesDone() && this.performList.Count == 0)
            {
                Debug.LogWarning("Enemy done");
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

    public void CollectAction(HandleTurn input)
    {
        performList.Add(input);
    }

    //-----------------------------------Enemy------------------------------------
    public void SelectEnemyWithKeyboard()
    {
        if (!isSelectingEnemy || enemiesInCombat.Count == 0) return;

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

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ConfirmEnemySelection();
        }
    }

    public void HighlightEnemy()
    {
        if (enemiesInCombat == null || enemiesInCombat.Count == 0)
        {
            return;
        }

        if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemiesInCombat.Count)
        {
            selectedEnemyIndex = 0;
        }

        if (enemySelected != null)
        {
            EnemySelected oldSelection = enemySelected.GetComponent<EnemySelected>();
            if (oldSelection != null)
            {
                oldSelection.HideChooseEnemy();
            }
        }

        GameObject enemy = enemiesInCombat[selectedEnemyIndex];
        this.ClearEnemyInfoPanel();
        this.CreateEnemyInfoPanel(enemy);

        enemySelected = enemy;

        HeroStateMachine activeHero = null;
        if (playerToManage.Count > 0)
        {
            activeHero = playerToManage[0].GetComponent<HeroStateMachine>();
        }


        EnemySelected newSelection = enemySelected.GetComponent<EnemySelected>();
        if (newSelection != null)
        {
            newSelection.ShowChooseEnemy(activeHero);
        }
    }

    private void ConfirmEnemySelection()
    {
        this.playerChoice.AttackerTarget = enemiesInCombat[selectedEnemyIndex];
        this.isSelectingEnemy = false;
        this.playerInput = PlayerGUI.DONE;

        StartCoroutine(DeactivateAfterAction());
    }

    private IEnumerator DeactivateAfterAction()
    {
        yield return new WaitForSeconds(1f);

        // Đã sửa lại để gọi hàm Hide thay vì SetActive(false)
        if (enemySelected != null)
        {
            EnemySelected es = enemySelected.GetComponent<EnemySelected>();
            if (es != null) es.HideChooseEnemy();

            enemySelected = null;
        }
    }

    //--------------------------------------------Hero-----------------------------------------
    protected void PlayerInputDone()
    {
        this.performList.Add(this.playerChoice);
        this.ClearAttackPanel();
        ButtonController.Instance.ClearAttackTypeInfoPanel();
        if (this.playerToManage.Count > 0)
        {
            GameObject currentHero = playerToManage[0];
            currentHero.transform.Find("Body").transform.Find("Choose").gameObject.SetActive(false);
            heroesDoneTurn.Add(currentHero);
            playerToManage.RemoveAt(0);
        }

        if (this.playerToManage.Count == 0)
        {
            playerInput = PlayerGUI.WAITING;
        }
    }

    public void ClearAttackPanel()
    {
        ButtonController.Instance.actionPanel.SetActive(false);
        foreach (GameObject attackButton in ButtonController.Instance.actionButtons)
        {
            DestroyImmediate(attackButton, true);
        }
        foreach (GameObject skillsButton in ButtonController.Instance.skillsButtons)
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

    public void CreateEnemyInfoPanel(GameObject enemy)
    {
        GameObject enemyInfo = Instantiate(this.enemyInfoPanel) as GameObject;
        this.enemyStats = enemyInfo.transform.GetComponent<EnemyPanelStats>();

        EnemyStateMachine etm = enemy.GetComponent<EnemyStateMachine>();
        this.enemyStats.enemyName.text = etm.baseEnemy.theName;
        this.enemyStats.enemyHP.text = etm.baseEnemy.curHP.ToString();
        etm.enemyHPBarFill = this.enemyStats.hpBarFill;
        etm.curHpNumber = this.enemyStats.enemyHP;
        etm.enemyHPBarFill.fillAmount = (float)etm.baseEnemy.curHP / etm.baseEnemy.baseHP;

        enemyInfo.transform.SetParent(this.enemyInfoSpacer, false);
        this.enemyInfoList.Add(enemyInfo);
    }

    public void ClearEnemyInfoPanel()
    {
        foreach (GameObject enemyInfo in enemyInfoList)
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
            enemySM.timer--;

            if (enemySM.timer <= 0)
            {
                this.enemyTurn = true;
                this.heroTurn = false;

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
                heroesToRevive.RemoveAt(i);
            }
        }
    }
}