using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyWatcher : MonoBehaviour {

    public delegate void KeyEvent();
    public static event KeyEvent OnKeyAcknowledged;
    public static event KeyEvent OnKeyUp;

    private float _waitTime = 0.01f;
    private KeyCode _lastKey = KeyCode.AltGr;

    private void Update()
    {
        KeyCode[] watchedKeys = { KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.DownArrow };

        foreach(KeyCode k in watchedKeys)
        {
            if(Input.GetKeyDown(k))
            {
                StopAllCoroutines();
                StartCoroutine(PrepareToLaunch());
                _lastKey = k;
            }
            else if (Input.GetKeyUp(k))
            {
                _lastKey = KeyCode.AltGr;
                if(OnKeyUp != null)
                {
                    OnKeyUp();
                }
            }
        }
    }

    private IEnumerator PrepareToLaunch()
    {
        yield return new WaitForSeconds(_waitTime);

        if(OnKeyAcknowledged != null)
        {
            OnKeyAcknowledged();
        }
    }

    public static void ForceAck()
    {
        if (OnKeyAcknowledged != null)
        {
            OnKeyAcknowledged();
        }
    }
}
