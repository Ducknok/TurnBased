using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;

public class HeroStateMachine : MonoBehaviour
{
    private CombatStateMachine combatStateMachine;
    public CombatZone combatZone;
    public BaseHero baseHero;

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

    //For the Player Bar
    private CinemachineImpulseSource impulseSource;
    public GameObject choose;
    private Image heroHPBarFill;
    private Image heroHPBarTrail;
    private Image heroMPBarFill;
    private Image heroMPBarTrail;
    private float trailDelay = 0.4f;


    //Choose Player for action
    private int selectedHeroIndex = 0; // Hero đang được chọn

    //IENUMERATOR
    public GameObject enemyToAttack;
    public float animSpeed = 15f;
    private int heroIndex; // Chỉ số của hero trong playerPositions
    public bool attackEnd;
    private bool actionStarted;
    //Dead
    public GameObject body;
    public Animator anim;
    private bool alive = true;
    //Hero Panel;
    private HeroPanelStats stats;
    public GameObject playerPanel;
    public Transform heroPanelSpacer;





    // Start is called before the first frame update
    void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        //Find spacer
        this.heroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("Panel").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");
        if (heroPanelSpacer == null)
        {
            Debug.LogError("Không tìm thấy BattleCanvas!");
            return;
        }
        //Create panel, fill in info
        this.CreateHeroPanel();


        this.choose.SetActive(false);
        this.combatStateMachine = GameObject.Find("StartCombat").GetComponent<CombatStateMachine>();
        this.combatZone = GameObject.Find("StartCombat").GetComponent<CombatZone>();
        this.body = this.transform.Find("Body").gameObject;
        this.anim = this.body.GetComponent<Animator>();
        this.impulseSource = this.transform.GetComponent<CinemachineImpulseSource>();

        // Xác định chỉ số của hero trong playerPosition
        for (int i = 0; i < this.combatZone.players.Length; i++)
        {
            if (this.combatZone.players[i] == this.gameObject)
            {
                this.heroIndex = i;
                break;
            }
        }

        if (this.heroIndex == -1)
        {
            Debug.LogError($"Không tìm thấy hero {this.gameObject.name} trong CombatSystem.players.");
        }

        this.currentState = TurnState.PROCESSING;
    }

    private void Awake()
    {
        this.baseHero.curHP = this.baseHero.baseHP;
        
    }
    // Update is called once per frame
    private void Update()
    {

        Debug.Log(this.currentState);
        // Kiểm tra nếu chuột trái được click
        switch (this.currentState)
        {
            case (TurnState.PROCESSING):
                this.anim.SetBool("IdleBattle", true);
                this.HandleHeroSelection();
                break;
            case (TurnState.ADDTOLIST):
                if (!this.combatStateMachine.playerToManage.Contains(this.gameObject))
                {
                    this.combatStateMachine.playerToManage.Add(this.gameObject);
                }
                this.currentState = TurnState.WAITING;
                break;
            case (TurnState.WAITING):

                if (this.combatStateMachine.heroTurn && !this.combatStateMachine.heroesDoneTurn.Contains(this.gameObject) &&  this.combatStateMachine.combatZone.isInCombat) this.currentState = TurnState.PROCESSING;
                break;
            case (TurnState.DEAD):
                if (!this.alive)
                {
                    return;
                }
                else
                {
                    //Change tag
                    this.gameObject.tag = "DeadPlayer";
                    //not attackable by enemy
                    this.combatStateMachine.playersInCombat.Remove(this.gameObject);
                    //not managable
                    this.combatStateMachine.playerToManage.Remove(this.gameObject);

                    //reset gui
                    this.combatStateMachine.actionPanel.SetActive(false);
                    //remove item from performList
                    if (this.combatStateMachine.playersInCombat.Count > 0)
                    {
                        for (int i = 0; i < this.combatStateMachine.performList.Count; i++)
                        {
                            if (this.combatStateMachine.performList[i].AttacksGameObject == this.gameObject)
                            {
                                this.combatStateMachine.performList.Remove(this.combatStateMachine.performList[i]);
                            }

                            if (this.combatStateMachine.performList[i].AttackerTarget == this.gameObject)
                            {
                                this.combatStateMachine.performList[i].AttackerTarget = this.combatStateMachine.playersInCombat[Random.Range(0, this.combatStateMachine.playersInCombat.Count)];
                            }
                        }
                    }
                    //animation dead
                    this.anim.SetTrigger("Dead");
                    // Không tạo action và skill panel nếu hero đã chết
                    this.combatStateMachine.actionPanel.SetActive(false);
                    this.combatStateMachine.skillPanel.SetActive(false);
                    //reset heroinput
                    this.combatStateMachine.combatState = CombatStateMachine.PerformAction.CHECKALIVE;
                    this.alive = false;
                    //deactivate the selector
                    if (this.choose == null) return;
                    this.choose.SetActive(false);
                }
                break;
            case (TurnState.ACTION):
                StartCoroutine(TimeForAction());
                break;
        }

    }

    // Thay đổi cách chọn hero
    private void HandleHeroSelection()
    {
        if (this.combatStateMachine.isSelectingEnemy || this.combatStateMachine.enemyTurn) return;
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
        if (this.combatZone.players.Length == 0) return;

        // Tắt action panel của hero cũ nếu có
        if (selectedHeroIndex >= 0 && selectedHeroIndex < this.combatZone.players.Length)
        {
            var previousHero = this.combatZone.players[selectedHeroIndex].GetComponent<HeroStateMachine>();

            if (previousHero != null)
            {
                this.choose.SetActive(false);
                this.combatStateMachine.playerToManage.Remove(previousHero.gameObject);
                this.combatStateMachine.ClearAttackPanel();
                this.combatStateMachine.playerInput = CombatStateMachine.PlayerGUI.ACTIVATE;
            }
        }

        // Tìm hero mới không chết
        int newIndex = selectedHeroIndex;
        do
        {
            newIndex += direction;

            // Quay vòng nếu đi quá giới hạn
            if (newIndex < 0) newIndex = this.combatZone.players.Length - 1;
            if (newIndex >= this.combatZone.players.Length) newIndex = 0;

            // Nếu hero mới không chết, thì chọn nó
            if (this.combatZone.players[newIndex].GetComponent<HeroStateMachine>().currentState != TurnState.DEAD)
            {
                selectedHeroIndex = newIndex;
                break;
            }

        } while (newIndex != selectedHeroIndex); // Nếu quay lại hero cũ thì dừng lại (tất cả hero đã chết)

        // Thêm hero mới vào danh sách
        var selectedHero = this.combatZone.players[selectedHeroIndex].GetComponent<HeroStateMachine>();
        if (!this.combatStateMachine.playerToManage.Contains(selectedHero.gameObject))
        {
            this.combatStateMachine.playerToManage.Add(selectedHero.gameObject);
        }
        // Cập nhật vị trí actionPanel & skillPanel
        this.UpdateActionPanelPosition(selectedHero);
    }

    //Update action position
    private void UpdateActionPanelPosition(HeroStateMachine selectedHero)
    {
        RectTransform actionPanelRect = this.combatStateMachine.actionPanel.GetComponent<RectTransform>();
        RectTransform skillPanelRect = this.combatStateMachine.skillPanel.GetComponent<RectTransform>();

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
        this.combatStateMachine.actionPanel.SetActive(true);
        this.combatStateMachine.skillPanel.SetActive(false);
    }

    private IEnumerator TimeForAction()
    {
        if (this.actionStarted)
        {
            yield break;
        }
        this.actionStarted = true;

        //animate the enemy near the player to attack
        Vector3 enemyPosition = new Vector3(this.enemyToAttack.transform.Find("Body").position.x - 1f, this.enemyToAttack.transform.Find("Body").position.y, this.enemyToAttack.transform.Find("Body").position.z);
        //Debug.LogWarning( enemyPosition);
        while (MoveTowardsEnemy(enemyPosition))
        {
            yield return null;
        }


        //wait abit
        yield return new WaitForSeconds(0.5f);

        //animate back to start position
        this.anim.SetBool("IdleBattle", false);
        this.anim.SetTrigger("Attack_1");
        this.combatStateMachine.UpdateEnemyTimer();
        
        StartCoroutine(MoveTowardsStart());
        

        
    }
    private bool MoveTowardsEnemy(Vector3 target)
    {
        Transform body = this.transform.Find("Body");
        if (body == null) return false;

        body.DOMove(target, 0.5f).SetEase(Ease.Linear);
        return Vector3.Distance(body.position, target) > 0.1f;
        //return target != (this.transform.Find("Body").position = Vector3.MoveTowards(this.transform.Find("Body").position, target, this.animSpeed * Time.deltaTime));
    }
    private bool MoveTowardsStart(Vector3 target)
    {
        Transform body = this.transform.Find("Body");
        if (body == null) return false;

        body.DOMove(target, 0.5f).SetEase(Ease.Linear);
        return Vector3.Distance(body.position, target) > 0.1f;
        //return target != (this.transform.Find("Body").position = Vector3.MoveTowards(this.transform.Find("Body").position, target, animSpeed * Time.deltaTime));
    }
    public void TakeDamage(float getDamageAmount)
    {
        
        bool isCritical = Random.Range(0, 100) < 5;
        if (isCritical) getDamageAmount *= 2;
        DamagePopup.Create(this.transform.Find("Body").position, getDamageAmount, isCritical, false);
        this.baseHero.curHP -= getDamageAmount;
        CameraShakeManager.instance.CameraShake(this.impulseSource);
        float ratio = this.baseHero.curHP / this.baseHero.baseHP;

        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.heroHPBarFill.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
        sequence.AppendInterval(this.trailDelay);
        sequence.Append(this.heroHPBarTrail.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
        sequence.Play();

        if (this.baseHero.curHP <= 0)
        {
            this.baseHero.curHP = 0;
            this.currentState = TurnState.DEAD;
        }
        this.UpdateHeroPanel();
    }
    //Do damage
    public void DoDamage()
    {
        float calDamage = this.baseHero.curATK + combatStateMachine.performList[0].choosenAttack.attackDamage;
        if (this.combatStateMachine.performList.Count > 0)
        {
            this.ManaBar();
            this.enemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calDamage, this.combatStateMachine.performList[0].choosenAttack.effect1, this.combatStateMachine.performList[0].choosenAttack.effect2);
            this.UpdateHeroPanel();
        }
        else
        {
            return;
        }
    }
    protected void ManaBar()
    {
        if (this.combatStateMachine.performList[0].choosenAttack.attackType == BaseAttack.AttackType.NormalAttack)
        {
            this.RestoreMana();
        }
        else
        {
            this.DescreaseMana();
        }
        
        if (this.baseHero.curMP <= 0)
        {
            this.baseHero.curMP = 0;
            //Debug.Log("Het mana");
        }
    }
    protected void RestoreMana()
    {
        // Tăng mana hiện tại
        this.baseHero.curMP += 3f;

        // Đảm bảo mana không vượt quá giá trị tối đa
        if (this.baseHero.curMP > this.baseHero.baseMP)
        {
            this.baseHero.curMP = this.baseHero.baseMP;
        }

        // Tính tỷ lệ mana để cập nhật thanh UI
        float ratio = this.baseHero.curMP / this.baseHero.baseMP;

        // Tạo Sequence để làm animation
        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.heroMPBarFill.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
        sequence.AppendInterval(this.trailDelay);
        sequence.Append(this.heroMPBarTrail.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
        sequence.Play();
    }
    protected void DescreaseMana()
    {
        this.baseHero.curMP -= this.combatStateMachine.performList[0].choosenAttack.attackCost;
        float ratio = this.baseHero.curMP / this.baseHero.baseMP;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.heroMPBarFill.DOFillAmount(ratio, 0.25f)).SetEase(Ease.InOutSine);
        sequence.AppendInterval(this.trailDelay);
        sequence.Append(this.heroMPBarTrail.DOFillAmount(ratio, 0.3f)).SetEase(Ease.InOutSine);
        sequence.Play();
    }
    //Create a player panel
    public void CreateHeroPanel()
    {
        this.playerPanel = Instantiate(this.playerPanel) as GameObject;
        this.stats = this.playerPanel.GetComponent<HeroPanelStats>();
        this.InitializeStatHero();
        
    }
    public void InitializeStatHero()
    {
        this.stats.heroName.text = this.baseHero.theName;
        this.stats.heroHP.text = this.baseHero.curHP.ToString();
        this.stats.heroMP.text = this.baseHero.curMP.ToString();
        this.heroHPBarFill = this.stats.hpBarFill;
        this.heroHPBarTrail = this.stats.hpBarTrail;
        this.heroHPBarFill.fillAmount = 1f;
        this.heroHPBarTrail.fillAmount = 1f;
        this.heroMPBarFill = this.stats.hpBarFill;
        this.heroMPBarTrail = this.stats.hpBarTrail;
        this.heroMPBarFill.fillAmount = 1f;
        this.heroMPBarTrail.fillAmount = 1f;
        this.heroMPBarFill = this.stats.mpBarFill;
        this.heroMPBarTrail = this.stats.mpBarTrail;
        this.playerPanel.transform.SetParent(this.heroPanelSpacer, false);
    }
    //Update stats hp, mp, heal
    public void UpdateHeroPanel()
    {
        this.stats.heroHP.text = this.baseHero.curHP.ToString();
        this.stats.heroMP.text = this.baseHero.curMP.ToString();
    }

    IEnumerator MoveTowardsStart()
    {
        yield return new WaitForSeconds(0.25f);
        if (this.combatStateMachine.performList[0].choosenAttack.attackType == BaseAttack.AttackType.NormalAttack)
        {
            DamagePopup.Create(this.body.transform.position, 3f, false, true);
        }
        this.anim.SetBool("IdleBattle", true);
        yield return new WaitForSeconds(0.5f);
        //do damage
        this.DoDamage();
        Vector3 firstPosition = this.combatZone.playerPositions[this.heroIndex].position;
        while (this.MoveTowardsStart(firstPosition)) { yield return null; }
        //remvoe this performer from the list in CSM
        this.combatStateMachine.performList.RemoveAt(0);

        
        //reset CSM  -> WAIT
        if (this.combatStateMachine.combatState != CombatStateMachine.PerformAction.WIN && this.combatStateMachine.combatState != CombatStateMachine.PerformAction.LOSE)
        {
            this.combatStateMachine.combatState = CombatStateMachine.PerformAction.WAIT;
            //reset this player state
            this.currentState = TurnState.WAITING;
        }
        if (this.combatStateMachine.AreAllHeroesDone())
        {
            this.combatStateMachine.heroesDoneTurn.Clear();

            Debug.LogWarning("hero done");
            if (this.combatStateMachine.AreAllEnemiesDone())
            {
                this.combatStateMachine.heroesDoneTurn.Clear();
                this.combatStateMachine.heroTurn = true;
                this.combatStateMachine.enemyTurn = false;
            }
            
        }
        this.actionStarted = false;
    }
}
