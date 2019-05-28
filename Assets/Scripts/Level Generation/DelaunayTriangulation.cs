using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour
{
    List<Node[]> tris = new List<Node[]>();
    List<Node> nodes = new List<Node>();
    Node[] startingNodes = new Node[3];
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
                    Debug.Log("remove");
                    tris.RemoveAt(i);
                    break;
                }
            }
        }
    }
    void EncapsulateRectangle(float minX, float maxX, float minY, float maxY)
    {
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

    private void OnDrawGizmos()
    {
        Debug.Log(tris.Count);
        for (int i = 0; i < tris.Count; i++)
        {
            Gizmos.DrawLine(tris[i][0].Pos3D, tris[i][1].Pos3D);
            Gizmos.DrawLine(tris[i][1].Pos3D, tris[i][2].Pos3D);
            Gizmos.DrawLine(tris[i][2].Pos3D, tris[i][0].Pos3D);
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
