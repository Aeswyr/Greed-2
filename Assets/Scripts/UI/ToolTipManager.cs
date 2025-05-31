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
