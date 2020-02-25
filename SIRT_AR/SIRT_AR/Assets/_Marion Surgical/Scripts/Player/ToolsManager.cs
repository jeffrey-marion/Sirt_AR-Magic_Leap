using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToolsManager : MonoBehaviour
{
    //The tools manager is designed to allow the user to switch between tools and for the menus to easily access each tool
    // It also keeps track of variables that multiple tools may need access to (such as a folder of prefabs)

    /// <summary>
    /// How to add New Tools and Folders:
    /// 
    /// Folders:
    /// Add an Empty GameObject in the following GameObject:
    ///     Player > Tools
    /// The name of the Empty GameObject will be the name of the Folder
    /// 
    /// Tools:
    /// Make a GameObject with a _Tool class script attached to it. Attach that GameObject to a Folder.
    /// </summary>
    public static ToolsManager instance;
    internal class ToolFolder {
        internal GameObject folder;
        internal List<_Tool> allTools = new List<_Tool>();

        internal void setAllTools(GameObject _folder) {
            folder = _folder;
            allTools = folder.GetComponentsInChildren<_Tool>().ToList<_Tool>();
        }
    }

    internal List<ToolFolder> allToolFolders = new List<ToolFolder>();
    _Tool activeTool;
    List<_Tool> allTools = new List<_Tool>();
    internal GameObject BulletFolder;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
        for (int i = 0; i < transform.childCount; i++) {
            ToolFolder newFolder = new ToolFolder();
            newFolder.setAllTools(transform.GetChild(i).gameObject);
            allToolFolders.Add(newFolder);

            for (int ii = 0; ii < newFolder.allTools.Count; ii++) {
                allTools.Add(newFolder.allTools[ii]);
            }
        }
        activeTool = allTools[0];
        SwapTool(activeTool);
        StartCoroutine(initialDeactivate());

        BulletFolder = new GameObject();
        BulletFolder.name = "Bullet Folder";
        BulletFolder.transform.parent = transform;
        BulletFolder.transform.position = Vector3.zero;
        BulletFolder.transform.rotation = Quaternion.identity;
        BulletFolder.transform.localScale = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerScript.instance.setUpComplete) {
            return;
        }
    }

    IEnumerator initialDeactivate() {
        for (int i = 0; i < allToolFolders.Count; i++) {
            allToolFolders[i].folder.SetActive(false);
        }

        yield return new WaitForEndOfFrame();
        while (!PlayerScript.instance.setUpComplete) {
            yield return null;
        }

        for (int i = 0; i < allToolFolders.Count; i++) {
            allToolFolders[i].folder.SetActive(true);
        }
    }

    void nextTool() {
        int currentTool = 0;

        for (int i = 0; i < allTools.Count; i++) {
            if (activeTool == allTools[i]) {
                currentTool = i;
                i = allTools.Count;
            }
        }

        int nextTool = (currentTool + 1) % allTools.Count;

        SwapTool(allTools[nextTool]);
    }

    public void SwapTool(_Tool selectedTool) {
        for (int i = 0; i < allTools.Count; i++) {
            allTools[i].isActive = allTools[i] == selectedTool;
            allTools[i].gameObject.SetActive(allTools[i] == selectedTool);
        }
        activeTool = selectedTool;
    }
}
