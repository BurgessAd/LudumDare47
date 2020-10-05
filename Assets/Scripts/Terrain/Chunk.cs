using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]

public class Chunk : MonoBehaviour
{
	private MeshFilter meshFilter;
	private MeshCollider meshCollider;
	public Mesh mesh;

	public Vector3Int renderTo;

	public Vector3Int renderFrom;

	public bool shouldRerender = true;

	public float[] isoData;

	public Color[] colourData;

	public bool isInitialized = false;

	public ExportableChunkData GetExportableData() 
	{
		ExportableChunkData chunk = ScriptableObject.CreateInstance(typeof(ExportableChunkData)) as ExportableChunkData;
		chunk.colorData = colourData;
		chunk.isoData = isoData;
		return chunk;
	}

	public void LoadImportedData(ExportableChunkData chunk) 
	{
		colourData = chunk.colorData;
		isoData = chunk.isoData;
		isInitialized = true;
	}


	public void DestroyChunkAndData()
	{
		DestroyImmediate(gameObject);
	}
	public void SetMaterial(Material mat)
	{
		GetComponent<MeshRenderer>().sharedMaterial = mat;
	}

	public ref Mesh GetMesh() 
	{
		return ref mesh;
	}

	public void SetMesh(in Vector3[] vertices, in int[] triangles, in Color[] colours)
	{
		if (!mesh)
		{
			Awake();
		}
		mesh.Clear();
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetColors(colours);
		mesh.RecalculateNormals();
	}

	public void SetCollider()
	{
		meshCollider.sharedMesh = mesh;
	}

	//Make sure we have meshFilter, meshRenderer, meshCollider all set up, set the mesh for the chunk.
	//Reset colliders at the end of setup.
	public void Awake()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		mesh = new Mesh();
		meshFilter.sharedMesh = mesh;
	}
}
