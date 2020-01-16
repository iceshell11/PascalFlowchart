using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static bool HorizontalLayout { get; private set; }
    public static float Margin { get; private set; }
    public static int CharLimit { get; private set; }
    public static float LineDistance { get; private set; }
    public static float MarginLineDistance { get { return FlowBuilder.g_Margin * FlowBuilder.g_LineDistance; } }
    public static bool Colorize { get; private set; }
    public static bool Attach { get; private set; }
    public static LayerMask NodesMask { get; set; }
    public static LayerMask LinesMask { get; set; }
    [SerializeField] private ScreenCapture screenCapture;
    [SerializeField] private LayerMask _nodesMask;
    [SerializeField] private LayerMask _linesMask;
    [SerializeField] private FlowBuilder builder;
    [SerializeField] private Text codeInput;
    [SerializeField] private InputField _charLimit;
    [SerializeField] private InputField codeInputField;
    [SerializeField] private Slider marginSlider;
    [SerializeField] private Slider lineDistanceSlider;
    [SerializeField] private List<Color> VarColors;

    void Awake()
    {
        Margin = 50;
        LineDistance = 0.5f;
        marginSlider.value = Margin;
        lineDistanceSlider.value = LineDistance;
        CharLimit = int.Parse(_charLimit.text);
        Colorize = true;
        Attach = true;
        NodesMask = _nodesMask;
        LinesMask = _linesMask;
    }

    void Start()
    {
        var data = GUIUtility.systemCopyBuffer;
        if (data.Contains("begin") && data.Contains("end."))
        {
            LoadFromBuffer();
            Generate();
        }
    }
    public void Generate()
    {
        FlowBuilder.g_Margin = Margin;
        FlowBuilder.g_LineDistance = LineDistance;
        FlowBuilder.g_VarColors = VarColors.ToList();
        FlowBuilder.g_Colorize = Colorize;

        builder.Generate(codeInputField.text);
    }

    public void SaveToPNG(bool split)
    {
        if (Application.isMobilePlatform)
        {
            Message.Instance.Show("Не поддерживается на мобильных платформах");
        }
        else
        {
            screenCapture.Capture(split);
        }
    }

    public void LayoutChange()
    {
        HorizontalLayout = !HorizontalLayout;
        builder.ReplaceFunctions();
    }
    public void LoadFromBuffer()
    {
        codeInputField.text = GUIUtility.systemCopyBuffer;
    }
    public void UpdateMargin(float value)
    {
        Margin = value;
    }

    public void UpdateLineMargin(float value)
    {
        LineDistance = value;
    }
    public void CharLimitUpdate(string value)
    {
        if (value != "")
        {
            CharLimit = int.Parse(value);
            ColorizeChange(Colorize);
        }
    }
    public void ColorizeChange(bool value)
    {
        Colorize = value;
        if (value)
        {
            FlowBuilder.g_VarColors = VarColors;
            builder.ColorizeNames();
        }
        else
        {
            FlowBuilder.g_VarColors = new List<Color> { Color.black };
            builder.ColorizeNames(new Color(50/255f, 50 / 255f, 50 / 255f, 1));
        }
    }
    public void AttachChange(bool value)
    {
        Attach = value;
    }
    public void PathfindingChange(bool value)
    {
        NodeLink.Pathfinding = value;
        //NodeLink.LineAttaching = value;
    }

    public void ChangeFontSize(float fontSize)
    {
        builder.ChangeFontSize((int)fontSize);
    }
}
