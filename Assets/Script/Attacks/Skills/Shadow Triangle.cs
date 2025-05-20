using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowTriangle : SkillBehaviour
{
    protected override void ApplySkillEffects(GameObject attacker, GameObject target)
    {
        HeroStateMachine hero = attacker.transform.GetComponent<HeroStateMachine>();
        // T¨ªnh to¨¢n damage d?a tr¨ºn c¨¢c thu?c t¨ªnh c?a hero v¨¤ target
        float damage = skillData.attackDamage * hero.baseHero.baseATK;

        // ?p d?ng damage v¨¤o target
        EnemyStateMachine enemy = target.GetComponent<EnemyStateMachine>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, skillData.effect1, skillData.effect2);
        }

        //// T?o hi?u ?ng va ch?m
        //if (impactEffectPrefab != null)
        //{
        //    Instantiate(impactEffectPrefab, target.transform.position, Quaternion.identity);
        //}
    }

    // B?n c¨® th? ghi ?¨¨ ph??ng th?c Activate ?? th¨ºm logic ??c bi?t
    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hero = attacker.transform.GetComponent<HeroStateMachine>();
        Transform enemy = target.transform.Find("Body");
        Animator anim = hero.transform.Find("Body").GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(hero.currentAttack.skillData.attackName);
        }
        if (enemy == null)
        {
            Debug.LogError("Kh?ng c¨® enemy ?? t?n c?ng.");
            yield break;
        }

        Vector3 enemyPos = enemy.position;
        float radius = 1.5f;
        Vector3[] positions = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            positions[i] = enemyPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        }

        List<Transform> clones = new List<Transform>();

        for (int i = 0; i < positions.Length; i++)
        {
            yield return MoveToPosition(hero.transform, positions[i], 0.25f);

            Transform clone = VFXSpawner.Instance.Spawn(VFXSpawner.ducknokClone, positions[i], Quaternion.identity);
            if (clone != null)
            {
                clone.gameObject.SetActive(true);
                clones.Add(clone);
            }
        }

        // Quay l?i v? tr¨ª ban ??u
        yield return MoveToPosition(hero.transform, hero.transform.position, 0.3f);

        yield return new WaitForSeconds(1f);

        foreach (Transform clone in clones)
        {
            if (clone != null)
                CoroutineHelper.Start(MoveAndDestroy(clone, enemyPos, 0.2f));
        }
    }
    private IEnumerator MoveToPosition(Transform obj, Vector3 target, float duration)
    {
        Vector3 start = obj.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.position = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        obj.position = target;
    }

    private IEnumerator MoveAndDestroy(Transform clone, Vector3 target, float duration)
    {
        Vector3 start = clone.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            clone.position = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        // Add impact VFX if needed
    }
}
