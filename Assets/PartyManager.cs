using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance;

    public List<PlayerMovement> partyMembers = new List<PlayerMovement>();
    private int currentLeaderIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetLeader(currentLeaderIndex);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            SwitchLeader();
        }
    }

    public void SwitchLeader()
    {
        currentLeaderIndex++;
        if (currentLeaderIndex >= partyMembers.Count)
            currentLeaderIndex = 0;

        SetLeader(currentLeaderIndex);
    }

    private void SetLeader(int index)
    {
        for (int i = 0; i < partyMembers.Count; i++)
        {
            PlayerMovement member = partyMembers[i];
            member.isLeader = (i == index);
            member.ClearHistory();

            FollowerMovement follower = member.GetComponent<FollowerMovement>();
            if (follower != null)
            {
                if (i == index)
                {
                    follower.enabled = false; // Leader không follow ai
                }
                else
                {
                    follower.leader = partyMembers[i - 1]; // Theo người đứng trước mình
                    follower.followDelay = 10; // Bạn có thể điều chỉnh cho từng nhân vật
                    follower.enabled = true;
                }
            }
        }
    }
}
