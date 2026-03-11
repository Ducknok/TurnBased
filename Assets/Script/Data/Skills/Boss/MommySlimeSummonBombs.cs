using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MommySlimeSummonBombs : SkillBehaviour
{
    [Header("Summon Bomb Settings")]
    public GameObject bombSlimePrefab; // Kéo Prefab c?a con Slime Bom vào ??y
    public int spawnCount = 2;

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        Transform body = attacker.transform.Find("Body");

        // ?ánh d?u Boss vào tr?ng thái ch? (Ch? ??nh ngh?a 1 script BossAI l?u bi?n này)
        var bossAI = attacker.GetComponent<BossStateMachine>();
        if (bossAI != null)
        {
            bossAI.activeBombs = spawnCount;
            bossAI.isWaitingForBombs = true; // B?t c? khóa ho?t ??ng c?a Boss
        }

        // --- Hi?u ?ng r?n ?? (T??ng t? cái c?) ---
        if (body != null)
        {
            body.DOScale(new Vector3(1.3f, 1.3f, 1f), 0.5f).SetEase(Ease.OutBack);
            body.DOShakePosition(0.5f, 0.2f, 20);
        }
        yield return new WaitForSeconds(0.6f);

        // --- Tìm danh sách Hero ?? chia m?c tiêu cho t?ng con Bom ---
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");

        // --- Kh?c ra 2 con Slime Bom ---
        for (int i = 0; i < spawnCount; i++)
        {
            if (body != null) body.DOPunchScale(new Vector3(-0.3f, 0.3f, 0), 0.3f, 10, 1f);

            // Bắt đầu sinh quái
            GameObject babyBomb = Instantiate(bombSlimePrefab, attacker.transform.position, Quaternion.identity);

            // --- SỬA LẠI ĐOẠN TÍNH VỊ TRÍ RỚT XUỐNG ---
            // Con thứ nhất bay lên góc trái, con thứ 2 bay xuống góc trái
            float offsetX = Random.Range(-5f, -7.0f); // Quăng xa ra phía Hero
            float offsetY = (i == 0) ? Random.Range(2f, 3f) : Random.Range(-2f, -3f);

            Vector3 dropPos = attacker.transform.position + new Vector3(offsetX, offsetY, 0);
            babyBomb.transform.DOJump(dropPos, 2f, 1, 0.5f).SetEase(Ease.OutQuad);

            var bombScript = babyBomb.GetComponent<BabySlimeBomb>();
            if (bombScript != null)
            {
                bombScript.Initialize(bossAI);
            }

            yield return new WaitForSeconds(0.2f);
        }

        // Thu ng??i v?
        yield return new WaitForSeconds(0.5f);
        if (body != null) body.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad);
    }
}