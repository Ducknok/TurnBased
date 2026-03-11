using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Linq;

public class BossStateMachine : EnemyStateMachine
{
    [Header("Bomb Mechanic")]
    public int activeBombs = 0;
    public bool isWaitingForBombs = false;

    // Con bom gọi hàm này khi nó nổ hoặc bị hero đánh chết
    public void OnBombDestroyed()
    {
        activeBombs--;
        if (activeBombs <= 0)
        {
            isWaitingForBombs = false; // Mở khóa cho Boss đánh tiếp
            Debug.Log("Boss đã hết chờ, quay lại chiến đấu!");
        }
    }
    public override void GenerateLocks()
    {
        if (isWaitingForBombs) return; // Đang đợi bom thì không làm gì cả

        base.GenerateLocks(); // Ngược lại thì gọi logic gốc của lớp cha
    }

    public override void GenerateTimerIcon()
    {
        if (isWaitingForBombs) return; // Đang đợi bom thì không hiện số

        base.GenerateTimerIcon(); // Ngược lại thì gọi logic gốc của lớp cha
    }
    // ==========================================

    protected override IEnumerator TimeForAction()
    {
        if (this.actionStarted || !this.enemyAttacked || this.isLockBrokenOnce)
        {
            yield break;
        }
        this.actionStarted = true;

        if (this.isWaitingForBombs)
        {
            Debug.Log("Boss đang ngồi nhìn đống bom... Bỏ qua lượt đánh!");
            this.transform.DOPunchScale(new Vector3(0.1f, -0.1f, 0), 0.5f, 2);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return StartCoroutine(this.currentAttack.Activate(this.gameObject, this.playerToAttack));
        }

        this.combatStateMachine.enemiesAttacked.Add(this.gameObject);
        StartCoroutine(MoveTowardsStart());
    }

}
