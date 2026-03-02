using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyMoveToCombat : DucMonobehaviour
{
    public Transform targetPosition;  // V? tr¨ª m¨¤ enemy s? nh?y t?i khi combat b?t ??u
    public float jumpHeight = 2f;     // ?? cao c?a c¨² nh?y
    public float jumpDuration = 1f;   // Th?i gian th?c hi?n c¨² nh?y

    // H¨¤m g?i khi combat b?t ??u
    public void StartCombat()
    {

        if (this.targetPosition != null)
        {
            Debug.Log("Combat start position is set.");
        }
        else
        {
            Debug.LogError("Target position is not assigned!");
        }
        JumpToCombatPosition();
    }

    private void JumpToCombatPosition()
    {
        this.transform.DOMove(targetPosition.position, jumpDuration).SetEase(Ease.InOutQuad)
       .OnComplete(() =>
       {
            // Ki?m tra l?i c¨¢c ??i t??ng tr??c khi th?c hi?n h¨¤nh ??ng trong callback
            if (targetPosition != null)
           {
               //Debug.Log("Target position is valid!");
               StartCombat();
           }
           else
           {
               Debug.LogError("Target position is null in OnComplete!");
           }
       });
    }
}
