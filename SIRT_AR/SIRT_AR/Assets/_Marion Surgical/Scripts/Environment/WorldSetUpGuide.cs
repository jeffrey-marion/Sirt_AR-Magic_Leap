using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class WorldSetUpGuide : MonoBehaviour
{
    public static WorldSetUpGuide instance;
    public GameObject[] meshingObjects;
    public MeshingScript meshingScript;

    GameObject playerObject;
    #region Part 1: Clear Space
    public GameObject ClearSpaceParent;
    public GameObject PressTriggerObject;
    public float TimeUntilPressTriggerAppears = 5f;
    float PressTriggerTimeEnd;
    bool startSetUp = false;
    #endregion

    #region Part 2: SetUp
    public GameObject examineFolder;
    class EyeViews {
        internal GameObject mainObject;
        float TimeToChargeMultiplier;
        float TimePassed = 0f;
        Image filler;
        bool _complete = false;

        internal bool complete {
            get {
                return _complete;
            }
        }
        int eyeing = 0;

        internal void SetUp(GameObject m, float t) {
            mainObject = m;
            TimeToChargeMultiplier = 1f / t;
            filler = m.transform.GetChild(0).GetComponent<Image>();
            filler.fillAmount = 0f;
        }

        internal void Update() {
            if (complete) {
                mainObject.SetActive(false);
                return;
            }
            if (eyeing > 0) {
                TimePassed += Time.deltaTime * TimeToChargeMultiplier;

                if (TimePassed >= 1f) {
                    _complete = true;
                    WorldSetUpGuide.instance.onFinishEye();
                }
                eyeing -= 1;
            } else {
                TimePassed -= Time.deltaTime * TimeToChargeMultiplier;
            }
            TimePassed = Mathf.Clamp(TimePassed, 0f, 1.1f);
            filler.fillAmount = TimePassed;
        }

        internal void onEye() {
            
            eyeing = 10;
        }
    }

    public GameObject EyeViewObject;
    public float eyeViewObjectDistance = 2f;
    public float eyeViewTime = 1f;
    List<EyeViews> eyeViews = new List<EyeViews>();
    int completedEyeViews = 0;
    int TotalEyeViews = 24;
    public LayerMask targetMask;
    #endregion

    #region Part 3: Smoothing
    public float timeForSmoothingPrompt = 5f;
    #endregion

    bool finishSetUp = false;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }

        playerObject = FindObjectOfType<PlayerScript>().gameObject;


        for (int i = 0; i < meshingObjects.Length; i++) {
            meshingObjects[i].SetActive(false);
        }

        #region Part 1: Clear Space
        PressTriggerObject.SetActive(false);
        PressTriggerTimeEnd = Time.time + TimeUntilPressTriggerAppears;
        #endregion

        #region Part 2: SetUp
        examineFolder.SetActive(false);
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (finishSetUp) {
            return;
        }

        #region Part 1: Clear Space
        if (!startSetUp) {
            ClearSpaceParent.transform.position = playerObject.transform.position + (playerObject.transform.forward * 2f);
            ClearSpaceParent.transform.localEulerAngles = new Vector3(playerObject.transform.eulerAngles.x, playerObject.transform.eulerAngles.y, ClearSpaceParent.transform.localEulerAngles.z);

            if (Time.time > PressTriggerTimeEnd) {
                PressTriggerObject.SetActive(true);
                if (PlayerScript.instance.Trigger.getButtonDown) {
                    for (int i = 0; i < meshingObjects.Length; i++) {
                        meshingObjects[i].SetActive(true);
                    }
                    ClearSpaceParent.SetActive(false);
                    startSetUp = true;
                    MLEyes.Start();
                    meshingScript.onClear();
                    examineFolder.SetActive(true);
                    meshingScript.transform.position = playerObject.transform.position;
                    SetEyeViewPositions();
                }
            }

            return;
        }
        #endregion

        #region Part 2: SetUp

        bool inView = false;
        EyeViewObject.transform.parent.position = playerObject.transform.position;
        for (int i = 0; i < eyeViews.Count; i++) {
                eyeViews[i].Update();
                if (inCenterOfScreen(eyeViews[i].mainObject.transform.position) && !eyeViews[i].complete) {
                    eyeViews[i].onEye();
                inView = true;
            }
        }

        if (!inView) {
            Vector3 shootPoint = playerObject.transform.position + (playerObject.transform.forward * eyeViewObjectDistance);
            float closestDist = 999999999999999999999999f;
            int selectedPoint = 0;

            for (int i = 0; i < eyeViews.Count; i++) {
                if (!eyeViews[i].complete) {
                    float dist = Vector3.Distance(eyeViews[i].mainObject.transform.position, shootPoint);

                    if (dist < closestDist) {
                        selectedPoint = i;
                        closestDist = dist;
                    }
                }
            }

            PlayerUI.instance.onPoint(eyeViews[selectedPoint].mainObject.transform.position, 0.01f);

        }
        #endregion
    }

    bool inCenterOfScreen(Vector3 worldPos) {
        Vector3 viewPortPos = Camera.main.WorldToViewportPoint(worldPos);

        if (viewPortPos.z > 0f &&
            viewPortPos.x > 0.4f &&
            viewPortPos.x < 0.6f &&
            viewPortPos.y > 0.4f &&
            viewPortPos.y < 0.6f) {
            return true;
        }

        return false;
    }

    void SetEyeViewPositions() {
        Transform cameraView = playerObject.transform;
        EyeViewObject.transform.parent.position = playerObject.transform.position;

        for (int i = 0; i < TotalEyeViews; i++) {
            GameObject newEye = EyeViewObject;

            if (i > 0) {
                newEye = Instantiate(EyeViewObject) as GameObject;
                newEye.transform.parent = EyeViewObject.transform.parent;
                newEye.transform.localScale = EyeViewObject.transform.localScale;
            }

            newEye.name = "Eye Target " + i;
            Vector3 dir = Vector3.zero;

            switch (i) {
                case 0:
                    dir = cameraView.forward;
                    break;

                case 1:
                    dir = -cameraView.forward;
                    break;

                case 2:
                    dir = -cameraView.right;
                    break;

                case 3:
                    dir = cameraView.right;
                    break;

                case 4:
                    dir = Vector3.Normalize(cameraView.forward + cameraView.up);
                    break;

                case 5:
                    dir = Vector3.Normalize(cameraView.forward - cameraView.up);
                    break;

                case 6:
                    dir = Vector3.Normalize(cameraView.forward - cameraView.right);
                    break;

                case 7:
                    dir = Vector3.Normalize(cameraView.forward + cameraView.right);
                    break;

                case 8:
                    dir = Vector3.Normalize(-cameraView.forward + cameraView.up);
                    break;

                case 9:
                    dir = Vector3.Normalize(-cameraView.forward - cameraView.up);
                    break;

                case 10:
                    dir = Vector3.Normalize(-cameraView.forward - cameraView.right);
                    break;

                case 11:
                    dir = Vector3.Normalize(-cameraView.forward + cameraView.right);
                    break;

                case 12:
                    dir = Vector3.Normalize(-cameraView.right + cameraView.up);
                    break;

                case 13:
                    dir = Vector3.Normalize(-cameraView.right - cameraView.up);
                    break;

                case 14:
                    dir = Vector3.Normalize(cameraView.right + cameraView.up);
                    break;

                case 15:
                    dir = Vector3.Normalize(cameraView.right - cameraView.up);
                    break;

                case 16:
                    dir = Vector3.Normalize(cameraView.forward + cameraView.up - cameraView.right);
                    break;

                case 17:
                    dir = Vector3.Normalize(cameraView.forward + cameraView.up + cameraView.right);
                    break;

                case 18:
                    dir = Vector3.Normalize(cameraView.forward - cameraView.up - cameraView.right);
                    break;

                case 19:
                    dir = Vector3.Normalize(cameraView.forward - cameraView.up - cameraView.right);
                    break;

                case 20:
                    dir = Vector3.Normalize(-cameraView.forward + cameraView.up - cameraView.right);
                    break;

                case 21:
                    dir = Vector3.Normalize(-cameraView.forward + cameraView.up + cameraView.right);
                    break;

                case 22:
                    dir = Vector3.Normalize(-cameraView.forward - cameraView.up - cameraView.right);
                    break;

                case 23:
                    dir = Vector3.Normalize(-cameraView.forward - cameraView.up + cameraView.right);
                    break;
            }

            newEye.transform.position = cameraView.transform.position + (dir * eyeViewObjectDistance);
            newEye.transform.LookAt(cameraView, Vector3.up);
            EyeViews newEyeView = new EyeViews();
            newEyeView.SetUp(newEye, eyeViewTime);

            eyeViews.Add(newEyeView);
        }
    }

    public void onFinishEye() {
        completedEyeViews++;


        if (completedEyeViews == TotalEyeViews) {
            PlayerUI.instance.onDisplayInfo("Look around to find unexamined surfaces", timeForSmoothingPrompt * 0.5f);
            finishSetUp = true;
            StartCoroutine(done());
            for (int i = 0; i < eyeViews.Count; i++) {
                eyeViews[i].Update();
            }
        } else {
            PlayerUI.instance.onDisplayInfo(Mathf.CeilToInt(((float)completedEyeViews) / ((float)TotalEyeViews) * 100f) + "%\nComplete");
        }
    }

    IEnumerator done() {
        meshingScript.onDeactivate();
        yield return new WaitForSeconds(timeForSmoothingPrompt);

        bool pressed = false;
        float lastFrameTime = Time.time;

        while (!pressed) {
            float thisFrameTime = Time.time % timeForSmoothingPrompt;
            if (thisFrameTime < lastFrameTime) {
                PlayerUI.instance.onDisplayInfo("Press Trigger when ready", timeForSmoothingPrompt * 0.25f);
            }
            lastFrameTime = thisFrameTime;
            pressed = PlayerScript.instance.Trigger.getButtonDown;
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        meshingScript.onClear();
        meshingScript.ToggleMeshScanning();
        PlayerScript.instance.setUpComplete = true;

        this.enabled = false;
    }
}
