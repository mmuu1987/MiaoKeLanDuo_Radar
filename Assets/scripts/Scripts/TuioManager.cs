using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TUIOsharp;
using TUIOsharp.DataProcessors;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public enum EventState
{
    None,

    /// <summary>
    /// 进入     
    /// </summary>
    Enter,

    /// <summary>
    /// 进入中
    /// </summary>
    Entering,

    /// <summary>
    /// 离开
    /// </summary>
    Exit
}

/// <summary>
/// 事件信息
/// </summary>
public class EventInfo
{
    public int ID;

    public EventState State;

    /// <summary>
    /// 触摸事件的位置
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// 进入雷达的时候的位置
    /// </summary>
    public Vector2 EnterPos;

    /// <summary>
    /// 离开雷达的时候的位置
    /// </summary>
    public Vector2 ExitPos;

    /// <summary>
    /// 进入时间
    /// </summary>
    public float EnterTime;

    /// <summary>
    /// 离开时间
    /// </summary>
    public float ExitTime;

    public Vector2 Delta;

    public Vector2 PreviousPos;

    public EventInfo(int id = -1)
    {
        ID = id;
        Position = Vector2.zero;
        EnterPos = Vector2.zero;
        ExitPos = Vector2.zero;
        Delta = Vector2.zero;
        PreviousPos = Vector2.zero;
        State = EventState.None;
        EnterTime = -1f;
        ExitTime = -1f;
    }
}

/// <summary>
/// 雷达管理器，获取的点位信息是左下角为参考系，这一点很重要
/// </summary>
public class TuioManager : MonoBehaviour
{
    //public enum MessageType
    //{
    //    TUIO,
    //    OSC
    //};

    private int port = 3333;

    //public static MessageType messageType = MessageType.TUIO;
    private bool degs = false;

    private bool invertX = false;
    private bool invertY = true;

    public static TuioManager Instance;

    private TuioServer _tuioServer;
    private int _screenWidth;
    private int _screenHeight;

    private List<EventInfo> _eventInfos = new List<EventInfo>();

    /// <summary>
    /// 当前雷达接收到触摸的个数
    /// </summary>
    public int TouchCount
    {
        get
        {
            return _eventInfos.Count;
        }
    }

    public bool IsShowTip = true;

    private static WaitForEndOfFrame _waitForEndOfFrame;
    private Coroutine _coroutine;

    /// <summary>
    /// 触摸进入雷达监测范围事件
    /// </summary>
    public event Action<EventInfo> EnterEvent;

    private List<IClick> _clicks = new List<IClick>();

    /// <summary>
    /// 雷达持续监测到触摸进入监测范围
    /// </summary>
    public event Action<EventInfo> EnteringEvent;

    /// <summary>
    /// 雷达监测到了触摸离开监测范围
    /// </summary>
    public event Action<EventInfo> ExitEvent;

    private List<Image> _tipObjects = new List<Image>();

    private Vector2 delta;

    private Vector2 _previousPos;

    private void Connect()
    {
        if (!Application.isPlaying) return;

        if (_tuioServer != null) Disconnect();
        if (Instance != null) throw new UnityException("不允许多个单例");
        Instance = this;

        _tuioServer = new TuioServer(port);

        _waitForEndOfFrame = new WaitForEndOfFrame();
      
        Debug.Log("TUIO Port" + port);
        _tuioServer.Connect();
        Debug.Log("TUIO Connect");
    }

    /// <summary>
    /// 移除的索引   
    /// </summary>
    private List<EventInfo> _exitIndexs = new List<EventInfo>();

    /// <summary>
    /// 把从雷达获取的数据，放到unity主线程来处理
    /// </summary>
    /// <returns></returns>
    private void Update()
    {
        
          
       
            _exitIndexs.Clear();
            for (int i = 0; i < _eventInfos.Count; i++)
            {
                if (i < 0) break;
                EventInfo info = _eventInfos[i];

                if (info == null)
                {
                    continue;
                }
                switch (info.State)
                {
                    case EventState.None:
                        break;
                    //tuio服务器有时候来不及更新状态，会调用几次enter，所以，我们在unity主线程里，主动出击，当有事件进来后，我们主动改状态为entering
                    case EventState.Enter:
                        info.EnterTime = Time.realtimeSinceStartup;
                        if (EnterEvent != null) EnterEvent(info);

                        
                        info.State = EventState.Entering;//主动改状态,在下一个循环进行更新  
                        _eventInfos[i] = info;

                        List<IClick> enterList = new List<IClick>();

                        foreach (IClick click in _clicks)
                        {
                            if (click.IsContains(info))
                            {
                                enterList.Add(click);
                               // Debug.Log("添加到点击范围判断");
                            }
                        }

                        enterList.Sort(((clickA, clickB) =>
                        {
                            if (clickA.GetIndex() >= clickB.GetIndex())
                            {
                                return -1;
                            }
                            
                            return 1;
                        }));

                        if(enterList.Count>0)
                         enterList.First().Click(info);
                        //else Debug.Log("没有ui点击");
                        
                        //Debug.Log(EventInfos.Count);
                       // Debug.Log("id " + info.ID + "  Enter. position is " + info.EnterPos);
                        break;

                    case EventState.Entering:
                        if (EnteringEvent != null) EnteringEvent(info);
                       // Debug.Log("id " + info.ID + "  Entering . position is " );
                        break;

                    case EventState.Exit:
                        _exitIndexs.Add(info);
                        info.ExitTime = Time.realtimeSinceStartup;
                        
                        if (ExitEvent != null) ExitEvent(info);
                       //  Debug.Log("id " + info.ID + "  exit . position is " + info.ExitPos);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (IsShowTip)
                for (int i = 0; i < _eventInfos.Count; i++)
                {
                    EventInfo info = _eventInfos[i];

                    Vector3 pos = info.Position;

                    Image tip = _tipObjects[i];
                    tip.rectTransform.sizeDelta = Vector2.one*50f;
                    tip.transform.position = pos;
                    if (!tip.gameObject.activeInHierarchy) tip.gameObject.SetActive(true);
                }

            foreach (EventInfo info in _exitIndexs)
            {
                if(_eventInfos.Contains(info))
                  _eventInfos.Remove(info);
            }



            for (int i = 0; i < _eventInfos.Count; i++)
            {
                _eventInfos[i].Delta = Vector2.zero;
            }
        
    }

    public void AddClick(IClick click)
    {
        _clicks.Add(click);
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButton(0))
    //    {
    //        Debug.Log(Input.mousePosition);
    //    }
    //}
    private void OnEnable()
    {
        Debug.Log(string.Format("TUIO listening on port {0}... (Press escape to quit)", port));

        if (IsShowTip)
        {
            if (_tipObjects.Count == 0)
            {
                GameObject prefab = new GameObject("tip", typeof(Image));

                Canvas canvas = FindObjectOfType<Canvas>();

                for (int i = 0; i < 15; i++)
                {
                    GameObject go = Instantiate(prefab, canvas.transform);

                    Image image = go.GetComponent<Image>();

                    image.rectTransform.position = Vector3.zero;

                    _tipObjects.Add(image);

                    go.SetActive(false);
                }
            }
        }

        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        // tuio

        CursorProcessor cursorProcessor = new CursorProcessor();
        cursorProcessor.CursorAdded += OnCursorAdded;
        cursorProcessor.CursorUpdated += OnCursorUpdated;
        cursorProcessor.CursorRemoved += OnCursorRemoved;

        //BlobProcessor blobProcessor = new BlobProcessor();
        //blobProcessor.BlobAdded += OnBlobAdded;
        //blobProcessor.BlobUpdated += OnBlobUpdated;
        //blobProcessor.BlobRemoved += OnBlobRemoved;

        //ObjectProcessor objectProcessor = new ObjectProcessor();
        //objectProcessor.ObjectAdded += OnObjectAdded;
        //objectProcessor.ObjectUpdated += OnObjectUpdated;
        //objectProcessor.ObjectRemoved += OnObjectRemoved;

        // listen...
        Connect();
        _tuioServer.AddDataProcessor(cursorProcessor);
        //_tuioServer.AddDataProcessor(blobProcessor);
        //_tuioServer.AddDataProcessor(objectProcessor);

        Debug.Log("connect");
    }

    protected void OnDisable()
    {
        Disconnect();
    }

    #region 光标 和  一团

    private void OnCursorAdded(object sender, TuioCursorEventArgs e)
    {
        var entity = e.Cursor;
        lock (_tuioServer)
        {
            //var x = invertX ? (1 - entity.X) : entity.X;
            //var y = invertY ? (1 - entity.Y) : entity.Y;
            var x = entity.X * _screenWidth;
            var y = (1 - entity.Y) * _screenHeight;
           // Debug.Log(string.Format("{0} Cursor Added {1}      :{2} , {3}", ((CursorProcessor)sender).FrameNumber, entity.Id, x, y));

            Vector2 pos = new Vector2(x, y);
            int id = e.Cursor.Id;
            
            bool isContains = false;
            for (int i = 0; i < _eventInfos.Count; i++)
            {
                if (_eventInfos[i].ID == id)
                {
                    isContains = true;
                    break;
                }
            }
            if (!isContains)
            {
                EventInfo info = new EventInfo();
                info.ID = id;
                info.State = EventState.Enter;
                info.EnterPos = pos;
                info.Position = pos;
                info.PreviousPos = pos;
                info.Delta = Vector2.zero;
                _eventInfos.Add(info);
            }
            else
            {
                Debug.LogError("没有清空离开的ID " + id);
            }
        }
        //Debug.Log("OnCursorAdded");
    }

    private void OnCursorUpdated(object sender, TuioCursorEventArgs e)
    {
        var entity = e.Cursor;
        lock (_tuioServer)
        {
            //var x = invertX ? (1 - entity.X) : entity.X;
            //var y = invertY ? (1 - entity.Y) : entity.Y;
            var x = (entity.X * _screenWidth);
            var y = (1 - entity.Y) * _screenHeight;

            int id = e.Cursor.Id;
            Vector2 pos = new Vector2(x, y);
            // Debug.Log(string.Format("{0} Cursor Moved {1}:{2},{3}", ((CursorProcessor)sender).FrameNumber, entity.Id, x, y));
            //MyTest.Instance.LimitGetPos(remapCoordinates(new Vector2(x, y)));
            //项目中测试方法
            //MyTest.Instance.LimitGetPos(new Vector2(x, y));

            EventInfo info = new EventInfo();
            int index = -1;
            for (int i = 0; i < _eventInfos.Count; i++)
            {
                if (_eventInfos[i].ID == id)
                {
                    info = _eventInfos[i];
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                info.State = EventState.Entering;

                info.Delta =  pos - info.PreviousPos;
              
                //Debug.Log("Delta is " + info.Delta);  

                info.PreviousPos = info.Position;
                info.Position = pos;

                _eventInfos[index] = info;
            }
            else
            {
                Debug.LogError("没有经过OnObjectAdded接口进来的野生ID");
            }

        }
        //Debug.Log("OnCursorUpdated");
    }

    private void OnCursorRemoved(object sender, TuioCursorEventArgs e)
    {
        var entity = e.Cursor;
        lock (_tuioServer)
        {

            var x = entity.X * _screenWidth;
            var y = (1 - entity.Y) * _screenHeight;

           // Debug.Log(string.Format("{0} Cursor Removed {1}", ((CursorProcessor)sender).FrameNumber, entity.Id));
          


            EventInfo info = new EventInfo();
            Vector2 pos = new Vector2(x, y);
            int index = -1;
            for (int i = 0; i < _eventInfos.Count; i++)
            {
                if (_eventInfos[i].ID == entity.Id)
                {
                    info = _eventInfos[i];
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                info.State = EventState.Exit;
                info.Position = pos;
                info.ExitPos = pos;
                _eventInfos[index] = info;
                // Debug.Log("exit id is " + id + "  index is " + index);
            }
            else
            {
                Debug.LogError("没有经过OnObjectAdded接口进来的野生ID");
            }

            //Debug.Log("OnCursorRemove");
        }
    }

    private void OnBlobAdded(object sender, TuioBlobEventArgs e)
    {
        var entity = e.Blob;
        lock (_tuioServer)
        {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            Debug.Log(string.Format("{0} Blob Added {1}:{2},{3} {4:F3}", ((BlobProcessor)sender).FrameNumber, entity.Id, x, y, angle));
        }
        Debug.Log("OnBlobAdded");
    }

    private void OnBlobUpdated(object sender, TuioBlobEventArgs e)
    {
        var entity = e.Blob;
        lock (_tuioServer)
        {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            Debug.Log(string.Format("{0} Blob Moved {1}:{2},{3} {4:F3}", ((BlobProcessor)sender).FrameNumber, entity.Id, x, y, angle));
        }
        Debug.Log("OnBlobUpdated");
    }

    private void OnBlobRemoved(object sender, TuioBlobEventArgs e)
    {
        var entity = e.Blob;
        lock (_tuioServer)
        {
            Debug.Log(string.Format("{0} Blob Removed {1}", ((BlobProcessor)sender).FrameNumber, entity.Id));
        }
        Debug.Log("OnBlobRemoved");
    }

    #endregion 暂时不用的


    #region  物体

    private void OnObjectAdded(object sender, TuioObjectEventArgs e)
    {
        var entity = e.Object;
        int id;
        Vector2 pos;

        lock (_tuioServer)
        {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            id = entity.ClassId;
            pos = new Vector2(_screenWidth * x, _screenHeight * y);
            Debug.Log(string.Format("{0} Object Added {1}/{2}:{3},{4} {5:F3}", ((ObjectProcessor)sender).FrameNumber, entity.ClassId, entity.Id, pos.x, pos.y, angle));
            //Debug.Log("OnObjectAdded " + id);

            bool isContains = false;
            for (int i = 0; i < _eventInfos.Count; i++)
            {
                if (_eventInfos[i].ID == id)
                {
                    isContains = true;
                    break;
                }
            }
            if (!isContains)
            {
                EventInfo info = new EventInfo();
                info.ID = id;
                info.State = EventState.Enter;
                info.EnterPos = pos;
                info.Position = pos;
                _eventInfos.Add(info);
            }
            else
            {
                Debug.LogError("没有清空离开的ID " + id);
            }
        }
    }

    private void OnObjectUpdated(object sender, TuioObjectEventArgs e)
    {
        var entity = e.Object;
        int id;
        Vector2 pos;
        lock (_tuioServer)
        {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            id = entity.ClassId;
            pos = new Vector2(_screenWidth * x, _screenHeight * y);
            // Debug.Log(pos);
            //Debug.Log(string.Format("{0} Object Moved {1}/{2}:{3},{4} {5:F3}", ((ObjectProcessor)sender).FrameNumber, entity.ClassId, entity.Id, pos.x, pos.y, angle));

            EventInfo info = new EventInfo();
            int index = -1;
            for (int i = 0; i < _eventInfos.Count; i++)
            {
                if (_eventInfos[i].ID == id)
                {
                    info = _eventInfos[i];
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                info.State = EventState.Entering;
                info.Position = pos;

                _eventInfos[index] = info;
            }
            else
            {
                Debug.LogError("没有经过OnObjectAdded接口进来的野生ID");
            }
        }
    }

    private void OnObjectRemoved(object sender, TuioObjectEventArgs e)
    {
        //Debug.Log("OnObjectRemoved");
        var entity = e.Object;
        int id;
        Vector2 pos;
        lock (_tuioServer)
        {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            id = entity.ClassId;
            pos = new Vector2(_screenWidth * x, _screenHeight * y);
            // Debug.Log(string.Format("{0} Object Removed {1}/{2}:{3},{4} {5:F3}", ((ObjectProcessor)sender).FrameNumber, entity.ClassId, entity.Id, pos.x, pos.y, angle));
            //Debug.Log("OnObjectRemoved " + id);

            EventInfo info = new EventInfo();
            int index = -1;
            for (int i = 0; i < _eventInfos.Count; i++)
            {
                if (_eventInfos[i].ID == id)
                {
                    info = _eventInfos[i];
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                info.State = EventState.Exit;
                info.Position = pos;
                info.ExitPos = pos;
                _eventInfos[index] = info;
                // Debug.Log("exit id is " + id + "  index is " + index);
            }
            else
            {
                Debug.LogError("没有经过OnObjectAdded接口进来的野生ID");
            }
        }
    }

    #endregion


    private void Disconnect()
    {
        if (_tuioServer != null)
        {
            _tuioServer.RemoveAllDataProcessors();
            _tuioServer.Disconnect();
            _tuioServer = null;
            _waitForEndOfFrame = null;
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = null;
            Instance = null;
        }
    }
}