using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDebugCanvas : MonoBehaviour
{
	public Text debugText;

	private Transform playerTrans;
	private Transform cameraTrans;

	private static PlayerDebugCanvas _instance;
	public static PlayerDebugCanvas Instance
	{
		get{ return _instance; }
	}

    // Start is called before the first frame update
    void Awake()
    {
		_instance = this;
		var temp = GameObject.FindGameObjectWithTag("Player");
		if (temp)
			playerTrans = temp.transform;
		if (Camera.main)
			cameraTrans = Camera.main.transform;
    }

	void LateUpdate()
	{
		if (playerTrans)
		{
			transform.position = playerTrans.position + Vector3.up * 2f;
		}
		if (cameraTrans)
		{
			transform.LookAt(cameraTrans);
		}
	}

    public void SetDebugText(string text)
	{
		if(debugText)
			debugText.text = text;
	}
}
