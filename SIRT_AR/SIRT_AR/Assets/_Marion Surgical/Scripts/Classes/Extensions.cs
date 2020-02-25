using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    #region Line Renderer
    public static float length(this LineRenderer lineRenderer) {
        float end = 0f;

        for (int i = 1; i < lineRenderer.positionCount; i++) {
            float dist = Vector3.Distance(lineRenderer.GetPosition(i - 1), lineRenderer.GetPosition(i));
            end += dist;
        }
        return end;
    }
    #endregion

    #region Material
    public static void ActivateMaterial_changeTexture( this Material material, Texture texture) {
        material.SetTexture("_MainTex", texture);
    }

    public static void ActivateMaterial_changeColor(this Material material, Color color) {
        material.SetColor("_Color", color);
    }

    public static void ActivateMaterial_changeActivation(this Material material, bool activated) {
        float newValue = 0;

        if (activated) {
            newValue = 1;
        }

        material.SetFloat("_Activated", newValue);
    }

    public static Color ActiveMaterial_getColor(this Material material) {
       return material.GetColor("_Color");
    }

    public static Texture ActiveMaterial_getTexture(this Material material) {
        return material.GetTexture("_MainTex");
    }
    #endregion
}
