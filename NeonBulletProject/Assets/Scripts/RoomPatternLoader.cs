using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomPatternLoader
{
    public RoomPatternLoader()
    {
    }

    public struct TileInfo
    {
        public TileType type;
        public Vector2Int relativePosition;
        public int gridIndex;

        public TileInfo(int gridIndex, TileType type, Vector2Int relativePosition)
        {
            this.gridIndex = gridIndex;
            this.type = type;
            this.relativePosition = relativePosition;
        }
    }

    public bool CheckIfTileContainsWalls(Color32[] pixels, int index)
    {
        return pixels[index] != Color.black;
    }

    public Direction GetWallsFacingDirection(Vector2Int gridPos, Color32[] pixels, int gridStride)
    {
        Direction dir = new Direction();

        if (gridPos.x == 0 || CheckIfTileContainsWalls(pixels, (gridPos.x - 1) + gridPos.y * gridStride))
            dir |= Direction.Left;

        if (gridPos.y == 0 || CheckIfTileContainsWalls(pixels, gridPos.x + (gridPos.y - 1) * gridStride))
            dir |= Direction.Bottom;

        if ((gridPos.x == gridStride - 1) || CheckIfTileContainsWalls(pixels, (gridPos.x + 1) + gridPos.y * gridStride))
            dir |= Direction.Right;

        if ((gridPos.y == gridStride - 1) || CheckIfTileContainsWalls(pixels, gridPos.x + (gridPos.y + 1) * gridStride))
            dir |= Direction.Top;

        return dir;
    }

    public List<TileInfo> GetPatternInfo(string patternName)
    {
        var texture = Resources.Load("Textures/" + patternName) as Texture2D;
        var pixels = texture.GetPixels32();
        var result = new List<TileInfo>();

        for (int i = 0; i < texture.width * texture.height; i++)
        {
            Color pixelColor = pixels[i];
            TileType type = TileType.Floor;

            if (pixelColor == Color.white)
                type = TileType.Empty;
            else if (pixelColor == Color.green)
                type = TileType.PossibleEntrance;
            else if (pixelColor == Color.blue)
                type = TileType.PossibleExit;

            var relativePos = new Vector2Int((i % texture.width), (i / texture.width));
            var info = new TileInfo(i, type, relativePos);
            result.Add(info);
        }

        return result;
    }
}