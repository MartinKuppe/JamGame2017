using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SwissArmyKnife;

public class BroadcastButton : Singleton<BroadcastButton>, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public Button button;

    private Image backgroundImage;
    private float backgroundScale = 1;

    public GameObject hoverMarker;

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

    public void OnSelect()
    {
        OnPointerEnter(null);
        hoverMarker.SetActive(true);
    }

    public void OnDeselect()
    {
        OnPointerExit(null);
        hoverMarker.SetActive(false);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        backgroundImage = GetComponentInChildren<Image>();

        hoverMarker.SetActive(false);
    }
    
    private void Update()
    {
        bool blocked = PropagandaDescription.GetOngoingPropaganda();
        backgroundScale = pointerIsOnButton && !blocked ? backgroundScale + 0.02f : 1;
        if (backgroundScale > 2.5) backgroundScale = 1;
        
        backgroundImage.rectTransform.localScale = new Vector3(backgroundScale, backgroundScale);
    }
}
