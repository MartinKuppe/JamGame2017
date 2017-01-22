using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor.Events;
using UnityEditor;

[CustomEditor(typeof(SFXButton)), CanEditMultipleObjects]
class SFXButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Binding", EditorStyles.boldLabel);

        var sound = (Sound)EditorGUILayout.ObjectField("SetSound", null, typeof(Sound), false);
        if (sound != null)
        {
            var sfxButton = (SFXButton)target;
            sfxButton.SoundName = sound.Name;
        }

        if (GUILayout.Button("Autobind"))
        {
            var sfxButton = (SFXButton)target;
            var button = sfxButton.GetComponent<Button>();

            UnityEventTools.AddPersistentListener(button.onClick, new UnityAction(sfxButton.OnClick));
        }
    }
}
#endif

public class SFXButton : MonoBehaviour {

    public string SoundName;

    public void OnClick()
    {
        if (SoundName != null)
            SoundSystem.Play(SoundName);
    }
}
