using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToggleOnOff : MonoBehaviour
{
    //game object to toggle on or off
    public GameObject gameObjectToToggle;

    //toggle a game component on or off
    public void ToggleOnOff()
    {
        gameObjectToToggle.SetActive(!gameObjectToToggle.activeSelf);
    }
}
