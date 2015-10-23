﻿using UnityEngine;
using System.Collections;

public class EntityMotor : MonoBehaviour
{
    public delegate void ClickAction();
    public static event ClickAction OnAction;

    [SerializeField] private Vector2 currentCoords;
    [SerializeField] private GameObject currentTile;
    private GameObject previousTile;

    public float moveDuration;
    [SerializeField] private int moveDistance;
    [SerializeField] private int moveAmount = 0;

    private bool isMoving = false;

    private EntityType _EntityType;

    void Awake()
    {
        _EntityType = GetComponent<EntityType>();
    }

    void Start()
    {
        // Initial entity-to-tile setup
        currentCoords = new Vector2((int)transform.position.x, (int)transform.position.z);
        transform.position = new Vector3(currentCoords.x, transform.position.y, currentCoords.y);   // Lock transform into int coordinates
        foreach (GameObject tile in MapGenerator.tiles)
        {
            Tile _tile = tile.GetComponent<Tile>();
            if (_tile.TileCoord == currentCoords)
            {
                currentTile = tile;
                _tile.TileEnitities.Add(gameObject);
                previousTile = currentTile;
                break;
            }
        }
    }

    // Rescan grid to update entity-to-tile data
    void UpdatePosition()
    {
        currentCoords = new Vector2((int)transform.position.x, (int)transform.position.z);
        transform.position = new Vector3(currentCoords.x, transform.position.y, currentCoords.y);   // Lock transform into int coordinates
        foreach (GameObject tile in MapGenerator.tiles)
        {
            Tile _tile = tile.GetComponent<Tile>();
            if (_tile.TileCoord == currentCoords)
            {
                currentTile = tile;
                if(!_tile.TileEnitities.Contains(gameObject))
                    _tile.TileEnitities.Add(gameObject);
                if (previousTile != currentTile)
                    previousTile.GetComponent<Tile>().RemoveEntity(gameObject);
                previousTile = currentTile;
                break;
            }
        }
    }

    public void ShowMoveTiles()
    {

    }

    // Recursion that Calculate how many tiles it can possibly move without interference.
    private int GetMoveAmount(Vector3 direction, GameObject currentTile)
    {
        Tile _tile = currentTile.GetComponent<Tile>();
        if (moveAmount < moveDistance)
        {
            if (direction == Vector3.forward && _tile.TopTile && isWalkableTile(_tile.TopTile.GetComponent<Tile>())){
                moveAmount++;
                GetMoveAmount(direction, _tile.TopTile);
            }
            else if (direction == Vector3.right && _tile.RightTile && isWalkableTile(_tile.RightTile.GetComponent<Tile>())){
                moveAmount++;
                GetMoveAmount(direction, _tile.RightTile);
            }
            else if (direction == Vector3.back && _tile.BottomTile && isWalkableTile(_tile.BottomTile.GetComponent<Tile>())){
                moveAmount++;
                GetMoveAmount(direction, _tile.BottomTile);
            }
            else if (direction == Vector3.left && _tile.LeftTile && isWalkableTile(_tile.LeftTile.GetComponent<Tile>())){
                moveAmount++;
                GetMoveAmount(direction, _tile.LeftTile);
            }
        }
        //Debug.Log("Movement Check");
        return moveAmount;
    }

    // If there is a entity on the tile, check for its type and return a boolean that determines if that tile is walkable
    private bool isWalkableTile(Tile tile)
    {
        bool isWalkable = true;
        if (tile.TileEnitities.Count != 0)
        {
            if (tile.ContainsEntityTag("Obstacle"))
                isWalkable = false;

            else if (tile.ContainsEntityTag("Portal"))
                if ((int)tile.GetEntityByTag("Portal").GetComponent<Portal>().acceptedType == (int)_EntityType.myType)
                {
                    moveAmount = moveDistance-1;
                    Debug.Log("At Portal");
                }
        }
        return isWalkable;
    }

    // public method to perform Move action
    public void Move(Vector3 direction)
    {
        if (!isMoving)
        {
            moveAmount = 0;
            StartCoroutine(Move(transform, direction, GetMoveAmount(direction, currentTile), moveDuration));
        }
    }

    IEnumerator Move(Transform source, Vector3 direction, int distance, float overTime)
    {
        isMoving = true;
        //transform.rotation = Quaternion.LookRotation(direction);
        Vector3 newPosition = source.position + direction * distance;
        float startTime = Time.time;
        while (Time.time < startTime + overTime)
        {
            source.position = Vector3.Lerp(source.position, newPosition, (Time.time - startTime) / overTime);
            if (source.position == newPosition)
                break;
            yield return null;
        }
        source.position = newPosition;
        UpdatePosition();
        isMoving = false;
        //Debug.Log(gameObject.name + " finished move in " + Time.time + " seconds");
        // Send an event to update turn
        if (OnAction != null)
            OnAction();
    }

    // Accessors and Mutators
    public GameObject CurrentTile
    {
        get { return currentTile; }
        set { currentTile = value; }
    }
    public bool IsMoving
    {
        get { return isMoving; }
        set { isMoving = value; }
    }
    public int GetMoveDistance
    {
        get { return moveDistance; }
        set { moveDistance = value; }
    }
}