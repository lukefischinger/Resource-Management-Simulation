using UnityEngine;
using QuikGraph;
using System.Collections.Generic;
using System.Linq;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Algorithms.Observers;

public class BuildingGraph
{

    public AdjacencyGraph<int, Edge<int>> walkways { get; private set; } = new AdjacencyGraph<int, Edge<int>>();
    Dictionary<(int, int), IEnumerable<Edge<int>>> paths = new Dictionary<(int, int), IEnumerable<Edge<int>>>();
    Dictionary<(int, int), float> lookupTimes = new Dictionary<(int, int), float>();

    const float lookupTimeRemoval = 60f;



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
    }

    void TryAddEdge(Edge<int> edge)
    {
        if (walkways.ContainsVertex(edge.Source) && walkways.ContainsVertex(edge.Target))
        {
            walkways.AddEdge(edge);
        }
    }

    void AddAllAdjacentEdges(int v)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
                if (Mathf.Abs(i) != Mathf.Abs(j))
                {
                    TryAddEdge(new Edge<int>(v, v + i * Dimension + j));
                    TryAddEdge(new Edge<int>(v + i * Dimension + j, v));
                }
        }
    }

    void AddClosestEdges(int v)
    {
        for (int i = -1; i <= 1; i += 2)
        {
            int v2 = FindFirstVertexUpDown(v, i);
            if (v2 != -1)
            {
                walkways.AddEdge(new Edge<int>(v, v2));
                walkways.AddEdge(new Edge<int>(v2, v));
            }

            v2 = FindFirstVertexLeftRight(v, i);
            if (v2 != -1)
            {
                walkways.AddEdge(new Edge<int>(v, v2));
                walkways.AddEdge(new Edge<int>(v2, v));
            }
        }
    }

    public void AddAllAdjacentVertices(Vector2 vec, bool addOtherNearest)
    {
        int v = (int)vec.x * Dimension + (int)vec.y;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (j != 0 || i != 0)
                {
                    int curr = v + i * Dimension + j;
                    AddVertexAndEdges(curr);

                    if (addOtherNearest)
                    {
                        if (j != 0 && i != 0)
                        {
                            int v2 = FindFirstVertexUpDown(curr, j);
                            if (v2 != -1)
                            {
                                walkways.AddEdge(new Edge<int>(curr, v2));
                                walkways.AddEdge(new Edge<int>(v2, curr));
                            }
                            v2 = FindFirstVertexLeftRight(curr, i);
                            if (v2 != -1)
                            {
                                walkways.AddEdge(new Edge<int>(curr, v2));
                                walkways.AddEdge(new Edge<int>(v2, curr));
                            }
                        }
                    }
                }
            }
        }
    }



    public void RemoveIntersectingEdges()
    {

        var toRemove = new List<Edge<int>>();

        foreach (Edge<int> edge in walkways.Edges)
        {

            Vector2 source = ToVector(edge.Source),
            target = ToVector(edge.Target),
            orthogonal = new Vector2(-(target - source).y, (target - source).x).normalized * 0.4f;



            Vector2[] sources = new Vector2[] { source, source + orthogonal, source - orthogonal };

            foreach (var src in sources)
            {

                RaycastHit2D hit = Physics2D.Raycast(src, target - source, (source - target).magnitude, LayerMask.GetMask("Default"));

                if (hit.collider != null)
                {
                    toRemove.Add(edge);
                }

            }
            Debug.DrawLine(source, target, Color.blue, 3f);
        }

        foreach (Edge<int> edge in toRemove)
        {
            walkways.RemoveEdge(edge);
        }


    }




    public void AddVertexAndEdges(int v)
    {
        walkways.AddVertex(v);
        AddAllAdjacentEdges(v);

    }

    public void RemoveVertex(int v)
    {
        walkways.RemoveVertex(v);
    }

    public bool Contains(Vector2 vec)
    {
        return walkways.ContainsVertex(ToVertex(vec));
    }

    int FindFirstVertexLeftRight(int v, int dir = -1)
    {
        List<int> toSearch = new List<int> { v + dimension * dir };

        while (toSearch.Count > 0)
        {
            int curr = toSearch[0];

            if (walkways.ContainsVertex(curr))
            {
                return curr;
            }
            else
            {
                toSearch.RemoveAt(0);

                TryAdd(toSearch, curr + dimension * dir);
                if (curr % Dimension < Dimension - 1 && curr % Dimension >= v % Dimension)
                {
                    TryAdd(toSearch, curr + 1);
                }
                if (curr % Dimension > 0 && curr % Dimension <= v % Dimension)
                {
                    TryAdd(toSearch, curr - 1);
                }
            }


        }

        return -1;
    }


    Vector2 ToVector(int v)
    {
        return new Vector2(v / Dimension + 0.5f, v % Dimension + 0.5f);
    }

    int FindFirstVertexUpDown(int v, int dir = -1)
    {
        List<int> toSearch = new List<int>();

        if ((dir < 0 && v % Dimension > 0) || (dir > 0 && v % Dimension < Dimension - 1))
        {
            toSearch.Add(v + dir);
        }

        while (toSearch.Count > 0)
        {
            int curr = toSearch[0];

            if (walkways.ContainsVertex(curr))
            {
                return curr;
            }
            else
            {
                toSearch.RemoveAt(0);
                if ((dir < 0 && curr % Dimension > 0) || (dir > 0 && curr % Dimension < Dimension - 1))
                {
                    TryAdd(toSearch, curr + dir);
                }

                if (curr / Dimension >= v / Dimension)
                {
                    TryAdd(toSearch, curr + dimension);
                }
                if (curr / Dimension <= v / Dimension)
                {
                    TryAdd(toSearch, curr - dimension);
                }
            }

        }

        return -1;
    }

    void TryAdd(List<int> list, int vert)
    {
        if (vert >= 0 && vert % dimension < dimension && vert / dimension < dimension && !list.Contains(vert))
        {
            list.Add(vert);
        }
    }

    public IEnumerable<Edge<int>> CalculatePath(Vector2 start, Vector2 end)
    {

        int s = (int)start.x * Dimension + (int)start.y,
            e = (int)end.x * Dimension + (int)end.y;

        if (paths.ContainsKey((s, e)))
        {
            lookupTimes[(s, e)] = Time.time;
            return paths[(s, e)];
        }
        else if (paths.ContainsKey((e, s)))
        {
            lookupTimes[(e, s)] = Time.time;
            return paths[(e, s)].Reverse();
        }

        bool startAdded = false;
        if (!walkways.ContainsVertex(s))
        {
            walkways.AddVertex(s);
            AddClosestEdges(s);
            startAdded = true;
        }

        AddVertexAndEdges(e);

        var AStar = new AStarShortestPathAlgorithm<int, Edge<int>>(walkways, EdgeWeight, x => Euclidean(x, e));
        var AStarPath = new VertexPredecessorRecorderObserver<int, Edge<int>>();
        AStarPath.Attach(AStar);
        AStar.SetRootVertex(s);
        AStar.StartVertex += (x => { if (x == e) AStar.Abort(); });
        AStar.Compute();

        AStarPath.TryGetPath(e, out IEnumerable<Edge<int>> result);


        walkways.RemoveVertex(e);
        if (startAdded)
        {
            walkways.RemoveVertex(s);
        }


        paths.Add((s, e), result);
        lookupTimes[(s, e)] = Time.time;

        return result;
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

    double EdgeWeight(Edge<int> edge)
    {
        return (new Vector2(edge.Source / dimension, edge.Source % dimension) - new Vector2(edge.Target / dimension, edge.Target % dimension)).magnitude;
    }

    double Manhattan(int start, int end)
    {
        return Mathf.Abs((end / Dimension) - (start / Dimension)) + Mathf.Abs((end % Dimension) - (start % Dimension));
    }

    double Euclidean(int start, int end)
    {
        return Mathf.Pow((end / Dimension) - (start / Dimension), 2) + Mathf.Pow((end % Dimension) - (start % Dimension), 2);
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




