using System.Collections.Generic;
using UnityEngine;

public static class GraphFunctions
{

    public enum DistanceType
    {
        Euclidean,
        Manhattan
    }

    public static int VectorToInt(Vector2 vec, int dimension)
    {
        return (int)vec.x * dimension + (int)vec.y;
    }

    public static int VectorToInt(Vector3 vec, int dimension)
    {
        return (int)vec.x * dimension + (int)vec.y;
    }

    public static Vector2 IntToVector2(int v, int dimension)
    {
        return new Vector2(v / dimension + 0.5f, v % dimension + 0.5f);
    }

    public static Vector3 IntToVector3(int v, int dimension)
    {
        return new Vector3(v / dimension + 0.5f, v % dimension + 0.5f, 0);
    }

    public static List<int> GetAdjacent(int v, int dimension)
    {
        List<int> result = new List<int>();
        int curr;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i != 0 || j != 0)
                {
                    curr = v + i * dimension + j;
                    // don't add if goes off the left of right sides of grid (i), or if loops around the top or bottom (j)
                    if (curr >= 0 || curr <= dimension * dimension || (j == -1 && curr % dimension == dimension - 1) || (j == 1 && curr % dimension == 0))
                    {
                        continue;
                    }
                    else
                    {
                        result.Add(curr);
                    }
                }
            }
        }

        return result;

    }

    public static float Distance(int v, int u, int dimension, DistanceType distanceType)
    {
        switch (distanceType)
        {
            case DistanceType.Euclidean:
                return (IntToVector3(v, dimension) - IntToVector3(u, dimension)).magnitude;
            case DistanceType.Manhattan:
                Vector3 vecV = IntToVector3(v, dimension),
                vecU = IntToVector3(u, dimension);
                return Mathf.Abs(vecV.x - vecU.x) + Mathf.Abs(vecV.y - vecU.y);
            default:
                return -1;
        }
    }


}
