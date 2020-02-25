using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

[CreateAssetMenu(fileName = "BlasterData", menuName = "ScriptableObjects/CreateBlasterData", order = 1)]
public class BlasterComponents : ScriptableObject
{
    //This Scriptable object allows us to quickly make a blaster and bullet with easily adjustable values
    [Header("Blaster")]
    public string BlasterName;
    public bool holdToFire = false;
    public float roundsPerSecond = 1f;
    internal float secondsPerRound {
        get {
            return 1f / roundsPerSecond;
        }
    }
    public AudioClip fireSound;

    [System.Serializable]
    public class Kickback {
        public MLInputControllerFeedbackPatternVibe Pattern;
        public MLInputControllerFeedbackIntensity Intensity;
    }
    public Kickback kickback;

    [Space]
    [Space]
    [Space]
    [Header("Bullet")]
    public GameObject bulletShape;
    public float bulletSpeed = 335f;
    public bool usesGravity = false;

    public enum deathType { collision, time};
    public deathType bulletDeath;
    public int collisionsUntilDeath;
    public float TimeUntilDeath;
    public GameObject collisionEffect;
    public AudioClip collisionSound;

}

/*
Blaster speed from Star Wars = 34.9
Slowest bullet speed = 335
Nerf gun = 10.4
*/
