#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CreateAssetMenu(menuName = "Video Choices/Video Choice Graph")]
public class VideoChoiceGraph : NodeGraph { }

public class VideoChoiceGraphWindow : EditorWindow
{
    [MenuItem("Window/Video Choice Graph")]
    public static void OpenGraphWindow()
    {
        var graph = CreateInstance<VideoChoiceGraph>();
        NodeEditorWindow.Open(graph);
    }
}
#endif