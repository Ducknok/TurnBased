using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DoTween for smooth animations

public class CombatZone : MonoBehaviour
{
    [SerializeField] private FollowerMovement followerMovement; // Reference to the FollowerMovement script
    [Header("Combat Settings")]
    public Transform[] playerPositions;   // Set positions for players during combat
    public Transform[] enemyPositions;    // Set positions for enemies during combat
   

    [Header("Player and Enemy References")]
    public GameObject[] players;          // Array of player characters
    public GameObject[] enemies;          // Array of enemies
    protected GameObject[] bodies;

    [Header("Combat Flow Settings")]
    public float movementDuration = 1f;   // Duration for moving characters to combat positions
    public float jumpHeight = 2f;         // Jump height for entering combat

    public bool isInCombat = false;      // State to check if combat is active



    // Trigger combat when player enters a combat zone
    protected void Awake()
    {
        // Khởi tạo mảng bodies với cùng kích thước mảng players
        this.bodies = new GameObject[players.Length];

        // Duyệt qua từng player để tìm object con "Body"
        for (int i = 0; i < players.Length; i++)
        {
            Transform bodyTransform = players[i].transform.Find("Body");
            if (bodyTransform != null)
            {
                bodies[i] = bodyTransform.gameObject;
                //Debug.Log($"Body found for player {players[i].name}: {bodies[i].name}");
            }
            else
            {
                Debug.LogWarning($"Body not found for player {players[i].name}");
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

        // Kiểm tra kích thước mảng trước khi di chuyển
        if (players.Length > playerPositions.Length || enemies.Length > enemyPositions.Length)
        {
            Debug.LogError("Not enough combat positions for all players or enemies.");
            yield break;
        }

        // Move players
        for (int i = 0; i < players.Length; i++)
        {
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
    }

    // End the combat
    public void EndCombat()
    {
        Debug.Log("Combat Ended!");
        isInCombat = false;
        this.followerMovement.JumpToFollowPosition(); // Reset follower position to leader's position
        // Reset positions, handle rewards, etc.
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
