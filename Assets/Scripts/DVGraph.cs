using System.Collections.Generic;
using UnityEngine;

public class DVGraph
{
    public int width;
    public int height;

    public int nodeSize;

    public DNode[,] dNodes;
    //First [,] - coords on grid, second - [0,0] - left node; [0,1] - right node; [1,0] - up node; [1,1] - down node;
    public VNode[,][,] vNodes;

    //Сoordinates correspond to the coordinates of vNodes array; true - Delaunay edge is ascending (/); false - Delaunay edge is descending (\);
    public bool[,] dEdgesOrientation;

    /// <summary>
    /// Fills the provided list with neighboring Delaunay nodes
    /// </summary>
    /// <param name="dNode">The target node</param>
    /// <param name="neighbors">Neighbors list. Clears before filling</param>
    public void GetDNodeNeighbors(DNode dNode, List<DNode> neighbors)
    {
        neighbors.Clear();

        int x = dNode.arrayX;
        int y = dNode.arrayY;

        //Orthogonal neighbors
        if (x != 0)
            neighbors.Add(dNodes[x - 1, y]);
        if (x != width - 1)
            neighbors.Add(dNodes[x + 1, y]);
        if (y != 0)
            neighbors.Add(dNodes[x, y - 1]);
        if (y != height - 1)
            neighbors.Add(dNodes[x, y + 1]);

        //Diagonal neighbors
        if (x > 0 && y > 0) if (dEdgesOrientation[x - 1, y - 1])
            neighbors.Add(dNodes[x - 1, y - 1]);
        if (x > 0 && y < height - 1) if (!dEdgesOrientation[x - 1, y])
            neighbors.Add(dNodes[x - 1, y + 1]);
        if (x < width - 1 && y < height - 1) if (dEdgesOrientation[x, y])
            neighbors.Add(dNodes[x + 1, y + 1]);
        if (x < width - 1 && y > 0) if (!dEdgesOrientation[x, y - 1])
            neighbors.Add(dNodes[x + 1, y - 1]);
    }

    /// <summary>
    /// Fills the provided list with neighboring Voronoi nodes
    /// </summary>
    /// <param name="vNode">The target node</param>
    /// <param name="neighbors">Neighbors list. Clears before filling</param>
    public void GetVNodeNeighbors(VNode vNode, List<VNode> neighbors)
    {
        neighbors.Clear();

        int x = vNode.arrayX;
        int y = vNode.arrayY;

        bool isCurrentLeft = vNodes[x, y][0, 0] == vNode;
        bool isCurrentUp = vNodes[x, y][1, 0] == vNode;

        neighbors.Add(isCurrentLeft ? vNodes[x, y][0, 1] : vNodes[x, y][0, 0]);
        if (isCurrentLeft && x > 0)
            neighbors.Add(vNodes[x - 1, y][0, 1]);
        else if (!isCurrentLeft && x < width - 2)
            neighbors.Add(vNodes[x + 1, y][0, 0]);

        if (!isCurrentUp && y > 0)
            neighbors.Add(vNodes[x, y - 1][1, 0]);
        else if (isCurrentUp && y < height - 2)
            neighbors.Add(vNodes[x, y + 1][1, 1]);
    }

    public DVGraph(int width, int height, int nodeSize)
    {
        this.width = width;
        this.height = height;

        this.nodeSize = nodeSize;
    }
}