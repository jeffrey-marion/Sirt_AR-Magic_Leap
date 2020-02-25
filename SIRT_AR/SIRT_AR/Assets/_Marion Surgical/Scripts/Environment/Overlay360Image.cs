using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overlay360Image : MonoBehaviour
{
    //This script is to turn on or off 
    MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Camera.main.transform.position;
        if (GameMenu.instance == null) {
            meshRenderer.enabled = false;
        } else {
            meshRenderer.enabled = true;
        }
    }
}
