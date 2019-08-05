using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

    // attach this script to a game object with a MeshRenderer component and a MeshFilter component.

    MeshRenderer meshRend;
    MeshFilter meshFilt;
    Mesh mesh;

    // resolution of game view
    int resWidth = 800;
    int resHeight = 500;

    // if you want to take a screenshot each iteration, set to true.
    bool takeScreenShots = false;

    void Start()
    {
        meshFilt = GetComponent<MeshFilter>();
        meshRend = GetComponent<MeshRenderer>();
        mesh = new Mesh();
        Vector3 p1 = new Vector3(-8, 0, -8);
        Vector3 p2 = new Vector3(0, 0, 5.85f);
        Vector3 p3 = new Vector3(8, 0, -8);
        Vector3 p4 = new Vector3(-8, 0, 5.85f);
        Vector3 p5 = new Vector3(8, 0, 5.85f);
        List<Vector3[]> triangles = new List<Vector3[]> { new Vector3[] {p1, p2, p3 },
                                                          new Vector3[] {p4, p2, p1 },
                                                          new Vector3[] {p5, p3, p2 }
        };

        ScapeGen(triangles, 5, 0.18f);
    }

    void CreateMesh(List<Vector3[]> triangles)
    {
        Vector3[] vertices;
        List<int> indices;
        CreateFaces(triangles, out vertices, out indices);
        mesh.vertices = vertices;
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
        meshFilt.mesh = mesh;
    }


    int? AlreadyInEdges(Vector3[] edge, List<Vector3[]> edges)
    {
        int? index = null;
        for (int i = 0;  i < edges.Count; i++)
        {
            Vector3 ePoint1 = edges[i][0];
            Vector3 ePoint2 = edges[i][1];
            Vector3 edgePoint1 = edge[0];
            Vector3 edgePoint2 = edge[1];
            if (ePoint1.x == edgePoint1.x && ePoint1.y == edgePoint1.y && ePoint1.z == edgePoint1.z
                && ePoint2.x == edgePoint2.x && ePoint2.y == edgePoint2.y && ePoint2.z == edgePoint2.z)
            {
                return i;
            }
        }
        return index;
    }

    float RandomGauss()
    {
        float u1 = Random.value*0.98f+0.01f;
        float u2 = Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                     Mathf.Sin(2.0f * Mathf.PI * u2);
        return randStdNormal;
    }


    void ScapeGen(List<Vector3[]> triangles, int detail, float roughness)
    {

        List<Vector3[]> edges = new List<Vector3[]>();
        Dictionary<int, Vector3> points = new Dictionary<int, Vector3>();
        int indexCount = 0;
        for (int i = 0; i < detail; i++)
        {
            
            if (takeScreenShots)
            {
                // create inital mesh before starting, in order to take screenshot
                CreateMesh(triangles);
                TakeScreenshot(i);
            }

            List<Vector3[]> newT = new List<Vector3[]>();
            foreach (Vector3[] t in triangles)
            {
                List<Vector3> newTrianglePoints = new List<Vector3>(t);
                int j = 0;
                int k = -2;
                while (j < 3)
                {
                    Vector3 point1 = t[j];
                    Vector3 point2 = t[(3 + k) % 3];
                    Vector3[] edge = new Vector3[] { point1, point2 };
                    Vector3[] edgeReverse = new Vector3[] { point2, point1 };
                    Vector3 midPoint = new Vector3();
                    if (AlreadyInEdges(edge, edges) == null && AlreadyInEdges(edgeReverse, edges) == null)
                    {
                        float dist = (point2 - point1).magnitude;
                        float offset = RandomGauss() * Mathf.Pow(dist* roughness, 1.5f);
                        midPoint = point1 + (point2 - point1) * 0.5f;
                        midPoint += new Vector3(0, offset, 0);
                        points[indexCount] = midPoint;
                        points[indexCount + 1] = midPoint;
                        indexCount += 2;
                        edges.Add(edge);
                        edges.Add(edgeReverse);
                    }
                    else
                    {
                        int index = (int)(AlreadyInEdges(edge, edges) != null ? AlreadyInEdges(edge, edges) : AlreadyInEdges(edgeReverse, edges));
                        midPoint = points[index];
                    }
                    newTrianglePoints.Add(midPoint);
                    k += 1;
                    j += 1;
                }
                Vector3[] t1 = new Vector3[] { newTrianglePoints[0], newTrianglePoints[3], newTrianglePoints[5] };
                Vector3[] t2 = new Vector3[] { newTrianglePoints[1], newTrianglePoints[4], newTrianglePoints[3] };
                Vector3[] t3 = new Vector3[] { newTrianglePoints[2], newTrianglePoints[5], newTrianglePoints[4] };
                Vector3[] t4 = new Vector3[] { newTrianglePoints[3], newTrianglePoints[4], newTrianglePoints[5] };
                newT.Add(t1);
                newT.Add(t2);
                newT.Add(t3);
                newT.Add(t4);
            }
            triangles = newT;
        }
        CreateMesh(triangles);
        if (takeScreenShots)
        {
            TakeScreenshot(detail);
        }
        
    }

    void CreateFaces(List<Vector3[]> triangles, out Vector3[] vertices, out List<int> indices)
    {
        Dictionary<Vector3, int> points = new Dictionary<Vector3, int>();
        int count = -1;
        indices = new List<int>();
        foreach (Vector3[] t in triangles)
        {
            foreach (Vector3 point in t)
            {
                bool condition = false;
                foreach (Vector3 p in points.Keys)
                {
                    if (p == point)
                    {
                        condition = true;
                    }
                }
                if (!condition)
                {
                    count += 1;
                    points[point] = count;
                }
                indices.Add(points[point]);
            }
        }
        vertices = new Vector3[points.Count];
        foreach (Vector3 p in points.Keys)
        {
            int i = points[p];
            vertices[i] = p;
        }
    }

    void TakeScreenshot(int i)
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        Camera.main.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        Camera.main.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string fileName = string.Format("{0}/landscapeShots/landscapeDetail{1}.png",
                                        Application.dataPath,
                                        i);
        System.IO.File.WriteAllBytes(fileName, bytes);
    }


}
