using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    public Image image;
    float counter;
    public float lerpSpeed = .05f;
    public float waittime = 1;
    // Update is called once per frame
    void Update()
    {
        Color tempColor = image.color;
        tempColor.a = Mathf.Min(1, tempColor.a + lerpSpeed);
        image.color = tempColor;
        if (tempColor.a >= 1)
        {
            counter += Time.deltaTime;
            if (counter >= waittime)
            {
                SceneManager.LoadScene(1);
            }
        }
    }
}
