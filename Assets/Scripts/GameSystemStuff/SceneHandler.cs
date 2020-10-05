using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<AudioManager>().Play("Background_Boingy");
    }

    public void SwitchScene()
	{
        FindObjectOfType<AudioManager>().stop("Background_Boingy");
        FindObjectOfType<AudioManager>().Play("Background_harmonica");
        SceneManager.LoadScene("Scene1");

    }
    public void Level2()
    {
        FindObjectOfType<AudioManager>().stop("Background_Boingy");
        FindObjectOfType<AudioManager>().Play("Background_Sunset");
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
