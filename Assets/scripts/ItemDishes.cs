using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 核心菜式的配料组件
/// </summary>
public class ItemDishes : MonoBehaviour
{

    public RectTransform RectTransform;

    public Transform Root;

    private Vector2 _dir;

    private float _moveSpeed = 1f;

    private float _maxRadius;

    private Vector2 _circleCenter;

    private float _maxSpeed = 1f;

    private Coroutine _coroutine;

    private ListenTuio _listenTuio;

    /// <summary>
    /// 是否接收到雷达触摸的消息
    /// </summary>
    private bool _isTouch = false;

    /// <summary>
    /// 碰撞后随机位置再次出现
    /// </summary>
    private Coroutine _generateCoroutine;
    public RectTransform CuRectTransform { get; private set; }


    /// <summary>
    /// 是否进入圆形范围
    /// </summary>
    public bool IsEnter = false;



    private Vector2 clickDir = new Vector2();

    /// <summary>
    /// 父物体
    /// </summary>
    private Transform _originalRectParenTransform;

    private Vector2 _originalPos;

    private Transform _root;
    private void Awake()
    {
        RectTransform = this.GetComponent<RectTransform>();


        _listenTuio = this.GetComponent<ListenTuio>();

        if(_listenTuio==null)Debug.Log($"{this.name} 丢失组件");

        CuRectTransform = this.GetComponent<RectTransform>();

        _listenTuio.Pointing += _listenTuio_Pointing;

        _listenTuio.PointEnter += _listenTuio_PointEnter;

        _listenTuio.PointExit += _listenTuio_RadarExitEvent;


        _originalRectParenTransform = CuRectTransform.parent;

    }

    public void Init(Transform root, Vector3 circleCenter, float maxSpeed)
    {
        if (RectTransform == null) RectTransform = this.GetComponent<RectTransform>();

       

        _circleCenter = circleCenter;

        _maxSpeed = maxSpeed;

       // _root = root;

    }


    private void _listenTuio_Pointing(EventInfo obj)
    {

       // CuRectTransform.anchoredPosition += obj.Delta * 0.5f;

        CuRectTransform.anchoredPosition = obj.Position + clickDir;
    }

    private void _listenTuio_RadarExitEvent(EventInfo obj)
    {
        //Debug.Log("触摸离开  "+this.name);
        _isTouch = false;

        CuRectTransform.parent = _originalRectParenTransform;

        //CuRectTransform.anchorMin = Vector2.zero;
        //CuRectTransform.anchorMax = Vector2.zero;

        CuRectTransform.anchoredPosition = _originalPos;
    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {
        //  Debug.Log("触摸进入  " + this.name);
        _isTouch = true;
        _originalPos = CuRectTransform.anchoredPosition;
        Vector2 scorrenPos = RectTransformUtility.WorldToScreenPoint(null, CuRectTransform.position);

        CuRectTransform.parent = Root;

        clickDir = scorrenPos - obj.Position;

        //改变父物体后需要重新激活该锚点的位置，我们目前以左下为锚点，所以min max都是zero
        CuRectTransform.anchorMin= Vector2.zero;
        CuRectTransform.anchorMax = Vector2.zero;

        CuRectTransform.anchoredPosition = obj.Position + clickDir;

      

        Debug.Log($"clickDir is {clickDir}");

      

       
    }
   
  
    /// <summary>
    /// 碰撞到圆形后,该材料消失，隔开一段时间继续出现在屏幕中
    /// 消失之后才触发下一步动作
    /// </summary>
    public void ColliderEvent(Vector2 targetPos, Action action)
    {
        IsEnter = true;

        if (this.gameObject.activeSelf)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);

            this.RectTransform.DOAnchorPos(targetPos, 0.5f).OnComplete((() =>
            {
                if (action != null) action();
            }));

            this.GetComponent<MaskableGraphic>().DOFade(0f, 0.5f);

            StartCoroutine(GlobSetting.WaitTime(Random.Range(1f, 3f), (() =>
            {

                IsEnter = false;
              

                this.GetComponent<MaskableGraphic>().DOFade(1f, 0.5f);
            })));

        }


    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //CheckRange();
    }
}
