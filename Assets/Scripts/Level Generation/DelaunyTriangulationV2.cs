using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunyTriangulationV2 : MonoBehaviour
{
    List<Node> UnsortedNodes = new List<Node>();
    List<Node> SortedNodes = new List<Node>();

    List<Tri> tris = new List<Tri>();

    List<Tri> triangulationQueue = new List<Tri>();
    public float rad = 1;
    public bool gizmos = false;
    private void Start()
    {
    }

    public void DelaunayTriangulate(List<Node> NodeSet, Vector2 min, Vector2 max) {
        UnsortedNodes = NodeSet;
        Tri EncapTri = EncapsulateDomain(min, max);

        foreach (Node node in EncapTri.GetNodes())
        {
            SortedNodes.Add(node);
        }
        tris.Add(EncapTri.Add());
        while (UnsortedNodes.Count > 0)
        {
            Tri encapsulatingTri = FindEncapsulatingTri(tris, UnsortedNodes[UnsortedNodes.Count - 1]);
            if (encapsulatingTri == null)
            {
                Debug.LogWarning("Could not find encapsulating tri for node at: " + UnsortedNodes[UnsortedNodes.Count - 1].Pos2D);
                UnsortedNodes.Remove(UnsortedNodes[UnsortedNodes.Count - 1]);
                continue;
            }
            Tri[] splitTris = SplitTri(encapsulatingTri, UnsortedNodes[UnsortedNodes.Count - 1]);

            tris.Remove(encapsulatingTri.Remove());

            SortedNodes.Add(UnsortedNodes[UnsortedNodes.Count - 1]);
            UnsortedNodes.Remove(UnsortedNodes[UnsortedNodes.Count - 1]);
            foreach (Tri tri in splitTris)
            {
                tris.Add(tri.Add());
                triangulationQueue.Add(tri);
            }
            while (triangulationQueue.Count > 0)
            {
                Tri[] triSet;
                bool isBadQuad = false;
                triSet = VerifyTri(triangulationQueue[0]);
                if (triSet == null)
                {
                    triangulationQueue.Remove(triangulationQueue[0]);
                }
                else
                {
                    isBadQuad = true;
                }

                if (isBadQuad)
                {
                    Debug.Log(triSet[0].PrintTri());
                    Debug.Log(triSet[1].PrintTri());
                    tris.Remove(triSet[0]);
                    tris.Remove(triSet[1]);
                    
                    triangulationQueue.Remove(triSet[0]);
                    triangulationQueue.Remove(triSet[1]);
                    triSet[0].Remove();
                    triSet[1].Remove();
                    tris.Add(triSet[2].Add());
                    tris.Add(triSet[3].Add());
                    triangulationQueue.Add(triSet[2]);
                    triangulationQueue.Add(triSet[3]);
                }
            }
        }
        for (int i = tris.Count - 1; i >= 0; i--)
        {
            if (tris[i].A.Pos2D == this.SortedNodes[0].Pos2D || tris[i].A.Pos2D == this.SortedNodes[1].Pos2D || tris[i].A.Pos2D == this.SortedNodes[2].Pos2D)
            {
                tris.RemoveAt(i);

            }
            else
            if (tris[i].B.Pos2D == this.SortedNodes[0].Pos2D || tris[i].B.Pos2D == this.SortedNodes[1].Pos2D || tris[i].B.Pos2D == this.SortedNodes[2].Pos2D)
            {
                tris.RemoveAt(i);

            }
            else
            if (tris[i].C.Pos2D == this.SortedNodes[0].Pos2D || tris[i].C.Pos2D == this.SortedNodes[1].Pos2D || tris[i].C.Pos2D == this.SortedNodes[2].Pos2D)
            {
                tris.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// return tri that encapsulates the given range
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    private Tri EncapsulateDomain(Vector2 min, Vector2 max) {
        float tan = Mathf.Tan(45 * Mathf.Deg2Rad);
        float width = max.x - min.x;
        float height = max.y - min.y;
        Vector2 apos = new Vector2(min.x - 2, min.y - 2);
        Vector2 bpos = new Vector2(width + width * tan, min.y);
        Vector2 cpos = new Vector2(min.x, height + height / tan);
        Node a = new Node(apos);
        Node b = new Node(bpos);
        Node c = new Node(cpos);
        return new Tri(a, b, c);
    }
    /// <summary>
    /// Returns the tri that node is in or null if outside all
    /// </summary>
    /// <param name="node">Node to find encapsulating tri</param>
    /// <returns></returns>
    private Tri FindEncapsulatingTri(List<Tri> tris,Node node) {
        for (int i = 0; i < tris.Count; i++)
        {
            if (tris[i].InTri(node))
            {
                return tris[i];
            }
        }
        return null;
    }
    /// <summary>
    /// return abd,bcd,cad
    /// </summary>
    /// <param name="tri">Tri ABC</param>
    /// <param name="node">Node D</param>
    /// <returns></returns>
    private Tri[] SplitTri(Tri tri, Node node) {
        Tri triABD = new Tri(tri.A, tri.B, node);
        Tri triBCD = new Tri(tri.B, tri.C, node);
        Tri triCAD = new Tri(tri.C, tri.A, node);

        if (tri.ABNeighbor != null)
        {
            triABD.ABNeighbor = tri.ABNeighbor;
            triABD.ABNeighborSide = tri.ABNeighborSide;
        }
        triABD.BCNeighbor = triBCD;
        triABD.BCNeighborSide = Side.CA;
        triABD.CANeighbor = triCAD;
        triABD.CANeighborSide = Side.BC;

        //BCD
        if (tri.BCNeighbor != null)
        {
            triBCD.ABNeighbor = tri.BCNeighbor;
            triBCD.ABNeighborSide = tri.BCNeighborSide;
        }
        triBCD.BCNeighbor = triCAD;
        triBCD.BCNeighborSide = Side.CA;
        triBCD.CANeighbor = triABD;
        triBCD.CANeighborSide = Side.BC;

        //CAD
        if (tri.CANeighbor != null)
        {
            triCAD.ABNeighbor = tri.CANeighbor;
            triCAD.ABNeighborSide = tri.CANeighborSide;
        }
        triCAD.BCNeighbor = triABD;
        triCAD.BCNeighborSide = Side.CA;
        triCAD.CANeighbor = triBCD;
        triCAD.CANeighborSide = Side.BC;

        return new Tri[] { triABD,triBCD,triCAD};
    }
    private Tri[] VerifyTri(Tri tri) {
        Tri[] AB,BC,CA;
        AB = TestQuad(tri, Side.AB);
        if (AB != null)
        {
            return AB;
        }
        BC = TestQuad(tri, Side.BC);
        if (BC != null)
        {
            return BC;
        }
        CA = TestQuad(tri, Side.CA);
        if (CA != null)
        {
            return CA;
        }
        return null;
    }
    /// <summary>
    /// If the quad is bad return [0] abc [1] acd [2] bcd [3] abd else return null
    /// </summary>
    /// <param name="tri">Tri to Test</param>
    /// <param name="side">Side to Test</param>
    /// <returns></returns>
    private Tri[] TestQuad(Tri tri, Side side) {
        if (!tri.HasSide(side))
        {
            return null;
        }
        Tri[] solution = new Tri[4];
        solution[0] = tri;
        solution[1] = tri.NeighborOn(side);
        if (IsQuadBad(tri.PrevNode(tri.NoneSharedNodeOn(side)),tri.NoneSharedNodeOn(side), tri.NextNode(tri.NoneSharedNodeOn(side)), tri.NeighborOn(side).NoneSharedNodeOn(tri.NeighborSideOn(side))))
        {
            solution[2] = new Tri(tri.NoneSharedNodeOn(side), tri.NextNode(tri.NoneSharedNodeOn(side)), tri.NeighborOn(side).NoneSharedNodeOn(tri.NeighborSideOn(side)));
            solution[3] = new Tri(tri.PrevNode(tri.NoneSharedNodeOn(side)), tri.NoneSharedNodeOn(side), tri.NeighborOn(side).NoneSharedNodeOn(tri.NeighborSideOn(side)));
            return solution;
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// if quad abcd is bad return true
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    /// <param name="D"></param>
    /// <returns></returns>
    private bool IsQuadBad(Node A, Node B, Node C, Node D) {
        float Xca = A.Pos2D.x - C.Pos2D.x, Xba = A.Pos2D.x - B.Pos2D.x, Xbd = D.Pos2D.x - B.Pos2D.x, Xcd = D.Pos2D.x - C.Pos2D.x;
        float Yca = A.Pos2D.y - C.Pos2D.y, Yba = A.Pos2D.y - B.Pos2D.y, Ybd = D.Pos2D.y - B.Pos2D.y, Ycd = D.Pos2D.y - C.Pos2D.y;
        float a = (Xca * Xba + Yca * Yba) * (Xbd * Ycd - Xcd * Ybd);
        float b = (Yca * Xba - Xca * Yba) * (Xbd * Xcd + Ycd * Ybd);
        if (a < b)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void OnDrawGizmos()
    {
        if (gizmos)
        {
            for (int i = 0; i < tris.Count; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(tris[i].A.Pos3D, tris[i].B.Pos3D);
                Gizmos.DrawLine(tris[i].B.Pos3D, tris[i].C.Pos3D);
                Gizmos.DrawLine(tris[i].C.Pos3D, tris[i].A.Pos3D);
            }
            for (int i = 0; i < UnsortedNodes.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(UnsortedNodes[i].Pos3D, rad);
            }
            for (int i = 0; i < SortedNodes.Count; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(SortedNodes[i].Pos3D, rad);
            }
        }
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
        
    }
    public Tri Add() {
        A.AddConnection(B);
        A.AddConnection(C);
        B.AddConnection(A);
        B.AddConnection(C);
        C.AddConnection(A);
        C.AddConnection(B);
        return this;
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
    public Node NextNode(Node node) {
        if (node == A)
        {
            return B;
        }
        else if (node == B)
        {
            return C;
        }
        else if (node == C)
        {
            return A;
        }
        else {
            Debug.LogWarning("couldnt find node " + node.Pos2D + " in tri " + A.Pos2D + " " + B.Pos2D + " " + C.Pos2D);
            return null;
        }
    }
    public Node PrevNode(Node node)
    {
        if (node == A)
        {
            return C;
        }
        else if (node == B)
        {
            return A;
        }
        else if (node == C)
        {
            return B;
        }
        else
        {
            Debug.LogWarning("couldnt find node " + node.Pos2D + " in tri " + A.Pos2D + " " + B.Pos2D + " " + C.Pos2D);
            return null;
        }
    }
    public Node[] GetNodes() {
        Node[] nodes = new Node[]{A,B,C};
        return nodes;
    }
    public Tri NeighborOn(Side side)
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
    public Side NeighborSideOn(Side side) {
        switch (side)
        {
            case Side.AB:
                return ABNeighborSide;
            case Side.BC:
                return BCNeighborSide;
            case Side.CA:
                return CANeighborSide;
            default:
                return Side.AB;
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
    public bool HasSide(Side side)
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
    public string PrintTri() {
        return "Tri with nodes " + A.Pos2D + " " + B.Pos2D + " " + C.Pos2D;
    }
}
