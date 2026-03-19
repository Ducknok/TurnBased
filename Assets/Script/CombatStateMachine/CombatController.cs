using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatController : Singleton<CombatController>
{
    [SerializeField] protected CombatZone cbz;
    public CombatZone CBZ => cbz;
    [SerializeField] protected CombatStateMachine cbm;
    public CombatStateMachine CBM => cbm;

    protected override void Awake()
    {
        base.Awake();
        this.LoadComponent();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
 
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoaded(scene, mode);
        this.LoadComponent();
    }
    public void LoadComponent()
    {
        //Debug.LogError("LoadComponent of " + this.gameObject);
        this.LoadCombatStateMachine();
        this.LoadCombatZone();
    }
    private void LoadCombatZone()
    {
        if (this.cbz != null) return;
        this.cbz = FindAnyObjectByType<CombatZone>();
    }
    private void LoadCombatStateMachine()
    {
        if (this.cbm != null) return;
        this.cbm = FindAnyObjectByType<CombatStateMachine>();
    }
}
