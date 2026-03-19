using Cinemachine;
using UnityEngine;

public abstract class TakeDamageController : DucMonobehaviour
{
    public abstract void TakeDamage(GameObject target, int getDamageAmount);
    public abstract void TakeDamage(GameObject target, int getDamageAmount, BaseAttack.Effect attackType1, BaseAttack.Effect attackType2);

}
