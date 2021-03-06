﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGen : MonoBehaviour {

    // Noise
    [SerializeField, Range(1, 8)] private int octaves = 5;
    [SerializeField] private float lacunarity = 1f;
    [SerializeField, Range(0, 1)] protected float gain = 0.391f;
    [SerializeField] private float perlinScale = 0.15f;

    [SerializeField] private Gradient gradient;

    float minTerrainHeight;
    float maxTerrainHeight;

    public Material meshMaterial;
    Mesh mesh;
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Color[] vertColours;

    public int quadsPerTile = 20;
    public int xSize = 21;  // Width of tile
    public int zSize = 21;  

    /*
        *---**---*---*
        | 4 |  5 | 6 |
        *---**---*---*
        | 1 |  2 | 3 |
        *---**---*---*
        3 boxes wide has 4 vertices
        vertex count = (xSize + 1) * (zSize+1)
    */


    // Use this for initialization
    void Start()
    {

        mf = gameObject.AddComponent<MeshFilter>(); // Container for the mesh
        mr = gameObject.AddComponent<MeshRenderer>(); // Draw
        mc = gameObject.AddComponent<MeshCollider>();
        mesh = mf.mesh;

        CreateMesh();
        UpdateMesh();
    }

    void CreateMesh()
    {
        NoiseGen noise = new NoiseGen(octaves, lacunarity, gain, perlinScale);

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i =0,  z = 0; z <= zSize; z++)
        {
            for(int x =0; x <= xSize; x++)
            {
                // Altering the y value to make uneven terrain
                float y = 4.3f * noise.GetFractalNoise(transform.position.x + x, transform.position.z + z);

                vertices[i] = new Vector3(x, y, z);
                //vertices[i] = new Vector3(x, SamplePerlin(transform.position.x + x, transform.position.z + i), z);

                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        // Creates the triangles/quad for the mesh
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                // First triangle
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                // Second triangle
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                // 2 triangles make a quad

                vert++;
                tris += 6;
            }
            vert++;
        }

        SetUvs();
        SetVertexColours();

    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        // Normals are used to calculate how lighting looks
        mesh.RecalculateNormals();
        mesh.colors = vertColours;

        mr.material = meshMaterial;
        mc.sharedMesh = mesh;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        mr.receiveShadows = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Perlin noise
    public static float SamplePerlin(float x, float y)
    {
        return ( Mathf.PerlinNoise(x * .3f, y * 0.3f) * 2f);
    }

    void SetVertexColours()
    {
        vertColours = new Color[vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float terrainHeight = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y); //gives a value between 0 and 1;
                vertColours[i] = gradient.Evaluate(terrainHeight);
                i++;
            }
        }
    }

    void SetUvs()
    {
        uvs = new Vector2[vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }
    }

    /*
    // Using Gizmos to show the vertices
    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for(int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }
    */
}
