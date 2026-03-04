using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MagneticThunderstorm : SkillBehaviour
{
    [Header("Magnetic Settings")]
    public float aoeRadius = 8f;
    public float pullDuration = 0.6f;

    [Header("VFX Settings")]
    public float nukeOffsetY = 5f;

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Transform body = attacker.transform.Find("Body");
        Animator anim = body.GetComponent<Animator>();

        List<GameObject> targetList = GetTargetsInAoE(attacker, target, target.transform.position, aoeRadius);
        if (targetList.Count == 0) targetList.Add(target); 

        Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
        Vector3 centerPoint = Vector3.zero;

        foreach (var t in targetList)
        {
            if (t != null)
            {
                originalPositions.Add(t, t.transform.position);
                centerPoint += t.transform.position;
            }
        }

        centerPoint /= targetList.Count;

        if (anim != null) anim.Play(skillData.attackName);

        Vector3 startPos = body.position;
        yield return body.DOMove(startPos + new Vector3(-0.5f, 0.5f, 0), 0.4f).SetEase(Ease.OutQuad).WaitForCompletion();


        Transform orb = VFXSpawner.Instance.Spawn(VFXSpawner.magneticOrb, centerPoint + Vector3.up, Quaternion.identity);
        if (orb != null) orb.gameObject.SetActive(true);

        foreach (var t in targetList)
        {
            if (t != null && t.activeInHierarchy)
            {
                t.transform.DOKill();


                Vector3 pullPos = Vector3.Lerp(originalPositions[t], centerPoint, 0.8f);
                t.transform.DOMove(pullPos, pullDuration).SetEase(Ease.InBack);

            }
        }

        yield return new WaitForSeconds(pullDuration + 0.1f);

        Vector3 nukeSpawnPos = centerPoint + new Vector3(0, nukeOffsetY, 0);
        Transform nuke = VFXSpawner.Instance.Spawn(VFXSpawner.thunderNuke, nukeSpawnPos, Quaternion.identity);

        if (nuke != null)
        {
            nuke.gameObject.SetActive(true);
            DOVirtual.DelayedCall(1.0f, () => VFXSpawner.Instance.Despawn(nuke));
        }

        if (orb != null) VFXSpawner.Instance.Despawn(orb);

        foreach (var t in targetList)
        {
            if (t != null && t.activeInHierarchy)
            {
                this.ApplySingleTargetDamage(attacker, t);

                t.transform.DOKill();

                if (originalPositions.ContainsKey(t))
                {
                    t.transform.DOMove(originalPositions[t], 0.3f).SetEase(Ease.OutExpo);
                }


            }
        }

        yield return new WaitForSeconds(0.5f);

        yield return body.DOMove(startPos, 0.3f).SetEase(Ease.InOutQuad).WaitForCompletion();
    }
}