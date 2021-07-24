using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGO : MonoBehaviour {
    public MeshContent.MeshData meshData;

    public MeshFilter filter;
    public MeshRenderer meshRenderer;

    public bool isDup;
    public Material customizeMat;

    // test
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;

    public void Init(MeshContent.MeshData meshData)
    {
        this.meshData = meshData;
        isDup = false;

        // deal with color
        KeyValuePair<Material, Color> materialInfo;
        if (Utility.colorToMaterialInfoMap2.TryGetValue(meshData.color, out materialInfo)) {
            meshRenderer.sharedMaterial = materialInfo.Key;
            //matColor = materialInfo.Value;
        }
        else {
            Color matColor = new Color(Mathf.Pow(meshData.color.r, 0.45f), Mathf.Pow(meshData.color.g, 0.45f), Mathf.Pow(meshData.color.b, 0.45f));
            Material mat = new Material(customizeMat);
            mat.SetColor("_Color", matColor);
            meshRenderer.sharedMaterial = mat;

            Utility.colorToMaterialInfoMap2.Add(meshData.color, new KeyValuePair<Material, Color>(mat, matColor));
        }

        //Color c = new Color(Mathf.Pow(meshData.color.r, 0.45f), Mathf.Pow(meshData.color.g, 0.45f), Mathf.Pow(meshData.color.b, 0.45f));
        //customizeMat.SetColor("_Color", c);
        //meshRenderer.material = customizeMat;

        UpdateMeshDataAll();
    }

    public void UpdateMeshDataTransform()
    {
        //transform.position = meshData.position;
        //transform.rotation = Quaternion.Euler(
        //    meshData.rotation.y * Mathf.Rad2Deg, -meshData.rotation.x * Mathf.Rad2Deg, meshData.rotation.z * Mathf.Rad2Deg
        //);
        //transform.localScale = meshData.scale;
        // test
        pos = meshData.position;
        rot = Quaternion.Euler(
            meshData.rotation.x * Mathf.Rad2Deg, meshData.rotation.y * Mathf.Rad2Deg, meshData.rotation.z * Mathf.Rad2Deg
        );
        scale = meshData.scale;
        //Debug.Log(scale.ToString("F3"));
    }
    public void UpdateMeshDataAll()
    {
        filter.mesh = meshData.mesh;
        UpdateMeshDataTransform();
    }
}
