using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldUI_Measurements : MonoBehaviour
{
    Measurements currentMeasurement {
        get {
            if (GameMenu.instance == null) {
                return Measurements.feet;
            }
            string status = GameMenu.instance.GetOptionStatus("Measurement");
            if (status == "Metres") {
                return Measurements.meters;
            } else if (status == "Feet") {
                return Measurements.feet;
            }

            Debug.Log("There's no measurement called " + status);
            return Measurements.feet;
        }
    }

    Coroutine hideCo;
    Text text;
    RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        rect = GetComponent<RectTransform>();
        text.text = "";
    }

    public void onShowLength(LineRenderer lineRenderer) {

       float lengthInMeters = lineRenderer.length();
       text.text = measurementInString(lengthInMeters);
        int pos1 = Mathf.CeilToInt((lineRenderer.positionCount - 1) * 0.5f - 1f);
        int pos2 = Mathf.FloorToInt((lineRenderer.positionCount - 1) * 0.5f + 1f);

        Vector3 vec1 = lineRenderer.GetPosition(pos1);
        Vector3 vec2 = lineRenderer.GetPosition(pos2);

        vec1 = new Vector3(vec1.x, Mathf.Min(vec1.y, vec2.y), vec1.z);
        vec2 = new Vector3(vec2.x, vec1.y, vec2.z);

        Vector3 CenterPos = (vec1 + vec2) * 0.5f;

        Vector3 ForwardPos = Vector3.Normalize(vec2 - vec1);
        Quaternion forwardQuat = Quaternion.LookRotation(ForwardPos, Vector3.up);

        float distFromLine = ((lineRenderer.startWidth + lineRenderer.endWidth) * 0.5f) + 0.05f;

        Vector3 endPos1 = CenterPos + (forwardQuat * Vector3.right * distFromLine);
        Vector3 endPos2 = CenterPos + (forwardQuat * Vector3.left * distFromLine);
        Vector3 endPos = endPos1;

        if (Vector3.Distance(Camera.main.transform.position, endPos1) > Vector3.Distance(Camera.main.transform.position, endPos2)) {
            endPos = endPos2;
        }

        transform.position = endPos;

        Quaternion newRot = Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(CenterPos - endPos));
        newRot.eulerAngles = new Vector3(90f, newRot.eulerAngles.y, newRot.eulerAngles.z);
        transform.rotation = newRot;
        transform.position += Vector3.up * 0.05f;

        rect.sizeDelta = new Vector2(lengthInMeters * 100f, rect.sizeDelta.y);

        if(hideCo != null) {
            StopCoroutine(hideCo);
        }

        hideCo = StartCoroutine(hide());
    }

    public void onShowLength(Vector3 startVec, Vector3 endVec) {
        float lengthInMeters = Vector3.Distance(startVec, endVec);
        text.text = measurementInString(lengthInMeters);

        Vector3 vec1 = startVec;
        Vector3 vec2 = endVec;

        vec1 = new Vector3(vec1.x, Mathf.Min(vec1.y, vec2.y), vec1.z);
        vec2 = new Vector3(vec2.x, vec1.y, vec2.z);

        Vector3 CenterPos = (vec1 + vec2) * 0.5f;

        Vector3 ForwardPos = Vector3.Normalize(vec2 - vec1);
        Quaternion forwardQuat = Quaternion.LookRotation(ForwardPos, Vector3.up);

        float distFromLine = 0.25f;

        Vector3 endPos1 = CenterPos + (forwardQuat * Vector3.right * distFromLine);
        Vector3 endPos2 = CenterPos + (forwardQuat * Vector3.left * distFromLine);
        Vector3 endPos = endPos1;

        if (Vector3.Distance(Camera.main.transform.position, endPos1) > Vector3.Distance(Camera.main.transform.position, endPos2)) {
            endPos = endPos2;
        }

        transform.position = endPos;

        Quaternion newRot = Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(CenterPos - endPos));
        newRot.eulerAngles = new Vector3(90f, newRot.eulerAngles.y, newRot.eulerAngles.z);
        transform.rotation = newRot;
        transform.position += Vector3.up * 0.05f;

        rect.sizeDelta = new Vector2(lengthInMeters * 100f, rect.sizeDelta.y);

        if (hideCo != null) {
            StopCoroutine(hideCo);
        }

        hideCo = StartCoroutine(hide());
    }

    IEnumerator hide() {
        yield return new WaitForSeconds(1f);

        text.text = "";
    }

    string measurementInString(float m) {
        if (currentMeasurement == Measurements.meters) {
            if (m < 1f) {
                return "" + Mathf.Floor(100f * m) + "cm";
            } else {
                return "" + Mathf.Floor(m * 100f) * 0.01f + "m";
            }
        } else {
            float inches = m * 39.3700787f;
            float feet = Mathf.Floor(inches / 12f);
            inches -= feet * 12f;

            return (feet + "' " + Mathf.Floor(inches) + "\"");
        }
    }
}

public enum Measurements { meters, feet};


