using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 根据雷达提供的信息，把拖动转换成图片的旋转
/// </summary>
public class DragRotation : MonoBehaviour
{
    private ListenTuio _listenTuio;


    private Vector2 centerPos;

    /// <summary>
    /// 触控开始的时候记录的，触控点到圆心(雷达)的向量
    /// </summary>
    private Vector2 _enterDir;

    /// <summary>
    /// 触控开始的时候，记录触控点的位置
    /// </summary>
    private Vector2 _enterPos;


    /// <summary>
    /// 触控结束，离开桌面的时候，记录触控开始点与结束点的向量
    /// </summary>
    private Vector2 _exitDir;
    public RectTransform CuRectTransform { get; private set; }

    public float maxDis=920f;

    public float minDis=750f;

    public float RotationSpeed = 0.1f;

    /// <summary>
    /// 拖动的时候忽略掉，所在区域的集合
    /// </summary>
    public List<RectTransform> IgnoreRect= new List<RectTransform>();

    private Quaternion _curQuaternion;

    /// <summary>
    /// 是否在圆圈范围内触发点击
    /// </summary>
    private bool isEnterCircle;

    private bool _isEnter = false;

    private int _enterId;
    // Start is called before the first frame update
    void Start()
    {

        _listenTuio = this.GetComponent<ListenTuio>();

        CuRectTransform = this.GetComponent<RectTransform>();

        centerPos = CuRectTransform.anchoredPosition;
        Debug.Log("中心点位置是： " + centerPos);
     
        TuioManager.Instance.EnterEvent += _listenTuio_PointEnter;
        TuioManager.Instance.EnteringEvent += EnteringEvent;
        TuioManager.Instance.ExitEvent += _listenTuio_RadarExitEvent;
        _isEnter = false;
    }

    private void EnteringEvent(EventInfo obj)
    {
       

        if (!_isEnter) return;

        if (obj.ID != _enterId) return;

       // Debug.Log("持续进入中 "+ obj.Position) ;
        Rotation(obj);
    }
 
    private void _listenTuio_RadarExitEvent(EventInfo obj)
    {
      
       if (_enterId != obj.ID) return;

        Rotation(obj);
        _enterDir = Vector2.zero;
        _isEnter = false;

        // Debug.Log("触摸离开，位置是" + obj.ExitPos);

    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {
        if (_isEnter) return;//只允许一个触摸进入

        _enterId = obj.ID;
        _isEnter = true;
        //Debug.Log("触摸进入 位置是 " + obj.EnterPos);
        _enterPos = obj.EnterPos;
        _enterDir = (_enterPos - centerPos).normalized;
       
        _curQuaternion = CuRectTransform.rotation;

    }

    /// <summary>
    /// 检测是否是在符合的范围
    /// </summary>
    private bool CheckRange(Vector2 enterPos)
    {
        float dis = Vector2.Distance(enterPos, centerPos);

        foreach (RectTransform rectTransform in IgnoreRect)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);

            Vector2 size = rectTransform.sizeDelta;

            float radius = Mathf.Sqrt(Mathf.Pow(size.x / 2f, 2) + Mathf.Pow(size.y / 2f, 2));

            float disTemp = Vector2.Distance(screenPos, enterPos);

            if (radius >= disTemp)//如果在忽略移动的框里，则不移动
                return false;
        }

        if (dis <= maxDis & dis >= minDis) return true;
        return false;
    }
    private void Rotation(EventInfo obj)
    {
         // Debug.Log("触摸离开，位置是" + obj.Position);


        isEnterCircle= CheckRange(obj.Position);

        //Debug.Log("isEnterCircle is " + isEnterCircle);


        if (!isEnterCircle)
        {
            _enterDir = Vector2.zero;

            
            return;
        }

         if (_enterDir == Vector2.zero) return;



        Vector3 newDir  = ( obj.Position - this.centerPos).normalized;

        //叉乘_enterDir  _exitDir，判断旋转的方向

        Vector3 dir = Vector3.Cross(newDir, _enterDir);


        float angle = Vector3.Angle(newDir, _enterDir);

        if (dir.z > 0)
        {
           // Debug.Log("顺时针方向 " + dir);
            angle *= -1;
        }
        else
        {
            //Debug.Log("逆时针方向 " + dir);
        }

        this.CuRectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle*3f)) * _curQuaternion;
    }
    // Update is called once per frame
    void Update()
    {

    }

}
