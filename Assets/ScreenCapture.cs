using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ScreenCapture : MonoBehaviour
{
    private int resWidth, resHeight;
    public Transform Target;
    public float Padding;
    public Camera Camera;
    [ContextMenu("Capture")]
    public void Capture(bool split)
    {
        Vector2 topLeft, bottomRight;
        Vector3[] data;
        topLeft.x = Mathf.Min(Target.GetComponentsInChildren<Node>().Where(x=>x.tag == "Block").Min(x => x.GetWidthLimits().x), Target.GetComponentsInChildren<LineRenderer>().Min(
            x =>
            {
                data = new Vector3[x.positionCount];
                x.GetPositions(data);
                return x.transform.position.x + data.Min(q => q.x);
            }));
        topLeft.y = Target.GetComponentsInChildren<Transform>().Where(x => x.tag == "Block").Max(x => x.position.y);
        bottomRight.x = Mathf.Max(Target.GetComponentsInChildren<Node>().Where(x => x.tag == "Block").Max(x => x.GetWidthLimits().y), Target.GetComponentsInChildren<LineRenderer>().Max(
            x =>
            {
                data = new Vector3[x.positionCount];
                x.GetPositions(data);
                return x.transform.position.x + data.Max(q => q.x);
            }));
        bottomRight.y = Target.GetComponentsInChildren<Transform>().Where(x => x.tag == "Block").Min(x => x.position.y);
        if (!split)
        {
            float ratio = Screen.width / (float)Screen.height;
            float size = Mathf.Max((bottomRight.x - topLeft.x + Padding) / 2 / ratio, (topLeft.y - bottomRight.y + Padding) / 2);
            Camera.orthographicSize = size;
            Camera.rect = new Rect(0, 0, (bottomRight.x - topLeft.x + Padding) / (topLeft.y - bottomRight.y + Padding), 1);
            RendererList.Add((topLeft + bottomRight) / 2f);
            Camera.transform.position = (topLeft + bottomRight) / 2f;
            Camera.transform.position += Vector3.back;
        }
        else
        {
            Camera.rect = new Rect(0,0,1,1);
            Camera.orthographicSize = 500;
            float height = Camera.orthographicSize;
#if UNITY_EDITOR
            float width = Camera.orthographicSize / Screen.currentResolution.height * Screen.currentResolution.width;
#else
        float width = Camera.orthographicSize / Screen.height * Screen.width;
#endif
            var sPoint = topLeft + new Vector2(width - Padding, -height + Padding);
            Camera.transform.position = sPoint;
            RendererList.Clear();
            do
            {
                do
                {
                    //var g = new GameObject(Camera.transform.position.ToString());
                    //g.transform.position = transform.position;
                    //GameObject.Instantiate(Camera.gameObject,Camera.transform.position, Quaternion.identity);
                    RendererList.Add(transform.position);
                    Camera.transform.Translate(new Vector2(0, -height * 2), Space.World);
                }
                while (Camera.transform.position.y + height - Padding > bottomRight.y);

                Camera.transform.position = new Vector2(Camera.transform.position.x + width * 2, sPoint.y);
            }
            while (Camera.transform.position.x - width + Padding < bottomRight.x);
        }

        filePath = GetScreenCapturePath();
    }

    public List<Vector2> RendererList;
    public int FileCounter = 0;
    private bool toRenderer = false;
    private string filePath;
    private void Update()
    {
        if (!toRenderer && RendererList.Count > 0)
        {
            Camera.transform.position = (Vector3)RendererList[0] + Vector3.forward * -10;
            RendererList.RemoveAt(0);
            toRenderer = true;
        }
    }

    private void LateUpdate()
    {
        if (toRenderer)
        {
            var filename = CamCapture(filePath);
            toRenderer = false;
            if (RendererList.Count == 0)
            {
                FileCounter = 0;
                ShowExplorer(filename);
            }
        }
    }

    string CamCapture(string path)
    {
        int size = (int)Mathf.Clamp(Camera.orthographicSize / 250, 2, 8);
        resWidth = Screen.width * size;
        resHeight = Screen.height * size;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        Camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D((int)(resWidth * Camera.rect.width), resHeight, TextureFormat.RGB24, false);
        Camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, (int)(resWidth * Camera.rect.width), resHeight), 0, 0);
        Camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();



        FileCounter++;
        var filename = path + FileCounter + ".png";


        System.IO.File.WriteAllBytes(filename, bytes);
        print(string.Format("Took screenshot to: {0}", filename));
        return filename;
    }
    public static void ShowExplorer(string itemPath)
    {
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }
    string GetScreenCapturePath()
    {
        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PascalFlowChart\"))
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PascalFlowChart\");
        }
        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PascalFlowChart\"))
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PascalFlowChart\");
        }
        var folderName = DateTime.Now.ToString("G").Replace("/", "-").Replace(":", "-").Replace(@"\", "-");
        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PascalFlowChart\" + folderName + @"\"))
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PascalFlowChart\" + folderName + @"\");
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PascalFlowChart\" + folderName + @"\";
    }
}
