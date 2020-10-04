using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public Text cowsInPen;
    public PenBehaviour pen;
    public KillZone kz;
    // Start is called before the first frame update
    void Start()
    {
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateText()
	{
        cowsInPen.text = "Cows in Pen: " + pen.CowsInPen +  "\nCows dead: "+kz.CowsDead;

    }

}
