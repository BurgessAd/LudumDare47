using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// each grass patch is responsible for the mesh vertices of the grass
// and their sizes
// we want to be able to add/remove grass vertices
// as well as change their maximum length
// we want to grab all vertices around a circle, and run them through a compute shader (? or multithreaded) in order to determine if we should affect them
// vertex color alpha is grass length proportional to total length
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GrassPatchComponent : MonoBehaviour
{
    [SerializeField] private float m_MaxGrassDensity;
    [SerializeField] private float m_GrassMapGenerationHeight;
    [SerializeField] private float m_GrassLength;
    [SerializeField] private LayerMask m_GrassGenerationLayer;
    [SerializeField] private Tuple<Vector3, Vector3> m_GrassGenerationBounds;
    [SerializeField] private Texture2D m_GrassMap;
    [SerializeField] private bool m_bHasBoundsDefined;

    [SerializeField] private Texture2D m_MapResizerTex;
    [SerializeField] private GameObject m_MapVisualizerPrefab;


    float m_lastGrassLength = -1;
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Vector3> normals = new List<Vector3>();
    private readonly List<Color32> colors = new List<Color32>();
    private MaterialPropertyBlock m_PropertyBlock;
    private MeshRenderer m_MeshRenderer;

    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        m_PropertyBlock = new MaterialPropertyBlock();
    }
	#region getters and setters
    public Texture2D GrassMap { get => m_GrassMap; set { m_GrassMap = value; m_bHasBoundsDefined = true; } }
    public float GrassMapGenerationAttemptHeight { get => m_GrassMapGenerationHeight; set { m_GrassMapGenerationHeight = value; } }
    public LayerMask GrassGenerationLayer { get => m_GrassGenerationLayer; set { m_GrassGenerationLayer = value; } }
    public float MaxGrassDensity { get => m_MaxGrassDensity; set { m_MaxGrassDensity = value; } }
    public Tuple<Vector3, Vector3> GrassGenerationBounds { get => m_GrassGenerationBounds; set { m_GrassGenerationBounds = value; } }
    public bool HasBoundsDefined { get => m_bHasBoundsDefined; }
    #endregion

	public void Update()
	{
        if (m_lastGrassLength != m_GrassLength) 
        {

            m_MeshRenderer.GetPropertyBlock(m_PropertyBlock);
            m_PropertyBlock.SetFloat("", m_GrassLength);
            m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
            m_lastGrassLength = m_GrassLength;
        }

	}

	public GameObject CreateGrassPaintVisualizer(in Vector3 pos)
	{
        return Instantiate(m_MapVisualizerPrefab, pos, Quaternion.AngleAxis(-90, new Vector3(1, 0, 0)), null);
	}

    public void DeleteGrassPaintVisualizer(GameObject visualizer) 
    {
        DestroyImmediate(visualizer);
    }

    // define longest tex size as 720
    static readonly float maxPixelLength = 720;

    public void UpdateGrassPaintVisualizer(in SpriteRenderer spriteRenderer) 
    {
        Vector3 worldOffset = GrassGenerationBounds.Item2 - GrassGenerationBounds.Item1;
        float maxSize = Mathf.Max(Mathf.Abs(worldOffset.x), Mathf.Abs(worldOffset.z));
        float pixelToWorldScale = maxSize / maxPixelLength;

        spriteRenderer.sprite = Sprite.Create(m_MapResizerTex, new Rect(0, 0, (int)m_GrassMap.width, (int)m_GrassMap.height), new Vector2(0, 1), pixelToWorldScale);
    }

    int i = 0;
    public void UpdateGrassSizeVisualizer(in SpriteRenderer spriteRenderer, in Vector3 anchorA, in Vector3 anchorB) 
    {

        float maxResizerPixelLen = Mathf.Max(m_MapResizerTex.width, m_MapResizerTex.height);


        Vector3 worldOffset = anchorB - anchorA;
        float maxWorldSize = Mathf.Max(Mathf.Abs(worldOffset.x), Mathf.Abs(worldOffset.z));
        float worldToPixelScale = maxResizerPixelLen / maxWorldSize;

        // if we're in the negatives, reverse the pivots
        Vector2Int pivotPoints = new Vector2Int(worldOffset.x > 0 ? 0 : 1, worldOffset.y > 0 ? 0 : 1);



        Vector3 texSize = worldToPixelScale * worldOffset;
        i++;
        if (i == 10) 
        {
            i = 0;
            Debug.Log(maxResizerPixelLen);
            Debug.Log(texSize);
            Debug.Log(worldToPixelScale);
        }
        


        spriteRenderer.sprite = Sprite.Create(m_MapResizerTex, new Rect(0, 0, (int)Mathf.Abs(texSize.x), (int)Mathf.Abs(texSize.z)), pivotPoints, worldToPixelScale);
    }

	public void DestroyGrassObject() 
    {
        Destroy(gameObject);
    }

    public void CreateGrassFromParams() 
    {
        // each pixel represents density within each pixel
        // but this density must be re-mapped to a density per unit area, as the texture is always 256x256
        // we need to calculate scale factor - use the world-space rect

        float generationBoundsXSize_ws = m_GrassGenerationBounds.Item2.x - m_GrassGenerationBounds.Item1.x;
        float generationBoundsZSize_ws = m_GrassGenerationBounds.Item2.z - m_GrassGenerationBounds.Item1.z;

        float pixelSize_ws = (m_GrassGenerationBounds.Item2.x - m_GrassGenerationBounds.Item1.x ) / m_GrassMap.width;
        // density is grass / unit area: figure out number of blades of grass in each pixel in world space.
        float bladesPerFullPixel = m_MaxGrassDensity * Mathf.Pow(pixelSize_ws, 2);

        // iterate over each pixel in texture, map it to a size in world space, and populate randomly with raycasts based on the density required.

        float bladesPseudoRand = 0;

        Color[] pixels = m_GrassMap.GetPixels();
        vertices.Clear();
        for (int z = 0; z < m_GrassMap.height; z++)
        {
            float zPos = ((float)z / m_GrassMap.height) * generationBoundsZSize_ws + m_GrassGenerationBounds.Item1.z;
            for (int x = 0; x < m_GrassMap.width; x++)
            {
                float xPos = ((float)x / m_GrassMap.width) * generationBoundsXSize_ws + m_GrassGenerationBounds.Item1.x;
                // get xPos of left side, and yPos of bottom side
           

                // left to right, top to bottom (row after row), so each z up is a width's worth
                int pixelIndex = z * m_GrassMap.width + x;
                Color pixelColor = pixels[pixelIndex];

                float bladesForPixel = pixelColor.r * bladesPerFullPixel;
                float heightForPixel = pixelColor.g * m_GrassMapGenerationHeight;

                bladesPseudoRand += bladesForPixel;
                while(bladesPseudoRand > 0) 
                {
                    float xBladePos = xPos + UnityEngine.Random.Range(0, pixelSize_ws);
                    float zBladePos = zPos + UnityEngine.Random.Range(0, pixelSize_ws);
                    if (!PlaceGrassBladeAtPos(xBladePos, zBladePos, heightForPixel)) 
                    {
                        Debug.LogWarning("Attempt to place grass blade failed - no raycast hit");
                    }
                    bladesPseudoRand--;
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        int[] indices = new int[vertices.Count];
        for (int i = 0; i < vertices.Count; i++) 
        {
            indices[i] = i;
        }
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        GetComponent<MeshFilter>().mesh = mesh;
    }

	private bool PlaceGrassBladeAtPos(in float x, in float y, in float height) 
    {
        if (Physics.Raycast(new Vector3(x, m_GrassMapGenerationHeight, y), -Vector3.up, out RaycastHit hit, Mathf.Infinity, m_GrassGenerationLayer)) 
        {
            vertices.Add(hit.point);
            colors.Add(new Color(height, 0, 0, 0));
            return true;
        }
        return false;
    }
}
