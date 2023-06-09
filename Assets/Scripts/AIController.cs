﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 140;

    private Node[,] graph;
    public Node[,] Graph {get {return graph;} set {graph = value;}}

    [Header("Pathfinding")]
    private GameObject monster;
    public GameObject Monster
    {
        get { return monster; }
        set { monster = value; }
    }

    private GameObject player;
    public GameObject Player 
    {
        get { return player; }
        set { player = value; }
    }

    private float hallWidth;
    public float HallWidth 
    {
        get { return hallWidth; }
        set { hallWidth = value; }
    }

    [SerializeField] private float monsterSpeed;
    private int startRow = -1;
    private int startCol = -1;


    // Called in another script
    public void StartAI()
    {
        startRow = graph.GetUpperBound(0) - 1; //maze man's start position; -1 to take into account the walls
        startCol = graph.GetUpperBound(1) - 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (startRow != -1 && startCol != -1) //checks if the man isnt in his start position
        {
            int playerCol = (int)Mathf.Round(player.transform.position.x / hallWidth); //maze cells are hallwidth wide so divide by hallwidth to get player
            int playerRow = (int)Mathf.Round(player.transform.position.z / hallWidth); //rounds it to an int

            List<Node> path = FindPath(startRow, startCol, playerRow, playerCol); //sends FindPath(); the start pos of man and the current pos of player

            if (path != null && path.Count > 1) //if there is a returned path and not in the same cell as player (>1)
            {
                Node nextNode = path[1]; //go to the next position (path[0] is current cell monster is on)
                float nextX = nextNode.y * hallWidth; //getting the size of the cell
                float nextZ = nextNode.x * hallWidth;
                Vector3 endPosition = new Vector3(nextX, 0f, nextZ); //sets a vector3 for the next desired position *
                float step =  monsterSpeed * Time.deltaTime;
                monster.transform.position = Vector3.MoveTowards(monster.transform.position, endPosition, step);
                Vector3 targetDirection = endPosition - monster.transform.position;
                Vector3 newDirection = Vector3.RotateTowards(monster.transform.forward, targetDirection, step, 0.0f);
                monster.transform.rotation = Quaternion.LookRotation(newDirection);
                if(monster.transform.position == endPosition) //if the monster reaches the desired position *
                {
                    startRow = nextNode.x; //resets his start pos to the next node because this is checking every frame
                    startCol = nextNode.y;
                }
            }
        }
    }

    //Calculates the distance of node from goal and node from start
    private int CalculateDistanceCost(Node a, Node b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = xDistance - yDistance;
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    //Calculates the lowest "cost" to be able to move
    public Node GetLowestFCostNode(List<Node> pathNodeList)
    {
        Node lowestFCostNode = pathNodeList[0];
        for(int i = 1; i < pathNodeList.Count; i++)//go through the list
        {
            if(pathNodeList[i].fCost < lowestFCostNode.fCost)//if its lower than the "lowest"
            {
                lowestFCostNode = pathNodeList[i]; //swap
            }
        }
        return lowestFCostNode;
    }

    //Finds the adjacent nodes
    private List<Node> GetNeighbourList(Node currentNode)
    {
        List<Node> neighbourList = new List<Node>();

        if(currentNode.x - 1 >= 0)
        {
            neighbourList.Add(graph[currentNode.x - 1,currentNode.y]);

            if(currentNode.y - 1 >= 0)
                neighbourList.Add(graph[currentNode.x - 1, currentNode.y - 1]);
            if(currentNode.y + 1 < graph.GetLength(1))
                neighbourList.Add(graph[currentNode.x - 1, currentNode.y + 1]);
        }

        if(currentNode.x + 1 < graph.GetLength(0))
        {
            neighbourList.Add(graph[currentNode.x + 1, currentNode.y]);
                
            if(currentNode.y - 1 >= 0) 
                neighbourList.Add(graph[currentNode.x + 1, currentNode.y - 1]);
            if(currentNode.y + 1 < graph.GetLength(1)) 
                neighbourList.Add(graph[currentNode.x + 1, currentNode.y + 1]);
        }

        if(currentNode.y - 1 >= 0) 
            neighbourList.Add(graph[currentNode.x, currentNode.y - 1]);
        if(currentNode.y + 1 < graph.GetLength(1)) 
            neighbourList.Add(graph[currentNode.x, currentNode.y + 1]);
            
        return neighbourList;
    }

    //Stores a list of nodes from end to start
    private List<Node> CalculatePath(Node endNode)
    {
        List<Node> path = new List<Node>();
        path.Add(endNode);
        Node currentNode = endNode;
        while(currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    //Path finding
    public List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        Node startNode = graph[startX,startY];
        Node endNode = graph[endX, endY];

        List<Node> openList = new List<Node> { startNode };
        List<Node> closedList = new List<Node>();

        int graphWidth = graph.GetLength(0); //the outerbounds
        int graphHeight = graph.GetLength(1);

        for(int x = 0; x < graphWidth; x++)
            for(int y = 0; y < graphHeight; y++)
            {
                Node pathNode = graph[x, y];
                pathNode.gCost = int.MaxValue; //starts at the highest possible value
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }

        startNode.gCost = 0; //closest to start
        startNode.hCost = CalculateDistanceCost(startNode, endNode); //end node
        startNode.CalculateFCost();

        while(openList.Count > 0) ///while there are still open nodes
        {
            Node currentNode = GetLowestFCostNode(openList);
            if(currentNode == endNode) //if it reaches the goal
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode); //remove current node from open list
            closedList.Add(currentNode); //then add it to closed list

            foreach(Node neighbourNode in GetNeighbourList(currentNode)) //for every neighbouring node in the closed list
            {
                if(closedList.Contains(neighbourNode)) //if its already in the list
                {
                    continue; //no double checking, break current loop, continue
                }

                if(!neighbourNode.isWalkable) //if not walkable
                {
                    closedList.Add(neighbourNode); //add it to closed
                    continue; //break if loop, continue
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if(tentativeGCost < neighbourNode.gCost) //if neighbour has lower gcost
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);  
                    neighbourNode.CalculateFCost(); //recalculates fcost

                    if(!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode); //then adds it to the open list
                    }
                }
            }
        }

        //out of nodes on the open list
        return null;
    }

    //Removes the monster
    public void StopAI()
    {
        startRow = -1;
        startCol = -1;
        Destroy(monster);
    }
}
