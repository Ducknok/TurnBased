using System.Collections;
using System.Collections.Generic;
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

    private void SpawnEffect(string prefab, Vector3 position)
    {
        Transform newVFX = VFXSpawner.Instance.Spawn(prefab, position, Quaternion.identity);
        if (newVFX == null) return;

        newVFX.gameObject.SetActive(true);
    }

    public void CloneTriangle()
    {
        StartCoroutine(new CloneTriangleSkill(hsm).Execute());
    }
}
