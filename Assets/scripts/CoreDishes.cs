using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoreDishes : MonoBehaviour
{


    public RectTransform RectTransform;

    public RectTransform RotationRectTransform;

    /// <summary>
    /// 开启添加原材料的玩法，true为开启，false为关闭
    /// </summary>
    public Action<bool, RectTransform> AdddFlavouring;

    public List<RectTransform> FoodPartList = new List<RectTransform>();

    public float maxDis = 100;

    public float minDis = 0f;

    public GameObject DragItem;

    private ListenTuio _listenTuio;

    /// <summary>
    /// 关闭玩法
    /// </summary>
    private Coroutine _close;

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


    /// <summary>
    /// 是否在圆圈范围内触发点击
    /// </summary>
    private bool isEnterCircle;


    private Quaternion _curQuaternion;

    private List<ItemDishes> _itemDisheses = new List<ItemDishes>();


    private bool _isEnter = false;

    private int _enterId;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform = this.GetComponent<RectTransform>();

        RotationRectTransform = this.transform.Find("Rotation").GetComponent<RectTransform>();

        _listenTuio = this.GetComponent<ListenTuio>();

        _listenTuio.OnClick += _listenTuio_OnClick;

        TuioManager.Instance.EnterEvent += _listenTuio_PointEnter;
        TuioManager.Instance.EnteringEvent += EnteringEvent;
        TuioManager.Instance.ExitEvent += _listenTuio_RadarExitEvent;


        float angle = 360f / FoodPartList.Count;

        for (int i = 0; i < FoodPartList.Count; i++)
        {
            Quaternion q = Quaternion.Euler(new Vector3(0f, 0f, i * angle));

            Vector2 disDir = q * Vector2.up * 200f;//180不是角度，而是距离

            GameObject go = Instantiate(DragItem, RotationRectTransform.transform);

            ItemDishes itemDishes = go.GetComponent<ItemDishes>();

            itemDishes.name = FoodPartList[i].name;//赋予名字一致


            itemDishes.CuRectTransform.anchoredPosition = this.RotationRectTransform.anchoredPosition + disDir;
            _itemDisheses.Add(itemDishes);


        }

        ShowItemDishes(false);
    }

    private void _listenTuio_RadarExitEvent(EventInfo obj)
    {

        if (_enterId != obj.ID) return;
        Rotation(obj);
        _enterDir = Vector2.zero;
        _isEnter = false;
    }

    private void EnteringEvent(EventInfo obj)
    {

        if (!_isEnter) return;

        if (obj.ID != _enterId) return;

        Rotation(obj);
       
    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {
        //Debug.Log("触摸进入 位置是 " + obj.EnterPos);

        if (_isEnter) return;//只允许一个触摸进入

        _enterId = obj.ID;
        _isEnter = true;

        this.centerPos = RectTransformUtility.WorldToScreenPoint(null, RotationRectTransform.position);

        _enterPos = obj.EnterPos;
        _enterDir = (_enterPos - centerPos).normalized;//归到同一个坐标下后计算方向
        isEnterCircle = CheckRange(obj.EnterPos);
        //Debug.Log("dis is " + _enterPos);
        _curQuaternion = RotationRectTransform.rotation;
    }


    /// <summary>
    /// 检测是否是在符合的范围
    /// </summary>
    private bool CheckRange(Vector2 enterPos)
    {
        float dis = Vector2.Distance(enterPos, centerPos);


        if (dis <= maxDis & dis >= minDis) return true;
        return false;
    }

    private void _listenTuio_OnClick()
    {
       //Debug.Log(this.name);

       if (AdddFlavouring != null) AdddFlavouring(true, RectTransform);

       ShowItemDishes(true);
       //定时关闭玩法
       if (_close!=null)StopCoroutine(_close);
       _close = StartCoroutine(GlobSetting.WaitTime(10f, (() =>
       {
           if (AdddFlavouring != null) AdddFlavouring(false, RectTransform);
           ShowItemDishes(false);
       })));
    }
    private void ShowItemDishes(bool isShow)
    {

        

        foreach (ItemDishes dishes in _itemDisheses)
        {
            dishes.gameObject.SetActive(isShow);
        }

        foreach (RectTransform rectTransform in FoodPartList)
        {
            rectTransform.gameObject.SetActive(isShow);
        }
    }


    private void Rotation(EventInfo obj)
    {
        // Debug.Log("触摸离开，位置是" + obj.Position);


        isEnterCircle = CheckRange(obj.Position);

        if (!isEnterCircle)
        {
            _enterDir = Vector2.zero;
            return;
        }

        if (_enterDir == Vector2.zero) return;

        this.centerPos = RectTransformUtility.WorldToScreenPoint(null, RotationRectTransform.position);

        Vector3 newDir = (obj.Position - this.centerPos).normalized;

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

        this.RotationRectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle * 3f)) * _curQuaternion;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
