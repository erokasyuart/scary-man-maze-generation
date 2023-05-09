using System;
using UnityEngine;

[RequireComponent(typeof(MazeConstructor))]           

public class GameController : MonoBehaviour
{
    private MazeConstructor constructor;

    public GameObject playerPrefab;
    public GameObject monsterPrefab;

    [SerializeField] private int rows;
    [SerializeField] private int cols;

    private AIController aIController;

    void Awake()
    {
        constructor = GetComponent<MazeConstructor>();
        aIController = GetComponent<AIController>();
    }
    
    void Start()
    {
        constructor.GenerateNewMaze(rows, cols, OnTreasureTrigger, OnMonsterTrigger);

        aIController.Graph = constructor.graph;
        aIController.Player = CreatePlayer(); //used in AIController
        aIController.Monster = CreateMonster(OnMonsterTrigger);
        aIController.HallWidth = constructor.hallWidth;
        aIController.StartAI();
    }

    void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            int playerCol = (int)Mathf.Round(player.transform.position.x / hallWidth); //maze cells are hallwidth wide so divide by hallwidth to get player
            int playerRow = (int)Mathf.Round(player.transform.position.x / hallWidth); //rounds it to an int
            aIController.FindPath(0, 0, playerCol, playerRow);
            for (int i = 0; i < 5; i++)
            {
                GameObject guide = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                guide.transform.position = new Vector3(0, .5f, 0);
            }
        }
    }

    private GameObject CreatePlayer() //playing player in square [1,1]
    {
        Vector3 playerStatPosition = new Vector3(constructor.hallWidth, 1, constructor.hallWidth);
        GameObject player = Instantiate(playerPrefab, playerStatPosition, Quaternion.identity);
        player.tag = "Generated";

        return player;
    }

    private GameObject CreateMonster(TriggerEventHandler monsterCallback)
    {
        Vector3 monsterPosition = new Vector3(constructor.goalCol * constructor. hallWidth, 0f, constructor.goalRow * constructor.hallWidth); //sets the monster's spawn position
        GameObject monster = Instantiate(monsterPrefab, monsterPosition, Quaternion.identity); //instantiates monster prefab
        monster.tag = "Generated";
        TriggerEventRouter trigger = monster.AddComponent<TriggerEventRouter>(); //just adds the TriggerEventRouter script
        trigger.callback = monsterCallback;

        return monster;
    }

    private void OnTreasureTrigger(GameObject trigger, GameObject other)
    {
        Debug.Log("You Won!");
        aIController.StopAI();
    }

    //advanced assessment task
    private void OnMonsterTrigger(GameObject trigger, GameObject other)
    {
        Debug.Log("You got got!");
    }
}