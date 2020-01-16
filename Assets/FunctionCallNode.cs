using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FunctionCallNode : CodeNode
{
    public FunctionNode Function;
    public override void OnPointerEnter(BaseEventData data)
    {
        base.OnPointerEnter(data);
        if (Function != null)
        {
            Function.Colorize(colorData.HoverChildColor, colorData.HoverChildColor, colorData.LineColor, colorData.HoverOutlineColor);
        }
    }
    public override void OnPointerExit(BaseEventData data)
    {
        if (Function != null)
        {
            Function.Colorize(colorData.NormalColor, colorData.NormalColor, colorData.NormalColor, null);
        }
        base.OnPointerExit(data);
    }
}
