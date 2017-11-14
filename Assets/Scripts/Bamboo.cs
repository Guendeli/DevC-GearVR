using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Bamboo : MonoBehaviour {

    public float uMinOuter = 0; 
    public float uMaxOuter = 0.2f;

    public float vMinBump = 0.1f;
    public float vMidBump = 0.2f;
    public float vMaxBump = 0.3f;
    public float vMaxOuter = 1f;

    public float uMinInner = 0; 
    public float uMaxInner = 0.2f;
    public float vMinInner = 0;
    public float vMaxInner = 0.1f;


    public int NumberOfSegments = 5;
    public int PanelsPerSegment = 12;
    public float SegmentHeight = 1f;
    public float BumpHeight = 0.2f;
    public float BumpRadius = 1.1f;
    public float OuterRadius = 1f;
    public float InnerRadius = 0.8f;

    public float InitialMass = 10f;


    public List<Vector4> DownCuts = new List<Vector4>();
    public List<Vector4> UpCuts = new List<Vector4>();


    private Vector3[] Vertices;
    private int[] Triangles;
    private Vector3[] Normals;
    private Vector2[] UVs;


    private Mesh Mesh;
    private Material Material;
    private Mesh ColliderMesh;

    private Vector3[] ColliderVertices;
    private int[] ColliderTriangles;


    private List<Bamboo> Children = new List<Bamboo>();

    public float LastCutTime = -1;

    private enum SegmentMeshGroup
    {
        BumpBottom,
        BumpMid,
        BumpTop,
        OuterSegmentTop,
        InnerSegmentTop,
        InnerSegmentBottom,

        Count
    }

    private enum CapMeshGroup
    {
        TopOutside,
        TopInside,
        BottomInside,
        BottomOutside,

        Count
    }

    private void DestroyChildren()
    {
        foreach (var child in Children)
        {
            child.DestroyChildren();
            Destroy(child.gameObject);
        }
        Children.Clear();
    }

    public void Reset()
    {
        DestroyChildren();
        UpCuts.Clear();
        DownCuts.Clear();
        GenerateMeshes();
    }

    private void Awake()
    {
        int count = (int)SegmentMeshGroup.Count;
        int capCount = (int)CapMeshGroup.Count;

        Vertices = new Vector3[(NumberOfSegments * count + capCount) * (PanelsPerSegment + 1)];
        Triangles = new int[(NumberOfSegments * count + capCount) * PanelsPerSegment * 6];
        Normals = new Vector3[(NumberOfSegments * count + capCount) * (PanelsPerSegment + 1)];
        UVs = new Vector2[(NumberOfSegments * count + capCount) * (PanelsPerSegment + 1)];

        Mesh = new Mesh() { hideFlags = HideFlags.DontSave, name = "Bamboo Mesh" };

        ColliderVertices = new Vector3[PanelsPerSegment * 2 + 2];
        ColliderTriangles = new int[PanelsPerSegment * 12];

        ColliderMesh = new Mesh() { hideFlags = HideFlags.DontSave, name = "Bamboo Collision Mesh" };

        GetComponent<MeshFilter>().sharedMesh = Mesh;
    }

    private void Start()
    {
        GenerateMeshes();
    }
       
    private void GenerateMeshes()
    {
        GenerateMesh(UpCuts, DownCuts);
        GenerateColliderMesh(UpCuts, DownCuts);
    }

    private bool GetNextI(int i, out int nextI)
    {
        nextI = i + 1;
        if (i < NumberOfSegments * (int)SegmentMeshGroup.Count)
        {
            int segmentIndex = i / (int)SegmentMeshGroup.Count;
            SegmentMeshGroup groupIndex = (SegmentMeshGroup)(i % (int)SegmentMeshGroup.Count);

            switch (groupIndex)
            {
                case SegmentMeshGroup.BumpBottom:
                    return true;
                case SegmentMeshGroup.BumpMid:
                    return true;
                case SegmentMeshGroup.BumpTop:
                    return true;
                case SegmentMeshGroup.OuterSegmentTop:
                    if (segmentIndex < NumberOfSegments - 1)
                    {
                        nextI = i + 3;
                        return true;
                    }
                    return false;
                case SegmentMeshGroup.InnerSegmentTop:
                    if (segmentIndex > 1)
                    {
                        nextI = i - 5;
                        return true;
                    }
                    return false;
                case SegmentMeshGroup.InnerSegmentBottom:       
                    nextI = i - 1;
                    return true;
                    
            }
        }
        return false;
    }

    private bool GetPreviousI(int i, out int prevI)
    {
        prevI = i - 1;
        if (i < NumberOfSegments * (int)SegmentMeshGroup.Count)
        {
            int segmentIndex = i / (int)SegmentMeshGroup.Count;
            SegmentMeshGroup groupIndex = (SegmentMeshGroup)(i % (int)SegmentMeshGroup.Count);

            switch (groupIndex)
            {
                case SegmentMeshGroup.BumpBottom:
                    if (segmentIndex > 1)
                    {
                        prevI = i - 3;
                        return true;
                    }
                    return false;
                case SegmentMeshGroup.BumpMid:
                    return true;
                case SegmentMeshGroup.BumpTop:
                    return true;
                case SegmentMeshGroup.OuterSegmentTop:                    
                    return true;
                case SegmentMeshGroup.InnerSegmentTop:
                    prevI = i + 1;
                    return true;
                case SegmentMeshGroup.InnerSegmentBottom:
                    if (segmentIndex < NumberOfSegments - 1)
                    {
                        prevI = i + 5;
                        return true;
                    }
                    return false;

            }
        }
        return false;
    }

    private void GetParams(int i, out float offset, out float radius, out bool shouldAddTris, out float uMin, out float uMax, out float v, out float normalUp, out float normalDown, out float normalOut)
    {
        offset = 0;
        radius = OuterRadius;
        shouldAddTris = true;

        uMin = uMinOuter;
        uMax = uMaxOuter;
        v = 0;

        normalUp = 0;
        normalDown = 0;
        normalOut = 1;

        if (i < NumberOfSegments * (int)SegmentMeshGroup.Count)
        {
            int segmentIndex = i / (int)SegmentMeshGroup.Count;
            SegmentMeshGroup groupIndex = (SegmentMeshGroup)(i % (int)SegmentMeshGroup.Count);

            offset = segmentIndex * SegmentHeight;

            switch (groupIndex)
            {
                case SegmentMeshGroup.BumpBottom:
                    v = vMinBump;
                    break;
                case SegmentMeshGroup.BumpMid:
                    v = vMidBump;
                    offset += BumpHeight / 2;
                    radius = BumpRadius;
                    break;
                case SegmentMeshGroup.BumpTop:
                    v = vMaxBump;
                    offset += BumpHeight;
                    break;
                case SegmentMeshGroup.OuterSegmentTop:
                    v = vMaxOuter;
                    offset += SegmentHeight;
                    shouldAddTris = false;
                    break;
                case SegmentMeshGroup.InnerSegmentTop:
                    uMin = uMinInner;
                    uMax = uMaxInner;
                    v = vMaxInner;
                    offset += SegmentHeight;
                    radius = InnerRadius;
                    normalOut = -1;
                    break;
                case SegmentMeshGroup.InnerSegmentBottom:
                    uMin = uMinInner;
                    uMax = uMaxInner;
                    v = vMinInner;
                    radius = InnerRadius;
                    shouldAddTris = false;
                    normalOut = -1;
                    break;
            }
        }
        else
        {
            CapMeshGroup groupIndex = (CapMeshGroup)(i - NumberOfSegments * (int)SegmentMeshGroup.Count);
            uMin = uMinInner;
            uMax = uMaxInner;
            v = vMinInner;
            normalOut = 0;

            switch (groupIndex)
            {
                case CapMeshGroup.TopOutside:
                    v = vMaxInner;
                    offset = NumberOfSegments * SegmentHeight;
                    normalUp = 1;
                    break;
                case CapMeshGroup.TopInside:
                    offset = NumberOfSegments * SegmentHeight;
                    radius = InnerRadius;
                    normalUp = 1;
                    shouldAddTris = false;
                    break;
                case CapMeshGroup.BottomInside:
                    normalDown = 1;
                    radius = InnerRadius;
                    break;
                case CapMeshGroup.BottomOutside:
                    normalDown = 1;
                    v = vMaxInner;
                    shouldAddTris = false;
                    break;
            }
        }
    }

    private void GenerateMesh(List<Vector4> upCuts, List<Vector4> downCuts)
    { 
        float wedgeAngle = 2 * Mathf.PI / PanelsPerSegment;

        int tIndex = 0;
        for(int i = 0; i < NumberOfSegments * (int)SegmentMeshGroup.Count + (int)CapMeshGroup.Count; i++)
        {
            float offset = 0;
            float radius = OuterRadius;
            bool shouldAddTris = true;

            float uMin = uMinOuter;
            float uMax = uMaxOuter;
            float v = 0;

            float normalUp = 0;
            float normalDown = 0;
            float normalOut = 1;

            GetParams(i, out offset, out radius, out shouldAddTris, out uMin, out uMax, out v, out normalUp, out normalDown, out normalOut);

            Vector3 center = Vector3.up * offset;

            for(int j = 0; j <= PanelsPerSegment; j++)
            {
                float sinw = Mathf.Sin(wedgeAngle * j);
                float cosw = Mathf.Cos(wedgeAngle * j);

                Vector3 outVector = new Vector3(cosw, 0, sinw);

                int index = i * (PanelsPerSegment + 1) + j;

                Vector3 intendedPosition = center + outVector * radius;
                Vector3 maximumPosition = center + outVector * OuterRadius;

                float finalV = v;


                float finalMinY = 0;
                float finalMaxY = NumberOfSegments * SegmentHeight;

                float finalOutsideMinY = 0;
                float finalOutsideMaxY = NumberOfSegments * SegmentHeight;

                Vector3 upNormal = Vector3.up;
                Vector3 downNormal = Vector3.down;

                float finalTopCenter = NumberOfSegments * SegmentHeight;
                float finalBottomCenter = 0;
                for (int k = 0; k < downCuts.Count; k++)
                {
                    var downCut = downCuts[k];
                    float minY = downCut.w - ((intendedPosition.x) * downCut.x + (intendedPosition.z) * downCut.z) / downCut.y;
                    float outsideMinY = downCut.w - ((maximumPosition.x) * downCut.x + (maximumPosition.z) * downCut.z) / downCut.y;

                    if (minY > finalMinY)
                    {
                        finalMinY = minY;
                        finalOutsideMinY = outsideMinY;
                        downNormal = downCut;
                        finalBottomCenter = downCut.w;

                    }
                }

                for(int k = 0; k < upCuts.Count; k++)
                {
                    var upCut = upCuts[k];
                    float maxY = upCut.w - ((intendedPosition.x) * upCut.x + (intendedPosition.z) * upCut.z) / upCut.y;
                    float outsideMaxY = upCut.w - ((maximumPosition.x) * upCut.x + (maximumPosition.z) * upCut.z) / upCut.y;

                    if (maxY < finalMaxY)
                    {
                        finalMaxY = maxY;
                        finalOutsideMaxY = outsideMaxY;
                        upNormal = upCut;
                        finalTopCenter = upCut.w;
                    }
                }

                if (radius > OuterRadius && finalOutsideMaxY <= finalOutsideMinY)
                {
                    intendedPosition = maximumPosition;
                    finalMaxY = finalOutsideMaxY;
                    finalMinY = finalOutsideMinY;
                    radius = OuterRadius;

                }

                if (finalMaxY <= finalMinY)
                {
                    // Find collision point
                    //     finalTopCenter |\ /| finalMaxY
                    //                    | X |
                    //  finalBottomCenter |/ \| finalMinY

                    // finalTopCenter * (1 - x) + finalMinY * x == finalBottomCenter * (1 - x) * finalMaxY
                    // (finalTopCenter - finalBottomCenter) * (1 - x) + (finalMinY - finalMaxY) == 0
                    // (finalTopCenter - finalBottomCenter) + (finalMinY + finalBottomCenter - finalMaxY - finalTopCenter) * x == 0

                    var x = (finalBottomCenter - finalTopCenter) / (finalMaxY + finalBottomCenter - finalMinY - finalTopCenter);

                    if (finalBottomCenter == finalTopCenter)
                        x = 0;

                    var height = Mathf.Lerp(finalTopCenter, finalMaxY, x);
                    intendedPosition = outVector * x * radius + Vector3.up * height;

                    if (x * radius < InnerRadius)
                    {
                        var lineDir = Vector3.Cross(upNormal, downNormal).normalized;

                        // parameterized formula
                        // a = l.x * l.x + l.z * l.z
                        // b = 2 * l.x * p.x + 2 * l.z * p.z
                        // c = p.x * p.x + p.z * p.z - radius * radius

                        float a = lineDir.x * lineDir.x + lineDir.z * lineDir.z;
                        float b = 2 * lineDir.x * intendedPosition.x + 2 * lineDir.z * intendedPosition.z;
                        float c = intendedPosition.x * intendedPosition.x + intendedPosition.z * intendedPosition.z - radius * radius;


                        float root1 = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
                        float root2 = (-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);


                        if (Mathf.Abs(root1) < Mathf.Abs(root2))
                        {
                            intendedPosition += root1 * lineDir;
                        }
                        else if (Mathf.Abs(root2) < Mathf.Abs(root1))
                        {
                            intendedPosition += root2 * lineDir;
                        }
                        else
                        {
                            var pos = intendedPosition + root1 * lineDir;
                            pos.y = 0;

                            if (Vector3.Dot(outVector, pos) > 0)
                            {
                                intendedPosition += root1 * lineDir;
                            }
                            else
                            {
                                intendedPosition += root2 * lineDir;
                            }
                        }
                    }
                }                
                else if(intendedPosition.y <= finalMinY)
                {
                    if(radius > OuterRadius)
                    {
                        intendedPosition = maximumPosition;
                        finalMinY = finalOutsideMinY;

                    }
                    intendedPosition.y = finalMinY;
                }                  
                else if (intendedPosition.y >= finalMaxY)
                {
                    if (radius > OuterRadius)
                    {
                        intendedPosition = maximumPosition;
                        finalMaxY = finalOutsideMaxY;
                    }

                    intendedPosition.y = finalMaxY;
                }

                if(intendedPosition.y > offset)
                {
                    int nextI;

                    if(GetNextI(i, out nextI))
                    {                   
                        float nextOffset, nextV;
                        float _a, _c, _d, _e, _f, _g;
                        bool _b;
                        GetParams(nextI, out nextOffset, out _a, out _b, out _c, out _d, out nextV, out _e, out _f, out _g);

                        finalV = Mathf.Lerp(v, nextV, Mathf.InverseLerp(offset, nextOffset, intendedPosition.y));
                    }
                }
                else if(intendedPosition.y < offset)
                {
                    int prevI;

                    if(GetPreviousI(i, out prevI))
                    {                   
                        float prevOffset, prevV;
                        float _a, _c, _d, _e, _f, _g;
                        bool _b;
                        GetParams(prevI, out prevOffset, out _a, out _b, out _c, out _d, out prevV, out _e, out _f, out _g);

                        finalV = Mathf.Lerp(prevV, v, Mathf.InverseLerp(prevOffset, offset, intendedPosition.y));
                    }
                }

                Vertices[index] = intendedPosition;

                Normals[index] = outVector.normalized * normalOut + upNormal * normalUp + downNormal * normalDown;
                UVs[index] = new Vector2(Mathf.Lerp(uMin, uMax, j / (float)PanelsPerSegment), finalV);

                if (shouldAddTris && j != PanelsPerSegment)
                {
                    Triangles[tIndex++] = index;
                    Triangles[tIndex++] = (i + 1) * (PanelsPerSegment + 1) + j;
                    Triangles[tIndex++] = i * (PanelsPerSegment + 1) + (j + 1);

                    Triangles[tIndex++] = i * (PanelsPerSegment + 1) + (j + 1);
                    Triangles[tIndex++] = (i + 1) * (PanelsPerSegment + 1) + j;
                    Triangles[tIndex++] = (i + 1) * (PanelsPerSegment + 1) + (j + 1);
                }
            }
        }

        Mesh.vertices = Vertices;
        Mesh.triangles = Triangles;
        Mesh.uv = UVs;
        Mesh.normals = Normals;

        Mesh.UploadMeshData(false);

    }
        
    private void GenerateColliderMesh(List<Vector4> upCuts, List<Vector4> downCuts)
    {
        float wedgeAngle = 2 * Mathf.PI / PanelsPerSegment;
        int tIndex = 0;

        float finalBottom = 0;
        float finalTop = NumberOfSegments * SegmentHeight;

        for (int i = 0; i < downCuts.Count; i++)
        {
            finalBottom = Mathf.Max(downCuts[i].w, finalBottom);
        }

        for(int i = 0; i < upCuts.Count; i++)
        {
            finalTop = Mathf.Min(upCuts[i].w, finalTop);
        }

        if (finalTop < finalBottom)
        {
            // this is a degenerative case.
        }

        int topCenterIndex = PanelsPerSegment * 2;
        int bottomCenterIndex = PanelsPerSegment * 2 + 1;
        ColliderVertices[topCenterIndex] = Vector3.up * finalTop;
        ColliderVertices[bottomCenterIndex] = Vector3.up * finalBottom;

        for(int j = 0; j < PanelsPerSegment; j++)
        {
            float sinw = Mathf.Sin(wedgeAngle * j);
            float cosw = Mathf.Cos(wedgeAngle * j);

            Vector3 outVector = new Vector3(cosw, 0, sinw) * OuterRadius;

            float finalMinY = 0;
            float finalMaxY = NumberOfSegments * SegmentHeight;

            float finalBottomCenter = 0;
            float finalTopCenter = NumberOfSegments * SegmentHeight;


            for (int i = 0; i < downCuts.Count; i++)
            {
                var downCut = downCuts[i];
                float minY = downCut.w - ((outVector.x) * downCut.x + (outVector.z) * downCut.z) / downCut.y;

                if (minY > finalMinY)
                {
                    finalMinY = minY;
                    finalBottomCenter = downCut.w;
                }
            }

            for(int i = 0; i < upCuts.Count; i++)
            {
                var upCut = upCuts[i];
                float maxY = upCut.w - ((outVector.x) * upCut.x + (outVector.z) * upCut.z) / upCut.y;

                if (maxY < finalMaxY)
                {
                    finalMaxY = maxY;
                    finalTopCenter = upCut.w;
                }
            }

            Vector3 top = outVector + finalMaxY * Vector3.up;
            Vector3 bottom = outVector + finalMinY * Vector3.up;

            //check if finalMinY > finalMaxY
            if (finalMinY > finalMaxY)
            {                
                if (finalTopCenter != finalBottomCenter)
                {
                    // do some math to figure out new top and bottom.
                    var x = (finalBottomCenter - finalTopCenter) / (finalMaxY + finalBottomCenter - finalMinY - finalTopCenter);

                    var height = Mathf.Lerp(finalTopCenter, finalMaxY, x);
                    top = bottom = outVector * x + Vector3.up * height;
                }
                else
                {
                    top = bottom = Vector3.up * finalTopCenter;
                }
            }

            ColliderVertices[j] = top;
            ColliderVertices[j + PanelsPerSegment] = bottom;

            ColliderTriangles[tIndex++] = j;
            ColliderTriangles[tIndex++] = (j + 1) % PanelsPerSegment + PanelsPerSegment;
            ColliderTriangles[tIndex++] = j + PanelsPerSegment;

            ColliderTriangles[tIndex++] = j;
            ColliderTriangles[tIndex++] = (j + 1) % PanelsPerSegment;
            ColliderTriangles[tIndex++] = (j + 1) % PanelsPerSegment + PanelsPerSegment;

            ColliderTriangles[tIndex++] = j;
            ColliderTriangles[tIndex++] = topCenterIndex;
            ColliderTriangles[tIndex++] = (j + 1) % PanelsPerSegment;


            ColliderTriangles[tIndex++] = (j + 1) % PanelsPerSegment + PanelsPerSegment;
            ColliderTriangles[tIndex++] = bottomCenterIndex;
            ColliderTriangles[tIndex++] = j + PanelsPerSegment;

        }

        ColliderMesh.vertices = ColliderVertices;
        ColliderMesh.triangles = ColliderTriangles;

        ColliderMesh.UploadMeshData(false);
        GetComponent<MeshCollider>().sharedMesh = ColliderMesh;

        GetComponent<Rigidbody>().mass = (finalTop - finalBottom) / (SegmentHeight * NumberOfSegments);
    }
        
    private void Update()
    {
#if UNITY_EDITOR && DEBUG_GENERATE_MESH
        GenerateMeshes();

        for(int i = 0; i < UpCuts.Count; i++)
        {
            Debug.DrawLine(transform.TransformPoint(new Vector3(0, UpCuts[i].w, 0)), transform.TransformPoint(new Vector3(0, UpCuts[i].w, 0)) + transform.TransformDirection(UpCuts[i]) * 3, Color.red);
        }

        for (int i = 0; i < DownCuts.Count; i++)
        {
            Debug.DrawLine(transform.TransformPoint(new Vector3(0, DownCuts[i].w, 0)), transform.TransformPoint(new Vector3(0, DownCuts[i].w, 0)) + transform.TransformDirection(DownCuts[i]) * 3, Color.red);
        }

#endif
    }


    public void Split(Vector4 upCut)
    {
        float finalBottom = 0;
        float finalTop = NumberOfSegments * SegmentHeight;

        for (int i = 0; i < DownCuts.Count; i++)
        {
            finalBottom = Mathf.Max(DownCuts[i].w, finalBottom);
        }

        for (int i = 0; i < UpCuts.Count; i++)
        {
            finalTop = Mathf.Min(UpCuts[i].w, finalTop);
        }

        // Too small of a piece
        if (upCut.w < finalBottom + 0.01f)
        {
            return;
        }

        // Too small of a piece
        if (upCut.w > finalTop - 0.01f)
        {
            return;
        }

        var copy = Instantiate(this);

        Children.Add(copy);

        var rb = copy.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;

        var downCut = new Vector4(-upCut.x, -upCut.y, -upCut.z, upCut.w);
        copy.DownCuts.Add(downCut);

        UpCuts.Add(upCut);

        GenerateMeshes();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody && collision.rigidbody.GetComponent<Shuriken>())
        {
            if (collision.relativeVelocity.magnitude > 7)
            {
                if(Time.time - 0.5f < LastCutTime)
                {
                    return;
                }

                LastCutTime = Time.time;

                var firstContact = collision.contacts[0].point;

                var normal = collision.rigidbody.rotation * Vector3.back;

                // put collision in local space.
                firstContact = transform.InverseTransformPoint(firstContact);
                normal = transform.InverseTransformDirection(normal);

                if (normal.y != 0)
                {
                    float y = firstContact.y + ((-firstContact.x) * normal.x + (-firstContact.z) * normal.z) / normal.y;

                    var cut = new Vector4(normal.x, normal.y, normal.z, y);
                    Split(cut);
                }
            }
        }
    }
}
