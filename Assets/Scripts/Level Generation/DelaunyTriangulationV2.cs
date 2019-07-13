using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunyTriangulationV2 : MonoBehaviour
{
    List<Node> UnsortedNodes = new List<Node>();
    List<Node> SortedNodes = new List<Node>();
    List<Node> TempNodes = new List<Node>();

    List<Tri> tris = new List<Tri>();
    List<Tri> triangulationQueue = new List<Tri>();
    Side returnedTestQuadSide;

    public float rad = 1;
    public bool gizmos = false;

    /// <summary>
    /// Take a list of nodes and vectors for the max and min size. 
    /// Triangulate the most equilateral path between all points with no overlapping paths.
    /// Only garunteed to work with convex surfaces.
    /// </summary>
    /// <param name="NodeSet">List of nodes to triangulate</param>
    /// <param name="min">Position of the most minimal node</param>
    /// <param name="max">Position of the maximun node</param>
    public void DelaunayTriangulation(List<Node> NodeSet, Vector2 min, Vector2 max) {
        UnsortedNodes = NodeSet;
        Tri EncapTri = EncapsulateDomain(min, max);

        foreach (Node node in EncapTri.GetNodes())
        {
            SortedNodes.Add(node);
            TempNodes.Add(node);
        }
        tris.Add(EncapTri.AddNodeConnections());
        Triangulate();
    }

    /// <summary>
    /// Take a list of nodes and vectors for the max and min size, as well as some starting nodes to refine how the paths are made. 
    /// Triangulate the most equilateral path between all points with no overlapping paths.
    /// Removes starting nodes after calculation. Only garunteed to work with convex surfaces, but the starting nodes can help refine the object.
    /// </summary>
    /// <param name="StartingNodes">//En</param>
    /// <param name="NodeSet"></param>
    public void DelaunayTriangulation(List<Node> startingNodes, List<Node> NodeSet, Vector2 min, Vector2 max) {
        UnsortedNodes = NodeSet;
        Tri EncapTri = EncapsulateDomain(min, max);

        foreach (Node node in EncapTri.GetNodes())
        {
            SortedNodes.Add(node);
            TempNodes.Add(node);
        }
        foreach (Node node in startingNodes) {
            UnsortedNodes.Add(node);
            TempNodes.Add(node);
        }
        tris.Add(EncapTri.AddNodeConnections());
        Triangulate();
    }

    /// <summary>
    /// Takes care of all the triangulation from the member variables initialized through one of the DelaunayTriangulation methods.
    /// </summary>
    private void Triangulate() {
        while (UnsortedNodes.Count > 0)
        {
            //Find encapsulating tri for current node
            Tri encapsulatingTri = FindEncapsulatingTri(tris, UnsortedNodes[UnsortedNodes.Count - 1]);
            //Check that a tri was found
            if (encapsulatingTri == null)
            {
                Debug.LogWarning("Could not find encapsulating tri for node at: " + UnsortedNodes[UnsortedNodes.Count - 1].Pos2D);
                UnsortedNodes.Remove(UnsortedNodes[UnsortedNodes.Count - 1]);
                continue;
            }

            //get the split tris tris
            Tri[] splitTris = SplitTri(encapsulatingTri, UnsortedNodes[UnsortedNodes.Count - 1]);
            if (splitTris != null)
            {
                //if splittri sucset neighbors of split tri

                if (encapsulatingTri.ABNeighbor != null)
                {
                    splitTris[0].UpdateNeighbor(Side.AB, encapsulatingTri.ABNeighbor, encapsulatingTri.ABNeighborConnectingSide);
                }
                if (encapsulatingTri.BCNeighbor != null)
                {
                    splitTris[1].UpdateNeighbor(Side.AB, encapsulatingTri.BCNeighbor, encapsulatingTri.BCNeighborConnectingSide);
                }
                if (encapsulatingTri.CANeighbor != null)
                {
                    splitTris[2].UpdateNeighbor(Side.AB, encapsulatingTri.CANeighbor, encapsulatingTri.CANeighborConnectingSide);
                }
                splitTris[0].UpdateNeighbor(Side.BC, splitTris[1], Side.CA);
                splitTris[1].UpdateNeighbor(Side.BC, splitTris[2], Side.CA);
                splitTris[2].UpdateNeighbor(Side.BC, splitTris[0], Side.CA);
            }
            else
            {
                Debug.LogWarning("Node was not in tri specified: " + encapsulatingTri.PrintTri() + " Node: " + UnsortedNodes[UnsortedNodes.Count - 1].Pos2D);
                UnsortedNodes.Remove(UnsortedNodes[UnsortedNodes.Count - 1]);
                continue;
            }


            if (!tris.Remove(encapsulatingTri.RemoveNodeConnections()))
            {
                Debug.Log("Failed to remove containing tri " + encapsulatingTri.PrintTri());
            }

            SortedNodes.Add(UnsortedNodes[UnsortedNodes.Count - 1]);
            if (!UnsortedNodes.Remove(UnsortedNodes[UnsortedNodes.Count - 1]))
            {
                Debug.Log("failed to remove unsorted node " + UnsortedNodes[UnsortedNodes.Count - 1]);
            }

            foreach (Tri tri in splitTris)
            {
                tris.Add(tri.AddNodeConnections());
                triangulationQueue.Add(tri);
            }
            while (triangulationQueue.Count > 0)
            {
                Tri[] triSet;
                triSet = VerifyTri(triangulationQueue[0]);
                if (triSet == null)
                {
                    if (!triangulationQueue.Remove(triangulationQueue[0]))
                    {
                        Debug.Log("Failed to remove triangulationQueue[0] " + triangulationQueue[0].PrintTri());
                    }
                }
                else
                {
                    //set neighbors of new tris
                    if (triSet[0].GetNeighbor(triSet[0].PrevSide(returnedTestQuadSide)) != null)
                    {
                        triSet[2].UpdateNeighbor(Side.AB, triSet[0].GetNeighbor(triSet[0].PrevSide(returnedTestQuadSide)), triSet[0].GetNeighborConnectingSide(triSet[0].PrevSide(returnedTestQuadSide)));
                    }
                    if (triSet[0].GetNeighbor(returnedTestQuadSide).GetNeighbor(triSet[0].GetNeighbor(returnedTestQuadSide).NextSide(triSet[0].GetNeighborConnectingSide(returnedTestQuadSide))) != null)
                    {
                        triSet[2].UpdateNeighbor(Side.BC, triSet[0].GetNeighbor(returnedTestQuadSide).GetNeighbor(triSet[0].GetNeighbor(returnedTestQuadSide).NextSide(triSet[0].GetNeighborConnectingSide(returnedTestQuadSide))),
                            triSet[0].GetNeighbor(returnedTestQuadSide).GetNeighborConnectingSide(triSet[0].GetNeighbor(returnedTestQuadSide).NextSide(triSet[0].GetNeighborConnectingSide(returnedTestQuadSide))));
                    }


                    if (triSet[0].GetNeighbor(triSet[0].NextSide(returnedTestQuadSide)) != null)
                    {
                        triSet[3].UpdateNeighbor(Side.AB, triSet[0].GetNeighbor(triSet[0].NextSide(returnedTestQuadSide)), triSet[0].GetNeighborConnectingSide(triSet[0].NextSide(returnedTestQuadSide)));
                    }
                    if (triSet[0].GetNeighbor(returnedTestQuadSide).GetNeighbor(triSet[0].GetNeighbor(returnedTestQuadSide).PrevSide(triSet[0].GetNeighborConnectingSide(returnedTestQuadSide))) != null)
                    {
                        triSet[3].UpdateNeighbor(Side.CA, triSet[0].GetNeighbor(returnedTestQuadSide).GetNeighbor(triSet[0].GetNeighbor(returnedTestQuadSide).PrevSide(triSet[0].GetNeighborConnectingSide(returnedTestQuadSide))),
                            triSet[0].GetNeighbor(returnedTestQuadSide).GetNeighborConnectingSide(triSet[0].GetNeighbor(returnedTestQuadSide).PrevSide(triSet[0].GetNeighborConnectingSide(returnedTestQuadSide))));
                    }

                    triSet[2].UpdateNeighbor(Side.CA, triSet[3], Side.BC);


                    if (!tris.Remove(triSet[0]))
                    {
                        Debug.Log("Failed to remove TriSet[0] " + triSet[0].PrintTri());
                    }
                    if (!tris.Remove(triSet[1]))
                    {
                        Debug.Log("Failed to remove TriSet[1] " + triSet[1].PrintTri());
                        if (tris.Contains(triSet[1]))
                        {
                            Debug.Log("but does contain triset[1]");
                        }
                    }
                    if (!triangulationQueue.Remove(triSet[0]))
                    {
                        Debug.Log("Failed to remove triangulationqueu tri " + triSet[0].PrintTri());
                    }
                    if (triangulationQueue.Contains(triSet[1]))
                    {
                        if (!triangulationQueue.Remove(triSet[1]))
                        {
                            Debug.Log("Failed to remove triangulationqueu tri " + triSet[1].PrintTri());
                        }
                    }

                    triSet[0].RemoveNodeConnections();
                    triSet[1].RemoveNodeConnections();
                    tris.Add(triSet[2].AddNodeConnections());
                    tris.Add(triSet[3].AddNodeConnections());
                    triangulationQueue.Add(triSet[2]);
                    triangulationQueue.Add(triSet[3]);
                }
            }
        }
        RemoveStartingNodes();
    }

    /// <summary>
    /// Removes all nodes from the tempnode list
    /// </summary>
    private void RemoveStartingNodes() {
        for (int i = tris.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < TempNodes.Count; j++)
            {
                if (tris[i].SharesNode(TempNodes[j]))
                {
                    tris.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// return tri that encapsulates the given range
    /// </summary>
    /// <param name="min">Minimum Range Value</param>
    /// <param name="max">Maximum Range Value</param>
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
    /// Returns the tri from given list that the given node is in or null if none are found
    /// </summary>
    /// <param name="tris">List of tris from which to find encapsulating tri</param>
    /// <param name="node">Node of wich we want to find encapsulating tri</param>
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
    /// Given a tri and node located inside it returns three tris that are the outcome of splitting the tri on the node. Returns null if node is outside.
    /// </summary>
    /// <param name="tri">Containing tri</param>
    /// <param name="node">Inner node</param>
    /// <returns></returns>
    private Tri[] SplitTri(Tri tri, Node node) {
        if (!tri.InTri(node))
        {
            return null;
        }
        Tri triABD = new Tri(tri.A, tri.B, node);
        Tri triBCD = new Tri(tri.B, tri.C, node);
        Tri triCAD = new Tri(tri.C, tri.A, node);
        return new Tri[] { triABD,triBCD,triCAD};
    }

    /// <summary>
    /// Takes a tri and tests the quads made up by each side to see if any are in a none optimal configuration, returns the first bad quad result it gets, or null.
    /// </summary>
    /// <param name="tri">Tri to be tested</param>
    /// <returns></returns>
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
    /// If the quad is bad return [0] abc [1] acd [2] bcd [3] abd and sets returnedQuadTestSide member variable to the tested side else return null
    /// </summary>
    /// <param name="tri">Tri to Test</param>
    /// <param name="side">Side to Test</param>
    /// <returns></returns>
    private Tri[] TestQuad(Tri tri, Side side) {
        //If theres no side on the given side to check return null
        if (!tri.HasSide(side))
        {
            return null;
        }

        //prepare the array for returning and add starting tris
        Tri[] solution = new Tri[4];
        solution[0] = tri;
        solution[1] = tri.GetNeighbor(side);

        //Set the nodes that make up quad ABC ACD
        Node A, B, C, D;
        A = tri.PrevNode(tri.OpposingNode(side));
        B = tri.OpposingNode(side);
        C = tri.NextNode(tri.OpposingNode(side));
        D = tri.GetNeighbor(side).OpposingNode(tri.GetNeighborConnectingSide(side));
        if (IsQuadBad(A, B, C, D))
        {
            //Exposes side used to class
            returnedTestQuadSide = side;
            //Add tris that make up flipped quad to solution array
            solution[2] = new Tri(B, C, D);
            solution[3] = new Tri(A, B, D);
            
            return solution;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Tests if the quad formed by ABC and BDC is the most equilateral configuration, if it isnt return true.
    /// </summary>
    /// <returns></returns>
    private bool IsQuadBad(Node A, Node B, Node C, Node D) {
        float Xca = A.Pos2D.x - C.Pos2D.x, Xba = A.Pos2D.x - B.Pos2D.x, Xbd = D.Pos2D.x - B.Pos2D.x, Xcd = D.Pos2D.x - C.Pos2D.x;
        float Yca = A.Pos2D.y - C.Pos2D.y, Yba = A.Pos2D.y - B.Pos2D.y, Ybd = D.Pos2D.y - B.Pos2D.y, Ycd = D.Pos2D.y - C.Pos2D.y;
        float a = (Xca * Xba + Yca * Yba) * (Xbd * Ycd - Xcd * Ybd);
        float b = (Yca * Xba - Xca * Yba) * (Xbd * Xcd + Ycd * Ybd);
        if (a <= b)
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
//public enum Side
//{
//    AB, BC, CA 
//}
//public class Node
//{

//    List<Node> connectedNodes = new List<Node>();
//    Vector2 pos;

//    public Node(Vector2 pos)
//    {
//        this.pos = pos;
//    }
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

    

//    public Vector2 Pos2D { get => pos; }
//    public Vector3 Pos3D { get => new Vector3(pos.x, 0, pos.y); }
//    public List<Node> GetConnections { get => connectedNodes; private set => connectedNodes = value; }
//    public void AddConnection(Node connection)
//    {
//        connectedNodes.Add(connection);
//    }
//    public void RemoveConnection(Node connection)
//    {
//        connectedNodes.Remove(connection);
//    }
//}
//public class Tri
//{
//    Node a;
//    Node b;
//    Node c;
//    public Tri ABNeighbor;
//    public Side ABNeighborConnectingSide;
//    public Tri BCNeighbor;
//    public Side BCNeighborConnectingSide;
//    public Tri CANeighbor;
//    public Side CANeighborConnectingSide;

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
//    /*Getters and Setters*/
//    public Node[] GetNodes()
//    {
//        Node[] nodes = new Node[] { A, B, C };
//        return nodes;
//    }
//    public Node A { get => a; private set => a = value; }
//    public Node B { get => b; private set => b = value; }
//    public Node C { get => c; private set => c = value; }
//    public void UpdateNeighbor(Side side, Tri tri, Side neighborSide) {
//        SetNeighbor(side, tri, neighborSide);
//        tri.SetNeighbor(neighborSide, this, side);
//    }
//    private void SetNeighbor(Side side, Tri tri, Side neighborSide)
//    {
//        if (side == Side.AB)
//        {
//            ABNeighbor = tri;
//            ABNeighborConnectingSide = neighborSide;
//        }
//        else if (side == Side.BC)
//        {
//            BCNeighbor = tri;
//            BCNeighborConnectingSide = neighborSide;
//        }
//        else
//        {
//            CANeighbor = tri;
//            CANeighborConnectingSide = neighborSide;
//        }
        
//    }
//    public Tri GetNeighbor(Side side)
//    {
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
//    public Side GetNeighborConnectingSide(Side side)
//    {
//        switch (side)
//        {
//            case Side.AB:
//                return ABNeighborConnectingSide;
//            case Side.BC:
//                return BCNeighborConnectingSide;
//            case Side.CA:
//                return CANeighborConnectingSide;
//            default:
//                return Side.AB;
//        }
//    }
//    /*Initializer*/
//    public Tri(Node a, Node b, Node c)
//    {
//        A = a;
//        B = b;
//        C = c;
        
//    }

//    public Tri AddNodeConnections() {
//        A.AddConnection(B);
//        A.AddConnection(C);
//        B.AddConnection(A);
//        B.AddConnection(C);
//        C.AddConnection(A);
//        C.AddConnection(B);
//        return this;
//    }
//    public Tri RemoveNodeConnections()
//    {
//        A.RemoveConnection(B);
//        A.RemoveConnection(C);
//        B.RemoveConnection(A);
//        B.RemoveConnection(C);
//        C.RemoveConnection(A);
//        C.RemoveConnection(B);
//        return this;
//    }
//    public Node NextNode(Node node) {
//        if (node == A)
//        {
//            return B;
//        }
//        else if (node == B)
//        {
//            return C;
//        }
//        else if (node == C)
//        {
//            return A;
//        }
//        else {
//            Debug.LogWarning("couldnt find node " + node.Pos2D + " in tri " + A.Pos2D + " " + B.Pos2D + " " + C.Pos2D);
//            return null;
//        }
//    }
//    public Node PrevNode(Node node)
//    {
//        if (node == A)
//        {
//            return C;
//        }
//        else if (node == B)
//        {
//            return A;
//        }
//        else if (node == C)
//        {
//            return B;
//        }
//        else
//        {
//            Debug.LogWarning("couldnt find node " + node.Pos2D + " in tri " + A.Pos2D + " " + B.Pos2D + " " + C.Pos2D);
//            return null;
//        }
//    }
//    public Node OpposingNode(Side side)
//    {
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
//    public bool HasSide(Side side)
//    {
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
//    public Side NextSide(Side side) {
//        if (side == Side.AB)
//        {
//            return Side.BC;
//        }
//        else if (side == Side.BC)
//        {
//            return Side.CA;
//        }
//        else
//        {
//            return Side.AB;
//        }
//    }
//    public Side PrevSide(Side side)
//    {
//        if (side == Side.CA)
//        {
//            return Side.BC;
//        }
//        else if (side == Side.BC) 
//        {
//            return Side.AB;
//        }
//        else
//        {
//            return Side.CA;
//        }
//    }
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
//    public string PrintTri() {
//        return "Tri with nodes " + A.Pos2D + " " + B.Pos2D + " " + C.Pos2D;
//    }
//    public Node FirstNodeOnSide(Side side) {
//        if (side == Side.AB)
//        {
//            return A;
//        }else if (side == Side.BC)
//        {
//            return B;
//        }
//        else
//        {
//            return C;
//        }
//    }
//    public Node LastNodeOnSide(Side side)
//    {
//        if (side == Side.AB)
//        {
//            return B;
//        }
//        else if (side == Side.BC)
//        {
//            return C;
//        }
//        else
//        {
//            return A;
//        }
//    }
//    public bool SharesNode(Node node) {
//        for (int i = 0; i < 3; i++)
//        {
//            if (GetNodes()[i] == node)
//            {
//                return true;
//            }
//        }
//        return false;
//    }
//}
