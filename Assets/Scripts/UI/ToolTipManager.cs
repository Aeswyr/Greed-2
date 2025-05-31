using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipManager : Singleton<ToolTipManager>
{
    [SerializeField] private ItemDescriptionLibrary itemDescriptions;
    [SerializeField] private GameObject toolTipPrefab;
    private List<ToolTip> toolTips = new();
    public ToolTip CreateTooltip(PickupType type, Vector3 position)
    {
        var toolTip = Instantiate(toolTipPrefab, position, Quaternion.identity, transform).GetComponent<ToolTip>();

        toolTip.SetTitleText(itemDescriptions.GetName(type));
        toolTip.SetBodyText(itemDescriptions.GetDesc(type));

        /*
        RectTransform rect = (RectTransform)toolTip.transform;
        Vector3[] corners = new Vector3[4];
        rect.GetLocalCorners(corners);
        float width = ((RectTransform)transform).rect.width;
        foreach (var corner in corners)
        {
            if (corner.x < 200)
            {
                toolTip.transform.localPosition += -corner.x * Vector3.right;
            }
            else if (corner.x > width)
            {
                toolTip.transform.localPosition += (width - corner.x) * Vector3.right;
            }
        }
        toolTip.UpdatePointerPosition(position.x);
        */

        toolTips.Add(toolTip);

        return toolTip;
    }

    public void ClearTooltips()
    {
        foreach (var toolTip in toolTips)
        {
            Destroy(toolTip.gameObject);
        }
        toolTips.Clear();
    }

    public void ClearTooltip(ToolTip tooltip)
    {
        if (tooltip == null || !toolTips.Contains(tooltip))
        {
            return;
        }
        toolTips.Remove(tooltip);
        Destroy(tooltip.gameObject);
    }
}
