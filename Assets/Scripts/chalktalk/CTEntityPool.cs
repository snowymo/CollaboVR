using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chalktalk
{
    // every frame we need to start with a "free" list,
    // using entries as needed and incrementing the count for every entry we get,
    // at the end of the frame, the count is reset so the entries can be reinitialized for the new frame,
    // unused entries are disabled
    public class SubList<T>
    {
        public List<T> buffer;
        public int countElementsInUse;
        public int prevCountElementsInUse;
        

        public SubList(int capInit = 100)
        {
            buffer = new List<T>(capInit);
            countElementsInUse = 0;
            prevCountElementsInUse = 0;
            
        }
    }

    // TODO for later if we need pools for other uses
    public class EntityPool<T>
    {
        public SubList<T> data;
        public GameObject prefab;

        public EntityPool(GameObject prefab, int capInit = 100)
        {
            data = new SubList<T>(capInit);
            this.prefab = prefab;
        }
    }

    public class CTEntityPool
    {
        public SubList<SketchCurve> withLinesList, withFillList, withTextList;
        public GameObject linePrefab, fillPrefab, textPrefab;

        public void Init(GameObject linePrefab, GameObject fillPrefab, GameObject textPrefab, int nLines, int nFill, int nText)
        {

            nLines = Mathf.Max(nLines, 2);
            nFill = Mathf.Max(nFill, 2);
            nText = Mathf.Max(nText, 2);

            this.linePrefab = linePrefab;
            this.fillPrefab = fillPrefab;
            this.textPrefab = textPrefab;

            withLinesList = new SubList<SketchCurve>(nLines);
            withFillList = new SubList<SketchCurve>(nFill);
            withTextList = new SubList<SketchCurve>(nText);

            // pre-allocate
            AllocateAndInitLines(linePrefab, nLines, withLinesList.buffer);
            for (int i = 0; i < withLinesList.buffer.Count; i += 1)
            {
                withLinesList.buffer[i].enabled = false;
                withLinesList.buffer[i].line.enabled = false;
                //withLinesList.buffer[i].gameObject.SetActive(false);
            }

            AllocateAndInitFills(fillPrefab, nFill, withFillList.buffer);
            for (int i = 0; i < withFillList.buffer.Count; i += 1)
            {
                withFillList.buffer[i].enabled = false;
            }

            AllocateAndInitText(textPrefab, nText, withTextList.buffer);
            for (int i = 0; i < withTextList.buffer.Count; i += 1)
            {
                withTextList.buffer[i].enabled = false;
            }

        }

        public static int ID = 0;
        public static int ID_LINES = 0;
        public static int ID_FILL = 0;
        public static int ID_TEXT = 0;


        public static int allocCount = 0;
        // pre-allocate buffer for lines
        private static void AllocateAndInitLines(GameObject prefab, int count, List<SketchCurve> list)
        {
            for (; count > 0; count -= 1)
            {
                allocCount += 1;
                GameObject go = GameObject.Instantiate(prefab);
                go.name = "L:" + ID + ":" + ID_LINES;
                ID += 1;
                ID_LINES += 1;

                // TODO could use prefabs instead to do a single GameObject.Instantiate instead of adding
                // these components one-by-one
                SketchCurve c = go.GetComponent<SketchCurve>();
                c.line = c.gameObject.AddComponent<LineRenderer>();
                c.line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                c.line.receiveShadows = false;
                //
                //c.line.sharedMaterials = new Material[2];

                //c.line.sharedMaterial = Curve.mainMaterial;
                //c.materialPropertyBlock = new MaterialPropertyBlock();

                //c.CacheShaderPropID();

                c.enabled = false;
                c.line.enabled = false;

                c.type = ChalktalkDrawType.STROKE;

                list.Add(c);

                // TODO: use vectrosity line
#if USE_VECTROSITY
            go.SetActive(false);
#endif
            }
        }

        // pre-allocate buffer for fills
        private static void AllocateAndInitFills(GameObject prefab, int count, List<SketchCurve> list)
        {
            for (; count > 0; count -= 1)
            {
                GameObject go = GameObject.Instantiate(prefab);
                go.name = "F:" + ID + ":" + ID_FILL;
                ID += 1;
                ID_FILL += 1;
                SketchCurve c = go.GetComponent<SketchCurve>();

                c.shape = new Mesh();
                c.meshRenderer = go.AddComponent<MeshRenderer>();
                c.meshFilter = go.AddComponent<MeshFilter>();

                c.enabled = false;
                c.meshRenderer.enabled = false;

                c.type = ChalktalkDrawType.FILL;

                list.Add(c);
            }
        }

        // pre-allocate buffer for text
        private static void AllocateAndInitText(GameObject prefab, int count, List<SketchCurve> list)
        {
            for (; count > 0; count -= 1)
            {
                GameObject go = GameObject.Instantiate(prefab);
                go.name = "T:" + ID + ":" + ID_TEXT;
                ID += 1;
                ID_TEXT += 1;

                SketchCurve c = go.GetComponent<SketchCurve>();
                c.textMesh = go.AddComponent<TextMesh>();
                c.meshRenderer = go.GetComponent<MeshRenderer>();
                c.isDup = false;

                c.enabled = false;
                c.meshRenderer.enabled = false;

                c.type = ChalktalkDrawType.TEXT;
                list.Add(c);

                //TODO: use vectrosity text
#if USE_VECTROSITY
            go.SetActive(false);
#endif
            }
        }



        public SketchCurve GetCTEntityLine()
        {
            // grow if all space used
            if (withLinesList.countElementsInUse == withLinesList.buffer.Count)
            {
                AllocateAndInitLines(linePrefab, withLinesList.buffer.Count, withLinesList.buffer);
            }

            // get the curve 
            SketchCurve c = withLinesList.buffer[withLinesList.countElementsInUse];
            if (withLinesList.prevCountElementsInUse <= withLinesList.countElementsInUse)
            {
                c.enabled = true;
                //todo I am using vectorLine now
                if(GlobalToggleIns.GetInstance().rendererForLine == GlobalToggle.LineOption.LineRenderer)
                    c.line.enabled = true;

            }

            // now one more element is in use this frame
            withLinesList.countElementsInUse += 1;
            return c;
        }

        public SketchCurve GetCTEntityFill()
        {
            // grow if all space used
            if (withFillList.countElementsInUse == withFillList.buffer.Count)
            {
                AllocateAndInitFills(fillPrefab, withFillList.buffer.Count, withFillList.buffer);
            }

            // get the curve 
            SketchCurve c = withFillList.buffer[withFillList.countElementsInUse];
            if (withFillList.prevCountElementsInUse <= withFillList.countElementsInUse)
            {
                c.enabled = true;
                c.meshRenderer.enabled = true;
            }

            // now one more element is in use this frame
            withFillList.countElementsInUse += 1;
            return c;
        }

        public SketchCurve GetCTEntityText()
        {
            // grow if all space used
            if (withTextList.countElementsInUse == withTextList.buffer.Count)
            {
                AllocateAndInitText(textPrefab, withTextList.buffer.Count, withTextList.buffer);
            }

            // get the curve 
            SketchCurve c = withTextList.buffer[withTextList.countElementsInUse];
            if (withTextList.prevCountElementsInUse <= withTextList.countElementsInUse)
            {
                c.enabled = true;
                c.meshRenderer.enabled = true;
            }

            // now one more element is in use this frame
            withTextList.countElementsInUse += 1;
            return c;
        }

        // disable un-needed entities
        public void DisableUnusedEntitiesLines()
        {
            SubList<SketchCurve> list = withLinesList;
            List<SketchCurve> buff = list.buffer;

            int bound = Mathf.Min(
                Mathf.Max(list.countElementsInUse, list.prevCountElementsInUse),
                buff.Count
            );

            for (int i = list.countElementsInUse; i < bound; i += 1)
            {
                buff[i].enabled = false;
                buff[i].line.enabled = false;
                //buff[i].gameObject.SetActive(false);
            }
            list.prevCountElementsInUse = list.countElementsInUse;
        }
        public void DisableUnusedEntitiesFill()
        {
            SubList<SketchCurve> list = withFillList;
            List<SketchCurve> buff = list.buffer;

            int bound = Mathf.Min(
                Mathf.Max(list.countElementsInUse, list.prevCountElementsInUse),
                buff.Count
            );


            for (int i = list.countElementsInUse; i < bound; i += 1)
            {
                buff[i].enabled = false;
                buff[i].meshRenderer.enabled = false;
            }
            list.prevCountElementsInUse = list.countElementsInUse;
        }
        public void DisableUnusedEntitiesText()
        {
            SubList<SketchCurve> list = withTextList;
            List<SketchCurve> buff = list.buffer;

            int bound = Mathf.Min(
                Mathf.Max(list.countElementsInUse, list.prevCountElementsInUse),
                buff.Count
            );


            for (int i = list.countElementsInUse; i < bound; i += 1)
            {
                buff[i].enabled = false;
                buff[i].meshRenderer.enabled = false;
            }
            list.prevCountElementsInUse = list.countElementsInUse;
        }

        // reset buffers to position 0
        public void RewindBuffers()
        {
            withLinesList.countElementsInUse = 0;
            withFillList.countElementsInUse = 0;
            withTextList.countElementsInUse = 0;
        }

        // call at the end of the frame before rendering
        public void FinalizeFrameData()
        {
            DisableUnusedEntitiesLines();
            DisableUnusedEntitiesFill();
            DisableUnusedEntitiesText();
            RewindBuffers();
        }

        public bool ApplyBoard(List<ChalktalkBoard> ctBoards)
        {
            bool ret = true;
            //withLinesList, withFillList, withTextList;
            for(int i = 0; i < withLinesList.countElementsInUse; i++)
            {
                if (!withLinesList.buffer[i].ApplyTransform(ctBoards)) {
                    ret = false;
                    break;
                }                    
            }
            for (int i = 0; i < withFillList.countElementsInUse; i++)
            {
                if (!withFillList.buffer[i].ApplyTransform(ctBoards)) {
                    ret = false;
                    break;
                }
            }
            for (int i = 0; i < withTextList.countElementsInUse; i++)
            {
                if (!withTextList.buffer[i].ApplyTransform(ctBoards)) {
                    ret = false;
                    break;
                }
            }
            return ret;
        }



        //public static void AddEntityWithStroke(List<GameObject> entities) {

        //    while (lineRenderers.Count < c.Count) {
        //        lineRenderers.Add(UnityEngine.Object.Instantiate(lineRendererPrefab));
        //    }
        //    while (lineRenderers.Count > c.Count) {
        //        LineRenderer rend = lineRenderers[lineRenderers.Count - 1];
        //        lineRenderers.RemoveAt(lineRenderers.Count - 1);
        //        UnityEngine.Object.Destroy(rend.gameObject);
        //    }
        //    for (int k = 0; k < c.Count; k++) {
        //        List<Vector3> curve = c[k];
        //        lineRenderers[k].positionCount = curve.Count;

        //        Vector3[] arr = curve.ToArray();
        //        for (int i = 0; i < curve.Count; i++) {
        //            arr[i] = transform.TransformPoint(arr[i]);
        //        }
        //        lineRenderers[k].SetPositions(arr);
        //    }
        //}
    }
}