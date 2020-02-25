using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool_Blaster : _Tool
{
    public BlasterComponents data;
    public bool showProjection = false;

    GameObject ActiveFolder;
    GameObject InactiveFolder;

    AudioSource fireSource;

    bool isReady = false;
    // Start is called before the first frame update
    void Start()
    {
        if (data.bulletShape == null) {
            Debug.LogError("Error: the Blaster named " + data.name + " doesn't have a shape");
            Destroy(gameObject);
            return;
        }

        if (data.bulletShape.GetComponent<Bullet>() == null) {
            Debug.LogError("Error: " + data.bulletShape.name + " doesn't have a Bullet script");
            Destroy(gameObject);
            return;
        }
        gameObject.name = data.BlasterName;
        Tool_Start(gameObject);
        StartCoroutine(setUp());
    }

    IEnumerator setUp() {

        while (ToolsManager.instance == null) {
            yield return null;
        }
        isReady = true;
        useGravity = data.usesGravity;

        if (data.fireSound != null) {
            fireSource = gameObject.AddComponent<AudioSource>();
            fireSource.playOnAwake = false;
            fireSource.clip = data.fireSound;
        }
        projectionSpeed = data.bulletSpeed;

        GameObject folder = new GameObject();

        folder.name = data.BlasterName;
        folder.transform.parent = ToolsManager.instance.BulletFolder.transform;

        ActiveFolder = new GameObject();
        ActiveFolder.name = "Active";
        ActiveFolder.transform.parent = folder.transform;

        InactiveFolder = new GameObject();
        InactiveFolder.name = "Inactive";
        InactiveFolder.transform.parent = folder.transform;

        for (int i = 0; i < 100; i++) {
            GameObject newBullet = Instantiate(data.bulletShape) as GameObject;
            newBullet.transform.parent = InactiveFolder.transform;
            newBullet.SetActive(false);
            newBullet.GetComponent<Bullet>().data = data;
        }

        InvokeRepeating("clearInactiveBullets", 1f, 1f);
    }

    private void OnEnable() {
        if (!isReady) {
            StartCoroutine(setUp());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!canUse || !isReady) {
            displayProjection(false);
            return;
        }
        Tool_Update();
        displayProjection(showProjection);
        if (PlayerScript.instance.Trigger.getButtonDown) {
            if (data.fireSound != null) {
                fireSource.Play();
            }

            PlayerScript.instance.StartControllerVibrate(data.kickback.Pattern, data.kickback.Intensity);

            if (InactiveFolder.transform.childCount <= 0) {
                GameObject newBullet = Instantiate(data.bulletShape) as GameObject;
                newBullet.transform.parent = InactiveFolder.transform;
                newBullet.SetActive(false);
                newBullet.GetComponent<Bullet>().data = data;
            }
            GameObject selected = InactiveFolder.transform.GetChild(0).gameObject;
            selected.transform.parent = ActiveFolder.transform;
            selected.SetActive(true);
            selected.GetComponent<Bullet>().onRelease(transform.position, transform.rotation);
        }

        UI_ControllerPrompts.instance.onChangePrompts("", "", "", "Fire");
    }

    void clearInactiveBullets() {
        for (int i = ActiveFolder.transform.childCount - 1; i > -1; i--) {
            GameObject selected = ActiveFolder.transform.GetChild(i).gameObject;
            if (!selected.activeInHierarchy) {
                selected.transform.parent = InactiveFolder.transform;
            }
        }
    }

}
