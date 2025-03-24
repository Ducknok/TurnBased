using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LockSystem
{
    public List<BaseAttack.Effect> requiredTypes = new List<BaseAttack.Effect>();

    public LockSystem(List<BaseAttack.Effect> attackTypes)
    {
        this.requiredTypes = new List<BaseAttack.Effect>(attackTypes);
    }

    public void TryBreakLock(BaseAttack.Effect attackType1, BaseAttack.Effect attackType2)
    {
        // Kiểm tra và xóa attackType1 nếu có
        if (requiredTypes.Contains(attackType1))
        {
            requiredTypes.Remove(attackType1);
        }
        // Kiểm tra và xóa attackType1 nếu có
        if (attackType2 != attackType1 && requiredTypes.Contains(attackType2))
        {
            requiredTypes.Remove(attackType2);
        }
    }

    public bool IsBroken()
    {
        return requiredTypes.Count == 0; // Lock bị phá nếu không còn AttackType nào
    }
}
