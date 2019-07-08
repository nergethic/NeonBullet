using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Flags]
public enum Direction
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8
}

public enum TileType
{
    Floor,
    PossibleEntrance,
    PossibleExit,
    Empty
}

public struct RoomGridTile
{
    public Direction wallsFacingDirections;
    public GameObject floorTile;
    public TileType type;
    public Vector2Int relativePosition;
}

struct WallChainInfo
{
    public short startIndex;
    public byte length;
    public Direction wallFacingDirection;
};