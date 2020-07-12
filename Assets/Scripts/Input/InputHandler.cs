using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Gathers input for current layout and sends it to current focused IInputReciever
/// </summary>
public class InputHandler : MonoBehaviour
{
    [SerializeField]
    private InputMode inputMode = InputMode.Keyboard;

    public List<IInputReciever> recievers = new List<IInputReciever>();

    private IInputReciever activeReciever;

    public void RequestFocus(IInputReciever reciever) {
        if (!recievers.Contains(reciever))
            recievers.Add(reciever);
        activeReciever = reciever;
    }

    private void Update()
    {
        if (recievers.Count <= 0 || activeReciever == null)
            return;

        switch (inputMode)
        {
            case InputMode.Null:
                throw new System.Exception("No input type detected");
            case InputMode.Keyboard:
                //sends all data for keyboard interactions
                ButtonInput forward = GetKeyInformation(KeyCode.W, "Forward");
                break;
            case InputMode.Controller:
                //sends all data for Controller interactions
                break;
            default:
                break;
        }
    }
    //later use a serialized dictionary for keys and tags
    private ButtonInput GetKeyInformation(KeyCode key, string tag) {
        return new ButtonInput(tag,Input.GetKeyDown(key), Input.GetKey(key), Input.GetKeyUp(key) );
    }
}
