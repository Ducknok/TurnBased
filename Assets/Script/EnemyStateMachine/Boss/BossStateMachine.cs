using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;

public class BossStateMachine : DucMonobehaviour
{
    public CombatStateMachine combatStateMachine;

    [Header("Boss Info")]
    public string bossName = "Hắc Long Vương";
    public BaseHero bossStats; // Vẫn giữ BaseHero cho Boss

    [Header("AI & Skills (Scriptable Objects)")]
    public List<BaseAttack> normalAttacks;
    public List<BaseAttack> specialAttacks;
    public SkillBehaviour currentAttack;

    public enum TurnState
    {
        PROCESSING,
        WAITING,
        ACTION,
        DEAD,
    }

    public TurnState currentState;

    [Header("Boss UI")]
    [SerializeField] private Slider bossHPSlider;
    [SerializeField] private Text bossNameText;
    public GameObject choose; // Vòng sáng dưới chân (nếu có)

    [Header("Action Data")]
    public HandleTurn savedAttack;
    public Animator anim;
    public GameObject playerToAttack;
    public bool actionStarted = false;
    public bool enemyAttacked;

    private bool alive = true;

    [Header("Hệ thống Khóa (Lock)")]
    public EnemyUI enemyUI;
    public List<LockSystem> activeLocks = new List<LockSystem>();
    public int maxAttackType = 2;
    public int timer;
    public bool isLockBrokenOnce = false;

    private Vector3 initialPosition;

    protected override void Awake()
    {
        base.Awake();
        this.combatStateMachine = GameObject.Find("CombatManager").GetComponent<CombatStateMachine>();
        this.anim = this.transform.Find("Body").GetComponent<Animator>();
        this.enemyUI = this.transform.GetComponent<EnemyUI>(); // Boss dùng chung EnemyUI để vẽ Icon
    }

    protected override void Start()
    {
        base.Start();
        DOTween.SetTweensCapacity(500, 50);

        // Khởi tạo máu và UI cho Boss
        if (this.bossStats != null)
        {
            this.bossStats.curHP = this.bossStats.baseHP;
            if (this.bossHPSlider != null)
            {
                this.bossHPSlider.maxValue = this.bossStats.baseHP;
                this.bossHPSlider.value = this.bossStats.curHP;
            }
        }

        if (this.bossNameText != null) this.bossNameText.text = this.bossName;

        this.currentState = TurnState.PROCESSING;
        if (this.choose != null) this.choose.SetActive(false);
    }

    public void StartCombatFlow()
    {
        this.initialPosition = this.transform.position;
        this.ChooseAction();
        this.currentState = TurnState.WAITING;
        Debug.Log($"[Turn-Base] Boss {bossName} đã sẵn sàng vào vị trí.");
    }

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
        if (this.anim != null && this.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") == false)
        {
            this.anim.Play("Idle");
        }

        if (!this.combatStateMachine.heroTurn)
        {
            this.combatStateMachine.enemyTurn = true;
        }

        if (this.combatStateMachine.enemyTurn && this.timer <= 0 && !this.enemyAttacked &&
             !this.combatStateMachine.enemiesAttacked.Contains(this.gameObject) &&
                this.currentState != TurnState.DEAD && this.alive)
        {
            this.enemyAttacked = true;
            this.combatStateMachine.CollectAction(savedAttack);
        }
    }

    // ==========================================
    // NHẬN SÁT THƯƠNG
    // ==========================================
    public void TakeDamage(int damage)
    {
        if (bossStats == null || !alive) return;

        bossStats.curHP -= damage;

        Transform body = transform.Find("Body");
        SpriteRenderer sr = body != null ? body.GetComponent<SpriteRenderer>() : null;

        // Hiệu ứng Visual
        if (sr != null)
        {
            sr.DOComplete();
            sr.DOColor(Color.red, 0.1f).OnComplete(() => sr.DOColor(Color.white, 0.1f));
        }

        if (body != null)
        {
            body.DOComplete();
            body.DOShakePosition(0.3f, 0.5f, 20);
        }

        // Cập nhật thanh máu bự của Boss
        if (bossHPSlider != null)
        {
            bossHPSlider.DOComplete();
            bossHPSlider.DOValue(bossStats.curHP, 0.3f);
        }

        if (bossStats.curHP <= 0 && this.alive)
        {
            this.CheckAlive();
        }
    }

    // ==========================================
    // CƠ CHẾ NGẮT CHIÊU (INTERRUPT / TIMER)
    // ==========================================
    public void OnHeroPerformedAction()
    {
        if (this.currentState == TurnState.WAITING && this.timer > 0)
        {
            this.timer--;
            this.GenerateTimerIcon();
        }
    }

    public void OnAllLocksBroken()
    {
        Debug.Log($"<color=cyan>[Interrupt] Boss {bossName} bị phá sạch Lock! Hủy chiêu thức.</color>");

        if (this.enemyUI != null)
        {
            StartCoroutine(this.enemyUI.ClearAllAttackTypeIcons());
            StartCoroutine(this.enemyUI.ClearTimerIcon());
        }

        this.activeLocks.Clear();
        this.timer = 1; // Reset về lượt chờ ngắn
        if (this.anim != null) this.anim.Play("Hurt");
        this.transform.DOShakePosition(0.5f, 0.2f);
        this.ChooseAction();
    }


    public void ChooseAction()
    {
        if (this.timer > 0 && this.activeLocks.Count > 0) return;

        this.savedAttack = new HandleTurn();
        HandleTurn myAttack = new HandleTurn();

        // Đảm bảo Attacker khớp với tên GameObject để CBM tìm ra Boss
        myAttack.Attacker = this.gameObject.name;
        myAttack.Type = "Enemy";
        myAttack.AttacksGameObject = this.gameObject;

        if (this.combatStateMachine.playersInCombat.Count > 0)
            myAttack.AttackerTarget = this.combatStateMachine.playersInCombat[Random.Range(0, this.combatStateMachine.playersInCombat.Count)];
        else
        {
            GameObject[] heroes = GameObject.FindGameObjectsWithTag("Player");
            if (heroes.Length > 0) myAttack.AttackerTarget = heroes[Random.Range(0, heroes.Length)];
        }

        // Ưu tiên dùng skill từ ScriptableObject
        float chance = Random.value;
        if (chance < 0.4f && this.specialAttacks != null && this.specialAttacks.Count > 0)
            myAttack.choosenAttack = this.specialAttacks[Random.Range(0, this.specialAttacks.Count)];
        else if (this.normalAttacks != null && this.normalAttacks.Count > 0)
            myAttack.choosenAttack = this.normalAttacks[Random.Range(0, this.normalAttacks.Count)];

        if (myAttack.choosenAttack != null)
        {
            this.currentAttack = GetSkillBehaviourForAttack(myAttack.choosenAttack);

            if (myAttack.choosenAttack.attackType == BaseAttack.AttackType.SpecialAttack)
            {
                this.timer = Random.Range(3, 5);
                this.GenerateLocks();
            }
            else
            {
                this.timer = 1;
                if (this.enemyUI != null) StartCoroutine(this.enemyUI.ClearAllAttackTypeIcons());
            }

            this.GenerateTimerIcon();
            this.savedAttack = myAttack;
        }
    }

    private IEnumerator TimeForAction()
    {
        if (this.actionStarted || !this.enemyAttacked || this.isLockBrokenOnce) yield break;

        if (savedAttack.choosenAttack != null && savedAttack.choosenAttack.attackType == BaseAttack.AttackType.SpecialAttack && this.activeLocks.Count == 0)
        {
            this.CheckCombatState();
            yield break;
        }

        this.actionStarted = true;

        if (this.currentAttack != null)
        {
            yield return StartCoroutine(this.currentAttack.Activate(this.gameObject, this.playerToAttack));
        }

        if (this.enemyUI != null)
        {
            StartCoroutine(this.enemyUI.ClearAllAttackTypeIcons());
            StartCoroutine(this.enemyUI.ClearTimerIcon());
        }
        this.activeLocks.Clear();

        this.combatStateMachine.enemiesAttacked.Add(this.gameObject);
        StartCoroutine(MoveTowardsStart());
    }

    private void CheckAlive()
    {
        if (!this.alive) return;
        this.alive = false;
        this.combatStateMachine.enemiesInCombat.Remove(this.gameObject);

        if (this.enemyUI != null)
        {
            StartCoroutine(this.enemyUI.ClearAllAttackTypeIcons());
            StartCoroutine(this.enemyUI.ClearTimerIcon());
        }

        // Ẩn thanh máu Boss khi chết
        if (this.bossHPSlider != null) this.bossHPSlider.gameObject.SetActive(false);
        if (this.bossNameText != null) this.bossNameText.gameObject.SetActive(false);

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        if (this.anim != null) this.anim.Play("Dead");

        Transform sr = transform.Find("Body");
        if (sr != null) sr.GetComponent<SpriteRenderer>().DOFade(0, 1.5f);

        StartCoroutine(this.DestroyObject());
        this.combatStateMachine.combatState = CombatStateMachine.PerformAction.CHECKALIVE;
    }

    IEnumerator MoveTowardsStart()
    {
        yield return new WaitForSeconds(0.6f);
        Transform body = this.transform.Find("Body");
        if (body != null) body.DOMove(initialPosition, 0.5f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.6f);
        this.CheckCombatState();
    }

    public void CheckCombatState()
    {
        if (this.combatStateMachine.performList.Count > 0 && this.combatStateMachine.performList[0].AttacksGameObject == this.gameObject)
        {
            this.combatStateMachine.performList.RemoveAt(0);
        }
        this.combatStateMachine.combatState = CombatStateMachine.PerformAction.WAIT;
        this.actionStarted = false;
        this.enemyAttacked = false;
        this.isLockBrokenOnce = false;

        this.currentState = TurnState.WAITING;
        this.ChooseAction();
    }

    public void GenerateLocks()
    {
        this.activeLocks.Clear();

        List<BaseAttack.Effect> heroEffects = new List<BaseAttack.Effect>();

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

    public void GenerateTimerIcon()
    {
        if (this.enemyUI != null)
        {
            StartCoroutine(this.enemyUI.ClearTimerIcon());
            this.enemyUI.SetTimerIcon(this.timer);
        }
    }

    public SkillBehaviour GetSkillBehaviourForAttack(BaseAttack baseAttack)
    {
        if (baseAttack == null) return null;

        SkillBehaviour existingSkill = GetComponentsInChildren<SkillBehaviour>()
            .FirstOrDefault(s => s.skillData.attackName == baseAttack.attackName);
        if (existingSkill != null) return existingSkill;

        string skillPrefabPath = "Skills/" + baseAttack.attackName.Replace(" ", "");
        GameObject skillPrefab = Resources.Load<GameObject>(skillPrefabPath);
        if (skillPrefab != null)
        {
            Transform skillsParent = this.transform.Find("Skills");
            if (skillsParent == null)
            {
                GameObject skillsObj = new GameObject("Skills");
                skillsObj.transform.SetParent(this.transform);
                skillsParent = skillsObj.transform;
            }
            GameObject skillObj = Instantiate(skillPrefab, skillsParent);
            SkillBehaviour newSkill = skillObj.GetComponent<SkillBehaviour>();
            if (newSkill != null) newSkill.skillData = baseAttack;
            return newSkill;
        }
        return null;
    }

    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(this.gameObject);
    }
}