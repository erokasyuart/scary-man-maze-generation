using UnityEngine;

public class MazeConstructor : MonoBehaviour
{
    public bool showDebug;

    public GameObject treasure;
    
    [SerializeField] private Material mazeMat1;
    [SerializeField] private Material mazeMat2;
    [SerializeField] private Material startMat;
    [SerializeField] private Material treasureMat;
    [SerializeField] private Material guideMat;

    public int[,] data
    {
        get; private set;
    }

    public int[,] FromDimensions(int sizeRows, int sizeCols)
    {
        int[,] maze = new int[sizeRows, sizeCols];
        
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {        
            for (int j = 0; j <= cMax; j++)
            {       
                if (i == 0 || j == 0 || i == rMax || j == cMax)
                {               
                    maze[i, j] = 1;
                }                                    
                else if (i % 2 == 0 && j % 2 == 0 && Random.value > placementThreshold)                                    
                {
                    maze[i, j] = 1;

                    int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1);
                    int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                    maze[i+a, j+b] = 1;
                }
            } 
        }
        return maze;
    }

    public float placementThreshold = 0.1f; //chance of empty space

    private MazeMeshGenerator meshGenerator;
    
    public Node[,] graph;

    public float hallWidth{get; private set;}
    public int goalRow{get; private set;}
    public int goalCol{get; private set;}

    void Awake()
    {
        // default to walls surrounding a single empty cell
        data = new int[,]
        {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        };

        meshGenerator = new MazeMeshGenerator();

        hallWidth = meshGenerator.width;
    }

    public void GenerateNewMaze(int sizeRows, int sizeCols, TriggerEventHandler treasureCallback, TriggerEventHandler monsterCallback)
    {
        DisposeOldMaze();

        if (sizeRows % 2 == 0 && sizeCols % 2 == 0) //checks if both are even
        {
            Debug.LogError("Odd numbers work better for dungeon size.");
        }
        data = FromDimensions(sizeRows, sizeCols);

        graph = new Node[sizeRows, sizeCols];
        for (int i = 0; i < sizeRows; i++) //going through maze data
        {
            for (int j = 0; j < sizeCols; j++)
            {
                graph[i, j] = data[i,j] == 0 ? new Node(i, j, true) : new Node(i, j, false); //for every 0 isWalkabale = true
            }
        }

        DisplayMaze();

        goalRow = data.GetUpperBound(0) - 1;
        goalCol = data.GetUpperBound(1) - 1;

        PlaceGoal(treasureCallback);
    }

    void OnGUI()
    {
        if (!showDebug)
        {
            return;
        }
        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        string msg = "";

        for (int i = rMax; i >= 0; i--)
        {
            for (int j = 0; j <= cMax; j++)
                msg += maze[i, j] == 0 ? "...." : "==";
            msg += "\n";
        }

        GUI.Label(new Rect(20, 20, 500, 500), msg);
    
    }

    private void DisplayMaze()
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;
        go.name = "Procedural Maze";
        go.tag = "Generated";

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = meshGenerator.FromData(data);
        
        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] {mazeMat1, mazeMat2};
    }

    public void DisposeOldMaze()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Generated");
        foreach (GameObject go in objects)
        {
            Destroy(go);
        }
    }

    private void PlaceGoal(TriggerEventHandler treasureCallback)
    {
        treasure = GameObject.CreatePrimitive(PrimitiveType.Cube); //make a cube
        treasure.transform.position = new Vector3(goalCol * hallWidth, .5f, goalRow * hallWidth); //position it at the end of the maze
        treasure.name = "Treasure";
        treasure.tag = "Generated";

        treasure.GetComponent<BoxCollider>().isTrigger = true;
        treasure.GetComponent<MeshRenderer>().sharedMaterial = treasureMat; //give it the treasure material

        TriggerEventRouter tc = treasure.AddComponent<TriggerEventRouter>(); //a script that is called at collision
        tc.callback = treasureCallback;
    }

    public void PlaceGuide(Node node)
    {
        GameObject guide = GameObject.CreatePrimitive(PrimitiveType.Sphere); //creates sphere
        guide.transform.position = new Vector3(node.y * hallWidth, .5f, node.x * hallWidth); //changes the position
        guide.name = "Guide"; //gives the sphere a name
        guide.tag = "Generated";

        guide.GetComponent<SphereCollider>().isTrigger = true; //disables the collision
        guide.GetComponent<MeshRenderer>().sharedMaterial = guideMat;
    }
}