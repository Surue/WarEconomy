using Unity.Collections;
using Unity.Mathematics;

namespace AI {
// Persistent node on map 
public struct PathNode {
    public float3 position;

    public float x {
        get => position.x;
        set => position.x = value;
    }
    
    public float y {
        get => position.y;
        set => position.y = value;
    }

    public float z {
        get => position.z;
        set => position.z = value;
    }
    
    public int index;
}

// Data used when computing path
public struct PathNodeCost {
    public float moveCost;
    public float heuristicCost;
    public float totalCost;

    public int cameFromIndex;

    public void CalculateTotalCost() {
        totalCost = moveCost + heuristicCost;
    }
}

public struct PathNodeLink {
    public int otherIndex;
    public float distance;
}
}