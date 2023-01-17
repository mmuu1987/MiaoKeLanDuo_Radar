using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public List<DragMove> DragMoves = new List<DragMove>();
    


    public List<Circle> Circles = new List<Circle>();



    /// <summary>
    /// 检测是否进入该范围 
    /// </summary>
    private void CheckRang()
    {
        foreach (Circle circle in Circles)
        {
            bool isEnter = false;
            foreach (DragMove dragMove in DragMoves)
            {
                if (!dragMove.IsEnter)
                {
                    isEnter = circle.IsEnter(dragMove);

                    if (isEnter)
                    {
                        dragMove.IsEnter = true;
                    }
                    
                }
               
            }
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame  
    void Update()
    {
     CheckRang();   
    }
}
