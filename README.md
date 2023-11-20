# Resource-Management-Simulation
## Overview
This is a simple resource management/supply chain simulation, with workers that transfer resources between source nodes, processing nodes, and storage nodes. Workers receive assignments to withdraw/deposit resources from a particular node, find the closest available node to deposit/withdraw resources to complete their assignment, and calculate the quickest path between these two nodes using A* pathfinding.

When assignments are completed, workers join an idle workers queue, from which they are given new assignments, which are also queued according to their priority level.

The graph (i.e., nodes and edges) representation of the system of resource nodes and the A* pathfinding algorithm were custom implementations using [UnsafeLists](https://docs.unity3d.com/Packages/com.unity.collections@0.4/api/Unity.Collections.LowLevel.Unsafe.UnsafeList.html) (an unmanaged C# type for use in Unity), in order to take advantage of Unity's highly performant Job system and Burst Compiler. See 
- [NativeGraphs.cs](https://github.com/lukefischinger/Resource-Management-Simulation/blob/master/Assets/Scripts/NativeGraph.cs) for custom Graph data structure implementation,
- [AStarJob.cs](https://github.com/lukefischinger/Resource-Management-Simulation/blob/master/Assets/Scripts/AStarJob.cs) for A* algorithm implementation using IJobParallelFor, and
- [PathwayGraph.cs](https://github.com/lukefischinger/Resource-Management-Simulation/blob/master/Assets/Scripts/PathwayGraph.cs) for execution of a batch of AStarJob calls.

## Built With
- Unity and C#
- Job system and unmanaged data types (UnsafeLists)

## Gifs 
(visuals are a work in progress)

### 200 workers finding their assignments (red lines are paths calculated with A*)
![Resource Management Gif](https://github.com/lukefischinger/Resource-Management-Simulation/assets/107618359/7d4f0ae4-432f-4b98-b441-b47c5a6e4494)

### Workers recalculating paths when an obstacle (the white box) is placed in their way
![Resource Management Obstacle](https://github.com/lukefischinger/Resource-Management-Simulation/assets/107618359/558745c1-1b82-4976-907e-f901ca4f51ba)

## Author
Luke Fischinger

## GNU General Public License v3.0

See [License](https://github.com/lukefischinger/Resource-Management-Simulation/blob/master/License) for full text.
