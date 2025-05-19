using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    private static PartyManager instance;
    public static PartyManager Instance => instance;

    public List<PlayerMovement> partyMembers = new List<PlayerMovement>();
    private int currentLeaderIndex = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Nếu bạn muốn giữ PartyManager khi load scene mới:
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject); // Chỉ destroy bản clone thôi
        }
    }


    private void Start()
    {
        SetLeader(this.currentLeaderIndex);
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
