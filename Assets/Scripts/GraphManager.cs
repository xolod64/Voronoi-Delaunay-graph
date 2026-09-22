using UnityEngine;
using UnityEngine.InputSystem;

public class GraphManager : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public int nodeSize = 32;
    public int diagonalStepsCount = 2;
    public int seed = 1488;
    private int minBorderLength = 3;

    public float pixelsPerSecond = 200f;
    private Vector2 keyboardAccumulator;

    public GraphVisualizer visualizer;
    private System.Random random;
    private DVGraph graph;

    void Update()
    {
        if (Keyboard.current == null) return; // немає клавіатури (наприклад, платформа без неї)

        float dx = 0, dy = 0;

        if (Keyboard.current.dKey.isPressed) dx += 1;
        if (Keyboard.current.aKey.isPressed) dx -= 1;
        if (Keyboard.current.wKey.isPressed) dy += 1;
        if (Keyboard.current.sKey.isPressed) dy -= 1;

        if (dx != 0 || dy != 0)
        {
            keyboardAccumulator += new Vector2(dx, dy) * pixelsPerSecond * Time.deltaTime;

            int ix = (int)keyboardAccumulator.x;
            int iy = (int)keyboardAccumulator.y;

            if (ix != 0 || iy != 0)
            {
                keyboardAccumulator.x -= ix;
                keyboardAccumulator.y -= iy;
                visualizer.MoveOffset(ix, iy);
            }
        }
    }

    public void GenerateCustomGraph()
    {
        random = new System.Random(seed);
        graph = DelaunayVoronoiGraphGenerator.GenerateGraph(width, height, nodeSize, diagonalStepsCount, minBorderLength, random);
        visualizer.SetGraphAndInitTexture(graph);
    }

    public void GenerateSmallScaleGraph()
    {
        random = new System.Random(seed);
        graph = DelaunayVoronoiGraphGenerator.GenerateGraph(width, height, 32, 4, 4, random);
        visualizer.SetGraphAndInitTexture(graph);
    }

    public void GenerateMediumScaleGraph()
    {
        random = new System.Random(seed);
        graph = DelaunayVoronoiGraphGenerator.GenerateGraph(width, height, 48, 6, 5, random);
        visualizer.SetGraphAndInitTexture(graph);
    }

    public void GenerateLargeScaleGraph()
    {
        random = new System.Random(seed);
        graph = DelaunayVoronoiGraphGenerator.GenerateGraph(width, height, 64, 8, 6, random);
        visualizer.SetGraphAndInitTexture(graph);
    }
}
