using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRoom : MonoBehaviour
{
    public GameObject playerprefab;
    public GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
        GameObject player = Instantiate(playerprefab, transform.position + new Vector3(0, 2, 0), Quaternion.identity);
        //player.transform.position = transform.position;// + new Vector3(0,2,0);
        Instantiate(cam);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
