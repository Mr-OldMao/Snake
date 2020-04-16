using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 蛇（蛇头、蛇身）移动
/// </summary>
public class Move : MonoBehaviour
{
    public BuildType buildType = BuildType.PC;
    public GameModel gameModel = GameModel.Old;
    public float moveDistance = 10;                   //每次移动的距离
    public float moveHZ = 0.1f;                       //移动的频率 s/次  即速度
    public bool isAddSpeedState = false;              //是否在加速状态 
    public float PC_AddSpeedNeedTimer = 1f;           //PC端加速所需的时间 
    public float Normal_Rotate = 5f;                  //普通模式下


    public List<Transform> snakePosList;            //记录蛇的行走轨迹
    public int curSnakeLength = 0;                      //当前蛇的长度 包括蛇头 

    public Image img_Head;
    public Image[] Img_SnakeBodyColor;   //0-白 1-彩色
    public AudioManager audioManagerScript;

    private float m_PressKeyTimeByPC = 0;           //PC端按下移动按键的时长
    private float m_MoveHZTimer = 0;                //移动频率计时器
    private TurnType curMoveOrient;                 //当前移动的方向
    private float statrSpeed;                       //速度初始值
    private Transform bodyParent;


    public void Start()
    {
        statrSpeed = moveHZ;
        bodyParent = GameObject.Find("Canvas").GetComponent<Transform>();

        //标记蛇头位置
        snakePosList.Add(GetComponent<RectTransform>());
        curSnakeLength = 1;
    }
    private void FixedUpdate()
    {
        //改变移动的方向
        ChangeMoveOrient();

        //移动频率
        m_MoveHZTimer += Time.deltaTime;
        if (m_MoveHZTimer >= moveHZ)
        {
            SnakeMove();   //移动
            m_MoveHZTimer = 0;
        }

        //加速逻辑
        if (buildType == BuildType.PC)
        {
            //判定PC端加速条件
            DecidePCAddSpeed();
        }
        //加速 、恢复原速
        if (isAddSpeedState && statrSpeed == moveHZ)
            AddSpeed();
        else if (!isAddSpeedState)
            ReturnBeforeSpeed();

    }

    #region 加速逻辑


    /// <summary>
    /// 加速
    /// </summary>
    private void AddSpeed()
    {
        moveHZ = statrSpeed / 5;
    }
    /// <summary>
    /// 改为原来速度
    /// </summary>
    private void ReturnBeforeSpeed()
    {
        moveHZ = statrSpeed;
    }
    /// <summary>
    /// 判定PC端加速条件
    /// </summary>
    private void DecidePCAddSpeed()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            m_PressKeyTimeByPC += Time.deltaTime;
            if (m_PressKeyTimeByPC >= PC_AddSpeedNeedTimer)
            {
                isAddSpeedState = true;
            }
        }
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) ||
            Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            m_PressKeyTimeByPC = 0;
            isAddSpeedState = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isAddSpeedState = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isAddSpeedState = false;
        }
    }

    #endregion

    #region 移动逻辑

    /// <summary>
    /// 改变移动的方向 并移动
    /// 配置移动的方式 根据两个平台   三种游戏模式
    /// PC: 经典版本 规则 只水平和竖直方向旋转
    ///     普通版本 规则 可向任意方向旋转
    /// Android
    /// </summary>
    public void ChangeMoveOrient()
    {
        //PC 
        if (buildType == BuildType.PC)
        {
            ////得到Z轴的转动角度
            //float curSnakeAngles = transform.localEulerAngles.z % 360;
            // Debug.Log("蛇头(Z轴)的转动角度：" + curSnakeAngles);
            //目标方向跟当前方向 不允许在同一直线上
            if (Input.GetKey(KeyCode.W) && curMoveOrient != TurnType.Down)
            {
                curMoveOrient = TurnType.Up;
                //旋转
                SnakeTurnImpl(TurnType.Up);
            }
            if (Input.GetKey(KeyCode.S) && curMoveOrient != TurnType.Up)
            {
                curMoveOrient = TurnType.Down;
                SnakeTurnImpl(TurnType.Down);
            }
            if (Input.GetKey(KeyCode.A) && curMoveOrient != TurnType.Right)
            {
                curMoveOrient = TurnType.Left;
                SnakeTurnImpl(TurnType.Left);
            }
            if (Input.GetKey(KeyCode.D) && curMoveOrient != TurnType.Left)
            {
                curMoveOrient = TurnType.Right;
                SnakeTurnImpl(TurnType.Right);
            }
        }
        else if (buildType == BuildType.Android)
        {
            //TODO
        }
    }

    /// <summary>
    /// 蛇头旋转实现
    /// 经典模式 规则：只水平和竖直方向旋转
    /// 普通模式 规则：运行任意转向  每次5度
    /// </summary>
    private void SnakeTurnImpl(TurnType turn)
    {
        //得到Z轴的转动角度
        float curSnakeAngles = transform.localEulerAngles.z % 360;
        Debug.Log("蛇头(Z轴)的转动角度：" + curSnakeAngles);
        switch (turn)
        {
            case TurnType.Up:
                //经典模式  规则只水平和竖直方向旋转
                if (gameModel == GameModel.Old)
                {
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                }
                //普通模式  规则：运行任意转向  每次5度
                if (gameModel == GameModel.Normal)
                {
                    if (curSnakeAngles < 180)
                    {
                        transform.localEulerAngles -= new Vector3(0, 0, Normal_Rotate);
                    }
                    else if (curSnakeAngles > 180)
                    {
                        transform.localEulerAngles += new Vector3(0, 0, Normal_Rotate);
                    }
                }
                break;
            case TurnType.Down:
                //经典模式  规则只水平和竖直方向旋转
                if (gameModel == GameModel.Old)
                {
                    transform.localEulerAngles = new Vector3(0, 0, 180);
                }
                //普通模式  规则：运行任意转向  每次5度
                if (gameModel == GameModel.Normal)
                {
                    if (curSnakeAngles >= 180)
                    {
                        transform.localEulerAngles -= new Vector3(0, 0, Normal_Rotate);
                    }
                    if (curSnakeAngles < 180)
                    {
                        transform.localEulerAngles += new Vector3(0, 0, Normal_Rotate);
                    }
                }
                break;
            case TurnType.Left:
                //经典模式  规则只水平和竖直方向旋转
                if (gameModel == GameModel.Old)
                {
                    transform.localEulerAngles = new Vector3(0, 0, 90);
                }
                //普通模式  规则：运行任意转向  每次5度
                if (gameModel == GameModel.Normal)
                {
                    if (curSnakeAngles >= 90)
                    {
                        transform.localEulerAngles -= new Vector3(0, 0, Normal_Rotate);
                    }
                    if (curSnakeAngles < 90)
                    {
                        transform.localEulerAngles += new Vector3(0, 0, Normal_Rotate);
                    }
                }
                break;
            case TurnType.Right:
                //经典模式  规则只水平和竖直方向旋转
                if (gameModel == GameModel.Old)
                {
                    transform.localEulerAngles = new Vector3(0, 0, 270);
                }
                //普通模式  规则：运行任意转向  每次5度
                if (gameModel == GameModel.Normal)
                {
                    if (curSnakeAngles >= 270)
                    {
                        transform.localEulerAngles -= new Vector3(0, 0, Normal_Rotate);
                    }
                    if (curSnakeAngles < 270)
                    {
                        transform.localEulerAngles += new Vector3(0, 0, Normal_Rotate);
                    }
                }
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// 移动
    /// </summary>
    private void SnakeMove()
    {
        //上一次蛇头的位置 
        snakePosList[0] = GetComponent<Transform>();
        Debug.Log("上一次蛇头的位置:" + transform.position);

        //更改身体转向
        //移动身体
        for (int i = curSnakeLength - 1; i > 0; i--)
        {
            snakePosList[i].transform.position = snakePosList[i - 1].transform.position; 
        }

        //移动 一直向自己的y方向移动
        transform.Translate(new Vector3(0, moveDistance, 0)); 

        //更新蛇头的位置 
        snakePosList[0] = GetComponent<Transform>();
        Debug.Log("当前蛇头的位置:" + transform.position);
    }

    #endregion
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Food")
        {
            Destroy(coll.gameObject);
            //加分 
            //TODO
            //播放音效
            audioManagerScript.PlayAudio(1);
            new CreateFood().FoodByEat();
            //蛇身+1 
            CreateBody();
        }
    }

    /// <summary>
    /// 增加蛇身体
    /// </summary>
    private void CreateBody()
    {
        //1生成白色  0生成彩色
        int doubleNum = snakePosList.Count % 2;
        //实例化 
        Image bodyClone = doubleNum == 0 ?
            Instantiate(Img_SnakeBodyColor[1], bodyParent, false) :
            Instantiate(Img_SnakeBodyColor[0], bodyParent, false);
        snakePosList.Add(bodyClone.GetComponent<RectTransform>());
        //蛇长度+1
        curSnakeLength++;
    }
}
 
/// <summary>
/// 当前移动的方向
/// </summary>
public enum TurnType
{
    Up,
    Down,
    Left,
    Right
}