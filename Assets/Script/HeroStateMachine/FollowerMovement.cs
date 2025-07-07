using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class FollowerMovement : DucMonobehaviour
{
    public PlayerMovement leader;
    public int followDelay = 10; // Số lượng frame trễ
    public float moveSpeed = 5f;
    public float stopDistance = 0.05f;
    public float desiredDistance = 2f;               // Giữ khoảng cách này với leader
    public float warpDistanceThreshold = 5f;         // Nếu leader cách quá xa thì teleport
    public float stuckTimeThreshold = 1.5f;          // Nếu bị kẹt lâu hơn thế này mới warp
    public float warpDuration = 0.35f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform body;

    private float stuckTimer = 0f;
    private Vector2 lastPosition;
    private bool isWarping = false;

    protected override void Awake()
    {
        this.body = this.transform.parent;
        this.rb = body.GetComponent<Rigidbody2D>();
        this.anim = body.GetComponent<Animator>();
        this.lastPosition = rb.position;
    }

    protected override void FixedUpdate()
    {
        this.CheckState();   
    }
    public override void CheckState()
    {
        if (CombatController.Instance.CBZ.isInCombat || leader == null || leader.positionHistory.Count <= followDelay || isWarping)
        {
            rb.velocity = Vector2.zero;
            anim.SetFloat("Speed", 0);
            return;
        }

        Vector3 targetPos = leader.positionHistory[followDelay];
        float distanceToLeader = Vector2.Distance(rb.position, leader.transform.position);

        // Nếu đang kẹt (không di chuyển)
        if ((rb.position - lastPosition).sqrMagnitude < 0.001f)
        {
            stuckTimer += Time.fixedDeltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }

        // Nếu bị kẹt quá lâu và leader đi quá xa, warp đến gần leader
        if (stuckTimer > stuckTimeThreshold && distanceToLeader > warpDistanceThreshold)
        {
            WarpToLeader();
            return;
        }

        // Nếu đã ở gần leader, không cần di chuyển
        if (distanceToLeader <= desiredDistance)
        {
            rb.velocity = Vector2.zero;
            anim.SetFloat("Speed", 0);
            return;
        }

        // Di chuyển về vị trí delay
        Vector2 direction = (targetPos - body.position);
        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            direction.Normalize();
            Vector2 move = direction * moveSpeed;
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

            // Animation update
            anim.SetFloat("Horizontal", direction.x);
            anim.SetFloat("Vertical", direction.y);
            anim.SetFloat("Speed", move.sqrMagnitude);

            if (direction != Vector2.zero)
            {
                anim.SetFloat("LastHorizontal", direction.x);
                anim.SetFloat("LastVertical", direction.y);
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            anim.SetFloat("Speed", 0);
        }

        lastPosition = rb.position;
    }
    private void WarpToLeader()
    {
        isWarping = true;
        rb.velocity = Vector2.zero;
        anim.SetFloat("Speed", 0);

        Vector3 targetPos = leader.transform.position - (Vector3)(leader.lastMove.normalized * desiredDistance);
        body.DOMove(targetPos, warpDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            isWarping = false;
            stuckTimer = 0f;
            lastPosition = rb.position;
        });
    }

    protected override void OnEnable() { base.OnEnable(); }
    protected override void OnDisable() { base.OnDisable(); }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoaded(scene, mode);
    }
}
