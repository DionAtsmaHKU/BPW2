using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This script places props and enemies in empty rooms.
public class PropPlacementManager : MonoBehaviour
{
    [SerializeField] Room room;
    RoomData roomData;

    [SerializeField] List<Prop> propsToPlace;

    [SerializeField, Range(0, 1)]
    private float cornerPropPlacementChance = 0.5f;

    [SerializeField]
    private GameObject propPrefab, enemyPrefab, tutorialEnemyPrefab;

    private void OnEnable()
    {
        RoomController.OnRoomGenFinished += ProcessRoom;
    }

    private void OnDestroy()
    {
        RoomController.OnRoomGenFinished -= ProcessRoom;
    }

    private void Awake()
    {
        roomData = room.roomData;
    }
    
    /* This function decides which props go on which tiles (corners, near walls,
     * or near the middle of the room) and uses PlaceCornerProps to place them. */
    public void ProcessRoom()
    {
        if (room == null || roomData == null || room.name.Contains("Start"))
            return;

        // Places tutorial enemy
        if (room != null && room.name.Contains("Tutorial"))
        {
            GameObject enemy = Instantiate(tutorialEnemyPrefab);
            room.roomData.EnemiesInRoom.Add(enemy);
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            enemyScript.homeRoom = room;
            return;
        }

        // Place props in corners
        List<Prop> cornerProps = propsToPlace.Where(x => x.corner).ToList();
        PlaceCornerProps(cornerProps);

        // Place props near left wall
        List<Prop> leftWallProps = propsToPlace
            .Where(x => x.nearwallLeft)
            .OrderByDescending(x => x.propSize.x * x.propSize.y)
            .ToList();
        PlaceProps(leftWallProps, roomData.NearWallTilesLeft, PlacementOriginCorner.BottomLeft);

        // Place props near right wall
        List<Prop> rightWallProps = propsToPlace
            .Where(x => x.nearwallRight)
            .OrderByDescending(x => x.propSize.x * x.propSize.y)
            .ToList();
        PlaceProps(rightWallProps, roomData.NearWallTilesRight, PlacementOriginCorner.TopRight);

        // Place props near top wall
        List<Prop> topWallProps = propsToPlace
            .Where(x => x.nearwallUp)
            .OrderByDescending(x => x.propSize.x * x.propSize.y)
            .ToList();
        PlaceProps(topWallProps, roomData.NearWallTilesUp, PlacementOriginCorner.TopLeft);

        // Place props near bottom wall
        List<Prop> bottomWallProps = propsToPlace
            .Where(x => x.nearwallDown)
            .OrderByDescending(x => x.propSize.x * x.propSize.y)
            .ToList();
        PlaceProps(bottomWallProps, roomData.NearWallTilesDown, PlacementOriginCorner.BottomLeft);

        // Place props on inner tiles
        List<Prop> innerProps = propsToPlace
            .Where(x => x.inner)
            .OrderByDescending(x => x.propSize.x * x.propSize.y)
            .ToList();
        PlaceProps(innerProps, roomData.InnerTiles, PlacementOriginCorner.BottomLeft);
    }

    // This function places the inner props and wall props based on the given placement, availableTiles and wallProps input.
    private void PlaceProps(List<Prop> wallProps, HashSet<Vector2Int> availableTiles, PlacementOriginCorner placement)
    {
        // Remove path positions from the initial nearWallTiles so nothing spawns there
        HashSet<Vector2Int> tempPositions = new HashSet<Vector2Int>(availableTiles);
        tempPositions.ExceptWith(roomData.Path);

        // Placing props
        foreach (Prop prop in wallProps)
        {
            int quantity = UnityEngine.Random.Range(prop.quantityMin, prop.quantityMax + 1);
            for (int i = 0; i < quantity; i++)
            {
                tempPositions.ExceptWith(roomData.PropPositions);

                // Shuffle variations
                List<Vector2Int> availablePositions = tempPositions.OrderBy(x => Guid.NewGuid()).ToList();

                if (!TryPlacingPropBruteForce(prop, availablePositions, placement))
                    break;
            }
        }
    }

    // Tries to brute-force place props at position placement according to the list of availablePositions.
    private bool TryPlacingPropBruteForce(Prop propToPlace, List<Vector2Int> availablePositions, PlacementOriginCorner placement) 
    {
        for (int i = 0; i < availablePositions.Count; i++)
        {
            Vector2Int position = availablePositions[i];
            if (roomData.PropPositions.Contains(position))
                continue;

            // Check if there is enough space to place the prop
            List<Vector2Int> freePositionsAround = TryToFitProp(propToPlace, availablePositions, position, placement);
            if (freePositionsAround.Count == propToPlace.propSize.x * propToPlace.propSize.y)
            {
                PlacePropGameObjectAt(position, propToPlace);
                
                // Add prop to the HashSet
                foreach (Vector2Int pos in freePositionsAround)
                {
                    roomData.PropPositions.Add(pos);
                }
                return true;
            }
        }
        return false;
    }

    /* This function checks whether a prop fits by checking from which corner it's placing the object,
     * and seeing if there's any props that it would overlap with, depending on the prop's size.
     * It adds the free positions to the freePositions List<Vector2Int> and returns this list. */
    
    private List<Vector2Int> TryToFitProp(Prop prop, List<Vector2Int> availablePositions, 
        Vector2Int origin, PlacementOriginCorner placement)
    {
        List<Vector2Int> freePositions = new();

        if (placement == PlacementOriginCorner.BottomLeft)
        {
            for (int xOffset = 0; xOffset < prop.propSize.x; xOffset++)
            {
                for (int yOffset = 0; yOffset < prop.propSize.y; yOffset++)
                {
                    Vector2Int tempPos = origin + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else if (placement == PlacementOriginCorner.BottomRight)
        {
            for (int xOffset = -prop.propSize.x + 1; xOffset <= 0; xOffset++)
            {
                for (int yOffset = 0; yOffset < prop.propSize.y; yOffset++)
                {
                    Vector2Int tempPos = origin + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else if (placement == PlacementOriginCorner.TopLeft)
        {
            for (int xOffset = 0; xOffset < prop.propSize.x; xOffset++)
            {
                for (int yOffset = -prop.propSize.y + 1; yOffset <= 0; yOffset++)
                {
                    Vector2Int tempPos = origin + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else
        {
            for (int xOffset = -prop.propSize.x + 1; xOffset <= 0; xOffset++)
            {
                for (int yOffset = -prop.propSize.y + 1; yOffset <= 0; yOffset++)
                {
                    Vector2Int tempPos = origin + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }

        return freePositions;
    }

    // Places props in the corners based on the cornerPropPlacementChance.
    private void PlaceCornerProps(List<Prop> cornerProps)
    {
        float tempChance = cornerPropPlacementChance;

        foreach(Vector2Int cornerTile in roomData.CornerTiles)
        {
            if (UnityEngine.Random.value < cornerPropPlacementChance)
            {
                Prop propToPlace = cornerProps[UnityEngine.Random.Range(0, cornerProps.Count)];
                PlacePropGameObjectAt(cornerTile, propToPlace);
            }
            else
            {
                tempChance = Mathf.Clamp01(tempChance + 0.1f);
            }
        }
    }

    /* This function instantiates the prop propToPlace or an enemy at the input placementPosition, 
     * giving it its sprite, collider and positio, and adding it to lists. */
    private GameObject PlacePropGameObjectAt(Vector2Int placementPosition, Prop propToPlace)
    {
        // Instantiate prop or enemy
        GameObject prop;
        if (propToPlace.isEnemy)
        {
            prop = Instantiate(enemyPrefab);
            prop.AddComponent<Enemy>();
            Enemy enemyScript = prop.GetComponent<Enemy>();
            enemyScript.homeRoom = room;

            if (propToPlace.isBigEnemy)
            {
                enemyScript.hp = 30;
                enemyScript.enemyAtt = 0;
                enemyScript.enemyDef = -2;
            }
        } else { prop = Instantiate(propPrefab); }

        SpriteRenderer propSpriteRenderer = prop.GetComponentInChildren<SpriteRenderer>();

        // Set the sprite
        propSpriteRenderer.sprite = propToPlace.propSprite;

        // Add a collider
        CapsuleCollider2D col = propSpriteRenderer.gameObject.AddComponent<CapsuleCollider2D>();
        col.offset = Vector2.zero;
        if (propToPlace.propSize.x > propToPlace.propSize.y)
            col.direction = CapsuleDirection2D.Horizontal;

        Vector2 size = new Vector2(propToPlace.propSize.x * 0.8f, propToPlace.propSize.y * 0.8f);
        col.size = size;

        // Adjust sprite position
        prop.transform.localPosition = (Vector2)placementPosition;
        propSpriteRenderer.transform.localPosition = (Vector2)propToPlace.propSize * 0.5f + room.GetRoomCentre() + Vector2.one * 1.5f;

        // Adds the prop and its position into lists.
        roomData.PropPositions.Add(placementPosition);
        roomData.PropObjectRefrences.Add(prop);
        return prop;
    }

    // Enum for the 4 diagonal directions for placement origins.
    public enum PlacementOriginCorner
    { 
        BottomLeft, BottomRight, TopLeft, TopRight
    }
}
