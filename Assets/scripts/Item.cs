using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{


    public RectTransform RectTransform;


    private Vector2 _dir;

    private float _moveSpeed = 1f;

    private float _maxRadius;

    private Vector2 _circleCenter;

    private float _maxSpeed = 1f;

    private Coroutine _coroutine;
    private void Awake()
    {
        RectTransform = this.GetComponent<RectTransform>();
    }

    public void Init(float radius,Vector3 circleCenter,float maxSpeed)
    {
        if (RectTransform == null) RectTransform = this.GetComponent<RectTransform>();

        _maxRadius = radius;

        _circleCenter = circleCenter;

        _maxSpeed = maxSpeed;

        SetRandomMove();

        if(_coroutine!=null)StopCoroutine(_coroutine);
        _coroutine= StartCoroutine(RandChange());

    }
    private void SetRandomMove()
    {

        _dir = Random.insideUnitCircle.normalized;

        _moveSpeed = Random.Range(10f, _maxSpeed);

        float dis = Random.Range(0, _maxRadius);

        Vector2 pos = _circleCenter + _dir * dis;

        RectTransform.anchoredPosition = pos;


    }
    private IEnumerator RandChange()
    {

        while (true)
        {
            float randTime = Random.Range(2f, 10f);

            yield return new WaitForSeconds(randTime);


            _dir = Random.insideUnitCircle.normalized;

            _moveSpeed = Random.Range(10f, _maxSpeed);
        }
    }
    /// <summary>
    /// ¼ì²â·¶Î§
    /// </summary>
    private void CheckRange()
    {

        Vector2 add = _dir * _moveSpeed * Time.deltaTime;

        this.RectTransform.anchoredPosition += add;

        float dis = Vector3.Distance(RectTransform.anchoredPosition, _circleCenter);

        if (dis >= _maxRadius)
        {
            _dir = -_dir;

            this.RectTransform.anchoredPosition -= add;

            _moveSpeed = Random.Range(10f, _maxSpeed);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckRange();
    }
}
