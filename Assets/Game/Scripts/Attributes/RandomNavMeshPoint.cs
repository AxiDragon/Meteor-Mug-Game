using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//BY u/ccKep NOT MINE
public class RandomNavMeshPoint : MonoBehaviour
{
    private static NavMeshTriangulation nav;
    private static Mesh mesh;
    private static float totalArea;

    private void Awake()
    {
        nav = NavMesh.CalculateTriangulation();
        mesh = new Mesh();
        mesh.vertices = nav.vertices;
        mesh.triangles = nav.indices;

        totalArea = 0.0f;
        for (var i = 0; i < mesh.triangles.Length / 3; i++) totalArea += GetTriangleArea(i);
    }

    /**
     * Get a random triangle on the NavMesh
     * Steps:
     * 1. Get a random triangle on the mesh (weighted by it's area)
     * 2. Get a random point inside that triangle
     */
    public static Vector3 GetRandomPointOnNavMesh()
    {
        var triangle = GetRandomTriangleOnNavMesh();
        return GetRandomPointOnTriangle(triangle);
    }

    /**
     * Get a random triangle on the NavMesh that has connectivity to a starting point
     * Steps:
     * 1. Get a random triangle on the mesh (weighted by it's area), connected to the triangle of startingPoint
     * 2. Get a random point inside that triangle
     */
    public static Vector3 GetConnectedPointOnNavMesh(Vector3 startingPoint)
    {
        var triangle = GetRandomConnectedTriangleOnNavMesh(startingPoint);
        return GetRandomPointOnTriangle(triangle);
    }

    /**
     * Grabs a random triangle in the mesh, weighted by size so random point distribution is even
     */
    private static int GetRandomTriangleOnNavMesh()
    {
        var rnd = Random.Range(0, totalArea);
        var nTriangles = mesh.triangles.Length / 3;
        for (var i = 0; i < nTriangles; i++)
        {
            rnd -= GetTriangleArea(i);
            if (rnd <= 0)
                return i;
        }

        return 0;
    }

    /**
     * Grabs a random triangle in the mesh (connected to p), weighted by size so random point distribution is even
     */
    private static int GetRandomConnectedTriangleOnNavMesh(Vector3 p)
    {
        // Check for triangle connectivity and calculate total area of all *connected* triangles
        var nTriangles = mesh.triangles.Length / 3;
        var tArea = 0.0f;
        var connectedTriangles = new List<int>();
        var path = new NavMeshPath();
        for (var i = 0; i < nTriangles; i++)
        {
            path.ClearCorners();
            if (NavMesh.CalculatePath(p, mesh.vertices[mesh.triangles[3 * i + 0]], NavMesh.AllAreas, path))
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    tArea += GetTriangleArea(i);
                    connectedTriangles.Add(i);
                }
        }

        var rnd = Random.Range(0, tArea);

        foreach (var i in connectedTriangles)
        {
            rnd -= GetTriangleArea(i);
            if (rnd <= 0)
                return i;
        }

        return 0;
    }


    /**
     * Gets a random point on a triangle.
     * 
     * @var int idx THe triangle index in the NavMesh
     */
    private static Vector3 GetRandomPointOnTriangle(int idx)
    {
        var v = new Vector3[3];


        v[0] = mesh.vertices[mesh.triangles[3 * idx + 0]];
        v[1] = mesh.vertices[mesh.triangles[3 * idx + 1]];
        v[2] = mesh.vertices[mesh.triangles[3 * idx + 2]];

        var a = v[1] - v[0];
        var b = v[2] - v[1];
        var c = v[2] - v[0];

        // Generate a random point in the trapezoid
        var result = v[0] + Random.Range(0f, 1f) * a + Random.Range(0f, 1f) * b;

        // Barycentric coordinates on triangles
        var alpha = ((v[1].z - v[2].z) * (result.x - v[2].x) + (v[2].x - v[1].x) * (result.z - v[2].z)) /
                    ((v[1].z - v[2].z) * (v[0].x - v[2].x) + (v[2].x - v[1].x) * (v[0].z - v[2].z));
        var beta = ((v[2].z - v[0].z) * (result.x - v[2].x) + (v[0].x - v[2].x) * (result.z - v[2].z)) /
                   ((v[1].z - v[2].z) * (v[0].x - v[2].x) + (v[2].x - v[1].x) * (v[0].z - v[2].z));
        var gamma = 1.0f - alpha - beta;

        // The selected point is outside of the triangle (wrong side of the trapezoid), project it inside through the center.
        if (alpha < 0 || beta < 0 || gamma < 0)
        {
            var center = v[0] + c / 2;
            center = center - result;
            result += 2 * center;
        }

        return result;
    }

    /**
     * Helper function to calculate the area of a triangle.
     * Used as weights when selecting a random triangle so bigger triangles have a higher chance (hence yielding an even distribution of points on the entire mesh)
     * 
     * @var int idx The index of the triangle to calculate the area of
     */
    private static float GetTriangleArea(int idx)
    {
        var v = new Vector3[3];


        v[0] = mesh.vertices[mesh.triangles[3 * idx + 0]];
        v[1] = mesh.vertices[mesh.triangles[3 * idx + 1]];
        v[2] = mesh.vertices[mesh.triangles[3 * idx + 2]];

        var a = v[1] - v[0];
        var b = v[2] - v[1];
        var c = v[2] - v[0];

        var ma = a.magnitude;
        var mb = b.magnitude;
        var mc = c.magnitude;

        var area = 0f;

        var S = (ma + mb + mc) / 2;
        area = Mathf.Sqrt(S * (S - ma) * (S - mb) * (S - mc));

        return area;
    }
}