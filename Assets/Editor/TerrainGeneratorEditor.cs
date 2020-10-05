using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    // Start is called before the first frame update
    TerrainGenerator terrainGenerator;
    Editor terrainGeneratorEditor;
    bool autoUpdate = true;
    int _choiceIndex = 0;
    int _loadChoiceIndex = 0;
    static string m_AssetPathString = "TerrainAssets";

    void CreateNewTerrainData(in string terrainDataName)
    {
        
        _choiceIndex = GetTerrain.Count()-1;
        Terrain newTerrain = terrainGenerator.CreateTerrain(terrainDataName);
        GetTerrain.Add(newTerrain);
        terrainGenerator.ActiveTerrainDataSettingsChanged();
    }


    void DeleteTerrainData(in int i)
    {
        if (GetTerrain.Count > 0)
        {
            GetTerrain[i].DeleteTerrain();
            GetTerrain.RemoveAt(i);
            if (i > GetTerrain.Count - 1)
            {
                _choiceIndex = GetTerrain.Count - 1;
            }
        }
        
    }
    
    void ClearTerrain(in int i)
    {
        if (GetTerrain.Count > 0)
        {
            GetTerrain[i].ResetTerrain();
        }
        
    }
    int cachedIndex = -1;
    string newTerrainDataName = "New TerrainData";
    public override void OnInspectorGUI()
    {
        cachedIndex = _choiceIndex;
        DrawDefaultInspector();
        if (_choiceIndex > GetTerrain.Count-1 || _choiceIndex < 0)
        {
            _choiceIndex = GetTerrain.Count - 1;
        }
        GUILayout.Label("Save or load terrain");
        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Export terrain")) 
        {
            if (HasValidTerrainSelected())
            {
                int chunkNum = 0;
                foreach(Chunk chunk in GetTerrain[_choiceIndex].m_TerrainChunks) 
                {
                    string assetPath = "Assets/Resources/" + m_AssetPathString + "/chunkMesh" + chunkNum.ToString() + ".asset";
                    AssetDatabase.CreateAsset(chunk.GetMesh(), assetPath);
                    chunkNum++;
                }
                AssetDatabase.Refresh();
                DeleteTerrainData(_choiceIndex);
            }
            
        }

        if (GUILayout.Button("Import Terrain"))
        {
            string assetPath = "Assets/Resources/" + m_AssetPathString;
            Object[] assets = Resources.LoadAll<Mesh>(m_AssetPathString);
            if (assets.Length > 0)
            {
                GameObject chunkHolder = new GameObject(name = "MeshHolder");
                for (int i = 0; i < assets.Length; i++) 
                {
                    GameObject newGameObject = new GameObject();
                    MeshFilter meshFilter = (MeshFilter)newGameObject.AddComponent(typeof(MeshFilter));
                    MeshRenderer meshRenderer = (MeshRenderer)newGameObject.AddComponent(typeof(MeshRenderer));
                    MeshCollider meshCollider = (MeshCollider)newGameObject.AddComponent(typeof(MeshCollider));
                    meshFilter.sharedMesh = (Mesh)assets[i];
                    meshCollider.sharedMesh = (Mesh)assets[i];
                    meshRenderer.material = terrainGenerator.GetMeshMaterial;
                    newGameObject.transform.SetParent(chunkHolder.transform);
                    newGameObject.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            else 
            {
                Debug.Log("Failed to find anything in folder to load! Are you sure there's meshes to load in Assets/Resources/TerrainAssets?");
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("Choose or delete terrain");
        GUILayout.BeginHorizontal("box");

        _choiceIndex = EditorGUILayout.Popup("Current Terrain", _choiceIndex, _terrainData.Select(x=>x.name).ToArray());

        if (cachedIndex != _choiceIndex && GetTerrain.Count > 0) 
        {
            terrainGenerator.LoadTerrainForEditing(GetTerrain[_choiceIndex]);
        }

        if (GUILayout.Button("Clear Terrain"))
        {
            ClearTerrain(_choiceIndex);
        }

        if (GUILayout.Button("Delete Terrain"))
        {
            DeleteTerrainData(_choiceIndex);
        }

        GUILayout.EndHorizontal();
        GUILayout.Label("Create new terrain");

        GUILayout.BeginHorizontal("box");
        newTerrainDataName = GUILayout.TextField(newTerrainDataName, 25, GUILayout.Width(200.0f));

        if (GUILayout.Button("Create New Terrain"))
        {
            if (!newTerrainDataName.Equals(""))
            {
                CreateNewTerrainData(newTerrainDataName);
                newTerrainDataName = "";    
            }
        }
        if (_choiceIndex > GetTerrain.Count - 1 || _choiceIndex < 0)
        {
            _choiceIndex = GetTerrain.Count - 1;
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Update Terrain");

        if (GUILayout.Button("Update"))
        {
            terrainGenerator.ActiveTerrainDataSettingsChanged();
        }
        autoUpdate = GUILayout.Toggle(autoUpdate, "Auto-Update Terrain");
        GUILayout.EndHorizontal();



        if (GetTerrain.Count > 0)
        {  
            DrawSettingsEditor(GetTerrain[_choiceIndex], ref terrainGeneratorEditor);
        } 


    }

    void DrawSettingsEditor(Object settings, ref Editor editor) 
    {
        if (settings != null) 
        {
            //_choiceIndex = EditorGUILayout.Popup("Label", _choiceIndex, _choices);
            //terrain.selectedTerrainData = terrain.terrainData[_choiceIndex];
            bool foldout = EditorGUILayout.InspectorTitlebar(true, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();
                }
                if (check.changed && autoUpdate)
                {
                    terrainGenerator.ActiveTerrainDataSettingsChanged();
                }
            }
        }
    }

    bool HasValidTerrainSelected() 
    {
        return _choiceIndex > -1 && _choiceIndex < GetTerrain.Count && GetTerrain.Count > 0;
    }

    public static object GetObjectsAtPath (string path) 
    {
        ArrayList al = new ArrayList();
        string pathData = Application.dataPath + "/Resources/" + path;
        string[] fileEntries = Directory.GetFiles(pathData);
        string[] assetNames = fileEntries.Select(x => x.Remove(0, pathData.Length)).ToArray();
        for(int i = 0; i < assetNames.Length; i++)
        {

            string assetPath = path + assetNames[i];
            Object[] t = Resources.LoadAll(assetPath);

            if (t != null)
                al.Add(t);
        }
        object[] result = new object[al.Count];
        for (int i = 0; i < al.Count; i++)
            result[i] = al[i];

        return result;
    }


    private void OnEnable()
    {
        terrainGenerator = (TerrainGenerator)target;
        _terrainData.Clear();
        for (int i = GetTerrain.Count-1; i > 0; i--)
        {
            if (GetTerrain[i] == null)
            {
                GetTerrain.RemoveAt(i);
            }
        }
        foreach (Terrain terrain in FindObjectsOfType<Terrain>())
        {
            _terrainData.Add(terrain);
        }
        if (GetTerrain.Count > 0) 
        {
            terrainGenerator.LoadTerrainForEditing(GetTerrain[_choiceIndex]);
        }
    }

    private List<Terrain> GetTerrain
    {
        get => _terrainData;
    }
    private List<Terrain> _terrainData = new List<Terrain>();
}
