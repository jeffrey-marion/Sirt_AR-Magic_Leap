using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ControllerPrompts : MonoBehaviour
{
    public static UI_ControllerPrompts instance;
    [System.Serializable]
    public class prompt {
        public Text textPrompt;
        public Transform pointingArea;
        public bool startAtX;

        internal string text {
            get {
                return textPrompt.text;
            }

            set {
                textPrompt.text = value;
            }
        }

        public void SetUp() {
            if (textPrompt.GetComponent<UI_ControllerPrompts_Text>() != null) {
                return;
            }
            UI_ControllerPrompts_Text script = textPrompt.gameObject.AddComponent<UI_ControllerPrompts_Text>();
            script.text = textPrompt;
            script.StartAtX = startAtX;
            script.pointingArea = pointingArea;
            script.SetUp();
        }

        public void SetActive(bool on) {
            textPrompt.gameObject.SetActive(on);
        }
    }

    public prompt TouchPadPrompt;
    public prompt HomePrompt;
    public prompt BumperPrompt;
    public prompt TriggerPrompt;

    internal bool openedMenu = false;

    string lastSetting;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
        TouchPadPrompt.SetUp();
        HomePrompt.SetUp();
        BumperPrompt.SetUp();
        TriggerPrompt.SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (lastSetting != GameMenu.instance.GetOptionStatus("Controller Prompts")) {
            lastSetting = GameMenu.instance.GetOptionStatus("Controller Prompts");

            bool turnOn = true;
            if (lastSetting == "Off") {
                turnOn = false;
            }
            TouchPadPrompt.SetActive(turnOn);
            HomePrompt.SetActive(turnOn);
            BumperPrompt.SetActive(turnOn);
            TriggerPrompt.SetActive(turnOn);
        }

        if (lastSetting == "Off") {
            return;
        }
        transform.position = PlayerScript.instance.controllerPos;
        transform.rotation = PlayerScript.instance.controllerRot;
    }

    public void onChangePrompts(string touchPadString, string homeString, string bumperString, string triggerString) {
        TouchPadPrompt.text = touchPadString;
        HomePrompt.text = homeString;
        BumperPrompt.text = bumperString;
        TriggerPrompt.text = triggerString;

        if(!openedMenu && homeString == "") {
            HomePrompt.text = "Menu";
        }
    }
}

public class UI_ControllerPrompts_Text : MonoBehaviour {
    internal Text text;
    internal bool StartAtX;
    internal Transform pointingArea;
    LineRenderer lineRenderer;
    Transform player;
    RectTransform rect;

    float halfWidth;
    float halfHeight;

    internal void SetUp() {
        lineRenderer = GetComponent<LineRenderer>();
        player = Camera.main.transform;
        rect = GetComponent<RectTransform>();
        halfWidth = (rect.rect.width * transform.lossyScale.x * 0.5f);
        halfHeight = (rect.rect.height * transform.lossyScale.y * 0.5f);
    }

    private void Update() {
        transform.rotation = player.rotation;

        if (text.text == "") {
            lineRenderer.positionCount = 0;
        } else {
            lineRenderer.positionCount = 3;
            Vector3 area = transform.InverseTransformPoint(pointingArea.position);
            if (StartAtX) {
                Vector3 pos1 = transform.position + (transform.right * halfWidth * -1f);
                if (area.x > 0) {
                    pos1 = transform.position + (transform.right * halfWidth);
                }
                lineRenderer.SetPosition(0, pos1);
                lineRenderer.SetPosition(1, transform.TransformPoint(new Vector3(area.x, 0f, area.z)));
            } else {
                Vector3 pos1 = transform.position + (transform.up * halfHeight * -1f);
                if (area.y > 0) {
                    pos1 = transform.position + (transform.up * halfHeight);
                }
                lineRenderer.SetPosition(0, pos1);
                lineRenderer.SetPosition(1, transform.TransformPoint(new Vector3(0f, area.y, area.z)));
            }

            lineRenderer.SetPosition(2, pointingArea.position);
        }
    }
}
