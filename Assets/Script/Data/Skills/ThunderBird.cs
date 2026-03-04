using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ThunderBird : SkillBehaviour
{
    [Header("ThunderBird Settings")]
    public float flightDuration = 0.6f;

    public float hitRadius = 8.0f;

    public float aoeRadius = 8f;

    public float maxFlightDistance = 12f;

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Transform body = attacker.transform.Find("Body");
        Animator anim = body.GetComponent<Animator>();

        Vector3 startPos = body.position;
        Vector3 spawnPos = startPos + new Vector3(0, 0.5f, 0);

        Vector3 direction = target.transform.position.x >= startPos.x ? Vector3.right : Vector3.left;
        Vector3 endPos = spawnPos + direction * maxFlightDistance;

        if (anim != null) anim.Play(skillData.attackName);

        // Chỉ lấy những quái vật nằm trong bán kính aoeRadius quanh mục tiêu được chọn
        List<GameObject> potentialTargets = GetTargetsInAoE(attacker, target, target.transform.position, aoeRadius);
        if (potentialTargets.Count == 0) potentialTargets.Add(target);

        HashSet<GameObject> hitThisPhase = new HashSet<GameObject>();

        Transform bird = VFXSpawner.Instance.Spawn(VFXSpawner.thunderBird, spawnPos, Quaternion.identity);
        if (bird != null) bird.gameObject.SetActive(true);

        void CheckCollisions()
        {
            if (bird == null) return;

            foreach (var t in potentialTargets)
            {
                if (t == null || !t.activeInHierarchy || hitThisPhase.Contains(t)) continue;

                float distanceX = Mathf.Abs(bird.position.x - t.transform.position.x);

                if (distanceX <= hitRadius)
                {
                    hitThisPhase.Add(t);
                    this.ApplySingleTargetDamage(attacker, t);
                }
            }
        }

        hitThisPhase.Clear();

        float angleOut = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bird.rotation = Quaternion.Euler(0, 0, angleOut);
        bird.localScale = new Vector3(bird.localScale.x, direction.x < 0 ? -Mathf.Abs(bird.localScale.y) : Mathf.Abs(bird.localScale.y), bird.localScale.z);

        Tween flyOut = bird.DOMove(endPos, flightDuration)
                           .SetEase(Ease.OutQuad)
                           .OnUpdate(() => CheckCollisions());

        yield return flyOut.WaitForCompletion();

        yield return new WaitForSeconds(0.1f);
        hitThisPhase.Clear();

        Vector3 backDirection = -direction;
        float angleBack = Mathf.Atan2(backDirection.y, backDirection.x) * Mathf.Rad2Deg;
        bird.rotation = Quaternion.Euler(0, 0, angleBack);
        bird.localScale = new Vector3(bird.localScale.x, backDirection.x < 0 ? -Mathf.Abs(bird.localScale.y) : Mathf.Abs(bird.localScale.y), bird.localScale.z);

        Vector3 returnPos = body.position + new Vector3(0, 0.5f, 0);
        Tween flyBack = bird.DOMove(returnPos, flightDuration)
                            .SetEase(Ease.InQuad)
                            .OnUpdate(() => CheckCollisions());

        yield return flyBack.WaitForCompletion();

        if (bird != null && bird.gameObject.activeInHierarchy)
            VFXSpawner.Instance.Despawn(bird);
    }
}