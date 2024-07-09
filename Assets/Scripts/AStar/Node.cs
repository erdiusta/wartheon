using UnityEngine;
using System;

public class Node : IComparable<Node>
{
    public Vector2Int gridPosition;
    public int gCost = 0;
    public int hCost = 0;
    public Node parentNode;

    public Node(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
        parentNode = null;
    }

    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        // Compare will be <0 if this instance Fcost is less than nodeToCompare.FCost
        // Compare will be >0 if this instance Fcost is greater than nodeToCompare.FCost
        // Compare will be ==0 if the values are the same

        int compare = FCost.CompareTo(nodeToCompare.FCost);

        if (compare == 0)
        {
            compare = FCost.CompareTo(nodeToCompare.hCost);
        }

        return compare;
    }
}
