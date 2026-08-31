using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Creates a private copy of this object's mesh with barycentric coordinates
/// stored in vertex colors. Works with MeshFilter and SkinnedMeshRenderer.
/// The imported mesh asset itself is never modified.
/// </summary>
[DisallowMultipleComponent]
public sealed class WireframeMeshData : MonoBehaviour
{
    private MeshFilter meshFilter;
    private SkinnedMeshRenderer skinnedRenderer;
    private Mesh sourceMesh;
    private Mesh generatedMesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        skinnedRenderer = GetComponent<SkinnedMeshRenderer>();

        sourceMesh = meshFilter != null
            ? meshFilter.sharedMesh
            : skinnedRenderer != null
                ? skinnedRenderer.sharedMesh
                : null;

        if (sourceMesh == null)
        {
            Debug.LogError(
                "WireframeMeshData requires a MeshFilter or " +
                "SkinnedMeshRenderer with a mesh.", this);
            enabled = false;
            return;
        }

        if (!sourceMesh.isReadable)
        {
            Debug.LogError(
                $"'{sourceMesh.name}' is not readable. Enable Read/Write in " +
                "the model's Import Settings to use WireframeMeshData.", this);
            enabled = false;
            return;
        }

        generatedMesh = BuildWireframeMesh(sourceMesh);

        if (meshFilter != null)
            meshFilter.sharedMesh = generatedMesh;
        else
            skinnedRenderer.sharedMesh = generatedMesh;
    }

    private void OnDestroy()
    {
        if (generatedMesh == null)
            return;

        if (meshFilter != null && meshFilter.sharedMesh == generatedMesh)
            meshFilter.sharedMesh = sourceMesh;

        if (skinnedRenderer != null && skinnedRenderer.sharedMesh == generatedMesh)
            skinnedRenderer.sharedMesh = sourceMesh;

        Destroy(generatedMesh);
    }

    private static Mesh BuildWireframeMesh(Mesh source)
    {
        var sourceTriangles = new int[source.subMeshCount][];
        int destinationCount = 0;

        for (int submesh = 0; submesh < source.subMeshCount; submesh++)
        {
            if (source.GetTopology(submesh) != MeshTopology.Triangles)
                continue;

            sourceTriangles[submesh] = source.GetTriangles(submesh);
            destinationCount += sourceTriangles[submesh].Length;
        }

        int[] sourceForDestination = new int[destinationCount];
        var destinationTriangles = new int[source.subMeshCount][];
        var barycentrics = new Color32[destinationCount];
        int destination = 0;

        for (int submesh = 0; submesh < source.subMeshCount; submesh++)
        {
            int[] triangles = sourceTriangles[submesh];

            if (triangles == null)
            {
                destinationTriangles[submesh] = System.Array.Empty<int>();
                continue;
            }

            destinationTriangles[submesh] = new int[triangles.Length];

            for (int corner = 0; corner < triangles.Length; corner++)
            {
                sourceForDestination[destination] = triangles[corner];
                destinationTriangles[submesh][corner] = destination;
                barycentrics[destination] = (corner % 3) switch
                {
                    0 => new Color32(255, 0, 0, 255),
                    1 => new Color32(0, 255, 0, 255),
                    _ => new Color32(0, 0, 255, 255)
                };
                destination++;
            }
        }

        Mesh result = new Mesh
        {
            name = source.name + " (Wireframe)",
            indexFormat = destinationCount > 65535
                ? IndexFormat.UInt32
                : IndexFormat.UInt16
        };

        result.vertices = Remap(source.vertices, sourceForDestination);

        if (source.normals.Length == source.vertexCount)
            result.normals = Remap(source.normals, sourceForDestination);

        if (source.tangents.Length == source.vertexCount)
            result.tangents = Remap(source.tangents, sourceForDestination);

        result.colors32 = barycentrics;

        for (int channel = 0; channel < 8; channel++)
        {
            var sourceUVs = new List<Vector4>();
            source.GetUVs(channel, sourceUVs);

            if (sourceUVs.Count == source.vertexCount)
                result.SetUVs(channel, Remap(sourceUVs, sourceForDestination));
        }

        BoneWeight[] sourceWeights = source.boneWeights;
        if (sourceWeights.Length == source.vertexCount)
        {
            result.boneWeights = Remap(sourceWeights, sourceForDestination);
            result.bindposes = source.bindposes;
        }

        result.subMeshCount = source.subMeshCount;
        for (int submesh = 0; submesh < source.subMeshCount; submesh++)
            result.SetTriangles(destinationTriangles[submesh], submesh, false);

        CopyBlendShapes(source, result, sourceForDestination);
        result.bounds = source.bounds;
        return result;
    }

    private static T[] Remap<T>(T[] source, int[] mapping)
    {
        var result = new T[mapping.Length];
        for (int i = 0; i < mapping.Length; i++)
            result[i] = source[mapping[i]];
        return result;
    }

    private static List<T> Remap<T>(List<T> source, int[] mapping)
    {
        var result = new List<T>(mapping.Length);
        for (int i = 0; i < mapping.Length; i++)
            result.Add(source[mapping[i]]);
        return result;
    }

    private static void CopyBlendShapes(
        Mesh source,
        Mesh destination,
        int[] mapping)
    {
        var deltaVertices = new Vector3[source.vertexCount];
        var deltaNormals = new Vector3[source.vertexCount];
        var deltaTangents = new Vector3[source.vertexCount];

        for (int shape = 0; shape < source.blendShapeCount; shape++)
        {
            string shapeName = source.GetBlendShapeName(shape);
            int frameCount = source.GetBlendShapeFrameCount(shape);

            for (int frame = 0; frame < frameCount; frame++)
            {
                source.GetBlendShapeFrameVertices(
                    shape,
                    frame,
                    deltaVertices,
                    deltaNormals,
                    deltaTangents);

                destination.AddBlendShapeFrame(
                    shapeName,
                    source.GetBlendShapeFrameWeight(shape, frame),
                    Remap(deltaVertices, mapping),
                    Remap(deltaNormals, mapping),
                    Remap(deltaTangents, mapping));
            }
        }
    }
}
