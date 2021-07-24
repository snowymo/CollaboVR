using UnityEngine;
using System.Collections.Generic;

public class GlowObjectCmd : MonoBehaviour
{
	public Color GlowColor;
	float LerpFactor = 4.5f;

	public Renderer[] Renderers
	{
		get;
		private set;
	}

	public Color CurrentColor
	{
		get { return _currentColor; }
	}

	private Color _currentColor;
	private Color _targetColor;

    public void Enable()
    {
        GlowController.RegisterObject(this);
    }
    public void Disable()
    {
        GlowController.DeregisterObject(this);
    }

    void Start()
	{
		Renderers = GetComponentsInChildren<Renderer>();
		GlowController.RegisterObject(this);
	}

	/// <summary>
	/// Update color, disable self if we reach our target color.
	/// </summary>
	private void Update()
	{
        _targetColor = GlowColor;
        enabled = true;
        _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * LerpFactor);
        if (_currentColor.Equals(_targetColor))
		{
            enabled = false;
		}

        enabled = false; // temp
	}
}
