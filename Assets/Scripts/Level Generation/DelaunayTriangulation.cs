using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour
{
    List<Node[]> tris = new List<Node[]>();
    List<Node> nodes = new List<Node>();
    Node[] startingNodes = new Node[3];
    public bool gizmos= false;
    private void Start()
    {

        //List<Node> nodes = new List<Node>();
        //nodes.Add(new Node(new Vector2(1,1)));
        //GenerateGraph(nodes,0,2,0,2);
    }
    
    public void GenerateGraph(List<Node> startingNodes, float minX, float maxX, float minY, float maxY) {
        nodes = startingNodes;
        EncapsulateRectangle(minX, maxX, minY, maxY);
        for (int i = 0; i < nodes.Count-3; i++)
        {
            for (int j = 0; j < tris.Count; j++)
            {
                bool inTri = InTri(nodes[i].Pos3D, tris[j][0].Pos3D, tris[j][1].Pos3D, tris[j][2].Pos3D);
                if (inTri)
                {
                    SplitTri(nodes[i], tris[j]);
                    //add tries abd, bcd, cad to triangulate
                    //while triangulate.count > 0
                        //checktriangulation triangulate(0)
                    break;
                }
            }
        }
        for (int i = tris.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < 3; j++)
            {
                if (tris[i][j].Pos2D == this.startingNodes[0].Pos2D || tris[i][j].Pos2D == this.startingNodes[1].Pos2D || tris[i][j].Pos2D == this.startingNodes[2].Pos2D)
                {
                    //Debug.Log("remove");
                    tris.RemoveAt(i);
                    break;
                }
            }
        }
    }
    void checkTriangulation(){
        /* given tri abc
        for points connected to a, if more than one also connect to b
            incircle point c,and tri a, b, shared point that is not d(u for undefined)
            if incircle
                tris remove abu and acb
                tris add acu and cbu
                add triangulate some squares
        remove triangulate(0)
        */
    }
    void EncapsulateRectangle(float minX, float maxX, float minY, float maxY){
        float tan = Mathf.Tan(45 * Mathf.Deg2Rad);
        float width = maxX - minX;
        float height = maxY - minY;
        Vector2 apos = new Vector2(minX-2, minY - 2);
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
        tris.Add(new Node[] { a, c, b });
    }

    bool InTri(Vector3 point, Vector3 vertA, Vector3 vertB, Vector3 vertC) {
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

    void SplitTri(Node point, Node[] currentTri) {
        tris.Remove(currentTri);
        tris.Add(new Node[] { currentTri[0], currentTri[1], point });
        tris.Add(new Node[] { currentTri[1], currentTri[2], point });
        tris.Add(new Node[] { currentTri[2], currentTri[0], point });
    }

    bool InCircle(Vector2 point, Vector2 vertA, Vector2 vertB, Vector2 vertC) {

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

    private void OnDrawGizmos() {
        if(gizmos){
            for (int i = 0; i < tris.Count; i++)
            {
                Gizmos.DrawLine(tris[i][0].Pos3D, tris[i][1].Pos3D);
                Gizmos.DrawLine(tris[i][1].Pos3D, tris[i][2].Pos3D);
                Gizmos.DrawLine(tris[i][2].Pos3D, tris[i][0].Pos3D);
            }
        }        
        
    }
}
public class Node {

    List<Node> connectedNodes;
    Vector2 pos;
    
    public Node(Vector2 pos)
    {
        this.pos = pos;
    }

    public Vector2 Pos2D { get => pos; }
    public Vector3 Pos3D { get => new Vector3(pos.x,0,pos.y); }
    public void AddConnection(Node connection) {

    }
    public void RemoveConnection() {

    }
}
