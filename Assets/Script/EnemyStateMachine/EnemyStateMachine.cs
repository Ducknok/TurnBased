using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;

public class EnemyStateMachine : DucMonobehaviour
{
    public CombatStateMachine combatStateMachine;
    public BaseEnemy baseEnemy;
    public SkillBehaviour currentAttack;
    public EnemyMoveToCombat enemyMoveToCombat;
    public enum TurnState
    {
        PROCESSING,
        WAITING,
        ACTION,
        DEAD,
    }

    public TurnState currentState;

    // This gameobject
    public GameObject choose;
    public Image enemyHPBarFill;
    public TextMeshProUGUI curHpNumber;

    // Time for action
    public HandleTurn savedAttack;
    public Animator anim;
    public GameObject playerToAttack;
    public bool actionStarted = false;
    public bool enemyAttacked;

    // Alive
    private bool alive = true;

    [Header("Lock")]
    public EnemyUI enemyUI;
    public List<LockSystem> activeLocks = new List<LockSystem>();
    public int maxAttackType = 2; // Maximum number of locks
    public int timer;
    public bool isLockBrokenOnce = false; // Check if lock has been broken once

    // Store initial position
    private Vector3 initialPosition;

    protected override void Awake()
    {
        this.combatStateMachine = GameObject.Find("CombatManager").GetComponent<CombatStateMachine>();
        this.enemyMoveToCombat = this.transform.GetComponentInChildren<EnemyMoveToCombat>();
        this.anim = this.transform.Find("Body").GetComponent<Animator>();
        this.enemyUI = this.transform.GetComponent<EnemyUI>();

    }
    // Start is called before the first frame update
    protected override void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        this.baseEnemy.curHP = this.baseEnemy.baseHP;
        this.timer = Random.Range(1, this.combatStateMachine.playersInCombat.Count + 1);

        this.currentState = TurnState.PROCESSING;
        this.choose.SetActive(false);
    }


    public void StartCombatFlow()
    {
        this.initialPosition = this.transform.position;

        this.ChooseAction();

        this.currentState = TurnState.WAITING;
    }

    // Update is called once per frame
    protected override void Update()
    {
        this.HandleCurrentState();
    }
    private void HandleCurrentState()
    {
        switch (this.currentState)
        {
            case TurnState.WAITING:
                this.CheckWaiting();
                break;
            case TurnState.DEAD:
                this.CheckAlive();
                break;
            case TurnState.ACTION:
                StartCoroutine(this.TimeForAction());
                break;
        }
    }
    private void CheckWaiting()
    {
        if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") == false)
        {
            this.anim.Play("Idle");
        }
        if (!this.combatStateMachine.heroTurn)
        {
            this.combatStateMachine.enemyTurn = true; // Enable enemy turn
        }
        if (this.combatStateMachine.enemyTurn && this.timer <= 0 && !this.enemyAttacked &&
             !this.combatStateMachine.enemiesAttacked.Contains(this.gameObject) &&
                this.currentState != TurnState.DEAD && this.alive) // THÊM KIỂM TRA ALIVE
        {
            this.enemyAttacked = true;
            this.combatStateMachine.CollectAction(savedAttack);
            //this.currentState = TurnState.ACTION;
        }
    }
    private void CheckAlive()
    {
        if (!this.alive)
        {
            return;
        }
        else
        {
            this.alive = false;
            this.combatStateMachine.enemiesInCombat.Remove(this.gameObject);

            for (int i = this.combatStateMachine.performList.Count - 1; i >= 0; i--)
            {
                if (this.combatStateMachine.performList[i].AttacksGameObject == this.gameObject)
                {
                    this.combatStateMachine.performList.RemoveAt(i);
                }
            }
            for (int i = this.combatStateMachine.enemiesAttacked.Count - 1; i >= 0; i--)
            {
                if (this.combatStateMachine.enemiesAttacked[i].gameObject == this.gameObject)
                {
                    this.combatStateMachine.enemiesAttacked.RemoveAt(i);
                }
            }

            // 2. GỌI COROUTINE TẠO NHỊP DỪNG TRƯỚC KHI GỤC NGÃ
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator DeathRoutine()
    {

        yield return new WaitForSeconds(0.6f);
        this.anim.Play("Dead");

        StartCoroutine(this.ClearEnemyInfo());
        StartCoroutine(this.DestroyObject());

        this.combatStateMachine.combatState = CombatStateMachine.PerformAction.CHECKALIVE;
    }
    public void ChooseAction()
    {
        
        this.savedAttack = new HandleTurn();
        if (this.savedAttack.Attacker != null) return;
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = this.baseEnemy.theName;
        myAttack.Type = "Enemy";
        myAttack.AttacksGameObject = this.gameObject;

        // Tránh lỗi nếu playersInCombat rỗng
        if (this.combatStateMachine.playersInCombat.Count > 0)
        {
            myAttack.AttackerTarget = this.combatStateMachine.playersInCombat[Random.Range(0, combatStateMachine.playersInCombat.Count)];
        }
        else
        {
            GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
            if (heroes.Length > 0) myAttack.AttackerTarget = heroes[Random.Range(0, heroes.Length)];
        }

        int num = Random.Range(0, this.baseEnemy.normalAttacks.Count);
        myAttack.choosenAttack = this.baseEnemy.normalAttacks[num];
        this.currentAttack = GetSkillBehaviourForAttack(myAttack.choosenAttack);
        if (myAttack == null) return;
        if (myAttack.choosenAttack.attackType == BaseAttack.AttackType.SpecialAttack)
        {
            this.timer = Random.Range(2, 4);
            this.GenerateLocks();
            this.GenerateTimerIcon();
        }
        else
        {
            this.GenerateTimerIcon();
        }

        this.savedAttack = myAttack;
    }

    // Attack
    protected virtual IEnumerator TimeForAction()
    {
        if (this.actionStarted || !this.enemyAttacked || this.isLockBrokenOnce)
        {
            yield break;
        }
        this.actionStarted = true;
        yield return StartCoroutine(this.currentAttack.Activate(this.gameObject, this.playerToAttack));


        this.combatStateMachine.enemiesAttacked.Add(this.gameObject);
        StartCoroutine(MoveTowardsStart());
    }

    public IEnumerator ClearEnemyInfo()
    {
        yield return new WaitForSeconds(0.5f);
        this.combatStateMachine.ClearEnemyInfoPanel();
    }

    protected IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(0.75f);
        Destroy(this.gameObject);
    }

    protected virtual IEnumerator MoveTowardsStart()
    {

        yield return new WaitForSeconds(0.6f);
        if (this.anim != null && !this.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            this.anim.Play("Idle");
        }

        this.transform.DOKill();

        this.transform.Find("Body").DOMove(initialPosition, 0.5f)
            .SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(1f);
        this.CheckCombatState();
        yield return new WaitForSeconds(1f);
        this.currentState = TurnState.WAITING;

    }
    public void CheckCombatState()
    {
        this.RemoveInPerformList();
        this.combatStateMachine.combatState = CombatStateMachine.PerformAction.WAIT;
        // Reset action flags
        this.actionStarted = false;
        this.enemyAttacked = false;
        this.isLockBrokenOnce = false;
        // Update enemiesAttacked list
        if (!this.combatStateMachine.enemiesAttacked.Contains(this.gameObject))
        {
            this.combatStateMachine.enemiesAttacked.Add(this.gameObject);
        }
        this.CheckTurn();
        // Set state back to WAITING
        this.currentState = TurnState.WAITING;
        //Debug.Log($"State changed to WAITING for {gameObject.name}");
    }
    public void RemoveInPerformList()
    {
        // Update combat state and remove action from performList
        if (this.combatStateMachine.performList.Count > 0 && this.combatStateMachine.performList[0].AttacksGameObject == this.gameObject)
        {
            this.combatStateMachine.performList.RemoveAt(0);
        }

    }
    private void CheckTurn()
    {
        bool allHeroesDone = this.combatStateMachine.AreAllHeroesDone();
        bool allEnemiesDone = this.combatStateMachine.AreAllEnemiesDone();

        if (allHeroesDone && allEnemiesDone)
        {
            // Reset enemy state, bắt đầu lượt mới cho hero
            this.ChooseActionAfterDone();
        }
        else if (allHeroesDone)
        {
            // Hero đã xong, chuyển lượt cho enemy
            this.combatStateMachine.enemiesAttacked.Clear();
            this.combatStateMachine.heroTurn = false;
            this.combatStateMachine.enemyTurn = true;
        }
        else
        {
            // Reset enemy state, bắt đầu lượt mới cho hero
            this.ChooseActionAfterDone();
        }
    }
    private void ChooseActionAfterDone()
    {
        this.combatStateMachine.enemiesAttacked.Clear();
        this.combatStateMachine.heroTurn = true;
        this.combatStateMachine.enemyTurn = false;
        this.timer = Random.Range(1, this.combatStateMachine.playersInCombat.Count + 1);
        this.ChooseAction();
    }

    //-----------------------------GENERATE-------------------------
    public virtual void GenerateLocks()
    {
       
        this.activeLocks.Clear();

        // 1. TÌM HỆ PHÁI CỦA CÁC HERO ĐANG CÓ TRÊN SÂN MỘT CÁCH CHẮC CHẮN NHẤT
        List<BaseAttack.Effect> heroEffects = new List<BaseAttack.Effect>();

        // Mở rộng phạm vi tìm kiếm: Không phụ thuộc vào mảng playersInCombat nữa
        List<GameObject> heroesToScan = new List<GameObject>();
        if (this.combatStateMachine != null && this.combatStateMachine.playersInCombat.Count > 0)
        {
            heroesToScan.AddRange(this.combatStateMachine.playersInCombat);
        }
        else
        {
            // Cứu cánh: Tự tìm trên toàn bản đồ
            heroesToScan.AddRange(GameObject.FindGameObjectsWithTag("Hero"));
        }

        Debug.Log($"[EnemyStateMachine] Bắt đầu quét điểm yếu. Đã tìm thấy {heroesToScan.Count} Heroes trên bản đồ.");

        foreach (GameObject heroGO in heroesToScan)
        {
            if (heroGO == null || !heroGO.activeInHierarchy) continue;
            HeroStateMachine hero = heroGO.GetComponent<HeroStateMachine>();

            if (hero != null && hero.baseHero != null)
            {
                // Lấy hệ nguyên tố (Wind, Lightning...)
                if (System.Enum.TryParse(hero.baseHero.elemental.ToString(), out BaseAttack.Effect elemEffect))
                {
                    heroEffects.Add(elemEffect);
                }

                // Lấy nghề nghiệp để suy ra đòn đánh (Warrior -> Sword, Lancer -> Lance)
                string hType = hero.baseHero.heroType.ToString();
                if (hType == "Warrior") heroEffects.Add(BaseAttack.Effect.Sword);
                else if (hType == "Lancer") heroEffects.Add(BaseAttack.Effect.Lance);
            }
        }

        // Lọc bỏ các hệ bị trùng nhau
        heroEffects = heroEffects.Distinct().ToList();

        // Đề phòng lỗi (Fallback): Nếu không có Hero nào (Điều này rất hiếm khi xảy ra nữa)
        if (heroEffects.Count == 0)
        {
            Debug.LogWarning("[EnemyStateMachine] KHÔNG THỂ TÌM THẤY HERO NÀO! Bắt buộc phải random toàn bộ hệ.");
            foreach (BaseAttack.Effect eff in System.Enum.GetValues(typeof(BaseAttack.Effect)))
            {
                heroEffects.Add(eff);
            }
        }
        else
        {
            Debug.Log($"[EnemyStateMachine] Điểm yếu chốt sổ cho Boss: {string.Join(", ", heroEffects)}");
        }

        // 2. SINH RA ĐIỂM YẾU BẤT KỲ TỪ DANH SÁCH CỦA HERO
        for (int i = 0; i < 1; i++)
        {
            int numTypes = 3; // Mỗi khóa có 3 điểm yếu
            List<BaseAttack.Effect> types = new List<BaseAttack.Effect>();

            for (int j = 0; j < numTypes; j++)
            {
                // Chỉ random trong số các hệ mà Hero đang sở hữu
                BaseAttack.Effect randomType = heroEffects[Random.Range(0, heroEffects.Count)];
                types.Add(randomType);
            }

            // Gửi danh sách đã bốc thăm này cho UI hiển thị
            this.enemyUI.SetAttackTypes(types);
            activeLocks.Add(new LockSystem(types));
        }
    }
    public virtual void GenerateTimerIcon()
    {
        
        this.enemyUI.SetTimerIcon(this.timer);
    }
    public SkillBehaviour GetSkillBehaviourForAttack(BaseAttack baseAttack)
    {
        SkillBehaviour existingSkill = GetComponentsInChildren<SkillBehaviour>()
            .FirstOrDefault(s => s.skillData.attackName == baseAttack.attackName);

        if (existingSkill != null)
        {
            return existingSkill;
        }

        string skillPrefabPath = "Skills/" + baseAttack.attackName.Replace(" ", "");
        GameObject skillPrefab = Resources.Load<GameObject>(skillPrefabPath);
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