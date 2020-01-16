using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class Node : MonoBehaviour, ILimiter
{
    public int Index;
    public Vector2 Offset = new Vector2(0, 50);
    public Vector2 ExitPoint = new Vector2(0, -50);
    [SerializeField]
    private Text TextRenderer;
    protected Vector2? WidthLimit;
    [SerializeField]
    private List<Transform> EnterPoints;
    [SerializeField]
    private List<NodeLink> ExitPoints;
    [SerializeField]
    private List<Image> MainObjects;
    [SerializeField]
    protected ColorData colorData;
    protected string OriginalText { get; set; }


    public string Text
    {
        get
        {
            return OriginalText;
        }
        set
        {
            OriginalText = value;
            VisibleText = value;
        }
    }

    public string VisibleText
    {
        set
        {
            Regex regex = new Regex(@"@%\d+%@");
            TextRenderer.text = regex.Replace(value,x=>
            {
                var liral = FlowBuilder.Literals[int.Parse(x.Value.Cut(2, x.Value.Length - 3))];
                if (liral.Length > UIController.CharLimit)
                {
                    return liral.Substring(0, UIController.CharLimit).Trim() + "...";
                }
                else
                {
                    return liral;
                }
            });
        }
    }
    public virtual void MoveTo(Vector2 point)
    {
        transform.position = point;
    }

    public virtual Vector2 GetBottom()
    {
        var l = GetExitPoint("Exit").GetLowest(); //(Vector2)transform.position + ExitPoint;
        return l;
    }
    public virtual void Translate(Vector2 delta)
    {
        transform.position = (Vector2)transform.position + delta;
    }
    public Vector2 GetBound(Vector2 from, Vector2 to)
    {
        return GetBound(to - from);
    }
    public virtual Vector2 GetBound(Vector2 vector)
    {
        return new Vector2(Mathf.Sign(vector.x) * GetComponent<RectTransform>().sizeDelta.x/2, Mathf.Sign(vector.y) * GetComponent<RectTransform>().sizeDelta.y /2) + new Vector2(-UIController.Margin*UIController.LineDistance,UIController.Margin*UIController.LineDistance);
    }
    public virtual Vector2 GetWidthLimits()
    {
        //if (!WidthLimit.HasValue)
        //{
        //    WidthLimit = new Vector2(-GetComponent<RectTransform>().sizeDelta.x / 2, GetComponent<RectTransform>().sizeDelta.x / 2);
        //}
        //return WidthLimit.Value + transform.position.x * Vector2.one;
        return new Vector2(-GetComponent<RectTransform>().sizeDelta.x / 2, GetComponent<RectTransform>().sizeDelta.x / 2) + transform.position.x * Vector2.one;
    }


    public virtual Node GetFirst()
    {
        return this;
    }
    public virtual INodeLink GetExitPoint(string name)
    {
        return ExitPoints.Single(x => x.gameObject.name == name);
    }

    public Transform GetEnterPoint(string name)
    {
        return EnterPoints.Single(x => x.gameObject.name == name);
    }
    public virtual void OnPointerEnter(BaseEventData data)
    {
        Colorize(colorData.HoverColor, colorData.HoverChildColor,colorData.LineColor, colorData.HoverOutlineColor);
    }
    public virtual void OnPointerExit(BaseEventData data)
    {
        Colorize(colorData.NormalColor, colorData.NormalColor, colorData.NormalColor, null);
    }

    public virtual void Colorize(Color mainColor, Color childColor, Color lineColor, Color? outlineColor)
    {
        foreach (var obj in MainObjects)
        {
            obj.color = mainColor;
        }

        var images = transform.GetComponentsInChildren<Image>(true).ToList();
        var im = GetComponent<Image>();
        if (im != null)
        {
            images.Add(im);
        }
        foreach (var img in images.Where(x =>MainObjects.All(y=>y != x)))
        {
            img.color = childColor;
        }
        
        var outline = transform.GetComponentsInChildren<Outline>(true).ToList();
        outline.Add(GetComponent<Outline>());
        foreach (var o in outline.Where(x => x != null))
        {
            if (outlineColor.HasValue)
            {
                o.effectColor = outlineColor.Value;
                o.enabled = true;
            }
            else
            {
                o.enabled = false;
            }
        }
        var lines = transform.GetComponentsInChildren<LineRenderer>(true).ToList();
        foreach (var line in lines)
        {
            line.startColor = lineColor;
            line.endColor = lineColor;
        }

    }

    public virtual void ColorizeNames(NameColor[] nameColors)
    {
        if (TextRenderer == null) return;

        var str = OriginalText;
        foreach (var nameColor in nameColors)
        {
            Regex regex = new Regex(@"([^a-zA-Z0-9]" + nameColor.Name + @"[^a-zA-Z0-9])|(^" + nameColor.Name + @"[^a-zA-Z0-9])|([^a-zA-Z0-9]" + nameColor.Name + @"$)|(^" + nameColor.Name + @"$)", RegexOptions.IgnoreCase);
            str = regex.Replace(str, m=>
            {
                return (m.Value.StartsWith(nameColor.Name) ? "" : m.Value[0].ToString()) + "<color=" + nameColor.Color.ToHex() + ">" + nameColor.Name + "</color>" + (m.Value.EndsWith(nameColor.Name) ? "" : m.Value[m.Length-1].ToString());
            });
        }

        VisibleText = str;
    }


    public virtual void ChangeFontSize(int size)
    {
        if (TextRenderer == null) return;

        TextRenderer.resizeTextForBestFit = false;
        TextRenderer.fontSize = size;
    }
}

public interface ILimiter
{
    Transform transform { get; }
    Vector2 GetWidthLimits();
    Vector2 GetBottom();
}
