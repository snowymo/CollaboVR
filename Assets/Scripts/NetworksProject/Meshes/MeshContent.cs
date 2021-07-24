using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshContent {

    public class MeshData {
        public int ID;
        public int subID;
        public int boardID;
        public MeshContent.SHAPE_TYPE type;
        public Color color;

        public Mesh mesh;
        public Matrix4x4 xform; // TODO thinking of sending the whole matrix at some point instead

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public Material mat;

        public Vector3[] vertices;
        public int[] triangles;

        public Vector3[] originalVertices;
        public int[] originalTriangles;
        public Vector3[] originalNormals;

        public Vector4[] tangents;
        public Vector3[] normals;
        public Vector2[] uvs;

        public MeshData returnACopy()
        {
            MeshData md = new MeshData();
            md.ID = ID;
            md.subID= subID;
            md.type= type;
            md.boardID = boardID;

            Mesh meshCopy = new Mesh();
            meshCopy.vertices = mesh.vertices;
            //meshCopy.colors = new Color[mesh.colors.Length];
            meshCopy.colors = mesh.colors;
            //meshCopy.normals = new Vector3[mesh.normals.Length];
            meshCopy.normals = mesh.normals;
            //meshCopy.triangles = new int[mesh.triangles.Length];
            meshCopy.triangles = mesh.triangles;
            
            //meshCopy.uv = new Vector2[ mesh.uv.Length];
            meshCopy.uv = mesh.uv;
            //meshCopy.tangents = new Vector4[ mesh.tangents.Length];
            meshCopy.tangents = mesh.tangents;

            md.xform= xform; // TODO thinking of sending the whole matrix at some point instead
            md.position= position;
            md.rotation= rotation;
            md.scale= scale;

            md.mat= mat;

            
            md.mesh = meshCopy;

            md.triangles= triangles;

            md.originalVertices= originalVertices;
            md.originalTriangles= originalTriangles;
            md.originalNormals= originalNormals;

            md.tangents= tangents;
            md.normals= normals;
            md.uvs= uvs;
            return md;
        }
    }

    public enum SHAPE_TYPE : byte {
        Cube,
        Cylinder,
        Disk,
        Extruded,
        OpenCylinder,
        Parametric,
        Polyhedron,
        Revolved,
        Sphere,
        Square,
        Torus
    }

    public struct Triangle {
        public int v1;
        public int v2;
        public int v3;
    }

    // Most of these functions are modified versions of Gabriel's code. Thank you!


    public static MeshData CreateCubeMesh(int assetID, int subID, bool isDynamic = false)
    {
        Vector3[] vertices = new Vector3[6 * 4];
        Vector4[] tangents = new Vector4[6 * 4];
        Vector3[] normals = new Vector3[6 * 4];
        Vector2[] uvs = new Vector2[6 * 4];
        int[] triangles = new int[6 * 2 * 3];

        Vector3[,] faces = new Vector3[6, 3] {
                { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) },
                { new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0) },
                { new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0) },
                { new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 1, 0) },
                { new Vector3(0, -1, 0), new Vector3(0, 0, -1), new Vector3(1, 0, 0) },
                { new Vector3(-1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1) },
            };

        Mesh mesh = new Mesh();
        if (isDynamic) {
            mesh.MarkDynamic();
        }

        for (int i = 0; i < 6; i++) {
            vertices[i * 4 + 0] = faces[i, 0] - faces[i, 1] - faces[i, 2];
            vertices[i * 4 + 1] = faces[i, 0] + faces[i, 1] - faces[i, 2];
            vertices[i * 4 + 2] = faces[i, 0] + faces[i, 1] + faces[i, 2];
            vertices[i * 4 + 3] = faces[i, 0] - faces[i, 1] + faces[i, 2];

            triangles[i * 6 + 0] = i * 4 + 0;
            triangles[i * 6 + 1] = i * 4 + 1;
            triangles[i * 6 + 2] = i * 4 + 2;

            triangles[i * 6 + 3] = i * 4 + 3;
            triangles[i * 6 + 4] = i * 4 + 0;
            triangles[i * 6 + 5] = i * 4 + 2;

            tangents[i * 4 + 0] = faces[i, 1];
            tangents[i * 4 + 1] = faces[i, 1];
            tangents[i * 4 + 2] = faces[i, 1];
            tangents[i * 4 + 3] = faces[i, 1];

            normals[i * 4 + 0] = faces[i, 0];
            normals[i * 4 + 1] = faces[i, 0];
            normals[i * 4 + 2] = faces[i, 0];
            normals[i * 4 + 3] = faces[i, 0];

            uvs[i * 4 + 0] = new Vector2(0, 0);
            uvs[i * 4 + 1] = new Vector2(1, 0);
            uvs[i * 4 + 2] = new Vector2(1, 1);
            uvs[i * 4 + 3] = new Vector2(0, 1);
        }

        mesh.vertices = vertices;
        mesh.tangents = tangents;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        return new MeshData {
            mesh = mesh,
            xform = Matrix4x4.identity,
            ID = assetID,
            subID = subID,
            originalVertices = null,
            originalTriangles = null,
            originalNormals = null,
            vertices = vertices,
            triangles = triangles,
            tangents = tangents,
            normals = normals,
            uvs = uvs
        };
    }

    public static MeshData CreatePolyhedronMesh(int assetID, int subID, bool isDynamic, Vector3[] vertices, int[] triangles, Vector3[] normals = default(Vector3[]), 
        Vector3 position = default(Vector3), Vector3 rotation = default(Vector3), float scale = 1.0f)
    {
        List<Vector3> finalVertexList = new List<Vector3>();
        List<Vector3> finalNormalList = new List<Vector3>();
        List<Vector4> finalTangentList = new List<Vector4>();
        List<Vector2> finalUVList = new List<Vector2>();
        int parity = 1;

        Debug.Assert((triangles.Length % 3) == 0);

        for (int t = 0; t < triangles.Length; t += 3) {
            var tri = new Triangle();
            tri.v1 = triangles[t];
            tri.v2 = triangles[t + 1];
            tri.v3 = triangles[t + 2];

            AddTriangle(ref tri, vertices, triangles, normals, parity, finalVertexList, finalNormalList,
                        finalTangentList, finalUVList);
            parity = 1 - parity;
        }

        Mesh mesh = new Mesh();
        if (isDynamic) {
            mesh.MarkDynamic();
        }



        Vector3[] _vertices = finalVertexList.ToArray();
        Vector3[] _normals = finalNormalList.ToArray();
        Vector4[] _tangents = finalTangentList.ToArray();
        Vector2[] _uvs = finalUVList.ToArray();

        mesh.vertices = _vertices;
        mesh.normals = _normals;
        mesh.tangents = _tangents;
        mesh.uv = _uvs;



        int triComponentCount = triangles.Length;
        int[] indexList = new int[triComponentCount * 2];
        for (int i = 0; i < triComponentCount; i += 1) {
            indexList[i] = i;
        }
        for (int i = 0; i < triComponentCount; i += 1) {
            indexList[triComponentCount + i] = triComponentCount - i - 1;
        }
        mesh.triangles = indexList;

        return new MeshData {
            mesh = mesh,
            xform = Matrix4x4.identity,
            ID = assetID,
            subID = subID,
            originalVertices = vertices,
            originalTriangles = triangles,
            originalNormals = normals,
            vertices = _vertices,
            triangles = indexList,
            tangents = _tangents,
            normals = _normals,
            uvs = _uvs
        };
    }

    private static void AddTriangle(ref Triangle t, Vector3[] vertices, int[] triangles, Vector3[] normals, int parity,
                             List<Vector3> vertexListToAddTo, List<Vector3> normalListToAddTo,
                             List<Vector4> tangentListToAddTo, List<Vector2> uvListToAddTo)
    {
        Vector3 a = vertices[t.v1];
        Vector3 b = vertices[t.v2];
        Vector3 c = vertices[t.v3];
        Vector4 tangent = (b - a).normalized;
        Vector3 genNormal = Vector3.Cross(tangent, c - b).normalized;

        vertexListToAddTo.Add(a);
        vertexListToAddTo.Add(b);
        vertexListToAddTo.Add(c);

        int n = normalListToAddTo.Count;

        if (normals == null) {
            normalListToAddTo.Add(genNormal);
            normalListToAddTo.Add(genNormal);
            normalListToAddTo.Add(genNormal);
        }
        else {
            normalListToAddTo.Add(normals.Length > n ? normals[n].normalized : genNormal);
            normalListToAddTo.Add(normals.Length > n + 1 ? normals[n + 1].normalized : genNormal);
            normalListToAddTo.Add(normals.Length > n + 2 ? normals[n + 2].normalized : genNormal);
        }

        tangentListToAddTo.Add(tangent);
        tangentListToAddTo.Add(tangent);
        tangentListToAddTo.Add(tangent);

        uvListToAddTo.Add(new Vector2(parity, parity));
        uvListToAddTo.Add(new Vector2(parity, 1 - parity));
        uvListToAddTo.Add(new Vector2(1 - parity, 1 - parity));
    }

    public static MeshData UpdatePolyhedronMeshData(MeshContent.MeshData meshData, Vector3[] vertices, int[] triangles, Vector3[] normals = default(Vector3[]),
    Vector3 position = default(Vector3), Vector3 rotation = default(Vector3), float scale = 1.0f)
    {
        List<Vector3> finalVertexList = new List<Vector3>();
        List<Vector3> finalNormalList = new List<Vector3>();
        List<Vector4> finalTangentList = new List<Vector4>();
        List<Vector2> finalUVList = new List<Vector2>();
        int parity = 1;

        Debug.Assert((triangles.Length % 3) == 0);

        for (int t = 0; t < triangles.Length; t += 3) {
            var tri = new Triangle();
            tri.v1 = triangles[t];
            tri.v2 = triangles[t + 1];
            tri.v3 = triangles[t + 2];

            AddTriangle(ref tri, vertices, triangles, normals, parity, finalVertexList, finalNormalList,
                        finalTangentList, finalUVList);
            parity = 1 - parity;
        }

        Vector3[] _vertices = finalVertexList.ToArray();
        Vector3[] _normals = finalNormalList.ToArray();
        Vector4[] _tangents = finalTangentList.ToArray();
        Vector2[] _uvs = finalUVList.ToArray();

        meshData.originalVertices = vertices;
        meshData.originalTriangles = triangles;
        meshData.originalNormals = normals;
        meshData.tangents = _tangents;
        meshData.uvs = _uvs;

        meshData.mesh.vertices = _vertices;
        meshData.mesh.normals = _normals;
        meshData.mesh.tangents = _tangents;
        meshData.mesh.uv = _uvs;

        int triComponentCount = triangles.Length;
        int[] indexList = new int[triComponentCount * 2];
        for (int i = 0; i < triComponentCount; i += 1) {
            indexList[i] = i;
        }
        for (int i = 0; i < triComponentCount; i += 1) {
            indexList[triComponentCount + i] = triComponentCount - i - 1;
        }
        meshData.triangles = indexList;
        meshData.mesh.triangles = indexList;

        return meshData;
    }



    


    public abstract class Parametric_Pure {
        /// <summary>
        /// The number of triangles to have along the direction of the U parameter.
        /// </summary>
        public int numU = 30;
        /// <summary>
        /// The number of triangles to have along the direction of the V parameter.
        /// </summary>
        public int numV = 30;

        /// <summary>
        /// Override this function to implement the parametric function that defines this mesh.
        /// </summary>
        /// <returns>The surface of the mesh at the given U and V parametric coordinates.</returns>
        /// <param name="u">The first parametric coordinate, varies between 0 and 1.</param>
        /// <param name="v">The second parametric coordinate, varies between 0 and 1.</param>
        public abstract Vector3 ParametricFunction(float u, float v);

        public MeshContent.MeshData CreateMesh(int assetID, int subID, bool isDynamic)
        {
            float divU = 1.0f / numU;
            float divV = 1.0f / numV;

            int numVertices = (numU + 1) * 2 * numV + numV;
            Vector3[] vertices = new Vector3[numVertices];
            Vector4[] tangents = new Vector4[numVertices];
            Vector3[] normals = new Vector3[numVertices];
            Vector2[] uvs = new Vector2[numVertices];
            int[] triangles = GenerateTriangleStripIndices(numVertices - 2);

            int currVertIndex = 0;

            Action<float, float> addVertex = (float inU, float inV) => {
                // Compute parametric vertex
                vertices[currVertIndex] = ParametricFunction(Mathf.Clamp01(inU), Mathf.Clamp01(inV));

                Vector3 uTangent = Vector3.zero;
                Vector3 vTangent = Vector3.zero;
                Vector3 normal = Vector3.zero;
                GetNormalAndTangents(inU, inV, divU, divV, out uTangent, out vTangent, out normal);

                if (normal == Vector3.zero) {
                    // We're likely at a singularity or pole in the model. 
                    normal = GetNormalAtSingularity(inU, inV, divU, divV);
                }

                tangents[currVertIndex] = uTangent;
                normals[currVertIndex] = normal;
                uvs[currVertIndex] = new Vector2(inU, inV);

                currVertIndex++;
            };

            float u = 0;
            float d = divU;
            // Zigzag across rows to form a triangle strip.
            for (int j = 0; j < numV; j++) {
                float v = j * divV;
                for (int i = 0; i <= numU; i++) {
                    addVertex(u, v);
                    addVertex(u, v + divV);
                    if (i < numU) {
                        u += d;
                    }
                    else {
                        addVertex(u, v);
                    }
                }
                d = -d;
            }

            Mesh mesh = new Mesh();
            if (isDynamic) {
                mesh.MarkDynamic();
            }

            mesh.vertices = vertices;
            mesh.tangents = tangents;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            return new MeshData {
                mesh = mesh,
                xform = Matrix4x4.identity,
                ID = assetID,
                subID = subID,
                originalVertices = vertices,
                originalTriangles = triangles,
                originalNormals = normals,
                vertices = vertices,
                triangles = triangles,
                tangents = tangents,
                normals = normals,
                uvs = uvs
            };
        }

        /// <summary>
        /// If you're at a pole in the model, like the ends of a sphere, you can't take the 
        /// cross product of the two tangents to get the normal, because varying one of the
        /// parametric directions leaves you at the same location in 3D space, leading one of
        /// the tangents to be zero when attempting to calculate them by finite differencing.
        /// To remedy this, we take the normals at 4 points near the pole and average them, to try
        /// and get an approximation of the normal.
        /// </summary>
        /// <returns>An approximation of the normal at the singularity.</returns>
        /// <param name="inU">U parametric coordinate.</param>
        /// <param name="inV">V parametric coordinate.</param>
        /// <param name="divU">The spacing between vertices along the U axis.</param>
        /// <param name="divV">The spacing between vertices along the V axis.</param>
        private Vector3 GetNormalAtSingularity(float inU, float inV, float divU, float divV)
        {
            // Take the normal at four different points nearby this vertex, then average them.
            Vector2[] nearbyPoints = {
                new Vector2(Mathf.Clamp01(inU + divU / 100), Mathf.Clamp01(inV + divV / 100)),
                new Vector2(Mathf.Clamp01(inU + divU / 100), Mathf.Clamp01(inV - divV / 100)),
                new Vector2(Mathf.Clamp01(inU - divU / 100), Mathf.Clamp01(inV - divV / 100)),
                new Vector2(Mathf.Clamp01(inU - divU / 100), Mathf.Clamp01(inV + divV / 100))
            };

            Vector3 averageNormal = Vector3.zero;
            for (int i = 0; i < nearbyPoints.Length; i++) {
                float currU = nearbyPoints[i].x;
                float currV = nearbyPoints[i].y;

                Vector3 uTangent = Vector3.zero;
                Vector3 vTangent = Vector3.zero;
                Vector3 normal = Vector3.zero;
                GetNormalAndTangents(currU, currV, divU, divV, out uTangent, out vTangent, out normal);

                averageNormal = averageNormal * i / (i + 1) + normal / (i + 1);
            }

            return averageNormal.normalized;
        }

        /// <summary>
        /// Gets the normal and tangents of the surface at the given parametric coordinates.
        /// </summary>
        /// <param name="inU">U parametric coordinate.</param>
        /// <param name="inV">V parametric coordinate.</param>
        /// <param name="divU">The spacing between vertices along the U axis.</param>
        /// <param name="divV">The spacing between vertices along the V axis.</param>
        /// <param name="uTangent">Gets set to the tangent in the u direction.</param>
        /// <param name="vTangent">Gets set to the tangent in the v direction.</param>
        /// <param name="normal">Gets set to the normal vector.</param>
        private void GetNormalAndTangents(float inU, float inV, float divU, float divV,
                                          out Vector3 uTangent, out Vector3 vTangent,
                                          out Vector3 normal)
        {
            // Approximate tangents via finite differencing
            Vector3 pu = ParametricFunction(inU + divU / 200, inV);
            Vector3 nu = ParametricFunction(inU - divU / 200, inV);
            Vector3 pv = ParametricFunction(inU, inV + divV / 200);
            Vector3 nv = ParametricFunction(inU, inV - divV / 200);

            uTangent = (pu - nu).normalized;
            vTangent = (pv - nv).normalized;
            normal = Vector3.Cross(vTangent, uTangent).normalized;
        }
    }

    /// <summary>
    /// Generates a set of indices that are suitable for assigning to the mesh.triangles array
    /// for any object that has its vertices in a triangle strip.
    /// </summary>
    /// <returns>The full list of indices for all triangles.</returns>
    /// <param name="numTris">The number of triangles you have.</param>
    public static int[] GenerateTriangleStripIndices(int numTris)
    {
        int[] tris = new int[numTris * 3];
        for (int i = 0; i < numTris; i++) {
            if (i % 2 == 0) {
                tris[i * 3] = i;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }
            else {
                tris[i * 3] = i + 1;
                tris[i * 3 + 1] = i;
                tris[i * 3 + 2] = i + 2;
            }
        }
        return tris;
    }

    /// <summary>
    /// This implements support for a mesh defined by a surface of revolution around the Z axis.
    /// </summary>
    public abstract class Revolved_Pure : Parametric_Pure {

        /// <summary>
        /// Override this function to define the shape of the revolved surface.
        /// </summary>
        /// <returns>The point on the XY plane that will be revolved around the axis.</returns>
        /// <param name="t">The parameter, which varies from 0 to 1.</param>
        public abstract Vector2 RevolutionFunction(float t);

        public override Vector3 ParametricFunction(float u, float v)
        {
            float theta = 2 * Mathf.PI * -u;
            Vector2 xz = RevolutionFunction(v);
            return new Vector3(xz.x * Mathf.Cos(theta),
                                xz.x * Mathf.Sin(theta),
                                xz.y);
        }
    }

    /// <summary>
    /// Generates a full or partial sphere.
    /// </summary>
    public class SphereMesh : Revolved_Pure {

        /// <summary>
        /// Increase this to cut away part of the sphere starting at one of the poles, allowing you
        /// to have domes, half-spheres, and so on.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float amountToCutOff = 0.0f;

        public override Vector2 RevolutionFunction(float t)
        {
            float phi = Mathf.PI * (Mathf.Max(amountToCutOff, t) - 0.5f);
            return new Vector2(Mathf.Cos(phi), Mathf.Sin(phi));
        }

        public static SphereMesh sphere = new SphereMesh();

    }
    public static MeshContent.MeshData CreateSphereMesh(int assetID, int subID, bool isDynamic, float amountToCutOff = 0.0f)
    {
        SphereMesh.sphere.amountToCutOff = amountToCutOff;

        return SphereMesh.sphere.CreateMesh(assetID, subID, isDynamic);
    }

    public class TorusMesh : Revolved_Pure {
        /// <summary>
        /// The minor radius of the torus, i.e. the radius of the "tube".
        /// </summary>
        public float radius = 0.3f;

        public override Vector2 RevolutionFunction(float t)
        {
            float phi = 2 * Mathf.PI * t;
            return new Vector2(1 - radius * Mathf.Cos(phi), -radius * Mathf.Sin(phi));
        }

        public static TorusMesh torus = new TorusMesh();
    }
    public static MeshContent.MeshData CreateTorusMesh(int assetID, int subID, bool isDynamic, float radius = 0.3f)
    {
       TorusMesh.torus.radius = radius;

        return TorusMesh.torus.CreateMesh(assetID, subID, isDynamic);
    }

    public class OpenCylinderMesh : Revolved_Pure {
        /// <summary>
        /// The number of segments around the circle used to make this cylinder.
        /// </summary>
        public int numSegments = 10;

        public override Vector2 RevolutionFunction(float t)
        {
            return new Vector2(1, 2 * t - 1);
        }

        public void Init()
        {
            numU = numSegments;
            numV = 1;
        }

        public static OpenCylinderMesh openCylinder = new OpenCylinderMesh();
    }
    public static MeshContent.MeshData CreateOpenCylinderMesh(int assetID, int subID, bool isDynamic, int nSteps = 10)
    {
        OpenCylinderMesh.openCylinder.numSegments = nSteps;

        OpenCylinderMesh.openCylinder.Init();

        return OpenCylinderMesh.openCylinder.CreateMesh(assetID, subID, isDynamic);
    }

    public class DiskMesh : Revolved_Pure {
        /// <summary>
        /// The number of triangles/wedges this disk should be built of.
        /// </summary>
        public int numSegments = 10;

        public override Vector2 RevolutionFunction(float t)
        {
            return new Vector2(t + 0.001f, 0);
        }

        public void Init()
        {
            numU = numSegments;
            numV = 1;
        }

        public static DiskMesh disk = new DiskMesh();
    }
    public static MeshContent.MeshData CreateDiskMesh(int assetID, int subID, bool isDynamic, int nSteps = 10)
    {
        DiskMesh.disk.numSegments = nSteps;

        DiskMesh.disk.Init();

        return DiskMesh.disk.CreateMesh(assetID, subID, isDynamic);
    }

    public class SquareMesh : Parametric_Pure {
        public override Vector3 ParametricFunction(float u, float v)
        {
            return new Vector3(2 * u - 1, 2 * v - 1, 0);
        }

        public static SquareMesh square = new SquareMesh();
    }
    public static MeshContent.MeshData CreateSquareMesh(int assetID, int subID, bool isDynamic)
    {
        return SquareMesh.square.CreateMesh(assetID, subID, isDynamic);
    }



    public static Dictionary<int, MeshContent.MeshData> idToMeshMap = new Dictionary<int, MeshContent.MeshData>();
    public static Dictionary<int, MeshGO> idToMeshGOMap = new Dictionary<int, MeshGO>();
    public static Dictionary<int, List<MeshContent.MeshData>> idToMeshesMap = new Dictionary<int, List<MeshContent.MeshData>>(); // TODO
    public static Queue<int> needToUpdateQ = new Queue<int>();


    public static Queue<MeshContent.MeshData> activeMeshData = new Queue<MeshContent.MeshData>();
}
