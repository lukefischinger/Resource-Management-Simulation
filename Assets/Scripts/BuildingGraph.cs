using UnityEngine;
using QuikGraph;
using QuikGraph.Algorithms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using QuikGraph.Algorithms.ShortestPath;
using Unity.VisualScripting;
using QuikGraph.Algorithms.Observers;

public class BuildingGraph
{

    AdjacencyGraph<int, Edge<int>> buildings = new AdjacencyGraph<int, Edge<int>>();

    private int count = 0;
    private int dimension;
    public int Dimension
    {
        get
        {
            return dimension;
        }
        private set
        {
            dimension = value;
            max = dimension * dimension;
        }
    }

    private int max;

    public BuildingGraph(int dimension)
    {
        Dimension = dimension;

        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                int curr = i * dimension + j;
                buildings.AddVertex(curr);

                AddAllAdjacentEdges(curr);
            }
        }
    }

    void TryAddEdge(Edge<int> edge)
    {
        if (buildings.ContainsVertex(edge.Source) && buildings.ContainsVertex(edge.Target))
        {
            buildings.AddEdge(edge);
        }
    }

    void AddAllAdjacentEdges(int v)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
                if (i != j)
                {
                    TryAddEdge(new Edge<int>(v, v + i * Dimension + j));
                    TryAddEdge(new Edge<int>(v + i * Dimension + j, v));

                }
        }
    }


    void AddVertexAndEdges(int v)
    {
        buildings.AddVertex(v);
        AddAllAdjacentEdges(v);
    }

    public void RemoveVertex(int v)
    {
        buildings.RemoveVertex(v);
    }

    public bool Contains(Vector2 vec)
    {
        return buildings.ContainsVertex(ToVertex(vec));
    }

    public IEnumerable<Edge<int>> CalculatePath(Vector2 start, Vector2 end)
    {
        count = 0;

        int s = (int)start.x * Dimension + (int)start.y,
            e = (int)end.x * Dimension + (int)end.y;

        AddVertexAndEdges(e);

        var AStar = new AStarShortestPathAlgorithm<int, Edge<int>>(buildings, x => 1, x => Euclidean(x, e));
        var AStarPath = new VertexPredecessorRecorderObserver<int, Edge<int>>();
        AStarPath.Attach(AStar);
        AStar.SetRootVertex(s);
        AStar.FinishVertex += (x => { if (x == e) AStar.Abort(); });
        AStar.Compute();

        AStarPath.TryGetPath(e, out IEnumerable<Edge<int>> result);


        buildings.RemoveVertex(e);

        return result;
    }


    double Manhattan(int start, int end)
    {
        return Mathf.Abs((end / Dimension) - (start / Dimension)) + Mathf.Abs((end % Dimension) - (start % Dimension));
    }

    double Euclidean(int start, int end) {
        return Mathf.Sqrt(Mathf.Pow((end / Dimension) - (start / Dimension), 2) + Mathf.Pow((end % Dimension) - (start % Dimension), 2));
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
}




