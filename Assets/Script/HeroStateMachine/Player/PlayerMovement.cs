using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : DucMonobehaviour
{
    [Header("Leader")]
    public bool isLeader = false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private MainInventoryController mainInventory;

    public Vector2 movement;
    public Vector2 lastMove = Vector2.down;

    public List<Vector3> positionHistory = new List<Vector3>();
    private float recordTimer;
    [SerializeField] private float recordInterval = 0.1f;

    protected override void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();

        if (mainInventory == null)
            mainInventory = FindAnyObjectByType<MainInventoryController>();
    }

    protected override void Update()
    {
        MoveInput();
    }

    protected override void FixedUpdate()
    {
        CheckState();
    }

    private void MoveInput()
    {
        if (CombatController.Instance.CBZ.isInCombat || mainInventory.isMainInventoryOpen)
            return;

        if (!isLeader) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.y);

        anim.SetFloat("Speed", movement.sqrMagnitude);

        if (movement != Vector2.zero)
        {
            lastMove = movement.normalized;

            anim.SetFloat("LastHorizontal", movement.x);
            anim.SetFloat("LastVertical", movement.y);
        }
    }

    public override void CheckState()
    {
        if (CombatController.Instance.CBZ.isInCombat || mainInventory.isMainInventoryOpen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!isLeader) return;

        rb.linearVelocity = movement.normalized * moveSpeed;

        // record movement history for followers
        recordTimer += Time.fixedDeltaTime;

        if (recordTimer >= recordInterval)
        {
            positionHistory.Insert(0, transform.position);
            recordTimer = 0f;

            if (positionHistory.Count > 200)
                positionHistory.RemoveAt(positionHistory.Count - 1);
        }
    }

    public void ClearHistory()
    {
        positionHistory.Clear();
    }
}