using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabySlimeBomb : EnemyStateMachine
{
    private BossStateMachine motherBoss;
    private bool isResolved = false;

    // Boss chỉ gọi hàm này để truyền reference, không làm gì thêm
    public void Initialize(BossStateMachine boss)
    {
        motherBoss = boss;
    }

    // GHI ĐÈ HÀM START ĐỂ XỬ LÝ VIÊC SINH RA GIỮA TRẬN
    protected override void Start()
    {
        this.baseEnemy = Instantiate(this.baseEnemy);
        // 1. Cho lớp cha (EnemyStateMachine) chạy setup máu, gán state PROCESSING mặc định...
        base.Start();

        // 2. NGAY SAU ĐÓ, vì con Bom này sinh ra giữa trận, ta ép nó vào Combat luôn
        if (this.combatStateMachine != null && !this.combatStateMachine.enemiesInCombat.Contains(this.gameObject))
        {
            this.combatStateMachine.enemiesInCombat.Add(this.gameObject);

            // Hàm này của bạn sẽ bốc thăm chiêu, tự sinh Lock, vẽ UI Icon và đổi State sang WAITING
            this.StartCombatFlow();
        }
    }

    protected override void Update()
    {
        // Giữ lại não bộ của lớp cha
        base.Update();

        // KHÓA AN TOÀN: Nếu máu <= 0 mà chưa chết thì ép phải chết (Chống Cương thi)
        if (this.baseEnemy.curHP <= 0 && this.currentState != TurnState.DEAD)
        {
            this.currentState = TurnState.DEAD;
        }

        if (isResolved) return;

        // KIỂM TRA PHÁ LOCK HOẶC CHẾT
        if (this.isLockBrokenOnce || this.baseEnemy.curHP <= 0)
        {
            isResolved = true;
            this.baseEnemy.curHP = 0; // Đảm bảo HP = 0 để hệ thống biết nó chết thật

            if (motherBoss != null) motherBoss.OnBombDestroyed();

            // Đổi state sang DEAD để EnemyStateMachine tự chạy anim gục ngã và dọn dẹp
            this.currentState = TurnState.DEAD;
        }
    }

    // GHI ĐÈ TimeForAction ĐỂ BOM NỔ XONG LÀ CHẾT, KHÔNG ĐI BỘ VỀ CHỖ CŨ
    protected override IEnumerator TimeForAction()
    {
        if (this.actionStarted) yield break;
        this.actionStarted = true;

        yield return StartCoroutine(this.currentAttack.Activate(this.gameObject, this.playerToAttack));

        this.combatStateMachine.enemiesAttacked.Add(this.gameObject);
        this.RemoveInPerformList();
        this.combatStateMachine.combatState = CombatStateMachine.PerformAction.WAIT;

        // 4. CHẾT NGAY LẬP TỨC (Thay vì gọi MoveTowardsStart như quái thường)
        this.baseEnemy.curHP = 0;
        this.currentState = TurnState.DEAD;
    }

    public void OnExploded()
    {
        if (isResolved) return;
        isResolved = true;

        if (motherBoss != null) motherBoss.OnBombDestroyed();
        motherBoss.CheckCombatState();
        this.currentState = TurnState.DEAD;
    }
}