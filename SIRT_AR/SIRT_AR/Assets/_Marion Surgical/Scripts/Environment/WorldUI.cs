using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUI : MonoBehaviour
{
    public static WorldUI instance;

    public WorldUI_Measurements measurements;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }

        if (measurements == null && GetComponentInChildren<WorldUI_Measurements>() != null) {
            measurements = GetComponentInChildren<WorldUI_Measurements>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
