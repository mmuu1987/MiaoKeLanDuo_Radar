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
    /// �������ԭ���ϵ��淨��trueΪ������falseΪ�ر�
    /// </summary>
    public Action<bool, RectTransform> AdddFlavouring;

    public List<RectTransform> FoodPartList = new List<RectTransform>();

    public float maxDis = 100;

    public float minDis = 0f;

    public GameObject DragItem;

    private ListenTuio _listenTuio;

    /// <summary>
    /// �ر��淨
    /// </summary>
    private Coroutine _close;

    private Vector2 centerPos;

    /// <summary>
    /// ���ؿ�ʼ��ʱ���¼�ģ����ص㵽Բ��(�״�)������
    /// </summary>
    private Vector2 _enterDir;

    /// <summary>
    /// ���ؿ�ʼ��ʱ�򣬼�¼���ص��λ��
    /// </summary>
    private Vector2 _enterPos;


    /// <summary>
    /// ���ؽ������뿪�����ʱ�򣬼�¼���ؿ�ʼ��������������
    /// </summary>
    private Vector2 _exitDir;


    /// <summary>
    /// �Ƿ���ԲȦ��Χ�ڴ������
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

            Vector2 disDir = q * Vector2.up * 200f;//180���ǽǶȣ����Ǿ���

            GameObject go = Instantiate(DragItem, RotationRectTransform.transform);

            ItemDishes itemDishes = go.GetComponent<ItemDishes>();

            itemDishes.name = FoodPartList[i].name;//��������һ��


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
        //Debug.Log("�������� λ���� " + obj.EnterPos);

        if (_isEnter) return;//ֻ����һ����������

        _enterId = obj.ID;
        _isEnter = true;

        this.centerPos = RectTransformUtility.WorldToScreenPoint(null, RotationRectTransform.position);

        _enterPos = obj.EnterPos;
        _enterDir = (_enterPos - centerPos).normalized;//�鵽ͬһ�������º���㷽��
        isEnterCircle = CheckRange(obj.EnterPos);
        //Debug.Log("dis is " + _enterPos);
        _curQuaternion = RotationRectTransform.rotation;
    }


    /// <summary>
    /// ����Ƿ����ڷ��ϵķ�Χ
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
       //��ʱ�ر��淨
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
        // Debug.Log("�����뿪��λ����" + obj.Position);


        isEnterCircle = CheckRange(obj.Position);

        if (!isEnterCircle)
        {
            _enterDir = Vector2.zero;
            return;
        }

        if (_enterDir == Vector2.zero) return;

        this.centerPos = RectTransformUtility.WorldToScreenPoint(null, RotationRectTransform.position);

        Vector3 newDir = (obj.Position - this.centerPos).normalized;

        //���_enterDir  _exitDir���ж���ת�ķ���

        Vector3 dir = Vector3.Cross(newDir, _enterDir);


        float angle = Vector3.Angle(newDir, _enterDir);

        if (dir.z > 0)
        {
            // Debug.Log("˳ʱ�뷽�� " + dir);
            angle *= -1;
        }
        else
        {
            //Debug.Log("��ʱ�뷽�� " + dir);
        }

        this.RotationRectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle * 3f)) * _curQuaternion;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
