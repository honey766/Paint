using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardScrollbar : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CharacterSwiper characterSwiper;
    [SerializeField] private Color color1, color2, color12;
    private Scrollbar scrollbar;

    private void Start()
    {
        scrollbar = GetComponent<Scrollbar>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // int random = Random.Range(0, 3);
        // ColorBlock cb = scrollbar.colors;

        // if (random == 0) cb.pressedColor = color1;
        // else if (random == 1) cb.pressedColor = color2;
        // else cb.pressedColor = color12;

        // scrollbar.colors = cb;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        characterSwiper.OnEndDrag();
    }
}
