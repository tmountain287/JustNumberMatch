using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomScrollRect : ScrollRect
{
    public bool blockDrag = false;

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (!blockDrag)
            base.OnInitializePotentialDrag(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (!blockDrag)
            base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!blockDrag)
            base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (!blockDrag)
            base.OnEndDrag(eventData);
    }
}
