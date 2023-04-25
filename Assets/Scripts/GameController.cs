using System;
using UnityEngine;

[RequireComponent(typeof(MazeConstructor))]           

public class GameController : MonoBehaviour
{
    private MazeConstructor constructor;

    public GameObject playerPrefab;

    [SerializeField] private int rows;
    [SerializeField] private int cols;

    void Awake()
    {
        constructor = GetComponent<MazeConstructor>();
    }
    
    void Start()
    {
        constructor.GenerateNewMaze(rows, cols);

        CreatePlayer();
    }

    private void CreatePlayer() //playing player in square [1,1]
    {
        Vector3 playerStatPosition = new Vector3(constructor.hallWidth, 1, constructor.hallWidth);
        GameObject player = Instantiate(playerPrefab, playerStatPosition, Quaternion.identity);
        player.tag = "Generated";
    }
}