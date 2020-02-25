using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    public static DebugText instance;
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
        text = GetComponent<Text>();
        text.text = "";
    }

    public void onChangeText(string newText) {
        text.text = newText;
    }
}
