using UnityEngine;

public abstract class AttackComponentBase : MonoBehaviour
{
    public abstract void AttackTarget(in GameObject target);
}
