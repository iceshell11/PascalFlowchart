using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FunctionNode : ParentNode
{
    public string Name;
    public string Unit;
    [SerializeField]
    private List<GameObject> HoverObjects;
    public List<string> Args = new List<string>();
    public List<string> Vars = new List<string>();
    public override void Translate(Vector2 delta)
    {
        transform.Translate(delta);
    }

    public void UpdateUnitName(string name)
    {
        Unit = name;
        Text = "Начало <" + Name + "> модуля " + Unit;
    }

    public override void OnPointerEnter(BaseEventData data)
    {
        var p = (data as PointerEventData).pointerEnter;
        if (HoverObjects.Any(x=>x == p))
        {
            base.OnPointerEnter(data);
        }
    }

    public override void OnPointerExit(BaseEventData data)
    {
        var p = (data as PointerEventData).pointerEnter;
        if (HoverObjects.Any(x => x == p))
        {
            base.OnPointerExit(data);
        }
    }
}
