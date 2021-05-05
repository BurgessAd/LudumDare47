using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectColorChanger))]
public class ObjectColourChangerEditor : Editor
{
    ObjectColorChanger colorChanger;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        colorChanger.RandomizeOnStart = GUILayout.Toggle(colorChanger.RandomizeOnStart, "Randomize On Start");
        EditorGUILayout.LabelField("Animation References", EditorStyles.boldLabel);
        ref List<ObjectColorChangeMaterialSetting> materialColourSettings = ref colorChanger.GetMaterialColourSettings();
        string[] choices = new string[materialColourSettings.Count];
        for (int i = 0; i < materialColourSettings.Count; i++)
        {
            choices[i] = "Material " +  materialColourSettings[i].m_RendererMaterialNum.ToString();
        }
        int chosenIndex = colorChanger.m_MaterialColourSettingReference;
        chosenIndex = Mathf.Clamp(chosenIndex, 0, materialColourSettings.Count);
        chosenIndex = EditorGUILayout.Popup(chosenIndex, choices);
        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(chosenIndex == materialColourSettings.Count - 1 && materialColourSettings.Count > 0))
            {
                if (GUILayout.Button("Next Setting"))
                {
                    chosenIndex++;
                }
            }
            using (new EditorGUI.DisabledScope(chosenIndex == 0))
            {
                if (GUILayout.Button("Previous Setting"))
                {
                    chosenIndex--;
                }
            }
            using (new EditorGUI.DisabledScope(materialColourSettings.Count == 0))
            {
                if (GUILayout.Button("Delete Setting"))
                {
                    materialColourSettings.RemoveAt(chosenIndex);
                }
            }

            if (GUILayout.Button("Add Setting"))
            {
                if (materialColourSettings.Count == 0 || chosenIndex == materialColourSettings.Count)
                {
                    materialColourSettings.Add(new ObjectColorChangeMaterialSetting());
                }
                else
                {
                    materialColourSettings.Insert(chosenIndex + 1, new ObjectColorChangeMaterialSetting());
                }
            }
        }

        colorChanger.m_MaterialColourSettingReference = chosenIndex;

        EditorGUILayout.Space();

        if (materialColourSettings.Count > 0)
        {
            ObjectColorChangeMaterialSetting clip = materialColourSettings[chosenIndex];

            clip.m_RendererMaterialNum = EditorGUILayout.IntField("Material Num", clip.m_RendererMaterialNum);

            SerializedObject serializedGradient = new SerializedObject(colorChanger);
            SerializedProperty colorGradient = serializedGradient.FindProperty("gradient");
            EditorGUILayout.PropertyField(colorGradient);
            serializedGradient.ApplyModifiedProperties();
        }

        if (!colorChanger.RandomizeOnStart) 
        {
            if (GUILayout.Button("Reroll Colour"))
            {
                colorChanger.ChangeColour();
            }
        }
    }

    void OnEnable()
    {
        colorChanger = (ObjectColorChanger) target;
    }
}
