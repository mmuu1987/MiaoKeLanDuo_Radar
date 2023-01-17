using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Circle : MonoBehaviour
{

    public float Radius = 200f;

    private RectTransform _cuRectTransform;

    public bool IsEnter(DragMove dragMove)
    {
        float dis = Vector2.Distance(_cuRectTransform.anchoredPosition, dragMove.CuRectTransform.anchoredPosition);

        if (dis >= Radius) return false;


        Debug.Log("dragmove "+ dragMove.name+"  Ω¯»Î¡Àcircle "+ this.name);
        return true;
    }
    // Start is called before the first frame update
    void Start()
    {
        _cuRectTransform = this.GetComponent<RectTransform>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
