using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct AStarJob : IJobParallelFor
{
    // inputs
    [ReadOnly] 
    public NativeArray<NativeGraph> graphs;
    public NativeArray<int> s, t;

    // outputs
    public NativeArray<UnsafeList<int>> paths;

    public void Execute(int i)
    {
        paths[i] = ShortestPathAStar(graphs[i], s[i], t[i]);
    }


    // returns an empty NativeList of vertices if no path is found
    // return list is in reverse order
    public UnsafeList<int> ShortestPathAStar(NativeGraph graph, int s, int t)
    {
        int n = graph.edges.Length;
        int curr;
        int next;
        float weight;
        float bestDistance;

        UnsafeList<Edge> currEdges;
        NativeArray<bool> inTree = new NativeArray<bool>(n, Allocator.Temp);
        NativeArray<float> f = new NativeArray<float>(n, Allocator.Temp);
        NativeArray<int> parent = new NativeArray<int>(n, Allocator.Temp);

        for (int i = 0; i < n; i++)
        {
            inTree[i] = false;
            f[i] = float.MaxValue;
            parent[i] = -1;
        }

        f[s] = 0;
        curr = s;



        // terminates when t is found
        while (inTree[curr] == false && curr != t)
        {
            inTree[curr] = true;
            currEdges = graph.edges[curr];

            for (int i = 0; i < graph.edges[curr].Length; i++)
            {
                next = currEdges[i].target;
                weight = currEdges[i].weight;

                if (f[next] > (f[curr] + weight + EuclideanDistance(curr, t, graph.dimension)))
                {
                    f[next] = f[curr] + weight + EuclideanDistance(curr, t, graph.dimension);
                    parent[next] = curr;
                }
            }

            curr = 0;
            bestDistance = float.MaxValue;

            for (int i = 0; i < n; i++)
            {
                if (inTree[i] == false && bestDistance > f[i])
                {
                    bestDistance = f[i];
                    curr = i;
                }
            }

        }

        UnsafeList<int> path = new UnsafeList<int>(0, Allocator.Temp);


        if (curr != t)
        {
            return path;
        }

        while (curr != s)
        {
            if (curr < 0)
            {
                return path;
            }
            path.Add(curr);
            curr = parent[curr];
        }
        path.Add(curr);

        return path;
    }

    float EuclideanDistance(int u, int v, int dimension)
    {
        int uX = u / dimension,
            uY = u % dimension,
            vX = v / dimension,
            vY = v % dimension;

        return math.sqrt(math.pow(uX - vX, 2f) + math.pow(uY - vY, 2));
    }
}

