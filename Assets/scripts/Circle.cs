using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class Circle : MonoBehaviour
{

    public float Radius = 200f;

    public RectTransform CuRectTransform { get; private set; }

    private Vector2 _originalPos;
    private CuisineInfoItem _curCuisineInfoItem;

    /// <summary>
    /// 当前初始化的信息
    /// </summary>
    private GameObject _info;

    private Coroutine _waiTimecCoroutine;

    /// <summary>
    /// 该UI的屏幕坐标位置
    /// </summary>
    private Vector2 _screenPos;

    private Scrollbar _scrollbar;
    /// <summary>
    /// 是否触发了碰撞事件
    /// </summary>
    public bool IsRight { get; private set; } = false;

    /// <summary>
    /// 是否有材料拖进了菜盘
    /// </summary>
    public bool IsTrigger = false;

    public bool IsTest = false;
    public void Init(GameObject info, CuisineInfoItem item)
    {

        _curCuisineInfoItem = item;

         _info = Instantiate(info, this.transform);

         _info.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -188.9f);

         _info.transform.Find("Close").GetComponent<ListenTuio>().OnClick += Circle_OnClick;

         _info.transform.Find("Up").GetComponent<ListenTuio>().OnClick += Up;

         _info.transform.Find("Down").GetComponent<ListenTuio>().OnClick += Down;

         _scrollbar = _info.transform.Find("Scroll View/Scrollbar Vertical").GetComponent<Scrollbar>();

         _info.transform.Find("Title").GetComponent<Text>().text = item.CuisineName;

         _info.transform.Find("Scroll View/Viewport/Content/Text").GetComponent<Text>().text = item.Description;

        _info.SetActive(false);
    }

    private void Down()
    {
       // Debug.Log("Down");
        _scrollbar.value += 0.2f;


    }

    private void Up()
    {
        //Debug.Log("Up");
        _scrollbar.value -= 0.2f;
    }

    private void Circle_OnClick()
    {
        Debug.Log("Circle_OnClick");

        if (_waiTimecCoroutine != null) StopCoroutine(_waiTimecCoroutine);
        _info.gameObject.SetActive(false);

        CuRectTransform.DOAnchorPos(_originalPos, 0.35f).OnComplete((() =>
        {
            IsTrigger = false; //重新激活菜盘

        }));
       


    }
    public bool IsEnter(RectTransform item)
    {
        if (IsTrigger) return false;

         _screenPos = RectTransformUtility.WorldToScreenPoint(null, CuRectTransform.transform.position);
        

        float dis = Vector2.Distance(_screenPos, item.anchoredPosition);

        if (dis >= Radius) return false;
        IsTrigger = true;
        Debug.Log("dragmove " + item.name + "  进入了circle " + this.name);
        return true;
    }

    public void SetTrigger(bool isTrigger)
    {
        IsRight = isTrigger;

        //Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, CuRectTransform.transform.position);

        //Vector2 dir = screenPos-new Vector2(Screen.width/2f,Screen.height/2f);

        //dir = dir.normalized;

        //设置方向
        Vector2 dir = new Vector2(Screen.width / 2f, Screen.height / 2f) - CuRectTransform.anchoredPosition;

        dir = Vector3.Normalize(dir);

       // CuRectTransform.up = dir;


        Vector2 targetPos = CuRectTransform.anchoredPosition + dir * 150f;
        CuRectTransform.DOAnchorPos(targetPos, 0.35f);

        ShowInfo();
    }

    private void ShowInfo()
    {
        //材料进来后，两种显示形式，一种是正确的表现，一种是不正确的表现

        if (IsRight)
        {
            //匹配正确则显示菜式信息
            _info.gameObject.SetActive(true);
        }
        else
        {
            //匹配不正确则显示黑暗料理
        }

        if(_waiTimecCoroutine!=null)StopCoroutine(_waiTimecCoroutine);
        _waiTimecCoroutine = StartCoroutine(GlobSetting.WaitTime(10f, (Circle_OnClick)));
    }
    // Start is called before the first frame update
    void Start()
    {
        CuRectTransform = this.GetComponent<RectTransform>();

        //设置方向
        Vector3 dir = new Vector2(Screen.width / 2f, Screen.height / 2f) - CuRectTransform.anchoredPosition;

        dir = Vector3.Normalize(dir);

        CuRectTransform.up = dir;
        _originalPos = CuRectTransform.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //private void OnGUI()
    //{
    //    if (IsTest)
    //    {
    //        if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "test"))
    //        {
    //           // CuRectTransform.
    //        }
    //    }
       
    //}
}
