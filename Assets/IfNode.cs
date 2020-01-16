using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class IfNode : Node
{
    public string Condition { get { return Text; } set { Text = value;} }
    public List<Node> TrueNodes;
    public List<Node> FalseNodes;
    public NodeLink ExitJoint;
    public override Vector2 GetBottom()
    {
        var lastT = TrueNodes.LastOrDefault();
        var lastF = FalseNodes.LastOrDefault();
        Node t;
        if (lastF == null)
        {
            t = lastT;
        }
        else if (lastT == null)
        {
            t = lastF;
        }
        else if(lastF.GetBottom().y < lastT.GetBottom().y)
        {
            t = lastF;
        }
        else
        {
            t = lastT;
        }
        return new Vector2(t.transform.position.x, t.GetBottom().y - 2 * UIController.Margin);
    }

    public override void Translate(Vector2 delta)
    {
        foreach (var node in TrueNodes)
        {
            node.Translate(delta);
        }
        foreach (var node in FalseNodes)
        {
            node.Translate(delta);
        }
        base.Translate(delta);
    }
    public override Vector2 GetWidthLimits()
    {
        List<Vector2> limits = TrueNodes.Select(x => x.GetWidthLimits()).ToList();
        limits.AddRange(FalseNodes.Select(x => x.GetWidthLimits()));
        limits.Add(base.GetWidthLimits());
        float max, min;
        max = TrueNodes.Any() ? limits.Max(l => l.y) : base.GetWidthLimits().y + UIController.MarginLineDistance * 2;
        min = FalseNodes.Any() ? limits.Min(l => l.x) : base.GetWidthLimits().x - UIController.MarginLineDistance * 2;
        return new Vector2(min, max);
    }

    public override INodeLink GetExitPoint(string name)
    {
        if (name == "Exit")
        {
            var list = new List<INodeLink>();

            if (TrueNodes.Any())
            {
                list.Add(TrueNodes.Last().GetExitPoint(name));
            }
            else
            {
                list.Add(GetExitPoint("True"));
            }

            if (FalseNodes.Any())
            {
                list.Add(FalseNodes.Last().GetExitPoint(name));
            }
            else
            {
                list.Add(GetExitPoint("False"));
            }
            new NodeLinkHub(list).SetTarget(this, null,null, "Exit");
        }

        return base.GetExitPoint(name);
    }

    public override void ColorizeNames(NameColor[] nameColors)
    {
        base.ColorizeNames(nameColors);
        foreach (var trueNode in TrueNodes)
        {
            trueNode.ColorizeNames(nameColors);
        }

        foreach (var falseNode in FalseNodes)
        {
            falseNode.ColorizeNames(nameColors);
        }
    }

    void FixedUpdate()
    {
        ExitJoint.transform.position = new Vector3(transform.position.x, GetBottom().y + 0.8f * UIController.Margin,0);
    }

    public override void ChangeFontSize(int size)
    {
        TrueNodes.ForEach(x=>x.ChangeFontSize(size));
        FalseNodes.ForEach(x=>x.ChangeFontSize(size));
        base.ChangeFontSize(size);
    }
}
