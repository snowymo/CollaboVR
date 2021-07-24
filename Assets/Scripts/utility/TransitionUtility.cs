using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransitionUtility {

    [System.Serializable]
    public struct ColorInterp
    {
        public Proc_ColorInterp interpProc;
        [HideInInspector]
        public float timeElapsed;
        public float timeDuration;

        public ColorInterp(Proc_ColorInterp interpProc, float timeDuration)
        {
            this.interpProc = interpProc;
            this.timeElapsed = 0.0f;
            this.timeDuration = timeDuration;
        }

        public void ReSet()
        {
            this.timeElapsed = 0.0f;
        }
    }

    [System.Serializable]
    public struct TransitionOverlay 
    {
        public GameObject obj;
        public Color startColor;
        public Color endColor;
        [HideInInspector]
        public Material mat;
        [HideInInspector]
        public int colorID;
        [HideInInspector]
        public Renderer rend;

        public TransitionOverlay(TransitionOverlay transitionOverlay)
        {
            this.obj = GameObject.Instantiate(transitionOverlay.obj);
            this.obj.name = "transitionOverlay";

            this.startColor = transitionOverlay.startColor;
            this.endColor = transitionOverlay.endColor;

            this.rend = this.obj.GetComponent<Renderer>();

            this.mat = new Material(rend.sharedMaterial);
            this.rend.sharedMaterial = this.mat;
            this.colorID = Shader.PropertyToID("_Color");

            UpdateColor(this.startColor);
        }
        public TransitionOverlay(GameObject prefab, Color startColor, Color endColor)
        {
            this.obj = GameObject.Instantiate(prefab);
            this.obj.name = "transitionOverlay";

            this.startColor = startColor;
            this.endColor = endColor;

            this.rend = this.obj.GetComponent<Renderer>();

            this.mat = new Material(rend.sharedMaterial);
            this.rend.sharedMaterial = this.mat;
            this.colorID = Shader.PropertyToID("_Color");

            UpdateColor(this.startColor);
        }

        public void UpdateColor(Color c)
        {
            this.mat.SetColor(this.colorID, c);
        }
    }

    public delegate Color Proc_ColorInterp(Color a, Color b, float t);

    public static Color TwoWay_ColorSmoothstep(Color a, Color b, float t)
    {
        t = Mathf.Clamp01(t);

        if (t < 0.5f) {
            return new Color(
                Mathf.SmoothStep(a.r, b.r, t * 2),
                Mathf.SmoothStep(a.g, b.g, t * 2),
                Mathf.SmoothStep(a.b, b.b, t * 2),
                Mathf.SmoothStep(a.a, b.a, t * 2)
            );
        }

        return new Color(
            Mathf.SmoothStep(a.r, b.r, (1 - t) * 2),
            Mathf.SmoothStep(a.g, b.g, (1 - t) * 2),
            Mathf.SmoothStep(a.b, b.b, (1 - t) * 2),
            Mathf.SmoothStep(a.a, b.a, (1 - t) * 2)
        );
    }

    public static Color TwoWay_ColorMiddleFlatline(Color a, Color b, float t)
    {
        t = Mathf.Clamp01(t);

        const float FRAC = 0.4f;
        const float MULT = 1.0f / FRAC;

        if (t < FRAC) {
            return new Color(
                Mathf.SmoothStep(a.r, b.r, t * MULT),
                Mathf.SmoothStep(a.g, b.g, t * MULT),
                Mathf.SmoothStep(a.b, b.b, t * MULT),
                Mathf.SmoothStep(a.a, b.a, t * MULT)
            );
        }
        else if (t < (1 - FRAC)) {
            return b;
        }
        else {
            return new Color(
                Mathf.SmoothStep(a.r, b.r, (1 - t) * MULT),
                Mathf.SmoothStep(a.g, b.g, (1 - t) * MULT),
                Mathf.SmoothStep(a.b, b.b, (1 - t) * MULT),
                Mathf.SmoothStep(a.a, b.a, (1 - t) * MULT)
            );
        }
    }
}

