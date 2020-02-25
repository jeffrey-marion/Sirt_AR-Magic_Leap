using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    internal BlasterComponents data;

    bool setUp = false;
    LineRenderer lineRenderer;
    public LayerMask environment;
    AudioSource collisionSource;

    GameObject activeCollision;
    GameObject inactiveCollision;

    public void onRelease(Vector3 pos, Quaternion direction) {
        if (!setUp) {
            settingUp();
        }
        transform.position = pos;
        transform.rotation = direction;
        StartCoroutine(soaring());
    }

    void settingUp() {
        setUp = true;

        if (GetComponent<LineRenderer>() != null) {
            lineRenderer = GetComponent<LineRenderer>();
        }
        

        if (data.collisionSound != null) {
            collisionSource = gameObject.AddComponent<AudioSource>();
            collisionSource.playOnAwake = false;
            collisionSource.clip = data.collisionSound;
        }

        if (data.collisionEffect != null) {
            activeCollision = new GameObject();
            activeCollision.name = "Active Collision Effect";
            activeCollision.transform.parent = transform.parent.parent;

            inactiveCollision = new GameObject();
            inactiveCollision.name = "Inactive Collision Effect";
            inactiveCollision.transform.parent = transform.parent.parent;

            for (int i = 0; i < 100; i++) {
                GameObject newCol = Instantiate(data.collisionEffect) as GameObject;
                newCol.transform.parent = inactiveCollision.transform;
                newCol.SetActive(false);
            }
        }
    }

    IEnumerator soaring() {
        int collisionsLeft = data.collisionsUntilDeath;
        float timeToEnd = Time.time + data.TimeUntilDeath;

        Vector3 momentum = transform.forward * data.bulletSpeed;

        bool end = false;

        while (end == false) {

            List<Vector3> allPoints = new List<Vector3>();
            float currentSpeed = momentum.magnitude;

            for (float distanceLeft = currentSpeed * Time.deltaTime; distanceLeft > 0;) {
                allPoints.Add(transform.position);
                float distanceToRemove = distanceLeft;

                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.Normalize(momentum), out hit, distanceLeft, environment)) {
                    Vector3 point = hit.point;
                    Vector3 newDirection = Vector3.Reflect(Vector3.Normalize(momentum), hit.normal);
                    distanceToRemove = Vector3.Distance(transform.position, point);
                    momentum = newDirection * currentSpeed;
                    transform.position = point;
                    if (data.bulletDeath == BlasterComponents.deathType.collision) {
                        collisionsLeft--;
                    }

                    if (inactiveCollision != null) {
                        if (inactiveCollision.transform.childCount <= 0) {
                            GameObject newCol = Instantiate(data.collisionEffect) as GameObject;
                            newCol.transform.parent = inactiveCollision.transform;
                            newCol.SetActive(false);
                        }

                        GameObject selected = inactiveCollision.transform.GetChild(0).gameObject;
                        selected.transform.parent = activeCollision.transform;
                        selected.SetActive(true);
                        selected.transform.position = point;
                        selected.transform.rotation = Quaternion.FromToRotation(selected.transform.forward, hit.normal);
                        ToolsManager.instance.StartCoroutine(swapWhenInactive(selected));
                    }

                    if (collisionSource != null) {
                        collisionSource.PlayOneShot(collisionSource.clip);
                    }
                } else {
                    transform.position += Vector3.Normalize(momentum) * distanceLeft;
                }

                if (data.bulletDeath == BlasterComponents.deathType.collision && collisionsLeft <= 0) {
                    distanceLeft = 0;
                } else {
                    distanceLeft -= distanceToRemove;
                }
            }

            if (data.usesGravity) {
                momentum += Physics.gravity * Time.deltaTime;
            }

            allPoints.Add(transform.position);

            if (lineRenderer != null) {
                lineRenderer.positionCount = 0;

                for (int i = 0; i < allPoints.Count; i++) {
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(i, allPoints[i]);
                }
            }

            if (data.bulletDeath == BlasterComponents.deathType.collision) {
                end = collisionsLeft <= 0;
            }

            if (data.bulletDeath == BlasterComponents.deathType.time) {
                end = timeToEnd < Time.time;
            }

            if (Vector3.Distance(transform.position, Camera.main.transform.position) > 100f) {
                end = true;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }

    IEnumerator swapWhenInactive(GameObject selected) {
        while (selected.activeInHierarchy) {
            yield return null;
        }
        selected.transform.parent = inactiveCollision.transform;
    }
}
