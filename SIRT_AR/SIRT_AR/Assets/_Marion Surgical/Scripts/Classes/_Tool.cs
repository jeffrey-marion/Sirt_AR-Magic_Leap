using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Tool: MonoBehaviour {
    public bool isActive;

    internal bool canUse {
        get {
            return isActive && !GameMenu.instance.gameObject.activeInHierarchy;
        }
    }
    bool _isSetUp = false;
    internal Transform _transform;

    public bool useGravity;

    LineRenderer lineRenderer;

    // Collision Class is to get data about where the controller is pointing
    internal class Collision {
        internal Vector3 point;
        internal GameObject gameObject;
        internal Vector3 angleOfPoint;
        internal float angleDifference;
    }
    public Material lineRendererMaterial;
    public Gradient lineRendererColor;
    internal Collision collision = new Collision();
    internal float projectionSpeed = 5f;
    public LayerMask lineRendererHit;

    // Call Tool_Start when the tool starts to set up everything
    public void Tool_Start(GameObject gameObject)
    {
        _transform = gameObject.transform;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineRendererMaterial;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.colorGradient = lineRendererColor;
        _isSetUp = true;
    }

    // Call Tool_Update on Update
    public void Tool_Update()
    {
        if (!_isSetUp) {
            Debug.LogError("Forgot to set up selected Tool");
        }
        _transform.position = PlayerScript.instance.controllerPos;
        _transform.rotation = PlayerScript.instance.controllerRot;
    }


    //Use Display Projection as True to display where the controller is pointing and to map out the collision
    // Use Display Projection as false to remove display of controller and to not map out collision
    public void displayProjection(bool isDisplaying) {

        collision.point = Vector3.zero;
        collision.gameObject = null;
        lineRenderer.positionCount = 0;
        if (!isDisplaying) {
            return;
        }

        float TimePerFrame = Time.deltaTime;
        Vector3 momentum = _transform.forward * projectionSpeed;

        for (int frames = 0; frames < 300; frames++) {
            lineRenderer.positionCount++;
            if (frames == 0) {
                lineRenderer.SetPosition(frames, _transform.position);
            } else {
                Vector3 newPos = lineRenderer.GetPosition(frames - 1) + (momentum * TimePerFrame);
                lineRenderer.SetPosition(frames, newPos);
                if (useGravity) {
                    momentum += Physics.gravity * TimePerFrame;
                }

                RaycastHit hit;

                if (Physics.Linecast(lineRenderer.GetPosition(frames - 1), newPos, out hit, lineRendererHit)) {
                    collision.point = hit.point;
                    collision.gameObject = hit.transform.gameObject;
                    collision.angleOfPoint = hit.normal * 360f;
                    Vector3 directionFromLastFrame = Vector3.Normalize(newPos - lineRenderer.GetPosition(frames - 1));
                    collision.angleDifference = Vector3.Angle(directionFromLastFrame, hit.normal);
                    frames = 301;
                }
            }
        }


    }
}
