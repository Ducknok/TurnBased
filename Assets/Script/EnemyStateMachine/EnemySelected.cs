using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelected : MonoBehaviour
{
    public GameObject enemy;
    public GameObject chooseEnemy;
    private void Start()
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
