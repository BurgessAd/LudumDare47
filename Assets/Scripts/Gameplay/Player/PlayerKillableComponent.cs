using UnityEngine;
public interface IKillableComponent 
{
    void OnKilled();
}
public class PlayerKillableComponent : MonoBehaviour, IKillableComponent
{
    [SerializeField]
    private CowGameManager m_Manager;
    void IKillableComponent.OnKilled()
    {
        m_Manager.OnPlayerKilled();
    }
}
