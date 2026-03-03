using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelected : DucMonobehaviour
{
    public GameObject enemy;
    public GameObject chooseEnemy;

    private List<GameObject> currentAoeTargets = new List<GameObject>();

    protected override void Start()
    {
        this.enemy = this.transform.gameObject;
    }

    public void ShowChooseEnemy(HeroStateMachine activeHero = null)
    {
        this.chooseEnemy.SetActive(true);

        currentAoeTargets.Clear();

        if (activeHero != null && activeHero.currentAttack != null)
        {
            SkillBehaviour currentSkill = activeHero.currentAttack;
            BaseAttack skillData = currentSkill.skillData;
            if (skillData.maxEnemyCount > 1)
            {
                float aoeRadius = 5f;
                currentAoeTargets = currentSkill.GetTargetsInAoE(activeHero.gameObject, this.gameObject, this.transform.position, aoeRadius);

                foreach (GameObject t in currentAoeTargets)
                {
                    if (t != this.gameObject) 
                    {
                        EnemySelected es = t.GetComponent<EnemySelected>();
                        if (es != null)
                        {
                            es.chooseEnemy.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    public void HideChooseEnemy()
    {
        this.chooseEnemy.SetActive(false);

        foreach (GameObject t in currentAoeTargets)
        {
            if (t != null && t != this.gameObject)
            {
                EnemySelected es = t.GetComponent<EnemySelected>();
                if (es != null)
                {
                    es.chooseEnemy.SetActive(false);
                }
            }
        }
        currentAoeTargets.Clear();
    }
}
