using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

#region Wall Builder Tool
public class Tool_WallBuilder : _Tool
{

    enum Mode { Build, Rotate, Design, Scale };
    Mode currentMode = Mode.Build;
    public List<GameObject> allControllerOptions;

    public Transform selectionCircle;
    bool touchDown = false;
    Vector3 lastTouch = Vector3.zero;
    public float touchDistance = 0.5f;
    internal MLInputController _controller;


    bool firstState = true;

    [Header("Build State")]
    public GameObject spot1;
    public GameObject spot2;
    public GameObject wall;
    public LineRenderer line;
    public Transform meshParent;

    internal _Wall selectedWall;
    internal _Wall HighlightingWall;

    GameObject environmentFolder;
    GameObject wallFolder;

    float highestYPoint = -999999999999999f;

    [Header("Design State")]
    public GameObject wallScroller;
    _WallScroller wallScrollerScript;
    public GameObject wallOptionPrefab;
    public List<Texture> wallTextures;
    public List<Color> wallColors;
    public GameObject upArrow;
    public GameObject downArrow;
    public GameObject leftArrow;
    public GameObject rightArrow;

    // Start is called before the first frame update
    void Start()
    {
        Tool_Start(gameObject);

        environmentFolder = new GameObject();
        environmentFolder.transform.position = Vector3.zero;
        environmentFolder.transform.localScale = Vector3.one;
        environmentFolder.transform.localEulerAngles = Vector3.zero;
        environmentFolder.name = "Wall Builder - Environment Folder";

        if (GameObject.Find("Environment") != null) {
            environmentFolder.transform.parent = GameObject.Find("Environment").transform;
        }

        wallFolder = new GameObject();
        wallFolder.name = "Wall Folder";
        wallFolder.transform.position = Vector3.zero;
        wallFolder.transform.localScale = Vector3.one;
        wallFolder.transform.localEulerAngles = Vector3.zero;

        wallFolder.transform.parent = environmentFolder.transform.parent;

        spot1.transform.parent = environmentFolder.transform;
        spot2.transform.parent = environmentFolder.transform;
        wall.AddComponent<_Wall>();
        wall.transform.parent = wallFolder.transform;

        for (int i = 0; i < 50; i++) {
            GameObject newWall = Instantiate(wall) as GameObject;
            newWall.transform.parent = wallFolder.transform;
            newWall.SetActive(false);
            newWall.GetComponent<MeshRenderer>().material.ActivateMaterial_changeTexture(wallTextures[0]);
            newWall.GetComponent<MeshRenderer>().material.ActivateMaterial_changeColor(wallColors[0]);
        }
        wall.SetActive(false);
        wall.GetComponent<MeshRenderer>().material.ActivateMaterial_changeTexture(wallTextures[0]);
        wall.GetComponent<MeshRenderer>().material.ActivateMaterial_changeColor(wallColors[0]);

        wallScrollerScript = wallScroller.AddComponent<_WallScroller>();
        wallScrollerScript.prefab = wallOptionPrefab;
        wallScrollerScript.wallTextures = wallTextures;
        wallScrollerScript.wallColors = wallColors;
        wallScrollerScript.upArrow = upArrow;
        wallScrollerScript.downArrow = downArrow;
        wallScrollerScript.leftArrow = leftArrow;
        wallScrollerScript.rightArrow = rightArrow;
        wallScrollerScript.SetUp();
        wallScroller.transform.parent = environmentFolder.transform;
        wallScroller.SetActive(false);
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
                firstState = true;
                line.positionCount = 0;
                displayProjection(false);
                spot1.SetActive(false);
                spot2.SetActive(false);
                selectedWall = null;
                wallScroller.SetActive(false);
            }
            return;
        }
        Tool_Update();
        if (firstState && selectedWall == null) {
            SelectMode();
        }

        switch (currentMode) {
            case Mode.Build:
                BuildState();
                break;

            case Mode.Design:
                DesignState();
                break;

            case Mode.Rotate:
                RotateState();
                break;

            case Mode.Scale:
                ScaleState();
                break;
        }

    }

    #region Build State
    void BuildState() {

        displayProjection(true);

        if (collision.gameObject != null) {
            if (firstState) {
                    line.positionCount = 0;
                    spot1.SetActive(true);
                    spot2.SetActive(false);
                    spot1.transform.position = collision.point;
                    spot1.transform.rotation = Quaternion.FromToRotation(Vector3.forward, collision.angleOfPoint);

                    if (PlayerScript.instance.Trigger.getButtonDown) {
                        firstState = false;
                        PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                    }
            } else {
                spot2.SetActive(true);
                spot2.transform.position = collision.point;
                spot2.transform.rotation = Quaternion.FromToRotation(Vector3.forward, collision.angleOfPoint);
                line.positionCount = 2;
                line.SetPosition(0, spot1.transform.position);
                line.SetPosition(1, spot2.transform.position);
                WorldUI.instance.measurements.onShowLength(line);
                if (PlayerScript.instance.Trigger.getButtonDown) {
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                    SpawnWall();
                }
            }
        }

        UI_ControllerPrompts.instance.onChangePrompts("Select Mode", "", "", "Place");
        
    }

    void SpawnWall()
    {
        GameObject selectedWall = null;

        for (int i = 0; i < wallFolder.transform.childCount; i++)
        {
            if (!wallFolder.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                selectedWall = wallFolder.transform.GetChild(i).gameObject;
                i = wallFolder.transform.childCount;
            }
        }

        if (selectedWall == null)
        {
            selectedWall = Instantiate(wall) as GameObject;
            selectedWall.transform.parent = wallFolder.transform;
        }

        selectedWall.SetActive(true);

        float yBottom = Mathf.Min(spot1.transform.position.y, spot2.transform.position.y);


        if (highestYPoint == -999999999999999f)
        {
            int highest = 0;
            float highestY = yBottom;
            for (int i = 0; i < meshParent.childCount; i++)
            {
                float thisY = meshParent.GetChild(i).GetComponent<MeshCollider>().bounds.center.y;
                if (thisY > highestY)
                {
                    highest = i;
                    highestY = thisY;
                }
            }

            highestYPoint = highestY;
        }

        float yTop = highestYPoint;

        float xLength = Vector2.Distance(new Vector2(spot1.transform.position.x, spot1.transform.position.z), new Vector2(spot2.transform.position.x, spot2.transform.position.z));
        float yLength = yTop - yBottom;
        float zLength = 0.05f;

        float xPos = (spot1.transform.position.x + spot2.transform.position.x) * 0.5f;
        float yPos = (yBottom + yTop) * 0.5f;
        float zPos = (spot1.transform.position.z + spot2.transform.position.z) * 0.5f;

        Vector3 direction = Vector3.Normalize(spot2.transform.position - spot1.transform.position);

        float yAngle = Quaternion.FromToRotation(Vector3.right, direction).eulerAngles.y;

        selectedWall.transform.position = new Vector3(xPos, yPos, zPos);
        selectedWall.transform.localEulerAngles = new Vector3(0f, yAngle, 0f);
        selectedWall.transform.localScale = new Vector3(xLength, yLength, zLength);

        firstState = true;

        Renderer renderer = selectedWall.GetComponent<Renderer>();
        renderer.material.mainTextureScale = new Vector2(xLength * 10f, yLength * 10f);
        selectedWall.GetComponent<_Wall>().onPlay();
    }
    #endregion

    #region Design State
    void DesignState() {
        displayProjection(true);
        if (selectedWall == null) {
            if (collision.gameObject != null) {
                if (collision.gameObject.GetComponent<_Wall>() != null) {
                    if (HighlightingWall == null || collision.gameObject != HighlightingWall.gameObject) {
                        PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
                    }
                    HighlightingWall = collision.gameObject.GetComponent<_Wall>();
                } else {
                    HighlightingWall = null;
                }
            }


            if (HighlightingWall != null){
                line.positionCount = 0;
                spot1.SetActive(false);
                spot2.SetActive(false);
                UI_ControllerPrompts.instance.onChangePrompts("Select Mode", "", "", "Select");
                if (PlayerScript.instance.Trigger.getButtonDown) {
                    selectedWall = HighlightingWall;
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                    wallScroller.SetActive(true);
                    wallScrollerScript.onSelectWall(selectedWall);
                }
            } else {
                UI_ControllerPrompts.instance.onChangePrompts("Select Mode", "", "", "");
            }
        } else {
            displayProjection(false);

            if (PlayerScript.instance.Trigger.getButtonDown) {
                selectedWall = null;
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                wallScroller.SetActive(false);
            }

            UI_ControllerPrompts.instance.onChangePrompts("Select Design", "", "", "Close");
        }
    }
    #endregion

    #region Scale State
    #region Scale State - Variables
    bool grabbingWall;
    Vector3 selectedPos;
    Vector3 selectedPosScaled {
        get {
            return new Vector3(selectedPos.x * selectedWall.transform.lossyScale.x, selectedPos.y * selectedWall.transform.lossyScale.y, selectedPos.z * selectedWall.transform.lossyScale.z);
        }
    }
    float selectedDist;
    int selectedSide; //0 = (-1, 0, 0) || 1 = (1, 0, 0) || 2 = (0, 0, -1) || 3 = (0, 0, 1) || 4 = (0, -1, 0) || 5 = (0, 1, 0)
    #endregion
    void ScaleState()
    {
        // This Mode allows the user to scale the wall or move it. Scaling works by selecting a side of a wall and moving that side
        displayProjection(true);

        #region Scale State - Selecting the Wall
        if (selectedWall == null) {
            if (collision.gameObject != null) {
                if (collision.gameObject.GetComponent<_Wall>() != null) {
                    if (HighlightingWall == null || collision.gameObject != HighlightingWall.gameObject) {
                        PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
                    }
                    HighlightingWall = collision.gameObject.GetComponent<_Wall>();
                } else {
                    HighlightingWall = null;
                }
            }

            if (HighlightingWall != null) {
                line.positionCount = 0;
                spot1.SetActive(false);
                spot2.SetActive(false);
                UI_ControllerPrompts.instance.onChangePrompts("Select Mode", "", "Grab Wall", "Grab Side");
                if (PlayerScript.instance.Trigger.getButtonDown || PlayerScript.instance.Bumper.getButtonDown) {
                    selectedWall = HighlightingWall;
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                    grabbingWall = PlayerScript.instance.Bumper.getButtonDown;
                    selectedPos = selectedWall.transform.InverseTransformPoint(collision.point);
                    selectedDist = Vector3.Distance(_controller.Position, collision.point);

                    if (PlayerScript.instance.Trigger.getButtonDown) {
                        float x = Mathf.Abs(Mathf.Abs(selectedPosScaled.x) - (selectedWall.transform.lossyScale.x * 0.5f));
                        float y = Mathf.Abs(Mathf.Abs(selectedPosScaled.y) - (selectedWall.transform.lossyScale.y * 0.5f));
                        float z = Mathf.Abs(Mathf.Abs(selectedPosScaled.z) - (selectedWall.transform.lossyScale.z * 0.5f));
                        float min = Mathf.Min(x, y, z);
                        if (min == x) {
                            if (selectedPos.x < 0) {
                                selectedSide = 0;
                            } else {
                                selectedSide = 1;
                            }
                        } else if (min == y) {
                            if (selectedPos.y < 0) {
                                selectedSide = 4;
                            } else {
                                selectedSide = 5;
                            }
                        } else {
                            if (selectedPos.z < 0) {
                                selectedSide = 2;
                            } else {
                                selectedSide = 3;
                            }
                        }

                    }
                }
            } else {
                UI_ControllerPrompts.instance.onChangePrompts("Select Mode", "", "", "");
            }
            #endregion
        #region Scale State - Grabbing and Moving Wall
        }
        else {
            Vector3 newPos = _controller.Position + (transform.forward * selectedDist);

            if (grabbingWall) {
                Vector3 oldPos = selectedWall.transform.TransformPoint(selectedPos);
                Vector3 diff = newPos - oldPos;
                diff = new Vector3(diff.x, 0f, diff.z);
                selectedWall.transform.position += diff;
                UI_ControllerPrompts.instance.onChangePrompts("", "", "Release Wall", "");

                if (!PlayerScript.instance.Bumper.getButton) {
                    selectedWall = null;
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                }
                
            }
            #endregion
        #region Scale State - Grabbing a side of a wall and moving it
            else
            {

            Vector3 oldPos = selectedWall.transform.TransformPoint(selectedPos);
                Vector3 diff = selectedWall.transform.InverseTransformPoint( newPos - oldPos);
                newPos = selectedWall.transform.InverseTransformPoint(newPos);
                Vector3 oldScale = selectedWall.transform.localScale;
                Vector3 otherPos1 = selectedWall.transform.position;
                Vector3 otherPos2 = selectedWall.transform.position;
                switch (selectedSide)
                {
                    case 0:
                        newPos = new Vector3(newPos.x, 1f, 1f);
                        break;

                    case 1:
                        newPos = new Vector3(newPos.x, 1f, 1f);
                        break;

                    case 4:
                        newPos = new Vector3(1f, newPos.y, 1f);
                        break;

                    case 5:
                        newPos = new Vector3(1f, newPos.y, 1f);
                        break;

                    case 2:
                        newPos = new Vector3(1f, 1f, newPos.z); 
                        break;

                    case 3:
                        newPos = new Vector3(1f, 1f, newPos.z);
                        break;
                }
                otherPos1 = otherSideOfWall();
                selectedWall.transform.localScale = new Vector3(Mathf.Abs(newPos.x * oldScale.x), Mathf.Abs(newPos.y * oldScale.y), Mathf.Abs(newPos.z * oldScale.z));

                otherPos2 = otherSideOfWall();

                selectedWall.transform.position += otherPos1 - otherPos2;

                UI_ControllerPrompts.instance.onChangePrompts("", "", "", "Release Side");

                if (!PlayerScript.instance.Trigger.getButton) {
                    selectedWall = null;
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                }
            }
        }
        #endregion
    }

    #region Sides of the Wall

    //Selects the World Position of the opposite side of the selected side of a wall
    Vector3 otherSideOfWall()
    {
        switch (selectedSide)
        {
            case 0:
                return selectedWall.transform.TransformPoint(new Vector3(selectedWall.GetComponent<BoxCollider>().bounds.extents.x, 0f, 0f));

            case 1:
                return selectedWall.transform.TransformPoint(new Vector3(-selectedWall.GetComponent<BoxCollider>().bounds.extents.x, 0f, 0f));

            case 4:
                return selectedWall.transform.TransformPoint(new Vector3(0f, selectedWall.GetComponent<BoxCollider>().bounds.extents.y, 0f));

            case 5:
                return selectedWall.transform.TransformPoint(new Vector3(0f, -selectedWall.GetComponent<BoxCollider>().bounds.extents.y, 0f));

            case 2:
                return selectedWall.transform.TransformPoint(new Vector3(0f, 0f, selectedWall.GetComponent<BoxCollider>().bounds.extents.z));

            case 3:
                return selectedWall.transform.TransformPoint(new Vector3(0f, 0f, -selectedWall.GetComponent<BoxCollider>().bounds.extents.z));
        }

        Debug.LogError("Error: Wall isn't selected or there's no selected side");
        return Vector3.zero;
    }

    //Selects the World Positions of the selected side of a wall
    Vector3 sideOfWall()
    {
        switch (selectedSide)
        {
            case 1:
                return selectedWall.transform.TransformPoint(new Vector3(selectedWall.GetComponent<BoxCollider>().bounds.extents.x, 0f, 0f));

            case 0:
                return selectedWall.transform.TransformPoint(new Vector3(-selectedWall.GetComponent<BoxCollider>().bounds.extents.x, 0f, 0f));

            case 5:
                return selectedWall.transform.TransformPoint(new Vector3(0f, selectedWall.GetComponent<BoxCollider>().bounds.extents.y, 0f));

            case 4:
                return selectedWall.transform.TransformPoint(new Vector3(0f, -selectedWall.GetComponent<BoxCollider>().bounds.extents.y, 0f));

            case 3:
                return selectedWall.transform.TransformPoint(new Vector3(0f, 0f, selectedWall.GetComponent<BoxCollider>().bounds.extents.z));

            case 2:
                return selectedWall.transform.TransformPoint(new Vector3(0f, 0f, -selectedWall.GetComponent<BoxCollider>().bounds.extents.z));
        }

        Debug.LogError("Error: Wall isn't selected or there's no selected side");
        return Vector3.zero;
    }
    #endregion
    #endregion

    #region Rotate State
    /// <summary>
    /// NOTE FROM JEFF:
    /// 
    /// The ability to Rotate is not properly implemented and I recommend not touching it. Whenever I get access to a Magic Leap again, I will complete this task.
    /// In the event you need to have the Rotate Wall function completed before I get back to it, Here's how it was going to work:
    /// 
    /// 1. The User can select a side of a wall by pointing and pressing the trigger button
    /// 2. While holding down the trigger, the user can point where the selected side will be rotated towards (with the other side of the wall staying still)
    /// 
    /// The "CircularLinearCollision" class is to detect if the controller is pointing to a spot where the wall can be rotated to and, if so, where.
    /// </summary>
    void RotateState() {
        displayProjection(true);
        if (selectedWall == null)
        {
            if (collision.gameObject != null)
            {
                if (collision.gameObject.GetComponent<_Wall>() != null)
                {
                    if (HighlightingWall == null || collision.gameObject != HighlightingWall.gameObject)
                    {
                        PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
                    }
                    HighlightingWall = collision.gameObject.GetComponent<_Wall>();
                }
                else
                {
                    HighlightingWall = null;
                }
            }

            if (HighlightingWall != null)
            {
                line.positionCount = 0;
                spot1.SetActive(false);
                spot2.SetActive(false);
                UI_ControllerPrompts.instance.onChangePrompts("Select Mode", "", "Grab Wall", "");
                if (PlayerScript.instance.Trigger.getButtonDown)
                {
                    selectedWall = HighlightingWall;
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
                    selectedPos = selectedWall.transform.InverseTransformPoint(collision.point);

                    float x = Mathf.Abs(Mathf.Abs(selectedPosScaled.x) - (selectedWall.transform.lossyScale.x * 0.5f));
                    float y = Mathf.Abs(Mathf.Abs(selectedPosScaled.y) - (selectedWall.transform.lossyScale.y * 0.5f));
                    float z = Mathf.Abs(Mathf.Abs(selectedPosScaled.z) - (selectedWall.transform.lossyScale.z * 0.5f));
                    float min = Mathf.Min(x, y, z);
                    if (min == x)
                    {
                            if (selectedPos.x < 0)
                            {
                                selectedSide = 0;
                            }
                            else
                            {
                                selectedSide = 1;
                            }
                        }
                        else if (min == y)
                        {
                            if (selectedPos.y < 0)
                            {
                                selectedSide = 4;
                            }
                            else
                            {
                                selectedSide = 5;
                            }
                        }
                        else
                        {
                            if (selectedPos.z < 0)
                            {
                                selectedSide = 2;
                            }
                            else
                            {
                                selectedSide = 3;
                            }
                        }

                        Debug.Log(selectedSide + "/" + selectedPosScaled + "/" + selectedWall.transform.lossyScale * 2f);
                    
                }
            }
            else
            {
                UI_ControllerPrompts.instance.onChangePrompts("Select Mode", "", "", "");
            }
        }
        else
        {
            CircularLinearCollision clc = new CircularLinearCollision();
            Vector3 cent = otherSideOfWall();
            clc.circleCenter = new Vector2(cent.x, cent.z);
            //CenterIndicator.position = new Vector3(clc.circleCenter.x, cent.y, clc.circleCenter.y);
            Vector3 oldPos = selectedWall.transform.TransformPoint(new Vector3(selectedPos.x, 0f, selectedPos.z));

            clc.circleRadius = Vector3.Distance(cent, oldPos);
            /*
            switch (selectedSide)
            {
                case 0:
                    clc.circleRadius = selectedWall.bounds.x;
                    break;

                case 1:
                    clc.circleRadius = selectedWall.bounds.x;
                    break;

                case 4:
                    clc.circleRadius = selectedWall.bounds.y;
                    break;

                case 5:
                    clc.circleRadius = selectedWall.bounds.y;
                    break;

                case 2:
                    clc.circleRadius = selectedWall.bounds.z;
                    break;

                case 3:
                    clc.circleRadius = selectedWall.bounds.z;
                    break;
            }
            */
            clc.setLinearEquation(gameObject);
            Vector3 connectionPoint = Vector3.zero;
            Color lineColor = Color.green;
            if (clc.isConnected)
            {
                List<Vector2> _allPoints = clc.pointsOfCollision;
                List<Vector3> allPoints = new List<Vector3>();

                int selectedPoint = -999;
                float dist = 999999999999999999f;

                for (int i = 0; i < _allPoints.Count; i++)
                {
                    allPoints.Add(new Vector3(_allPoints[i].x, selectedWall.transform.position.y, _allPoints[i].y));
                    float _dist = Vector3.Distance(transform.position, allPoints[i]);

                    if (_dist < dist)
                    {
                        dist = _dist;
                        selectedPoint = i;
                    }
                }

                if (selectedPoint != -999)
                {
                    connectionPoint = allPoints[selectedPoint];
                    selectedDist = dist;
                    Debug.Log("Hit Circle");
                    lineColor = Color.red;
                } else
                {
                    connectionPoint = transform.position + (transform.forward * selectedDist);
                    connectionPoint = new Vector3(connectionPoint.x, selectedWall.transform.position.y, connectionPoint.z);
                    Debug.Log("Miss Circle");
                }
            } else
            {
                connectionPoint = transform.position + (transform.forward * selectedDist);
                connectionPoint = new Vector3(connectionPoint.x, selectedWall.transform.position.y, connectionPoint.z);
                Debug.Log("Miss Circle");
            }

            Debug.DrawLine(transform.position, connectionPoint, lineColor);
            /*
            Vector3 oldPos = selectedWall.transform.TransformPoint(new Vector3(selectedPos.x, 0f, selectedPos.z));
            Vector3 dir1 = Vector3.Normalize(oldPos - cent);
            Vector3 dir2 = Vector3.Normalize(connectionPoint - cent);
            Quaternion angleChange = Quaternion.FromToRotation(dir1, dir2);
            selectedWall.transform.localEulerAngles += new Vector3(0f, angleChange.y, 0f);

            //selectedWall.transform.Rotate(angleChange.eulerAngles);
            Vector3 newPos = otherSideOfWall();
            selectedWall.transform.position += cent - newPos;
            */

            //NOTE TO JEFF: TEST THIS OUT

            UI_ControllerPrompts.instance.onChangePrompts("", "", "", "Release Side");

            if (!PlayerScript.instance.Trigger.getButton)
            {
                selectedWall = null;
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
            }
        }
    }

    class CircularLinearCollision
    {
        public Vector2 circleCenter;
        public float circleRadius;

        float Linear_A;
        float Linear_B;
        enum Direction { up, upRight, upLeft, down, downRight, downLeft, right, left }
        Direction Linear_Direction = new Direction();
        Vector2 corePosition;
        public void setLinearEquation(GameObject coreObject)
        {
            Vector3 _pointA = coreObject.transform.position;
            Vector3 _pointB = coreObject.transform.position + coreObject.transform.forward;

            Vector2 pointA = new Vector2(_pointA.x, _pointA.z);
            Vector2 pointB = new Vector2(_pointB.x, _pointB.z);

            if (pointB.x > pointA.x)
            {
                if (pointB.y > pointA.y)
                {
                    Linear_Direction = Direction.upRight;
                }
                else if (pointB.y < pointA.y)
                {
                    Linear_Direction = Direction.downRight;
                }
                else
                {
                    Linear_Direction = Direction.right;
                }
            }
            else if (pointB.x < pointA.x)
            {
                if (pointB.y > pointA.y)
                {
                    Linear_Direction = Direction.upLeft;
                }
                else if (pointB.y < pointA.y)
                {
                    Linear_Direction = Direction.downLeft;
                }
                else
                {
                    Linear_Direction = Direction.left;
                }
            }
            else
            {
                if (pointB.y > pointA.y)
                {
                    Linear_Direction = Direction.up;
                }
                else
                {
                    Linear_Direction = Direction.down;
                }
            }
            corePosition = new Vector2(coreObject.transform.position.x, coreObject.transform.position.z);

            Linear_A = (pointB.y - pointA.y) / (pointB.x - pointA.x);
            Linear_B = pointA.y - (Linear_A * pointA.x);
        }

        bool isInFront(Vector2 point)
        {
            bool _isInFront = true;
            if (Linear_Direction.ToString().Contains("Right") || Linear_Direction.ToString().Contains("right"))
            {
                if (point.x <= corePosition.x)
                {
                    _isInFront = false;
                }
            }

            if (Linear_Direction.ToString().Contains("Left") || Linear_Direction.ToString().Contains("left"))
            {
                if (point.x >= corePosition.x)
                {
                    _isInFront = false;
                }
            }

            if (Linear_Direction == Direction.up || Linear_Direction == Direction.down)
            {
                if (point.x != corePosition.x)
                {
                    _isInFront = false;
                }
            }

            if (Linear_Direction.ToString().Contains("Up") || Linear_Direction.ToString().Contains("up"))
            {
                if (point.y <= corePosition.y)
                {
                    _isInFront = false;
                }
            }

            if (Linear_Direction.ToString().Contains("Down") || Linear_Direction.ToString().Contains("down"))
            {
                if (point.y >= corePosition.y)
                {
                    _isInFront = false;
                }
            }

            if (Linear_Direction == Direction.left || Linear_Direction == Direction.right)
            {
                if (point.y != corePosition.y)
                {
                    _isInFront = false;
                }
            }

            return _isInFront;
        }

        public bool isConnected
        {
            get
            {
                /// d = Linear_B - circleCenter.y
                /// (x - circleCenter.x)^2 + ((Linear_A * x) + d)^2 = r ^ 2
                /// (x^2 - (2 * circleCenter.x * x) + circleCenter.x^2)
                /// +
                /// ((Linear_A * x)^2 + (2 * d * Linear_A * x) d^2)
                /// - r^2
                /// 
                /// a = Linear_A + 1
                /// b = (2 * circleCenter.x) + (2 * d * Linear_A)
                /// c = (circleCenter.x ^ 2) + (d^2) - (r^2)
                /// 

                float d = Linear_B - circleCenter.y;
                float a = Linear_A + 1;
                float b = (-2 * circleCenter.x) + (2 * d * Linear_A);
                float c = Mathf.Pow(circleCenter.x, 2) + Mathf.Pow(d, 2) + Mathf.Pow(circleRadius, 2);

                float f = 1f / (2 * a);

                if (Mathf.Pow(b, 2) >= (4 * a * c))
                {

                    float e = Mathf.Sqrt(Mathf.Pow(b, 2) - (4 * a * c));
                    float x1 = ((-1f * b) + e) / (2 * a);
                    float y1 = (Linear_A * x1) + Linear_B;
                    if (isInFront(new Vector2(x1, y1)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public List<Vector2> pointsOfCollision
        {
            get
            {
                float d = Linear_B - circleCenter.y;
                float a = Linear_A + 1;
                float b = (-2 * circleCenter.x) + (2 * d * Linear_A);
                float c = Mathf.Pow(circleCenter.x, 2) + Mathf.Pow(d, 2) + Mathf.Pow(circleRadius, 2);
                float f = 1f / (2 * a);

                List<Vector2> allPoints = new List<Vector2>();

                if (Mathf.Pow(b, 2) == (4 * a * c))
                {
                    float x = (-1f * b) / (2 * a);
                    float y = (Linear_A * x) + Linear_B;
                    allPoints.Add(new Vector2(x, y));
                }
                else
                {
                    float e = Mathf.Sqrt(Mathf.Pow(b, 2) - (4 * a * c));
                    float x1 = ((-1f * b) + e) / (2 * a);
                    float x2 = ((-1f * b) - e) / (2 * a);
                    float y1 = (Linear_A * x1) + Linear_B;
                    float y2 = (Linear_A * x2) + Linear_B;

                    allPoints.Add(new Vector2(x1, y1));
                    allPoints.Add(new Vector2(x2, y2));
                }

                return allPoints;
            }
        }
    }

    #endregion

    #region Change State
    void SelectMode() {
        if (_controller == null) {
            _controller = MLInput.GetController(MLInput.Hand.Left);

            if (_controller == null) {
                _controller = MLInput.GetController(MLInput.Hand.Right);
            }
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
    }

    void changeSelection() {
        int selection = 0;
        float closest = 9999999999f;
        for (int i = 0; i < allControllerOptions.Count; i++) {
            Vector3 norm = Vector3.Normalize(allControllerOptions[i].transform.localPosition);
            float i_dist = Vector3.Distance(norm, _controller.Touch1PosAndForce);

            if (i_dist < closest) {
                selection = i;
                closest = i_dist;
            }
        }
        if ((int)currentMode != selection) {
            PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
            currentMode = (Mode)selection;
            selectionCircle.position = allControllerOptions[selection].transform.position;
        }
    }
    #endregion
}
#endregion

#region Wall Behaviour
public class _Wall: MonoBehaviour {

    #region Variables
    bool OldState = false;
    internal MeshRenderer meshRenderer;
    Tool_WallBuilder wallBuilder;
    BoxCollider col;
    internal Vector3 bounds {
        get {
            if (col == null) {
                col = GetComponent<BoxCollider>();
            }

            return col.bounds.extents;
        }
    }
    #endregion

    #region Growing Animation
    public void onPlay() {
        wallBuilder = FindObjectOfType<Tool_WallBuilder>();
        meshRenderer = GetComponent<MeshRenderer>();
        StartCoroutine(build());
    }

    IEnumerator build() {
        float height = transform.localScale.y;
        float ground = transform.position.y - (height * 0.5f);
        float originalY = transform.position.y;

        transform.localScale = new Vector3(transform.localScale.x, 0f, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, ground, transform.position.z);

        while (transform.localScale.y < height) {
            PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Buzz, MLInputControllerFeedbackIntensity.Medium);
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Clamp(transform.localScale.y + (3f * Time.deltaTime), 0f, height), transform.localScale.z);
            transform.position = new Vector3(transform.position.x, ground + (transform.lossyScale.y * 0.5f), transform.position.z);
            yield return null;
        }

        transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
    }
    #endregion

    #region Change Wall State
    private void Update() {
        bool isActivated = true;

        if (wallBuilder.canUse && (wallBuilder.selectedWall != this && wallBuilder.HighlightingWall != this)) {
            isActivated = false;
        }

        if (OldState != isActivated) {
            OldState = isActivated;
            meshRenderer.material.ActivateMaterial_changeActivation(isActivated);
        }
    }
    #endregion

    #region Change Variables
    public void onChangeTexture(Texture texture) {
        meshRenderer.material.ActivateMaterial_changeTexture(texture);
    }

    public void onChangeColor(Color color) {
        meshRenderer.material.ActivateMaterial_changeColor(color);
    }
    #endregion 
}
#endregion

#region Wall Variable Changer
public class _WallScroller: MonoBehaviour {
    #region Variables
    GameObject textureFolder;
    GameObject colorFolder;

    List<GameObject> allTextureObjects = new List<GameObject>();
    List<GameObject> allColorObjects = new List<GameObject>();

    internal GameObject prefab;
    internal List<Texture> wallTextures;
    internal List<Color> wallColors;

    float colorAngles;
    float textureAngles;

    internal _Wall selectedWall;

    int selectedColor = 0;
    int selectedTexture = 0;

    Vector3 fullScale;
    Vector3 shrunkScale;
    Vector3 returnScale(bool isSelected) {
        if (isSelected) {
            return fullScale;
        } else {
            return shrunkScale;
        }
    }

    internal Transform arrowParent;
    internal GameObject upArrow;
    internal GameObject downArrow;
    internal GameObject rightArrow;
    internal GameObject leftArrow;
    #endregion

    public void SetUp() {
        textureFolder = new GameObject();
        textureFolder.transform.parent = transform;
        textureFolder.name = "Texture Folder";

        colorFolder = new GameObject();
        colorFolder.transform.parent = transform;
        colorFolder.name = "Color Folder";

        bool usedPrefab = false;
        textureAngles = 360f / ((float)wallTextures.Count);

        Texture originalTexture = prefab.GetComponent<MeshRenderer>().material.mainTexture;

        for (int i = 0; i < wallTextures.Count; i++) {
            Material newMaterial = prefab.GetComponent<MeshRenderer>().material;
            GameObject newObject = prefab;
            if (!usedPrefab) {
                usedPrefab = true;
            } else {
                newObject = Instantiate(prefab) as GameObject;
                newObject.transform.parent = prefab.transform.parent;
                newObject.transform.position = prefab.transform.position;
                newObject.transform.localScale = prefab.transform.localScale;
                newMaterial = newObject.GetComponent<MeshRenderer>().material;
            }
            newMaterial.ActivateMaterial_changeColor(Color.white);
            newMaterial.ActivateMaterial_changeTexture(wallTextures[i]);
            newObject.transform.parent = textureFolder.transform;
            newObject.transform.localEulerAngles = new Vector3(0f, i * textureAngles, 0f);
            allTextureObjects.Add(newObject);
        }

        colorAngles = 360f / ((float)wallColors.Count);

        for (int i = 0; i < wallColors.Count; i++) {
            Material newMaterial = prefab.GetComponent<MeshRenderer>().material;
            GameObject newObject = prefab;
            if (!usedPrefab) {
                usedPrefab = true;
            } else {
                newObject = Instantiate(prefab) as GameObject;
                newObject.transform.parent = prefab.transform.parent;
                newObject.transform.position = prefab.transform.position;
                newObject.transform.localScale = prefab.transform.localScale;
                newMaterial = newObject.GetComponent<MeshRenderer>().material;
            }
            newMaterial.ActivateMaterial_changeColor(wallColors[i]);
            newMaterial.ActivateMaterial_changeTexture(originalTexture);
            newObject.transform.parent = colorFolder.transform;
            newObject.transform.localEulerAngles = new Vector3(0f, i * colorAngles, 0f);
            allColorObjects.Add(newObject);
        }

        Vector3 upInch = Vector3.up * prefab.GetComponent<MeshRenderer>().bounds.extents.y * prefab.transform.localScale.y * 4f;
        textureFolder.transform.position -= upInch;
        colorFolder.transform.position += upInch;

        fullScale = prefab.transform.localScale;
        shrunkScale = fullScale * 0.5f;

        arrowParent = upArrow.transform.parent;
    }

    // Get variables of selected wall
    public void onSelectWall(_Wall _selectedWall) {
        selectedWall = _selectedWall;
        selectingTexture = true;

        for (int i = 0; i < wallTextures.Count; i++) {
            if (_selectedWall.meshRenderer.material.ActiveMaterial_getTexture() == wallTextures[i]) {
                selectedTexture = i;
                i = wallTextures.Count;
            }
        }

        for (int i = 0; i < wallColors.Count; i++) {
            if (_selectedWall.meshRenderer.material.ActiveMaterial_getColor() == wallColors[i]) {
                selectedColor = i;
                i = wallColors.Count;
            }
        }

        swapDesign();
    }

    // Change the Color or Texture of Wall - Visual
    int oldSelectedTexture = 0;
    int oldSelectedColor = 0;
    void swapDesign() {
        for (int i = 0; i < allTextureObjects.Count; i++) {
            allTextureObjects[i].GetComponent<MeshRenderer>().material.ActivateMaterial_changeActivation(selectedTexture == i && selectingTexture);
            allTextureObjects[i].transform.localScale = returnScale(selectedTexture == i && selectingTexture);
        }

        for (int i = 0; i < allColorObjects.Count; i++) {
            allColorObjects[i].GetComponent<MeshRenderer>().material.ActivateMaterial_changeActivation(selectedColor == i && !selectingTexture);
            allColorObjects[i].transform.localScale = returnScale(selectedColor == i && !selectingTexture);
        }

        

            upArrow.SetActive(selectingTexture);
            downArrow.SetActive(!selectingTexture);

        if (oldSelectedColor == selectedColor && oldSelectedTexture == selectedTexture) {
            return;
        }

        oldSelectedColor = selectedColor;
        oldSelectedTexture = selectedTexture;

        textureFolder.transform.localEulerAngles = new Vector3(0f, 360f - (((float)selectedTexture) * textureAngles), 0f);
        colorFolder.transform.localEulerAngles = new Vector3(0f, 360f - (((float)selectedColor) * colorAngles), 0f);

        selectedWall.meshRenderer.material.ActivateMaterial_changeColor(wallColors[selectedColor]);
        selectedWall.meshRenderer.material.ActivateMaterial_changeTexture(wallTextures[selectedTexture]);
    }

    bool selectingTexture;

    bool touchDown = false;
    Vector3 lastTouch = Vector3.zero;
    float touchDistance = 0.1f;
    internal MLInputController _controller;
    private void Update() {
        // Code to Adjust position of UI so that it's always in front of the wall and facing the camera
        transform.position = Camera.main.transform.position;
        Vector3 endPos = new Vector3(selectedWall.transform.position.x, transform.position.y, selectedWall.transform.position.z);
        Quaternion newRot = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(endPos - transform.position));
        transform.rotation = newRot;
        float dist = 99999999999f;

        for (int i = 0; i < 3; i++) {
            RaycastHit hit;
            float _dist = 0f;
            switch (i) {
                case 0:
                    if (Physics.Linecast(transform.position, endPos, out hit)) {
                        _dist = Vector3.Distance(transform.position, hit.point);
                        if (_dist != 0f) {
                            dist = Mathf.Min(_dist - 0.1f, dist);
                        }
                    }
                    break;

                case 1:
                    if (Physics.Linecast(transform.position, leftArrow.transform.position, out hit)) {
                        _dist = Vector3.Distance(transform.position, hit.point);
                        if (_dist != 0f) {
                            dist = Mathf.Min(_dist - 0.1f, dist);
                        }
                    }
                    break;

                case 2:
                    if (Physics.Linecast(transform.position, rightArrow.transform.position, out hit)) {
                        _dist = Vector3.Distance(transform.position, hit.point);
                        if (_dist != 0f) {
                            dist = Mathf.Min(_dist - 0.1f, dist);
                        }
                    }
                    break;
            }
        }

        for (int i = 0; i < allTextureObjects.Count; i++) {
            allTextureObjects[i].transform.position = textureFolder.transform.position + (allTextureObjects[i].transform.forward * dist);
        }

        for (int i = 0; i < allColorObjects.Count; i++) {
            allColorObjects[i].transform.position = colorFolder.transform.position + (allColorObjects[i].transform.forward * dist);
        }

        //Move Arrows to match either Texture Selection or Color
        if (selectingTexture) {
            arrowParent.position = allTextureObjects[selectedTexture].transform.position;
            arrowParent.rotation = allTextureObjects[selectedTexture].transform.rotation;
        } else {
            arrowParent.position = allColorObjects[selectedColor].transform.position;
            arrowParent.rotation = allColorObjects[selectedColor].transform.rotation;
        }
        arrowParent.transform.position -= arrowParent.transform.forward * 0.1f;

        if (_controller == null) {
            _controller = PlayerScript.instance._controller;
        }
        if (_controller == null) {
            return;
        }

        // Change Texture or Color - Input
        if (_controller.Touch1Active) {
            if (touchDown) {
                touchDistance = GameMenu.instance.GetOptionStatusAsFloat("Scrolling Sensitivity");
                float distX = _controller.Touch1PosAndForce.x - lastTouch.x;
                float distY = _controller.Touch1PosAndForce.y - lastTouch.y;

                if (Mathf.Abs(distY) > touchDistance) {
                    if (distY > 0) {
                        selectingTexture = false;
                    } else {
                        selectingTexture = true;
                    }
                    lastTouch = _controller.Touch1PosAndForce;
                    swapDesign();
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
                }

                if (Mathf.Abs(distX) > touchDistance) {
                    int moveAmount = -1;
                    if (distX > 0) {
                        moveAmount = 1;
                    } 

                    if (selectingTexture) {
                        selectedTexture += moveAmount;
                        selectedTexture = (selectedTexture + allTextureObjects.Count) % allTextureObjects.Count;
                    } else {
                        selectedColor += moveAmount;
                        selectedColor = (selectedColor + allColorObjects.Count) % allColorObjects.Count;
                    }

                    lastTouch = _controller.Touch1PosAndForce;
                    swapDesign();
                    PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
                }
            } else {
                touchDown = true;
                lastTouch = _controller.Touch1PosAndForce;
            }
        } else {
            touchDown = false;
        }
    }
}
#endregion
