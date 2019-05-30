using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour
{
    List<Tri> tris = new List<Tri>();
    List<Tri> triangulate = new List<Tri>();
    List<Node> nodes = new List<Node>();
    Node[] startingNodes = new Node[3];
    public bool gizmos= false;


    private void Start()
    {
        //nodes.Add(new Node(Vector2.zero));
        //nodes.Add(new Node(Vector2.right));
        //nodes.Add(new Node(Vector2.up));
        //tris.Add(new Tri(nodes[0], nodes[1], nodes[2]));
    }
    
    public void GenerateGraph(List<Node> startingNodes, float minX, float maxX, float minY, float maxY) {
        nodes = startingNodes;
        EncapsulateRectangle(minX, maxX, minY, maxY);
        //for all nodes minus the 3 added starting nodes
        for (int i = 0; i < nodes.Count-3; i++)
        {
            //for existing tris
            for (int j = 0; j < tris.Count; j++)
            {
                if (tris[j].InTri(nodes[i]))
                {
                    SplitTri(nodes[i], tris[j]);
                    triangulate.Add(tris[tris.Count-1]);
                    triangulate.Add(tris[tris.Count-2]);
                    triangulate.Add(tris[tris.Count-3]);
                    while (triangulate.Count > 0) {
                        Triangulate(triangulate[triangulate.Count - 1]);
                    }
                    break;
                }
            }
        }
        //remove starting tris
        //for (int i = tris.Count - 1; i >= 0; i--)
        //{
        //    for (int j = 0; j < 3; j++)
        //    {
        //        if (tris[i][j].Pos2D == this.startingNodes[0].Pos2D || tris[i][j].Pos2D == this.startingNodes[1].Pos2D || tris[i][j].Pos2D == this.startingNodes[2].Pos2D)
        //        {
        //            //Debug.Log("remove");
        //            tris.RemoveAt(i);
        //            break;
        //        }
        //    }
        //}
    }



    /// <summary>
    /// Adds nodes a, b, c and tri abc so that any point in between min max area would be inside
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <param name="minY"></param>
    /// <param name="maxY"></param>
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
        startingNodes[0] = a;
        startingNodes[1] = b;
        startingNodes[2] = c;
        nodes.Add(a);
        nodes.Add(b);
        nodes.Add(c);
        tris.Add(new Tri(a, c, b));
    }



    /// <summary>
    /// remove current tri ABC and add ABD, BCD, CAD
    /// </summary>
    /// <param name="point">Node D</param>
    /// <param name="currentTri">Tri ABC</param>
    void SplitTri(Node point, Tri currentTri)
    {
        tris.Remove(currentTri.Remove());
        tris.Add(new Tri(currentTri.A, currentTri.B, point ));
        tris.Add(new Tri(currentTri.B, currentTri.C, point ));
        tris.Add(new Tri(currentTri.C, currentTri.A, point ));
    }



    bool Triangulate(Tri tri)
    {
        //for points connected to ab
        for (int i = 0; i < tri.A.GetConnections.Count; i++)
        {
            //for those points connected to the points off a
            for (int j = 0; j < tri.A.GetConnections[i].GetConnections.Count; j++)
            {
                //if they connect to b then we have found a quad
                if (tri.A.GetConnections[i].GetConnections[j] == tri.B)
                {
                    //if the quads notC is in a triangulated circle from our tri flip the quad
                    if (InCircle(tri.A.GetConnections[i], tri))
                    {
                        Tri adc = new Tri(tri.A, tri.A.GetConnections[i], tri.C);
                        Tri bcd = new Tri(tri.B, tri.C, tri.A.GetConnections[i]);

                        tris.Add(adc);
                        tris.Add(bcd);
                        RemoveTri(tri);
                        RemoveTri(new Tri(tri.A, tri.B, tri.A.GetConnections[i]));
                        triangulate.Remove(tri);
                        //add adc bdc
                        triangulate.Add(tris[tris.Count - 1]);
                        triangulate.Add(tris[tris.Count - 2]);
                        return true;
                    }
                }
            }
        }
        //for points connected to bc
        for (int i = 0; i < tri.B.GetConnections.Count; i++)
        {
            //for those points connected to the points off a
            for (int j = 0; j < tri.B.GetConnections[i].GetConnections.Count; j++)
            {
                //if they connect to b then we have found a quad
                if (tri.B.GetConnections[i].GetConnections[j] == tri.C)
                {
                    //if the quads notC is in a triangulated circle from our tri flip the quad
                    if (InCircle(tri.B.GetConnections[i], tri))
                    {
                        Tri cad = new Tri(tri.C, tri.A, tri.B.GetConnections[i]);
                        Tri abd = new Tri(tri.A, tri.B, tri.B.GetConnections[i]);
                        tris.Add(cad);
                        tris.Add(abd);
                        RemoveTri(tri);
                        RemoveTri(new Tri(tri.C, tri.B, tri.B.GetConnections[i]));
                        triangulate.Remove(tri);
                        triangulate.Add(tris[tris.Count - 1]);
                        triangulate.Add(tris[tris.Count - 2]);
                        return true;
                    }
                }
            }
        }
        //for points connected to ca
        for (int i = 0; i < tri.C.GetConnections.Count; i++)
        {
            //for those points connected to the points off a
            for (int j = 0; j < tri.C.GetConnections[i].GetConnections.Count; j++)
            {
                //if they connect to b then we have found a quad
                if (tri.C.GetConnections[i].GetConnections[j] == tri.A)
                {
                    //if the quads notC is in a triangulated circle from our tri flip the quad
                    if (InCircle(tri.C.GetConnections[i], tri))
                    {
                        Tri bcd = new Tri(tri.A, tri.C, tri.C.GetConnections[i]);
                        Tri abd = new Tri(tri.A, tri.B, tri.C.GetConnections[i]);

                        tris.Add(bcd);
                        tris.Add(abd);
                        RemoveTri(tri);
                        RemoveTri(new Tri(tri.A, tri.C, tri.C.GetConnections[i]));
                        triangulate.Remove(tri);
                        triangulate.Add(tris[tris.Count - 1]);
                        triangulate.Add(tris[tris.Count - 2]);
                        return true;
                    }
                }
            }
        }
        triangulate.Remove(tri);
        return false;
    }

    /// <summary>
    /// returns if point is inside tri vertAvertBvertC
    /// </summary>
    /// <param name="point"></param>
    /// <param name="vertA"></param>
    /// <param name="vertB"></param>
    /// <param name="vertC"></param>
    /// <returns></returns>
    bool InCircle(Node node, Tri tri) {
        Vector2 vertA = tri.A.Pos2D, vertB = tri.B.Pos2D, vertC = tri.C.Pos2D;
        Vector2 point = node.Pos2D;
        bool undefinedAB = false, undefinedBC = false;

        float xDist = vertB.x - vertA.x;
        float yDist = vertB.y - vertA.y;
        Vector2 centerAB = vertA + new Vector2(xDist/2,yDist/2);

        float slopeAB, perpSlopeAB = 0, perpInterceptAB = 0;
        //AB is undefined
        if ((vertB.x - vertA.x) == 0)
        {
            perpSlopeAB = 0;
        }
        else
        {
            slopeAB = (vertB.y - vertA.y) / (vertB.x - vertA.x);
            //perpSlopeAB is undefned
            if (slopeAB == 0)
            {
                undefinedAB = true;
            }
            else
            {
                perpSlopeAB = -1 / slopeAB;
                perpInterceptAB = centerAB.y - (perpSlopeAB * centerAB.x);
            }
        }
        

        xDist = vertC.x - vertB.x;
        yDist = vertC.y - vertB.y;
        Vector2 centerBC = vertB + new Vector2(xDist/2,yDist/2);

        float slopeBC, perpSlopeBC = 0, perpInterceptBC = 0;
        //BC is undefined
        if ((vertC.x - vertB.x) == 0)
        {
            perpSlopeBC = 0;
        }
        else
        {
            slopeBC = (vertC.y - vertB.y) / (vertC.x - vertB.x);
            //perpSlopeBC is undefned
            if (slopeBC == 0)
            {
                undefinedBC = true;

            }
            else
            {
                perpSlopeBC = -1 / slopeBC;
                perpInterceptBC = centerBC.y - (perpSlopeBC * centerBC.x);
            }
        }
        
        float xIntercept, yIntercept;
        if(undefinedAB){
            xIntercept = centerAB.x;
            yIntercept = perpSlopeBC * xIntercept + perpInterceptBC;
        }else if(undefinedBC){
            xIntercept = centerBC.x;
            yIntercept = perpSlopeAB * xIntercept + perpInterceptAB;
        }else{
            xIntercept = (perpInterceptBC - perpInterceptAB) / (perpSlopeAB - perpSlopeBC);
            yIntercept = perpSlopeAB * xIntercept + perpInterceptAB;
        }
        
        Vector2 circleCenter = new Vector2(xIntercept, yIntercept);
        if ((circleCenter - point).magnitude <= (circleCenter - vertA).magnitude)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    void RemoveTri(Tri tri) {
        for (int i = 0; i < tris.Count; i++)
        {
            if (tris[i] == new Tri(tri.A, tri.B, tri.C))
            {
                tris.Remove(tris[i].Remove());
                break;
            }
            else if (tris[i] == new Tri(tri.B, tri.C, tri.A))
            {
                tris.Remove(tris[i].Remove());
                break;
            }
            else if (tris[i] == new Tri(tri.C, tri.A, tri.B))
            {
                tris.Remove(tris[i].Remove());
                break;
            }
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
public class Node {

    List<Node> connectedNodes = new List<Node>();
    Vector2 pos;
    
    public Node(Vector2 pos)
    {
        this.pos = pos;
    }

    public Vector2 Pos2D { get => pos; }
    public Vector3 Pos3D { get => new Vector3(pos.x,0,pos.y); }
    public List<Node> GetConnections { get => connectedNodes; private set => connectedNodes = value; }

    public void AddConnection(Node connection) {
        connectedNodes.Add(connection);
    }
    public void RemoveConnection(Node connection) {
        connectedNodes.Remove(connection);
    }
}
public class Tri {
    Node a;
    Node b;
    Node c;


    public Tri(Node a, Node b, Node c) {
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
    public Tri Remove() {
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

    /// <summary>
    /// Returns false if node is not positioned inside tri
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool InTri(Node node)
    {
        Vector3 vertA = A.Pos3D, vertB = B.Pos3D, vertC = C.Pos3D;
        Vector3 point = node.Pos3D;

        Vector3 sideA = vertB - vertA;
        float certaintyA = Vector3.Cross(sideA, point - vertA).y;
        if (certaintyA < 0)
        {
            return false;
        }

        Vector3 sideB = vertC - vertB;
        float certaintyB = Vector3.Cross(sideB, point - vertB).y;
        if (certaintyB < 0)
        {
            return false;
        }

        Vector3 sideC = vertA - vertC;
        float certaintyC = Vector3.Cross(sideC, point - vertC).y;
        if (certaintyC < 0)
        {
            return false;
        }

        return true;
    }

}
