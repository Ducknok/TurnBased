using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;
    public static PlayerController Instance => instance;

    [SerializeField] private List<HeroStateMachine> heroSMList;  // Danh s¨¢ch ch?a t?t c? HeroStateMachine
    public List<HeroStateMachine> HeroSMList => heroSMList;

    [SerializeField] private CombatZone combatZone;
    public CombatZone CombatZone => combatZone;

    protected void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject); // Gi? PlayerController gi?a c¨¢c scene
        this.LoadCombatZone();
        this.LoadHeroSM();
    }

    protected void LoadCombatZone()
    {
        if (this.combatZone != null) return;
        this.combatZone = GameObject.FindObjectOfType<CombatZone>();
    }

    protected void LoadHeroSM()
    {
        if (this.heroSMList != null && this.heroSMList.Count > 0) return; // N?u ?? c¨® heroSMList r?i th¨¬ kh?ng c?n t¨¬m l?i
        this.heroSMList = new List<HeroStateMachine>(GameObject.FindObjectsOfType<HeroStateMachine>());
    }
}
