using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BroadcastButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image backgroundImage;
    private float backgroundScale = 1;

    [SerializeField]
    private bool pointerIsOnButton;

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerIsOnButton = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerIsOnButton = false;
    }

    private void Awake()
    {
        backgroundImage = GetComponentInChildren<Image>();
    }
    
    private void Update()
    {
        bool blocked = PropagandaDescription.GetOngoingPropaganda();
        backgroundScale = pointerIsOnButton && !blocked ? backgroundScale + 0.02f : 1;
        if (backgroundScale > 2.5) backgroundScale = 1;
        
        backgroundImage.rectTransform.localScale = new Vector3(backgroundScale, backgroundScale);
    }
}
