using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedBorder : MonoBehaviour
{
    public List<Renderer> renderers;

    // Start is called before the first frame update
    void Start()
    {
        if (renderers == null)
            renderers = new List<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeColor(Color c)
    {
        foreach (Renderer r in renderers)
            r.material.color = c;
    }
}
