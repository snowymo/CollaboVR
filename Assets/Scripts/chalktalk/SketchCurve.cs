using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using System.Linq;

namespace Chalktalk
{
    public enum ChalktalkDrawType { STROKE = 0, FILL = 1, TEXT = 2 };

    public enum ChalkTalkType { CURVE, PROCEDURE };

    public class SketchCurve : MonoBehaviour
    {
        /// <summary>
        /// property for current sketch curve
        /// </summary>
        public int sketchPageID;
        public Vector3[] points, transformedPoints;
        public Color color = Color.black;
        Color matColor;
        public float width = 0.0f;
        public ChalktalkDrawType type;
        public Vector3 textPos = Vector3.zero;
        public float textScale = 1f;
        public string text;
        ChalktalkBoard refBoard;
        public bool isDup;
        public bool isOutlineFrame;

        /// <summary>
        /// property related to visualize
        /// </summary>
        public LineRenderer line;
        public MeshRenderer meshRenderer;
        public TextMesh textMesh;

        public Mesh shape;
        public MeshFilter meshFilter;

        public static float CT_TEXT_SCALE_FACTOR = 0.638f * 0.855f;
        public float facingDirection = 0f;

        /// <summary>
        /// vectrosity
        /// </summary>
        VectorLine vectrosityLine;
        VectorLine vText;
        public Transform forDrawTransform;
        List<Vector3> vPoints;

        /// <summary>
        /// material
        /// </summary>
        public Material defaultMat;
        //public Material glowMat;

        // Use this for initialization
        void Start()
        {
            vPoints = new List<Vector3>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void InitWithText(string textStr, Vector3 tp, float s, float fd, Color c, ChalktalkDrawType t, int spID = 0)
        {
            textPos = tp;
            textScale = s;
            facingDirection = fd;
            sketchPageID = spID;
            color = c;
            text = textStr;
            type = t;
            isDup = false;
            //isOutlineFrame = false;
        }


        void DrawTextMeshText()
        {
            gameObject.SetActive(true);

            textMesh.anchor = TextAnchor.MiddleCenter;
            //                    textMesh.fontSize = 3;
            //textMesh.font = Resources.Load("Nevis") as Font;
            textMesh.text = text;
            //textMesh.font = myfont;
            //textMesh.font.material = fontMat;
            textMesh.fontSize = 355;
            textMesh.characterSize = 0.2f;
            textMesh.color = color;
            // debug
            //print("textPos:" + textPos);
            //print("refBoard.localScale:" + refBoard.localScale);
            //print("GlobalToggleIns.GetInstance().ChalktalkBoardScale:" + GlobalToggleIns.GetInstance().ChalktalkBoardScale);
                
            transform.localPosition = new Vector3(textPos.x * refBoard.boardScale,
                textPos.y * refBoard.boardScale,
                textPos.z * refBoard.boardScale);

            transform.parent = refBoard.transform;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3( textScale * CT_TEXT_SCALE_FACTOR * refBoard.boardScale,
                textScale * CT_TEXT_SCALE_FACTOR * refBoard.boardScale, 1.0f);
        }
        // NEED TO TEST
        void DrawVectrosityText()
        {
            if (vText == null)
            {
                vText = new VectorLine("3DText-" + text, new List<Vector3>(), 1.0f);
            }

            vText.color = color;
            //vText.drawTransform.localRotation = Quaternion.Euler(0, facingDirection, 0);

            vText.MakeText(text, textPos, textScale);
           // Vector3 newpos = refBoard.TransformPoint(textPos);
           // forDrawTransform.localPosition = newpos;
           // forDrawTransform.localRotation = refBoard.rotation;

            vText.drawTransform = refBoard.transform;


            //vText.MakeText(text, textPos, textScale);

            vText.Draw3D();
        }

        public void InitWithLines(Vector3[] pts, Color c, float w, ChalktalkDrawType t, int spID = 0)
        {
            points = pts;
            width = w;
            sketchPageID = spID;
            type = t;
            isDup = false;
            // 
            line.useWorldSpace = false;



            //isOutlineFrame = false;
            //Material[] mats = line.sharedMaterials;
            //mats[1] = null;
            //line.sharedMaterials = mats;

            // do not replace the material if nothing has changed
            if (c == color) {
                return;
            }

            // optimization for material
            color = c;
            KeyValuePair<Material, Color> materialInfo;
            
            if (Utility.colorToMaterialInfoMap.TryGetValue(c, out materialInfo)) {
                line.sharedMaterial = materialInfo.Key;
                matColor = materialInfo.Value;
            }
            else {
                matColor = new Color(Mathf.Pow(c.r, 0.45f), Mathf.Pow(c.g, 0.45f), Mathf.Pow(c.b, 0.45f));
                Material mat = new Material(defaultMat);
                mat.SetColor("_Color", matColor);
                line.sharedMaterial = mat;

                Utility.colorToMaterialInfoMap.Add(c, new KeyValuePair<Material, Color>(mat, matColor));
            }
        }

        public bool ApplyTransform(List<ChalktalkBoard> boards)
        {
            //if (sketchPageID >= boards.Count)
            //return false;
            // when sketchPageID cannot find the corresponding boardID, create a new one
            //bool isFound = false;
            // because we have eyes-free mode, so boards[sketchPageID] maynot be the only board with specific page id
            if (sketchPageID >= ChalktalkBoard.curMaxBoardID)
                return false;
            List<ChalktalkBoard> relatedBoards = ChalktalkBoard.GetBoard(sketchPageID);
            if (relatedBoards.Count == 0)
                return false;
            for(int i = 0; i < relatedBoards.Count; i++)
            {
                // only the first one is working for eyesfree writing mode
                bool isBoardDup = relatedBoards[i].name.Contains("Dup");
                if ((relatedBoards[i].boardID == sketchPageID) && (isBoardDup == isDup))
                {
                    refBoard = relatedBoards[i];
                    if (GlobalToggleIns.GetInstance().rendererForLine == GlobalToggle.LineOption.Vectrosity) {
                        switch (type) {
                        case ChalktalkDrawType.STROKE:
                            DrawVectrosityLine();
                            break;
                        case ChalktalkDrawType.TEXT:
                            DrawVectrosityText();
                            break;
                        default:
                            break;
                        }
                    }
                    else {
                        switch (type) {
                        case ChalktalkDrawType.STROKE:
                            DrawLineRendererLine();
                            break;
                        case ChalktalkDrawType.TEXT:
                            DrawTextMeshText();
                            break;
                        case ChalktalkDrawType.FILL:
                            DrawWithFill();
                            break;
                        default:
                            break;
                        }
                    }
                    break;
                }
            }

            return true;
        }

        public void DrawLineRendererLine()
        {
            line.positionCount = points.Length;
            // need to apply transformation here
            transform.parent = refBoard.transform;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.localScale *= refBoard.boardScale;
            //transformedPoints = new Vector3[points.Length];
            //for(int i = 0; i < points.Length; i++)
            //{
            //    transformedPoints[i] = points[i]* refBoard.boardScale;
            //    transformedPoints[i] = refBoard.transform.rotation * transformedPoints[i] + refBoard.transform.position;
            //}
            //line.SetPositions(transformedPoints);
            line.SetPositions(points);

            line.startColor = matColor;
            line.endColor = matColor;
            // NEW
            //this.materialPropertyBlock.SetColor(colorPropID, c);
            //this.line.SetPropertyBlock(this.materialPropertyBlock);
            line.startWidth = width;
            line.endWidth = width;
        }

        // NEED TO TEST
        public void DrawVectrosityLine()
        {
            // TODO: VectorLine only takes List
            //vPoints.Clear();
            vPoints = points.ToList();
            if (vectrosityLine == null)
            {
                vectrosityLine = new VectorLine("haha", vPoints, width * 1080.0f / 3.0f, LineType.Continuous);  //TODO
            }
            else
            {
                vectrosityLine.points3 = vPoints;
            }
            //vectrosityLine.material = defaultMat;
            Color c = new Color(Mathf.Pow(color.r, 0.45f), Mathf.Pow(color.g, 0.45f), Mathf.Pow(color.b, 0.45f));
            //vectrosityLine.material.color = c;
            //vectrosityLine.material.SetColor("_EmissionColor", c);
            vectrosityLine.drawTransform = refBoard.transform;
            vectrosityLine.SetColor(c);
            vectrosityLine.Draw3D();
        }

        public void InitWithFill(Vector3[] pts, Color color, ChalktalkDrawType t, int spID = 0)
        {
            points = pts;
            type = t;
            sketchPageID = spID;
            isDup = false;

            int countSides = points.Length;
            int countTris = countSides - 2;
            int[] indices = new int[countTris * 3 * 2];
            for (int i = 0, off = 0; i < countTris; ++i, off += 6)
            {
                indices[off] = 0;
                indices[off + 1] = i + 1;
                indices[off + 2] = i + 2;
                indices[off + 3] = 0;
                indices[off + 4] = i + 2;
                indices[off + 5] = i + 1;
            }

            shape.vertices = points;
            shape.triangles = indices;

            Material mymat = new Material(defaultMat);
            // similar to what chalktalk do to the color TODO check if shader material is the same as what already exists (in which case, don't modify)
            Color c = new Color(Mathf.Pow(color.r, 0.45f), Mathf.Pow(color.g, 0.45f), Mathf.Pow(color.b, 0.45f));
            mymat.SetColor("_Color", c);
            meshRenderer.material = mymat;

            shape.RecalculateBounds();
            shape.RecalculateNormals();

            meshFilter.mesh = shape;
        }

        // TODO: need to test scale z is correct
        public void DrawWithFill()
        {
            transform.parent = refBoard.transform;
            transform.localScale *= refBoard.boardScale;         
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            //// need to apply transformation here
            //transformedPoints = new Vector3[points.Length];
            //for (int i = 0; i < points.Length; i++)
            //{
            //    transformedPoints[i] = points[i] * GlobalToggleIns.GetInstance().ChalktalkBoardScale;
            //}
            //shape.vertices = points;

            //shape.RecalculateBounds();
            //shape.RecalculateNormals();

            //meshFilter.mesh = shape;
        }
    }
}