using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 加速按钮
/// </summary>
public class SpeedUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float tiggerTimer = 3f;       //规定的触发时间
    public Button btn_SpeedUpContinue;   //持续加速按钮
    private float m_PressTimer;          //已按下去的时间
    private bool m_StartTime;            //开始计时
    private bool m_Tigger;               //触发开关 用于触发显示 持续加速按钮
    private SnakeHead snakeHeadScript;
    void Start()
    {
        m_StartTime = false;
        m_Tigger = false;
        m_PressTimer = 0;
        btn_SpeedUpContinue.gameObject.SetActive(false);
        snakeHeadScript = GameObject.FindGameObjectWithTag("PlayerHead").GetComponent<SnakeHead>();
    }

    //按下
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Down");
        snakeHeadScript.SetAddSpeedState(true);
        
        //处于持续加速状态
        if (btn_SpeedUpContinue.GetComponent<SpeedUpContinue>().isContinueSpeedUpState)
        {
            btn_SpeedUpContinue.gameObject.SetActive(false);
            snakeHeadScript.SetAddSpeedState(false);
            m_Tigger = false;
            btn_SpeedUpContinue.GetComponent<SpeedUpContinue>().isContinueSpeedUpState = false;
            m_PressTimer = 0;
        }
        //未触发 显示持续加速按钮 逻辑
        if (!m_Tigger)
        {
            m_StartTime = true;
        }
    }
    //抬起 
    public void OnPointerUp(PointerEventData eventData)
    { 
        //是否处于持续加速状态
        if (!btn_SpeedUpContinue.GetComponent<SpeedUpContinue>().isContinueSpeedUpState ) 
            snakeHeadScript.SetAddSpeedState(false); 
        m_StartTime = false;
        m_PressTimer = 0;
    }
    void Update()
    {
        //持续按压判定
        if (m_StartTime)
        {
            m_PressTimer += Time.deltaTime;
            if (m_PressTimer >= tiggerTimer)
            {
                m_Tigger = true;
                Debug.Log("触发长按加速逻辑");
                btn_SpeedUpContinue.gameObject.SetActive(true);
                //打开引导动画
                btn_SpeedUpContinue.GetComponent<SpeedUpContinue>().img_PassOnAnim.gameObject.SetActive(true);
                m_StartTime = false;
            }
        }
    } 
}
