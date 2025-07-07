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

    // Start is called before the first frame update
    protected override void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        this.combatStateMachine = GameObject.Find("CombatManager").GetComponent<CombatStateMachine>();
        this.enemyMoveToCombat = this.transform.GetComponentInChildren<EnemyMoveToCombat>();
        this.anim = this.transform.Find("Body").GetComponent<Animator>();
        this.enemyUI = this.transform.GetComponent<EnemyUI>();
        this.timer = Random.Range(1, this.combatStateMachine.playersInCombat.Count + 1);
        this.initialPosition = enemyMoveToCombat.targetPosition.transform.position; // Store initial position
        this.ChooseAction();
        this.currentState = TurnState.WAITING;
        this.choose.SetActive(false);
    }

    protected override void Awake()
    {
        this.baseEnemy.curHP = this.baseEnemy.baseHP;
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
        if (this.combatStateMachine.enemyTurn && this.timer == 0 && !this.enemyAttacked &&
             !this.combatStateMachine.enemiesAttacked.Contains(this.gameObject) &&
                this.currentState != TurnState.DEAD)
        {
            this.enemyAttacked = true;
            this.combatStateMachine.CollectAction(savedAttack);
            ///this.currentState = TurnState.ACTION;
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
            // Not attackable by heroes
            this.combatStateMachine.enemiesInCombat.Remove(this.gameObject);
            // Remove all inputs hero attacks
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
            // Set alive false
            this.alive = false;
            StartCoroutine(this.ClearEnemyInfo());
            // Destroy object after 3s and effect after dead
            StartCoroutine(this.DestroyObject());
            // Check alive
            this.combatStateMachine.combatState = CombatStateMachine.PerformAction.CHECKALIVE;
        }
    }
    // Choose enemy
    public void ChooseAction()
    {
        this.savedAttack = new HandleTurn();
        if (this.savedAttack.Attacker != null) return;
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = this.baseEnemy.theName;
        myAttack.Type = "Enemy";
        myAttack.AttacksGameObject = this.gameObject;
        myAttack.AttackerTarget = this.combatStateMachine.playersInCombat[Random.Range(0, combatStateMachine.playersInCombat.Count)];
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
    private IEnumerator TimeForAction()
    {
        if (this.actionStarted || !this.enemyAttacked || this.isLockBrokenOnce)
        {
            yield break;
        }
        this.actionStarted = true;
        StartCoroutine(this.currentAttack.Activate(this.gameObject, this.playerToAttack));
        yield return new WaitForSeconds(1f);
        this.combatStateMachine.enemiesAttacked.Add(this.gameObject);
        StartCoroutine(MoveTowardsStart());
    }

    public IEnumerator ClearEnemyInfo()
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
        //Debug.Log($"MoveTowardsStart started for {gameObject.name}");

        // Wait for attack animation to finish
        yield return new WaitForSeconds(0.6f);
        // Play Idle animation
        if (this.anim != null && !this.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            this.anim.Play("Idle");
            //Debug.Log($"Playing Idle animation for {gameObject.name}");
        }

        // Ensure no conflicting tweens
        this.transform.DOKill();

        // Move back to initial position
        //Debug.Log($"Moving {gameObject.name} to initial position: {initialPosition}");
        this.transform.Find("Body").DOMove(initialPosition, 0.5f)
            .SetEase(Ease.OutQuad);
            //.OnComplete(() => Debug.Log($"Move completed for {gameObject.name} at position: {this.transform.position}"));

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
    public void GenerateLocks()
    {
        this.activeLocks.Clear();
        for (int i = 0; i < 1; i++)
        {
            int numTypes = 3; // Each Lock has 1 or 2 Effects
            List<BaseAttack.Effect> types = new List<BaseAttack.Effect>();

            for (int j = 0; j < numTypes; j++)
            {
                BaseAttack.Effect randomType = (BaseAttack.Effect)Random.Range(0, System.Enum.GetValues(typeof(BaseAttack.Effect)).Length);
                types.Add(randomType);
            }
            this.enemyUI.SetAttackTypes(types);
            activeLocks.Add(new LockSystem(types));
        }
    }
    public void GenerateTimerIcon()
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