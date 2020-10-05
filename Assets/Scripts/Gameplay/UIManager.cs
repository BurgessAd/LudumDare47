using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{

    public Text cowsInPen;
    public PenBehaviour pen;
    public KillZone kz;
    public GameManager cgm;
    // Start is called before the first frame update
    void Start()
    {
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateText();
    }

    public void UpdateText()
	{
        cowsInPen.text = "Cows in Pen: " + pen.CowsInPen +  "\nCows dead: "+cgm.getDeadCows();
        if(pen.CowsInPen > 0)
        {
            FindObjectOfType<AudioManager>().stop("Background_harmonica");
            SceneManager.LoadScene("Transition");
        }
    }

}
