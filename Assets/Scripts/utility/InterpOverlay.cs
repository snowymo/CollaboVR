using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(Renderer))]
public class InterpOverlay : MonoBehaviour {
    public Color colorStart;
    public Color colorEnd;

    Material mat;
    Renderer rend;
    
    public bool isTransitioning = false;

    public float tDuration = 10.0f;
    
    float tStart = 0.0f;
    float tElapsed = 0.0f;

    public bool isTransitioningBackwards;
  
    void Start()
    {
        this.rend = GetComponent<Renderer>();

        this.mat = new Material(this.rend.material);

        this.mat.SetColor("_Color", colorStart);

        this.rend.sharedMaterial = this.mat;

        this.enabled = false;
	}
	
    public void BeginForwardTransition()
    {
        tStart = Time.time;
        tElapsed = 0.0f;
        isTransitioning = true;
        isTransitioningBackwards = false;
    }


    private void SwapColors()
    {
        Color temp = this.colorStart;
        this.colorStart = this.colorEnd;
        this.colorEnd = temp;
    }

    public void BeginBackwardTransition()
    {
        tStart = Time.time;
        tElapsed = 0.0f;
        isTransitioning = true;
        isTransitioningBackwards = true;
        SwapColors();
    }

    void EndTransition()
    {
        if (isTransitioningBackwards) {
            SwapColors();
        }
        isTransitioning = false;
    }

	void Update()
    {
        if (isTransitioning) {
            float t = (tElapsed) / tDuration;
            Color c = Color.Lerp(colorStart, colorEnd, t);
            this.mat.SetColor("_Color", c);

            tElapsed += Time.deltaTime;

            if (tElapsed >= tDuration) {
                EndTransition();
            }
        }

	}
}
