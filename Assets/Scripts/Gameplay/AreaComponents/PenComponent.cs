using System.Collections.Generic;
using UnityEngine;

public class PenComponent : MonoBehaviour
{
    [SerializeField]
    private CowGameManager m_Manager;

    [SerializeField]
    private List<string> m_PennableAnimalTags;

    private void OnTriggerEnter(Collider objCollided)
    {
        if (IsObjectPennable(objCollided.gameObject))
        {
            m_Manager.OnCowEnterGoal(objCollided.gameObject);
        }
    }

    private void OnTriggerExit(Collider objCollided)
    {
        if (IsObjectPennable(objCollided.gameObject)) 
        {
            m_Manager.OnCowLeaveGoal(objCollided.gameObject);
        }
    }

    private bool IsObjectPennable(in GameObject targetGameObject) 
    {
        if (targetGameObject.TryGetComponent(out GameTagComponent gameTagComponent))
        {
            for (int i = 0; i < m_PennableAnimalTags.Count; i++) 
            {
                if (gameTagComponent.GetObjectTag == m_PennableAnimalTags[i]) 
                {
                    return true;
                }
            }
        }
        return false;
    }
}
