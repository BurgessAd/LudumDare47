using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenBehaviour : MonoBehaviour
{

    public int CowsInPen = 0;
    public Material cowMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collision)
	{
        if (collision.gameObject.GetComponent<MeshRenderer>().material.name==cowMaterial.name + " (Instance)")
		{
            CowsInPen++;
		}
	}
    void OnTriggerExit(Collider collision)
    {
        
        if (collision.gameObject.GetComponent<MeshRenderer>().material.name == cowMaterial.name + " (Instance)")
        {
            CowsInPen--;
        }
    }
}
