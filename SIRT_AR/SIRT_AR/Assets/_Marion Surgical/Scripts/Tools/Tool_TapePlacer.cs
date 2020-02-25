using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class Tool_TapePlacer : _Tool {

    #region Tape Placer
    #region Variables
    [System.Serializable]
    public class tape {
        public GameObject gameObject;

        LineRenderer _lineRenderer;
        internal LineRenderer lineRenderer {
            get {
                if (_lineRenderer == null) {
                    _lineRenderer = gameObject.GetComponent<LineRenderer>();
                }
                return _lineRenderer;
            }
        }

        SpriteRenderer _spriteRenderer;
        internal SpriteRenderer spriteRenderer {
            get {
                if (_spriteRenderer == null) {
                    _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                }
                return _spriteRenderer;
            }
        }

        Transform _transform;
        internal Transform transform {
            get {
                if (_transform == null) {
                    _transform = gameObject.transform;
                }
                return _transform;
            }
        }
    }

    public tape TapePrefab;
    List<tape> allTapes = new List<tape>();
    tape selectedTape;
    Vector3 tapeSize;
    [System.Serializable]
    public class tapeType {
        public Sprite previewSprite;
        public Sprite sprite;
        public bool includeLine;

        public enum createType { line, lineAndSprite, sprite };
        internal createType type {
            get {
                if (includeLine) {
                    if (sprite != null) {
                        return createType.lineAndSprite;
                    } else {
                        return createType.line;
                    }
                } else {
                    return createType.sprite;
                }
            }
        }
    }

    public List<tapeType> allTapeTypes;
    int _selectedTypeInt = 0;
    int selectedTypeInt {
        set {
            if (value < allTapeTypes.Count) {
                _selectedTypeInt = value;
            }

            if (_selectedTypeInt < 0) {
                _selectedTypeInt = allTapeTypes.Count + _selectedTypeInt;
            }
        }

        get {
            return _selectedTypeInt;
        }
    }
    tapeType selectedTapeType {
        get {
            return allTapeTypes[selectedTypeInt];
        }
    }

    public List<Color> allColors;
    int _selectedColorInt = 0;
    int selectedColorInt {
        set {
            if (value < allColors.Count) {
                _selectedColorInt = value;
            }

            if (_selectedColorInt < 0) {
                _selectedColorInt = allColors.Count + _selectedColorInt;
            }
        }
        get{
        return _selectedColorInt;
        }
}
    Color selectedColor {
        get {
            return allColors[selectedColorInt];
        }
    }

    public GameObject spot;
    Vector3 spot1;
    Vector3 spot2;
    GameObject environmentFolder;
    GameObject tapeFolder;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Tool_Start(gameObject);

        environmentFolder = new GameObject();
        environmentFolder.transform.position = Vector3.zero;
        environmentFolder.transform.localScale = Vector3.one;
        environmentFolder.transform.localEulerAngles = Vector3.zero;
        environmentFolder.name = "Tape Placer - Environment Folder";

        if (GameObject.Find("Environment") != null) {
            environmentFolder.transform.parent = GameObject.Find("Environment").transform;
        }

        tapeFolder = new GameObject();
        tapeFolder.name = "Tape Folder";
        tapeFolder.transform.position = Vector3.zero;
        tapeFolder.transform.localScale = Vector3.one;
        tapeFolder.transform.localEulerAngles = Vector3.zero;

        TapePrefab.lineRenderer.endColor = Color.black;

        tapeFolder.transform.parent = environmentFolder.transform.parent;

        spot.transform.parent = environmentFolder.transform;
        TapePrefab.gameObject.transform.parent = tapeFolder.transform;
        allTapes.Add(TapePrefab);
        tapeSize = TapePrefab.gameObject.transform.localScale;

        for (int i = 0; i < 50; i++) {
            allTapes.Add(spawnNewTape());
        }
        TapePrefab.gameObject.SetActive(false);

        for (int i = allTapeTypes.Count - 1; i > -1; i--) {
           if (!allTapeTypes[i].includeLine && allTapeTypes[i].sprite == null) {
                allTapeTypes.RemoveAt(i);
            }
        }

    }

    private void OnEnable() {
        if (environmentFolder != null) {
            environmentFolder.SetActive(true);
        }
    }

    private void OnDisable() {
        if (environmentFolder != null) {
            environmentFolder.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!canUse) {
            if (GameMenu.instance.gameObject.activeInHierarchy) {
                displayProjection(false);
                spot.SetActive(false);
                if (selectedTape != null) {
                    selectedTape.gameObject.SetActive(false);
                    selectedTape = null;
                }
                UI.SetActive(false);
            }
            return;
        }
        UI.SetActive(true);
        Tool_Update();
        displayProjection(true);

        if (collision.gameObject != null) {
            SelectTapeType();
            if (selectedTape == null) {
                spot.SetActive(true);
                spot.transform.position = collision.point;
                spot.transform.rotation = Quaternion.FromToRotation(Vector3.forward, collision.angleOfPoint);

                if (PlayerScript.instance.Trigger.getButtonDown) {
                    for (int i = 0; i < allTapes.Count; i++) {
                        if (!allTapes[i].gameObject.activeInHierarchy) {
                            selectedTape = allTapes[i];
                            selectedTape.gameObject.SetActive(true);
                            i = allTapes.Count;
                        }
                    }

                    if (selectedTape == null) {
                        selectedTape = spawnNewTape();
                        selectedTape.gameObject.SetActive(true);
                    }

                    spot1 = collision.point;
                    spot.SetActive(false);
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                }

            } else {
                Vector3 tapePos = spot1;
                spot2 = collision.point;

                if (selectedTapeType.type != tapeType.createType.sprite) {
                    tapePos = spot2;
                    selectedTape.lineRenderer.positionCount = 2;
                    selectedTape.lineRenderer.SetPosition(0, spot1);
                    selectedTape.lineRenderer.SetPosition(1, spot2);
                    selectedTape.lineRenderer.startColor = selectedColor;
                    selectedTape.lineRenderer.endColor = selectedColor;


                } else {
                    selectedTape.lineRenderer.positionCount = 0;
                    float diameter = Vector3.Distance(spot1, spot2) * 2f;
                    selectedTape.transform.localScale = new Vector3(diameter, diameter, diameter);
                }
                selectedTape.spriteRenderer.sprite = selectedTapeType.sprite;
                selectedTape.spriteRenderer.color = selectedColor;

                Vector3 initialDirection = Vector3.Normalize(new Vector3(0f, spot2.y - spot1.y, spot2.z - spot1.z));
                Quaternion initialQuaternion = Quaternion.FromToRotation(Vector3.up, initialDirection);
                var test = initialQuaternion * Vector3.up;
                Vector3 facingDirection = Vector3.Normalize(spot2 - spot1);
                Quaternion test2 = Quaternion.FromToRotation(test, facingDirection);
                selectedTape.transform.eulerAngles = initialQuaternion.eulerAngles + test2.eulerAngles; 
                Quaternion additionalRotation = Quaternion.FromToRotation(selectedTape.transform.forward, Vector3.up);
                selectedTape.transform.position = tapePos;

                WorldUI.instance.measurements.onShowLength(spot1, spot2);

                if (PlayerScript.instance.Trigger.getButtonDown) {
                    selectedTape = null;
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                }
            }
        }
    }


    tape spawnNewTape()
    {
        GameObject newTapeObject = Instantiate(TapePrefab.gameObject) as GameObject;
        tape newTape = new tape();
        newTape.gameObject = newTapeObject;
        newTapeObject.transform.parent = tapeFolder.transform;
        newTapeObject.transform.localScale = tapeSize;
        newTapeObject.SetActive(false);
        return newTape;
    }
    #endregion

    #region Tape Trait Selector
    enum Selector { Color, Shape};
    Selector currentSelector = Selector.Color;
    public Transform selectionCircle;
    public Transform ColorParent;
    List<SpriteRenderer> allColorSprites = new List<SpriteRenderer>();
    public Transform ShapeParent;
    List<SpriteRenderer> allShapeSprites = new List<SpriteRenderer>();
    public GameObject UI;

    bool touchDown = false;
    Vector3 lastTouch = Vector3.zero;
    public float touchDistance = 0.5f;
    internal MLInputController _controller;
    public SpriteRenderer previewSprite;

    void SelectTapeType() {
        if (_controller == null) {
            _controller = MLInput.GetController(MLInput.Hand.Left);

            if (_controller == null) {
                _controller = MLInput.GetController(MLInput.Hand.Right);
            }
        }
        bool swap = false;
        if (allColorSprites.Count == 0 && allShapeSprites.Count == 0) {
            swap = true;
            for (int i = 0; i < ColorParent.childCount; i++) {
                SpriteRenderer rend = ColorParent.GetChild(i).GetComponent<SpriteRenderer>();
                if (allColors.Count > i) {
                    rend.color = allColors[i];
                } else {
                    rend.gameObject.SetActive(false);
                }

                allColorSprites.Add(rend);
            }

            for (int i = 0; i < ShapeParent.childCount; i++) {
                SpriteRenderer rend = ShapeParent.GetChild(i).GetComponent<SpriteRenderer>();
                if (allTapeTypes.Count > i) {
                    rend.sprite = allTapeTypes[i].previewSprite;
                } else {
                    rend.gameObject.SetActive(false);
                }

                allShapeSprites.Add(rend);
            }

            updatePreview();
        }
        if (_controller.Touch1Active) {
            if (touchDown) {
                touchDistance = GameMenu.instance.GetOptionStatusAsFloat("Scrolling Sensitivity");
                float dist = Vector2.Distance(new Vector2(lastTouch.x, lastTouch.y), new Vector2(_controller.Touch1PosAndForce.x, _controller.Touch1PosAndForce.y));

                if (Mathf.Abs(dist) > touchDistance) {
                    changeSelection();
                    lastTouch = _controller.Touch1PosAndForce;
                }

            } else {
                touchDown = true;
                lastTouch = _controller.Touch1PosAndForce;
                changeSelection();
            }
        } else {
            touchDown = false;
        }
       

        if (PlayerScript.instance.Bumper.getButtonDown) {
            if (currentSelector == Selector.Color) {
                currentSelector = Selector.Shape;
                for (int i = 0; i < allShapeSprites.Count; i++) {
                    allShapeSprites[i].color = selectedColor;
                }
            } else if (currentSelector == Selector.Shape) {
                currentSelector = Selector.Color;
            }

            PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
            swap = true;
        }

        if (swap) {
           switch (currentSelector) {
                case Selector.Color:
                    ColorParent.gameObject.SetActive(true);
                    ShapeParent.gameObject.SetActive(false);
                    selectionCircle.position = allColorSprites[selectedColorInt].transform.position;
                    break;

                case Selector.Shape:
                    ColorParent.gameObject.SetActive(false);
                    ShapeParent.gameObject.SetActive(true);
                    selectionCircle.position = allShapeSprites[selectedTypeInt].transform.position;
                    break;
            }
        }
        string otherSelector = "Color";

        if (currentSelector == Selector.Color) {
            otherSelector = "Shape";
        }

        UI_ControllerPrompts.instance.onChangePrompts("Select " + currentSelector.ToString(), "", "Swap to " + otherSelector, "Place");
    }
    void changeSelection() {
        int selection = 0;
        float closest = 9999999999f;

        if (currentSelector == Selector.Color) {
            for (int i = 0; i < allColors.Count; i++) {
                Vector3 norm = Vector3.Normalize(allColorSprites[i].transform.localPosition);
                float i_dist = Vector3.Distance(norm, _controller.Touch1PosAndForce);

                if (i_dist < closest) {
                    selection = i;
                    closest = i_dist;
                }
            }
            if (selectedColorInt != selection) {
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
                selectedColorInt = selection;
                selectionCircle.position = allColorSprites[selectedColorInt].transform.position;
            }
        } else if (currentSelector == Selector.Shape) {
            for (int i = 0; i < allTapeTypes.Count; i++) {
                Vector3 norm = Vector3.Normalize(allShapeSprites[i].transform.localPosition);
                float i_dist = Vector3.Distance(norm, _controller.Touch1PosAndForce);

                if (i_dist < closest) {
                    selection = i;
                    closest = i_dist;
                }
            }
            if (selectedTypeInt != selection) {
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
                selectedTypeInt = selection;
                selectionCircle.position = allShapeSprites[selectedTypeInt].transform.position;
            }
        }

        updatePreview();
    }

    void updatePreview() {
        previewSprite.sprite = selectedTapeType.previewSprite;
        previewSprite.color = selectedColor;
    }
    #endregion

}
