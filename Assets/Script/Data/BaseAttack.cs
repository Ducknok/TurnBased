using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq; // Thêm cái này để lọc danh sách cho gọn

[CreateAssetMenu(fileName = "New Skill", menuName = "RPG/Skills/New Skill")]
[Serializable]
public class BaseAttack : ScriptableObject
{
    public enum Effect { Sword, Lance, Wind, Lightning, Physical, None }
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

    // =================================================================
    // HÀM FIX LỖI: Tự động gom Hệ từ Skill + Hệ của bản thân Hero
    // =================================================================
    protected virtual List<BaseAttack.Effect> GetCombinedEffects(GameObject attacker)
    {
        HashSet<BaseAttack.Effect> effects = new HashSet<BaseAttack.Effect>();

        // 1. Lấy từ kỹ năng (Bỏ qua None/Physical nếu có thể để ưu tiên hệ nguyên tố)
        if (skillData.effect1 != BaseAttack.Effect.None && skillData.effect1 != BaseAttack.Effect.Physical)
            effects.Add(skillData.effect1);
        if (skillData.effect2 != BaseAttack.Effect.None && skillData.effect2 != BaseAttack.Effect.Physical)
            effects.Add(skillData.effect2);

        // 2. Lấy từ bản thân Hero (Elemental & HeroType)
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        if (hsm != null && hsm.baseHero != null)
        {
            // Lấy nguyên tố (Wind, Lightning...)
            if (Enum.TryParse(hsm.baseHero.elemental.ToString(), out BaseAttack.Effect elem))
            {
                if (elem != BaseAttack.Effect.None) effects.Add(elem);
            }

            // Lấy loại vũ khí (Warrior -> Sword, Lancer -> Lance)
            string hType = hsm.baseHero.heroType.ToString();
            if (hType == "Warrior") effects.Add(BaseAttack.Effect.Sword);
            else if (hType == "Lancer") effects.Add(BaseAttack.Effect.Lance);
        }

        List<BaseAttack.Effect> finalInfo = effects.ToList();
        if (finalInfo.Count == 0) finalInfo.Add(BaseAttack.Effect.Physical);

        return finalInfo;
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

        if (esm != null)
        {
            EnemyTakeDamage takeDamageComp = esm.GetComponent<EnemyTakeDamage>();
            if (takeDamageComp != null)
            {
                // THAY ĐỔI TẠI ĐÂY: Lấy danh sách hệ đã gộp (Skill + Hero)
                List<BaseAttack.Effect> combined = GetCombinedEffects(attacker);

                // Gửi 2 hệ đầu tiên (thường là Nguyên tố và Vũ khí) sang cho quái
                BaseAttack.Effect eff1 = combined[0];
                BaseAttack.Effect eff2 = combined.Count > 1 ? combined[1] : BaseAttack.Effect.None;

                takeDamageComp.TakeDamage(esm.gameObject, calDamage, eff1, eff2);
            }
        }
        else if (hsm != null)
        {
            hsm.GetComponent<HeroTakeDamage>().TakeDamage(hsm.gameObject, calDamage);
        }
    }

    protected virtual void ApplySingleTargetDamage(GameObject attacker, GameObject target)
    {
        float calDamage = skillData.attackDamage;

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

    protected virtual void ApplyAoEDamage(GameObject attacker, GameObject primaryTarget, Vector3 impactPoint, float aoeRadius)
    {
        int maxTargets = skillData.maxEnemyCount > 0 ? skillData.maxEnemyCount : 99;
        int hitCount = 0;
        float calDamage = skillData.attackDamage;

        HeroStateMachine heroAttacker = attacker.GetComponent<HeroStateMachine>();
        if (heroAttacker != null) calDamage += heroAttacker.baseHero.curATK;

        if (primaryTarget != null)
        {
            ApplyDamageToTarget(attacker, primaryTarget, calDamage);
            hitCount++;
        }

        string targetTag = (heroAttacker != null) ? "Enemy" : "Hero";
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject t in allTargets)
        {
            if (hitCount >= maxTargets) break;
            if (t == primaryTarget) continue;

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
            if (t == primaryTarget) continue;

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