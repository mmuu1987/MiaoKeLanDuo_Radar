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

    public float RotationSpeed = 0.1f;


    private Quaternion _curQuaternion;

    /// <summary>
    /// 是否在圆圈范围内触发点击
    /// </summary>
    private bool isEnterCircle;
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
    }

    private void EnteringEvent(EventInfo obj)
    {
        //Debug.Log("持续进入中 "+ obj.Position) ;
        Rotation(obj);
    }
 
    private void _listenTuio_RadarExitEvent(EventInfo obj)
    {
       // Debug.Log("触摸离开，位置是" + obj.ExitPos);

        Rotation(obj);
    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {

        Debug.Log("触摸进入 位置是 " + obj.EnterPos);
        _enterPos = obj.EnterPos;
        _enterDir = (obj.EnterPos - centerPos).normalized;

        float dis = Vector2.Distance(obj.EnterPos, centerPos);

        if (dis <= 820) isEnterCircle = true;
        else isEnterCircle = false;
        Debug.Log("dis is " +dis);
        _curQuaternion = CuRectTransform.rotation;
    }


    private void Rotation(EventInfo obj)
    {
      //  Debug.Log("触摸离开，位置是" + obj.ExitPos);

       if (!isEnterCircle) return;

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
