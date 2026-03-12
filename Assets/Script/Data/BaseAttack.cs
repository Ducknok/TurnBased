using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Skill", menuName = "RPG/Skills/New Skill")]
[Serializable]
public class BaseAttack : ScriptableObject
{
    public enum Effect { Sword, Lance, Wind, Lightning, Physical, None }
    public enum AttackType { NormalAttack, SpecialAttack }
    public Sprite attackImage;
    public string attackName;
    public string attackDescription;
    public int attackDamage;
    public int attackCost;
    public int maxEnemyCount;
    public AttackType attackType;
    public Effect effect1;
    public Effect effect2;
    public string hitParticleName;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying || string.IsNullOrWhiteSpace(attackName)) return;
        string assetPath = AssetDatabase.GetAssetPath(this);
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

    protected virtual List<BaseAttack.Effect> GetCombinedEffects(GameObject attacker)
    {
        List<BaseAttack.Effect> effects = new List<BaseAttack.Effect>();

        if (skillData.effect1 != BaseAttack.Effect.None)
            effects.Add(skillData.effect1);

        if (skillData.effect2 != BaseAttack.Effect.None && !effects.Contains(skillData.effect2))
            effects.Add(skillData.effect2);

        if (effects.Count == 0)
        {
            HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
            if (hsm != null && hsm.baseHero != null)
            {
                string hType = hsm.baseHero.heroType.ToString();
                if (hType == "Warrior") effects.Add(BaseAttack.Effect.Sword);
                else if (hType == "Lancer") effects.Add(BaseAttack.Effect.Lance);
            }

            if (effects.Count == 0) effects.Add(BaseAttack.Effect.Physical);
        }

        return effects;
    }


    protected virtual bool IsMagicalAttack()
    {

        if (skillData.effect1 == BaseAttack.Effect.Wind || skillData.effect1 == BaseAttack.Effect.Lightning)
        {
            return true;
        }
        return false;
    }

    protected virtual void ApplyDamageToTarget(GameObject attacker, GameObject target, int rawDamage)
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

        bool isMagic = IsMagicalAttack();
        int targetDefense = 0;

        if (esm != null && esm.baseEnemy != null)
            targetDefense = isMagic ? esm.baseEnemy.curMDEF : esm.baseEnemy.curDEF;
        else if (hsm != null && hsm.baseHero != null)
            targetDefense = isMagic ? hsm.baseHero.curMDEF : hsm.baseHero.curDEF;


        int finalDamage = Mathf.Max(1, rawDamage - targetDefense);

        if (rawDamage <= 0) finalDamage = 0;

        if (esm != null)
        {
            EnemyTakeDamage takeDamageComp = esm.GetComponent<EnemyTakeDamage>();
            if (takeDamageComp != null)
            {
                List<BaseAttack.Effect> combined = GetCombinedEffects(attacker);
                BaseAttack.Effect eff1 = combined[0];
                BaseAttack.Effect eff2 = combined.Count > 1 ? combined[1] : BaseAttack.Effect.None;

                takeDamageComp.TakeDamage(esm.gameObject, finalDamage, eff1, eff2);
            }
        }
        else if (hsm != null)
        {
            HeroTakeDamage heroTakeDamageComp = hsm.GetComponent<HeroTakeDamage>();
            if (heroTakeDamageComp != null) heroTakeDamageComp.TakeDamage(hsm.gameObject, finalDamage);
        }
    }

    protected virtual void ApplySingleTargetDamage(GameObject attacker, GameObject target)
    {
        int rawDamage = skillData.attackDamage;

        HeroStateMachine heroAttacker = attacker.GetComponent<HeroStateMachine>();
        EnemyStateMachine enemyAttacker = attacker.GetComponent<EnemyStateMachine>();

        bool isMagic = IsMagicalAttack();

        if (heroAttacker != null && heroAttacker.baseHero != null)
            rawDamage += isMagic ? heroAttacker.baseHero.curMATK : heroAttacker.baseHero.curATK;
        else if (enemyAttacker != null && enemyAttacker.baseEnemy != null)
            rawDamage += isMagic ? enemyAttacker.baseEnemy.curMATK : enemyAttacker.baseEnemy.curATK;
        ApplyDamageToTarget(attacker, target, rawDamage);

        if (heroAttacker != null && heroAttacker.heroPanelHandler != null)
        {
            heroAttacker.heroPanelHandler.UpdateHeroPanel();
        }
    }

    protected virtual void ApplyAoEDamage(GameObject attacker, GameObject primaryTarget, Vector3 impactPoint, float aoeRadius)
    {
        int maxTargets = skillData.maxEnemyCount > 0 ? skillData.maxEnemyCount : 99;
        int hitCount = 0;
        int rawDamage = skillData.attackDamage;

        HeroStateMachine heroAttacker = attacker.GetComponent<HeroStateMachine>();
        EnemyStateMachine enemyAttacker = attacker.GetComponent<EnemyStateMachine>();

        bool isMagic = IsMagicalAttack();
        if (heroAttacker != null && heroAttacker.baseHero != null)
            rawDamage += isMagic ? heroAttacker.baseHero.curMATK : heroAttacker.baseHero.curATK;
        else if (enemyAttacker != null && enemyAttacker.baseEnemy != null)
            rawDamage += isMagic ? enemyAttacker.baseEnemy.curMATK : enemyAttacker.baseEnemy.curATK;


        if (primaryTarget != null)
        {
            ApplyDamageToTarget(attacker, primaryTarget, rawDamage);
            hitCount++;
        }

        string targetTag = (heroAttacker != null) ? "Enemy" : "Hero";
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject t in allTargets)
        {
            if (hitCount >= maxTargets) break;
            if (t == primaryTarget || t == null || !t.activeInHierarchy) continue;

            float distance = Vector2.Distance(impactPoint, t.transform.position);

            if (distance <= aoeRadius)
            {
                ApplyDamageToTarget(attacker, t, rawDamage);
                hitCount++;
            }
        }

        if (heroAttacker != null && heroAttacker.heroPanelHandler != null)
            heroAttacker.heroPanelHandler.UpdateHeroPanel();
    }

    public virtual List<GameObject> GetTargetsInAoE(GameObject attacker, GameObject primaryTarget, Vector3 impactPoint, float aoeRadius)
    {
        List<GameObject> affectedTargets = new List<GameObject>();
        int maxTargets = skillData.maxEnemyCount > 0 ? skillData.maxEnemyCount : 99;

        if (primaryTarget != null)
        {
            affectedTargets.Add(primaryTarget);
        }

        string targetTag = attacker.GetComponent<HeroStateMachine>() != null ? "Enemy" : "Hero";
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject t in allTargets)
        {
            if (affectedTargets.Count >= maxTargets) break;
            if (t == primaryTarget || t == null || !t.activeInHierarchy) continue;

            float distance = Vector2.Distance(impactPoint, t.transform.position);
            if (distance <= aoeRadius)
            {
                affectedTargets.Add(t);
            }
        }

        return affectedTargets;
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