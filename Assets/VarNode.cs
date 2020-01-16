using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VarNode : CodeNode
{
    public List<Variable> Vars = new List<Variable>();

    public void AddVar(string var)
    {
        string type = var.Cut(var.IndexOf(':') + 1, var.Length - 1).Trim(' ',':');
        var = var.Cut(0, var.IndexOf(':') - 1);
        List<string> names = var.Split(',').Select(x => x.Trim()).ToList();

        Color rColor;
        foreach (var n in names)
        {
            if (FlowBuilder.g_VarColors.Count > 0)
            {
                rColor = FlowBuilder.g_VarColors.GetRandomItem();
                FlowBuilder.g_VarColors.Remove(rColor);
            }
            else
            {
                rColor = Color.green;
            }
            Vars.Add(new Variable(n, type, rColor));
        }

        Text = string.Join("\r\n", Vars.GroupBy(x=>x.Type).Select(x=>string.Join(", ", x.Select(y=> y.Name).ToArray()) + ": " + x.Key).ToArray());
    }

    public void AddVars(List<Variable> vars)
    {
        Vars.AddRange(vars);
        Text = string.Join("\r\n", Vars.GroupBy(x => x.Type).Select(x => string.Join(", ", x.Select(y => y.Name).ToArray()) + ": " + x.Key).ToArray());
    }

    public NameColor[] GetColors()
    {
        return Vars.Select(x => new NameColor(x.Name, x.Color)).ToArray();
    }
    [System.Serializable]
    public struct Variable
    {
        public string Name;
        public string Type;
        public Color Color;
        public Variable(string name, string type, Color? color = null)
        {
            Name = name;
            Type = type;
            Color = color ?? Color.black;
        }
    }
}
