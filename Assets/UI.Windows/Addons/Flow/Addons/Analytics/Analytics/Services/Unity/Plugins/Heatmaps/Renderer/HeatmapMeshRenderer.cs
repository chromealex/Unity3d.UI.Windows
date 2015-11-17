/// <summary>
/// Heat map mesh renderer.
/// </summary>
/// This is the default renderer that comes with the Heat Maps package.
/// It procedurally constructs a mesh to display Heat Map data. You
/// might consider writing your own renderer. If you do, we recommend
/// following the defined IHeatmapRenderer interface.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityAnalyticsHeatmap;
using UnityEngine;

public class HeatmapMeshRenderer : MonoBehaviour, IHeatmapRenderer
{

    const int k_NotRendering = 0;
    const int k_BeginRenderer = 1;
    const int k_RenderInProgress = 2;
    const int k_UpdateMaterials = 4;

    // Unity limit of vectors per mesh
    const int k_VerticesPerMesh = 65000;

    // Density Thresholds
    float m_HighThreshold;
    float m_LowThreshold;

    // Time limits
    float m_StartTime = 0f;
    float m_EndTime = 1.0f;

    [Range(0.1f, 100f)]
    float m_ParticleSize = 1.0f;
    int m_CurrentResolution;

    // Particle Rendering Data
    HeatPoint[] m_Data;
    float m_MaxDensity = 0f;

    RenderShape m_RenderStyle = RenderShape.Cube;
    RenderDirection m_RenderDirection = RenderDirection.YZ;

    Shader m_Shader;
    public Material[] m_Materials;

    int m_RenderState = k_NotRendering;

    List<GameObject> m_GameObjects = new List<GameObject>();

    void Start()
    {
        m_Shader = Shader.Find("Heatmaps/Particles/AlphaBlend");
    }

    public void UpdatePointData(HeatPoint[] newData, float newMaxDensity)
    {
        m_Data = newData;
        m_MaxDensity = newMaxDensity;
        m_RenderState = k_BeginRenderer;
    }

    public void UpdateColors(Color[] colors)
    {
        Color newLowColor = colors[0];
        Color newMediumColor = colors[1];
        Color newHighColor = colors[2];

        if (m_Materials == null || m_Materials.Length == 0 ||
            m_Materials[0].GetColor("_TintColor") != newLowColor ||
            m_Materials[1].GetColor("_TintColor") != newMediumColor ||
            m_Materials[2].GetColor("_TintColor") != newHighColor)
        {

            m_Shader = Shader.Find("Heatmaps/Particles/AlphaBlend");
            m_Materials = new Material[3];
            m_Materials[0] = new Material(m_Shader);
            m_Materials[0].SetColor("_TintColor", newLowColor);

            m_Materials[1] = new Material(m_Shader);
            m_Materials[1].SetColor("_TintColor", newMediumColor);

            m_Materials[2] = new Material(m_Shader);
            m_Materials[2].SetColor("_TintColor", newHighColor);

            m_RenderState = k_UpdateMaterials;
        }
    }

    public void UpdateThresholds(float[] threshholds)
    {
        float newLowThreshold = threshholds[0];
        float newHighThreshold = threshholds[1];
        if (m_HighThreshold != newHighThreshold || m_LowThreshold != newLowThreshold)
        {
            m_HighThreshold = newHighThreshold;
            m_LowThreshold = newLowThreshold;
            m_RenderState = k_UpdateMaterials;
        }
    }

    public float pointSize
    {
        get
        {
            return m_ParticleSize;
        }
        set
        {
            if (m_ParticleSize != value)
            {
                m_ParticleSize = value;
                m_RenderState = k_BeginRenderer;
            }
        }
    }

    public bool allowRender{ get; set; }

    public int currentPoints { get; set; }

    public int totalPoints { get; set; }

    public void UpdateTimeLimits(float startTime, float endTime)
    {
        if (m_StartTime != startTime || m_EndTime != endTime)
        {
            m_StartTime = startTime;
            m_EndTime = endTime;
            m_RenderState = k_BeginRenderer;
        }
    }

    public void UpdateRenderStyle(RenderShape style, RenderDirection direction)
    {
        if (style != m_RenderStyle || direction != m_RenderDirection)
        {
            m_RenderDirection = direction;
            m_RenderStyle = style;
            m_RenderState = k_BeginRenderer;
        }
    }

    public void RenderHeatmap()
    {
        if (allowRender)
        {
            switch (m_RenderState)
            {
                case k_BeginRenderer:
                    m_RenderState = k_RenderInProgress;
                    CreatePoints();
                    break;
                case k_RenderInProgress:
                    if (hasData())
                    {
                        // No-op for now.
                        //UpdateRenderCycle (0, data.Length, renderMaterials);
                    }
                    break;
                case k_UpdateMaterials:
                    if (hasData())
                    {
                        int pt = 0;         // cursor that increments each time we find a point in the time range
                        int indexPt = 0;    // cursor based on pt, but returns to 0 each time we shift to a new submap
                        int currentSubmap = 0;
                        int oldSubmap = -1;
                        int verticesPerShape = GetVecticesForShape();
                        GameObject go = null;
                        Material[] materials = null;
                        for (int a = 0; a < m_Data.Length; a++)
                        {
                            if (m_Data[a].time >= m_StartTime && m_Data[a].time <= m_EndTime)
                            {

                                currentSubmap = (pt * verticesPerShape) / k_VerticesPerMesh;

                                if (currentSubmap != oldSubmap)
                                {
                                    if (go != null && materials != null)
                                    {
                                        go.GetComponent<Renderer>().materials = materials;
                                    }

                                    indexPt = 0;
                                    go = m_GameObjects[currentSubmap];
                                    materials = go.GetComponent<Renderer>().sharedMaterials;
                                }


                                materials[indexPt] = PickMaterial(m_Data[a].density / m_MaxDensity);

                                oldSubmap = currentSubmap;
                                pt++;
                                indexPt++;
                            }
                        }
                        if (go != null && materials != null)
                        {
                            go.GetComponent<Renderer>().materials = materials;
                        }

                        m_RenderState = k_NotRendering;
                    }
                    break;
            }
        }
    }

    void CreatePoints()
    {
        if (hasData())
        {
            totalPoints = m_Data.Length;
            currentPoints = 0;

            var submaps = new List<List<HeatPoint>>();
            int currentSubmap = 0;
            int verticesPerShape = GetVecticesForShape();

            for (int a = 0; a < m_Data.Length; a++)
            {
                // FILTER FOR TIME
                if (m_Data[a].time >= m_StartTime && m_Data[a].time <= m_EndTime)
                {
                    currentPoints++;
                    if (submaps.Count <= currentSubmap)
                    {
                        submaps.Add(new List<HeatPoint>());
                    }
                    submaps[currentSubmap].Add(m_Data[a]);
                    currentSubmap = (currentPoints * verticesPerShape) / k_VerticesPerMesh;
                }
            }

            if (currentPoints == 0)
            {
                m_RenderState = k_NotRendering;
                return;
            }

            int neededSubmaps = submaps.Count;
            int currentSubmaps = m_GameObjects.Count;
            int addCount = neededSubmaps - currentSubmaps;

            if (addCount > 0)
            {
                // Add submaps if we need more
                for (int a = 0; a < addCount; a++)
                {
                    int submapID = currentSubmaps + a;
                    var go = new GameObject("Submap" + submapID);
                    go.AddComponent<HeatmapSubmap>();
                    go.GetComponent<MeshFilter>().sharedMesh = new Mesh();
                    go.transform.parent = gameObject.transform;
                    m_GameObjects.Add(go);
                }
            }
            else if (addCount < 0)
            {
                // Dispose of excess submaps
                for (var a = neededSubmaps; a < currentSubmaps; a++)
                {
                    Transform trans = gameObject.transform.FindChild("Submap" + a);
                    if (trans != null)
                    {
                        trans.parent = null;
                        m_GameObjects.Remove(trans.gameObject);
                        GameObject.DestroyImmediate(trans.gameObject);
                    }
                }
            }
            //Render submaps
            for (var a = 0; a < m_GameObjects.Count; a++)
            {
                Mesh renderMesh = m_GameObjects[a].GetComponent<MeshFilter>().sharedMesh;
                renderMesh.Clear();
                renderMesh.subMeshCount = submaps[a].Count;
                RenderSubmap(m_GameObjects[a], submaps[a]);
            }
            m_RenderState = k_NotRendering;
        }
    }

    void RenderSubmap(GameObject go, List<HeatPoint> submap)
    {
		
        var allTris = new List<int[]>();
        var allVectors = new List<Vector3[]>();
        Vector3[] vector3;
        var materials = new Material[submap.Count];

        for (int a = 0; a < submap.Count; a++)
        {
            materials[a] = PickMaterial(submap[a].density / m_MaxDensity);

            Vector3 position = submap[a].position;
            Vector3 rotation = submap[a].rotation;

            switch (m_RenderStyle)
            {
                case RenderShape.Cube:
                    vector3 = AddCubeVectorsToMesh(position.x, position.y, position.z);
                    allVectors.Add(vector3);
                    allTris.Add(AddCubeTrisToMesh(a * vector3.Length));
                    break;
                case RenderShape.Arrow:
                    vector3 = AddArrowVectorsToMesh(position, rotation);
                    allVectors.Add(vector3);
                    allTris.Add(AddArrowTrisToMesh(a * vector3.Length));
                    break;
                case RenderShape.Square:
                    vector3 = AddSquareVectorsToMesh(position.x, position.y, position.z);
                    allVectors.Add(vector3);
                    allTris.Add(AddSquareTrisToMesh(a * vector3.Length));
                    break;
                case RenderShape.Triangle:
                    vector3 = AddTriVectorsToMesh(position.x, position.y, position.z);
                    allVectors.Add(vector3);
                    allTris.Add(AddTriTrisToMesh(a * vector3.Length));
                    break;
            }
        }

        Vector3[] combinedVertices = allVectors.SelectMany(x => x).ToArray<Vector3>();
        Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
        mesh.vertices = combinedVertices;
        for (int j = 0; j < allTris.Count; j++)
        {
            int[] t = allTris[j];
            mesh.SetTriangles(t, j);
        }
        go.GetComponent<Renderer>().materials = materials;
        mesh.Optimize();

    }

    Vector3[] AddCubeVectorsToMesh(float x, float y, float z)
    {
        float halfP = m_ParticleSize / 2;

        var p0 = new Vector3(x - halfP, y - halfP, z - halfP);
        var p1 = new Vector3(x + halfP, y - halfP, z - halfP);
        var p2 = new Vector3(x + halfP, y + halfP, z - halfP);
        var p3 = new Vector3(x - halfP, y + halfP, z - halfP);

        var p4 = new Vector3(x - halfP, y - halfP, z + halfP);
        var p5 = new Vector3(x + halfP, y - halfP, z + halfP);
        var p6 = new Vector3(x + halfP, y + halfP, z + halfP);
        var p7 = new Vector3(x - halfP, y + halfP, z + halfP);

        return new Vector3[] { p0, p1, p2, p3, p4, p5, p6, p7 };
    }

    // Generate a cube mesh procedurally
    int[] AddCubeTrisToMesh(int offset)
    {
        var tris = new int[]
        {
            0, 1, 2,	// bottom
            0, 2, 3,
            4, 6, 5,	// top
            4, 7, 6,
            1, 6, 2,	// right
            1, 5, 6,

            3, 4, 0,	// left
            3, 7, 4,
            2, 7, 3,	// back
            2, 6, 7,
            0, 4, 5,	// front
            0, 5, 1
        };
        for (int a = 0; a < tris.Length; a++)
        {
            tris[a] += offset;
        }
        return tris;
    }

    Vector3[] AddArrowVectorsToMesh(Vector3 position, Vector3 rotation)
    {
        float halfP = m_ParticleSize / 2f;
        float thirdP = m_ParticleSize / 3f;

        var p0 = new Vector3(0f, 0f, -halfP);
        var p1 = new Vector3(0f, 0f, 0f);
        var p2 = new Vector3(-halfP, 0f, halfP);
        var p3 = new Vector3(-halfP, 0f, thirdP);
        var p4 = new Vector3(halfP, 0f, thirdP);
        var p5 = new Vector3(halfP, 0f, halfP);

        var v = new Vector3[] { p0, p1, p2, p3, p4, p5 };

        Quaternion q = Quaternion.Euler(rotation);

        for (int a = 0; a < v.Length; a++)
        {
            Matrix4x4 m = Matrix4x4.TRS(position, q, Vector3.one);
            v[a] = m.MultiplyPoint3x4(v[a]);
        }

        return v;
    }

    //Generate a pyramid mesh procedurally
    int[] AddArrowTrisToMesh(int offset)
    {
        var tris = new int[]
        {
            0, 1, 2,	// left
            0, 2, 3,
            0, 4, 1,	// right
            1, 4, 5

        };
        for (int a = 0; a < tris.Length; a++)
        {
            tris[a] += offset;
        }
        return tris;
    }

    Vector3[] AddSquareVectorsToMesh(float x, float y, float z)
    {
        float halfP = m_ParticleSize / 2;

        Vector3 p0, p1, p2, p3;

        switch (m_RenderDirection)
        {
            case RenderDirection.YZ:
                p0 = new Vector3(x, y - halfP, z - halfP);
                p1 = new Vector3(x, y + halfP, z - halfP);
                p2 = new Vector3(x, y + halfP, z + halfP);
                p3 = new Vector3(x, y - halfP, z + halfP);
                break;

            case RenderDirection.XZ:
                p0 = new Vector3(x - halfP, y, z - halfP);
                p1 = new Vector3(x + halfP, y, z - halfP);
                p2 = new Vector3(x + halfP, y, z + halfP);
                p3 = new Vector3(x - halfP, y, z + halfP);
                break;

            default:
                p0 = new Vector3(x - halfP, y - halfP, z);
                p1 = new Vector3(x + halfP, y - halfP, z);
                p2 = new Vector3(x + halfP, y + halfP, z);
                p3 = new Vector3(x - halfP, y + halfP, z);
                break;
        }

        return new Vector3[] { p0, p1, p2, p3 };
    }

    //Generate a procedural square
    int[] AddSquareTrisToMesh(int offset)
    {
        var tris = new int[]
        {
            offset, offset + 2, offset + 1, // top
            offset, offset + 3, offset + 2
        };
        return tris;
    }

    Vector3[] AddTriVectorsToMesh(float x, float y, float z)
    {
        float halfP = m_ParticleSize / 2;

        Vector3 p0, p1, p2;

        switch (m_RenderDirection)
        {
            case RenderDirection.YZ:
                p0 = new Vector3(x, y - halfP, z - halfP);
                p1 = new Vector3(x, y, z + halfP);
                p2 = new Vector3(x, y + halfP, z - halfP);
                break;

            case RenderDirection.XZ:
                p0 = new Vector3(x - halfP, y, z - halfP);
                p1 = new Vector3(x, y, z + halfP);
                p2 = new Vector3(x + halfP, y, z - halfP);
                break;

            default:
                p0 = new Vector3(x - halfP, y - halfP, z);
                p1 = new Vector3(x, y + halfP, z);
                p2 = new Vector3(x + halfP, y - halfP, z);
                break;
        }

        return new Vector3[] { p0, p1, p2 };
    }

    //Generate a procedural tri
    int[] AddTriTrisToMesh(int offset)
    {
        var tris = new int[]
        {
            offset, offset + 2, offset + 1	// top
        };
        return tris;
    }

    bool hasData()
    {
        return m_Data != null && m_Data.Length > 0;
    }

    int GetVecticesForShape()
    {
        // Verts is the number of UNIQUE vertices in each shape
        int verts = 0;
        switch (m_RenderStyle)
        {
            case RenderShape.Cube:
                verts = 8;
                break;
            case RenderShape.Arrow:
                verts = 6;
                break;
            case RenderShape.Square:
                verts = 4;
                break;
            case RenderShape.Triangle:
                verts = 3;
                break;
        }
        return verts;
    }



    int GetTrisForShape()
    {
        int tris = 0;
        switch (m_RenderStyle)
        {
            case RenderShape.Cube:
                tris = 32;
                break;
            case RenderShape.Arrow:
                tris = 4;
                break;
            case RenderShape.Square:
                tris = 6;
                break;
            case RenderShape.Triangle:
                tris = 3;
                break;
        }
        return tris;
    }

    Material PickMaterial(float value)
    {
        int i = 1;
        if (m_Materials == null)
            return null;
        if (value > m_HighThreshold)
        {
            i = 2;
        }
        else if (value < m_LowThreshold)
        {
            i = 0;
        }
        return m_Materials[i];
    }
}
