using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : SkillBehaviour
{
    protected override void ApplySkillEffects(HeroStateMachine hero, GameObject target)
    {
        // Tính toán damage dựa trên các thuộc tính của hero và target
        float damage = skillData.attackDamage * hero.baseHero.baseATK;

        // Áp dụng damage vào target
        EnemyStateMachine enemy = target.GetComponent<EnemyStateMachine>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, skillData.effect1, skillData.effect2);
        }

        //// Tạo hiệu ứng va chạm
        //if (impactEffectPrefab != null)
        //{
        //    Instantiate(impactEffectPrefab, target.transform.position, Quaternion.identity);
        //}
    }

    // Bạn có thể ghi đè phương thức Activate để thêm logic đặc biệt
    public override IEnumerator Activate(HeroStateMachine hero, GameObject target)
    {
        // Gọi logic cơ bản từ lớp cha
        yield return StartCoroutine(base.Activate(hero, target));
        
        Vector2 enemyPosition = new Vector2(hero.enemyToAttack.transform.position.x,
                                            hero.enemyToAttack.transform.position.y + 3f);
        // Thêm logic đặc biệt cho Fireball
        // Ví dụ: tạo một quả cầu lửa di chuyển từ hero đến target
        Transform newLightning = VFXSpawner.Instance.Spawn(VFXSpawner.lightningtStrike, enemyPosition, Quaternion.identity);
        newLightning.gameObject.SetActive(true);
    }
}
