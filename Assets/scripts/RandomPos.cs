using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �������λ�ã����漴�ƶ�������һ���ķ�Χ
/// </summary>
public class RandomPos : MonoBehaviour
{
    /// <summary>
    /// ����ڵİ뾶��Χ
    /// </summary>
    public float Radius;

    /// <summary>
    /// ����˶���Χ�����ĵ�
    /// </summary>
    public Vector3 CircleCenter;

    public List<Item> Papaws= new List<Item>();

    public float MaxSpeed = 100f;

    
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1920,2400,true);
        GeneratePos();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GeneratePos()
    {

        foreach (Item item in Papaws)
        {
            item.Init(Radius,CircleCenter, MaxSpeed);
        }


    }

   
    

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "test"))
        {
            GeneratePos();
        }
    }
#endif
}
