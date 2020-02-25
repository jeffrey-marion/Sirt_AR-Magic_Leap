using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class PlayerScript : MonoBehaviour
{
    // This Script is designed to track controller inputs and outputs
    #region Controller Variables
    public static PlayerScript instance;
    internal MLInputController _controller;

    public class button {
        bool _getButton = false;
        internal bool getButton {
            get {
                return _getButton;
            }
        }

        bool _getButtonUp = false;
        internal bool getButtonUp {
            get {
                return _getButtonUp;
            }
        }

        bool _getButtonDown = false;
        internal bool getButtonDown {
            get {
                return _getButtonDown;
            }
        }

        internal void onSetData(bool data) {
            if (data == true && _getButton == false) {
                _getButtonDown = true;
                PlayerScript.instance.StartCoroutine(removeButtonDown());
            } else if (data == false && _getButton == true) {
                _getButtonUp = true;
                PlayerScript.instance.StartCoroutine(removeButtonUp());
            }

            _getButton = data;
        }

        IEnumerator removeButtonDown() {
            yield return new WaitForEndOfFrame();
            _getButtonDown = false;
        }

        IEnumerator removeButtonUp() {
            yield return new WaitForEndOfFrame();
            _getButtonUp = false;
        }
    }

    public button Home = new button();
    public button Bumper = new button();
    public button Trigger = new button();

    internal bool setUpComplete = false;

    public Vector3 controllerPos {
        get {
            return _controller.Position;
        }
    }

    public Quaternion controllerRot {
        get {
            return _controller.Orientation;
        }
    }
    #endregion 
    // Start is called before the first frame update
    void Start() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }

        MLInput.Start();
        MLAudio.Start();

        MLInput.OnControllerButtonDown += OnButtonDown;
        MLInput.OnControllerButtonUp += OnButtonUp;

        _controller = MLInput.GetController(MLInput.Hand.Left);

        if (_controller == null) {
            _controller = MLInput.GetController(MLInput.Hand.Right);
        }
    }

    void OnDestroy() {
        MLInput.OnControllerButtonDown -= OnButtonDown;
        MLInput.OnControllerButtonUp -= OnButtonUp;
        MLInput.Stop();
        MLAudio.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        Trigger.onSetData(_controller.TriggerValue > 0.5f || (Trigger.getButton && _controller.TriggerValue > 0.4f));

        if (!GameMenu.instance.gameObject.activeInHierarchy && Home.getButtonDown && setUpComplete) {
            GameMenu.instance.gameObject.SetActive(true);
            UI_ControllerPrompts.instance.openedMenu = true;
        } else if (GameMenu.instance.gameObject.activeInHierarchy && setUpComplete) {
            MenuControls();
        }
    }

    bool touchDown = false;
    Vector3 lastTouch = Vector3.zero;
    public float touchDistance = 0.5f;
    void MenuControls() {
        if (_controller.Touch1Active) {
            if (touchDown) {
                touchDistance = GameMenu.instance.GetOptionStatusAsFloat("Scrolling Sensitivity");
                float dist = _controller.Touch1PosAndForce.y - lastTouch.y;

                if (Mathf.Abs(dist) > touchDistance) {
                    if (dist > 0) {
                        GameMenu.instance.onScrollUp = Mathf.FloorToInt(Mathf.Abs(dist / touchDistance));
                    } else {
                        GameMenu.instance.onScrollDown = Mathf.FloorToInt(Mathf.Abs(dist / touchDistance));
                    }
                    lastTouch = _controller.Touch1PosAndForce;
                }
            } else {
                touchDown = true;
                lastTouch = _controller.Touch1PosAndForce;
            }
        } else {
            touchDown = false;
        }
    }

    void OnButtonDown(byte controller_id, MLInputControllerButton button) {
        if (button == MLInputControllerButton.HomeTap) {
            Home.onSetData(true);
        }

        if (button == MLInputControllerButton.Bumper) {
            Bumper.onSetData(true);
        }
    }

    void OnButtonUp(byte controller_id, MLInputControllerButton button) {
        if (button == MLInputControllerButton.HomeTap) {
            Home.onSetData(false);
        }

        if (button == MLInputControllerButton.Bumper) {
            Bumper.onSetData(false);
        }
    }

    // Have the Controller Vibrate if the player wants it to
    public void StartControllerVibrate(MLInputControllerFeedbackPatternVibe pattern, MLInputControllerFeedbackIntensity intensity) {
        if (GameMenu.instance == null) {
            _controller.StartFeedbackPatternVibe(pattern, intensity);
            return;
        }

        if (GameMenu.instance.GetOptionStatus("Controller Vibrate") == "On") {
            _controller.StartFeedbackPatternVibe(pattern, intensity);
        }
    }
}
