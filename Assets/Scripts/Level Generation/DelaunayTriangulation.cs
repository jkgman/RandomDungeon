using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour
{
    List<Tri> tris = new List<Tri>();

    List<Node> domainNodes = new List<Node>();
    List<Node> triangulatedNodes = new List<Node>();

    List<Tri> triangulate = new List<Tri>();

    Node[] startingNodes = new Node[3];

    public bool gizmos= false;

    private void Start()
    {

    }
    
    public void GenerateGraph(List<Node> startingNodes, float minX, float maxX, float minY, float maxY) {
        domainNodes = startingNodes;
        EncapsulateRectangle(minX, maxX, minY, maxY);
        for (int i = domainNodes.Count - 1; i >= 0; i--)
        {
            triangulatedNodes.Add(AddNode(domainNodes[i]));
            domainNodes.RemoveAt(i);
            CheckTriangulation();
        }
    }
    

    void EncapsulateRectangle(float minX, float maxX, float minY, float maxY)
    {
        float tan = Mathf.Tan(45 * Mathf.Deg2Rad);
        float width = maxX - minX;
        float height = maxY - minY;
        Vector2 apos = new Vector2(minX - 2, minY - 2);
        Vector2 bpos = new Vector2(width + width * tan, minY);
        Vector2 cpos = new Vector2(minX, height + height / tan);
        Node a = new Node(apos);
        Node b = new Node(bpos);
        Node c = new Node(cpos);
        triangulatedNodes.Add(a);
        triangulatedNodes.Add(b);
        triangulatedNodes.Add(c);
        tris.Add(new Tri(a, b, c));
    }


    Node AddNode(Node node) {
        for (int i = 0; i < tris.Count; i++)
        {
            if (tris[i].InTri(node))
            {
                List<Tri> newTris = SplitTri(node, tris[i]);
                tris.Remove(tris[i].Remove());
                foreach (Tri tri in newTris)
                {
                    tris.Add(tri);
                    triangulate.Add(tri);
                }
                return node;
                
            }
        }
        Debug.Log("Couldn't add node");
        return null;
    }

    /// <summary>
    /// Return List of tris ABD, BCD, CAD
    /// </summary>
    /// <param name="point">Node D</param>
    /// <param name="currentTri">Tri ABC</param>
    List<Tri> SplitTri(Node point, Tri currentTri)
    {
        Tri triABD = new Tri(currentTri.A, currentTri.B, point);
        Tri triBCD = new Tri(currentTri.B, currentTri.C, point);
        Tri triCAD = new Tri(currentTri.C, currentTri.A, point);

        //ABD
        if (currentTri.ABNeighbor != null)
        {
            triABD.ABNeighbor = currentTri.ABNeighbor;
            if (currentTri.B == currentTri.ABNeighbor.A)
            {
                triABD.ABNeighborSide = Side.AB;
            }
            else if (currentTri.B == currentTri.ABNeighbor.B)
            {
                triABD.ABNeighborSide = Side.BC;
            }
            else if (currentTri.B == currentTri.ABNeighbor.C)
            {
                triABD.ABNeighborSide = Side.CA;
            }
        }
        triABD.BCNeighbor = triBCD;
        triABD.BCNeighborSide = Side.CA;
        triABD.CANeighbor = triCAD;
        triABD.CANeighborSide = Side.BC;

        //BCD
        if (currentTri.BCNeighbor != null)
        {
            triBCD.ABNeighbor = currentTri.BCNeighbor;
            if (currentTri.C == currentTri.ABNeighbor.A)
            {
                triBCD.ABNeighborSide = Side.AB;
            }
            else if (currentTri.C == currentTri.ABNeighbor.B)
            {
                triBCD.ABNeighborSide = Side.BC;
            }
            else if (currentTri.C == currentTri.ABNeighbor.C)
            {
                triBCD.ABNeighborSide = Side.CA;
            }
        }
        triBCD.BCNeighbor = triCAD;
        triBCD.BCNeighborSide = Side.CA;
        triBCD.CANeighbor = triABD;
        triBCD.CANeighborSide = Side.BC;

        //CAD
        if (currentTri.CANeighbor != null)
        {
            triCAD.ABNeighbor = currentTri.CANeighbor;
            if (currentTri.A == currentTri.ABNeighbor.A)
            {
                triCAD.ABNeighborSide = Side.AB;
            }
            else if (currentTri.A == currentTri.ABNeighbor.B)
            {
                triCAD.ABNeighborSide = Side.BC;
            }
            else if (currentTri.A == currentTri.ABNeighbor.C)
            {
                triCAD.ABNeighborSide = Side.CA;
            }
        }
        triCAD.BCNeighbor = triABD;
        triCAD.BCNeighborSide = Side.CA;
        triCAD.CANeighbor = triBCD;
        triCAD.CANeighborSide = Side.BC;

        return new List<Tri> { triABD , triBCD , triCAD };
    }

    void CheckTriangulation() {
        while (triangulate.Count > 0) {
            if (Triangulate(triangulate[triangulate.Count - 1], Side.AB))
            {

                //remove tri a and b from tris and triangulation
                //add flipped quads to tri and triangulation
            }
            else if (Triangulate(triangulate[triangulate.Count - 1], Side.BC))
            {

            }
            else if (Triangulate(triangulate[triangulate.Count - 1], Side.CA))
            {

            }
            else
            {
                //remove tri from triangulate
            }
        }
    }

    /// <summary>
    /// returns true if the quad is more optimal flipped
    /// </summary>
    /// <param name="tri"></param>
    /// <param name="side"></param>
    /// <returns></returns>
    bool Triangulate(Tri tri, Side side) {
        //if (!tri.HasSide(side))
        //{
        //    return false;
        //}
        //Node A = tri.A, B = tri.A, C = tri.A, D = tri.A;
        //switch (side)
        //{
        //    case Side.AB:
        //        A = tri.B;
        //        B = tri.C;
        //        C = tri.A;
        //        D = tri.ABNeighbor.GetOppositeNode(tri.ABNeighborSide);
        //        break;
        //    case Side.BC:
        //        A = tri.C;
        //        B = tri.A;
        //        C = tri.B;
        //        D = tri.BCNeighbor.GetOppositeNode(tri.BCNeighborSide);
        //        break;
        //    case Side.CA:
        //        A = tri.A;
        //        B = tri.B;
        //        C = tri.C;
        //        D = tri.CANeighbor.GetOppositeNode(tri.CANeighborSide);
        //        break;
        //    default:
        //        break;
        //}
        //float Xca = A.Pos2D.x - C.Pos2D.x, Xba = A.Pos2D.x - B.Pos2D.x, Xbd = D.Pos2D.x - B.Pos2D.x, Xcd = D.Pos2D.x - C.Pos2D.x;
        //float Yca = A.Pos2D.y - C.Pos2D.y, Yba = A.Pos2D.y - B.Pos2D.y, Ycd = D.Pos2D.y - C.Pos2D.y, Ybd = D.Pos2D.y - B.Pos2D.y;
        //float a = (Xca * Xba + Yca * Yba) * (Xbd * Ycd - Xcd * Ybd);
        //float b = (Yca * Xba - Xca * Yba) * (Xbd * Xcd + Ycd * Ybd);
        if(true)//if (a < b)
        {
            return false;
        }
        else
        {
            //triangulate.Remove(tri);
            //triangulate.Remove(tri.GetSideNeighbor(side));
            //tris.Remove(tri);
            //tris.Remove(tri.GetSideNeighbor(side));
            //Tri newA = new Tri(B,C,D);
            //Tri newB = new Tri(A,B,D);

            //tris.Add(newA);
            //tris.Add(newB);
            //triangulate.Add(newA);
            //triangulate.Add(newB);
            return true;
        }
        
    }

    private void OnDrawGizmos() {
        if (gizmos)
        {
            for (int i = 0; i < tris.Count; i++)
            {
                Gizmos.DrawLine(tris[i].A.Pos3D, tris[i].B.Pos3D);
                Gizmos.DrawLine(tris[i].B.Pos3D, tris[i].C.Pos3D);
                Gizmos.DrawLine(tris[i].C.Pos3D, tris[i].A.Pos3D);
            }
        }

    }
}
//public class Node {

//    List<Node> connectedNodes = new List<Node>();
//    Vector2 pos;

//    public override bool Equals(object obj)
//    {

//        if (obj == null || GetType() != obj.GetType())
//        {
//            return false;
//        }

//        if (pos == ((Node)obj).pos)
//        {
//            return true;
//        }
//        return false;
//    }
//    public override int GetHashCode()
//    {
//        // TODO: write your implementation of GetHashCode() here
//        throw new System.NotImplementedException();
//        return base.GetHashCode();
//    }

//    public Node(Vector2 pos)
//    {
//        this.pos = pos;
//    }

//    public Vector2 Pos2D { get => pos; }
//    public Vector3 Pos3D { get => new Vector3(pos.x,0,pos.y); }
//    public List<Node> GetConnections { get => connectedNodes; private set => connectedNodes = value; }

//    public void AddConnection(Node connection) {
//        connectedNodes.Add(connection);
//    }
//    public void RemoveConnection(Node connection) {
//        connectedNodes.Remove(connection);
//    }
//}
//public enum Side {
//    AB,BC,CA
//}
//public class Tri
//{
//    Node a;
//    Node b;
//    Node c;
//    public Tri ABNeighbor;
//    public Side ABNeighborSide;
//    public Tri BCNeighbor;
//    public Side BCNeighborSide;
//    public Tri CANeighbor;
//    public Side CANeighborSide;

//    public override bool Equals(object obj)
//    {

//        if (obj == null || GetType() != obj.GetType())
//        {
//            return false;
//        }

//        if (A == ((Tri)obj).A && B == ((Tri)obj).B && C == ((Tri)obj).C)
//        {
//            return true;
//        }
//        if (A == ((Tri)obj).C && B == ((Tri)obj).A && C == ((Tri)obj).B)
//        {
//            return true;
//        }
//        if (A == ((Tri)obj).B && B == ((Tri)obj).C && C == ((Tri)obj).A)
//        {
//            return true;
//        }
//        return false;
//    }
//    public override int GetHashCode()
//    {
//        // TODO: write your implementation of GetHashCode() here
//        throw new System.NotImplementedException();
//        return base.GetHashCode();
//    }

//    public Tri(Node a, Node b, Node c) {
//        A = a;
//        B = b;
//        C = c;
//        A.AddConnection(b);
//        A.AddConnection(c);
//        B.AddConnection(a);
//        B.AddConnection(c);
//        C.AddConnection(a);
//        C.AddConnection(b);
//    }
//    public Tri Remove() {
//        A.RemoveConnection(B);
//        A.RemoveConnection(C);
//        B.RemoveConnection(A);
//        B.RemoveConnection(C);
//        C.RemoveConnection(A);
//        C.RemoveConnection(B);
//        return this;
//    }


//    public Node A { get => a; private set => a = value; }
//    public Node B { get => b; private set => b = value; }
//    public Node C { get => c; private set => c = value; }
//    public Tri GetSideNeighbor(Side side) {
//        switch (side)
//        {
//            case Side.AB:
//                return ABNeighbor;
//            case Side.BC:
//                return BCNeighbor;
//            case Side.CA:
//                return CANeighbor;
//            default:
//                return null;
//        }
//    }
//    public Node GetOppositeNode(Side side) {
//        switch (side)
//        {
//            case Side.AB:
//                return C;
//            case Side.BC:
//                return A;
//            case Side.CA:
//                return B;
//            default:
//                return null;
//        }
//    }
//    public bool IsSide(Side side) {
//        switch (side)
//        {
//            case Side.AB:
//                if (ABNeighbor == null)
//                {
//                    return false;
//                }
//                break;
//            case Side.BC:
//                if (BCNeighbor == null)
//                {
//                    return false;
//                }
//                break;
//            case Side.CA:
//                if (CANeighbor == null)
//                {
//                    return false;
//                }
//                break;
//            default:
//                break;
//        }
//        return true;
//    }

//    /// <summary>
//    /// Returns false if node is not positioned inside tri
//    /// </summary>
//    /// <param name="node"></param>
//    /// <returns></returns>
//    public bool InTri(Node node)
//    {
//        Vector3 vertA = A.Pos3D, vertB = B.Pos3D, vertC = C.Pos3D;
//        Vector3 point = node.Pos3D;

//        Vector3 sideA = vertB - vertA;
//        float certaintyA = Vector3.Cross(point - vertA, sideA).y;
//        if (certaintyA < 0)
//        {
//            return false;
//        }

//        Vector3 sideB = vertC - vertB;
//        float certaintyB = Vector3.Cross(point - vertB, sideB).y;
//        if (certaintyB < 0)
//        {
//            return false;
//        }

//        Vector3 sideC = vertA - vertC;
//        float certaintyC = Vector3.Cross(point - vertC, sideC).y;
//        if (certaintyC < 0)
//        {
//            return false;
//        }

//        return true;
//    }

//}
