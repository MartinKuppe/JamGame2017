using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Particle)), CanEditMultipleObjects]
public class ParticleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Tool to register/unregister the particle --
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        var particles = targets;
        var settingsFiles = FindAssetsByType<ParticleRegistry>();

        var removable = new List<Particle>();
        var addable = new List<Particle>();

        foreach (var settings in settingsFiles)
        {
            removable.Clear();
            addable.Clear();

            for (int i = 0; i < particles.Length; i++)
            {
                var particle = (Particle)particles[i];
                (settings.Particles.Contains(particle) ? removable : addable).Add(particle);
            }

            if (removable.Count > 0 && GUILayout.Button("Remove " + removable.Count + " from " + settings.name))
            {
                foreach (var particle in removable)
                    settings.Particles.Remove(particle);

                EditorUtility.SetDirty(settings);
            }

            if (addable.Count > 0 && GUILayout.Button("Add " + addable.Count + " to " + settings.name))
            {
                foreach (var particle in addable)
                    settings.Particles.Add(particle);

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