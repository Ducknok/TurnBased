using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;
using Inventory;
using System.Linq;
using UnityEngine.SceneManagement;

public class HeroStateMachine : DucMonobehaviour
{
    public AgentWeapon agentWeapon;
    public BaseHero baseHero;
    public ItemInventoryController inventoryController;
    public SkillBehaviour currentAttack;
    public HeroPanelHandler heroPanelHandler;

    public enum TurnState
    {
        PROCESSING,
        ADDTOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD,
    }

    public TurnState currentState;
    public GameObject choose;
    //Choose Player for action
    private int selectedHeroIndex = 0; // Hero đang được chọn
    //IENUMERATOR
    public GameObject enemyToAttack;
    public float animSpeed = 15f;
    private int heroIndex; // Chỉ số của hero trong playerPositions
    public bool attackEnd;
    private bool actionStarted;
    //esm.
    public GameObject body;
    public Animator anim;
    private bool alive = true;
    
    //Revive hero
    public int turnsToRevive = 0;
    public int reviveTurnThreshold = 3;
    // Start is called before the first frame update
    protected override void Awake()
    {
        var existingHeroes = FindObjectsOfType<HeroStateMachine>();

        foreach (var hero in existingHeroes)
        {
            if (hero != this && hero.baseHero.theName == this.baseHero.theName)
            {
                Destroy(this.gameObject); // hoặc set inactive nếu cần giữ
                return;
            }
        }
        this.body = this.transform.Find("Body").gameObject;
        this.anim = this.body.GetComponent<Animator>();

        this.LoadComponent();
        DontDestroyOnLoad(this.gameObject);
    }
    protected override void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        this.choose.SetActive(false);
        //this.inventoryController = this.transform.GetComponent<InventoryController>();
        this.currentState = TurnState.PROCESSING;
    }

    protected override void Update()
    {
        this.HandleCurrentState();  
    }

    private void LoadComponent()
    {
        this.agentWeapon = this.GetComponent<AgentWeapon>();
        this.heroPanelHandler = this.transform.GetComponent<HeroPanelHandler>();
        this.baseHero.curHP = this.baseHero.baseHP;
        this.baseHero.curMP = this.baseHero.baseMP;
    }
    // Handle current state and check state
    private void HandleCurrentState()
    {
        //Debug.Log(this.currentState);
        // Kiểm tra nếu chuột trái được click
        switch (this.currentState)
        {
            case (TurnState.PROCESSING):
                this.HandleHeroSelection();
                break;
            case (TurnState.ADDTOLIST):
                this.CheckAddToList();
                break;
            case (TurnState.WAITING):
                this.CheckWaiting();
                break;
            case (TurnState.DEAD):
                this.CheckAlive();
                break;
            case (TurnState.ACTION):
                StartCoroutine(TimeForAction());
                break;
        }
    }
    private void CheckAddToList()
    {
        if (!CombatController.Instance.CBM.playerToManage.Contains(this.gameObject))
        {
            CombatController.Instance.CBM.playerToManage.Add(this.gameObject);
        }
        this.currentState = TurnState.WAITING;
    }
    private void CheckAlive()
    {
        if (!this.alive)
        {
            return;
        }
        else
        {
            //not attackable by enemy
            CombatController.Instance.CBM.playersInCombat.Remove(this.gameObject);
            //not managable
            CombatController.Instance.CBM.playerToManage.Remove(this.gameObject);

            //reset gui
            ButtonController.Instance.actionPanel.SetActive(false);
            //remove item from performList
            if (CombatController.Instance.CBM.playersInCombat.Count > 0)
            {
                for (int i = 0; i < CombatController.Instance.CBM.performList.Count; i++)
                {
                    if (CombatController.Instance.CBM.performList[i].AttacksGameObject == this.gameObject)
                    {
                        CombatController.Instance.CBM.performList.Remove(CombatController.Instance.CBM.performList[i]);
                    }

                    if (CombatController.Instance.CBM.performList[i].AttackerTarget == this.gameObject)
                    {
                        CombatController.Instance.CBM.performList[i].AttackerTarget = CombatController.Instance.CBM.playersInCombat[Random.Range(0, CombatController.Instance.CBM.playersInCombat.Count)];
                    }
                }
            }
            //animation dead
            this.anim.SetTrigger("Dead");
            // Không tạo action và skill panel nếu hero đã chết
            ButtonController.Instance.actionPanel.SetActive(false);
            ButtonController.Instance.skillPanel.SetActive(false);
            //reset heroinput
            CombatController.Instance.CBM.combatState = CombatStateMachine.PerformAction.CHECKALIVE;
            this.alive = false;
            this.turnsToRevive = 0; // Bắt đầu đếm từ 0
            CombatController.Instance.CBM.heroesToRevive.Add(this); // Thêm vào danh sách để theo dõi hồi sinh
            //deactivate the selector
            if (this.choose == null) return;
            this.choose.SetActive(false);
        }
    }
    private void CheckWaiting()
    {
        if (CombatController.Instance.CBM.heroTurn && !CombatController.Instance.CBM.heroesDoneTurn.Contains(this.gameObject) && CombatController.Instance.CBZ.isInCombat)
        {
            this.HandleHeroSelection();
            this.currentState = TurnState.PROCESSING;
        }
    }
    public void HeroPosition()
    {
        // Xác định chỉ số của hero trong playerPosition
        for (int i = 0; i < CombatController.Instance.CBZ.heros.Length; i++)
        {
            if (CombatController.Instance.CBZ.heros[i] == this.gameObject)
            {
                this.heroIndex = i;
                break;
            }
        }

        if (this.heroIndex == -1)
        {
            Debug.LogError($"Không tìm thấy hero {this.gameObject.name} trong CombatSystem.players.");
        }
    }
    // Thay đổi cách chọn hero
    private void HandleHeroSelection()
    {
        if (!CombatController.Instance.CBZ.isInCombat ||CombatController.Instance.CBM.isSelectingEnemy || CombatController.Instance.CBM.enemyTurn) return;
        //this.ChangeHeroSelection(1);
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            ChangeHeroSelection(-1);
        }
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            ChangeHeroSelection(1);
        }

    }
    private void ChangeHeroSelection(int direction)
    {
        if (CombatController.Instance.CBZ.heros.Length == 0) return;

        // Tắt action panel của hero cũ nếu có
        if (selectedHeroIndex >= 0 && selectedHeroIndex < CombatController.Instance.CBZ.heros.Length)
        {
            var previousHero = CombatController.Instance.CBZ.heros[selectedHeroIndex].GetComponent<HeroStateMachine>();

            if (previousHero != null)
            {
                this.choose.SetActive(false);
                CombatController.Instance.CBM.playerToManage.Remove(previousHero.gameObject);
                CombatController.Instance.CBM.ClearAttackPanel();
                CombatController.Instance.CBM.playerInput = CombatStateMachine.PlayerGUI.ACTIVATE;
            }
        }

        // Tìm hero mới không chết
        int newIndex = selectedHeroIndex;
        do
        {
            newIndex += direction;

            // Quay vòng nếu đi quá giới hạn
            if (newIndex < 0) newIndex = CombatController.Instance.CBZ.heros.Length - 1;
            if (newIndex >= CombatController.Instance.CBZ.heros.Length) newIndex = 0;

            // Nếu hero mới không chết, thì chọn nó
            if (CombatController.Instance.CBZ.heros[newIndex].GetComponent<HeroStateMachine>().currentState != TurnState.DEAD)
            {
                selectedHeroIndex = newIndex;
                break;
            }

        } while (newIndex != selectedHeroIndex); // Nếu quay lại hero cũ thì dừng lại (tất cả hero đã chết)

        // Thêm hero mới vào danh sách
        var selectedHero = CombatController.Instance.CBZ.heros[selectedHeroIndex].GetComponent<HeroStateMachine>();
        if (!CombatController.Instance.CBM.playerToManage.Contains(selectedHero.gameObject))
        {
            CombatController.Instance.CBM.playerToManage.Add(selectedHero.gameObject);
        }
        // Cập nhật vị trí actionPanel & skillPanel
        this.UpdateActionPanelPosition(selectedHero);
    }
    //Update action position
    private void UpdateActionPanelPosition(HeroStateMachine selectedHero)
    {
        RectTransform actionPanelRect = ButtonController.Instance.actionPanel.GetComponent<RectTransform>();
        RectTransform skillPanelRect = ButtonController.Instance.skillPanel.GetComponent<RectTransform>();

        // Lấy Canvas
        Canvas canvas = actionPanelRect.GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        // Chuyển đổi World Position của Hero sang Local Position trong Canvas
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            Camera.main.WorldToScreenPoint(selectedHero.transform.Find("Body").Find("Choose").position),
            null, // Overlay không cần camera
            out localPosition
        );

        // Điều chỉnh khoảng cách giữa Hero và Panel
        Vector2 offset = new Vector2(150f, -50f);

        // Cập nhật vị trí của Action Panel và Skill Panel
        actionPanelRect.anchoredPosition = localPosition + offset;
        skillPanelRect.anchoredPosition = localPosition + offset;

        // Hiển thị Panel
        ButtonController.Instance.actionPanel.SetActive(true);
        ButtonController.Instance.skillPanel.SetActive(false);
    }
    private IEnumerator TimeForAction()
    {
        if (this.actionStarted)
        {
            yield break;
        }
        this.actionStarted = true;
        //wait abit
        //animate back to start position

        StartCoroutine(this.currentAttack.Activate(this.gameObject, this.enemyToAttack)) ;
        yield return new WaitForSeconds(2f);
        CombatController.Instance.CBM.UpdateEnemyTimer();
        StartCoroutine(MoveTowardsStart());
    }
    
   
    public void ReviveHero()
    {
        HealController.Instance.RestoreHPAfterRevive(this);
        ManaController.Instance.RestoreManaAfterRevive(this);
        this.alive = true;
        this.currentState = TurnState.WAITING;

        this.heroPanelHandler.UpdateHeroPanel();
        this.anim.Play("IdleBattle");

        CombatController.Instance.CBM.playersInCombat.Add(this.gameObject);

        //Debug.Log($"{this.baseHero.theName} đã được hồi sinh!");
    }
    protected virtual bool MoveTowardsStart(Vector3 target)
    {
        Transform body = this.transform.Find("Body");
        if (body == null) return false;

        body.DOMove(target, 0.5f).SetEase(Ease.Linear);
        return Vector3.Distance(body.position, target) > 0.1f;
        //return target != (this.transform.Find("Body").position = Vector3.MoveTowards(this.transform.Find("Body").position, target, animSpeed * Time.deltaTime));
    }
    IEnumerator MoveTowardsStart()
    {
        yield return new WaitForSeconds(0.25f);
        if (CombatController.Instance.CBM.performList[0].choosenAttack.attackType == BaseAttack.AttackType.NormalAttack)
        {
            DamagePopup.Create(this.body.transform.position, 3f, false, true);
        }
        this.anim.Play("IdleBattle");
        yield return new WaitForSeconds(0.5f);
        Vector3 firstPosition = CombatController.Instance.CBZ.playerPositions[this.heroIndex].position;
        //Debug.LogError(firstPosition + ": First position");
        while (this.MoveTowardsStart(firstPosition)) { yield return null; }
        //remvoe this performer from the list in CSM
        CombatController.Instance.CBM.performList.RemoveAt(0);


        //reset CSM  -> WAIT
        if (CombatController.Instance.CBM.combatState != CombatStateMachine.PerformAction.WIN && CombatController.Instance.CBM.combatState != CombatStateMachine.PerformAction.LOSE)
        {
            CombatController.Instance.CBM.combatState = CombatStateMachine.PerformAction.WAIT;
            //reset this player state
            this.currentState = TurnState.WAITING;
        }
        if (CombatController.Instance.CBM.AreAllHeroesDone())
        {
            CombatController.Instance.CBM.heroesDoneTurn.Clear();

            Debug.LogWarning("hero done");
            CombatController.Instance.CBM.UpdateHeroRevival();
            if (CombatController.Instance.CBM.AreAllEnemiesDone() && CombatController.Instance.CBM.performList.Count == 0)
            {
                CombatController.Instance.CBM.heroesDoneTurn.Clear();
                CombatController.Instance.CBM.heroTurn = true;
                CombatController.Instance.CBM.enemyTurn = false;
            }
            
        }
        this.actionStarted = false;
    }
    public SkillBehaviour GetSkillBehaviourForAttack(BaseAttack baseAttack)
    {
        // Kiểm tra xem có skill behaviour nào đã được tạo cho skill này chưa
        //Debug.LogWarning(baseAttack);
        SkillBehaviour existingSkill = GetComponentsInChildren<SkillBehaviour>()
            .FirstOrDefault(s => s.skillData.attackName == baseAttack.attackName);
       //Debug.LogWarning(existingSkill);
        if (existingSkill != null)
        {
            return existingSkill;
        }

        // Nếu chưa có, tạo mới
        string skillPrefabPath = "Skills/" + baseAttack.attackName.Replace(" ", "");
        GameObject skillPrefab = Resources.Load<GameObject>(skillPrefabPath);
        //Debug.LogWarning(skillPrefabPath);
        Transform skillSpacer = this.transform.Find("Skills");
        if (skillPrefab != null)
        {
            GameObject skillObj = Instantiate(skillPrefab, skillSpacer);
            SkillBehaviour newSkill = skillObj.GetComponent<SkillBehaviour>();
            if (newSkill != null)
            {
                newSkill.skillData = baseAttack;
                return newSkill;
            }
        }

        Debug.LogWarning("Không tìm thấy prefab cho skill: " + baseAttack.attackName);
        return null;
    }

    
}
