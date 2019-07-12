using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Dungeon.Characters.Enemies;
/// <summary>
/// Generate an inter connected map of rooms from the start of an arc to the end
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    
    public LevelSettings settings;
    private List<Room> instantiatedRooms = new List<Room>();
    private DelaunayTriangulation triangulate;
    public static LevelGenerator instance;
    public GameObject bridge;
    public GameObject key;
    public List<Vector3> spawnPoints = new List<Vector3>();
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    //Gizmo Variables
    public float displayRadius = 1;
    public bool DrawOnlyInArc = false;
    public bool autoGenerate;
    public float roomSize = 10;
    public int rejectionSamples = 30;
    List<Vector2> points;
    Vector2 regionSize = Vector2.one;
    public bool gizmos = true;

    private void Start()
    {
        Generate(true);
        GameObject towerinst = Instantiate(settings.tower, Vector3.zero, Quaternion.identity,transform);
        towerinst.transform.localScale = Vector3.one * settings.arcRadius * 2;
        
    }


    /// <summary>
    /// Places a random Room from rooms at pos
    /// </summary>
    /// <param name="rooms"></param>
    /// <param name="pos"></param>
    bool PlaceRoom(Room[] rooms, Vector3 loc) {
        Room selectedRoom = rooms[Random.Range(0, rooms.Length)];
        if (PlaceRoom(selectedRoom, loc))
        {
            return true;
        }else
        {
            return false;
        }
        
    }
    bool PlaceRoom(Room room, Vector3 loc)
    {
        Room spawnRoom = Instantiate(room, loc, Quaternion.identity, transform);
        instantiatedRooms.Add(spawnRoom);
        spawnRoom.transform.forward = loc;
        spawnRoom.node = new Node(new Vector2(spawnRoom.transform.position.x, spawnRoom.transform.position.z));
        for (int i = 0; i < spawnRoom.spawnpositions.Count; i++)
        {
            spawnPoints.Add(spawnRoom.spawnpositions[i] + spawnRoom.transform.position);
        }
        return true;
    }
    
    public void Generate(bool spawn) {
        //Debug.Log("generate");
        int startoffset = 2;
        Vector2[] startpoints = new Vector2[2];
        
        float yMax = settings.arcRadius + settings.arcWidth;
        float xMin;
        float xMax;
        float yMin;
        if (settings.arcAngle > 180)
        {
            xMin = -settings.arcRadius - settings.arcWidth;
            xMax = -xMin;
            yMin = Mathf.Cos(Mathf.Deg2Rad * (settings.arcAngle / 2)) * (settings.arcRadius + settings.arcWidth);
        }
        else {
            xMin= Mathf.Sin(Mathf.Deg2Rad * -(settings.arcAngle / 2)) * (settings.arcRadius + settings.arcWidth);
            xMax = -xMin;
            yMin = Mathf.Cos(Mathf.Deg2Rad * (settings.arcAngle / 2)) * settings.arcRadius;
        }
        regionSize = new Vector3(xMax- xMin, yMax -yMin);
        
        Vector2 angleVec = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (startoffset - settings.arcAngle / 2)), Mathf.Cos(Mathf.Deg2Rad * (startoffset - settings.arcAngle / 2)));
        startpoints[1] = (angleVec * (settings.arcRadius + 5)) + new Vector2(regionSize.x / 2, regionSize.y / 2- (settings.arcRadius + settings.arcWidth - regionSize.y / 2f));
        
        
        angleVec = new Vector2(Mathf.Sin(Mathf.Deg2Rad * (settings.arcAngle - startoffset - settings.arcAngle / 2)), Mathf.Cos(Mathf.Deg2Rad * (settings.arcAngle - startoffset - settings.arcAngle / 2)));
        startpoints[0] = angleVec * (settings.arcRadius +5) + new Vector2(regionSize.x / 2, regionSize.y / 2 - (settings.arcRadius + settings.arcWidth - regionSize.y / 2f)); ;
        points = PoissonDiscSampler.GeneratePoints(roomSize, regionSize, startpoints, rejectionSamples);
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 offset = new Vector3(0, 0, settings.arcRadius + settings.arcWidth - regionSize.y / 2f);
            Vector3 loc = new Vector3(points[i].x - regionSize.x / 2, 0, points[i].y - regionSize.y / 2) + offset;
            if (spawn && i < startpoints.Length)
            {
                PlaceRoom(settings.reqRooms[i], loc);
            }
            else if (spawn && loc.magnitude >= settings.arcRadius && loc.magnitude <= settings.arcRadius + settings.arcWidth)
            {
                PlaceRoom(settings.rndRoomPool, loc);
            }
        }
        List<Node> startingNodes = new List<Node>();
        for (int i = 0; i < instantiatedRooms.Count; i++)
        {
            startingNodes.Add(instantiatedRooms[i].node);
        }
        triangulate = GetComponent<DelaunayTriangulation>();
        List<Node> starting = new List<Node>();
        starting.Add(new Node(new Vector2(0,settings.arcRadius/2)));
        //starting.Add(new Node(startpoints[0]));
        //starting.Add(new Node(startpoints[1]));
        triangulate.RunDelaunayTriangulation(starting, startingNodes,new Vector2(xMin,yMin), new Vector2(xMax,yMax));
        MakeConnections();
        SpawnEnemies();

    }
    private void SpawnEnemies() {
        int enemywithKey;
        if (settings.forcedKey)
        {
            enemywithKey = settings.requiredEnemy;
        }
        else
        {
            enemywithKey = Random.Range(0, settings.enemies.Count);
        }
        for (int i = 0; i < settings.enemies.Count; i++)
        {
            int levelBoss = settings.enemies[i].CountForLevel;
            if (i == enemywithKey)
            {
                levelBoss = Random.Range(0, settings.enemies[i].CountForLevel);
            }
            for (int j = 0; j < settings.enemies[i].CountForLevel; j++)
            {
                //get random from list of all spawn locations, remove after spawn
                int point = Random.Range(0, spawnPoints.Count);
                Enemy enemy = Instantiate(settings.enemies[i].EnemyPrefab, spawnPoints[point]+ new Vector3(0,2,0), Quaternion.identity).GetComponent<Enemy>();
                spawnPoints.RemoveAt(point);
                if (enemywithKey == j)
                {
                    enemy.SetDrop(key);
                }
            } 
        }
    }

    private void MakeConnections() {
        List<Tri> tris = triangulate.getTris();
        List<BridgePath> paths = new List<BridgePath>();
        for (int i = 0; i < tris.Count; i++)
        {
            BridgePath pathab = new BridgePath(tris[i].A.Pos3D, tris[i].B.Pos3D);
            BridgePath pathbc = new BridgePath(tris[i].B.Pos3D, tris[i].C.Pos3D);
            BridgePath pathca = new BridgePath(tris[i].C.Pos3D, tris[i].A.Pos3D);
            if (!paths.Contains(pathab)) {
                paths.Add(pathab);
            }
            if (!paths.Contains(pathbc))
            {
                paths.Add(pathbc);
            }
            if (!paths.Contains(pathca))
            {
                paths.Add(pathca);
            }
        }
        for (int i = 0; i < paths.Count; i++)
        {
            int chance = Random.Range(0, 100);
            if (chance<=66)
            {
                GameObject currentbridge = Instantiate(bridge);
                currentbridge.transform.position = paths[i].PointB + ((paths[i].PointA - paths[i].PointB) / 2);
                currentbridge.transform.LookAt(paths[i].PointA);
                currentbridge.transform.localScale = Vector3.forward * (paths[i].PointA - paths[i].PointB).magnitude + (new Vector3(3, .8f, 1) - Vector3.forward);
                currentbridge.transform.parent = gameObject.transform;
            }
        }

        //create list of all connection
        //spawn bridge from start to finnish
    }
    private void OnValidate()
    {
        if (autoGenerate)
        {
            Generate(false);
        }
    }
    
    void OnDrawGizmos()
    {
        if (gizmos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(0,0, settings.arcRadius + settings.arcWidth), displayRadius);
            Gizmos.DrawSphere(new Vector3(0, 0, -(settings.arcRadius + settings.arcWidth)), displayRadius);
            Gizmos.DrawSphere(new Vector3(-(settings.arcRadius + settings.arcWidth),0,0), displayRadius);
            Gizmos.DrawSphere(new Vector3(settings.arcRadius + settings.arcWidth, 0, 0), displayRadius);
            Gizmos.color = Color.grey;
            Vector3 offset = new Vector3(0, 0, settings.arcRadius + settings.arcWidth - regionSize.y / 2f);
            Gizmos.DrawWireCube(offset, new Vector3(regionSize.x, 0, regionSize.y));
            if (points != null)
            {
                foreach (Vector2 point in points)
                {
                    Vector3 loc = new Vector3(point.x - regionSize.x / 2, 0, point.y - regionSize.y / 2);
                    if (DrawOnlyInArc)
                    {
                        loc += offset;
                        if (loc.magnitude >= settings.arcRadius && loc.magnitude <= settings.arcRadius + settings.arcWidth)
                        {
                            Gizmos.DrawSphere(loc, displayRadius);
                        }
                    }
                    else
                    {
                        Gizmos.DrawSphere(loc + offset, displayRadius);
                    }

                }
            }
        }
    }
}

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    LevelGenerator gen;
    Editor levelSettings;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate"))
        {
            gen.Generate(false);
        }
        CreateCachedEditor(gen.settings, null, ref levelSettings);
        levelSettings.OnInspectorGUI();
    }

    private void OnEnable()
    {
        gen = (LevelGenerator)target;
    }
}
public class BridgePath{
    Vector3 pointA;
    Vector3 pointB;
    public BridgePath(Vector3 PointA, Vector3 PointB)
    {
        this.pointA = PointA;
        this.pointB = PointB;
    }

    public Vector3 PointA { get => pointA; private set => pointA = value; }
    public Vector3 PointB { get => pointB; private set => pointB = value; }

    public override bool Equals(object obj)
    {

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if ((PointA == ((BridgePath)obj).PointA && PointB == ((BridgePath)obj).PointB)|| (PointB == ((BridgePath)obj).PointA && PointA == ((BridgePath)obj).PointB))
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
}