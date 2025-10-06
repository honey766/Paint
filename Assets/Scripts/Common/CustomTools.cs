using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomTools
{
    private const int UILayer = 5;

    public static Quaternion GetRotationByDirection(Vector2Int direction)
    {
        switch (direction)
        {
            case Vector2Int v when v == Vector2Int.up:
                return Quaternion.identity;
            case Vector2Int v when v == Vector2Int.right:
                return Quaternion.Euler(new Vector3(0, 0, -90));
            case Vector2Int v when v == Vector2Int.down:
                return Quaternion.Euler(new Vector3(0, 0, 180));
            case Vector2Int v when v == Vector2Int.left:
                return Quaternion.Euler(new Vector3(0, 0, 90));
        }
        return Quaternion.identity;
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    private static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults;
    }
}