using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate2Quad : MonoBehaviour {

    public Mesh quadMesh;
	// Use this for initialization
	void Start () {
        MeshFilter filter = this.gameObject.AddComponent<MeshFilter>();
        Mesh quad2 = new Mesh();
        filter.mesh = quad2;


        var vertices = new Vector3[]{
            new Vector3(-0.5f, -0.5f, 0.0f),
            new Vector3( 0.5f,  0.5f, 0.0f),
            new Vector3( 0.5f, -0.5f, 0.0f),
            new Vector3(-0.5f,  0.5f, 0.0f),

            new Vector3(-0.5f, -0.5f, 0.0f),
            new Vector3( 0.5f,  0.5f, 0.0f),
            new Vector3( 0.5f, -0.5f, 0.0f),
            new Vector3(-0.5f,  0.5f, 0.0f),
        };

        var triangles = new int[]{
            0, 1, 2,    1, 0, 3,
            2 + 4, 1 + 4, 0 + 4,    3 + 4, 0 + 4, 1 + 4
        };
        var normals = new Vector3[]{
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, -1.0f),

            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 0.0f, 1.0f)
        };

        var uv = new Vector2[]{
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 1.0f)
        };

        quad2.vertices  = vertices;
        quad2.triangles = triangles;
        quad2.normals   = normals;
        quad2.uv        = uv;

        quad2.RecalculateBounds();
        quad2.RecalculateNormals();
        quad2.RecalculateTangents();


        //UnityEditor.AssetDatabase.CreateAsset(quad2, "Assets/Resources/Meshes/Quad2Faces.asset");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
