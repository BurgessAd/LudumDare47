using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] cows;
    // Start is called before the first frame update
    void Start()
    {
        cows = GameObject.FindGameObjectsWithTag("Cow");
    }


    public int getDeadCows()
    {
        int count = 0;
        for(int i = 0; i < cows.Length; i++)
        {
            if (cows[i] == null)
            {
                count++;
            }
        }
        return count;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
