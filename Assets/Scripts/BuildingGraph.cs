using QuikGraph;
using UnityEngine;
using static GraphFunctions;

public class BuildingGraph
{
    public int Dimension { get; private set; }

    public AdjacencyGraph<int, Edge<int>> Graph { get; private set; } = new AdjacencyGraph<int, Edge<int>>();

    public BuildingGraph(int dimension)
    {
        Dimension = dimension;
    }

    void AddNode(Vector2 position)
    {
        Graph.AddVertex(VectorToInt(position, Dimension));
    }


    public void AddNodeAndAdjacentEdges(Vector2 position) {
        AddNode(position);
        AddAdjacentEdges(VectorToInt(position, Dimension));
    }

    void AddAdjacentEdges(int v)
    {
        foreach (var u in GetAdjacent(v, Dimension))
        {
            if(Graph.ContainsVertex(u) && Graph.ContainsVertex(v)) {
                Graph.AddEdge(new Edge<int>(v, u));
                Graph.AddEdge(new Edge<int>(u, v));
            }
        }
    }

}
