using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{  
    [SerializeField] private static PlayerController instance;
    public static PlayerController Instance => instance;
    [SerializeField] private CombatZone combatZone;
    public CombatZone CombatZone => combatZone;


    protected void Awake()
    {
        this.LoadCombatZone();
    }
    protected void LoadCombatZone()
    {
        if (this.combatZone != null) return;
        this.combatZone = GameObject.FindObjectOfType<CombatZone>();
    }

}
