# Resource-Management-Simulation
This is a simple resource management/supply chain simulation, with workers that transfer resources between source nodes, processing nodes, and storage nodes. Workers receive assignments to withdraw/deposit resources from a particular node, find the closest available node to deposit/withdraw resources to complete their assignment, and calculate the quickest path between these two nodes using A* pathfinding.
When assignments are completed, workers join an idle workers queue, from which they are given new assignments, which are also queued according to their priority level.

The graph (i.e., nodes and edges) representation of the system of resource nodes and the A* pathfinding algorithm were custom implementations using [UnsafeLists](https://docs.unity3d.com/Packages/com.unity.collections@0.4/api/Unity.Collections.LowLevel.Unsafe.UnsafeList.html) (an unmanaged C# type for use in Unity), in order to take advantage of Unity's highly performant Job system and Burst Compiler. See 
- [NativeGraphs.cs](https://github.com/lukefischinger/Resource-Management-Simulation/blob/master/Assets/Scripts/NativeGraph.cs) for custom Graph data structure implementation, and
- [AStarJob.cs](https://github.com/lukefischinger/Resource-Management-Simulation/blob/master/Assets/Scripts/AStarJob.cs) for A* algorithm implementation using IJobParallelFor.

## Built With
- Unity and C#
- Job system and unmanaged data types (UnsafeLists)

## Author
Luke Fischinger

## GNU General Public License v3.0

See [License](https://github.com/lukefischinger/Resource-Management-Simulation/blob/master/License) for full text.
