using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �����״��ṩ����Ϣ�����϶�ת����ͼƬ����ת
/// </summary>
public class DragRotation : MonoBehaviour
{
    private ListenTuio _listenTuio;


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
    public RectTransform CuRectTransform { get; private set; }

    public float maxDis=920f;

    public float minDis=750f;

    public float RotationSpeed = 0.1f;

    /// <summary>
    /// �϶���ʱ����Ե�����������ļ���
    /// </summary>
    public List<RectTransform> IgnoreRect= new List<RectTransform>();

    private Quaternion _curQuaternion;

    /// <summary>
    /// �Ƿ���ԲȦ��Χ�ڴ������
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
        Debug.Log("���ĵ�λ���ǣ� " + centerPos);
     
        TuioManager.Instance.EnterEvent += _listenTuio_PointEnter;
        TuioManager.Instance.EnteringEvent += EnteringEvent;
        TuioManager.Instance.ExitEvent += _listenTuio_RadarExitEvent;
        _isEnter = false;
    }

    private void EnteringEvent(EventInfo obj)
    {
       

        if (!_isEnter) return;

        if (obj.ID != _enterId) return;

       // Debug.Log("���������� "+ obj.Position) ;
        Rotation(obj);
    }
 
    private void _listenTuio_RadarExitEvent(EventInfo obj)
    {
      
       if (_enterId != obj.ID) return;

        Rotation(obj);
        _enterDir = Vector2.zero;
        _isEnter = false;

        // Debug.Log("�����뿪��λ����" + obj.ExitPos);

    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {
        if (_isEnter) return;//ֻ����һ����������

        _enterId = obj.ID;
        _isEnter = true;
        //Debug.Log("�������� λ���� " + obj.EnterPos);
        _enterPos = obj.EnterPos;
        _enterDir = (_enterPos - centerPos).normalized;
       
        _curQuaternion = CuRectTransform.rotation;

    }

    /// <summary>
    /// ����Ƿ����ڷ��ϵķ�Χ
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

            if (radius >= disTemp)//����ں����ƶ��Ŀ�����ƶ�
                return false;
        }

        if (dis <= maxDis & dis >= minDis) return true;
        return false;
    }
    private void Rotation(EventInfo obj)
    {
         // Debug.Log("�����뿪��λ����" + obj.Position);


        isEnterCircle= CheckRange(obj.Position);

        //Debug.Log("isEnterCircle is " + isEnterCircle);


        if (!isEnterCircle)
        {
            _enterDir = Vector2.zero;

            
            return;
        }

         if (_enterDir == Vector2.zero) return;



        Vector3 newDir  = ( obj.Position - this.centerPos).normalized;

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

        this.CuRectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle*3f)) * _curQuaternion;
    }
    // Update is called once per frame
    void Update()
    {

    }

}
