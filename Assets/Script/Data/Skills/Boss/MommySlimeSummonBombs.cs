using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MommySlimeSummonBombs : SkillBehaviour
{
    [Header("Summon Bomb Settings")]
    public GameObject bombSlimePrefab;
    public int spawnCount = 2;

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        Transform body = attacker.transform.Find("Body");

        var bossAI = attacker.GetComponent<BossStateMachine>();
        if (bossAI != null)
        {
            bossAI.activeBombs = spawnCount;
            bossAI.isWaitingForBombs = true; 
        }

        if (body != null)
        {
            body.DOScale(new Vector3(1.3f, 1.3f, 1f), 0.5f).SetEase(Ease.OutBack);
            body.DOShakePosition(0.5f, 0.2f, 20);
        }
        yield return new WaitForSeconds(0.6f);

        for (int i = 0; i < spawnCount; i++)
        {
            if (body != null) body.DOPunchScale(new Vector3(-0.3f, 0.3f, 0), 0.3f, 10, 1f);

            GameObject babyBomb = Instantiate(bombSlimePrefab, attacker.transform.position, Quaternion.identity);

            float offsetX = Random.Range(-5f, -7.0f); 
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

        yield return new WaitForSeconds(0.5f);
        if (body != null) body.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad);
    }
}