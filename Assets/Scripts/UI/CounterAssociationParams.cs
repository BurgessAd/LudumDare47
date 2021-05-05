using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CounterImageAssociator")]
public class CounterAssociationParams : ScriptableObject
{
    [SerializeField]
    private List<Sprite> CounterFirstImages;
    [SerializeField]
    private List<EntityInformation> m_CounterFirstImageAssociatedType;
    public bool GetTextureFromAnimalType(EntityInformation entityInformation, out Sprite outTex) 
    {
        outTex = null;
        for (int i = 0; i < m_CounterFirstImageAssociatedType.Count; i++) 
        {
            if (m_CounterFirstImageAssociatedType[i] == entityInformation) 
            {
                outTex = CounterFirstImages[i];
                return true;
            }
        }
        return false;
    }

    [SerializeField]
    private List<Sprite> CounterSecondImages;
    [SerializeField]
    private List<CounterType> m_CounterSecondImageAssociatedType;

    public bool GetTextureFromGoalType(in CounterType counterType, out Sprite outTex)
	{
        outTex = null;
        for (int i = 0; i < m_CounterSecondImageAssociatedType.Count; i++)
        {
            if (m_CounterSecondImageAssociatedType[i] == counterType)
            {
                outTex = CounterSecondImages[i];
                return true;
            }
        }
        return false;
    }
}
