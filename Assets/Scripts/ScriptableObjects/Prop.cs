using UnityEngine;

/* This ScriptableObject contains the data for each prop, like its sprite, size,
 * where it should be placed, how many should be placed, or if it's a (big) enemy.
 * This is used in the PropPlacementManager to place the prop or enemy. */
[CreateAssetMenu]
public class Prop : ScriptableObject
{
    [Header("Prop data:")]
    public Sprite propSprite;
    public Vector2Int propSize = Vector2Int.one;
    public bool isEnemy = false;
    public bool isBigEnemy = false;

    [Space, Header("Placement type:")]
    public bool corner = true;
    public bool nearwallUp = true;
    public bool nearwallDown = true;
    public bool nearwallRight = true;
    public bool nearwallLeft = true;
    public bool inner = true;
    [Min(0)]
    public int quantityMin = 1;
    [Min(1)]
    public int quantityMax = 1;
}
