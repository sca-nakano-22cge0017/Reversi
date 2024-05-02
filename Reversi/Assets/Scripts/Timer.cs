using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    Text timerText;
    float timeLimit_sec = 60;
    float time = 0;

    bool timeUp = false;

    private void Awake()
    {
        timerText = GetComponent<Text>();
    }

    void OnEnable()
    {
        time = timeLimit_sec;
        timeUp = false;
        timerText.enabled = true;
    }

    void Update()
    {
        if(!timeUp)
        {
            time -= Time.deltaTime;
            timerText.text = ((int)time / 60).ToString("d2") + ":" + ((int)time % 60).ToString("d2");

            if (time <= 0)
            {
                timeUp = true;
            }
        }
    }
}
