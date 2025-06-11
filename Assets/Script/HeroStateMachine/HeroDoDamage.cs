using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroDoDamage : MonoBehaviour
{
    [SerializeField] private CombatController combatCtrl;
    [SerializeField] private HeroStateMachine hsm;
    [SerializeField] private Transform position;

    private void Awake()
    {
        LoadHeroSM();
        LoadCombatCtrl();
    }

    private void LoadHeroSM()
    {
        if (this.hsm == null)
            this.hsm = this.transform.parent.GetComponent<HeroStateMachine>();
    }

    private void LoadCombatCtrl()
    {
        if (this.combatCtrl == null)
            this.combatCtrl = FindObjectOfType<CombatController>();
    }

    public void DoDamage()
    {
        this.hsm.DoDamage();
    }

    public void SwordSlashEffect()
    {
        SpawnEffect(VFXSpawner.swordSlash, this.position.position);
    }

    public void LanceSlashEffect()
    {
        SpawnEffect(VFXSpawner.lanceSlash, this.position.position);
    }

    public void LightnintStrikeEffect()
    {
        Vector2 enemyPosition = new Vector2(this.hsm.enemyToAttack.transform.position.x,
                                            this.hsm.enemyToAttack.transform.position.y + 3.25f);
        if (float.IsNaN(enemyPosition.x) || float.IsNaN(enemyPosition.y))
        {
            Debug.LogError("enemyToAttack đang có position NaN!");
            return;
        }

        SpawnEffect(VFXSpawner.lightningtStrike, enemyPosition);
    }
    public void LightningTrail()
    {
        Transform body = hsm.transform.Find("Body");
        
        // Spawn hiệu ứng tại vị trí hiện tại của body
        Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.lightningTrail, new Vector3(hsm.transform.Find("Body").position.x + 4f, hsm.transform.Find("Body").position.y,0), Quaternion.identity);
        effect.gameObject.SetActive(true);
        // Gắn hiệu ứng làm con của body => sẽ di chuyển theo hero
        if (effect != null)
        {
            effect.SetParent(body);
        }
    }
    public void GroundSlash()
    {
        Transform origin = this.hsm.transform.Find("Body").Find("GroundSlashPosition");
        if (origin == null) return;

            Vector3 startPos = origin.position;
            Vector3 targetPos = this.hsm.enemyToAttack.transform.Find("Body").position;

            Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.groundSlash, startPos, Quaternion.identity);
            if (effect != null)
            {
                effect.gameObject.SetActive(true);

                // Hướng effect về phía enemy
                Vector3 dir = (targetPos - startPos).normalized;
                effect.right = dir; // Giả định VFX xoay theo trục right

                // Di chuyển tới enemy
                effect.DOMove(targetPos, 0.4f)
                      .SetEase(Ease.InQuad)
                      .OnComplete(() =>
                      {
                      // Gây damage hoặc hiệu ứng tại enemy
                      VFXSpawner.Instance.Despawn(effect);
                      });
            }
    }
    public void Tornado()
    {
        Vector2 enemyPosition = new Vector2(this.hsm.enemyToAttack.transform.position.x,
                                           this.hsm.enemyToAttack.transform.position.y - 1f);
        if (float.IsNaN(enemyPosition.x) || float.IsNaN(enemyPosition.y))
        {
            Debug.LogError("enemyToAttack đang có position NaN!");
            return;
        }

        SpawnEffect(VFXSpawner.tonardo, enemyPosition);
    }
    private void SpawnEffect(string prefab, Vector3 position)
    {
        Transform newVFX = VFXSpawner.Instance.Spawn(prefab, position, Quaternion.identity);
        if (newVFX == null) return;

        newVFX.gameObject.SetActive(true);
    }

    
}
