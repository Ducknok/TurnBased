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
    public enum Effect
    {
        Sword,
        Lance,
        Wind,
        Lightning,
    }
    public enum AttackType
    {
        NormalAttack,
        SpecialAttack,
    }
    public Sprite attackImage;
    public string attackName;
    public string attackDescription;
    public float attackDamage;  //Base Damage 15, lvl 10, stamina 35 = basedmg + lvl + stamina = 60
    public float attackCost;    //Mana cost
    public int maxEnemyCount;
    public AttackType attackType;
    public Effect effect1;
    public Effect effect2;
    public string hitParticleName;
#if UNITY_EDITOR
    // Hàm này tự động chạy mỗi khi bạn thay đổi giá trị trên Inspector
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

    protected virtual void ApplySkillEffects(GameObject attacker)
    {
        BurstDamage.Instance.DoDamage(attacker);
    }

    //// Lấy thời gian của animation
    protected virtual float GetAnimationDuration(Animator animator, string triggerName)
    {
        // Thực hiện logic để lấy thời gian của animation
        // Hoặc đơn giản hơn, bạn có thể lưu trữ thời gian trong SkillData
        return 1.0f; // Giá trị mặc định
    }
    protected virtual bool MoveTowardsTarget(GameObject hero, Vector3 target)
    {
        Transform body = hero.transform.Find("Body");
        if (body == null) return false;

        body.DOMove(target, 0.5f).SetEase(Ease.Linear);
        return Vector3.Distance(body.position, target) > 0.1f;
        //return target != (this.transform.Find("Body").position = Vector3.MoveTowards(this.transform.Find("Body").position, target, this.animSpeed * Time.deltaTime));
    }
   
}
