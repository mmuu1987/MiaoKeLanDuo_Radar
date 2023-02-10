using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{

    public List<DragMove> DragMoves = new List<DragMove>();

    public List<Item> Items = new List<Item>();
    


    public List<Circle> Circles = new List<Circle>();

    public List<CoreDishes> CoreDisheses = new List<CoreDishes>();

    public GameObject InfoGameObject;

    public ListenTuio TestListenTuio;

    private List<Circle> _triggerCircles = new List<Circle>();
    private Root _rootInfo;

    public VideoPlayer StandByVideoPlayer;

    public DragRotation DragRotation;
    /// <summary>
    /// 是否是待机
    /// </summary>
    private bool _isStandBy = true;


    private float _standbyTime;

    public float StandbyMaxTime = 5f;

   
    /// <summary>
    /// 检测是否进入该范围 
    /// </summary>
    private void CheckRang()
    {
        

        foreach (Circle circle in Circles)
        {
            bool isEnter = false;
            foreach (Item item in Items)
            {
                if (!item.IsEnter)
                {
                    isEnter = circle.IsEnter(item.RectTransform);//判断材料是否拖进了菜盘

                    if (isEnter)//如果材料拖进了菜盘
                    {
                        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, circle.transform.position);
                        item.ColliderEvent(screenPos, (() =>
                        {

                            CuisineInfoItem fit = null;

                            bool isMatch = IsMatching(circle.name, item.name, out fit);


                            //触发碰撞后的事件

                            if (isMatch)
                            {
                                Debug.Log("匹配成功");
                                circle.SetTrigger(true);
                            }
                            else
                            {
                                Debug.Log("匹配失败");
                                circle.SetTrigger(false);
                            }


                            _triggerCircles.Add(circle);//匹配到的放到列表

                            Debug.Log($"放入的原料是{item.name},菜式是{circle.name}，需要的原材料是：{fit.NeedFlavouring}");
                        }));

                       
                        
                      
                    }

                }

            }
        }
    }

    /// <summary>
    /// 是否显示该菜式的信息
    /// </summary>
    private void ShowInfo(bool isRight)
    {

    }
    void Start()
    {

        StandByVideoPlayer.url = Application.streamingAssetsPath + "/standBy.mp4";

        LoadInfo();


        Init();

       // Debug.Log(CoreDisheses.Count );
        foreach (CoreDishes coreDishes in CoreDisheses)
        {
            coreDishes.AdddFlavouring += AdddFlavouring;
        }
      
        //TestListenTuio.PointEnter += (info) =>
        //{
        //    Debug.Log($"点击到了中心坐标点计算的UI{TestListenTuio.name}");
        //};
    }

    /// <summary>
    /// 为大盘旋转添加忽略区域，或者回复忽略区域
    /// </summary>
    /// <param name="arg1">核心菜式是否添加true为添加，false为取消</param>
    /// <param name="arg2">核心菜式的RectTransform组件</param>
    private void AdddFlavouring(bool arg1, RectTransform arg2)
    {
        if (arg1)
        {
            if (!DragRotation.IgnoreRect.Contains(arg2))
            {
                DragRotation.IgnoreRect.Add(arg2);
            }
        }
        else
        {
            if (DragRotation.IgnoreRect.Contains(arg2))
            {
                DragRotation.IgnoreRect.Remove(arg2);
            }
        }
    }


    /// <summary>
    /// 初始化数据
    /// </summary>
    private void Init()
    {
        int count = 0;
        foreach (CuisineInfoItem item in _rootInfo.CuisineInfo)
        {
            foreach (Circle circle in Circles)
            {
                if (circle.name == item.CuisineName)
                {
                    circle.Init(InfoGameObject, item);
                    count++;
                }

            }
        }

        Debug.Log($"总共{Circles.Count}种菜式，匹配到json转化过来的{count}种");
    }
    private void LoadInfo()
    {
        string path = Application.streamingAssetsPath + "/Info.json";

       // byte[] bytes = File.ReadAllText(path);

        string temp = File.ReadAllText(path,Encoding.UTF8);

        _rootInfo = JsonMapper.ToObject<Root>(temp);

        Debug.Log("加载菜盘信息成功 "+_rootInfo.CuisineInfo[0].CuisineName);
    }

    /// <summary>
    /// 菜式跟原料是否匹配
    /// </summary>
    /// <param name="cuisineName"></param>
    /// <param name="needFlavouring"></param>
    /// <returns></returns>
    private bool IsMatching(string cuisineName,string needFlavouring,out CuisineInfoItem fit)
    {

        fit = null;
        foreach (CuisineInfoItem item in _rootInfo.CuisineInfo)
        {
            if (item.CuisineName == cuisineName)//菜式名匹配上 
            {
                if (item.NeedFlavouring == needFlavouring)//原料也匹配上
                {
                    fit = item;
                    return true;
                }

                fit = item;
            }
        }

       
        return false;
    }
    // Update is called once per frame  
    void Update()
    {
      CheckRang();

      if (!_isStandBy)
      {
          this._standbyTime += Time.deltaTime;

          if (this._standbyTime >= this.StandbyMaxTime)
          {
              _isStandBy = true;
              this.ActiveStandby(false);
          }
      }
      if (TuioManager.Instance.TouchCount>0)
      {
          if (_isStandBy)
          {
              this.ActiveStandby(true);
              _isStandBy = false;
          }
          // Debug.Log("重置时间");
          _standbyTime = 0f;
      }

    }

    private void ActiveStandby(bool isShow)
    {
        if (isShow)
        {


            StandByVideoPlayer.Stop();
            StandByVideoPlayer.gameObject.SetActive(false);
            //TCPUDPSocket.Instance.Write_UDPSenderOther("Go3");
            
        }
        else
        {

            StandByVideoPlayer.gameObject.SetActive(true);
            StandByVideoPlayer.Play();
        }

    }
}

/// <summary>
/// 菜式信息
/// </summary>
public class CuisineInfoItem
{

    
    /// <summary>
    /// 菜式名字
    /// </summary>
    public string CuisineName="";

    /// <summary>
    /// 菜式描述
    /// </summary>
    public string Description = "";

    /// <summary>
    /// 需要的调味品
    /// </summary>
    public string NeedFlavouring = "";
}

public class Root
{
    /// <summary>
    /// 
    /// </summary>
    public List<CuisineInfoItem> CuisineInfo { get; set; }
}
