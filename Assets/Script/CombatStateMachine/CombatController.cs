using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    protected static CombatController instance;
    public static CombatController Instance => instance;
    [SerializeField] protected CombatZone cbz;
    public CombatZone CBZ => cbz;
    [SerializeField] protected CombatStateMachine cbm;
    public CombatStateMachine CBM => cbm;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject); // giữ nó giữa các scene

        this.LoadCombatZone();
        this.LoadCombatStateMachine();
    }
    private void LoadCombatZone()
    {
        if (this.cbz != null) return;
        this.cbz = FindObjectOfType<CombatZone>();
    }
    private void LoadCombatStateMachine()
    {
        if (this.cbm != null) return;
        this.cbm = FindObjectOfType<CombatStateMachine>();
    }
}
