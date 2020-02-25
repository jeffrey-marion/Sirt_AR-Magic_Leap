using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class MeshingScript : MonoBehaviour {
    //This script is to scan the real world environment and change how it appears in the game
    #region Public Variables
    public Material BlackMaterial;
    public Material GroundMaterial;
    public Material InactiveMaterial;
    public MLSpatialMapper Mapper;
    #endregion

    #region Private Variables
    internal bool _visible = true;
    bool activated = true;
    #endregion

    #region Unity Methods
    private void Update() {
        UpdateMeshMaterial();
    }
    #endregion

    #region Public Methods
    public void ToggleMeshVisibility() {
        _visible = _visible ? false : true;
    }
    public void ToggleMeshScanning() {
        Mapper.gameObject.SetActive( Mapper.gameObject.activeInHierarchy ? false : true);
    }

    public void onClear() {
        activated = false;
    }

    public void onDeactivate() {
        activated = true;
    }
    #endregion

    #region Private Methods
    /// Switch mesh material based on whether meshing is active and mesh is visible
    /// visible & active = ground material
    /// visible & inactive = meshing off material
    /// invisible = black mesh
    private void UpdateMeshMaterial() {
        // Loop over all the child mesh nodes created by MLSpatialMapper script
        for (int i = 0; i < transform.childCount; i++) {
            // Get the child gameObject
            GameObject gameObject = transform.GetChild(i).gameObject;
            gameObject.layer = LayerMask.NameToLayer("Environment");
            // Get the meshRenderer component
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            // Get the assigned material
            Material material = meshRenderer.sharedMaterial;

            if (activated) {
                if (_visible) {
                    meshRenderer.enabled = true;
                    if (Mapper.enabled) {
                        if (material != GroundMaterial) {
                            meshRenderer.material = GroundMaterial;
                        }
                    } else if (material != InactiveMaterial) {
                        meshRenderer.material = InactiveMaterial;
                    }
                } else if (material != BlackMaterial) {
                    meshRenderer.material = BlackMaterial;
                }
            } else {
                if (GameMenu.instance == null) {
                    if (meshRenderer.enabled) {
                        meshRenderer.enabled = false;
                        //meshRenderer.material = BlackMaterial;
                    }
                } else {
                    string selectedOption = GameMenu.instance.GetOptionStatus("Real World Rendering");

                    switch (selectedOption) {
                        case "Ignore":
                            if (meshRenderer.enabled) {
                                meshRenderer.enabled = false;
                                //meshRenderer.material = BlackMaterial;
                            }
                            break;

                        case "Cut Out":
                            if (!meshRenderer.enabled) {
                                meshRenderer.enabled = true;
                                meshRenderer.material = BlackMaterial;
                            } else if (meshRenderer.material != BlackMaterial) {
                                meshRenderer.material = BlackMaterial;
                            }
                            break;

                        case "Wireframe":
                            if (!meshRenderer.enabled) {
                                meshRenderer.enabled = true;
                                meshRenderer.material = GroundMaterial;
                            } else if (meshRenderer.material != GroundMaterial) {
                                meshRenderer.material = GroundMaterial;
                            }
                            break;

                        default:
                            Debug.LogError("There's no option laid out for " + selectedOption);
                            break;
                    }
                }
            }
        }
    }
    #endregion
}
