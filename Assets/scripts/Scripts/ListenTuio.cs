using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     目前只支持Camera的Render Mode = Screen Space - Overly
/// </summary>
public class ListenTuio : MonoBehaviour,IClick
{
    /// <summary>
    ///     触摸是否进入
    /// </summary>
    private bool _isEnter;

    /// <summary>
    /// 进入的事件雷达事件ID
    /// </summary>
    private int _enterId;

    /// <summary>
    ///     触摸出现的时候，是否是首先进入该监听区域
    /// </summary>
    private bool _isFirstEnter;

    private Vector2 _leftDown;
    private Vector2 _leftUp;
    private MaskableGraphic _maskableGraphic;

    private Vector2 _pos;
    private Vector2 _rightDown;
    private Vector2 _rightUp;
    private Vector2 _sizeData;

    public event Action OnClick;

    public event Action<EventInfo> PointEnter;

    public event Action<EventInfo> PointExit;

    public event Action<EventInfo> Pointing;

    public event Action<EventInfo> RadarExitEvent; 

   

    private void EnterEvent(EventInfo info)
    {
        //Debug.Log("enter");

        var pos = info.Position;

        var isContains = GlobSetting.ContainsQuadrangle(_leftDown, _leftUp, _rightDown, _rightUp, pos);

        if (isContains)
        {
           // Debug.LogError("EnterEvent  触摸进入了该组件" + name);
            _isEnter = true;
            if (PointEnter != null) PointEnter(info);
            _isFirstEnter = true;
        }
        else
        {
            _isEnter = false;
        }
    }

    private void PointEnterEvent(EventInfo info)
    {
       

        if (_isEnter) return;//已经有触摸ID进入，其他的都触摸ID进入就不会触发

       // Debug.Log($"EnterEvent  触摸 进入 了该组件 {name} id 是 {info.ID}");
        _isEnter = true;
        if (PointEnter != null) PointEnter(info);
        _isFirstEnter = true;
        _enterId = info.ID;
    }

    private void Awake()
    {
        CuRectTransform = this.GetComponent<RectTransform>();
        _isEnter = false;
    }

    
    private void EnteringEvent(EventInfo info)
    {
        if (!_isEnter) return;

        if (info.ID != _enterId) return;

       

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, CuRectTransform.transform.position);

        UpdatePosition(screenPoint);

        var isContains = GlobSetting.ContainsQuadrangle(_leftDown, _leftUp, _rightDown, _rightUp, info.Position);

        if (isContains)//触摸点是否包含在UI的方框里
        {
            if (!_isEnter)
                //Debug.Log("EnteringEvent 进入了该组件" + this.name);
                if (PointEnter != null)
                    PointEnter(info);

            _isEnter = true;

            if (Pointing != null) Pointing(info);
            

        }
        else
        {
            //if (_isEnter)
           // Debug.Log($"EnteringEvent  触摸 离开 了该组件 {name} id 是 {info.ID}");
            if (PointExit != null)
                    PointExit(info);
            _isEnter = false;
        }
    }

    private void ExitEvent(EventInfo info)
    {

        if (!_isEnter) return;

        //if (info.ID != _enterId) return;


        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, CuRectTransform.transform.position);
        UpdatePosition(screenPoint);

        var isContains = GlobSetting.ContainsQuadrangle(_leftDown, _leftUp, _rightDown, _rightUp, info.ExitPos);


        if (isContains)//触摸点是否包含在UI的方框里
        {
            if (_isFirstEnter)
            {
                if (OnClick != null)
                    OnClick();
             
                _isFirstEnter = false;
              //  Debug.Log($"EnterEvent  触摸 离开 了该组件 {name} id 是 {info.ID},并触发了 点击 事件");
                if (PointExit != null)
                    PointExit(info);
                _isEnter = false;
            }

           
            
        }
        else
        {
            if (_isEnter)
            {
              //  Debug.Log($"EnterEvent  触摸 离开 了该组件 {name} id 是 {info.ID}");
                if (PointExit != null)
                    PointExit(info);
                _isEnter = false;
            }
        }
        
    }

    private void UpdatePosition()
    {
        _sizeData = _maskableGraphic.rectTransform.sizeDelta;

        _pos = _maskableGraphic.rectTransform.position;

        _leftDown = _pos + new Vector2(-_sizeData.x / 2, -_sizeData.y / 2);

        _leftUp = _pos + new Vector2(-_sizeData.x / 2, _sizeData.y / 2);

        _rightDown = _pos + new Vector2(_sizeData.x / 2, -_sizeData.y / 2);

        _rightUp = _pos + new Vector2(_sizeData.x / 2, _sizeData.y / 2);

       // RectTransformUtility.ScreenPointToWorldPointInRectangle()
    }

    private void UpdatePosition(Vector3 screenPos)
    {
        _sizeData = _maskableGraphic.rectTransform.sizeDelta;

        _pos = screenPos;

        _leftDown = _pos + new Vector2(-_sizeData.x / 2, -_sizeData.y / 2);

        _leftUp = _pos + new Vector2(-_sizeData.x / 2, _sizeData.y / 2);

        _rightDown = _pos + new Vector2(_sizeData.x / 2, -_sizeData.y / 2);

        _rightUp = _pos + new Vector2(_sizeData.x / 2, _sizeData.y / 2);

        // RectTransformUtility.ScreenPointToWorldPointInRectangle()
    }

    private void Start()
    {
        _maskableGraphic = GetComponent<MaskableGraphic>();
        //TuioManager.Instance.EnterEvent += EnterEvent;
        Click += PointEnterEvent;
        TuioManager.Instance.EnteringEvent += EnteringEvent;
        TuioManager.Instance.ExitEvent += ExitEvent;

       TuioManager.Instance.AddClick(this);

        // Debug.Log(_sizeData);
    }

    // Update is called once per frame
    private void Update()
    {
        //UpdatePosition();
    }

    public RectTransform CuRectTransform { get; set; }
    public int GetIndex()
    {
        return CuRectTransform.GetSiblingIndex();
    }

   
    public Action<EventInfo> Click { get; set; }
    bool IClick.IsContains(EventInfo info)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, CuRectTransform.transform.position);
        UpdatePosition(screenPoint);
       
         return   GlobSetting.ContainsQuadrangle(_leftDown, _leftUp, _rightDown, _rightUp, info.EnterPos);
       
       // return GlobSetting.ContainsQuadrangle(_leftDown, _leftUp, _rightDown, _rightUp, info.EnterPos);
    }

   
}


public interface IClick
{
    int GetIndex();

    Action<EventInfo> Click
    {
        get;
        set;
    }
        
    bool IsContains(EventInfo info);
   
}