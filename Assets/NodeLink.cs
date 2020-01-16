using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class NodeLink : MonoBehaviour, INodeLink
{
    private struct AttachLine
    {
        public Vector2 ConnectPoint;
        public NodeLink Link;
    }

    public Node DefaultLimiter;
    public const float Tolerance = 0.1f;
    private LineRenderer lineRenderer;
    public Vector2 ExitDirection;
    private Node Target;
    private Transform EnterPoint;
    private ILimiter Limit;
    private ILimiter YLimiter;
    public static bool DrawCollider = true;
    public static bool Pathfinding = true;
    private EdgeCollider2D edgeCollider2D;
    private static float MarginLineDistance { get { return UIController.MarginLineDistance; } }

    public NodeLink Attached { get; private set; }
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        if (DrawCollider)
        {
            edgeCollider2D = gameObject.GetComponent<EdgeCollider2D>();
            if (edgeCollider2D == null)
            {
                edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();
            }
        }
    }

    public Vector2 GetLowest()
    {
        return transform.position;
    }
    private NodeLink GetMainLink()
    {
        return Attached ?? this;
    }

    public Vector2 limitsDebug;
    public Transform limiterX;
    public void SetTarget(Node target, ILimiter limiter, ILimiter yLimiter, string enterPoint = "Enter")
    {
        Target = target;
        EnterPoint = Target.GetEnterPoint(enterPoint);
        Limit = limiter;

        YLimiter = yLimiter;
        UpdateLines();
    }

    public void SetTargetPoint(Transform point, ILimiter limiter, ILimiter yLimiter, string enterPoint = "Enter")
    {
        EnterPoint = point;
        Limit = limiter;

        YLimiter = yLimiter;
        UpdateLines();
    }
    private void UpdateLines()
    {
        //Vector2? localLimit = Limit != null ?  (Vector2?)Limit.GetWidthLimits() - Vector2.one * transform.position.x : null;

        //if (DefaultLimiter != null)
        //{
        //    var dLimit = DefaultLimiter.GetWidthLimits() - Vector2.one * transform.position.x;
        //    if (Target is ForNode)
        //    {
        //        print(localLimit);
        //    }
        //    localLimit = localLimit == null ? dLimit : new Vector2(Mathf.Min(localLimit.Value.x,dLimit.x), Mathf.Max(localLimit.Value.y, dLimit.y));
        //}


        GizmoPoints.Clear();
        GizmoLines.Clear();

        //if(localLimit.HasValue)
        //    GizmoLines.Add(new KeyValuePair<Vector2, ColoredVector>(transform.position - Vector3.right * localLimit.Value.x, new ColoredVector(transform.position = Vector3.right * localLimit.Value.y,Color.gray)));

        lineRenderer.positionCount = 6;
        lineRenderer.useWorldSpace = false;
        Vector2 targetPosition = transform.InverseTransformPoint(EnterPoint.position);
        Vector3[] positions = new Vector3[7];




        positions[0] = Vector2.zero;//Начало
        positions[1] = ExitDirection * MarginLineDistance;//Точка выхода

        Vector2? defLim = DefaultLimiter != null ? new Vector2?(DefaultLimiter.GetWidthLimits() - Vector2.one * transform.position.x) : null;
        Vector2? localLimit = Limit != null ? new Vector2(Limit.GetWidthLimits().x - transform.position.x, defLim.HasValue ? defLim.Value.y : 0) : defLim;
        float? yBottom = null;
        if (localLimit.HasValue)
        {
            if (positions[1].x > 0)
            {
                positions[1].x = Mathf.Max(localLimit.Value.y, positions[1].x);
            }
            else if (positions[1].x < 0)
            {
                positions[1].x = Mathf.Min(localLimit.Value.x, positions[1].x);
            }
        }
        if (YLimiter != null)
        {
            yBottom = YLimiter.GetBottom().y - FlowBuilder.g_Margin * FlowBuilder.g_LineDistance - transform.position.y;
        }

        positions[5] = targetPosition + ((Vector2)EnterPoint.up).normalized * MarginLineDistance;//Точка входа
        positions[6] = targetPosition;//Конец

        if (positions[6].y < positions[0].y && Mathf.Abs(positions[6].x - positions[0].x) < 5)
        {
            positions[1] = Utils.NanVector2;
            positions[2] = Utils.NanVector2;
            positions[3] = Utils.NanVector2;
            positions[4] = Utils.NanVector2;
            positions[5] = Utils.NanVector2;
        }
        else
        {
            if (positions[5].y > ExitDirection.y * MarginLineDistance) //Если точка входа выше точки выхода
            {
                if (ExitDirection.x > 0) //Если выходит справа
                {
                    positions[2] = new Vector2(positions[1].x, yBottom.HasValue ? yBottom.Value : positions[1].y);
                    positions[3] = new Vector2(localLimit.HasValue ? localLimit.Value.x : -MarginLineDistance * 5, positions[2].y);
                    if (!Pathfinding)
                    {
                        positions[4] = new Vector2(localLimit.HasValue ? localLimit.Value.x : -MarginLineDistance * 5, positions[5].y);
                    }
                    else
                    {
                        //positions[4] = new Vector2(localLimit.HasValue ? localLimit.Value.x : -MarginLineDistance * 5, positions[5].y);
                        positions[4] = new Vector2(float.NaN, 0);
                    }
                }
                else
                {
                    positions[2] = new Vector2(localLimit.HasValue ? localLimit.Value.x : -MarginLineDistance * 5, positions[1].y);
                    positions[3] = new Vector2(localLimit.HasValue ? localLimit.Value.x : -MarginLineDistance * 5, positions[5].y);
                    positions[4] = new Vector2(float.NaN, 0);
                }

                if(Limit != null)
                { 
                    GizmoLines.Add(new KeyValuePair<Vector2, ColoredVector>(new Vector2(Limit.GetWidthLimits().x, transform.position.y), new ColoredVector(new Vector2(Limit.GetWidthLimits().y, transform.position.y), Color.yellow)));
                    GizmoLines.Add(new KeyValuePair<Vector2, ColoredVector>(new Vector2(localLimit.Value.x + transform.position.x, transform.position.y - 10), new ColoredVector(new Vector2(localLimit.Value.y + transform.position.x, transform.position.y - 10), Color.magenta)));
                }
            }
            else
            {
                float minX = positions[1].x;
                if (Math.Abs(ExitDirection.x) < 0.0001f)   //Если выходит снизу
                {
                    if (localLimit.HasValue && localLimit.Value.y > minX) // Есть ограничение и правое ограничение больше точки выхода
                    {
                        minX = localLimit.Value.y; //Сдвигаем Х вправо до ограничения
                    }
                }
                else if (ExitDirection.x > 0)  //Выходит справа
                {
                    if (localLimit.HasValue && localLimit.Value.y > minX) // Есть ограничение и правое ограничение больше точки выхода
                    {
                        minX = localLimit.Value.y;//Сдвигаем Х вправо до ограничения
                    }

                    if (positions[6].x > minX)//Если точка входа правее точки выхода
                    {
                        minX = positions[6].x;//Сдвигаем Х до точки входа
                    }
                }
                else //Выходит слева
                {
                    if (localLimit.HasValue && localLimit.Value.x < minX)//Если ограничение слева левее точки выхода
                    {
                        minX = localLimit.Value.x;//Сдвигаем Х до левого ограничения
                    }
                    if (positions[6].x < minX)//Если точка входа левее точки выхода
                    {
                        minX = positions[6].x;//Сдвигаем Х до точки входа
                    }
                }
                positions[2] = new Vector2(minX, positions[1].y);
                positions[3] = new Vector2(minX, positions[5].y);
                positions[4] = new Vector2(float.NaN, 0);
            }
        }

        positions = positions.Where(p => !float.IsNaN(p.x)).ToArray();
        //clean up
        for (int i = 1; i < positions.Length - 1; i++)
        {
            if ((Math.Abs(positions[i].x - positions[i - 1].x) < Tolerance && Math.Abs(positions[i].x - positions[i + 1].x) < Tolerance) || (Math.Abs(positions[i].y - positions[i - 1].y) < Tolerance && Math.Abs(positions[i].y - positions[i + 1].y) < Tolerance))
            {
                positions[i] = Utils.NanVector2;
            }
        }

        positions = positions.Where(p => !float.IsNaN(p.x)).Select(x => transform.TransformPoint(x)).ToArray();
        if (Pathfinding)
        {
            List<Vector3> corrected = new List<Vector3>(positions);
            bool uncorrectable = false;
            for (int i = corrected.Count - 2; i > 0; i--)
            {
                RaycastHit2D hit;
                Node node;
                int c = 0;
                do
                {
                    hit = Physics2D.Raycast(corrected[i], Vector2.up, 1, UIController.NodesMask);
                    if (hit.transform != null && (node = hit.transform.GetComponent<Node>()) != null)
                    {
                        GizmoPoints.Add(new ColoredVector(hit.point, Color.red));
                        GizmoLines.Add(new KeyValuePair<Vector2, ColoredVector>(corrected[i], new ColoredVector(corrected[i + 1], Color.red)));
                        if (i == corrected.Count - 2 || i == 1)
                        {
                            uncorrectable = true;
                            break;
                        }

                        var bound = node.GetBound(corrected[i + 1] - corrected[i]);
                        corrected[i] = (Vector2) node.transform.position + bound +bound.normalized * MarginLineDistance;
                        if (!CorrectWay(corrected, i, i + 1))
                        {
                            corrected[i] = (Vector2) node.transform.position - (bound + bound.normalized * MarginLineDistance);
                            CorrectWay(corrected, i, i + 1);
                        }
                    }
                } while (hit.transform != null && hit.transform.GetComponent<Node>() != null && c++ < 5);
            }


            if (!uncorrectable && CorrectWay(corrected, 1, corrected.Count - 2))
            {
                positions = corrected.ToArray();
            }
        }
        if (UIController.Attach)
        {
            Attached = null;
            for (int i = 0; i < positions.Length; i++)
            {
                AttachLine? check = CastToAttach(positions[i]);
                if (check != null)
                {
                    Array.Resize(ref positions, i + 2);
                    positions[i + 1] = check.Value.ConnectPoint;
                    Attached = check.Value.Link;
                    break;
                }
            }
        }


        positions = positions.Select(x => transform.InverseTransformPoint(x)).ToArray();
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i].z = 0;
        }
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
        if (edgeCollider2D != null)
        {
            edgeCollider2D.points = positions.Select(x => (Vector2)x).ToArray();
        }
    }

    private struct ColoredVector
    {
        public Vector2 Vector;
        public Color Color;

        public ColoredVector(Vector2 vector, Color color)
        {
            Vector = vector;
            Color = color;
        }
    }
    private List<ColoredVector> GizmoPoints = new List<ColoredVector>();
    private List<KeyValuePair<Vector2, ColoredVector>> GizmoLines = new List<KeyValuePair<Vector2, ColoredVector>>();

    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        foreach (var gizmoPoint in GizmoPoints)
        {
            Gizmos.color = gizmoPoint.Color;
            Gizmos.DrawSphere(gizmoPoint.Vector, 5);
        }
        foreach (var line in GizmoLines)
        {
            Gizmos.color = line.Value.Color;
            Gizmos.DrawLine(line.Key, line.Value.Vector);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="from">Include</param>
    /// <param name="to">Exclude</param>
    /// <returns></returns>
    private static bool CorrectWay(List<Vector3> path, int from, int to)
    {
        RaycastHit2D hit;
        Node node;
        for (int i = from; i < to; i++)
        {
            if (Math.Abs(path[i].x - path[i + 1].x) > Tolerance && Math.Abs(path[i].y - path[i + 1].y) > Tolerance) //Добавление угла
            {
                Vector2 middle = Utils.NanVector2;
                bool isVertical = false;
                for (int j = 0; j < 2; j++)
                {
                    isVertical = j == 0 ? IsVertical(path[i - 1], path[i], path[i + 1]) : !isVertical;
                    middle = isVertical ? new Vector2(path[i].x, path[i + 1].y) : new Vector2(path[i + 1].x, path[i].y);
                    if ((hit = path[i].RaycastTo(middle, UIController.NodesMask)).transform != null || (hit = middle.RaycastTo(path[i + 1],UIController.NodesMask)).transform != null)
                    {
                        if ((node = hit.transform.GetComponent<Node>()) != null)
                        {
                            if (j != 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            var link = hit.transform.GetComponent<NodeLink>();

                            //todo:link mix
                            break;
                        }
                    }
                    else //Обход препядствия
                    {
                        break;
                    }
                }
                path.Insert(i + 1,middle);
            }
            else if((hit = path[i].RaycastTo(path[i + 1], UIController.NodesMask)).transform != null)
            {
                if ((node = hit.transform.GetComponent<Node>()) != null)
                {
                    bool isVertical = Math.Abs(path[i].y - path[i + 1].y) > Tolerance;
                    var bound = (isVertical ? node.GetBound(path[i],(Vector2)path[i + 1] + Vector2.right) : node.GetBound(path[i], (Vector2)path[i + 1] + Vector2.up)).Expand(MarginLineDistance) + (Vector2)node.transform.position;
                    var boundReverse = (isVertical ? node.GetBound(path[i+1], (Vector2)path[i] + Vector2.right) : node.GetBound(path[i+1], (Vector2)path[i] + Vector2.up)).Expand(MarginLineDistance) + (Vector2)node.transform.position;

                    List<Vector3> lineAdds = GetLineAdds(path, i, bound, boundReverse, isVertical);

                    bound = (isVertical ? node.GetBound(path[i], (Vector2)path[i + 1] + Vector2.left) : node.GetBound(path[i], (Vector2)path[i + 1] + Vector2.down)).Expand(MarginLineDistance) + (Vector2)node.transform.position;
                    boundReverse = (isVertical ? node.GetBound(path[i + 1], (Vector2)path[i] + Vector2.left) : node.GetBound(path[i + 1], (Vector2)path[i] + Vector2.down)).Expand(MarginLineDistance) + (Vector2)node.transform.position;
                    List<Vector3> lineAdds2 = GetLineAdds(path, i, bound, boundReverse, isVertical);

                    if (lineAdds == null)
                    {
                        if (lineAdds2 != null)
                        {
                            path.InsertRange(i + 1, lineAdds2);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (lineAdds2 == null)
                    {
                        path.InsertRange(i + 1, lineAdds);
                    }
                    else
                    {
                        path.InsertRange(i + 1, ComparePaths(lineAdds, lineAdds2) == Compare.Worse ? lineAdds2 : lineAdds);
                    }
                }
                else
                {
                    var link = hit.transform.GetComponent<NodeLink>();

                    //todo:link mix
                    break;
                }
            }
        }
        return true;
    }
    /// <summary>
    /// Return shortest path Sign
    /// </summary>
    /// <param name="listMinus"></param>
    /// <param name="listPlus"></param>
    /// <returns></returns>
    private static Compare ComparePaths(List<Vector2> list1, List<Vector2> list2)
    {
        float l1 = list1.Sum(x => x.sqrMagnitude);
        float l2 = list2.Sum(x => x.sqrMagnitude);
        if (Mathf.Approximately(l1, l2))
        {
            return Compare.Equal;
        }
        else
        {
            return l1 < l2 ? Compare.Better : Compare.Worse;
        }
    }

    private static Compare ComparePaths(List<Vector3> list1, List<Vector3> list2)
    {
        float l1 = list1.ToArray().GetPathLength();
        float l2 = list2.ToArray().GetPathLength();
        if (Mathf.Approximately(l1, l2))
        {
            return Compare.Equal;
        }
        else
        {
            return l1 < l2 ? Compare.Better : Compare.Worse;
        }
    }

    private static List<Vector3> GetLineAdds(List<Vector3> path, int index, Vector2 bound, Vector2 boundReverse, bool isVertical)
    {
        List<Vector3> lineAdds = new List<Vector3>(4);

        if (isVertical)
        {
            lineAdds.Add(new Vector2(path[index].x, bound.y));
            lineAdds.Add(new Vector2(bound.x, bound.y));
            lineAdds.Add(new Vector2(bound.x, boundReverse.y)); //boundReverse.y in the same time
            lineAdds.Add(new Vector2(path[index].x, boundReverse.y));
        }
        else
        {
            lineAdds.Add(new Vector2(bound.x, path[index].y));
            lineAdds.Add(new Vector2(bound.x, bound.y));
            lineAdds.Add(new Vector2(boundReverse.x, bound.y)); //boundReverse.y in the same time
            lineAdds.Add(new Vector2(boundReverse.x, path[index].y));
        }

        RaycastHit2D hit;
        for (int i = 0; i < lineAdds.Count - 1; i++)
        {
            if ((hit = lineAdds[i].RaycastTo(lineAdds[i + 1], UIController.NodesMask)).transform != null && hit.transform.GetComponent<Node>() != null)
            {
                return null;
            }
        }

        lineAdds.Reverse();
        return lineAdds;
    }
    private static bool IsVertical(Vector2 from, Vector2 center, Vector2 to)
    {
        return Mathf.Clamp(center.y, Mathf.Min(from.y, to.y), Mathf.Max(from.y, to.y)) == center.y;
    }
    private static bool IsVerticalLooking(Vector2 from, Vector2 to)
    {
        return Mathf.Abs(from.x - to.x) < Tolerance;
    }

    public Vector3[] GetPoints()
    {
        Vector3[] vert = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(vert);
        return vert;
    }
    private AttachLine? CastToAttach(Vector2 from)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(from, Vector2.down, 1000);
        AttachLine? line = null;
        float length = 0;
        foreach (var hit in hits)
        {
            NodeLink nodeLink;
            if ((nodeLink = hit.transform.GetComponent<NodeLink>()) != null)
            {
                if (nodeLink != this && nodeLink.EnterPoint == EnterPoint && nodeLink.GetMainLink() != this)
                {
                    line = new AttachLine { ConnectPoint = hit.point, Link = nodeLink };
                    length = Vector2.Distance(hit.point, transform.position);
                    break;
                }
            }
            else if(hit.collider.tag == "Block")
            {
                break;
            }
        }

        hits = Physics2D.RaycastAll(from, Vector2.left, 1000);
        foreach (var hit in hits)
        {
            NodeLink nodeLink;
            if ((nodeLink = hit.transform.GetComponent<NodeLink>()) != null && nodeLink.GetMainLink() != this)
            {
                if (nodeLink != this && nodeLink.EnterPoint == EnterPoint)
                {
                    if (Vector2.Distance(hit.point, transform.position) < length)
                    {
                        line = new AttachLine { ConnectPoint = hit.point, Link = nodeLink };
                        break;
                    }
                }
            }
            else if (hit.collider.tag == "Block")
            {
                break;
            }
        }

        return line;
    }

    void Update()
    {
        if (EnterPoint != null)
        {
            UpdateLines();
        }
    }

}

public class NodeLinkHub : INodeLink
{
    private List<INodeLink> List = new List<INodeLink>();
    public void SetTarget(Node target, ILimiter limiter, ILimiter yLimiter, string enterPoint = "Enter")
    {
        List.ForEach(x=>x.SetTarget(target, limiter, yLimiter, enterPoint));
    }
    public void SetTargetPoint(Transform point, ILimiter limiter, ILimiter yLimiter, string enterPoint = "Enter")
    {
        List.ForEach(x => x.SetTargetPoint(point, limiter, yLimiter, enterPoint));
    }
    public NodeLinkHub(List<INodeLink> list)
    {
        List = list;
    }
    public NodeLinkHub(params INodeLink[] list)
    {
        List = list.ToList();
    }

    public Vector2 GetLowest()
    {
        return List.Select(x=>x.GetLowest()).MinElement(x => x.y);
    }
}
public interface INodeLink
{
    void SetTarget(Node target, ILimiter limiter, ILimiter yLimiter, string enterPoint = "Enter");
    void SetTargetPoint(Transform point, ILimiter limiter, ILimiter yLimiter, string enterPoint = "Enter");
    Vector2 GetLowest();
}
