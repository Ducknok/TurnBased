using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{

    [SerializeField] private List<HeroStateMachine> heroSMList;  // Danh s¨¢ch ch?a t?t c? HeroStateMachine
    public List<HeroStateMachine> HeroSMList => heroSMList;

    protected override void Awake()
    {
        base.Awake();
        this.LoadComponent();
    }

    public void LoadComponent()
    {
        this.LoadHeroSM();
    }

    protected void LoadHeroSM()
    {
        if (this.heroSMList != null && this.heroSMList.Count > 0) return;
        this.heroSMList = new List<HeroStateMachine>(GameObject.FindObjectsOfType<HeroStateMachine>());
    }

}
