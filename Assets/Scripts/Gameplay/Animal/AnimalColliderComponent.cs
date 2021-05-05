using UnityEngine;

public class AnimalColliderComponent : MonoBehaviour
{
    [SerializeField] private AnimalComponent m_AnimalComponent;
    public AnimalComponent GetAnimalComponent => m_AnimalComponent;
}
