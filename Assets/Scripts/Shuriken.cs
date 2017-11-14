using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Shuriken : MonoBehaviour
{

    public float HoleRadius = 0.1f;
    public float MinRadius = 0.3f;
    public float MaxRadius = 0.6f;

    public float Thickness = 0.1f;

    public int TineCount = 4;


    private Vector3[] Vertices;
    private int[] Triangles;
    private Vector3[] Normals;
    private Vector2[] UVs;


    private Mesh Mesh;
    private Material Material;
    private Mesh ColliderMesh;

    private Vector3[] ColliderVertices;
    private int[] ColliderTriangles;
    private void Awake()
    {
        // verts =  TineCount * 2(outer + inner) * 2(sides) * 2 (far + near)
        //          + TineCount * 2 * 2;


        Vertices = new Vector3[TineCount * 4 * 3];
        Triangles = new int[TineCount * 4 * 3 * 3];
        Normals = new Vector3[TineCount * 4 * 3];
        UVs = new Vector2[TineCount * 4 * 3];

        Mesh = new Mesh() { hideFlags = HideFlags.DontSave, name = "Shuriken Mesh" };

        ColliderVertices = new Vector3[2 + TineCount];
        ColliderTriangles = new int[TineCount * 6];

        ColliderMesh = new Mesh() { hideFlags = HideFlags.DontSave, name = "Shuriken Collision Mesh" };

        GetComponent<MeshFilter>().sharedMesh = Mesh;
    }


    private void GenerateMesh()
    {
        float wedgeAngle = Mathf.PI / TineCount;

        int tIndex = 0;
        for (int i = 0; i < TineCount * 2; i++)
        {
            float sinw = Mathf.Sin(wedgeAngle * i);
            float cosw = Mathf.Cos(wedgeAngle * i);

            float length = i % 2 == 0 ? MaxRadius : MinRadius;

            Vertices[i * 6 + 0] = new Vector3(cosw * length, sinw * length, 0);
            Vertices[i * 6 + 1] = new Vector3(cosw * length, sinw * length, 0);
            Vertices[i * 6 + 2] = new Vector3(cosw * HoleRadius, sinw * HoleRadius, Thickness * 0.5f);
            Vertices[i * 6 + 3] = new Vector3(cosw * HoleRadius, sinw * HoleRadius, Thickness * -0.5f);
            Vertices[i * 6 + 4] = new Vector3(cosw * HoleRadius, sinw * HoleRadius, Thickness * 0.5f);
            Vertices[i * 6 + 5] = new Vector3(cosw * HoleRadius, sinw * HoleRadius, Thickness * -0.5f);

            Normals[i * 6 + 0] = new Vector3(cosw * 0.5f, sinw * 0.5f, length).normalized;
            Normals[i * 6 + 1] = new Vector3(cosw * 0.5f, sinw * 0.5f, -length).normalized;
            Normals[i * 6 + 2] = new Vector3(cosw * 0.5f, sinw * 0.5f, length).normalized;
            Normals[i * 6 + 3] = new Vector3(cosw * 0.5f, sinw * 0.5f, -length).normalized;
            Normals[i * 6 + 4] = new Vector3(-cosw, -sinw, 0);
            Normals[i * 6 + 5] = new Vector3(-cosw, -sinw, 0);

            UVs[i * 6 + 0] = new Vector2(i % 2, 1);
            UVs[i * 6 + 1] = new Vector2(i % 2, 1);
            UVs[i * 6 + 2] = new Vector2(i % 2, 0);
            UVs[i * 6 + 3] = new Vector2(i % 2, 0);

            UVs[i * 6 + 4] = new Vector2(i % 2, 0);
            UVs[i * 6 + 5] = new Vector2(i % 2, 0.1f);

            int nextI = (i + 1) % (TineCount * 2);

            Triangles[tIndex++] = i * 6;
            Triangles[tIndex++] = nextI * 6;
            Triangles[tIndex++] = i * 6 + 2;
            Triangles[tIndex++] = i * 6 + 2;
            Triangles[tIndex++] = nextI * 6;
            Triangles[tIndex++] = nextI * 6 + 2;

            Triangles[tIndex++] = i * 6 + 1;
            Triangles[tIndex++] = i * 6 + 3;
            Triangles[tIndex++] = nextI * 6 + 1;
            Triangles[tIndex++] = nextI * 6 + 1;
            Triangles[tIndex++] = i * 6 + 3;
            Triangles[tIndex++] = nextI * 6 + 3;

            Triangles[tIndex++] = i * 6 + 4;
            Triangles[tIndex++] = nextI * 6 + 4;
            Triangles[tIndex++] = i * 6 + 5;
            Triangles[tIndex++] = i * 6 + 5;
            Triangles[tIndex++] = nextI * 6 + 4;
            Triangles[tIndex++] = nextI * 6 + 5;
        }

        Mesh.vertices = Vertices;
        Mesh.triangles = Triangles;
        Mesh.uv = UVs;
        Mesh.normals = Normals;

        Mesh.UploadMeshData(false);
    }

    private void GenerateColliderMesh()
    {
        ColliderVertices[TineCount] = Vector3.forward * Thickness * 0.5f;
        ColliderVertices[TineCount + 1] = Vector3.forward * Thickness * -0.5f;

        float wedgeAngle = Mathf.PI * 2 / TineCount;

        int tIndex = 0;
        for (int i = 0; i < TineCount; i++)
        {
            float sinw = Mathf.Sin(wedgeAngle * i);
            float cosw = Mathf.Cos(wedgeAngle * i);

            ColliderVertices[i] = new Vector3(cosw * MaxRadius, sinw * MaxRadius, 0);

            ColliderTriangles[tIndex++] = i;
            ColliderTriangles[tIndex++] = (i + 1) % TineCount;
            ColliderTriangles[tIndex++] = TineCount;
            ColliderTriangles[tIndex++] = i;
            ColliderTriangles[tIndex++] = (i + 1) % TineCount;
            ColliderTriangles[tIndex++] = TineCount + 1;
        }

        ColliderMesh.vertices = ColliderVertices;
        ColliderMesh.triangles = ColliderTriangles;

        ColliderMesh.UploadMeshData(false);
        GetComponent<MeshCollider>().sharedMesh = ColliderMesh;
    }

    private void Start()
    {
        GenerateMeshes();
    }

    private void GenerateMeshes()
    {
        GenerateMesh();
        GenerateColliderMesh();
    }
    private void Update()
    {
#if UNITY_EDITOR && DEBUG_GENERATE_MESH
        GenerateMeshes();
#endif

        var rb = GetComponent<Rigidbody>();

        // if the shuriken is flying, apply gyroscopic effect
        if(rb.velocity.sqrMagnitude > 4)
        {
            var fwd = rb.rotation * Vector3.forward;

            var a = Vector3.Cross(rb.velocity, fwd);

            var b = Vector3.Cross(a, rb.velocity);

            var rotation = Quaternion.FromToRotation(fwd, b);

            rb.rotation = rotation * rb.rotation;
            rb.angularVelocity = rotation * rb.angularVelocity;
        }
    }
}