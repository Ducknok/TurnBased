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
    private ButtonController butCtrl;
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
    private void Start()
    {
        this.combatState = PerformAction.WAIT;
        this.combatZone = GameObject.FindObjectOfType<CombatZone>();
        
        this.butCtrl.actionPanel.SetActive(false);
        this.butCtrl.skillPanel.SetActive(false);
        this.heroTurn = true;
        playerInput = PlayerGUI.ACTIVATE;
    }
    private void Awake()
    {
        //if (this.enemiesInCombat != null) return;
        this.enemiesInCombat.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        //if (this.playersInCombat != null) return;
        this.playersInCombat.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        this.LoadButCtrl();
    }
    private void LoadButCtrl()
    {
        if (this.butCtrl != null) return;
        this.butCtrl = FindObjectOfType<ButtonController>();
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
                        this.butCtrl.actionPanel.SetActive(false);
                    }

                    if (playerToManage.Count > 0)
                    {
                        playerToManage[0].transform.Find("Body").transform.Find("Choose").gameObject.SetActive(true);
                        this.playerChoice = new HandleTurn();
                        this.butCtrl.actionPanel.SetActive(true);
                        this.butCtrl.CreateButton();
                        this.playerInput = PlayerGUI.WAITING;
                    }
                }
                break;
            case (PlayerGUI.WAITING):
                this.butCtrl.CheckState();
                
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
    [System.Obsolete]
    protected void PlayerInputDone()
    {
        this.performList.Add(this.playerChoice);
        this.ClearAttackPanel();
        this.butCtrl.ClearAttackTypeInfoPanel();
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
        this.butCtrl.actionPanel.SetActive(false);
        foreach (GameObject attackButton in this.butCtrl.actionButtons)
        {
            DestroyImmediate(attackButton, true);
        }
        foreach(GameObject skillsButton in this.butCtrl.skillsButtons)
        {
            DestroyImmediate(skillsButton, true);
        }
        this.butCtrl.skillsButtons.Clear();
        this.butCtrl.actionButtons.Clear();
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
