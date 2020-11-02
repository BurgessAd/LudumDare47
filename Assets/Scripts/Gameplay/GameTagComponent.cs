using UnityEngine;
public class GameTagComponent : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private string m_ObjectTag;
    public ref string GetObjectTag => ref m_ObjectTag;
}
