using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhileNode : ParentNode
{
    public bool Repeat;
    public string Condition { get { return Text; } set { Text = value; } }

    public override Node GetFirst()
    {
        return Repeat ? children.First() : base.GetFirst();
    }

    public override Vector2 GetWidthLimits()
    {
        var l = base.GetWidthLimits();
        return l;
    }
}
