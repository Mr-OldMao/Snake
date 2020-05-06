using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 持续加速
/// </summary>
public class SpeedUpContinue : MonoBehaviour, IPointerEnterHandler
{
    public bool isContinueSpeedUpState = false;  //是否处于持续加速状态
    public Image img_Context;                    //八边形框
    public Image img_PassOnAnim;                 //引导动画

    private SnakeHead snakeHeadScript;
    private float m_OffsetZ = 0;          //img_Contextd 的欧拉角偏移量
    void Awake()
    {
        img_PassOnAnim.gameObject.SetActive(false);
    }
    void Start()
    { 
        snakeHeadScript = GameObject.FindGameObjectWithTag("PlayerHead").GetComponent<SnakeHead>();
    }

    //经过 -- 实现持续加速
    public void OnPointerEnter(PointerEventData eventData)
    {
        snakeHeadScript.SetAddSpeedState(true);  //实现加速
        isContinueSpeedUpState = true;
        img_PassOnAnim.gameObject.SetActive(false) ;
    }

    void Update()
    {
        if (isContinueSpeedUpState)
        { 
            //旋转
            m_OffsetZ += Time.deltaTime * 50; 
            m_OffsetZ %= 360; 
            img_Context.transform.localEulerAngles = new Vector3(0, 0, m_OffsetZ);
        }
    }

}
