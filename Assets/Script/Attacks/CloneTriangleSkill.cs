using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneTriangleSkill
{
    private readonly HeroStateMachine hsm;
    private readonly Transform hero;

    public CloneTriangleSkill(HeroStateMachine hsm)
    {
        this.hsm = hsm;
        this.hero = hsm.transform;
    }

    public IEnumerator Execute()
    {
        Transform enemy = this.hsm.enemyToAttack.transform.Find("Body");

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
            yield return MoveToPosition(hero, positions[i], 0.25f);

            Transform clone = VFXSpawner.Instance.Spawn(VFXSpawner.ducknokClone, positions[i], Quaternion.identity);
            if (clone != null)
            {
                clone.gameObject.SetActive(true);
                clones.Add(clone);
            }
        }

        // Quay l?i v? tr¨ª ban ??u
        yield return MoveToPosition(hero, hsm.transform.position, 0.3f);

        yield return new WaitForSeconds(1f);

        foreach (Transform clone in clones)
        {
            if (clone != null)
                CoroutineHelper.Start(MoveAndDestroy(clone, enemyPos, 0.2f));
        }

        hsm.DoDamage();
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
