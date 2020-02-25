using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerUI_Hide : MonoBehaviour
{
    // This Script allows the UI over the controller (through a tool) to either stay on, hide upon inactivity, or stay off
    Vector3 fullSize;
    float lastTouch;
    public float TimeUntilHide = 2f;
    // Start is called before the first frame update
    void Start()
    {
        fullSize = transform.localScale;
    }

    private void OnEnable() {
        lastTouch = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerScript.instance._controller.Touch1Active) {
            lastTouch = Time.time;
        }

        switch (GameMenu.instance.GetOptionStatus("Touchpad Options")) {
            case ("Always On"):
                transform.localScale = fullSize;
                break;

            case ("Hide Upon Inactivity"):
                float timeAfterHide = Mathf.Clamp(Time.time - (lastTouch + TimeUntilHide), 0f, 1f);
                transform.localScale = Vector3.Lerp(fullSize, Vector3.zero, timeAfterHide * 2f);
                break;

            case ("Always Off"):
                transform.localScale = Vector3.zero;
                break;
        }
    }
}
