using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectColorChanger : MonoBehaviour
{
    [SerializeField] private Gradient m_Choices;
    [SerializeField] private MeshRenderer m_MeshRenderer;
    [SerializeField] private string m_ShaderID;

    private MaterialPropertyBlock m_MatPropertyBlock;
    private Color m_SavedColor;

    private List<ObjectColorChangeMaterialSetting> m_ColourSettings = new List<ObjectColorChangeMaterialSetting>();
    [SerializeField]
    [HideInInspector]
    public int m_MaterialColourSettingReference;

    public ref List<ObjectColorChangeMaterialSetting> GetMaterialColourSettings() 
    {
        return ref m_ColourSettings;
    }

    public bool RandomizeOnStart { get; set; }

	private void Awake()
	{
        float rand = Random.Range(0f, 1f);
        m_MatPropertyBlock = new MaterialPropertyBlock();
        m_SavedColor = m_Choices.Evaluate(rand);
	}

    public void ChangeColour() 
    {

    }

    void Update()
    {
        m_MeshRenderer.GetPropertyBlock(m_MatPropertyBlock);
        m_MatPropertyBlock.SetColor(m_ShaderID, m_SavedColor);
        m_MeshRenderer.SetPropertyBlock(m_MatPropertyBlock);
    }
}

[System.Serializable]
public class ObjectColorChangeMaterialSetting 
{
    public Gradient m_ColourGradient;
    public int m_RendererMaterialNum;
}
