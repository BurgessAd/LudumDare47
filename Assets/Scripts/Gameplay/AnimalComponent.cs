using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalComponent : MonoBehaviour
{
    private AnimalStateHandler ash;
    [SerializeField]
    private CowGameManager cowGameManager;

    [SerializeField]
    private GameObject leash;


    [SerializeField]
    private GameObject animal;
    private static GameObject player;
    private int speed = 3;
    private int evadeRadius = 10;
    private float evadeSpeedMultiplier = 2;
    private float jumpHeight = 2;
    [SerializeField]
    private GameObject body;
    float modifier = 1;
    float size = 1;
    Vector3 yeetDir = new Vector3(1, 1, 0);
    
    private float idleTimeout = 5;
    private float idleTimer = 0;
    private Vector3 moveDir = Vector3.zero;
    private Vector2 dir = Vector2.zero;
    private Vector3 surfaceNorm = new Vector3(0, 1, 0);
    private Transform bodyTransform;
    // Start is called before the first frame update
    void Start()
    {
       
        bodyTransform = body.transform;
        cowGameManager.cows.Add(gameObject);
        ash = gameObject.AddComponent<AnimalStateHandler>();
        ash.animalComponent = this;

        if (player == null)
        {
            player = GameObject.Find("Player");
        }
    }

    // Update is called once per frame
    public void addGravity()
    {
        animal.GetComponent<Rigidbody>().AddForce(new Vector3(0, -9, 0));

    }

}
