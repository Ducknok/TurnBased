using Cinemachine;
using UnityEngine;

public abstract class TakeDamageController : MonoBehaviour
{
    public abstract void TakeDamage(GameObject target, float getDamageAmount);
    public abstract void TakeDamage(GameObject target, float getDamageAmount, BaseAttack.Effect attackType1, BaseAttack.Effect attackType2);

}
