using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Prop : ScriptableObject
{
    [Header("Prop data:")]
    public Sprite propSprite;
    public Vector2Int propSize = Vector2Int.one;
    public bool isEnemy = false;

    [Space, Header("Placement type:")]
    public bool corner = true;
    public bool nearwallUp = true;
    public bool nearwallDown = true;
    public bool nearwallRight = true;
    public bool nearwallLeft = true;
    public bool inner = true;
    [Min(1)]
    public int quantityMin = 1;
    [Min(1)]
    public int quantityMax = 1;

    /*
    [Space, Header("Group placement:")]
    public bool placeAsGroup = false;
    [Min(1)]
    public int minGroupCount = 1;
    [Min(1)]
    public int maxGroupCount = 1;
    8
    */
}
