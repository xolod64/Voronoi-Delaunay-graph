using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GraphManager))] 
public class GraphManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(15);

        GraphManager scriptTarget = (GraphManager)target;

        if (GUILayout.Button("Generate Custom Graph", GUILayout.Height(20)))
        {
            scriptTarget.GenerateCustomGraph();
        }

        if (GUILayout.Button("Generate Small Scale Graph", GUILayout.Height(20)))
        {
            scriptTarget.GenerateSmallScaleGraph();
        }

        if (GUILayout.Button("Generate Medium Scale Graph", GUILayout.Height(20)))
        {
            scriptTarget.GenerateMediumScaleGraph();
        }

        if (GUILayout.Button("Generate Large Scale Graph", GUILayout.Height(20)))
        {
            scriptTarget.GenerateLargeScaleGraph();
        }
    }
}