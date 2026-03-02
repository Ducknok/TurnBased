using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DoTween for smooth animations
using UnityEngine.SceneManagement;

public class CombatZone : DucMonobehaviour
{
    public CombatStateMachine cbm;
    public CameraController cameraCtrl;
    [Header("Combat Settings")]
    public Transform[] playerPositions;   // Set positions for players during combat
    public Transform[] enemyPositions;    // Set positions for enemies during combat
    public Transform centerPosition;
    [Header("Player and Enemy References")]
    public GameObject[] heros;
    public GameObject[] enemies;          // Array of enemies
    protected GameObject[] bodies;
    [Header("Combat Flow Settings")]
    public float movementDuration = 1f;   // Duration for moving characters to combat positions
    public float jumpHeight = 2f;         // Jump height for entering combat
    public bool isInCombat = false;      // State to check if combat is active

    [System.Obsolete]
    protected override void Awake()
    {
        this.cbm = FindObjectOfType<CombatStateMachine>();
        StartCoroutine(WaitSetCameraController());
    }
    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.cbm = FindObjectOfType<CombatStateMachine>();
        StartCoroutine(WaitSetCameraController());
    }
    private IEnumerator WaitSetCameraController()
    {
        yield return null; // đợi 1 frame
        this.cameraCtrl = CameraController.Instance;
    }
    private void LoadHeroList()
    {

        var players = this.cbm.playersInCombat;

        this.heros = new GameObject[players.Count];
        this.bodies = new GameObject[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            GameObject hero = players[i];
            this.heros[i] = hero;

            if (hero == null)
            {
                Debug.LogWarning($"Hero {i} is null.");
                continue;
            }

            Transform bodyTransform = hero.transform.Find("Body");
            if (bodyTransform != null)
            {
                bodies[i] = bodyTransform.gameObject;
                // Debug.Log($"Body found for {hero.name}: {bodyTransform.name}");
            }
            else
            {
                Debug.LogWarning($"Body not found for hero {hero.name}");
            }
            // 🔽 Gọi HeroPosition() ở đây
            var heroSM = hero.GetComponent<HeroStateMachine>();
            if (heroSM != null)
            {
                heroSM.HeroPosition(); // 👈 Gọi hàm bạn đã viết
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy HeroCombatStateMachine trên {hero.name}");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Body") && !isInCombat)
        {
            isInCombat = true;
            StartCoroutine(InitiateCombat());
        }
    }
    private IEnumerator InitiateCombat()
    {
        //Debug.Log("Combat Started!");
        this.LoadHeroList();
        CameraController.Instance.SetCameraForCombat(this.centerPosition);
        // Kiểm tra kích thước mảng trước khi di chuyển
        if (heros.Length > playerPositions.Length || enemies.Length > enemyPositions.Length)
        {
            yield break;
        }

        // Move players
        for (int i = 0; i < heros.Length; i++)
        {
            //Debug.Log($"Player {i} move to {playerPositions[i].position}");
            MoveToPosition(bodies[i].transform, playerPositions[i].position);
        }

        // Move enemies
        for (int i = 0; i < enemies.Length; i++)
        {
            MoveToPosition(enemies[i].transform, enemyPositions[i].position);
        }

        yield return new WaitForSeconds(movementDuration + 0.1f);

        StartCombat();
    }
    // Move a character to a target position with a jump animation
    private void MoveToPosition(Transform character, Vector3 targetPosition)
    {
        //Debug.Log($"Moving {character.name} from {character.position} to {targetPosition}");
        Vector3[] path = new Vector3[]
        {
        character.position,
        new Vector3((character.position.x + targetPosition.x) / 2, targetPosition.y + jumpHeight, 0),
        targetPosition
        };

        character.DOPath(path, movementDuration, PathType.CatmullRom).SetEase(Ease.InOutQuad); // .OnComplete( /*=> Debug.Log($"{character.name} reached {targetPosition}")*/);
    }

    // Logic to handle the actual combat after positioning
    private void StartCombat()
    {

        this.isInCombat = true;
        foreach (var hero in PlayerController.Instance.HeroSMList)
        {
            hero.anim.SetFloat("Speed", 0);
            hero.anim.SetBool("IdleBattle", this.isInCombat);
        }
    }
    // End the combat
    public void EndCombat()
    {
        Debug.Log("Combat Ended!");
        isInCombat = false;
        foreach(var hero in PlayerController.Instance.HeroSMList)
        {
            hero.anim.SetBool("IdleBattle", this.isInCombat);
        }
        this.cbm.combatState = CombatStateMachine.PerformAction.WAIT;
        this.cbm.playerInput = CombatStateMachine.PlayerGUI.ACTIVATE;
        cameraCtrl.SetCameraFollowHero(PartyManager.Instance.currentLeader.transform.parent.GetComponent<HeroStateMachine>());
        this.DisActiveObject();
        // Reset positions, handle rewards, etc.
    }
    private void DisActiveObject()
    {
        this.gameObject.SetActive(false);
    }
    private void ActiveObject()
    {
        this.gameObject.SetActive(true);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var pos in playerPositions)
        {
            if (pos != null) Gizmos.DrawSphere(pos.position, 0.2f);
        }

        Gizmos.color = Color.red;
        foreach (var pos in enemyPositions)
        {
            if (pos != null) Gizmos.DrawSphere(pos.position, 0.2f);
        }
    } 
}
