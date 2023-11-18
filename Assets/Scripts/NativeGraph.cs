using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct Edge
{
    public int target;
    public float weight;

    public Edge(int target, float weight)
    {
        this.target = target;
        this.weight = weight;
    }
}

public struct NativeGraph
{
    public NativeArray<UnsafeList<Edge>> edges;
    public int dimension;

    public NativeGraph(List<Edge>[] edges, int dimension)
    {
        this.dimension = dimension;

        this.edges = new NativeArray<UnsafeList<Edge>>(edges.Length, Allocator.Persistent);
        for (int i = 0; i < edges.Length; i++)
        {
            if (edges[i] == null) continue;

            var list = new UnsafeList<Edge>(edges[i].Count, Allocator.Persistent);


            for (int j = 0; j < edges[i].Count; j++)
            {
                list.Add(edges[i][j]);
            }

            this.edges[i] = list;

        }
    }

    public NativeGraph(int dimension)
    {
        edges = new NativeArray<UnsafeList<Edge>>(dimension * dimension, Allocator.Persistent);
        for (int i = 0; i < edges.Length; i++)
        {
            var list = new UnsafeList<Edge>(1, Allocator.Persistent);
            edges[i] = list;

        }
        this.dimension = dimension;
    }


    public NativeGraph(NativeGraph graph) {
        dimension = graph.dimension;
        edges = new NativeArray<UnsafeList<Edge>>(graph.edges.Length, Allocator.Persistent);
        for(int i = 0; i < graph.edges.Length; i++) {
            var list = new UnsafeList<Edge>(graph.edges[i].Length, Allocator.Persistent);
            
            for(int j = 0; j < graph.edges[i].Length; j++) {
                list.Add(graph.edges[i][j]);
            }
            edges[i] = list;
        }
    }


    // adds a directed weighted edge from v to u
    public void AddEdge(int v, int u, float weight)
    {
        if (v >= 0 && v < edges.Length && u >= 0 && u < edges.Length)
        {
            var list = edges[v];
            if (list.Equals(null))
            {
                list = new UnsafeList<Edge>(1, Allocator.Persistent);
            }

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].target == u)
                {
                    return;
                }
            }

            list.Add(new Edge(u, weight));
            edges[v] = list;
        }
    }

    public void RemoveEdge(int v, int u)
    {
        for (int i = 0; i < edges[v].Length; i++)
        {
            UnsafeList<Edge> list = edges[v];
            if (edges[v][i].target == u)
            {
                list.RemoveAt(i);
                edges[v] = list;
                return;
            }
        }
    }

    public void Dispose()
    {
        foreach (var edgeList in edges)
        {
            if (edgeList.IsCreated)
            {
                edgeList.Dispose();
            }
        }
        if (edges.IsCreated)
        {
            edges.Dispose();
        }
    }
}











