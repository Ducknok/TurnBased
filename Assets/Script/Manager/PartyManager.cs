using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PartyManager : Singleton<PartyManager>
{

    public List<PlayerMovement> partyMembers = new List<PlayerMovement>();
    private int currentLeaderIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        
    }

    private void Start()
    {
        SetLeader(this.currentLeaderIndex);
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
        this.LoadPlayerMove();
    }
    public void LoadPlayerMove()
    {
        //Debug.LogError("LoadComponent of " + this.gameObject);
        foreach (var hero in CombatController.Instance.CBM.playersInCombat)
        {
            if (hero == null)
            {
                //Debug.LogWarning("Hero is null, có thể đã bị destroy.");
                continue;
            }

            Transform movement = hero.transform.Find("Movement");
            if (movement == null)
            {
               // Debug.LogWarning($"Không tìm thấy child 'Movement' trong {hero.name}");
                continue;
            }

            PlayerMovement plMove = movement.GetComponent<PlayerMovement>();
            if (plMove == null)
            {
                //Debug.LogWarning($"Không tìm thấy component PlayerMovement trên {movement.name}");
                continue;
            }

            if (!this.partyMembers.Contains(plMove))
            {
                this.partyMembers.Add(plMove);
            }
        }
    }


    // Switch leader theo vòng tròn (nếu bạn vẫn muốn dùng nút để xoay vòng)
    public void SwitchLeader()
    {
        this.currentLeaderIndex++;
        if (this.currentLeaderIndex >= this.partyMembers.Count)
            this.currentLeaderIndex = 0;

        SetLeader(this.currentLeaderIndex);
    }

    // Set leader bằng index
    public void SetLeader(int index)
    {
        
        if (index < 0 || index >= this.partyMembers.Count) return;

        var newLeader = this.partyMembers[index];
        this.partyMembers.RemoveAt(index);
        this.partyMembers.Insert(0, newLeader);

        this.UpdateLeaderAndFollowers();
    }

    // Set leader bằng HeroStateMachine
    public void SetLeader(HeroStateMachine heroSelected)
    {
        for (int i = 0; i < partyMembers.Count; i++)
        {
            if (partyMembers[i].GetComponentInParent<HeroStateMachine>() == heroSelected)
            {
                this.SetLeader(i); // Gọi lại hàm theo index
                return;
            }
        }
    }
    public void SyncWithCBM(List<GameObject> cbmList)
    {
        this.partyMembers.Clear();
        foreach (var obj in cbmList)
        {
            var hero = obj.GetComponentInChildren<PlayerMovement>();
            if (hero != null) partyMembers.Add(hero);
        }
    }
    // Cập nhật trạng thái leader-follower
    private void UpdateLeaderAndFollowers()
    {
        Debug.Log("Updating leader and followers...");
        for (int i = 0; i < partyMembers.Count; i++)
        {
            PlayerMovement member = partyMembers[i];
            member.isLeader = (i == 0);
            member.ClearHistory();

            FollowerMovement follower = member.GetComponent<FollowerMovement>();
            if (follower != null)
            {
                if (i == 0)
                {
                    follower.enabled = false; // Leader không follow ai
                }
                else
                {
                    follower.leader = partyMembers[i - 1]; // Theo người đứng trước
                    follower.followDelay = 10;
                    follower.enabled = true;
                }
            }
        }
    }

    
}
