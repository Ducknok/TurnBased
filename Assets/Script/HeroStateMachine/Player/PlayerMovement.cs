using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class PlayerMovement : DucMonobehaviour
{
    public bool isLeader = false;
    [SerializeField] private MainInventoryController mainInventory;
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] public Animator anim;
    public Vector2 movement;

    // Lưu lịch sử vị trí
    public List<Vector3> positionHistory = new List<Vector3>();
    private float recordTimer;
    [SerializeField] private float recordInterval = 0.1f;
    public Vector2 lastMove = Vector2.down;

    protected override void Awake()
    {
        this.rb = this.transform.parent.GetComponent<Rigidbody2D>();
        this.anim = this.transform.parent.GetComponent<Animator>();
        this.mainInventory = FindObjectOfType<MainInventoryController>();
    }
    protected override void Update()
    {
        this.MoveInput();
    }
    protected override void FixedUpdate()
    {
        this.CheckState();
        
    }
    private void MoveInput()
    {
        if (CombatController.Instance.CBZ.isInCombat || this.mainInventory.isMainInventoryOpen) return;
        else
        {
            if (!isLeader) return; // Chỉ Leader mới nhập input

            movement.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            anim.SetFloat("Horizontal", movement.x);
            anim.SetFloat("Vertical", movement.y);
            lastMove = movement.normalized;
            anim.SetFloat("Speed", movement.sqrMagnitude);

            if (movement != Vector2.zero)
            {
                anim.SetFloat("LastHorizontal", movement.x);
                anim.SetFloat("LastVertical", movement.y);
            }
        }
    }
    public override void CheckState()
    {
        // Nếu đang combat thì không cho di chuyển
        if (CombatController.Instance.CBZ.isInCombat || this.mainInventory.isMainInventoryOpen)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        else
        {
            if (this.isLeader)
            {
                rb.velocity = movement.normalized * moveSpeed;

                recordTimer += Time.fixedDeltaTime;
                if (recordTimer >= recordInterval)
                {
                    positionHistory.Insert(0, this.transform.parent.position);
                    recordTimer = 0f;
                }
            }
        }
    }
   
    public void ClearHistory()
    {
        positionHistory.Clear();
    }

    
}
