using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class EnemyStateMachine : MonoBehaviour
{
    private CombatStateMachine combatStateMachine;
    public BaseEnemy baseEnemy;
    public EnemyMoveToCombat enemyMoveToCombat;
    public enum TurnState
    {
        PROCESSING,
        WAITING,
        ACTION,
        DEAD,
    }

    public TurnState currentState;


    //this gameobject
    private CinemachineImpulseSource impulseSource;
    public GameObject choose;

    public Image enemyHPBarFill;
    public TextMeshProUGUI curHpNumber;
    private float trailDelay;
    //time for action
    public HandleTurn savedAttack;
    public Animator anim;
    public GameObject playerToAttack;
    private float animSpeed = 10f;
    public bool actionStarted = false;
    public bool enemyAttacked;
    //alive
    private bool alive = true;
    [Header("Lock")]
    public EnemyUI enemyUI;
    public List<LockSystem> activeLocks = new List<LockSystem>();
    public int maxAttackType = 2; // Số khóa tối đa
    public int timer;
    private bool isLockBrokenOnce = false; // Biến kiểm tra Lock đã bị phá lần nào chưa
    



    // Start is called before the first frame update
    private void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        this.combatStateMachine = GameObject.Find("StartCombat").GetComponent<CombatStateMachine>();
        this.enemyMoveToCombat = this.transform.GetComponentInChildren<EnemyMoveToCombat>();
        this.impulseSource = this.transform.GetComponent<CinemachineImpulseSource>();
        this.anim = this.transform.Find("Body").GetComponent<Animator>();
        this.enemyUI = this.transform.GetComponent<EnemyUI>();
        this.timer = Random.Range(1, this.combatStateMachine.playersInCombat.Count + 1);
        this.ChooseAction();
        this.currentState = TurnState.WAITING;
        this.choose.SetActive(false);
    }
    private void Awake()
    {
        this.baseEnemy.curHP = this.baseEnemy.baseHP; 
    }
    // Update is called once per frame
    private void Update()
    {
        switch (this.currentState)
        {
            case (TurnState.WAITING):
                this.anim.Play("Idle");
                if (!this.combatStateMachine.heroTurn)
                {
                    this.combatStateMachine.enemyTurn = true; // Bật lượt enemy
                }

                if (this.combatStateMachine.enemyTurn && this.timer == 0 && !this.enemyAttacked &&
                     !this.combatStateMachine.enemiesAttacked.Contains(this.gameObject) &&
                        this.currentState != TurnState.DEAD) // Kiểm tra trạng thái DEAD
                {
                    this.enemyAttacked = true;
                    this.combatStateMachine.CollectAction(savedAttack);
                }
                break;
            case (TurnState.DEAD):
                if (!this.alive)
                {
                    return;
                }
                else
                {
                    //not attackable by heroes
                    this.combatStateMachine.enemiesInCombat.Remove(this.gameObject);
                    //remove all inputs heroattacks
                    if (this.combatStateMachine.enemiesInCombat.Count > 0)
                    {
                        for (int i = 0; i < this.combatStateMachine.performList.Count; i++)
                        {
                            if (this.combatStateMachine.performList[i].AttacksGameObject == this.gameObject)
                            {
                                this.combatStateMachine.performList.Remove(this.combatStateMachine.performList[i]);
                            }
                        }
                        for (int i = 0; i < this.combatStateMachine.enemiesAttacked.Count; i++)
                        {
                            if (this.combatStateMachine.enemiesAttacked[i].gameObject == this.gameObject)
                            {
                                this.combatStateMachine.enemiesAttacked.Remove(this.combatStateMachine.enemiesAttacked[i]);
                            }
                        }
                    }

                    this.anim.Play("Dead");
                    //set alive false
                    this.alive = false;
                    StartCoroutine(this.ClearEnemyInfo());
                    //TODO: destroy object after 3s and effect after dead
                    StartCoroutine(this.DestroyObject());
                    //check alive
                    this.combatStateMachine.combatState = CombatStateMachine.PerformAction.CHECKALIVE;
                }
                break;
            case (TurnState.ACTION):
                StartCoroutine(this.TimeForAction());
                break;
        }
    }

    //Choose enemy
    public void ChooseAction()
    {
        this.savedAttack = new HandleTurn();
        if (this.savedAttack.Attacker != null) return;
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = this.baseEnemy.theName;
        myAttack.Type = "Enemy";
        myAttack.AttacksGameObject = this.gameObject;
        myAttack.AttackerTarget = this.combatStateMachine.playersInCombat[Random.Range(0, combatStateMachine.playersInCombat.Count)];
        int num = Random.Range(0, this.baseEnemy.attacks.Count);
        myAttack.choosenAttack = this.baseEnemy.attacks[num];
        //Debug.LogWarning(myAttack.choosenAttack);
        //Debug.LogWarning(BaseAttack.AttackType.SpecialAttack);
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

    //Attack
    private IEnumerator TimeForAction()
    {
        if (this.actionStarted || !this.enemyAttacked)
        {
            yield break;
        }
        this.actionStarted = true;

        //animate the enemy near the player to attack
        Vector3 playerPosition = new Vector3(this.playerToAttack.transform.Find("Body").position.x + 1f, this.playerToAttack.transform.Find("Body").position.y, this.playerToAttack.transform.Find("Body").position.z);
        //Debug.LogWarning(playerPosition);
        while (MoveTowardsPlayer(playerPosition))
        {
            yield return null;
        }


        //wait abit
        yield return new WaitForSeconds(0.5f);
        //animate back to start position
        this.anim.Play("Attack1");
        this.combatStateMachine.enemiesAttacked.Add(this.gameObject);
        this.StartCoroutine(MoveTowardsStart());
        

    }

    //Move enemy toward player
    private bool MoveTowardsPlayer(Vector3 target)
    {
        return target != (this.transform.position = Vector3.MoveTowards(this.transform.position, target, this.animSpeed * Time.deltaTime));
    }

    //Go back to start point
    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (this.transform.position = Vector3.MoveTowards(this.transform.position, target, animSpeed * Time.deltaTime));
    }

    //Damage for player
    public void DoDamage()
    {
        float calDamage = this.baseEnemy.curATK + this.combatStateMachine.performList[0].choosenAttack.attackDamage;
        this.playerToAttack.GetComponent<HeroStateMachine>().TakeDamage(calDamage);
    }


    //Take damage and -hp
    public void TakeDamage(float getDamageAmount, BaseAttack.Effect attackType1, BaseAttack.Effect attackType2)
    {
        CameraShakeManager.instance.CameraShake(impulseSource);

        // Kiểm tra nếu enemy có Lock
        bool hasLocks = activeLocks.Count > 0;
        bool allLocksBroken = false;

        if (hasLocks)
        {
            // Kiểm tra tất cả Lock hiện có
            foreach (var lockSystem in activeLocks)
            {
                lockSystem.TryBreakLock(attackType1, attackType2);
                this.enemyUI.GrayOutAttackType(attackType1, attackType2);
            }

            // Nếu tất cả Lock bị phá, tăng sát thương
            allLocksBroken = activeLocks.TrueForAll(lockSystem => lockSystem.IsBroken());
        }

        if (hasLocks && allLocksBroken && !this.isLockBrokenOnce)
        {
            getDamageAmount *= 1.5f; // Tăng 50% sát thương khi Lock bị phá
            this.isLockBrokenOnce = true;
            StartCoroutine(enemyUI.ClearAllAttackTypeIcons());
            // Debug.Log("🔥 Lock bị phá! Gây thêm sát thương!");
        }

        // Xử lý chí mạng
        bool isCritical = Random.Range(0, 100) < 20;
        if (isCritical) getDamageAmount *= 2;

        // Hiển thị popup sát thương
        DamagePopup.Create(this.transform.Find("Body").position, getDamageAmount, isCritical, false);
        this.baseEnemy.curHP -= getDamageAmount;
        this.curHpNumber.text = this.baseEnemy.curHP.ToString();

        // Cập nhật thanh máu
        float ratio = this.baseEnemy.curHP / this.baseEnemy.baseHP;
        if (this.enemyHPBarFill != null)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.enemyHPBarFill.DOFillAmount(ratio, 0.25f).SetEase(Ease.InOutSine));
            sequence.AppendInterval(this.trailDelay);
            sequence.Play();
        }

        StartCoroutine(this.ClearEnemyInfo());

        // Kiểm tra chết
        if (this.baseEnemy.curHP <= 0)
        {
            this.baseEnemy.curHP = 0;
            this.curHpNumber.text = this.baseEnemy.curHP.ToString();
            StartCoroutine(this.DeadSequence());
        }
    }

    IEnumerator DeadSequence()
    {
        yield return new WaitForSeconds(1f);
        this.currentState = TurnState.DEAD;
    }
    IEnumerator ClearEnemyInfo()
    {
        yield return new WaitForSeconds(0.5f);
        this.combatStateMachine.ClearEnemyInfoPanel();
    }
    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(0.75f);
        Destroy(this.gameObject);
    }
    IEnumerator MoveTowardsStart()
    {
        yield return new WaitForSeconds(0.45f);
        this.anim.Play("Idle");
        Vector3 firstPosition = enemyMoveToCombat.targetPosition.position;
        while (this.MoveTowardsStart(firstPosition)) { yield return null; }

        // Xóa enemy này khỏi danh sách thực hiện hành động
        this.combatStateMachine.performList.RemoveAt(0);
        this.combatStateMachine.combatState = CombatStateMachine.PerformAction.WAIT;

        // Reset trạng thái enemy
        this.actionStarted = false;
        this.enemyAttacked = false;

        // ✅ Kiểm tra nếu enemy chưa chết thì mới đánh dấu đã tấn công
        if (!this.combatStateMachine.enemiesAttacked.Contains(this.gameObject))
        {
            this.combatStateMachine.enemiesAttacked.Add(this.gameObject);
        }


        // ✅ Kiểm tra còn player nào chưa tấn công không?
        List<GameObject> readyPlayers = this.combatStateMachine.playersInCombat
            .FindAll(player => !this.combatStateMachine.heroesDoneTurn.Contains(player));

        if (readyPlayers.Count > 0 && this.timer != 0)
        {
            // Nếu còn player chưa tấn công → Nhường lượt cho player ngay lập tức
            this.combatStateMachine.heroTurn = true;
            this.combatStateMachine.enemyTurn = false;
        }
        else
        {
            if (this.combatStateMachine.AreAllEnemiesDone())
            {
                Debug.LogWarning("Enemy done");
                //Nếu tất cả enemy đã tấn công xong, reset Lock & Timer, rồi chuyển lượt cho player

                this.combatStateMachine.enemiesAttacked.Clear();
                this.combatStateMachine.heroTurn = true;
                this.combatStateMachine.enemyTurn = false;
                foreach (var enemy in this.combatStateMachine.enemiesInCombat)
                {
                   
                    EnemyStateMachine esm = enemy.GetComponent<EnemyStateMachine>();
                    Debug.LogWarning(esm.gameObject);
                    esm.timer = Random.Range(1, this.combatStateMachine.playersInCombat.Count + 1);
                    
                    esm.ChooseAction();
                }
            }
            else
            {
                this.combatStateMachine.heroTurn = true;
                this.combatStateMachine.enemyTurn = false;
            }
        }

        // Reset trạng thái enemy
        this.currentState = TurnState.WAITING;
    }


    //-----------------------------GENERATE-------------------------
    public void GenerateLocks()
    {
        this.activeLocks.Clear();

        for (int i = 0; i < 1; i++)
        {
            int numTypes = Random.Range(2, 3); // Mỗi Lock có 1 hoặc 2 Effect
            List<BaseAttack.Effect> types = new List<BaseAttack.Effect>();

            for (int j = 0; j < 3; j++)
            {
                BaseAttack.Effect randomType = (BaseAttack.Effect)Random.Range(0, System.Enum.GetValues(typeof(BaseAttack.Effect)).Length);
                types.Add(randomType); // Không cần kiểm tra trùng lặp
                
            }
            this.enemyUI.SetAttackTypes(types);
            activeLocks.Add(new LockSystem(types));
        }
    }
    public void GenerateTimerIcon()
    {
        this.enemyUI.SetTimerIcon(this.timer);
    }
    
}
