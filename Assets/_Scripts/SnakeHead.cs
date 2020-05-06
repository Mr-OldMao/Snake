using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 蛇（蛇头、蛇身）移动、转弯、生成、死亡
/// 分数传递给start场景
/// </summary>
public class SnakeHead : MonoBehaviour
{
    public BuildType buildType = BuildType.PC;        //平台
    public GameModel gameModel;                       //游戏模式
    public TurnType turnType;                         //旋转的方式
    public float moveDistance = 4;                    //每次移动的距离
    public float moveHZ = 0.1f;                       //移动的频率 s/次  即速度 
    public bool canPassWall = false;                  //是否可以穿墙 
    public bool canHeadPassSelfBody = true;           //是否允许自己蛇头碰到自己蛇身
    public bool canBackRotation = false;              //是否允许直接向后转动
    [SerializeField]
    private bool m_IsAddSpeedState = false;           //是否在加速状态 
    public float PC_AddSpeedNeedTimer = 1f;           //PC端 空格加速所需的时间 
    public int Normal_Rotate = 10;                    //普通模式转弯每次的偏移量  moveHZ秒/次; 
    public List<Transform> snakePosList;              //记录蛇的行走轨迹 
    public static int curSnakeLength = 0;             //当前蛇的长度 包括蛇头  
    //实体
    public Image img_Head;
    public Sprite[] spr_HeadColor;                    //0-蓝头  1-黄头
    public Image img_Body;
    public Sprite[] spr_BodyColor;                    //0-白 1-蓝色 2-黄色
    public ParticleSystem eff_Die;                    //死亡特效 
    [SerializeField]
    private GameObject easyTouch;                      //EasyTouch实体
    //派生类可继承数据
    protected CurMovePos curMoveOrient;               //当前移动的方向
    protected Transform bodyParent;                   //蛇身的父对象
    protected bool canTurn;                           //能否转动 
    protected float startSpeed;                       //速度初始值 

    //私有数据
    private float m_PressKeyTimeByPC = 0;             //PC端 按下移动按键的时长
    private float m_MoveHZTimer = 0;                  //移动频率计时器
    private bool canMove;                             //是否允许蛇头蛇身移动
    private float[] m_EasyTouchOffsetValue;           //安卓Easytouch xy偏移量   0-获取摇杆偏移摇杆中心的x坐标 1-获取摇杆偏移摇杆中心的Y坐标
    private AudioManager audioManagerScript;
    private UIManager uiManagerScript;

    public void Start()
    {
        gameModel = GameManager.curGameModel;
        canTurn = true;
        canMove = true;
        audioManagerScript = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        uiManagerScript = GameObject.Find("UIManager").GetComponent<UIManager>();
        bodyParent = GameObject.Find("Canvas_Snake").GetComponent<Transform>();
        //标记蛇头位置
        snakePosList.Add(GetComponent<RectTransform>());
        InitDataByModel();
        startSpeed = moveHZ;
    }

    private void FixedUpdate()
    {
        //旋转
        Turn();
        //移动频率
        m_MoveHZTimer += Time.deltaTime;
        if (m_MoveHZTimer >= moveHZ && canMove)
        {
            SnakeMove();      //移动
            canTurn = true;   //允许下一个转动
            m_MoveHZTimer = 0;
        }

        //加速逻辑
        if (buildType == BuildType.PC)
        {
            //判定PC端加速条件
            DecidePCAddSpeed();
        }
        //加速 、恢复原速
        if (m_IsAddSpeedState && startSpeed == moveHZ)
            AddSpeed();
        else if (!m_IsAddSpeedState)
            ReturnBeforeSpeed();
    }
    #region 功能类 

    /// <summary>
    /// 初始化数据
    /// </summary>
    protected virtual void InitDataByModel()
    {
        curSnakeLength = 1;
        m_IsAddSpeedState = false;

        //换蛇头的皮肤 
        if (GameManager.curGameSkin == GameSkin.Bule)
        {
            GetComponent<Image>().sprite = spr_HeadColor[0];
        }
        else if (GameManager.curGameSkin == GameSkin.Yellow)
        {
            GetComponent<Image>().sprite = spr_HeadColor[1];
        }
        //初始化数据
        if (gameModel == GameModel.Old)
        {
            moveDistance = 0.3F;
            moveHZ = 0.2f;
            canPassWall = false;
            canHeadPassSelfBody = false;
            canBackRotation = false;
        }
        else if (gameModel == GameModel.Normal)
        {
            moveDistance = 0.3F;
            moveHZ = 0.2f;
            canPassWall = true;
            canHeadPassSelfBody = false;
            canBackRotation = false;
        }
        else if (gameModel == GameModel.Game)
        {
            moveDistance = 0.3F;
            moveHZ = 0.2f;
            canPassWall = false;
            canHeadPassSelfBody = true;
            canBackRotation = true;
            transform.localPosition = new Vector3(400, 0, 0);
        }
        ////Android版本
        if (buildType == BuildType.Android)
        {
            EasyJoystick.On_JoystickMove += GetEasyTouchData;
            m_EasyTouchOffsetValue = new float[2];
            uiManagerScript.DisplaySpeedUpBtn(true);
        }
        else if (buildType == BuildType.PC)
        {
            //隐藏easyTouch面板
            if (easyTouch) easyTouch.gameObject.SetActive(false);
            //隐藏加速按钮
            uiManagerScript.DisplaySpeedUpBtn(false);
        }
    }

    /// <summary>
    /// 切换旋转方式 
    /// </summary>
    public void ChangeTurnType()
    {
        if (turnType == TurnType.Axis)
        {
            turnType = TurnType.free;
            moveDistance = 0.3F;
            moveHZ = 0.2f;
        }
        else if (turnType == TurnType.free)
        {
            turnType = TurnType.Axis;
            moveDistance = 0.3F;
            moveHZ = 0.2f;
        }
    }

    //获取是否处于加速状态
    public bool GetAddSpeedState()
    {
        return m_IsAddSpeedState;
    }
    //设置加速状态
    public void SetAddSpeedState(bool isAddSpeed)
    {
        m_IsAddSpeedState = isAddSpeed;
    }


    /// <summary>
    /// 实例化蛇身体
    /// </summary>
    public virtual void CreateBody()
    {
        //实例化 
        Image bodyClone = Instantiate(img_Body, bodyParent, false);
        //换皮肤
        //1生成白色  0生成彩色
        int doubleNum = snakePosList.Count % 2;
        if (GameManager.curGameSkin == GameSkin.Bule)
        {
            bodyClone.sprite = doubleNum == 0 ? spr_BodyColor[1] : spr_BodyColor[0];
        }
        else if (GameManager.curGameSkin == GameSkin.Yellow)
        {
            bodyClone.sprite = doubleNum == 0 ? spr_BodyColor[2] : spr_BodyColor[0];
        }
        snakePosList.Add(bodyClone.GetComponent<RectTransform>());
        //先随机生成在场景外
        bodyClone.transform.localPosition = new Vector3(6000f, 0, 0);
        //蛇长度+1
        curSnakeLength++;
    }

    private void GameOver()
    {
        Debug.Log("game over");
        uiManagerScript.ShowGameOver();
        canMove = false;
        if (gameModel == GameModel.Game)
            GameObject.FindGameObjectWithTag("EnemyHead").GetComponent<EnemyAI>().canMove = false;
        //音效
        audioManagerScript.StopAudio(0);
        audioManagerScript.PlayAudio(2);
        //粒子特效
        ParticleSystem effClone = Instantiate(eff_Die);
        effClone.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        effClone.Play();
        Destroy(effClone, 3F);
    }

    private void Win()
    {
        Debug.Log("player win");
        uiManagerScript.DisplayWinImg();
        canMove = false;
        GameObject enemyHead = GameObject.FindGameObjectWithTag("EnemyHead");
        enemyHead.GetComponent<EnemyAI>().canMove = false;
        //音效
        audioManagerScript.StopAudio(0);
        audioManagerScript.PlayAudio(2);
        //粒子特效
        ParticleSystem effClone = Instantiate(eff_Die);
        effClone.transform.position = new Vector3(enemyHead.transform.position.x, enemyHead.transform.position.y, 0);
        effClone.Play();
        Destroy(effClone, 3F);
    }



    /// <summary>
    /// 重启游戏  清空数据、清理场景
    /// </summary>
    public virtual void ReStartGame()
    {
        InitDataByModel();
        //清空蛇身
        GameObject[] playerBodyBefore = GameObject.FindGameObjectsWithTag("PlayerBody");
        foreach (GameObject item in playerBodyBefore)
            Destroy(item);
        GameObject[] enemyBodyBefore = GameObject.FindGameObjectsWithTag("EnemyBody");
        foreach (GameObject item in enemyBodyBefore)
            Destroy(item);
        //清空位置标记
        snakePosList.Clear();
        //蛇头位置重置
        if (gameModel == GameModel.Game) transform.localPosition = new Vector3(400, 0, 0);
        else transform.localPosition = Vector3.zero;
        //标记蛇头位置
        snakePosList.Add(GetComponent<RectTransform>());
        //重置背景颜色
        GameObject.Find("UIManager").GetComponent<UIManager>().ResetBGColor();
        //允许移动
        canMove = true;


        //Time.timeScale = 1;
    }


    /// <summary>
    /// 获取easy touch的 x、y偏移量 
    /// </summary>
    private void GetEasyTouchData(MovingJoystick move)
    {
        if (move.joystickName != "EasyTouchJoystick") return;
        m_EasyTouchOffsetValue[0] = move.joystickAxis.x;       //   获取摇杆偏移摇杆中心的x坐标
        m_EasyTouchOffsetValue[1] = move.joystickAxis.y;      //    获取摇杆偏移摇杆中心的y坐标  
    }

    #endregion

    #region 加速逻辑 
    /// <summary>
    /// 加速
    /// </summary>
    private void AddSpeed()
    {
        moveHZ = startSpeed / 3;
    }
    /// <summary>
    /// 改为原来速度
    /// </summary>
    private void ReturnBeforeSpeed()
    {
        moveHZ = startSpeed;
    }
    /// <summary>
    /// 判定PC端加速条件
    /// </summary>
    private void DecidePCAddSpeed()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            m_PressKeyTimeByPC += Time.deltaTime;
            if (m_PressKeyTimeByPC >= PC_AddSpeedNeedTimer)
            {
                m_IsAddSpeedState = true;
            }
        }
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) ||
             Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            m_PressKeyTimeByPC = 0;
            m_IsAddSpeedState = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.timeScale == 1)
        {
            m_IsAddSpeedState = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            m_IsAddSpeedState = false;
        }
    }

    #endregion

    #region 旋转逻辑

    /// <summary>
    /// 蛇头旋转
    /// 根据两个平台   三种游戏模式
    /// PC: 经典版本 规则 只水平和竖直方向旋转
    ///     普通版本 规则 可向任意方向旋转
    /// Android： todo
    /// </summary>
    public void Turn()
    {
        if (canTurn)
        {
            if (turnType == TurnType.Axis)
                TurnAxis();
            else if (turnType == TurnType.free)
                TurnFree();
        }
    }

    /// <summary>
    ///蛇头x y轴旋转实现  ！！！有bug和游戏体验的矛盾
    ///只允许90度旋转
    /// </summary>
    protected virtual void TurnAxis()
    {
        //PC移动
        if (buildType == BuildType.PC)
        {
            //！！！说明：使用GetKey有蛇身蛇头交互bug  使用GetKeyDown可以解决但是游戏体验没有前者好
            // (w||↑)&&  目标方向跟当前方向 不允许在同一直线上
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && curMoveOrient != CurMovePos.Down)
            {
                //记录当前的方向
                curMoveOrient = CurMovePos.Up;
                //旋转 
                transform.localEulerAngles = new Vector3(0, 0, 0);
                //静止旋转
                canTurn = false;
            }
            if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && curMoveOrient != CurMovePos.Up)
            {
                curMoveOrient = CurMovePos.Down;
                transform.localEulerAngles = new Vector3(0, 0, 180);
                canTurn = false;
            }
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && curMoveOrient != CurMovePos.Right)
            {
                curMoveOrient = CurMovePos.Left;
                transform.localEulerAngles = new Vector3(0, 0, 90);
                canTurn = false;
            }
            if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && curMoveOrient != CurMovePos.Left)
            {
                curMoveOrient = CurMovePos.Right;
                transform.localEulerAngles = new Vector3(0, 0, 270);
                canTurn = false;
            }
        }
        //Android  EasyTouch旋转移动逻辑
        else if (buildType == BuildType.Android)
        {
            float x = m_EasyTouchOffsetValue[0];
            float y = m_EasyTouchOffsetValue[1];
            //Debug.Log("EasyTouch x:" + x + ",y:" + y);
            //具体原理可见相应原理图
            //上
            if ((x > 0 && y > x) || (x < 0 && y > -x))
            {
                //不允许向反向移动  || 可以向反向移动
                if (curMoveOrient != CurMovePos.Down || canBackRotation)
                {
                    //Debug.Log("EasyTouch: Up");
                    //记录当前的方向
                    curMoveOrient = CurMovePos.Up;
                    //旋转 
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                    //静止旋转
                    canTurn = false;
                }
            }
            //下
            else if ((x > 0 && -y > x) || (x < 0 && -y > -x))
            {
                if (curMoveOrient != CurMovePos.Up || canBackRotation)
                {
                    //Debug.Log("EasyTouch: Down");
                    //记录当前的方向
                    curMoveOrient = CurMovePos.Down;
                    //旋转 
                    transform.localEulerAngles = new Vector3(0, 0, 180);
                    //静止旋转
                    canTurn = false;
                }
            }
            //左
            else if ((y > 0 && y < -x) || (y < 0 && -y < -x))
            {
                if (curMoveOrient != CurMovePos.Right || canBackRotation)
                {
                    //Debug.Log("EasyTouch: Left");
                    //记录当前的方向
                    curMoveOrient = CurMovePos.Left;
                    //旋转 
                    transform.localEulerAngles = new Vector3(0, 0, 90);
                    //静止旋转
                    canTurn = false;
                }
            }
            //右
            else if ((y > 0 && y < x) || (y < 0 && -y < x))
            {
                if (curMoveOrient != CurMovePos.Left || canBackRotation)
                {
                    // Debug.Log("EasyTouch: Right");
                    //记录当前的方向
                    curMoveOrient = CurMovePos.Right;
                    //旋转 
                    transform.localEulerAngles = new Vector3(0, 0, 270);
                    //静止旋转
                    canTurn = false;
                }
            }
        }
    }

    /// <summary>
    /// 蛇头自由旋转实现
    /// 规则：运行任意转向  每次Normal_Rotate度
    /// </summary>
    private void TurnFree()
    {
        if (buildType == BuildType.PC)
        {
            //得到Z轴的转动角度  把负角度转成[0,360]
            while (transform.localEulerAngles.z < 0)
                transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z + 360);
            int curSnakeAngles = (int)transform.localEulerAngles.z % 360;
            //Debug.Log("蛇头(Z轴)的转动角度：" + curSnakeAngles);
            //根据所在方位 实现旋转偏移
            if (curSnakeAngles >= 0 && curSnakeAngles <= 90)
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.RightArrow))
                {
                    curSnakeAngles -= Normal_Rotate;
                    canTurn = false;
                }
                else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow))
                {
                    curSnakeAngles += Normal_Rotate;
                    canTurn = false;
                }
            if (curSnakeAngles > 90 && curSnakeAngles <= 180)
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.UpArrow))
                {
                    curSnakeAngles -= Normal_Rotate;
                    canTurn = false;
                }
                else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow))
                {
                    curSnakeAngles += Normal_Rotate;
                    canTurn = false;
                }
            if (curSnakeAngles > 180 && curSnakeAngles <= 270)
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow))
                {
                    curSnakeAngles -= Normal_Rotate;
                    canTurn = false;
                }
                else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow))
                {
                    curSnakeAngles += Normal_Rotate;
                    canTurn = false;
                }
            if (curSnakeAngles > 270 && curSnakeAngles < 360)
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.DownArrow))
                {
                    curSnakeAngles -= Normal_Rotate;
                    canTurn = false;
                }
                else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.UpArrow))
                {
                    curSnakeAngles += Normal_Rotate;
                    canTurn = false;
                }
            transform.localEulerAngles = new Vector3(0, 0, curSnakeAngles);
        }
        //Android
        if (buildType == BuildType.Android)
        {
            //用于摇杆转角度
            float angle = Mathf.Atan2(m_EasyTouchOffsetValue[0], m_EasyTouchOffsetValue[1]) * Mathf.Rad2Deg;
            //Debug.Log(angle);
            transform.localEulerAngles = new Vector3(transform.localRotation.x, transform.localRotation.y, -angle);
            //  设置控制角色或物体方块的朝向（当前坐标+摇杆偏移量） 
        }
    }

    #endregion

    #region 移动逻辑
    /// <summary>
    /// 移动 
    /// 蛇头移动 -》蛇身跟随蛇头移动
    /// </summary>
    private void SnakeMove()
    {
        //上一次蛇头的位置 
        snakePosList[0] = GetComponent<Transform>();
        //Debug.Log("上一次蛇头的位置:" + transform.position);

        //更改身体转向
        //移动身体 
        for (int i = snakePosList.Count - 1; i > 0; i--)
        {
            snakePosList[i].transform.position = snakePosList[i - 1].transform.position;
        }

        //移动  
        //移动方式  一直向自己的y方向移动
        transform.Translate(new Vector3(0, moveDistance, 0));

        //更新蛇头的位置 
        snakePosList[0] = GetComponent<Transform>();
        //Debug.Log("当前蛇头的位置:" + transform.position);
    }
    #endregion

    #region 交互逻辑

    protected virtual void OnTriggerEnter2D(Collider2D coll)
    {
        //与食物交互
        if (coll.gameObject.tag == "Food")
        {
            Destroy(coll.gameObject);
            //播放音效
            audioManagerScript.PlayAudio(1); 
            //“食物”数量-1
            CreateFood.curFoodCount--;
            //蛇身+1 
            CreateBody();
        }
        //与墙壁交互
        if (coll.gameObject.tag == "Wall")
        {
            if (canPassWall)
            {
                //坐标反转
                if (coll.gameObject.name == "up")
                    transform.localPosition = new Vector3(transform.localPosition.x, -516f, transform.localPosition.z);
                else if (coll.gameObject.name == "down")
                    transform.localPosition = new Vector3(transform.localPosition.x, 516f, transform.localPosition.z);
                else if (coll.gameObject.name == "left")
                    transform.localPosition = new Vector3(940, transform.localPosition.y, transform.localPosition.z);
                else if (coll.gameObject.name == "right")
                    transform.localPosition = new Vector3(-940f, transform.localPosition.y, transform.localPosition.z);
            }
            else
            {
                GameOver();
            }
        }
        //玩家蛇头与玩家蛇身交互
        if (gameObject.tag == "PlayerHead" && coll.gameObject.tag == "PlayerBody")
        {
            if (canHeadPassSelfBody == false)
                GameOver();
        }
        //敌人蛇头与玩家蛇身交互
        if (gameObject.tag == "EnemyHead" && coll.gameObject.tag == "PlayerBody")
        {
            Debug.Log("~~~~~~~~~~~~~~~~~~~~~玩家胜~~~~~~~~~~~~~~~~~~");
            Win();
        }
        //玩家蛇头 碰撞 敌人蛇身或者蛇头
        else if (gameObject.tag == "PlayerHead" && (coll.gameObject.tag == "EnemyBody" || coll.gameObject.tag == "EnemyHead"))
        {
            GameOver();
        } 
    }

    #endregion 
}
#region Enum类型


/// <summary>
/// 当前移动的方向
/// </summary>
public enum CurMovePos
{
    Up,
    Down,
    Left,
    Right
}
/// <summary>
/// 当前旋转的方式
/// </summary>
public enum TurnType
{
    /// <summary>
    /// xy轴向移动
    /// </summary>
    Axis,
    /// <summary>
    /// 自由移动
    /// </summary>
    free
}
#endregion