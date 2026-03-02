using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelected : DucMonobehaviour
{
    public GameObject enemy;
    public GameObject chooseEnemy;
    protected override void Start()
    {
        this.enemy = this.transform.gameObject;
        Debug.LogWarning(this.enemy);
    }
    public void ShowChooseEnemy()
    {
        this.chooseEnemy.SetActive(true);
    }
    public void HideChooseEnemy()
    {
        this.chooseEnemy.SetActive(false);
    }
}
