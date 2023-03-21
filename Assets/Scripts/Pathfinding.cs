using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public static class Pathfinding
{
    // NOTE(sqdrck): This takes 0.08ms. May be room for optimisation, but i thing thats enough for now.
    public static float2[] CalculatePath(Tile[,] tiles, TileType walkableMask, int2 from, int2 to, bool includeLastNode = true)
    {
        System.Diagnostics.Stopwatch sw = new();
        sw.Restart();
        int xSize = tiles.GetLength(0);
        //int x = 3;
        int ySize = tiles.GetLength(1);
        //int y = 3;
        CalculatePathJob job = new CalculatePathJob
        {
            startPosition = from,
            endPosition = to,
            gridSize = new int2(xSize, ySize),
            graph = new NativeArray<PathNode>(xSize * ySize, Allocator.Persistent),
            result = new NativeList<int2>(Allocator.Persistent),
        };
        job.InitGraph();

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                job.SetIsWalkable(x, y, (int)(tiles[x, y].Type & walkableMask) != 0);
            }
        }

        var handle = job.Schedule();
        handle.Complete();

        int2[] result = new int2[job.result.Length];
        for (int i = 0; i < job.result.Length; i++)
        {
            result[job.result.Length - i - 1] = new int2(job.result[i].x, job.result[i].y);
        }

        job.result.Dispose();
        job.graph.Dispose();

        sw.Stop();
        //Debug.Log("Pathfinding time: " + sw.Elapsed.TotalMilliseconds + "ms");
        float2[] path = new float2[includeLastNode ? result.Length : result.Length - 1];
        for (int i = 0; i < path.Length; i++)
        {
            path[i] = new float2(result[i].x, result[i].y);
        }
        return path;
    }

    [BurstCompile]
    private struct CalculatePathJob : IJob
    {
        public int2 startPosition;
        public int2 endPosition;
        public int2 gridSize;
        public NativeList<int2> result;
        public NativeArray<PathNode> graph;

        public void SetIsWalkable(int x, int y, bool walkable)
        {
            PathNode walkablePathNode = graph[CalculateIndex(x, y)];
            walkablePathNode.SetIsWalkable(walkable);
            graph[CalculateIndex(x, y)] = walkablePathNode;
        }

        public void InitGraph()
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    PathNode pathNode = new PathNode();
                    pathNode.x = x;
                    pathNode.y = y;
                    pathNode.index = CalculateIndex(x, y);

                    pathNode.gCost = int.MaxValue;
                    pathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                    pathNode.CalculateFCost();

                    pathNode.cameFromNodeIndex = -1;
                    pathNode.isWalkable = true;

                    graph[pathNode.index] = pathNode;
                }
            }
        }

        public void Execute()
        {

            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(4, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left
            neighbourOffsetArray[1] = new int2(+1, 0); // Right
            neighbourOffsetArray[2] = new int2(0, +1); // Up
            neighbourOffsetArray[3] = new int2(0, -1); // Down
                                                       //neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
                                                       //neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
                                                       //neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
                                                       //neighbourOffsetArray[7] = new int2(+1, +1); // Right Up

            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y);

            PathNode startNode = graph[CalculateIndex(startPosition.x, startPosition.y)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            graph[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);

            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, graph);
                PathNode currentNode = graph[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    // Reached our destination!
                    break;
                }

                // Remove current node from Open List
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentNodeIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                    if (!IsPositionInsideGrid(neighbourPosition, gridSize))
                    {
                        // Neighbour not valid position
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        // Already searched this node
                        continue;
                    }

                    PathNode neighbourNode = graph[neighbourNodeIndex];
                    if (!neighbourNode.isWalkable)
                    {
                        // Not walkable
                        if (neighbourNode.index != endNodeIndex)
                        {
                            continue;
                        }
                    }

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        graph[neighbourNodeIndex] = neighbourNode;

                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                        }
                    }

                }
            }

            PathNode endNode = graph[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1)
            {
                // Didn't find a path!
                //Debug.Log("Didn't find a path!");
            }
            else
            {
                // Found a path
                CalculatePath(graph, endNode);
                foreach (var pathPosition in graph)
                {
                    //Debug.Log(pathPosition.x + ", " + pathPosition.y + " " + pathPosition.isWalkable.ToString());
                }
            }

            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }

        private void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                // Couldn't find a path!
            }
            else
            {
                // Found a path
                result.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    result.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }

            }
        }

        private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
        {
            return
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }

        private int CalculateIndex(int x, int y)
        {
            return x + y * gridSize.x;
        }

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return math.min(xDistance, yDistance) + 10 * remaining;
        }


        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }
            return lowestCostPathNode.index;
        }



    }

    private struct PathNode
    {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;

        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
        }
    }
}
