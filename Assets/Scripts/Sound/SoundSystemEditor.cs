using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SoundSystem))]
public class SoundSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        // Retrieve settings --
        var system = (SoundSystem)target;
        var settings = system.Settings;

        if (settings == null)
            return;

        // Sanitise settings --
        var layersValue = Enum.GetValues(typeof(Layer));
        while (settings.Layers.Count < layersValue.Length)
        {
            settings.Layers.Add(new LayerSettings());
        }

        // Display volumes --
        DisplayVolumeNode(settings.Master, system.GetMasterVolumeNode(), "Master");
        int i = 0;
        foreach (Layer layer in layersValue)
        {
            DisplayVolumeNode(settings.Layers[i].Volume, system.GetLayerVolumeNode(layer), layer.ToString());
            i++;
        }

        EditorUtility.SetDirty(target);
    }

    private void DisplayVolumeNode(VolumeNode node, VolumeNodeInstance nodeInst, string title)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        if(nodeInst != null)
        {
            nodeInst.IsMute = EditorGUILayout.Toggle("Mute", nodeInst.IsMute);
            nodeInst.UserVolume = EditorGUILayout.Slider("User Volume", nodeInst.UserVolume, 0, 1);
        }

        node.DefaultVolume = EditorGUILayout.Slider("Default Volume", node.DefaultVolume, 0, 1);
        node.VolumeCoef = EditorGUILayout.Slider("Volume Coef", node.VolumeCoef, 0, 1);
    }
}
#endif