using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public partial class LevelGenerator
{
    Mesh CreateMesh(Vector3[] vertices, Vector2[] uv, int[] triangles)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
    private void CombineMeshes(GameObject parent)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        for (int i = 1; i < meshFilters.Length; i++)
        {
            combine[i - 1].mesh = meshFilters[i].sharedMesh;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;
            UnityEngine.Object.Destroy(meshFilters[i].gameObject); // .SetActive(false);
        }

        MeshFilter meshFilter = parent.AddComponent<MeshFilter>() as MeshFilter;
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);
        parent.gameObject.SetActive(true);

        // (floor.AddComponent<MeshCollider>() as MeshCollider).sharedMesh = meshFilter.mesh;
    }

    GameObject CreateFloorTile(Vector3 position, float width, float height)
    {
        GameObject floorTile = new GameObject("floorTile");
        floorTile.transform.position = position;

        var vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, height),
            new Vector3(width, 0, height),
            new Vector3(width, 0, 0)
        };

        var uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };

        Mesh mesh = CreateMesh(vertices, uv, triangles: new int[] { 0, 1, 2, 2, 3, 0 });

        var meshFilter = floorTile.AddComponent<MeshFilter>() as MeshFilter;
        meshFilter.mesh = mesh;

        return floorTile;
    }

    GameObject CreateWallTile(Vector3 position, float width, Direction dir)
    {
        Vector3[] vertices = new Vector3[0];

        switch (dir)
        {
            case Direction.Left:
                vertices = new Vector3[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(0, wallHeight, 0),
                    new Vector3(0, 0, width),
                    new Vector3(0, wallHeight, width)
                };
                break;

            case Direction.Right:
                vertices = new Vector3[]
                {
                    new Vector3(tileSideLength, 0, width),
                    new Vector3(tileSideLength, wallHeight, width),
                    new Vector3(tileSideLength, 0, 0),
                    new Vector3(tileSideLength, wallHeight, 0)
                };
                break;

            case Direction.Top:
                vertices = new Vector3[]
                {
                    new Vector3(0, 0, width),
                    new Vector3(0, wallHeight, width),
                    new Vector3(width, 0, width),
                    new Vector3(width, wallHeight, width)
                };
                break;

            case Direction.Bottom:
                vertices = new Vector3[]
                {
                    new Vector3(width, 0, 0),
                    new Vector3(width, wallHeight, 0),
                    new Vector3(0, 0, 0),
                    new Vector3(0, wallHeight, 0)
                };
                break;
        }

        GameObject wallTile = new GameObject("wallTile");
        wallTile.transform.position = position;

        var uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };

        var mesh = CreateMesh(vertices, uv, triangles: new int[] { 0, 1, 3, 3, 2, 0 });

        var meshFilter = wallTile.AddComponent<MeshFilter>() as MeshFilter;
        meshFilter.mesh = mesh;

        (wallTile.AddComponent<MeshCollider>() as MeshCollider).sharedMesh = meshFilter.mesh;

        MeshRenderer meshRenderer = wallTile.AddComponent<MeshRenderer>() as MeshRenderer;
        meshRenderer.material = wallMaterial;

        return wallTile;
    }
}
