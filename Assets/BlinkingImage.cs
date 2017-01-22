using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlinkingImage : MonoBehaviour {

    private Image _img;
    public Color[] colors = new Color[2];

    private void Awake()
    {
        _img = GetComponent<Image>();
    }

    private void OnEnable()
    {
        StartCoroutine(BlinkingRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator BlinkingRoutine()
    {
        float blinkingTime = 0.8f;

        int index = 0;
        _img.color = colors[index];

        while (true)
        {
            index = (index + 1) % colors.Length;
            _img.color = colors[index];

            yield return new WaitForSeconds(blinkingTime);
        }
    }
}
