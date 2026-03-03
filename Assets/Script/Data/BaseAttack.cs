using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "RPG/Skills/New Skill")]
[Serializable]
public class BaseAttack : ScriptableObject
{
    public enum Effect { Sword, Lance, Wind, Lightning }
    public enum AttackType { NormalAttack, SpecialAttack }
    public Sprite attackImage;
    public string attackName;
    public string attackDescription;
    public float attackDamage;
    public float attackCost;
    public int maxEnemyCount;
    public AttackType attackType;
    public Effect effect1;
    public Effect effect2;
    public string hitParticleName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying || string.IsNullOrWhiteSpace(attackName)) return;
        string assetPath = AssetDatabase.GetAssetPath(this.GetInstanceID());
        if (string.IsNullOrEmpty(assetPath)) return;
        string currentFileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (currentFileName != attackName)
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                AssetDatabase.RenameAsset(assetPath, attackName);
                AssetDatabase.SaveAssets();
            };
        }
    }
#endif
}

public abstract class SkillBehaviour : DucMonobehaviour
{
    public BaseAttack skillData;

    public virtual IEnumerator Activate(GameObject attacker, GameObject target)
    {
        yield return new WaitForSeconds(0f);
    }

    protected virtual void ApplySkillEffects(GameObject attacker, GameObject target)
    {
        if (target != null)
        {
            ApplySingleTargetDamage(attacker, target);
        }
    }


    protected virtual void ApplyDamageToTarget(GameObject attacker, GameObject target, float calDamage)
    {
        Transform body = target.transform.Find("Body");
        Vector3 targetPosition = body != null ? body.position : target.transform.position;

        if (!string.IsNullOrEmpty(skillData.hitParticleName))
        {
            Transform hitParticle = VFXSpawner.Instance.Spawn(skillData.hitParticleName, targetPosition, Quaternion.identity);
            if (hitParticle != null) hitParticle.gameObject.SetActive(true);
        }

        target.transform.DOShakePosition(0.2f, 0.15f, 10);

        EnemyStateMachine esm = target.GetComponent<EnemyStateMachine>();
        HeroStateMachine hsm = target.GetComponent<HeroStateMachine>();

        if (esm != null) // Mục tiêu là QUÁI
        {
            EnemyTakeDamage takeDamageComp = esm.GetComponent<EnemyTakeDamage>();
            if (takeDamageComp != null)
            {
                takeDamageComp.TakeDamage(esm.gameObject, calDamage, skillData.effect1, skillData.effect2);
            }
        }
        else if (hsm != null)
        {
            hsm.GetComponent<HeroTakeDamage>().TakeDamage(hsm.gameObject, calDamage);

            Debug.Log($"<color=red>{attacker.name} vừa gây {calDamage} sát thương lên {target.name}!</color>");
        }
    }


    protected virtual void ApplySingleTargetDamage(GameObject attacker, GameObject target)
    {
        float calDamage = skillData.attackDamage;

        // KIỂM TRA AI LÀ NGƯỜI ĐÁNH ĐỂ LẤY SỨC MẠNH (ATK)
        HeroStateMachine heroAttacker = attacker.GetComponent<HeroStateMachine>();
        EnemyStateMachine enemyAttacker = attacker.GetComponent<EnemyStateMachine>();

        if (heroAttacker != null)
        {
            calDamage += heroAttacker.baseHero.curATK;
        }
        else if (enemyAttacker != null)
        {
            calDamage += enemyAttacker.baseEnemy.curATK; 
        }
        ApplyDamageToTarget(attacker, target, calDamage);

        if (heroAttacker != null && heroAttacker.heroPanelHandler != null)
        {
            heroAttacker.heroPanelHandler.UpdateHeroPanel();
        }
    }

    protected virtual void ApplyAoEDamage(GameObject attacker, Vector3 impactPoint, float aoeRadius)
    {
        int maxTargets = skillData.maxEnemyCount > 0 ? skillData.maxEnemyCount : 99;
        int hitCount = 0;
        float calDamage = skillData.attackDamage;

        HeroStateMachine heroAttacker = attacker.GetComponent<HeroStateMachine>();
        if (heroAttacker != null) calDamage += heroAttacker.baseHero.curATK;

        string targetTag = (heroAttacker != null) ? "Enemy" : "Hero";
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject t in allTargets)
        {
            if (hitCount >= maxTargets) break;
            float distance = Vector2.Distance(impactPoint, t.transform.position);

            if (distance <= aoeRadius)
            {
                ApplyDamageToTarget(attacker, t, calDamage);
                hitCount++;
            }
        }

        if (heroAttacker != null && heroAttacker.heroPanelHandler != null)
            heroAttacker.heroPanelHandler.UpdateHeroPanel();
    }


    protected virtual float GetAnimationDuration(Animator animator, string triggerName)
    {
        return 1.0f;
    }

    protected virtual bool MoveTowardsTarget(GameObject character, Vector3 target)
    {
        Transform body = character.transform.Find("Body");
        if (body == null) return false;

        body.DOMove(target, 0.5f).SetEase(Ease.Linear);
        return Vector3.Distance(body.position, target) > 0.1f;
    }
}