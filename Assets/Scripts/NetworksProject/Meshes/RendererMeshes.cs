
using System.Collections.Generic;
using UnityEngine;

public class RendererMeshes : MonoBehaviour {

    public Material mat;
    public Material customizeMat;
    Material sharedMaterial, sharedCusMat;

    Chalktalk.Renderer ctRenderer;

    Chalktalk.SubList<MeshGO> meshGOList = new Chalktalk.SubList<MeshGO>(2);

    int MESH_ALLOC_COUNT = 0;
    public void AllocateAndInitMeshes(GameObject prefab, int count, List<MeshGO> list)
    {
        for (; count > 0; count -= 1) {
            GameObject go = GameObject.Instantiate(prefab);
            go.name = "M:" + MESH_ALLOC_COUNT;
            MESH_ALLOC_COUNT += 1;

            MeshGO meshGO = go.GetComponent<MeshGO>();
            meshGO.filter = go.AddComponent<MeshFilter>();
            meshGO.meshRenderer = go.AddComponent<MeshRenderer>();
            meshGO.customizeMat = sharedCusMat;
            meshGO.meshRenderer.sharedMaterial = sharedMaterial;
            meshGO.meshRenderer.enabled = false;
            meshGO.enabled = false;

            list.Add(meshGO);

        }
    }

    public MeshGO GetMeshGO()
    {
        if (meshGOList.countElementsInUse == meshGOList.buffer.Count) {
            AllocateAndInitMeshes(meshPrefab, meshGOList.buffer.Count, meshGOList.buffer);
        }

        MeshGO mGO = meshGOList.buffer[meshGOList.countElementsInUse];
        if (meshGOList.prevCountElementsInUse <= meshGOList.countElementsInUse) {
            mGO.enabled = true;
            mGO.meshRenderer.enabled = true;
        }

        meshGOList.countElementsInUse += 1;
        return mGO;
    }

    public void DisableUnusedMeshes()
    {
        Chalktalk.SubList<MeshGO> list = meshGOList;
        List<MeshGO> buff = list.buffer;

        int bound = Mathf.Min(
            Mathf.Max(list.countElementsInUse, list.prevCountElementsInUse),
            buff.Count
        );

        for (int i = list.countElementsInUse; i < bound; i += 1) {
            buff[i].enabled = false;
            buff[i].meshRenderer.enabled = false;
        }
        list.prevCountElementsInUse = list.countElementsInUse;
    }

    public void Rewind()
    {
        DisableUnusedMeshes();
        meshGOList.countElementsInUse = 0;
    }

    public static RendererMeshes rm;

    public static void RenderMeshesRewind()
    {
        rm.Rewind();
    }


    public GameObject meshPrefab;
	void Start () {
        rm = this;
        ctRenderer = GameObject.Find("ChalktalkHandler").GetComponent<Chalktalk.Renderer>();
        sharedMaterial = new Material(mat);
        sharedCusMat = new Material(customizeMat);

        AllocateAndInitMeshes(meshPrefab, 2, meshGOList.buffer);
        for (int i = 0; i < meshGOList.buffer.Count; i += 1) {
            meshGOList.buffer[i].enabled = false;
            meshGOList.buffer[i].meshRenderer.enabled = false;
        }

    }

    float prevX = 0.0f;
    float prevY = 0.0f;
    float prevZ = 0.0f;

    float zChangeCounter = 0;

    bool DelayZMovement(MeshContent.MeshData meshInfo)
    {
        if (zChangeCounter != 0) {

            zChangeCounter -= 1;

            if (meshInfo.position.x != prevX ||
                meshInfo.position.y != prevY) {

                zChangeCounter = 0;

                prevX = meshInfo.position.x;
                prevY = meshInfo.position.y;
                prevZ = meshInfo.position.z;
            }
            else {
                return true;
            }
        }
        else {

            if (meshInfo.position.z != prevZ) {
                prevZ = meshInfo.position.z;

                if (meshInfo.position.x == prevX &&
                    meshInfo.position.y == prevY) {

                    zChangeCounter = 3;

                    return true;
                }
            }
        }

        return false;
    }


    // TODO reuse game objects
    Queue<GameObject> freeGameObjects = new Queue<GameObject>();
    bool GetOrDeferMeshGameObject(Queue<int> q, int key, out MeshGO meshGO)
    {
        //Debug.Log("Checking key: " + key);
        if (MeshContent.idToMeshGOMap.TryGetValue(key, out meshGO)) {
            if (DelayZMovement(meshGO.meshData)) {
                return false;
            }
        }
        else {
            // need to create a new game object

            Debug.Log("getting new game object");


            GameObject go = new GameObject();
            MeshGO mcGO = go.AddComponent<MeshGO>();

            Debug.Log("setting mesh data");
            mcGO.meshData = MeshContent.idToMeshMap[key];
            Debug.Log(mcGO.meshData);
            Debug.Log("mesh data id: [" + mcGO.meshData.ID + ":" + mcGO.meshData.subID + "]");
            go.name = "M:" + mcGO.meshData.ID + ":" + mcGO.meshData.subID;
            mcGO.meshRenderer.sharedMaterial = sharedMaterial;

            MeshContent.idToMeshGOMap.Add(key, mcGO);

            meshGO = mcGO;
        }

        return true;
    }

    // all objects that need to be updated this frame
    Queue<int> q = MeshContent.needToUpdateQ;
    // temp I'm just going to destroy the game objects every frame ...
    // store all objects that were actually updated
    HashSet<int> qUpdated = new HashSet<int>();

    void ApplyBoardToMesh(MeshGO go)
    {
        Transform xform = go.transform;
        Vector3 position = go.meshData.position;

        List<ChalktalkBoard> boards = ChalktalkBoard.GetBoard(go.meshData.boardID);
        if (boards.Count == 0) {
            Debug.Log("board list length is 0, boardID: " + go.meshData.boardID + " is invalid then");
            return;
        }

        for(int i = 0; i < boards.Count; i++) {
            if(go.isDup == boards[i].name.Contains("Dup")) {
                Transform refBoard = boards[i].transform;
                go.transform.parent = refBoard;
                go.transform.localPosition = go.pos * boards[i].boardScale;
                go.transform.localRotation = Quaternion.Inverse(go.rot);//go.rot;
                go.transform.localScale = go.scale * boards[i].boardScale;
            }            
        }

        
        //go.transform.localScale = new Vector3(go.scale.x / refBoard.localScale.x, go.scale.y / refBoard.localScale.y, go.scale.z / refBoard.localScale.z);

//        go.transform.position = refBoard.position + new Vector3(go.pos.x, go.pos.y, go.pos.z);
//        go.transform.rotation = refBoard.rotation * Quaternion.Inverse(go.rot);
//        go.transform.localScale = go.scale * boardScale;

    }

    void UpdateMeshGameObjects()
    {
        int count = q.Count;

        // check all objects' keys for update
        for (int i = 0; i < count; i += 1) {
            int key = q.Dequeue();

            //Debug.Log("updating key=[" + (key & 0x0000FFFF) + ":" + (key & 0xFFFF0000) + "]");

            MeshGO go;
            if (!GetOrDeferMeshGameObject(q, key, out go)) {
                //q.Enqueue(key);
                continue;
            }

            go.UpdateMeshDataAll();
            ApplyBoardToMesh(go);

            qUpdated.Add(key);
        }
    }

    // old-style pipeline

    // just a hack for now
    const int oldRegeneratePipelineMaxFrameDelayBeforeDeletion = 3;
    int consecutiveFramesNoRewinds = 0;
    public void SetMeshes()
    {
        int meshDataCount = MeshContent.activeMeshData.Count;
        for (int i = 0; i < meshDataCount; i += 1) {
            MeshContent.MeshData meshData = MeshContent.activeMeshData.Dequeue();

            MeshGO meshGO = GetMeshGO();
            meshGO.Init(meshData);

            if (GlobalToggleIns.GetInstance().MRConfig == GlobalToggle.Configuration.eyesfree) {
                meshGO.isDup = true;
                if(meshData.boardID == ChalktalkBoard.activeBoardID) {
                    MeshGO meshGO2 = GetMeshGO();
                    meshGO2.Init(meshData);
                    ApplyBoardToMesh(meshGO2);
                    //meshGO2.transform.localScale = new Vector3(meshGO2.transform.localScale.x, meshGO2.transform.localScale.y, 0.000001f);
                    //meshGO2.transform.localPosition = new Vector3(meshGO2.transform.localPosition.x, meshGO2.transform.localPosition.y, 0f);
                }                
            }

            ApplyBoardToMesh(meshGO);
        }

        //Rewind();
        //if (meshDataCount > 0 ||
        //    consecutiveFramesNoRewinds == oldRegeneratePipelineMaxFrameDelayBeforeDeletion) {

        //    consecutiveFramesNoRewinds = 0;
        //    Rewind();
        //}
        //else {
        //    consecutiveFramesNoRewinds += 1;
        //    Rewind();
        //}
    }

    public const bool oldRegeneratePipelineOn = true;

    void Update()
    {
        if (oldRegeneratePipelineOn) {
            SetMeshes();
            return;
        }

        UpdateMeshGameObjects();



        // TODO no deletion yet
        return;

        // TODO do NOT destroy game objects per frame... but need a reliable way for
        // Chalktalk to tell the client that a mesh sketch has been removed (use commands?)
        List<int> toRemove = new List<int>();
        foreach (KeyValuePair<int, MeshGO> entry in MeshContent.idToMeshGOMap) {
            int key = Utility.ShortPairToInt(entry.Value.meshData.ID, entry.Value.meshData.subID);
            if (!qUpdated.Contains(key)) {
                GameObject.Destroy(entry.Value.gameObject);
                toRemove.Add(key);
            }
        }
        for (int i = 0; i < toRemove.Count; i += 1) {
            MeshContent.idToMeshGOMap.Remove(toRemove[i]);
        }

        q.Clear();
        qUpdated.Clear();
    }
}
