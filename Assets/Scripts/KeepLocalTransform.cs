using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepLocalTransform : MonoBehaviour
{
	[SerializeField]
	bool position;
	[SerializeField]
	bool rotation;

	Vector3 originalPos;
	Vector3 originalEuler;
    // Start is called before the first frame update
    void Start()
    {
		originalPos = transform.localPosition;
		originalEuler = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
		if (rotation) transform.localEulerAngles = originalEuler;
		if (position) transform.localPosition = originalPos;
    }
}
