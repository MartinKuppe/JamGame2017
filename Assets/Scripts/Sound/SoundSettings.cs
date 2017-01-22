using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundSettings", menuName = "Sound/SoundSettings", order = 2)]
public class SoundSettings : ScriptableObject {

    public VolumeNode Master;
    public List<LayerSettings> Layers;
    public List<Sound> Sounds;
}

[Serializable]
public class VolumeNode
{
    [Range(0, 1)]
    public float DefaultVolume = 1;

    [Range(0, 1)]
    public float VolumeCoef = 1;
}

[Serializable]
public class LayerSettings
{
    public VolumeNode Volume = new VolumeNode();
}

