using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public partial class LevelGenerator : MonoBehaviour
{
    public Material floorMaterial;
    public Material wallMaterial;
    public GameObject grassPrefab;

    public float tileSideLength = 2.0f;
    private float wallHeight;

    private GameObject leftWall;
    private GameObject rightWall;
    private GameObject topWall;
    private GameObject bottomWall;
    private GameObject floorTile;

    private RoomGridTile[] grid;

    void Start()
    {
        int roomTileWidth = 16;
        grid = new RoomGridTile[roomTileWidth * roomTileWidth];

        wallHeight = tileSideLength * 2.0f * 1.618033f;

        leftWall = CreateWallTile(Vector3.zero, tileSideLength, Direction.Left);
        topWall = CreateWallTile(Vector3.zero, tileSideLength, Direction.Top);
        rightWall = CreateWallTile(Vector3.zero, tileSideLength, Direction.Right);
        bottomWall = CreateWallTile(Vector3.zero, tileSideLength, Direction.Bottom);
        floorTile = CreateFloorTile(Vector3.zero, tileSideLength, tileSideLength);

        Vector2 roomOrigin = new Vector2(0.0f, 0.0f);
        GenerateRoomFromPattern(roomOrigin, "pattern1");
    }

    GameObject PlaceTile(Vector3 position, GameObject parent)
    {
        var tile = Instantiate(floorTile, position, Quaternion.identity);
        tile.transform.parent = parent.transform;

        return tile;
    }

    void ProcessRoomPattern(Texture2D patternTexture, string patternName, Vector3 origin, GameObject parent)
    {
        // TODO: this function calls GetWallsFacingDirection, fills info grid, places tiles and plants grass, probably break it up into smaller pieces

        var loader = new RoomPatternLoader();
        var tilesInfo = loader.GetPatternInfo(patternName);

        foreach (var info in tilesInfo)
        {
            var i = info.gridIndex;
            grid[i].relativePosition = info.relativePosition;
            grid[i].type = info.type;

            switch (info.type)
            {
                case TileType.Floor:
                    var tileWorldPosition = new Vector3(origin.x + info.relativePosition.x * tileSideLength, 0.0f, origin.y + info.relativePosition.y * tileSideLength);
                    grid[i].floorTile = PlaceTile(tileWorldPosition, parent);
                    PlantGrassArea(tileWorldPosition, tileSideLength);
                    grid[i].wallsFacingDirections = loader.GetWallsFacingDirection(info.relativePosition, patternTexture.GetPixels32(), patternTexture.width);
                    break;

                case TileType.PossibleEntrance:
                    // TODO: gather room entry location probability
                    break;

                case TileType.PossibleExit:
                    // TODO: gather room exit location probability
                    break;
            }
        }
    }

    void GenerateRoomFromPattern(Vector2 roomOriginPosition, string patternName)
    {
        GameObject floor = new GameObject("Floor");
        MeshRenderer meshRenderer = floor.AddComponent<MeshRenderer>() as MeshRenderer;
        meshRenderer.material = floorMaterial;

        var patternTexture = Resources.Load("Textures/" + patternName) as Texture2D;

        ProcessRoomPattern(patternTexture, patternName, roomOriginPosition, floor);
        CombineMeshes(floor);

        // TODO: combine long walls
        var wallChainsInfo = GetWallsChainInfo(patternTexture.width, patternTexture.height);
        PlaceWallsWithVariations(roomOriginPosition, wallChainsInfo, patternTexture.width);
    }

    private List<WallChainInfo> GetOneDirectionWallsChainInfo(int width, int height, Direction dir)
    {
        WallChainInfo chainInfo;
        chainInfo.wallFacingDirection = dir;
        bool horizontal = (dir == Direction.Top) || (dir == Direction.Bottom);
        var wallChainsInfo = new List<WallChainInfo>();

        for (int y = 0; y < height; y++)
        {
            short wallsInChain = 0;
            bool wallChainFound = false;
            short startIndex = 0;
            int tileIndex = 0;

            Action addChainInfo = () => {
                chainInfo.startIndex = startIndex;
                chainInfo.length = (byte)wallsInChain;
                wallChainsInfo.Add(chainInfo);

                wallsInChain = 0;
                wallChainFound = false;
            };

            for (int x = 0; x < width; x++)
            {
                tileIndex = horizontal ? (x + y * width) : (y + x * width);
                var wallsFacingDirections = grid[tileIndex].wallsFacingDirections;
                bool wallSatisfiesChain = wallsFacingDirections.HasFlag(dir);

                if (wallSatisfiesChain)
                {
                    if (wallChainFound == false)
                    {
                        wallChainFound = true;
                        startIndex = (short)tileIndex;
                    }

                    wallsInChain++;
                }
                else if (wallChainFound && wallsFacingDirections == Direction.None)
                    addChainInfo();
            }

            if (wallChainFound) addChainInfo();
        }

        return wallChainsInfo;
    }

    private List<WallChainInfo> GetWallsChainInfo(int width, int height)
    {
        var result = new List<WallChainInfo>();

        foreach (var dir in (Direction[])Enum.GetValues(typeof(Direction)))
            result = result.Concat(GetOneDirectionWallsChainInfo(width, height, dir)).ToList();

        return result;
    }

    void PlaceWalls(Vector3 position, Direction directions)
    {
        if (directions == Direction.None) return;

        if (directions.HasFlag(Direction.Left))
            Instantiate(leftWall, position, Quaternion.identity);

        if (directions.HasFlag(Direction.Right))
            Instantiate(rightWall, position, Quaternion.identity);

        if (directions.HasFlag(Direction.Top))
            Instantiate(topWall, position, Quaternion.identity);

        if (directions.HasFlag(Direction.Bottom))
            Instantiate(bottomWall, position, Quaternion.identity);
    }

    void CreateWallVariations(WallChainInfo info, Vector3 position, Vector3 offset)
    {
        if (info.wallFacingDirection == Direction.Top)
        {
            CreateWallTile(position - offset + new Vector3(tileSideLength, 0.0f, tileSideLength), Math.Abs(offset.z), Direction.Left);
            CreateWallTile(position - offset - new Vector3(tileSideLength, 0.0f, -tileSideLength), Math.Abs(offset.z), Direction.Right);
        }

        if (info.wallFacingDirection == Direction.Bottom)
        {
            CreateWallTile(position + new Vector3(tileSideLength, 0.0f, 0.0f), Math.Abs(offset.z), Direction.Left);
            CreateWallTile(position - new Vector3(tileSideLength, 0.0f, 0.0f), Math.Abs(offset.z), Direction.Right);
        }

        if (info.wallFacingDirection == Direction.Left)
        {
            CreateWallTile(position + new Vector3(0, 0.0f, offset.x), Math.Abs(offset.x), Direction.Top);
            CreateWallTile(position + new Vector3(0, 0.0f, tileSideLength), Math.Abs(offset.x), Direction.Bottom);
        }

        if (info.wallFacingDirection == Direction.Right)
        {
            CreateWallTile(position + new Vector3(tileSideLength - offset.x, 0.0f, -offset.x), Math.Abs(offset.x), Direction.Top);
            CreateWallTile(position + new Vector3(tileSideLength - offset.x, 0.0f, tileSideLength), Math.Abs(offset.x), Direction.Bottom);
        }
    }

    private void PlaceWallsWithVariations(Vector3 roomOriginPosition, List<WallChainInfo> wallChainsInfo, int stride)
    {
        foreach (var chainInfo in wallChainsInfo)
        {
            bool horizontal = chainInfo.wallFacingDirection == Direction.Top || chainInfo.wallFacingDirection == Direction.Bottom;
            int ZNormalSign = (chainInfo.wallFacingDirection == Direction.Top || chainInfo.wallFacingDirection == Direction.Right) ? 1 : -1;

            for (int i = 0; i < chainInfo.length; i++)
            {
                int gridIndex = horizontal ? (chainInfo.startIndex + i) : (chainInfo.startIndex + i * stride);
                var relativePos = grid[gridIndex].relativePosition;
                var position = new Vector3(roomOriginPosition.x + relativePos.x * tileSideLength, 0.0f, roomOriginPosition.y + relativePos.y * tileSideLength);
                var offset = Vector3.zero;

                if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5)
                {
                    offset = horizontal ? new Vector3(0, 0, 1) : new Vector3(1, 0, 0);
                    offset *= ZNormalSign * UnityEngine.Random.Range(0.4f, 2.5f);
                    CreateWallVariations(chainInfo, position, offset);
                }
                    
                PlaceWalls(position - offset, chainInfo.wallFacingDirection);
            }
        }
    }

    private void PlantGrass(Vector3 position)
    {
        position.y += grassPrefab.transform.position.y;
        var angles = grassPrefab.transform.rotation.eulerAngles;
        var rotation = new Quaternion();
        angles.y = UnityEngine.Random.Range(0, 180.0f);
        rotation.eulerAngles = angles;

        Instantiate(grassPrefab, position, rotation);
    }

    private void PlantGrassArea(Vector3 originPosition, float areaWidth)
    {
        Action<Vector3> sampleAction = (samplePosition) =>
        {
            PlantGrass(samplePosition);
        };

        var ps = new PoissonSampler(sampleAction);
        ps.Sample(originPosition, areaWidth);
    }
}