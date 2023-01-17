using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TuioBtn : MonoBehaviour
{

    private ListenTuio _listenTuio;


    private Vector2 _centerDir;

    private Vector2 _pointEnter;

    private Vector2 _pointExit;

    private Vector2 _originalPos;

    private int _idEnter=-1;

    private List<int> _ids = new List<int>();
    private void Awake()
    {
        _listenTuio = this.GetComponent<ListenTuio>();

        _listenTuio.PointEnter += _listenTuio_PointEnter;






        _originalPos = this.GetComponent<Image>().rectTransform.anchoredPosition;

        _centerDir = new Vector2(Screen.width / 2f, Screen.height / 2f) - _originalPos;

        _centerDir = Vector3.Normalize(_centerDir);

    }

    private void Start()
    {
        TuioManager.Instance.ExitEvent += _listenTuio_PointExit;
    }

    private void Update()
    {
        if (moveImage != null)
        {
            moveImage.rectTransform.anchoredPosition += moveDir * Time.deltaTime * Speed;

            Vector2 pos = moveImage.rectTransform.anchoredPosition;

            if (pos.x <= 0 || pos.y <= 0 || pos.x >= Screen.width || pos.y >= Screen.height)
            {
                Destroy(moveImage.gameObject);
                moveImage = null;
            }
        }
    }


    public float Speed = 2f;

    private Image moveImage;

    private Vector2 moveDir;
    private void _listenTuio_PointExit(EventInfo info)
    {

        if (_idEnter != info.ID) return;

        _pointExit = info.ExitPos;

        Vector2 dir = _pointExit - _pointEnter;

        moveDir = Vector3.Normalize(dir);

        float v = Vector2.Dot(moveDir, _centerDir);

        Debug.LogError("得到的值为 " + v);//

        GameObject go = new GameObject("tip_time");

        go.transform.parent = this.transform.parent;

        moveImage = go.AddComponent<Image>();

        moveImage.rectTransform.anchorMin = Vector2.zero;
        moveImage.rectTransform.anchorMax = Vector2.zero;

        moveImage.rectTransform.anchoredPosition = _originalPos;

        _idEnter = -1;
    }

    private void _listenTuio_PointEnter(EventInfo info)
    {

        _ids.Add(info.ID);

        _pointEnter = info.EnterPos;

        _idEnter = info.ID;
        Debug.LogError("enter   " + info.ID);
    }


    // Start is called before the first frame update


    // Update is called once per frame

}
