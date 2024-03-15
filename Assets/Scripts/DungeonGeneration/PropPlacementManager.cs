using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class PropPlacementManager : MonoBehaviour
{
    [SerializeField] Room room;
    RoomData roomData;

    [SerializeField] List<Prop> propsToPlace;

    [SerializeField, Range(0, 1)]
    private float cornerPropPlacementChance = 0.5f;

    [SerializeField]
    private GameObject propPrefab, enemyPrefab;

    private void OnEnable()
    {
        RoomController.OnRoomGenFinished += ProcessRoom;
    }

    private void Awake()
    {
        roomData = room.roomData;
    }

    public void ProcessRoom()
    {
        if (roomData == null || name.Contains("Start") || name == null)
            return;
        
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

    private bool TryPlacingPropBruteForce(Prop propToPlace, List<Vector2Int> availablePositions, PlacementOriginCorner placement) 
    {
        // Try placing objects at the palcement
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

    private void PlaceCornerProps(List<Prop> cornerProps)
    {
        float tempChance = cornerPropPlacementChance;

        foreach(Vector2Int cornerTile in roomData.CornerTiles)
        {
            if (UnityEngine.Random.value < cornerPropPlacementChance)
            {
                Prop propToPlace = cornerProps[UnityEngine.Random.Range(0, cornerProps.Count)];
                PlacePropGameObjectAt(cornerTile, propToPlace);

                /*
                if (propToPlace.placeAsGroup)
                {
                    PlaceGroupObject(cornerTile, propToPlace, 2);
                } */
            }
            else
            {
                tempChance = Mathf.Clamp01(tempChance + 0.1f);
            }
        }
    }

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

        prop.transform.localPosition = (Vector2)placementPosition;
        // Adjust sprite position
        propSpriteRenderer.transform.localPosition = (Vector2)propToPlace.propSize * 0.5f + room.GetRoomCentre() + Vector2.one * 1.5f;

        // Add to StopMovement layer
        // Debug.Log("Layer voor: " + prop.layer);
        // prop.layer = 6;
        // prop.layer = LayerMask.NameToLayer("StopMovement");
        // Debug.Log("Layer na: " + prop.layer);
        // Save the prop in roomData

        roomData.PropPositions.Add(placementPosition);
        roomData.PropObjectRefrences.Add(prop);
        return prop;
    }

    private void PlaceGroupObject(Vector2Int groupOriginPos, Prop propToPlace, int searchOffset)
    {
        //
    }

    public enum PlacementOriginCorner
    { 
        BottomLeft, BottomRight, TopLeft, TopRight
    }
}
