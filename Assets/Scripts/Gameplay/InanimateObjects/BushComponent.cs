using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(FoodSourceComponent))]
public class BushComponent : MonoBehaviour, IFoodSourceSizeListener
{
    [Header("Animation and Generation params")]
    [SerializeField] private float m_TimeForFlowerAnim;
    [SerializeField] private uint m_TargetNumOfFlowers;
    [SerializeField] private float m_MaxAngleFromUp;
    [SerializeField] private float m_FlowerSizeRandom;
    [SerializeField] private float m_FlowerScalar;

    [Header("Object references")]
    [SerializeField] private GameObject m_BerryPrefab;

    private class Flower 
    {
        public float m_SizeScalar;
        public float m_CurrentSize;
        public float m_SizeChangeVelocity;
        public Transform m_FlowerTransform;
    }

    private float m_CurrentFoodSize = 0.0f;
    private readonly List<Flower> m_Flowers = new List<Flower>();

    static void AddMeshAndTransformDataForUse(in List<Mesh> meshes, in List<Transform> transforms, GameObject gameObjectToAdd) 
    {

        MeshFilter meshFilter = gameObjectToAdd.GetComponent<MeshFilter>();
        if (!meshFilter)
            return;
        transforms.Add(gameObjectToAdd.transform);
        meshes.Add(meshFilter.mesh);
    }

	private struct Triangle 
    {
        public Vector3 cornerA;
        public Vector3 cornerB;
        public Vector3 cornerC;

        public float CalculateArea() 
        {
            float ang = Vector3.Angle(cornerC - cornerA, cornerB - cornerA);
            return 0.5f * (cornerC - cornerA).magnitude * (cornerB - cornerA).magnitude * Mathf.Sin(Mathf.Deg2Rad * ang);
        }

        public Vector3 GetRandomPosOnTri()
        {
            Vector3 one = cornerB - cornerA;
            Vector3 two = cornerC - cornerA;
            float randA = UnityEngine.Random.Range(0.0f, 1.0f);
            float randB = UnityEngine.Random.Range(0.0f, 1.0f);
            if (randA + randB > 1.0f)
            {
                randA = 1 - randA;
                randB = 1 - randB;
            }
            Vector3 transformed = randA * one + randB * two;
            return cornerA + transformed;
        }

        public Vector3 GetNorm()
        {
            return Vector3.Normalize(Vector3.Cross(cornerB - cornerA, cornerC - cornerA));
        }
    }

    void ForEachValidTriInMesh(in Mesh mesh, in Transform transform, Action<Triangle> func)
    {
        Triangle triangle = new Triangle();
        for (int j = 0; j < mesh.triangles.Length - 2; j += 3)
        {
            triangle.cornerA = transform.rotation * Vector3.Scale(transform.localScale, mesh.vertices[mesh.triangles[j]]) + transform.position;
            triangle.cornerB = transform.rotation * Vector3.Scale(transform.localScale, mesh.vertices[mesh.triangles[j + 1]]) + transform.position;
            triangle.cornerC = transform.rotation * Vector3.Scale(transform.localScale, mesh.vertices[mesh.triangles[j + 2]]) + transform.position;

            Vector3 spawnUp = triangle.GetNorm();
            float angFromUp = Vector3.Angle(spawnUp, Vector3.up);

            if (angFromUp > m_MaxAngleFromUp)
                continue;

            func(triangle);
        }
    }

	private void Awake()
	{
        GetComponent<FoodSourceComponent>().AddListener(this);

        List<Mesh> meshes = new List<Mesh>();
        List<Transform> transforms = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++) 
        {
            AddMeshAndTransformDataForUse(meshes, transforms, transform.GetChild(i).gameObject);
        }

        AddMeshAndTransformDataForUse(meshes, transforms, gameObject);

        float validArea = 0.0f;
        int triNum = 0;

        for (int i = 0; i < meshes.Count; i++) 
        {
            ForEachValidTriInMesh(meshes[i], transforms[i], (Triangle tri) =>
            {
                triNum++;
                validArea += tri.CalculateArea();
            });
        }

        float areaPerTri = validArea / triNum;
        float currentArea = 0;

        for (int i = 0; i < meshes.Count; i++) 
        {
            ForEachValidTriInMesh(meshes[i], transforms[i], (Triangle tri) =>
            {
                currentArea += tri.CalculateArea();
                while (currentArea > areaPerTri) 
                {
                    currentArea -= areaPerTri;

                    Vector3 spawnPos = tri.GetRandomPosOnTri();
                    Quaternion spawnQuat = Quaternion.LookRotation(tri.GetNorm(), Vector3.up);
                    GameObject newFlower = Instantiate(m_BerryPrefab, spawnPos, spawnQuat);

                    Flower flower = new Flower
                    {
                        m_FlowerTransform = newFlower.transform,
                        m_CurrentSize = 1.0f,
                        m_SizeScalar = UnityEngine.Random.Range(1 - m_FlowerSizeRandom, 1 + m_FlowerSizeRandom),
                        m_SizeChangeVelocity = 0.0f
                    };
                    flower.m_FlowerTransform.position = spawnPos;
                    flower.m_FlowerTransform.parent = transform;

                    m_Flowers.Add(flower);
                }
            });
        }
    }

	public void OnSetFoodSize(float foodSize)
	{
        if (m_CurrentFoodSize != foodSize) 
        {
            m_CurrentFoodSize = foodSize;
            enabled = true;
        }
    }

    void Update()
    {
        bool allFlowersAtTarget = true;
        for (int i = 0; i < m_Flowers.Count; i++) 
        {
            float target = (float)i / m_Flowers.Count <= m_CurrentFoodSize ? 1.0f : 0.0f;

            if (Mathf.Abs(m_Flowers[i].m_CurrentSize - target) < 0.001f)
                continue;
            allFlowersAtTarget = false;
            m_Flowers[i].m_CurrentSize = Mathf.SmoothDamp(m_Flowers[i].m_CurrentSize, target, ref m_Flowers[i].m_SizeChangeVelocity, m_TimeForFlowerAnim);
            m_Flowers[i].m_FlowerTransform.localScale = m_Flowers[i].m_SizeScalar * m_Flowers[i].m_CurrentSize * m_FlowerScalar * Vector3.one;
        }
        if (allFlowersAtTarget)
            enabled = false;
    }
}
