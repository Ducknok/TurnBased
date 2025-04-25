using System.Collections.Generic;
using UnityEngine;

public class FollowerMovement : MonoBehaviour
{
    public LeaderMovement leader;  // tham chiếu đến leader
    public int delaySteps = 10;    // độ trễ

    private Vector3 targetPosition;

    void Update()
    {
        // Cập nhật mục tiêu nếu leader có đủ lịch sử
        if (leader.positionHistory.Count > delaySteps)
        {
            Vector3[] historyArray = leader.positionHistory.ToArray();
            targetPosition = historyArray[historyArray.Length - delaySteps];
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, leader.moveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Gọi hàm này khi muốn follower lập tức nhảy về đúng vị trí theo sau leader
    /// </summary>
    public void JumpToFollowPosition()
    {
        if (leader.positionHistory.Count > delaySteps)
        {
            Vector3[] historyArray = leader.positionHistory.ToArray();
            Vector3 jumpPosition = historyArray[historyArray.Length - delaySteps];
            this.transform.position = jumpPosition;
        }
        else
        {
            // Nếu chưa có đủ lịch sử, đứng ngay tại chỗ leader
            transform.position = leader.transform.position;
        }
    }
}
