using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapMaterial : MonoBehaviour
{
    private Renderer renderer;
    private Material material;
    void Awake()
    {
        renderer = gameObject.GetComponent<Renderer>();
        material = renderer.material;
    }
  
    public void ColorChange()
    {
        Level curr = GameManager.Instance.Levels[GameManager.Instance.currentLevel];
        Debug.Log(curr.CourtColor + "color");
        material.SetColor("_BaseColor", curr.CourtColor);
    }
    
}
