using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGen : MonoBehaviour {

    public int quadsPerTile = 10;
    //**
    // Noise
    [SerializeField, Range(1, 8)] private int octaves = 5;
    [SerializeField] private float lacunarity = 1f;
    [SerializeField, Range(0, 1)] protected float gain = 0.391f; //needs to be between 0 and 1 so that each octave contributes less to the final shape.
    [SerializeField] private float perlinScale = 0.15f;

    [SerializeField] private Gradient gradient;

    float minTerrainHeight; //bk
    float maxTerrainHeight; //bk

    protected List<Color32> vertexColours;  //ud

    public Material meshMaterial;
    Mesh mesh;
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;  //bk
    Color[] vertColours;


    public const int mapChunkSize = 21;
    public int xSize = 21;
    public int zSize = 21;

    /*
        *---**---*---*
        | 4 |  5 | 6 |
        *---**---*---*
        | 1 |  2 | 3 |
        *---**---*---*
        2 boxes wide has 4 vertices
        vertex count = (xSize + 1) * (zSize+1)
    */


    // Use this for initialization
    void Start()
    {
        //mesh = new Mesh();
        //GetComponent<MeshFilter>().mesh = mesh;

        mf = gameObject.AddComponent<MeshFilter>(); // Container for the mesh
        mr = gameObject.AddComponent<MeshRenderer>(); // Draw
        mc = gameObject.AddComponent<MeshCollider>();
        mesh = mf.mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
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

                if (y > maxTerrainHeight) //bk
                    maxTerrainHeight = y;
                if (y < minTerrainHeight) //bk
                    minTerrainHeight = y;
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        // bk
        uvs = new Vector2[vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }

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

        //SetVertexColours();

        /*
        vertices = new Vector3[]
        {
            // Creating 3 vertices to make a triangle
            new Vector3 (0,0,0),
            new Vector3 (0,0,1),
            new Vector3 (1,0,0),
            new Vector3 (1,0,1)
        };

        triangles = new int[]
        {
            0, 1, 2,
            1, 3, 2
        };
        */

    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        // Normals are used to calculate how lighting looks
        mesh.RecalculateNormals();

        mesh.colors = vertColours;  //bk

        //m.vertices = vertices;
        //m.uv = uv;
        //m.triangles = triangles;
        //m.RecalculateNormals();
        mr.material = meshMaterial;
        mc.sharedMesh = mesh;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        mr.receiveShadows = true;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for(int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
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
        float min = -10;
        float max = 15;

        float diff = max - min;

        for(int i=0; i< vertices.Length; i++)
        {
            vertexColours.Add(gradient.Evaluate((vertices[i].y - min) / diff));
        }
    }
}
