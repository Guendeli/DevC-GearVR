using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
public class FPSCounter : MonoBehaviour
{
    public int FrameCount = 60;

    public float ExpectedFps = 60;
    public float MinimumFps = 50;

    private long[] LastNFrames;

    private long LastTime;

    private int FrameIndex;

    private long RollingTotal;

    private Mesh Mesh;


    public float CharWidth = 3;
    public float CharHeight = 6;
    public float CharSpacing = 5;
    public float StrokeWidth = 0.2f;

    public Vector3 Position = new Vector3(-5, -6, 10);

    private int[] Triangles = new int[240];
    private Vector3[] Vertices = new Vector3[240];

    private int LastVertex = 0;

    private Material Material;

    private Transform Transform;

    void Awake()
    {
        LastNFrames = new long[FrameCount];

        LastTime = System.DateTime.UtcNow.Ticks;
        Mesh = new Mesh() { name = "FPS Count Mesh" };

        for(int i = 0; i < Triangles.Length; i++)
        {
            Triangles[i] = i;
        }

        GetComponent<MeshFilter>().sharedMesh = Mesh;

        Mesh.vertices = Vertices;
        Mesh.triangles = Triangles;
        Material = GetComponent<Renderer>().material;
        
        Transform = transform;
    }

	// Update is called once per frame
	void Update ()
    {
        long now = System.DateTime.UtcNow.Ticks;

        long delta = now - LastTime;

        RollingTotal += delta - LastNFrames[FrameIndex];
        
        LastNFrames[FrameIndex] = delta;

        LastTime = now;

        UpdateDisplay();

        FrameIndex = (FrameIndex + 1) % FrameCount;
	}

    void UpdateDisplay()
    {
        double fps = 10000000.0 * FrameCount / RollingTotal;

        Material.color = Color.Lerp(Color.red, Color.green, Mathf.InverseLerp(MinimumFps, ExpectedFps, (float)fps));

        // start at 100 fps
        double multiple = 100;

        int index = 0;
        bool firstDigit = true;
        for(int i = 0; i < 5; i++)
        {
            int digit = (int)(fps / multiple);

            if (!firstDigit || digit != 0)
            {
                DrawCharacter(ref index, i, (byte)(DigitMap(digit) | (multiple == 1 ? 1 << 7 : 0)));
                firstDigit = false;
            }

            fps -= digit * multiple;
            multiple /= 10;
        }

        for(int i = index; i < LastVertex; i++)
        {
            Vertices[i] = Vector3.zero;
        }
        LastVertex = index;

        Mesh.vertices = Vertices;
        Mesh.UploadMeshData(false);
    }

    void LateUpdate()
    {
        var cam = Camera.main;

        if (cam)
        {
            var camTransform = cam.transform;

            Transform.position = camTransform.position + cam.nearClipPlane * (Position.z * camTransform.forward + Position.x * camTransform.right + Position.y * camTransform.up);
            Transform.rotation = camTransform.rotation;
            Transform.localScale = 5f * cam.nearClipPlane * Vector3.one;
        }
    }


    // 7 Bar Display (+ dot):
    //        0
    //    -------- 
    //   |        |
    // 1 |        | 2
    //   |    3   |
    //    --------    
    //   |        |
    // 4 |        | 5
    //   |    6   |
    //    -------     [] 7
    private static byte DigitMap(int digit)
    {
        switch(digit)
        {
            case 0: return 1 << 0 | 1 << 1 | 1 << 2 | 1 << 4 | 1 << 5 | 1 << 6;
            case 1: return 1 << 2 | 1 << 5;
            case 2: return 1 << 0 | 1 << 2 | 1 << 3 | 1 << 4 | 1 << 6;
            case 3: return 1 << 0 | 1 << 2 | 1 << 3 | 1 << 5 | 1 << 6;
            case 4: return 1 << 1 | 1 << 2 | 1 << 3 | 1 << 5;
            case 5: return 1 << 0 | 1 << 1 | 1 << 3 | 1 << 5 | 1 << 6;
            case 6: return 1 << 0 | 1 << 1 | 1 << 3 | 1 << 4 | 1 << 5 | 1 << 6;
            case 7: return 1 << 0 | 1 << 2 | 1 << 5;
            case 8: return 1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4 | 1 << 5 | 1 << 6;
            case 9: return 1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 5;
            default: return 0;
        }
    }

    void DrawCharacter(ref int index, int offset, byte character)
    {
        // TOP
        if((character & (1 << 0)) != 0)
        {
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight - StrokeWidth, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight - StrokeWidth, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight - StrokeWidth, 0);
        }

        // MIDDLE
        if ((character & (1 << 3)) != 0)
        {
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight * 0.5f + StrokeWidth * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight * 0.5f + StrokeWidth * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight * 0.5f - StrokeWidth * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight * 0.5f + StrokeWidth * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight * 0.5f - StrokeWidth * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight * 0.5f - StrokeWidth * 0.5f, 0);
        }

        // BOTTOM
        if ((character & (1 << 6)) != 0)
        {
            Vertices[index++] = new Vector3(offset * CharSpacing, StrokeWidth, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, StrokeWidth, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, 0, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, StrokeWidth, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, 0, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, 0, 0);
        }

        // TOP LEFT
        if ((character & (1 << 1)) != 0)
        {
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + StrokeWidth, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + StrokeWidth, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + StrokeWidth, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight * 0.5f, 0);
        }

        // TOP RIGHT
        if ((character & (1 << 2)) != 0)
        {
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth - StrokeWidth, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth - StrokeWidth, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth - StrokeWidth, CharHeight * 0.5f, 0);
        }

        // BOTTOM LEFT
        if ((character & (1 << 4)) != 0)
        {
            Vertices[index++] = new Vector3(offset * CharSpacing, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + StrokeWidth, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, 0, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + StrokeWidth, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + StrokeWidth, 0, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing, 0, 0);
        }

        // BOTTOM RIGHT
        if ((character & (1 << 5)) != 0)
        {
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth - StrokeWidth, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth - StrokeWidth, 0, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, CharHeight * 0.5f, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth, 0, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth - StrokeWidth, 0, 0);
        }

        // DOT
        if ((character & (1 << 7)) != 0)
        {
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth + StrokeWidth, StrokeWidth, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth + StrokeWidth * 2, StrokeWidth, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth + StrokeWidth, 0, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth + StrokeWidth * 2, StrokeWidth, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth + StrokeWidth * 2, 0, 0);
            Vertices[index++] = new Vector3(offset * CharSpacing + CharWidth + StrokeWidth, 0, 0);
        }
    }

    


}
