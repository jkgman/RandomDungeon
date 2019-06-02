using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunyTriangulationV2 : MonoBehaviour
{
    List<Node> UnsortedNodes = new List<Node>();
    List<Node> SortedNodes = new List<Node>();

    List<Tri> tris = new List<Tri>();

    List<Tri> triangulationQueue = new List<Tri>();

    public bool gizmos = false;
    public void DelaunayTriangulate(List<Node> NodeSet, Vector2 min, Vector2 max) {
        //UnsortedNodes = NodeSet
        //Tri EncapTri = EncapsulateDomain(min,max)
        //SortedNodes.add(each EncapTri.Node)
        //tris.add(EncapTri)
        while (UnsortedNodes.Count > 0)
        {
            //find tri containing node
            //find 3 resulting nodes from split tri
            //remove old tri and node connections
            //add new tris and connections
            //
        }
    }

    private Tri EncapsulateDomain(Vector2 min, Vector2 max) {
        return null;
    }

}
public enum Side
{
    AB, BC, CA
}

public class Node
{

    List<Node> connectedNodes = new List<Node>();
    Vector2 pos;

    public Node(Vector2 pos)
    {
        this.pos = pos;
    }
    public override bool Equals(object obj)
    {

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (pos == ((Node)obj).pos)
        {
            return true;
        }
        return false;
    }
    public override int GetHashCode()
    {
        // TODO: write your implementation of GetHashCode() here
        throw new System.NotImplementedException();
        return base.GetHashCode();
    }

    

    public Vector2 Pos2D { get => pos; }
    public Vector3 Pos3D { get => new Vector3(pos.x, 0, pos.y); }
    public List<Node> GetConnections { get => connectedNodes; private set => connectedNodes = value; }
    public void AddConnection(Node connection)
    {
        connectedNodes.Add(connection);
    }
    public void RemoveConnection(Node connection)
    {
        connectedNodes.Remove(connection);
    }
}

public class Tri
{
    Node a;
    Node b;
    Node c;
    public Tri ABNeighbor;
    public Side ABNeighborSide;
    public Tri BCNeighbor;
    public Side BCNeighborSide;
    public Tri CANeighbor;
    public Side CANeighborSide;

    public override bool Equals(object obj)
    {

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (A == ((Tri)obj).A && B == ((Tri)obj).B && C == ((Tri)obj).C)
        {
            return true;
        }
        if (A == ((Tri)obj).C && B == ((Tri)obj).A && C == ((Tri)obj).B)
        {
            return true;
        }
        if (A == ((Tri)obj).B && B == ((Tri)obj).C && C == ((Tri)obj).A)
        {
            return true;
        }
        return false;
    }
    public override int GetHashCode()
    {
        // TODO: write your implementation of GetHashCode() here
        throw new System.NotImplementedException();
        return base.GetHashCode();
    }

    public Tri(Node a, Node b, Node c)
    {
        A = a;
        B = b;
        C = c;
        A.AddConnection(b);
        A.AddConnection(c);
        B.AddConnection(a);
        B.AddConnection(c);
        C.AddConnection(a);
        C.AddConnection(b);
    }
    public Tri Remove()
    {
        A.RemoveConnection(B);
        A.RemoveConnection(C);
        B.RemoveConnection(A);
        B.RemoveConnection(C);
        C.RemoveConnection(A);
        C.RemoveConnection(B);
        return this;
    }


    public Node A { get => a; private set => a = value; }
    public Node B { get => b; private set => b = value; }
    public Node C { get => c; private set => c = value; }
    public Tri TriOn(Side side)
    {
        switch (side)
        {
            case Side.AB:
                return ABNeighbor;
            case Side.BC:
                return BCNeighbor;
            case Side.CA:
                return CANeighbor;
            default:
                return null;
        }
    }
    public Node NoneSharedNodeOn(Side side)
    {
        switch (side)
        {
            case Side.AB:
                return C;
            case Side.BC:
                return A;
            case Side.CA:
                return B;
            default:
                return null;
        }
    }
    public bool IsSide(Side side)
    {
        switch (side)
        {
            case Side.AB:
                if (ABNeighbor == null)
                {
                    return false;
                }
                break;
            case Side.BC:
                if (BCNeighbor == null)
                {
                    return false;
                }
                break;
            case Side.CA:
                if (CANeighbor == null)
                {
                    return false;
                }
                break;
            default:
                break;
        }
        return true;
    }
    public bool InTri(Node node)
    {
        Vector3 vertA = A.Pos3D, vertB = B.Pos3D, vertC = C.Pos3D;
        Vector3 point = node.Pos3D;

        Vector3 sideA = vertB - vertA;
        float certaintyA = Vector3.Cross(point - vertA, sideA).y;
        if (certaintyA < 0)
        {
            return false;
        }

        Vector3 sideB = vertC - vertB;
        float certaintyB = Vector3.Cross(point - vertB, sideB).y;
        if (certaintyB < 0)
        {
            return false;
        }

        Vector3 sideC = vertA - vertC;
        float certaintyC = Vector3.Cross(point - vertC, sideC).y;
        if (certaintyC < 0)
        {
            return false;
        }

        return true;
    }

}
