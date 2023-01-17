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

    public float RotationSpeed = 0.1f;


    private Quaternion _curQuaternion;

    /// <summary>
    /// �Ƿ���ԲȦ��Χ�ڴ������
    /// </summary>
    private bool isEnterCircle;
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
    }

    private void EnteringEvent(EventInfo obj)
    {
        //Debug.Log("���������� "+ obj.Position) ;
        Rotation(obj);
    }
 
    private void _listenTuio_RadarExitEvent(EventInfo obj)
    {
       // Debug.Log("�����뿪��λ����" + obj.ExitPos);

        Rotation(obj);
    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {

        Debug.Log("�������� λ���� " + obj.EnterPos);
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
      //  Debug.Log("�����뿪��λ����" + obj.ExitPos);

       if (!isEnterCircle) return;

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
