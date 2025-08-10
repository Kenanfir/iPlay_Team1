using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralLightCone : MonoBehaviour
{
    [Header("Cone Settings")]
    [Tooltip("The angle of the light cone in degrees.")]
    [Range(0, 180)]
    public float coneAngle = 30f;

    [Tooltip("How far the light cone should extend.")]
    public float coneDistance = 100f;

    // Private components
    private MeshFilter meshFilter;
    private Mesh mesh;

    void Awake()
    {
        // Get the required components
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    void LateUpdate()
    {
        // Redraw the cone every frame to account for rotation
        DrawCone();
    }

    void DrawCone()
    {
        // Define the vertices of our triangle mesh
        Vector3[] vertices = new Vector3[3];

        // Vertex 0 is the tip of the cone (at the object's origin)
        vertices[0] = Vector3.zero;

        // Calculate the other two vertices based on the angle and distance
        float halfAngleRad = (coneAngle / 2f) * Mathf.Deg2Rad;
        
        // Top vertex
        Vector3 topDir = new Vector3(Mathf.Cos(halfAngleRad), Mathf.Sin(halfAngleRad), 0);
        vertices[1] = topDir * coneDistance;

        // Bottom vertex
        Vector3 bottomDir = new Vector3(Mathf.Cos(-halfAngleRad), Mathf.Sin(-halfAngleRad), 0);
        vertices[2] = bottomDir * coneDistance;

        // Define the triangles for the mesh
        int[] triangles = new int[3];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        // Clear the old mesh data and apply the new data
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Good practice, though not strictly needed for an unlit 2D mesh
    }
}
