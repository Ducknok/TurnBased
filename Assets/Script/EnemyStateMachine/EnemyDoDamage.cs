using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDoDamage : MonoBehaviour
{
    [SerializeField] private EnemyStateMachine esm;
    public EnemyStateMachine ESM => esm;
    
    private void Awake()
    {
        this.LoadEnemySM();
    }
    protected void LoadEnemySM()
    {
        if (this.esm != null) return; // N?u ?? c¨® heroSMList r?i th¨¬ kh?ng c?n t¨¬m l?i
        this.esm = this.transform.parent.GetComponent<EnemyStateMachine>();
    }
    public void DoDamage()
    {
        this.esm.DoDamage();
    }
    
}
