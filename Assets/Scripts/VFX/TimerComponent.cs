using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerComponent : MonoBehaviour
{
    [SerializeField]
    private float m_TimerTime;
    private void Awake()
    {
        StartCoroutine(Timer());
    }

    private IEnumerator Timer() 
    {
        yield return new WaitForSeconds(m_TimerTime);
        Destroy(gameObject);
    }
}
