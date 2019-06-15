using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeViewer : MonoBehaviour
{
    public float speed = 1;
   
    void Update()
    {
        Vector3 rot = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        Vector3 pos = new Vector3(Input.GetAxis("Horizontal"), 0 , Input.GetAxis("Vertical"));
        pos = Vector3.Min(pos, pos.normalized) * Time.deltaTime * speed;
        if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift))
        {
            pos.y -= speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            pos.y += speed * Time.deltaTime;
        }
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rot);
        transform.position += transform.forward * pos.z + transform.right * pos.x + transform.up * pos.y;
    }
}
