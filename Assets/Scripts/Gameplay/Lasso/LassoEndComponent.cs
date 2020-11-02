using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LassoEndComponent : MonoBehaviour
{

    public event Action OnHitGround;

    public event Action<AnimalComponent> OnHitAnimal;

    [SerializeField]
    private Rigidbody m_RigidBody;

    void OnCollisionEnter(Collision collision){
        GameObject hitObject = collision.gameObject;
        m_RigidBody.velocity = Vector3.zero;
        if (hitObject.TryGetComponent(out AnimalComponent animal)) 
        {
            OnHitAnimal(animal);
        }
        else 
        {
            OnHitGround();
        }
    }
}
