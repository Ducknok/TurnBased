using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnOnCollision : Despawn
{
    [SerializeField] private LayerMask bodyLayer;
    [SerializeField] private float checkRadius = 0.3f;
    [SerializeField] private Transform checkPoint; // N?i ki?m tra va ch?m (th??ng l¨¤ ??u vi¨ºn ??n)
    [SerializeField] private float delayBeforeDespawn = 0.1f;

    private bool hasHit = false;

    private void Update()
    {
        Collider2D hit = Physics2D.OverlapCircle(checkPoint.position, checkRadius, bodyLayer);
        if (hit != null)
        {
            Invoke(nameof(DespawnObject), delayBeforeDespawn);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (checkPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(checkPoint.position, checkRadius);
        }
    }

    public override void DespawnObject()
    {
        VFXSpawner.Instance.Despawn(this.transform); 
    }

    protected override bool CanDespawn()
    {
        return false;
    }
}
