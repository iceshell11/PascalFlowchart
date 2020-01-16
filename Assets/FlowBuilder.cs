using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum KeyWords
{
    program, begin, end, var, type, function, procedure, except, uses, @for, until, @while, @if, repeat, @const, @initialization, @finalization
}
public class FlowBuilder : MonoBehaviour
{
    public int MaxNodeLinesCount { get { return 5-3; } }
    public static List<Color> g_VarColors { get; set; }
    public static float g_Margin { get; set; }
    public static float g_LineDistance { get; set; }
    public static bool g_Colorize { get; set; }

    public Prefabs Prefabs;
    public RectTransform Root;
    public List<Node> Nodes;
    public Vector2 TopPoint;
    public Dictionary<string, FunctionNode> Functions = new Dictionary<string, FunctionNode>();
    public VarNode Vars;
    [Multiline]
    public string Code;
    public static List<string> Literals = new List<string>();
    public float IfWidth { get { return 100; } }
    public void DestroyNodes()
    {
        foreach (var o in GameObject.FindGameObjectsWithTag("Block"))
        {
            Destroy(o);
        }
        //foreach (var n in Nodes)
        //{
        //    n.SafeDestroyObject();
        //}
        Nodes.Clear();

        //foreach (var f in Functions)
        //{
        //    f.Value.SafeDestroyObject();
        //}
        Functions.Clear();
        //Vars.SafeDestroyObject();
        Vars = null;
    }

    public List<Node> GenerateUnit(List<string> lines, ref Vector2 point,ref Vector2 startPoint, string unitName = null)
    {
        var generatedNodes = new List<Node>();
        int index = -1;
        while (index < lines.Count - 1)
        {
            var nodes = GenerateNode(lines, point, ref index, Root, false);
            if (nodes == null)
            {
                break;
            }
            if (nodes.All(x => x is FunctionNode))
            {
                var func = nodes.Where(x => x is FunctionNode).Cast<FunctionNode>();
                if (unitName != null)
                {
                    foreach (var functionNode in func)
                    {
                        functionNode.UpdateUnitName(unitName);
                    }
                }

                Functions.AddRange(func.ToDictionary(x => x.Name, x => x));
            }
            else if (nodes.SingleOrDefault(x => x is VarNode) != null)
            {
                if (Vars == null)
                {
                    Vars = nodes.First() as VarNode;
                    Vars.transform.position = startPoint;
                    startPoint.y += 300;
                }
                else
                {
                    Vars.AddVars((nodes.First() as VarNode).Vars);
                }
            }
            else if (nodes.SingleOrDefault(x => x is ConstNode) != null)
            {
                nodes.SingleOrDefault().transform.position = startPoint;
                startPoint.y += 300;
            }
            else if(nodes.Any())
            {
                generatedNodes.AddRange(nodes);
                point = new Vector2(point.x, nodes.Last().GetBottom().y) + new Vector2(0, -g_Margin);
            }
        }
        return generatedNodes;
    }
    public void Generate(string code)
    {
        DestroyNodes();
#if !DEBUG
        try
#endif
        {    
            Code = code;
            if (Code != null)
            {
                int index = -1;
                Vector2 point = (Vector2)Root.position + new Vector2(Root.sizeDelta.x * 0.5f, Root.sizeDelta.y * -0.2f);

                var startNode = Root.AppendNew(Prefabs.BeginEndNode, point);
                Vector2 headPoint = (Vector2)startNode.transform.position + new Vector2(-200, 200);
                point = startNode.GetBottom() + new Vector2(0, -g_Margin*1.1f);
                Nodes.Add(startNode);
                startNode.Text = "Начало";

                Nodes.AddRange(GenerateUnit(ParseCode(Code), ref point, ref headPoint));
                //while (index < lines.Count - 1)
                //{
                //    var nodes = GenerateNode(lines, point, ref index, Root, false);
                //    if (nodes == null)
                //    {
                //        break;
                //    }
                //    if (nodes.All(x => x is FunctionNode))
                //    {
                //        Functions.AddRange(nodes.Where(x => x is FunctionNode).Cast<FunctionNode>().ToDictionary(x => x.Name, x => x));
                //    }
                //    else if (nodes.SingleOrDefault(x => x is VarNode) != null)
                //    {
                //        if (Vars == null)
                //        {
                //            Vars = nodes.First() as VarNode;
                //            Vars.transform.position = startNode.transform.position + new Vector3(-200, 200);
                //        }
                //        else
                //        {
                //            Vars.AddVars((nodes.First() as VarNode).Vars);
                //        }
                //    }
                //    else if (nodes.SingleOrDefault(x => x is ConstNode) != null)
                //    {
                //        nodes.SingleOrDefault().transform.position = startNode.transform.position + new Vector3(-200, 500);
                //    }
                //    else
                //    {
                //        Nodes.AddRange(nodes);
                //    }
                //}

                var endNode = Root.AppendNew(Prefabs.BeginEndNode, new Vector2(point.x, Nodes.Last().GetBottom().y - g_Margin) - startNode.Offset);
                Nodes.Add(endNode);
                endNode.Text = "Конец";

                point.y = Nodes.Last().GetBottom().y - 60 - UIController.Margin * 2;
                foreach (var function in Functions)
                {
                    function.Value.MoveTo(point);
                    point.y = function.Value.GetBottom().y - UIController.Margin;
                }


                GenerateLines(Nodes);
                foreach (var f in Functions)
                {
                    GenerateLines(new List<Node> { f.Value });
                }

                if (g_Colorize)
                {
                    ColorizeNames();
                }
            }
        }
#if !DEBUG
        catch (Exception e)
        {
            Message.Instance.Show("Построение схемы не завершено");
            UnityEngine.Debug.LogException(e);
        }
#endif
    }

    private static List<string> ParseCode(string code)
    {
        code = code.Replace("try", "try;");

        int lPos;
        //Замена литералов на ключевые ссылки
        while ((lPos = code.IndexOfAny(new []{'\'','"'})) != -1)
        {
            int lPos2;
            if (code[lPos] == '"')
            {
                lPos2 = code.IndexOf('"',lPos + 1);
            }
            else
            {
                lPos2 = code.IndexOf('\'', lPos + 1);
            }
            Literals.Add(code.Cut(lPos, lPos2));
            code = code.ReverseCut(lPos, lPos2).Insert(lPos,"@%" + (Literals.Count - 1) + "%@");
        }

        int sleshIndex;
        while ((sleshIndex = code.IndexOf("//")) != -1)
        {
            code = code.ReverseCut(sleshIndex, code.IndexOf("\r\n",sleshIndex) - 1);
        }
        while ((sleshIndex = code.IndexOf("{")) != -1)
        {
            code = code.ReverseCut(sleshIndex, code.IndexOf("}", sleshIndex));
        }

        code = code.Replace("initialization", "initialization;");
        var lines = code.Replace("\r\n", " ").Replace("\n", " ").Split(new[] { ";", " then ", " do " }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLower()).ToList();
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].IndexOf(" else ") != -1)
            {
                var nLines = lines[i].Split(new[] { " else " }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                lines.RemoveAt(i);
                lines.InsertRange(i, nLines.SplitList("else"));
            }
        }
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].IndexOf("begin ") != -1/* && !lines[i].StartsWith("procedure") && !lines[i].StartsWith("function")*/)
            {
                var nLines = lines[i].Split(new[] { "begin " }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
                lines.RemoveAt(i);
                lines.InsertRange(i, nLines.SplitList("begin"));
            }
        }
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].IndexOf("end") != -1)
            {
                var nLines = lines[i].Split(new[] { "end" }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
                lines.RemoveAt(i);
                lines.InsertRange(i, nLines.SplitList("end"));
            }
        }
        lines.RemoveAll(x => x == "." || x == "");

        //int nLine = 0;
        //for (int i = 0; i < literals.Count; i++)
        //{
        //    for (int j = nLine; j < lines.Count; j++)
        //    {
        //        if (lines[j].Contains("@%" + i + "%@"))
        //        {
        //            lines[j] = lines[j].Replace("@%" + i + "%@", literals[i]);
        //            nLine = j;
        //        }
        //    }
        //}

        return lines;
    }

    private CodeNode _lastCodeNode;
    private List<Node> GenerateNode(List<string> lines, Vector2 startPoint, ref int index, Transform parent, bool inBody)
    {
        index++;
        var nodes = new List<Node>();
        var line = lines[index];
        if (line == KeyWords.begin.ToString() || line == KeyWords.initialization.ToString())
        {
            List<Node> newNodes;
            while ((newNodes = GenerateNode(lines,startPoint,ref index, parent, true)) != null)
            {
                nodes.AddRange(newNodes);
                if (newNodes.Any())
                {
                    startPoint = new Vector2(startPoint.x, newNodes.Last().GetBottom().y) + new Vector2(0, -g_Margin);
                }
            }
        }
        else if (line == KeyWords.end.ToString() || line == KeyWords.end + ".")
        {
            return null;
        }
        else if (line.StartsWith("function ") || line.StartsWith("procedure "))
        {
            var fNode = parent.AppendNew(Prefabs.FuncNode, startPoint - Prefabs.FuncNode.Offset);
            fNode.Index = index;
            line = line.TrimString("function").TrimString("procedure").Trim();
            fNode.Name = line.Cut(0, line.IndexOf('(') - 1);
            fNode.Text = "Начало <" + fNode.Name + ">";

            do
            {
                int sPos = lines[index].IndexOf("(") + 1;
                int ePos = lines[index].IndexOf(")");
                fNode.Args.Add(lines[index].Cut(sPos, ePos == -1 ? lines[index].Length - 1 : ePos - 1));
            }
            while (lines[index++].IndexOf(")") == -1);


            while (lines[index] != KeyWords.begin.ToString())
            {   
                fNode.Vars.Add(lines[index].TrimString("vars").Trim(')', '(', ' '));
                index++;
            }
            index--;
            fNode.children.AddRange(GenerateNode(lines, (Vector2)fNode.transform.position + new Vector2(0, fNode.ExitPoint.y - g_Margin), ref index, fNode.transform, true));

            var endNode = fNode.transform.AppendNew(Prefabs.BeginEndNode, fNode.GetBottom() - Prefabs.FuncNode.Offset);
            endNode.Text = "Конец";
            fNode.children.Add(endNode);

            nodes.Add(fNode);
        }
        else if (line.StartsWithAny(KeyWords.@if))
        {
            var IfNode = parent.AppendNew(Prefabs.IfNode, startPoint - Prefabs.IfNode.Offset);
            IfNode.Index = index;
            IfNode.Condition = line.Remove(0, 2).Trim(' ');
            IfNode.TrueNodes.AddRange(GenerateNode(lines, (Vector2)IfNode.transform.position + new Vector2(IfWidth/2, IfNode.ExitPoint.y - g_Margin), ref index, parent, true));
            if (lines[index + 1] == "else")
            {
                index++;
                IfNode.FalseNodes.AddRange(GenerateNode(lines, (Vector2)IfNode.transform.position + new Vector2(IfNode.TrueNodes.Any(x => x is IfNode) ? -IfWidth : -IfWidth/2, IfNode.ExitPoint.y - g_Margin), ref index, parent, true));
                if (IfNode.FalseNodes.Any(x => x is IfNode))
                {
                    IfNode.TrueNodes.ForEach(x => x.Translate(new Vector2(IfWidth/2, 0)));
                }

                if (IfNode.TrueNodes.Any() && IfNode.FalseNodes.Any())
                {
                    var d = -IfNode.TrueNodes.Min(x=>x.GetWidthLimits().x) + IfNode.FalseNodes.Max(x=>x.GetWidthLimits().y);
                    if (d > 0)
                    {
                        IfNode.TrueNodes.ForEach(x => x.Translate(new Vector2(d/2 + UIController.MarginLineDistance, 0)));
                        IfNode.FalseNodes.ForEach(x => x.Translate(new Vector2(-d / 2 - UIController.MarginLineDistance, 0)));
                    }
                }
            }
            nodes.Add(IfNode);
            _lastCodeNode = null;
        }
        else if (line.StartsWithAny(KeyWords.@while))
        {
            var WhileNode = parent.AppendNew(Prefabs.WhileNode, startPoint - Prefabs.WhileNode.Offset);
            WhileNode.Index = index;
            WhileNode.Condition = line.Remove(0, 5).Trim(' ');
            WhileNode.children.AddRange(GenerateNode(lines, (Vector2)WhileNode.transform.position + new Vector2(0, WhileNode.ExitPoint.y - g_Margin), ref index, parent, true));
            nodes.Add(WhileNode);
            _lastCodeNode = null;
        }
        else if (line.StartsWith(KeyWords.repeat))
        {
            var WhileNode = parent.AppendNew(Prefabs.RepeatNode, startPoint - Prefabs.WhileNode.Offset);
            WhileNode.Index = index;
            List<Node> newNodes;
            while ((newNodes = GenerateNode(lines, startPoint, ref index, parent, true)) != null)
            {
                WhileNode.children.AddRange(newNodes);
                if (newNodes.Any())
                {
                    startPoint = new Vector2(startPoint.x, newNodes.Last().GetBottom().y) + new Vector2(0, -g_Margin);
                }

                if (newNodes.LastOrDefault() is UntilNode)
                {
                    break;
                }
            }
            nodes.Add(WhileNode);
        }
        else if (line.StartsWithAny(KeyWords.until))
        {
            var untilNode = parent.AppendNew(Prefabs.UntilNode, startPoint - Prefabs.WhileNode.Offset);
            line = line.Remove(0, 5).Trim();
            untilNode.Condition = line;
            nodes.Add(untilNode);
            _lastCodeNode = null;
        }
        else if (line.StartsWithAny(KeyWords.@for))
        {
            var ForNode = parent.AppendNew(Prefabs.ForNode, startPoint - Prefabs.ForNode.Offset);
            ForNode.Index = index;
            ForNode.Condition = line.Remove(0, 3).Trim(' ');
            ForNode.children.AddRange(GenerateNode(lines, (Vector2)ForNode.transform.position + new Vector2(0, ForNode.ExitPoint.y - g_Margin), ref index, parent, true));
            nodes.Add(ForNode);
            _lastCodeNode = null;
        }
        else if (line.StartsWith("write(") || line.StartsWith("read(") || line.StartsWith("writeln(") || line.StartsWith("readln(") || line == "writeln" || line == "write" || line == "read" || line == "readln")
        {
            if (_lastCodeNode != null && _lastCodeNode.EndIndex == index - 1 && _lastCodeNode.GetType() == typeof(IONode))
            {
                _lastCodeNode.EndIndex = index;
                _lastCodeNode.Text += "\n" + line + ";";
                if (_lastCodeNode.EndIndex - _lastCodeNode.Index > MaxNodeLinesCount)
                {
                    _lastCodeNode = null;
                }
            }
            else
            {
                _lastCodeNode = parent.AppendNew(Prefabs.IONode, startPoint - Prefabs.CodeNode.Offset);
                _lastCodeNode.Index = index;
                _lastCodeNode.EndIndex = index;
                _lastCodeNode.Text = line + ";";
                nodes.Add(_lastCodeNode);
            }

        }
        else if (line.StartsWithAny(KeyWords.var) && !inBody)
        {
            VarNode vNode = parent.AppendNew(Prefabs.VarNode, startPoint - Prefabs.VarNode.Offset);
            vNode.Index = index;
            vNode.EndIndex = index;
            vNode.AddVar(line.Remove(0,3).Trim());
            while (!lines[index + 1].StartsWithAny(KeyWords.begin, KeyWords.uses, KeyWords.function, KeyWords.procedure, KeyWords.type, KeyWords.@const)) //while (lines[index+1].ContainsAny("begin", "uses", "function", "procedure", "type") == null)
            {
                vNode.AddVar(lines[index+1]);
                index++;
            }
            nodes.Add(vNode);
        }
        else if (line.StartsWithAny(KeyWords.except))
        {
            var exceptNode = parent.AppendNew(Prefabs.IfNode, startPoint - Prefabs.WhileNode.Offset);
            exceptNode.Index = index;
            exceptNode.Condition = "{exception occured}";
            List<Node> node;
            while ((node = GenerateNode(lines, (Vector2)exceptNode.transform.position + new Vector2(0, exceptNode.ExitPoint.y - g_Margin), ref index, parent, true)) != null)
            {
                exceptNode.TrueNodes.AddRange(node);
            }
            nodes.Add(exceptNode);
            _lastCodeNode = null;
        }
        else if (line.StartsWithAny(KeyWords.@const))
        {
            ConstNode vNode = parent.AppendNew(Prefabs.ConstNode, startPoint - Prefabs.VarNode.Offset);
            vNode.Index = index;
            vNode.EndIndex = index;
            vNode.AddVar(line.Remove(0, 5).Trim());
            while (!lines[index + 1].StartsWithAny()) //while (lines[index+1].ContainsAny("begin", "uses", "function", "procedure", "type") == null)
            {
                vNode.AddVar(lines[index + 1]);
                index++;
            }
            nodes.Add(vNode);
        }
        else if (line.StartsWithAny(KeyWords.uses))
        {
            line = line.Remove(0, 4);
            try
            {

                foreach (var lib in line.Split(',').Select(x => x.Trim()))
                {
                    int inChar = lib.IndexOf(" in ");
                    string fileName = lib + ".pas";
                    if (inChar != -1)
                    {
                        int l1 = lib.IndexOf("@%");
                        int l2 = lib.IndexOf("%@");
                        if (l1 != -1 && l2 > l1)
                        {
                            fileName = Literals[int.Parse(lib.Cut(l1 + 2, l2 - 1))].Trim(' ','.', '\'', '/', (char)92);
                        }
                    }

                    print(fileName);
                    if (System.IO.File.Exists(fileName))
                    {
                        string code = System.IO.File.ReadAllText(fileName);
                        string finCode = null;
                        int implementation = code.IndexOf("implementation");
                        code = code.Remove(0, implementation + "implementation".Length);
                        int finalization = code.IndexOf("finalization");
                        int codeLen = code.Length;
                        if (finalization != -1)
                        {
                            finCode = code.Remove(0, finalization + "finalization".Length);
                            code = code.Substring(0, finalization) + "end.";
                        }
                        //print(code);
                        var parsed = ParseCode(code);
                        var unit = GenerateUnit(parsed, ref startPoint, ref TopPoint, lib.Split()[0]);
                        if (unit.Any())
                        {
                            nodes.AddRange(unit);
                            var joint = parent.AppendNew(Prefabs.JointNode, new Vector2(startPoint.x,unit.Last().GetBottom().y));
                            nodes.Add(joint);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
        else if (line.StartsWithAny(KeyWords.type, KeyWords.program))
        {
            
        }
        else
        {
            var function = Functions.Any() ? line.ContainsAny(Functions.Keys) : null;
            if (function != null)
            {
                FunctionCallNode node = parent.AppendNew(Prefabs.FunctionCallNode, startPoint - Prefabs.CodeNode.Offset);
                node.Index = index;
                node.Text = line + ";";
                node.Function = Functions[function];
                nodes.Add(node);
            }
            else
            {
                if (_lastCodeNode != null && _lastCodeNode.EndIndex == index - 1 && _lastCodeNode.GetType() == typeof(CodeNode))
                {
                    _lastCodeNode.EndIndex = index;
                    _lastCodeNode.Text += "\r\n" + line + ";";
                    if (_lastCodeNode.EndIndex - _lastCodeNode.Index > MaxNodeLinesCount)
                    {
                        _lastCodeNode = null;
                    }
                }
                else
                {
                    _lastCodeNode = parent.AppendNew(Prefabs.CodeNode, startPoint - Prefabs.CodeNode.Offset);
                    _lastCodeNode.Index = index;
                    _lastCodeNode.EndIndex = index;
                    _lastCodeNode.Text = line + ";";
                    nodes.Add(_lastCodeNode);
                }
            }
        }
        return nodes;
    }
    private void GenerateLines(List<Node> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node is IfNode)
            {
                IfNode ifNode = node as IfNode;

                if (ifNode.TrueNodes.Any())
                {
                    ifNode.GetExitPoint("True").SetTarget(ifNode.TrueNodes.First(), null, null);
                }
                if(ifNode.FalseNodes.Any())
                {
                    ifNode.GetExitPoint("False").SetTarget(ifNode.FalseNodes.First(), null, null);
                }

                GenerateLines(ifNode.TrueNodes);
                GenerateLines(ifNode.FalseNodes);
                if (i < nodes.Count - 1)
                {
                    ifNode.GetExitPoint("Exit").SetTarget(nodes[i + 1].GetFirst(), null, null);
                }
            }
            else if (node is WhileNode)
            {
                WhileNode whileNode = node as WhileNode;
                if (!whileNode.Repeat)
                {
                    whileNode.GetExitPoint("Start").SetTarget(whileNode.children.First(), null, null);
                }
                GenerateLines(whileNode.children);
                whileNode.children.Last().GetExitPoint(whileNode.Repeat ? "Start" : "Exit").SetTarget(whileNode.GetFirst(), whileNode.Repeat ? null : whileNode, null);
                if (i < nodes.Count - 1)
                {
                    if (!whileNode.Repeat)
                    {
                        whileNode.GetExitPoint("Exit").SetTarget(nodes[i + 1].GetFirst(), whileNode, null);
                    }
                    else
                    {
                        whileNode.children.Last().GetExitPoint("Exit").SetTarget(nodes[i + 1].GetFirst(), null, null);
                    }
                }
            }
            else if (node is ForNode)
            {
                ForNode forNode = node as ForNode;
                forNode.GetExitPoint("Start").SetTarget(forNode.children.First(), null, null);
                GenerateLines(forNode.children);
                var last = forNode.children.Last();
                last.GetExitPoint("Exit").SetTarget(forNode, forNode, last, "Loop");
                if (i < nodes.Count - 1)
                {
                    forNode.GetExitPoint("Exit").SetTarget(nodes[i + 1].GetFirst(), forNode, null);
                }
            }
            else if (node is FunctionNode)
            {
                FunctionNode fNode = node as FunctionNode;
                fNode.GetExitPoint("Start").SetTarget(fNode.children.First(), null, null);
                GenerateLines(fNode.children);
            }
            else /*if (node is CodeNode || node is BeginEndNode || node is Node)*/
            {
                if (node is CodeNode)
                {
                    var cNode = node as CodeNode;
                    while (cNode.Text.EndsWith("\r\n"))
                    {
                        cNode.Text = cNode.Text.Substring(0, cNode.Text.Length - 2);
                    }
                }
                if (i < nodes.Count - 1)
                {
                    node.GetExitPoint("Exit").SetTarget(nodes[i + 1].GetFirst(), null, null);
                }
            }
        }
    }
    public void ChangeFontSize(int size)
    {
        if (Vars != null)
        {
            Vars.ChangeFontSize(size);
        }

        foreach (var node in Nodes)
        {
            node.ChangeFontSize(size);
        }

        foreach (var f in Functions)
        {
            f.Value.ChangeFontSize(size);
        }
    }
    public void ColorizeNames(NameColor[] nameColors)
    {
        if (Vars != null)
        {
            Vars.ColorizeNames(nameColors);

            foreach (var node in Nodes)
            {
                node.ColorizeNames(nameColors);
            }

            foreach (var f in Functions)
            {
                f.Value.ColorizeNames(nameColors);
            }
        }
    }
    public void ColorizeNames()
    {
        if (Vars != null)
        {
            ColorizeNames(Vars.GetColors());
        }
    }
    public void ColorizeNames(Color color)
    {
        if (Vars != null)
        {
            ColorizeNames(Vars.GetColors().ToWithNewColor(color));
        }
    }

    public void ReplaceFunctions()
    {
        if (UIController.HorizontalLayout)
        {
            Vector2 startPoint = new Vector2(Nodes.Max(x => x.GetWidthLimits().y + 2 * UIController.Margin), Root.position.y + Root.sizeDelta.y * -0.2f);
            foreach (var function in Functions.Values)
            {
                startPoint.x += transform.position.x - function.GetWidthLimits().x;

                function.MoveTo(startPoint);
                startPoint.x = function.GetWidthLimits().y + 2 * UIController.Margin;
            }
        }
        else
        {
            Vector2 startPoint = new Vector2(Root.position.x + Root.sizeDelta.x * 0.5f, Nodes.Last().GetBottom().y - 60 - UIController.Margin * 2);
            foreach (var function in Functions)
            {
                function.Value.MoveTo(startPoint);
                startPoint.y = function.Value.GetBottom().y - UIController.Margin;
            }
        }
    }
}
[System.Serializable]
public class Prefabs
{
    public BeginEndNode BeginEndNode;
    public CodeNode CodeNode;
    public IfNode IfNode;
    public WhileNode WhileNode;
    public WhileNode RepeatNode;
    public ForNode ForNode;
    public UntilNode UntilNode;
    public IONode IONode;
    public FunctionNode FuncNode;
    public FunctionCallNode FunctionCallNode;
    public VarNode VarNode;
    public ConstNode ConstNode;
    public Node JointNode;
}
[System.Serializable]
public struct NameColor
{
    public string Name;
    public Color Color;

    public NameColor(string name, Color color)
    {
        Name = name;
        Color = color;
    }
}