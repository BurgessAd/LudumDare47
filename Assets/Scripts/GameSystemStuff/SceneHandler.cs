using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SwitchScene()
	{
        SceneManager.LoadScene("Scene1");
    }

    public void Quit()
	{
        Application.Quit();
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
