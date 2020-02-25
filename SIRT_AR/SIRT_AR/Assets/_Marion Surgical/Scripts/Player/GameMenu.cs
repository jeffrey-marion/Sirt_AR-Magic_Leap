using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;

public class GameMenu : MonoBehaviour
{
    public static GameMenu instance;
    bool completeSetUp = false;
    class ToolFolder {
        internal GameObject folder;
        internal List<_Tool> allTools = new List<_Tool>();

        internal List<string> allToolNames {
            get {
                List<string> results = new List<string>();

                for (int i = 0; i < allTools.Count; i++) {
                    results.Add(allTools[i]._transform.name);
                }

                return results;
            }
        }
    }
    List<ToolFolder> allToolFolders = new List<ToolFolder>();

    [System.Serializable]
    public class ListVisual {
        public GameObject prefab;
        public GameObject textPrefab;
        [Range(1, 7)]
        public int MenuLayer;
        Transform scrollerObject {
            get {
                return textPrefab.transform.parent;
            }
        }

        float scrollSize {
            get {
                return scrollerObject.transform.parent.GetComponent<RectTransform>().rect.height;
            }
        }

        List<Transform> allOptions = new List<Transform>();

        internal int LengthOfOptions {
            get {
                return allOptions.Count;
            }
        }

        internal GameMenuData gameMenuData;

        internal void gameMenuDataSetUp() {
            gameMenuData = prefab.AddComponent<GameMenuData>();
            gameMenuData.TextObject = textPrefab;
        }
        internal bool isSetUp = false;
        internal void SetUp(List<string> options,
            bool AddBackOption = false) {
            if (options.Count < 1) {
                Debug.LogError("No Options available for Set Up of List Visual");
            }
            isSetUp = true;
            if (allOptions.Count == 0) {
                allOptions.Add(textPrefab.transform);
            }
            allOptions[0].GetComponent<Text>().text = options[0];

            for (int i = 1; i < options.Count; i++) {
                if (allOptions.Count < i + 1) {
                    GameObject newText = Instantiate(textPrefab) as GameObject;
                    newText.transform.parent = scrollerObject;
                    newText.transform.localScale = textPrefab.transform.localScale;
                    newText.transform.localPosition = new Vector3(newText.transform.localPosition.x, newText.transform.localPosition.y, 0f);
                    newText.transform.localEulerAngles = textPrefab.transform.localEulerAngles;
                    allOptions.Add(newText.transform);
                }
                allOptions[i].GetComponent<Text>().text = options[i];
            }

            if (AddBackOption) {
                int newOption = options.Count;

                if (allOptions.Count < newOption + 1) {
                    GameObject newText = Instantiate(textPrefab) as GameObject;
                    newText.transform.parent = scrollerObject;
                    newText.transform.localScale = textPrefab.transform.localScale;
                    newText.transform.localPosition = new Vector3(newText.transform.localPosition.x, newText.transform.localPosition.y, 0f);
                    newText.transform.localEulerAngles = textPrefab.transform.localEulerAngles;
                    allOptions.Add(newText.transform);
                }
                allOptions[newOption].GetComponent<Text>().text = "Back";

                if (allOptions[newOption].GetComponentInChildren<Toggle>() != null) {
                    Destroy(allOptions[newOption].GetChild(0).gameObject);
                }

            }
        }
        /*
        internal void SetUpToggles_OneOption(int selectedValue) {
            for (int i = 0; i < allOptions.Count; i++) {
                if (allOptions[i].GetComponentInChildren<Toggle>() != null) {
                    allOptions[i].GetComponentInChildren<Toggle>().isOn = selectedValue == i;
                }
            }
        }

        internal void SetUpToggles(int selectedToggle, bool isOn) {
            if (allOptions[selectedToggle] == null) {
                Debug.LogError("This option doesn't have a toggle");
                return;
            }

            if (allOptions[selectedToggle].GetComponentInChildren<Toggle>() != null) {
                allOptions[selectedToggle].GetComponentInChildren<Toggle>().isOn = isOn;
            }
        }
        */
        internal void UpdateAdditionalInfo(int selected, string info) {
            if (allOptions[selected] == null) {
                Debug.Log("This option doesn't exist");
                return;
            }

            allOptions[selected].GetChild(0).GetComponent<Text>().text = info;
        }

        internal Vector3 getPosition(int selectedObject) {
            if (selectedObject < 0 || selectedObject >= allOptions.Count) {
                Debug.LogError("No position available - " + selectedObject);
                return Vector3.zero;
            }
            Transform selectedTransform = allOptions[selectedObject];

            float selected_y_height = selectedTransform.GetComponent<RectTransform>().rect.height;
            float selected_y_top = selectedTransform.localPosition.y;
            float selected_y_bottom = selectedTransform.localPosition.y - (selected_y_height);

            float scrollAmount = scrollerObject.GetComponent<RectTransform>().anchoredPosition.y;

            //Check to see if selected if above the top
            if (selected_y_top + scrollAmount >= 0f) {
                scrollerObject.transform.localPosition += (scrollAmount + selected_y_top + selected_y_height) * Vector3.down;
            }

            //Check to see if selected is below the bottom
            if (selected_y_bottom + scrollAmount <= -scrollSize) {
                scrollerObject.transform.localPosition += (selected_y_bottom + scrollAmount + scrollSize) * Vector3.down;
            }

            return selectedTransform.position;
        }

        internal void duplicate(ListVisual original, int layer) {
            GameObject newList = Instantiate(original.prefab) as GameObject;
            newList.transform.parent = original.prefab.transform.parent;
            newList.transform.localPosition = original.prefab.transform.localPosition;
            newList.transform.localScale = original.prefab.transform.localScale;
            newList.transform.localEulerAngles = original.prefab.transform.localEulerAngles;

            prefab = newList;
            if (textPrefab == null) {
                if (prefab.GetComponent<GameMenuData>() == null) {
                    Debug.LogError("List Visual Not set up correctly");
                }
                textPrefab = prefab.GetComponent<GameMenuData>().TextObject;
            }

            MenuLayer = layer;
        }
    }

    [System.Serializable]
    public class option {
        public string optionName;
        public List<string> options;
        public int initialOption = 0;

        bool setInitialOption = false;
        int _selectedOption;
        internal int selectedOption {
            set {
                if (!setInitialOption) {
                    _selectedOption = Mathf.Clamp(initialOption, 0, options.Count);
                    setInitialOption = true;
                }
                _selectedOption = value % options.Count;

                if (_selectedOption < 0) {
                    _selectedOption = options.Count + _selectedOption;
                }
            }
            get {
                if (!setInitialOption) {
                    _selectedOption = Mathf.Clamp(initialOption, 0, options.Count);
                    setInitialOption = true;
                }
                return _selectedOption;
            }
        }
        internal string selectedOptionString {
            get {
                return options[selectedOption];
            }
        }
        internal string selectedOptionString_InVisual {
            get {
                if (options.Count > 2) {
                    return selectedOptionString + " ▼";
                } else {

                    return selectedOptionString;
                }
            }
        }
        
        internal ListVisual menu = new ListVisual();

        internal void onSetUp() {
            if (options.Count > 2) {
                menu.duplicate(GameMenu.instance.menuPrefab, 3);
                menu.SetUp(options, true);
            }
        }

        internal void onChange() {
            if (options.Count < 3) {
                selectedOption++;
                GameMenu.instance.optionVisual.UpdateAdditionalInfo(GameMenu.instance.currentOnScreenSelection, selectedOptionString);
            } else {
                GameMenu.instance.optionMenuOpened = true;
                GameMenu.instance.optionMenuSelected = GameMenu.instance.currentOnScreenSelection;
                GameMenu.instance.toggleOptionsMenu();
            }

        }
    }
    public List<option> allOptions;

    public enum menus { mainMenu, Tool, Weather, Options};
    menus currentMenu = menus.mainMenu;

    int _currentOnScreenSelection;
    public int currentOnScreenSelection {
        get {
            return _currentOnScreenSelection;
        }

        set {
            int currentValue = value;

            int MaxValue = 0;

            switch (currentMenu) {
                case menus.mainMenu:
                    MaxValue = mainMenuVisual.LengthOfOptions;
                    break;

                case menus.Tool:
                    MaxValue = allToolListVisuals[selectedToolList].LengthOfOptions;
                    break;

                case menus.Weather:
                    MaxValue = weatherVisual.LengthOfOptions;
                    break;

                case menus.Options:
                    MaxValue = optionVisual.LengthOfOptions;

                    if (optionMenuOpened) {
                        MaxValue = allOptions[optionMenuSelected].menu.LengthOfOptions;
                    }
                    break;

                default:
                    Debug.LogError("Option not considered for currentOnScreenSelection int for the following menu: " + currentMenu.ToString());
                    break;

            }

            currentValue = Mathf.Clamp(currentValue, 0, MaxValue - 1);

            _currentOnScreenSelection = currentValue;
        }
    }

    public Transform Cursor;
    public ListVisual menuPrefab;
    ListVisual mainMenuVisual = new ListVisual();
    List<UnityAction> mainMenuOptions = new List<UnityAction>();

    int selectedToolList = 0;
    List<ListVisual> allToolListVisuals = new List<ListVisual>();

    ListVisual weatherVisual = new ListVisual();

    internal ListVisual optionVisual = new ListVisual();
    internal int optionMenuSelected = 0;
    internal bool optionMenuOpened = false;

    ListVisual openMenuVisual {
        get {
            switch (currentMenu) {
                case menus.mainMenu:
                    return mainMenuVisual;

                case menus.Tool:
                    Debug.Log(allToolListVisuals.Count + "/" + selectedToolList);
                    return allToolListVisuals[selectedToolList];

                case menus.Options:
                    return optionVisual;

                case menus.Weather:
                    return weatherVisual;

                default:
                    return null;
            }
        }
    }

    Vector3 fullScale = Vector3.zero;
    Coroutine scalingAnimation;

    public GameObject visualLayer;
    List<GameObject> allVisualLayers = new List<GameObject>();

    internal int onScrollUp = 0;
    internal int onScrollDown = 0;

    public RectTransform scrollerIcon;
    float scrollerMaxHeight;
    float ScrollerInchingHeight;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }

        fullScale = transform.localScale;

        transform.localScale = Vector3.zero;
        scrollerMaxHeight = scrollerIcon.sizeDelta.y;

        StartCoroutine(InitialSetUp());
    }

    IEnumerator InitialSetUp() {
        while (ToolsManager.instance == null) {
            yield return null;
        }

        for (int i = 0; i < ToolsManager.instance.allToolFolders.Count; i++) {
            ToolsManager.ToolFolder selectedFolder = ToolsManager.instance.allToolFolders[i];

            if (selectedFolder.allTools.Count > 0) {
                ToolFolder addingFolder = new ToolFolder();
                addingFolder.folder = selectedFolder.folder;
                addingFolder.allTools = selectedFolder.allTools;
                allToolFolders.Add(addingFolder);
            }
        }

        menuPrefab.gameMenuDataSetUp();

        mainMenuVisual.duplicate(menuPrefab, 1);
        List<string> allMainMenuNames = new List<string>();
        for (int i = 0; i < allToolFolders.Count; i++) {
            allMainMenuNames.Add(allToolFolders[i].folder.name);
        }
        allMainMenuNames.Add("Weather");
        allMainMenuNames.Add("Options");
        allMainMenuNames.Add("Close");

        mainMenuVisual.SetUp(allMainMenuNames);

        for (int i = 0; i < allToolFolders.Count; i++) {
            ListVisual toolList = new ListVisual();
            toolList.duplicate(menuPrefab, 2);
            toolList.SetUp(allToolFolders[i].allToolNames, true);
            allToolListVisuals.Add(toolList);
        }

        for (int i = 0; i < visualLayer.transform.childCount; i++) {
            allVisualLayers.Add(visualLayer.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < mainMenuVisual.LengthOfOptions; i++) {
            int menuCount = i - allToolListVisuals.Count;
            if (menuCount < 0) {
                int actionCount = i;
                UnityAction action = delegate { onSelectTool(actionCount); };
                mainMenuOptions.Add(action);
            } else if (menuCount == 0) {
                mainMenuOptions.Add(onSelectWeather);
            } else if (menuCount == 1) {
                mainMenuOptions.Add(onSelectOptions);
            } else {
                mainMenuOptions.Add(onCloseMenu);
            }
        }

        optionVisual.duplicate(menuPrefab, 2);
        List<string> allOptionsNames = new List<string>();

        for (int i = 0; i < allOptions.Count; i++) {
            allOptionsNames.Add(allOptions[i].optionName);
        }
        optionVisual.SetUp(allOptionsNames, true);
        for (int i = 0; i < allOptions.Count; i++) {
            allOptions[i].onSetUp();
            optionVisual.UpdateAdditionalInfo(i, allOptions[i].selectedOptionString_InVisual);
        }

        Destroy(menuPrefab.prefab);
        completeSetUp = true;

        gameObject.SetActive(false);
    }

    void OnEnable() {
        if (!completeSetUp) {
            return;
        }
        if (fullScale == Vector3.zero && transform.localScale != Vector3.zero) {
            fullScale = transform.localScale;
        }
        currentOnScreenSelection = 0;
        toggleMenu(menus.mainMenu);

        if (scalingAnimation != null) {
            StopCoroutine(scalingAnimation);
        }
        scalingAnimation = StartCoroutine(openMenu());
    }

        // Update is called once per frame
    void Update()
    {

        if (!completeSetUp) {
            return;
        }

        int oldSelection = currentOnScreenSelection;
        for (int i = 0; i < onScrollUp; i++) {
            currentOnScreenSelection--;
        }

        for (int i = 0; i < onScrollDown; i++) {
            currentOnScreenSelection++;
        }

        if (currentOnScreenSelection != oldSelection) {
            PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
        }

        onScrollUp = 0;
        onScrollDown = 0;

        scrollerIcon.anchoredPosition = new Vector2(0f, ((float)currentOnScreenSelection) * ScrollerInchingHeight * -1f);

        switch (currentMenu) {
            case menus.mainMenu:
                menu_MainMenu();
                break;

            case menus.Tool:
                menu_Tool();
                break;

            case menus.Weather:
               // menu_xRayMenu();
                return;

            case menus.Options:
                menu_Options();
                break;

            default:
                Debug.LogError("Additional Menu not coded for: " + currentMenu.ToString());
                break;
        }
    }


    void toggleMenu(menus swappedMenu) {
        currentOnScreenSelection = 0;
        currentMenu = swappedMenu;

        mainMenuVisual.prefab.SetActive(currentMenu == menus.mainMenu);
        for (int i = 0; i < allToolListVisuals.Count; i++) {
            allToolListVisuals[i].prefab.SetActive(currentMenu == menus.Tool && selectedToolList == i);
        }
        //weatherVisual.prefab.SetActive(currentMenu == menus.Weather);
        optionVisual.prefab.SetActive(currentMenu == menus.Options);

        for (int i = 0; i < allOptions.Count; i++) {
            if (allOptions[i].menu.isSetUp) {
                allOptions[i].menu.prefab.SetActive(false);
            }
        }

        int currentLayer = openMenuVisual.MenuLayer - 1;

        for (int i = 0; i < allVisualLayers.Count; i++) {
            if (i < currentLayer || i == allVisualLayers.Count - 1) {
                allVisualLayers[i].SetActive(true);
            } else {
                allVisualLayers[i].SetActive(false);
            }
        }

        ScrollerInchingHeight = scrollerMaxHeight / ((float)openMenuVisual.LengthOfOptions);
        scrollerIcon.sizeDelta = new Vector2(scrollerIcon.sizeDelta.x, ScrollerInchingHeight);
    }

    #region Menu Animations
    IEnumerator openMenu() {
        float Timer = 0f;

        while (Timer < 1f) {
            Timer += Time.deltaTime * 5f;
            transform.localScale = Vector3.Lerp(Vector3.zero, fullScale, Timer);
            yield return null;
        }
        transform.localScale = fullScale;
    }

    IEnumerator closeMenu() {
        float Timer = 0f;

        while (Timer < 1f) {
            Timer += Time.deltaTime * 5f;
            transform.localScale = Vector3.Lerp(fullScale, Vector3.zero, Timer);
            yield return null;
        }
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    void onCloseMenu() {
        if (scalingAnimation != null) {
            StopCoroutine(scalingAnimation);
        }
        scalingAnimation = StartCoroutine(closeMenu());
    }
    #endregion

    #region Menus

    void menu_MainMenu() {
        Cursor.position = mainMenuVisual.getPosition(currentOnScreenSelection);

        if (PlayerScript.instance.Trigger.getButtonDown) {
            if (mainMenuOptions[currentOnScreenSelection] != null) {
                mainMenuOptions[currentOnScreenSelection].Invoke();
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
            } else {
                Debug.LogError("No Event has been made for Main Menu Option #" + currentOnScreenSelection);
            }
        }

        if (PlayerScript.instance.Home.getButtonDown) {
            onCloseMenu();
            PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
        }
        UI_ControllerPrompts.instance.onChangePrompts("Scroll", "Close", "", "Select");
    }

    void menu_Tool() {
        Cursor.position = allToolListVisuals[selectedToolList].getPosition(currentOnScreenSelection);

        if (PlayerScript.instance.Trigger.getButtonDown) {
            if (allToolFolders[selectedToolList].allTools.Count > currentOnScreenSelection) {
                ToolsManager.instance.SwapTool(allToolFolders[selectedToolList].allTools[currentOnScreenSelection]);
                onCloseMenu();
            } else {
                toggleMenu(menus.mainMenu);
            }
            PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
        }

        if (PlayerScript.instance.Home.getButtonDown) {
            toggleMenu(menus.mainMenu);
            PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
        }
        UI_ControllerPrompts.instance.onChangePrompts("Scroll", "Back", "", "Select");
    }

    #region Options
    void menu_Options() {
        if (optionMenuOpened) {
            Cursor.position = allOptions[optionMenuSelected].menu.getPosition(currentOnScreenSelection);

            if (PlayerScript.instance.Trigger.getButtonDown) {
                if (allOptions[optionMenuSelected].options.Count > currentOnScreenSelection) {
                    allOptions[optionMenuSelected].selectedOption = currentOnScreenSelection;
                    optionMenuOpened = false;
                    toggleOptionsMenu();
                } else {
                    optionMenuOpened = false;
                    toggleOptionsMenu();
                }
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
            }
            if (PlayerScript.instance.Home.getButtonDown) {
                optionMenuOpened = false;
                toggleOptionsMenu();
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
            }

        } else {
            Cursor.position = optionVisual.getPosition(currentOnScreenSelection);

            if (PlayerScript.instance.Trigger.getButtonDown) {
                if (allOptions.Count > currentOnScreenSelection) {
                    allOptions[currentOnScreenSelection].onChange();
                } else {
                    toggleMenu(menus.mainMenu);
                }
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Medium);
            }
            if (PlayerScript.instance.Home.getButtonDown) {
                toggleMenu(menus.mainMenu);
                PlayerScript.instance.StartControllerVibrate(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
            }
        }
        UI_ControllerPrompts.instance.onChangePrompts("Scroll", "Back", "", "Select");
    }

    internal void toggleOptionsMenu() {
        currentOnScreenSelection = 0;

        ListVisual currentOptionVisual = optionVisual;
        optionVisual.prefab.SetActive(!optionMenuOpened);
        for (int i = 0; i < allOptions.Count; i++) {
            if (allOptions[i].menu.isSetUp) {
                allOptions[i].menu.prefab.SetActive(optionMenuOpened && optionMenuSelected == i);
                if (optionMenuOpened && optionMenuSelected == i) {
                    currentOptionVisual = allOptions[i].menu;
                }
            }
            optionVisual.UpdateAdditionalInfo(i, allOptions[i].selectedOptionString_InVisual);
        }

        int currentLayer = currentOptionVisual.MenuLayer - 1;

        for (int i = 0; i < allVisualLayers.Count; i++) {
            if (i < currentLayer || i == allVisualLayers.Count - 1) {
                allVisualLayers[i].SetActive(true);
            } else {
                allVisualLayers[i].SetActive(false);
            }
        }

        ScrollerInchingHeight = scrollerMaxHeight / ((float)currentOptionVisual.LengthOfOptions);
        scrollerIcon.sizeDelta = new Vector2(scrollerIcon.sizeDelta.x, ScrollerInchingHeight);
    }

    internal string GetOptionStatus(string optionName) {
        for (int i = 0; i < allOptions.Count; i++) {
            if (allOptions[i].optionName == optionName) {
                return allOptions[i].selectedOptionString;
            }
        }

        Debug.Log("There's no option called " + optionName);
        return "";
    }

    internal float GetOptionStatusAsFloat(string optionName) {
        string result = "";
        for (int i = 0; i < allOptions.Count; i++) {
            if (allOptions[i].optionName == optionName) {
                result = allOptions[i].selectedOptionString;
            }
        }
        if (result == "") {
            Debug.Log("There's no option called " + optionName);
            return 0f;
        }

        if (result.Contains(" ")) {
            int space = result.IndexOf(" ");
            result = result.Substring(0, space);
        }

        return float.Parse(result);
    }
    #endregion

    #endregion

    #region Main Menu Options
    void onSelectTool(int selectedFolder) {
        selectedToolList = selectedFolder;
        toggleMenu(menus.Tool);

    }

    void onSelectWeather() {

    }

    void onSelectOptions() {
        toggleMenu(menus.Options);
    }
    #endregion
}

public class GameMenuData : MonoBehaviour {
    public GameObject TextObject;
}
