using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : DucMonobehaviour
{
    public enum AIState { Idle, Wander, Chase }
    public AIState currentState = AIState.Idle;

    [Header("Tốc Độ & Tầm Nhìn")]
    public float wanderSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public float aggroRange = 5f;
    public float wanderRadius = 4f;
    public float waitTime = 2f;
    public float engageDistance = 0.8f; // Khoảng cách đụng nhau để vào trận

    [Header("Thành phần (References)")]
    private Transform targetPlayer;

    private Transform rootObj;
    private Vector3 startPosition;
    private Vector3 wanderTarget;
    private bool isWaiting = false;

    private Animator anim;
    private SpriteRenderer sr;

    private string currentAnimName = "";
    private Coroutine waitCoroutine;

    private CombatZone cbz;

    protected override void Awake()
    {
        base.Awake();

        EnemyStateMachine esm = GetComponentInParent<EnemyStateMachine>();
        rootObj = esm != null ? esm.transform : (transform.parent != null ? transform.parent : transform);

        Transform body = rootObj.Find("Body");
        if (body != null)
        {
            anim = body.GetComponent<Animator>();
            sr = body.GetComponent<SpriteRenderer>();
        }

        cbz = FindAnyObjectByType<CombatZone>();
    }

    protected override void Start()
    {
        base.Start();
        startPosition = rootObj.position;
        GetNextWanderTarget();
    }

    protected override void Update()
    {
        // 1. Kiểm tra Combat để khóa AI
        bool isInCombat = false;
        if (cbz != null && cbz.isInCombat) isInCombat = true;
        else if (CombatController.Instance != null && CombatController.Instance.CBZ != null)
            isInCombat = CombatController.Instance.CBZ.isInCombat;

        if (isInCombat)
        {
            PlayAnimation("Idle");
            return;
        }

        // 2. Quét mục tiêu bằng bộ Radar mới
        CheckAggro();

        // 3. Thực thi hành động ngay trong Update (Khắc phục lỗi đứng im của FixedUpdate)
        switch (currentState)
        {
            case AIState.Wander:
                HandleWander();
                break;
            case AIState.Chase:
                HandleChase();
                break;
            case AIState.Idle:
                HandleIdle();
                break;
        }
    }

    private void CheckAggro()
    {
        if (rootObj == null) return;

        float minDistance = aggroRange;
        Transform closestTarget = null;

        // Vơ vét toàn bộ mục tiêu có khả năng
        List<GameObject> allTargets = new List<GameObject>();
        try { allTargets.AddRange(GameObject.FindGameObjectsWithTag("Hero")); } catch { }
        try { allTargets.AddRange(GameObject.FindGameObjectsWithTag("Player")); } catch { }

        // Lấy tâm thực tế của Slime
        Transform myBody = rootObj.Find("Body");
        Vector3 myCenter = myBody != null ? myBody.position : rootObj.position;

        foreach (GameObject p in allTargets)
        {
            if (p == null || !p.activeInHierarchy) continue;

            // ========================================================
            // FIX CỐT LÕI: LUÔN LẤY "BODY" ĐỂ ĐO KHOẢNG CÁCH VÀ RƯỢT!
            // Vì Root Object thường bị kẹt ở (0,0,0) làm sai lệch khoảng cách.
            // ========================================================
            Transform targetBody = p.transform.Find("Body");
            Vector3 targetCenter = targetBody != null ? targetBody.position : p.transform.position;

            float dist = Vector2.Distance(myCenter, targetCenter);

            if (dist <= minDistance)
            {
                minDistance = dist;
                // Nếu tìm thấy Body thì khóa mục tiêu vào Body luôn
                closestTarget = targetBody != null ? targetBody : p.transform;
            }
        }

        if (closestTarget != null)
        {
            if (currentState != AIState.Chase)
            {
                Debug.Log($"<color=red>[EnemyAI] Đã phát hiện {closestTarget.name}! Bắt đầu RƯỢT!</color>");
                if (waitCoroutine != null) StopCoroutine(waitCoroutine);
                isWaiting = false;
            }

            currentState = AIState.Chase;
            targetPlayer = closestTarget;
        }
        else if (currentState == AIState.Chase)
        {
            currentState = AIState.Idle;
            startPosition = rootObj.position;
            targetPlayer = null;
        }
    }

    private void HandleWander()
    {
        if (isWaiting || rootObj == null) return;
        if (wanderSpeed <= 0) wanderSpeed = 1.5f;

        Vector3 targetPos = new Vector3(wanderTarget.x, wanderTarget.y, rootObj.position.z);
        rootObj.position = Vector3.MoveTowards(rootObj.position, targetPos, wanderSpeed * Time.deltaTime);

        Vector3 direction = (targetPos - rootObj.position).normalized;
        UpdateFacingDirection(direction);
        PlayAnimation("Walk");

        if (Vector3.Distance(rootObj.position, targetPos) < 0.1f)
        {
            if (!isWaiting) waitCoroutine = StartCoroutine(WaitRoutine());
        }
    }

    private void HandleChase()
    {
        if (targetPlayer == null || rootObj == null) return;

        if (chaseSpeed <= 0) chaseSpeed = 3.5f;

        // Điểm đích bây giờ chính là Body của Hero, nên quái sẽ rượt chuẩn 100%
        Vector3 targetPos = new Vector3(targetPlayer.position.x, targetPlayer.position.y, rootObj.position.z);

        // KIỂM TRA ĐỤNG NHAU (VA CHẠM) ĐỂ KÍCH HOẠT COMBAT
        if (Vector2.Distance(rootObj.position, targetPos) <= engageDistance)
        {
            if (cbz != null && !cbz.isInCombat)
            {
                Debug.Log("<color=orange>[EnemyAI] Đã bắt được Player! Kích hoạt Combat Zone!</color>");
                cbz.StartCombatEncounter(); // Gọi hàm khởi động từ Combat Zone
            }
            PlayAnimation("Idle"); // Đứng im chờ load trận
            return;
        }

        rootObj.position = Vector3.MoveTowards(rootObj.position, targetPos, chaseSpeed * Time.deltaTime);

        Vector3 direction = (targetPos - rootObj.position).normalized;
        UpdateFacingDirection(direction);

        PlayAnimation("Chase");
    }

    private void HandleIdle()
    {
        PlayAnimation("Idle");
        if (!isWaiting) waitCoroutine = StartCoroutine(WaitRoutine());
    }

    private IEnumerator WaitRoutine()
    {
        isWaiting = true;
        currentState = AIState.Idle;

        yield return new WaitForSeconds(waitTime);

        GetNextWanderTarget();
        currentState = AIState.Wander;
        isWaiting = false;
    }

    private void GetNextWanderTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
    }

    private void UpdateFacingDirection(Vector3 direction)
    {
        if (direction.x != 0 && sr != null)
        {
            sr.flipX = direction.x < 0;
        }
    }

    private void PlayAnimation(string animName)
    {
        if (anim == null || currentAnimName == animName) return;

        anim.Play(animName);
        currentAnimName = animName;
    }

    private void OnDrawGizmosSelected()
    {
        Transform myBody = transform.Find("Body");
        if (myBody == null && transform.parent != null) myBody = transform.parent.Find("Body");

        Vector3 centerPos = myBody != null ? myBody.position : transform.position;

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(centerPos, aggroRange); // Vẽ vòng tròn đỏ tại Body

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : centerPos, wanderRadius);
    }
}