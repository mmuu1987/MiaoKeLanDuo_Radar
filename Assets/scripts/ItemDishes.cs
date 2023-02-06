using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// ���Ĳ�ʽ���������
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
    /// �Ƿ���յ��״ﴥ������Ϣ
    /// </summary>
    private bool _isTouch = false;

    /// <summary>
    /// ��ײ�����λ���ٴγ���
    /// </summary>
    private Coroutine _generateCoroutine;
    public RectTransform CuRectTransform { get; private set; }


    /// <summary>
    /// �Ƿ����Բ�η�Χ
    /// </summary>
    public bool IsEnter = false;



    private Vector2 clickDir = new Vector2();

    /// <summary>
    /// ������
    /// </summary>
    private Transform _originalRectParenTransform;

    private Vector2 _originalPos;

    private Transform _root;
    private void Awake()
    {
        RectTransform = this.GetComponent<RectTransform>();


        _listenTuio = this.GetComponent<ListenTuio>();

        if(_listenTuio==null)Debug.Log($"{this.name} ��ʧ���");

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
        //Debug.Log("�����뿪  "+this.name);
        _isTouch = false;

        CuRectTransform.parent = _originalRectParenTransform;

        //CuRectTransform.anchorMin = Vector2.zero;
        //CuRectTransform.anchorMax = Vector2.zero;

        CuRectTransform.anchoredPosition = _originalPos;
    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {
        //  Debug.Log("��������  " + this.name);
        _isTouch = true;
        _originalPos = CuRectTransform.anchoredPosition;
        Vector2 scorrenPos = RectTransformUtility.WorldToScreenPoint(null, CuRectTransform.position);

        CuRectTransform.parent = Root;

        clickDir = scorrenPos - obj.Position;

        //�ı丸�������Ҫ���¼����ê���λ�ã�����Ŀǰ������Ϊê�㣬����min max����zero
        CuRectTransform.anchorMin= Vector2.zero;
        CuRectTransform.anchorMax = Vector2.zero;

        CuRectTransform.anchoredPosition = obj.Position + clickDir;

      

        Debug.Log($"clickDir is {clickDir}");

      

       
    }
   
  
    /// <summary>
    /// ��ײ��Բ�κ�,�ò�����ʧ������һ��ʱ�������������Ļ��
    /// ��ʧ֮��Ŵ�����һ������
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
