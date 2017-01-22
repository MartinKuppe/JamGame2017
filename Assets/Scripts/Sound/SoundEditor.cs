using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Sound)), CanEditMultipleObjects]
public class SoundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        // Tool to register/unregister the sound --
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        
        var sounds = targets;
        var settingsFiles = FindAssetsByType<SoundSettings>();

        var removable = new List<Sound>();
        var addable = new List<Sound>();

        foreach (var settings in settingsFiles)
        {
            removable.Clear();
            addable.Clear();

            for (int i = 0; i < sounds.Length; i++)
            {
                var sound = (Sound)sounds[i];
                (settings.Sounds.Contains(sound) ? removable : addable).Add(sound);
            }

            if (removable.Count > 0 && GUILayout.Button("Remove "+ removable.Count + " from " + settings.name))
            {
                foreach (var sound in removable)
                    settings.Sounds.Remove(sound);
                
                EditorUtility.SetDirty(settings);
            }

            if (addable.Count > 0 && GUILayout.Button("Add " + addable.Count + " to " + settings.name))
            {
                foreach (var sound in addable)
                    settings.Sounds.Add(sound);
                EditorUtility.SetDirty(settings);
            }
        }

        
    }

    public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
}
#endif