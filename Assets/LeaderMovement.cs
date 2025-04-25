using System.Collections.Generic;
using UnityEngine;

public class LeaderMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float recordInterval = 0.1f; // mỗi bao lâu sẽ ghi lại vị trí
    private float timer;

    public Queue<Vector3> positionHistory = new Queue<Vector3>();
    public int historyLimit = 100;

    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= recordInterval)
        {
            timer = 0f;
            positionHistory.Enqueue(transform.position);

            // Giữ số lượng vị trí vừa đủ
            if (positionHistory.Count > historyLimit)
            {
                positionHistory.Dequeue();
            }
        }
    }
}
