using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{

    public RectTransform Parent;

    public Image ChildImage;

    public Vector2 ScreenPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "uiTest"))
        {
            //Vector2 outVector2;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(Parent, ScreenPos, null,out outVector2);

            //Debug.Log(outVector2);


            
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, ChildImage.transform.position);


            Debug.Log(screenPoint);

            //Vector3 screenPos = Parent.transform.position;
            //Debug.Log(Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, screenPos.z+10f)));

            //Vector3 worldPos =Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, screenPos.z + 10f));

            //Debug.Log(Camera.main.worl);
        }
    }
}
