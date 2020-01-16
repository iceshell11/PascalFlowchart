using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParentNode : Node {

    public List<Node> children = new List<Node>();

    public override Vector2 GetBottom()
    {
        return children.Last().GetBottom() + ExitPoint;
    }
    public override void Translate(Vector2 delta)
    {
        foreach (var node in children)
        {
            node.Translate(delta);
        }
        base.Translate(delta);
    }
    public override Vector2 GetWidthLimits()
    {
        List<Vector2> limits = children.Select(x => x.GetWidthLimits()).ToList();
        limits.Add(base.GetWidthLimits());
        return new Vector2(limits.Min(l => l.x) - FlowBuilder.g_Margin * FlowBuilder.g_LineDistance, limits.Max(l => l.y) + FlowBuilder.g_Margin * FlowBuilder.g_LineDistance);
    }

    private void OnDestroy()
    {
        foreach (var child in children)
        {
            Destroy(child.gameObject);
        }
    }

    public override void ColorizeNames(NameColor[] nameColors)
    {
        base.ColorizeNames(nameColors);
        foreach (var child in children)
        {
            child.ColorizeNames(nameColors);
        }
    }

    public override void ChangeFontSize(int size)
    {
        base.ChangeFontSize(size);
        foreach (var child in children)
        {
            child.ChangeFontSize(size);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        var l = GetWidthLimits();
        Gizmos.DrawLine(new Vector2(l.x,transform.position.y),new Vector3(l.y,transform.position.y));
    }

}
