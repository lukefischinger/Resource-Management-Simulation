using UnityEngine;
using QuikGraph;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

using static GraphFunctions;
using Unity.Collections;
using Unity.Jobs;

public class PathwayGraph
{
    public NativeGraph pathways, buildings;
    List<int> buildingNodes;


    Dictionary<(int, int), IEnumerable<Edge<int>>> paths = new Dictionary<(int, int), IEnumerable<Edge<int>>>();
    Dictionary<(int, int), float> lookupTimes = new Dictionary<(int, int), float>();

    const float lookupTimeRemoval = 60f;

    public int Dimension { get; private set; }

    public PathwayGraph(List<int> buildingNodes, int dimension)
    {
        Dimension = dimension;
        this.buildingNodes = buildingNodes;
        pathways = new NativeGraph(Dimension);
        buildings = new NativeGraph(Dimension);
    }

    public PathwayGraph(int dimension)
    {
        Dimension = dimension;
        buildingNodes = new List<int>();
        pathways = new NativeGraph(Dimension);
        buildings = new NativeGraph(Dimension);
    }


    public void CalculatePaths(List<PathRequest> requests)
    {
        NativeArray<NativeGraph> graphs = new NativeArray<NativeGraph>(requests.Count, Allocator.TempJob);
        NativeArray<int> s = new NativeArray<int>(requests.Count, Allocator.TempJob);
        NativeArray<int> t = new NativeArray<int>(requests.Count, Allocator.TempJob);

        for (int i = 0; i < requests.Count; i++)
        {
            graphs[i] = CreatePathGraph(pathways, requests[i].start, requests[i].target);
            s[i] = requests[i].start;
            t[i] = requests[i].target;
        }

        NativeArray<UnsafeList<int>> paths = new NativeArray<UnsafeList<int>>(4, Allocator.TempJob);
        AStarJob aStarJob = new AStarJob();

        aStarJob.graphs = graphs;
        aStarJob.s = s;
        aStarJob.t = t;
        aStarJob.paths = paths;

        JobHandle handle = aStarJob.Schedule(requests.Count, 1);
        handle.Complete();


        for (int i = 0; i < requests.Count; i++)
        {
            requests[i].requester.SetPath(ToVector(paths[i], graphs[i].dimension));
            paths[i].Dispose();
        }
        paths.Dispose();
        s.Dispose();
        t.Dispose();
    }

    Vector2[] ToVector(UnsafeList<int> path, int dimension)
    {
        Vector2[] result = new Vector2[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            result[i] = IntToVector2(path[i], dimension);
        }
        return result;
    }


    NativeGraph CreatePathGraph(NativeGraph graph, int s, int t)
    {
        NativeGraph result = new NativeGraph(graph);
        Edge closest = GetClosestVertex(result, s, 0, result.dimension, 0, result.dimension);
        result.AddEdge(s, closest.target, closest.weight);
        AddClosestEdges(result, t);
        return result;
    }

    public void AddBuildingNode(int node)
    {
        if (!buildingNodes.Contains(node))
        {
            buildingNodes.Add(node);
        }

        AddClosestBuildingNodeEdges(buildings, node);
        AddSquareAround(pathways, node);
    }

    void AddClosestBuildingNodeEdges(NativeGraph graph, int v)
    {
        Edge edge;
        int minRow, maxRow, minCol, maxCol;
        int row = v % Dimension,
        col = v / Dimension;


        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                minRow = j < 0 ? 0 : row + 1;
                maxRow = j < 0 ? row : Dimension - 1;
                minCol = i < 0 ? 0 : (j < 0 ? col : col + 1);
                maxCol = i < 0 ? (j < 0 ? col - 1 : col) : Dimension - 1;

                edge = GetClosestBuildingNode(v, minRow, maxRow, minCol, maxCol);
                if (edge.target != -1)
                {
                    graph.AddEdge(v, edge.target, edge.weight);
                    graph.AddEdge(edge.target, v, edge.weight);

                    AddSquareEdgesFromBuildingNodeEdges(graph, pathways, v);
                }
            }
        }
    }


    void AddSquareEdgesFromBuildingNodeEdges(NativeGraph buildingGraph, NativeGraph pathGraph, int v)
    {
        int difference, rowDiff, colDiff;
        int currSource, currTarget;
        float weight;

        for (int k = 0; k < buildingGraph.edges[v].Length; k++)
        {
            difference = buildingGraph.edges[v][k].target - v;
            rowDiff = Mathf.Abs(difference % Dimension);
            colDiff = Mathf.Abs(difference / Dimension);

            weight = buildingGraph.edges[v][k].weight;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int mirrorRow = rowDiff < colDiff ? -1 : 1;

                    currSource = v + i * Dimension + j;
                    currTarget = v + difference + mirrorRow * i * Dimension - mirrorRow * j;

                    if ((i != 0 || j != 0) && currSource % Dimension + (difference - j) % Dimension < Dimension)
                    {
                        pathGraph.AddEdge(currSource, currTarget, weight);
                    }
                }
            }
        }
    }

    Edge GetClosestBuildingNode(int v, int minRow, int maxRow, int minCol, int maxCol)
    {
        float closestDistance = Mathf.Infinity;
        float currDistance;
        int closest = -1;
        int row, col;
        bool validVertex;

        for (int i = 0; i < buildingNodes.Count; i++)
        {
            row = buildingNodes[i] % Dimension;
            col = buildingNodes[i] / Dimension;
            validVertex = row >= minRow && row <= maxRow && col >= minCol && col <= maxCol;

            if (validVertex && buildingNodes[i] != v)
            {
                currDistance = Distance(v, buildingNodes[i], Dimension, DistanceType.Euclidean);
                if (currDistance < closestDistance)
                {
                    closestDistance = currDistance;
                    closest = buildingNodes[i];
                }
            }
        }

        return new Edge(closest, closestDistance);
    }

    void AddClosestEdges(NativeGraph graph, int v)
    {
        Edge e;
        int minRow, maxRow, minCol, maxCol;
        int row = v % Dimension,
        col = v / Dimension;


        for (int i = -1; i <= 1; i += 2)
        {
            minCol = 0; maxCol = Dimension;
            minRow = i == 1 ? row + 1 : 0;
            maxRow = i == 1 ? Dimension : row - 1;
            e = GetClosestVertex(graph, v, minRow, maxRow, minCol, maxCol);
            if (e.target != -1)
            {
                graph.AddEdge(v, e.target, e.weight);
                graph.AddEdge(e.target, v, e.weight);
            }

            minRow = 0; maxRow = Dimension;
            minCol = i == 1 ? col + 1 : 0;
            maxCol = i == 1 ? Dimension : col - 1;
            e = GetClosestVertex(graph, v, minRow, maxRow, minCol, maxCol);
            if (e.target != -1)
            {
                graph.AddEdge(v, e.target, e.weight);
                graph.AddEdge(e.target, v, e.weight);
            }
        }
    }

    void AddSquareAround(NativeGraph graph, int v)
    {
        for (int i = -1; i < 1; i++)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                // top and bottom
                graph.AddEdge(v + i * Dimension + j, v + (i + 1) * Dimension + j, 1);
                graph.AddEdge(v + (i + 1) * Dimension + j, v + i * Dimension + j, 1);

                // left and right
                graph.AddEdge(v + i + j * Dimension, v + i + 1 + j * Dimension, 1);
                graph.AddEdge(v + i + 1 + j * Dimension, v + i + j * Dimension, 1);
            }
        }
    }


    public void RemoveIntersectingEdges(NativeGraph graph)
    {

        var toRemove = new List<(int, int)>();
        UnsafeList<Edge> edgeList;

        for (int i = 0; i < graph.edges.Length; i++)
        {
            edgeList = graph.edges[i];
            foreach (Edge edge in edgeList)
            {
                Vector2 source = ToVector(i),
                target = ToVector(edge.target),
                orthogonal = new Vector2(-(target - source).y, (target - source).x).normalized * 0.4f;



                Vector2[] sources = new Vector2[] { source, source + orthogonal, source - orthogonal };

                foreach (var src in sources)
                {

                    RaycastHit2D hit = Physics2D.Raycast(src, target - source, (target - source).magnitude, LayerMask.GetMask("Default"));

                    if (hit.collider != null)
                    {
                        toRemove.Add((i, edge.target));
                    }

                }
            }

        }

        foreach ((int, int) edge in toRemove)
        {
            pathways.RemoveEdge(edge.Item1, edge.Item2);
        }


    }


    public void RemoveVertex(int v)
    {
        for (int i = 0; i < pathways.edges[v].Length; i++)
        {
            pathways.RemoveEdge(i, v);
            pathways.edges[v].Clear();
        }
    }


    public bool IsInPathways(Vector2 vec) {
        return pathways.edges[VectorToInt(vec, Dimension)].Length > 0;
    }


    Vector2 ToVector(int v)
    {
        return new Vector2(v / Dimension + 0.5f, v % Dimension + 0.5f);
    }




    Edge GetClosestVertex(NativeGraph graph, int v, int minRow, int maxRow, int minCol, int maxCol)
    {
        float closestDistance = Mathf.Infinity;
        float currDistance;
        int closest = -1;
        int row, col;
        bool validVertex;

        for (int i = 0; i < graph.edges.Length; i++)
        {
            row = i % Dimension;
            col = i / Dimension;
            validVertex = row >= minRow && row <= maxRow && col >= minCol && col <= maxCol;
            if (validVertex && i != v && graph.edges[i].Length > 0)
            {
                currDistance = Distance(v, i, Dimension, DistanceType.Euclidean);
                if (currDistance < closestDistance)
                {
                    closestDistance = currDistance;
                    closest = i;
                }
            }
        }

        return new Edge(closest, closestDistance);
    }



    public void RemoveOldLookupTimes(float currentTime)
    {
        List<(int, int)> toRemove = new List<(int, int)>();
        foreach (var kvp in lookupTimes)
        {
            if (currentTime - kvp.Value > lookupTimeRemoval)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            lookupTimes.Remove(key);
            paths.Remove(key);
        }
    }


    public Vector2[] ConvertToArray(IEnumerable<Edge<int>> path)
    {
        int v;
        Vector2[] result = new Vector2[path.Count() + 1];
        for (int i = 0; i < path.Count(); i++)
        {
            v = path.ElementAt(i).Source;
            result[i] = new Vector2(v / Dimension + 0.5f, v % Dimension + 0.5f);
        }

        v = path.ElementAt(path.Count() - 1).Target;
        result[path.Count()] = new Vector2(v / Dimension + 0.5f, v % Dimension + 0.5f);

        return result;
    }

    public int ToVertex(Vector2 vec)
    {
        return (int)vec.x * Dimension + (int)vec.y;
    }


    public void DrawGraph()
    {
        for (int i = 0; i < pathways.edges.Length; i++)
        {
            if (pathways.edges[i].Length > 0)

                foreach (var edge in pathways.edges[i])
                {
                    Debug.DrawLine(IntToVector3(i, Dimension), IntToVector3(edge.target, Dimension), Color.green, 1f);


                }
        }


        for (int i = 0; i < buildings.edges.Length; i++)
        {
            if (buildings.edges[i].Length > 0)

                foreach (var edge in buildings.edges[i])
                {
                    Debug.DrawLine(IntToVector3(i, Dimension), IntToVector3(edge.target, Dimension), Color.blue, 1f);
                }
        }
    }




}

public struct PathRequest
{
    public Worker requester;
    public int start;
    public int target;

    public PathRequest(Worker r, int s, int t)
    {
        requester = r;
        start = s;
        target = t;
    }
}




