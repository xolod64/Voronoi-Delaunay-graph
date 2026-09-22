using UnityEngine;
using System;
using Unity.Mathematics;
using System.Collections.Generic;

public static class DelaunayVoronoiGraphGenerator
{
    /// <summary>
    /// Generates Delaunay/Voronoi graph on grid
    /// </summary>
    /// <param name="width">grid size at x axis</param>
    /// <param name="height">grid size at y axis</param>
    /// <param name="nodeSize">size of the grid cell</param>
    /// <param name="diagonalStepsCount">number of steps that creates possible node positions (final number of positions equal 1 + diagonalStepsCount * 2)</param>
    /// <param name="minVoronoiEdgeLength">voronoi edge length limiter</param>
    /// <param name="random">seed</param>
    /// <returns>Delaunay/Voronoi graph</returns>
    /// <exception cref="ArgumentException">Occurs if minVoronoiEdgeLength > nodeSize</exception>
    /// <exception cref="ArgumentOutOfRangeException">Occurs if minVoronoiEdgeLength < 3 or diagonalStepsCount < 1</exception>
    /// <exception cref="InvalidOperationException">Occurs if graph cannot be generated at current sizes combination</exception>
    public static DVGraph GenerateGraph(int width, int height, int nodeSize, int diagonalStepsCount, int minVoronoiEdgeLength, System.Random random)
    {
        DVGraph newGraph = new DVGraph(width, height, nodeSize);

        int positionsCount = 1 + diagonalStepsCount * 2; //Final number of possible node positions 
        int allowedOffsetRadius = nodeSize / 8; //Offset from grid cell center that determines possible node positions 

        NodesCombination[,,,] combinationsMatrix; //Contains data about all possible nodes combinations in neighboring cells square

        CheckInputParameters();
        CalculatePossibleCombinations();
        ValidateCombinationsCompleteness();
        InitializeNodesAndEdges();
        GenerateNodesAndEdges();

        return newGraph;

        void CheckInputParameters()
        {
            if (minVoronoiEdgeLength > nodeSize)
            {
                throw new ArgumentException($"nodeSize={nodeSize} too small for minVoronoiEdgeLength={minVoronoiEdgeLength}: ");
            }

            if (minVoronoiEdgeLength < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(minVoronoiEdgeLength), minVoronoiEdgeLength, "must be equal or greater than 3");
            }

            if (diagonalStepsCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(diagonalStepsCount), diagonalStepsCount, "must be greater than 0");
            }
        }

        void CalculatePossibleCombinations()
        {
            int minVoronoiEdgeLengthSqr = minVoronoiEdgeLength * minVoronoiEdgeLength;
            combinationsMatrix = new NodesCombination[positionsCount, positionsCount, positionsCount, positionsCount];

            //Iteration over all possible nodes combinations
            for (int a = -diagonalStepsCount; a < diagonalStepsCount + 1; a++)
            {
                int aX = a * allowedOffsetRadius / diagonalStepsCount;
                int aY = aX;
                Vector2Int currentCenter = new Vector2Int(aX, aY);

                for (int b = -diagonalStepsCount; b < diagonalStepsCount + 1; b++)
                {
                    int bX = b * allowedOffsetRadius / diagonalStepsCount - nodeSize;
                    int bY = b * allowedOffsetRadius / diagonalStepsCount;
                    Vector2Int leftCenter = new Vector2Int(bX, bY);

                    for (int c = -diagonalStepsCount; c < diagonalStepsCount + 1; c++)
                    {
                        int cX = c * allowedOffsetRadius / diagonalStepsCount - nodeSize;
                        int cY = cX;
                        Vector2Int downLeftCenter = new Vector2Int(cX, cY);

                        for (int d = -diagonalStepsCount; d < diagonalStepsCount + 1; d++)
                        {
                            int dX = d * allowedOffsetRadius / diagonalStepsCount;
                            int dY = dX - nodeSize;
                            Vector2Int downCenter = new Vector2Int(dX, dY);

                            //Calculating first pair of circumscribed circles
                            Vector2Int firstCircleCenter = Vector2Int.RoundToInt(Utils.GetCircumcenter(leftCenter, downCenter, downLeftCenter));
                            Vector2Int secondCircleCenter = Vector2Int.RoundToInt(Utils.GetCircumcenter(leftCenter, downCenter, currentCenter));
                            float firstCirclesDist = (firstCircleCenter - secondCircleCenter).sqrMagnitude;

                            NodesCombination newCombination;
                            if ((firstCircleCenter - leftCenter).sqrMagnitude < (firstCircleCenter - currentCenter).sqrMagnitude) //Checking Delaunay criterion
                            {
                                //Form combination, where Delaunay edge goes between down and left center (\)(descending)
                                newCombination = FormCombination(firstCirclesDist, false, firstCircleCenter, secondCircleCenter);
                            }
                            else
                            {
                                //Calculating second pair of circumscribed circles
                                Vector2Int thirdCircleCenter = Vector2Int.RoundToInt(Utils.GetCircumcenter(currentCenter, downLeftCenter, leftCenter));
                                Vector2Int fourthCircleCenter = Vector2Int.RoundToInt(Utils.GetCircumcenter(currentCenter, downLeftCenter, downCenter));
                                float secondCirclesDist = (thirdCircleCenter - fourthCircleCenter).sqrMagnitude;

                                //Form combination, where Delaunay edge goes between down left and current center (/)(ascending)
                                newCombination = FormCombination(secondCirclesDist, true, thirdCircleCenter, fourthCircleCenter);
                            }

                            combinationsMatrix[a + diagonalStepsCount, b + diagonalStepsCount, c + diagonalStepsCount, d + diagonalStepsCount] = newCombination;

                            NodesCombination FormCombination(float circlesDist, bool isDiagonalAscending, Vector2Int aCircleCenter, Vector2Int bCircleCenter)
                            {
                                NodesCombination newCombination = new NodesCombination();

                                if (circlesDist < minVoronoiEdgeLengthSqr) //Checking algorithm min edge length criterion
                                {
                                    newCombination.isValid = false;
                                    return newCombination;
                                }

                                newCombination.isValid = true;
                                newCombination.isDiagonalAscending = isDiagonalAscending;

                                //Determine which voronoi node(circle center) is on the left(nodeA - left, nodeB - right)
                                newCombination.VNodeA = aCircleCenter.x < bCircleCenter.x ? aCircleCenter : bCircleCenter;
                                newCombination.VNodeB = aCircleCenter.x > bCircleCenter.x ? aCircleCenter : bCircleCenter;

                                //Determine which voronoi node(circle center) is upper
                                newCombination.isNodeAUpper = newCombination.VNodeA.y > newCombination.VNodeB.y;

                                return newCombination;
                            }
                        }
                    }
                }
            }
        }

        void ValidateCombinationsCompleteness()
        {
            int degenerateANeighborsCombinations = 0;

            //Iteration over all combinations of A neighbors and possible A positions to check their suitability
            //If none of the possible A positions yield a valid combination for a given B,C,D, the algorithm cannot proceed
            for (int b = 0; b < diagonalStepsCount * 2 + 1; b++)
            {
                for (int c = 0; c < diagonalStepsCount * 2 + 1; c++)
                {
                    for (int d = 0; d < diagonalStepsCount * 2 + 1; d++)
                    {
                        bool isCombinationDegenerate = true;

                        for (int a = 0; a < diagonalStepsCount * 2 + 1; a++)
                        {
                            if (combinationsMatrix[a, b, c, d].isValid) isCombinationDegenerate = false;
                        }

                        if (isCombinationDegenerate) degenerateANeighborsCombinations++;
                    }
                }
            }

            if (degenerateANeighborsCombinations > 0)
            {
                int totalANeighborsCombinations = (int)math.pow(diagonalStepsCount * 2 + 1, 3);
                throw new InvalidOperationException($"Combinations matrix is degenerated: {degenerateANeighborsCombinations}/{totalANeighborsCombinations} combinations arent valid");
            }
        }

        void InitializeNodesAndEdges()
        {
            newGraph.dNodes = new DNode[width, height];
            newGraph.vNodes = new VNode[width - 1, height - 1][,];
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    newGraph.vNodes[x, y] = new VNode[2, 2];
                }
            }

            newGraph.dEdgesOrientation = new bool[width - 1, height - 1];
        }

        void GenerateNodesAndEdges()
        {
            int[,] nodesPositions = new int[width, height];

            List<int> currentPossiblePositions = new List<int>(positionsCount);
            for (int x = 0; x < width; x++) //Iterate over all grid cells and choose node positions
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || y == 0)
                    {
                        nodesPositions[x, y] = random.Next(0, positionsCount);
                    }
                    else
                    {
                        currentPossiblePositions.Clear();

                        for (int i = 0; i < positionsCount; i++)
                        {
                            if (combinationsMatrix[i, nodesPositions[x - 1, y], nodesPositions[x - 1, y - 1], nodesPositions[x, y - 1]].isValid) currentPossiblePositions.Add(i);
                        }

                        nodesPositions[x, y] = currentPossiblePositions[random.Next(0, currentPossiblePositions.Count)];
                    }
                }
            }

            for (int x = 0; x < width; x++) //Iterate over all grid cells and save node and edge coordinates
            {
                for (int y = 0; y < height; y++)
                {
                    int coordsOffset = nodesPositions[x, y] - diagonalStepsCount;

                    //transform local coords to world coords
                    int newDNodeX = x * nodeSize + nodeSize / 2 + coordsOffset * allowedOffsetRadius / diagonalStepsCount;
                    int newDNodeY = y * nodeSize + nodeSize / 2 + coordsOffset * allowedOffsetRadius / diagonalStepsCount;

                    DNode newDNode = new DNode(x, y, newDNodeX, newDNodeY);

                    newGraph.dNodes[x, y] = newDNode;

                    if (x != 0 && y != 0)
                    {
                        NodesCombination currentCombination = combinationsMatrix[nodesPositions[x, y], nodesPositions[x - 1, y], nodesPositions[x - 1, y - 1], nodesPositions[x, y - 1]];

                        //transform local coords to world coords
                        int newVNodeAX = x * nodeSize + nodeSize / 2 + currentCombination.VNodeA.x;
                        int newVNodeAY = y * nodeSize + nodeSize / 2 + currentCombination.VNodeA.y;
                        int newVNodeBX = x * nodeSize + nodeSize / 2 + currentCombination.VNodeB.x;
                        int newVNodeBY = y * nodeSize + nodeSize / 2 + currentCombination.VNodeB.y;

                        VNode newVNodeA = new VNode(x - 1, y - 1, newVNodeAX, newVNodeAY);
                        VNode newVNodeB = new VNode(x - 1, y - 1, newVNodeBX, newVNodeBY);

                        newGraph.vNodes[x - 1, y - 1][0, 0] = newVNodeA;
                        newGraph.vNodes[x - 1, y - 1][0, 1] = newVNodeB;
                        newGraph.vNodes[x - 1, y - 1][1, 0] = currentCombination.isNodeAUpper ? newVNodeA : newVNodeB;
                        newGraph.vNodes[x - 1, y - 1][1, 1] = currentCombination.isNodeAUpper ? newVNodeB : newVNodeA;

                        newGraph.dEdgesOrientation[x - 1, y - 1] = currentCombination.isDiagonalAscending;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Contains data about 4 dNodes combinations: x,y; x-1,y; x,y-1; x-1,y-1;
    /// </summary>
    private struct NodesCombination
    {
        public bool isValid; //Is combination meets the conditions of the algorithm
        public bool isDiagonalAscending; //Is Delaunay edge goes from x-1,y-1 to x,y (/)

        public Vector2Int VNodeA; //Left node
        public Vector2Int VNodeB; //Right node
        public bool isNodeAUpper;
    }
}