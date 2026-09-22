using System.Collections.Generic;
using UnityEngine;

public class GraphVisualizer : MonoBehaviour
{
    [SerializeField] public SpriteRenderer spriteRenderer;
    private Texture2D texture;
    private Color32[] pixelBuffer;

    private int windowWidth = 480;
    private int windowHeight = 270;

    private int offsetX;
    private int offsetY;

    private DVGraph graph;
    private int graphWidth;
    private int graphHeight;

    public void SetGraphAndInitTexture(DVGraph graph)
    {
        this.graph = graph;

        graphWidth = graph.width * graph.nodeSize;
        graphHeight = graph.height * graph.nodeSize;

        offsetX = 0;
        offsetY = 0;

        CheckOffset();

        texture = new Texture2D(windowWidth, windowHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        pixelBuffer = new Color32[windowWidth * windowHeight];

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, windowWidth, windowHeight),
            new Vector2(0.5f, 0.5f),
            1f
        );

        spriteRenderer.sprite = sprite;

        RenderWindow();
    }

    public void RenderWindow()
    {
        // for (int py = 0; py < windowHeight; py++)
        // {
        //     for (int px = 0; px < windowWidth; px++)
        //     {
        //         pixelBuffer[py * windowWidth + px] = Color.black;
        //     }
        // }
        System.Array.Clear(pixelBuffer, 0, pixelBuffer.Length);

        int gridStartX = offsetX / graph.nodeSize;
        int gridStartY = offsetY / graph.nodeSize;
        int gridEndX = (offsetX + windowWidth) / graph.nodeSize + 1;
        int gridEndY = (offsetY + windowHeight) / graph.nodeSize + 1;

        List<Vector2Int> edge = new List<Vector2Int>();

        for (int gx = gridStartX; gx < gridEndX; gx++)
        {
            for (int gy = gridStartY; gy < gridEndY; gy++)
            {
                if (gx < graph.width && gy < graph.height)
                {
                    int dx = graph.dNodes[gx, gy].x - offsetX;
                    int dy = graph.dNodes[gx, gy].y - offsetY;

                    if (gx < graph.width - 1 && gy < graph.height - 1)
                    {
                        int dVAX = graph.vNodes[gx, gy][1, 0].x - offsetX;
                        int dVAY = graph.vNodes[gx, gy][1, 0].y - offsetY;
                        int dVBX = graph.vNodes[gx, gy][1, 1].x - offsetX;
                        int dVBY = graph.vNodes[gx, gy][1, 1].y - offsetY;

                        Utils.GetPointsOnLine(dVAX, dVAY, dVBX, dVBY, edge);
                        DrawEdge(edge);

                        if (gx > 0)
                        {
                            int upVAX = graph.vNodes[gx, gy][0, 0].x - offsetX;
                            int upVAY = graph.vNodes[gx, gy][0, 0].y - offsetY;
                            int upVBX = graph.vNodes[gx - 1, gy][0, 1].x - offsetX;
                            int upVBY = graph.vNodes[gx - 1, gy][0, 1].y - offsetY;

                            Utils.GetPointsOnLine(upVAX, upVAY, upVBX, upVBY, edge);
                            DrawEdge(edge);
                        }

                        if (gy > 0)
                        {
                            int rVAX = graph.vNodes[gx, gy][1, 1].x - offsetX;
                            int rVAY = graph.vNodes[gx, gy][1, 1].y - offsetY;
                            int rVBX = graph.vNodes[gx, gy - 1][1, 0].x - offsetX;
                            int rVBY = graph.vNodes[gx, gy - 1][1, 0].y - offsetY;

                            Utils.GetPointsOnLine(rVAX, rVAY, rVBX, rVBY, edge);
                            DrawEdge(edge);
                        }

                        void DrawEdge(List<Vector2Int> edge)
                        {
                            foreach (Vector2Int point in edge)
                            {
                                int px = point.x;
                                int py = point.y;

                                if (px >= 0 && px < windowWidth && py >= 0 && py < windowHeight) pixelBuffer[py * windowWidth + px] = Color.yellow;
                            }
                        }
                    }

                    if (dx >= 0 && dx < windowWidth && dy >= 0 && dy < windowHeight) pixelBuffer[dy * windowWidth + dx] = Color.green;
                }
            }
        }

        texture.SetPixels32(pixelBuffer);
        texture.Apply(false);
    }

    public void MoveOffset(int x, int y)
    {
        offsetX += x;
        offsetY += y;

        CheckOffset();
        RenderWindow();
    }

    private void CheckOffset()
    {
        offsetX = Mathf.Clamp(offsetX, 0, graphWidth - windowWidth);
        offsetY = Mathf.Clamp(offsetY, 0, graphHeight - windowHeight);
    }
}