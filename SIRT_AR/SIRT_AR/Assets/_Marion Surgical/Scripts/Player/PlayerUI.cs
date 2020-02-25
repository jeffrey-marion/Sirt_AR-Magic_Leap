using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;
    GameObject player;

    // The Pointer aspect can be adjusted to briefly appear to tell the player to rotate their head in a certain direction (See pointer in calibration to see how this works)
    #region Pointer
    public Transform PointerUI;
    Image PointerImage;
    Coroutine pointerFade;
    #endregion

    // The Info aspect can be adjusted to display text in front of the player (See text during calibration to see how this works)
    #region Info
    public Text infoText;
    Coroutine infoFade;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }

        player = Camera.main.gameObject;
        #region Pointer
        PointerImage = PointerUI.gameObject.GetComponentInChildren<Image>();
        PointerImage.color = new Color(PointerImage.color.r, PointerImage.color.g, PointerImage.color.b, 0f);
        #endregion
        #region Info Text
        infoText.color = new Color(infoText.color.r, infoText.color.g, infoText.color.b, 0f);
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
    }

    #region Pointer
    public void onPoint(Vector3 worldPos, float length = 1f) {
        PointerImage.color = Color.white;
        PointerUI.localEulerAngles = new Vector3(0f, 0f, RotateAngle(worldPos));

        if (pointerFade != null) {
            StopCoroutine(pointerFade);
        }

        pointerFade = StartCoroutine(onPointerFade(length));
    }

    public void onPoint(Vector3 worldPos, Color color, float length = 1f) {
        PointerImage.color = color;
        PointerUI.localEulerAngles = new Vector3(0f, 0f, RotateAngle(worldPos));

        if (pointerFade != null) {
            StopCoroutine(pointerFade);
        }

        pointerFade = StartCoroutine(onPointerFade(length));
    }

    float RotateAngle(Vector3 worldPos) {
        float xLength = player.transform.InverseTransformPoint(worldPos).x;
        float yLength = player.transform.InverseTransformPoint(worldPos).y;

        Vector2 pointB = new Vector2(xLength, yLength);
        Vector2 pointC = new Vector2(0f, 2f);
        float sideA = Vector2.Distance(pointB, pointC);
        float sideB = Vector2.Distance(Vector2.zero, pointC);
        float sideC = Vector2.Distance(Vector2.zero, pointB);


        float angle = Mathf.Acos((Mathf.Pow(sideB, 2f) + Mathf.Pow(sideC, 2f) - Mathf.Pow(sideA, 2f)) / (2f * sideB * sideC)) * Mathf.Rad2Deg;

        if (xLength > 0) {
            angle = 360f - angle;
        }

        return angle;
    }

    IEnumerator onPointerFade(float duration) {
        yield return new WaitForSeconds(duration);

        float Timer = 0f;

        while (Timer < 1f) {
            Timer += Time.deltaTime * 2f;
            PointerImage.color = new Color(PointerImage.color.r, PointerImage.color.g, PointerImage.color.b, 1f - Timer);
            yield return null;
        }
        
        StopCoroutine(pointerFade);
    }
    #endregion

    #region Info

    public void onDisplayInfo(string _Text, float length = 1f) {
        infoText.color = Color.white;
        infoText.text = _Text;

        if (infoFade != null) {
            StopCoroutine(infoFade);
        }

        infoFade = StartCoroutine(onInfoFade(length));
    }

    public void onDisplayInfo(string _Text, Color color, float length = 1f) {
        infoText.color = color;
        infoText.text = _Text;

        if (infoFade != null) {
            StopCoroutine(infoFade);
        }

        infoFade = StartCoroutine(onInfoFade(length));
    }

    IEnumerator onInfoFade(float duration) {
        yield return new WaitForSeconds(duration);

        float Timer = 0f;

        while (Timer < 1f) {
            Timer += Time.deltaTime * 2f;
            infoText.color = new Color(infoText.color.r, infoText.color.g, infoText.color.b, 1f - Timer);
            yield return null;
        }
    }

    #endregion
}
