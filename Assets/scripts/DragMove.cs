using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// �϶������
/// </summary>
public class DragMove : MonoBehaviour
{

    private ListenTuio _listenTuio;

    /// <summary>
    /// �Ƿ����Բ�η�Χ
    /// </summary>
    public bool IsEnter = false;

    public RectTransform CuRectTransform { get; private set; }

    // Start is called before the first frame update
    void Start()
    {

        _listenTuio = this.GetComponent<ListenTuio>();

        CuRectTransform = this.GetComponent<RectTransform>();

        

        _listenTuio.PointEnter += _listenTuio_PointEnter;

        _listenTuio.PointExit += _listenTuio_RadarExitEvent;

        _listenTuio.OnClick += _listenTuio_OnClick;

        _listenTuio.Pointing += _listenTuio_Pointing;
    }

    private void _listenTuio_Pointing(EventInfo obj)
    {
        //Debug.Log(obj.Position);  
        CuRectTransform.anchoredPosition += obj.Delta*0.5f;



    }

    private void _listenTuio_OnClick()
    {
       
    }

    private void _listenTuio_RadarExitEvent(EventInfo obj)
    {
     //  Debug.Log("�����뿪");
    }

    private void _listenTuio_PointEnter(EventInfo obj)
    {
       // Debug.Log("��������");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  
}
