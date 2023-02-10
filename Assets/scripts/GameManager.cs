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
    /// �Ƿ��Ǵ���
    /// </summary>
    private bool _isStandBy = true;


    private float _standbyTime;

    public float StandbyMaxTime = 5f;

   
    /// <summary>
    /// ����Ƿ����÷�Χ 
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
                    isEnter = circle.IsEnter(item.RectTransform);//�жϲ����Ƿ��Ͻ��˲���

                    if (isEnter)//��������Ͻ��˲���
                    {
                        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, circle.transform.position);
                        item.ColliderEvent(screenPos, (() =>
                        {

                            CuisineInfoItem fit = null;

                            bool isMatch = IsMatching(circle.name, item.name, out fit);


                            //������ײ����¼�

                            if (isMatch)
                            {
                                Debug.Log("ƥ��ɹ�");
                                circle.SetTrigger(true);
                            }
                            else
                            {
                                Debug.Log("ƥ��ʧ��");
                                circle.SetTrigger(false);
                            }


                            _triggerCircles.Add(circle);//ƥ�䵽�ķŵ��б�

                            Debug.Log($"�����ԭ����{item.name},��ʽ��{circle.name}����Ҫ��ԭ�����ǣ�{fit.NeedFlavouring}");
                        }));

                       
                        
                      
                    }

                }

            }
        }
    }

    /// <summary>
    /// �Ƿ���ʾ�ò�ʽ����Ϣ
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
        //    Debug.Log($"��������������������UI{TestListenTuio.name}");
        //};
    }

    /// <summary>
    /// Ϊ������ת��Ӻ������򣬻��߻ظ���������
    /// </summary>
    /// <param name="arg1">���Ĳ�ʽ�Ƿ����trueΪ��ӣ�falseΪȡ��</param>
    /// <param name="arg2">���Ĳ�ʽ��RectTransform���</param>
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
    /// ��ʼ������
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

        Debug.Log($"�ܹ�{Circles.Count}�ֲ�ʽ��ƥ�䵽jsonת��������{count}��");
    }
    private void LoadInfo()
    {
        string path = Application.streamingAssetsPath + "/Info.json";

       // byte[] bytes = File.ReadAllText(path);

        string temp = File.ReadAllText(path,Encoding.UTF8);

        _rootInfo = JsonMapper.ToObject<Root>(temp);

        Debug.Log("���ز�����Ϣ�ɹ� "+_rootInfo.CuisineInfo[0].CuisineName);
    }

    /// <summary>
    /// ��ʽ��ԭ���Ƿ�ƥ��
    /// </summary>
    /// <param name="cuisineName"></param>
    /// <param name="needFlavouring"></param>
    /// <returns></returns>
    private bool IsMatching(string cuisineName,string needFlavouring,out CuisineInfoItem fit)
    {

        fit = null;
        foreach (CuisineInfoItem item in _rootInfo.CuisineInfo)
        {
            if (item.CuisineName == cuisineName)//��ʽ��ƥ���� 
            {
                if (item.NeedFlavouring == needFlavouring)//ԭ��Ҳƥ����
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
          // Debug.Log("����ʱ��");
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
/// ��ʽ��Ϣ
/// </summary>
public class CuisineInfoItem
{

    
    /// <summary>
    /// ��ʽ����
    /// </summary>
    public string CuisineName="";

    /// <summary>
    /// ��ʽ����
    /// </summary>
    public string Description = "";

    /// <summary>
    /// ��Ҫ�ĵ�ζƷ
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
