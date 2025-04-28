using UnityEngine;

public class FollowerMovement : MonoBehaviour
{
    public PlayerMovement leader;
    public int followDelay = 10; // Số lượng vị trí delay
    public float minFollowDistance = 0.5f; // Khoảng cách tối thiểu cách Leader
    public float followSpeed = 2f; // Tốc độ di chuyển mượt mà

    private Rigidbody2D rb;
    private Animator anim;
    private Transform body;

    private void Awake()
    {
        this.body = this.transform.parent.Find("Body");
        this.rb = body.GetComponent<Rigidbody2D>();
        this.anim = body.GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (leader == null || leader.positionHistory.Count <= followDelay) return;
        this.Movement();
    }

    private void Movement()
    {
        Vector3 targetPos = leader.positionHistory[followDelay];
        Vector3 currentPos = body.position;

        Vector2 direction = targetPos - currentPos;
        float distance = direction.magnitude;

        if (distance > minFollowDistance)
        {
            // Di chuyển theo hướng Leader với tốc độ mượt mà
            rb.velocity = Vector2.Lerp(rb.velocity, direction.normalized * leader.moveSpeed, followSpeed * Time.deltaTime);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        anim.SetFloat("Horizontal", direction.normalized.x);
        anim.SetFloat("Vertical", direction.normalized.y);
        anim.SetFloat("Speed", rb.velocity.sqrMagnitude);

        if (direction != Vector2.zero)
        {
            anim.SetFloat("LastHorizontal", direction.normalized.x);
            anim.SetFloat("LastVertical", direction.normalized.y);
        }
    }
}
