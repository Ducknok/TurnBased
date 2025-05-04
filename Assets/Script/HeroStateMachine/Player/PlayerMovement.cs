using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isLeader = false;
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] public Animator anim;
    public Vector2 movement;

    // Lưu lịch sử vị trí
    public List<Vector3> positionHistory = new List<Vector3>();
    private float recordTimer;
    [SerializeField] private float recordInterval = 0.1f;

    private void Awake()
    {
        this.rb = this.transform.parent.transform.Find("Body").GetComponent<Rigidbody2D>();
        this.anim = this.transform.parent.transform.Find("Body").GetComponent<Animator>();
    }

    private void Update()
    {
        if (CombatController.Instance.CBZ.isInCombat) return;
        if (!isLeader) return; // Chỉ Leader mới nhập input

        movement.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.y);
        anim.SetFloat("Speed", movement.sqrMagnitude);

        if (movement != Vector2.zero)
        {
            anim.SetFloat("LastHorizontal", movement.x);
            anim.SetFloat("LastVertical", movement.y);
        }
    }

    private void FixedUpdate()
    {
            // Nếu đang combat thì không cho di chuyển
            if (PlayerController.Instance.CombatZone.isInCombat)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            if (isLeader)
            {
                rb.velocity = movement.normalized * moveSpeed;

                recordTimer += Time.fixedDeltaTime;
                if (recordTimer >= recordInterval)
                {
                    positionHistory.Insert(0, this.transform.parent.Find("Body").position);
                    recordTimer = 0f;
                }
            }
    }

    public void ClearHistory()
    {
        positionHistory.Clear();
    }
}
