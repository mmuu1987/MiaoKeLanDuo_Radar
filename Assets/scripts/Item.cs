using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Item : MonoBehaviour
{


    public RectTransform RectTransform;


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



   
    private void Awake()
    {
        RectTransform = this.GetComponent<RectTransform>();


        _listenTuio = this.GetComponent<ListenTuio>();

        CuRectTransform = this.GetComponent<RectTransform>();

        _listenTuio.Pointing += _listenTuio_Pointing;

        _listenTuio.PointEnter += _listenTuio_PointEnter;

        _listenTuio.PointExit += _listenTuio_RadarExitEvent;

    }

    public void Init(float radius,Vector3 circleCenter,float maxSpeed)
    {
        if (RectTransform == null) RectTransform = this.GetComponent<RectTransform>();

        _maxRadius = radius;

        _circleCenter = circleCenter;

        _maxSpeed = maxSpeed;

        SetRandomMove();

        if (this.gameObject.activeSelf)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(RandChange());
        }
      


    
    }


    private void _listenTuio_Pointing(EventInfo obj)
    {
        //Debug.Log(obj.Position);  

       

        CuRectTransform.anchoredPosition += obj.Delta * 0.5f;
    }

    private void _listenTuio_RadarExitEvent(EventInfo obj)
    {
          //Debug.Log("触摸离开  "+this.name);
        _isTouch = false;

       
    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {
       //  Debug.Log("触摸进入  " + this.name);
        _isTouch = true;
    }
    private void SetRandomMove()
    {

        _dir = Random.insideUnitCircle.normalized;

        _moveSpeed = Random.Range(10f, _maxSpeed);

        float dis = Random.Range(0, _maxRadius);

        Vector2 pos = _circleCenter + _dir * dis;

        RectTransform.anchoredPosition = pos;


    }
    private IEnumerator RandChange()
    {

        while (true)
        {
            float randTime = Random.Range(2f, 10f);

            yield return new WaitForSeconds(randTime);


            _dir = Random.insideUnitCircle.normalized;

            _moveSpeed = Random.Range(10f, _maxSpeed);
        }
    }

  
    /// <summary>
    /// 检测范围
    /// </summary>
    private void CheckRange()
    {

        if (_isTouch) return;

        if (IsEnter)
        {
            return;//如果碰撞到则不进行范围检测
        }

        Vector2 add = _dir * _moveSpeed * Time.deltaTime;

        this.RectTransform.anchoredPosition += add;

        float dis = Vector3.Distance(RectTransform.anchoredPosition, _circleCenter);

        if (dis >= _maxRadius)
        {
            _dir = -_dir;

            this.RectTransform.anchoredPosition -= add;

            _moveSpeed = Random.Range(10f, _maxSpeed);
        }
    }

    /// <summary>
    /// 碰撞到圆形后,该材料消失，隔开一段时间继续出现在屏幕中
    /// 消失之后才触发下一步动作
    /// </summary>
    public void ColliderEvent(Vector2 targetPos,Action action)
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
                SetRandomMove();
               
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
        CheckRange();
    }
}
