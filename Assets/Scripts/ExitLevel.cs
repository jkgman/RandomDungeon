using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ExitLevel : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        KeyHolder holder = other.GetComponent<KeyHolder>();
        if (holder != null && holder.haskey == true)
        {
            SceneManager.LoadScene(0);
        }
    }
}
