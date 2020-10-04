﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{

    public UIManager ui;
    public Material cowMaterial;
    public int CowsDead = 0;
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
        if (collision.gameObject.GetComponent<MeshRenderer>().material.name == cowMaterial.name + " (Instance)")
        {
            Destroy(collision.gameObject.transform.parent.gameObject);
            CowsDead++;
            ui.UpdateText();
        }
    }
    void OnTriggerExit(Collider collision)
    {

        if (collision.gameObject.GetComponent<MeshRenderer>().material.name == cowMaterial.name + " (Instance)")
        {
            
        }
    }
}