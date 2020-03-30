using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private float startOpacity;
    [SerializeField]
    private float endOpacity;
    [SerializeField]
    private float startWait = 1;
    [SerializeField]
    private float transitionDuration = 5;
    [SerializeField]
    private float endWait = 1;

    float startTime;

    private void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        Color tempColor = image.color;
        tempColor.a = Mathf.Clamp(Mathf.Lerp(startOpacity, endOpacity, (Time.time - startTime - startWait) / transitionDuration), 0, 1);
        image.color = tempColor;
        if ((Time.time - startTime) > endWait + transitionDuration + startWait)
        {
            SceneManager.LoadScene(1);
        }
    }
}
