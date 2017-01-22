using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Sound", menuName = "Sound/Sound", order = 1)]
public class Sound : ScriptableObject {

    public string Name;
    public AudioClip AudioClip;
    public Layer Layer;

    [Header("Settings")]
    public bool Loop;

    [Range(0, 1)]
    public float Volume = 1;

    [Range(-3, 3)]
    public float Pitch = 1;
    
    [Range(-1, 1)] [Tooltip("Left (-1) < Center (0) < Right (1)")]
    public float StereoPan = 0;
}
