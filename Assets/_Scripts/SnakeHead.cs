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
    public float moveDistance = 4;                   //每次移动的距离
    public float moveHZ = 0.1f;                       //移动的频率 s/次  即速度
    public bool isAddSpeedState = false;              //是否在加速状态 
    public bool canPassWall = false;                  //是否可以穿墙
    public float PC_AddSpeedNeedTimer = 1f;           //PC端 空格加速所需的时间 
    public int Normal_Rotate = 10;                    //普通模式转弯每次的偏移量  moveHZ秒/次; 
    public List<Transform> snakePosList;              //记录蛇的行走轨迹 
    public static int curSnakeLength = 0;             //当前蛇的长度 包括蛇头 

    //实体
    public Image img_Head;
    public Sprite[] spr_HeadColor;              //0-蓝头  1-黄头
    public Image img_Body;
    public Sprite[] spr_BodyColor;              //0-白 1-蓝色 2-黄色
    public ParticleSystem eff_Die;              //死亡特效


    private float m_PressKeyTimeByPC = 0;           //PC端 按下移动按键的时长
    private float m_MoveHZTimer = 0;                //移动频率计时器
    private CurMovePos curMoveOrient;               //当前移动的方向
    private float statrSpeed;                       //速度初始值 
    private Transform bodyParent;                   //蛇身的父对象
    private bool canTurn;                           //能否转动
    private bool canMove;                           //是否允许蛇头蛇身移动
    private AudioManager audioManagerScript;
    private UIManager uiManagerScript;
    public void Start()
    {
        gameModel = GameManager.curGameModel;
        canTurn = true;
        canMove = true;
        statrSpeed = moveHZ;
        audioManagerScript = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        uiManagerScript = GameObject.Find("UIManager").GetComponent<UIManager>();
        bodyParent = GameObject.Find("Canvas_Snake").GetComponent<Transform>();
        //标记蛇头位置
        snakePosList.Add(GetComponent<RectTransform>());
        InitDataByModel();
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
        if (isAddSpeedState && statrSpeed == moveHZ)
            AddSpeed();
        else if (!isAddSpeedState)
            ReturnBeforeSpeed();
    }
    #region 功能类 
    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitDataByModel()
    {
        curSnakeLength = 1;
        isAddSpeedState = false;
        //换蛇头的皮肤 
        if (GameManager.curGameSkin == GameSkin.Bule)
        {
            GetComponent<Image>().sprite = spr_HeadColor[0];
        }
        else if (GameManager.curGameSkin == GameSkin.Yellow)
        {
            GetComponent<Image>().sprite = spr_HeadColor[1];
        }
        //初始化模式数据
        if (gameModel == GameModel.Old)
        {
            moveDistance = 0.3F;
            moveHZ = 0.2f;
            canPassWall = false;
        }
        else if (gameModel == GameModel.Normal)
        {
            moveDistance = 0.3F;
            moveHZ = 0.2f;
            canPassWall = true;
        }
        else if (gameModel == GameModel.Game)
        {
            //TODO 
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

    /// <summary>
    /// 实例化蛇身体
    /// </summary>
    private void CreateBody()
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
        bodyClone.transform.localPosition = new Vector3(1500f, 0, 0);
        //蛇长度+1
        curSnakeLength++;
    }

    private void GameOver()
    {
        Debug.Log("game over");
        uiManagerScript.ShowGameOver();
        //静止头移动
        //moveDistance = 0;
        canMove = false;
        //音效
        audioManagerScript.StopAudio(0);
        audioManagerScript.PlayAudio(2);
        //粒子特效
        ParticleSystem effClone = Instantiate(eff_Die);
        effClone.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        effClone.Play();
        Destroy(effClone, 3F);
    }

    /// <summary>
    /// 重启游戏
    /// </summary>
    public void ReStartGame()
    {

        InitDataByModel();
        //清空蛇身
        GameObject[] bodyBefore = GameObject.FindGameObjectsWithTag("Body");
        foreach (GameObject item in bodyBefore)
            Destroy(item);
        //清空位置标记
        snakePosList.Clear();
        //蛇头位置重置
        transform.localPosition = Vector3.zero;
        //标记蛇头位置
        snakePosList.Add(GetComponent<RectTransform>());
        //重置背景颜色
        GameObject.Find("UIManager").GetComponent<UIManager>().ResetBGColor();
        //允许移动
        canMove = true;
        //Time.timeScale = 1;
    }
    #endregion

    #region 加速逻辑 
    /// <summary>
    /// 加速
    /// </summary>
    private void AddSpeed()
    {
        moveHZ = statrSpeed / 3;
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

        if (Input.GetKeyDown(KeyCode.Space) && Time.timeScale == 1)
        {
            isAddSpeedState = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isAddSpeedState = false;
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
            // 设置默认旋转的方向
            //PC 
            if (buildType == BuildType.PC)
            {
                if (turnType == TurnType.Axis)
                    TurnAxis();
                else if (turnType == TurnType.free)
                    TurnFree();
            }
            //Android
            else if (buildType == BuildType.Android)
            {
                //TODO
            }
        }
    }
    /// <summary>
    ///蛇头x y轴旋转实现  ！！！有bug和游戏体验的矛盾
    ///只允许90度旋转
    /// </summary>
    private void TurnAxis()
    {
        //！！！说明：使用GetKey有蛇身蛇头交互bug  使用GetKeyDown可以解决但是游戏体验没有前者好
        //目标方向跟当前方向 不允许在同一直线上
        if (Input.GetKey(KeyCode.W) && curMoveOrient != CurMovePos.Down)
        {
            //记录当前的方向
            curMoveOrient = CurMovePos.Up;
            //旋转 
            transform.localEulerAngles = new Vector3(0, 0, 0);
            //静止旋转
            canTurn = false;
        }
        if (Input.GetKey(KeyCode.S) && curMoveOrient != CurMovePos.Up)
        {
            curMoveOrient = CurMovePos.Down;
            transform.localEulerAngles = new Vector3(0, 0, 180);
            canTurn = false;
        }
        if (Input.GetKey(KeyCode.A) && curMoveOrient != CurMovePos.Right)
        {
            curMoveOrient = CurMovePos.Left;
            transform.localEulerAngles = new Vector3(0, 0, 90);
            canTurn = false;
        }
        if (Input.GetKey(KeyCode.D) && curMoveOrient != CurMovePos.Left)
        {
            curMoveOrient = CurMovePos.Right;
            transform.localEulerAngles = new Vector3(0, 0, 270);
            canTurn = false;
        }

    }

    /// <summary>
    /// 蛇头自由旋转实现
    /// 规则：运行任意转向  每次Normal_Rotate度
    /// </summary>
    private void TurnFree()
    {
        //得到Z轴的转动角度  把负角度转成[0,360]
        while (transform.localEulerAngles.z < 0)
            transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z + 360);
        int curSnakeAngles = (int)transform.localEulerAngles.z % 360;
        //  Debug.Log("蛇头(Z轴)的转动角度：" + curSnakeAngles);
        //根据所在方位 实现旋转偏移
        if (curSnakeAngles >= 0 && curSnakeAngles <= 90)
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D))
            {
                curSnakeAngles -= Normal_Rotate;
                canTurn = false;
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S))
            {
                curSnakeAngles += Normal_Rotate;
                canTurn = false;
            }
        if (curSnakeAngles > 90 && curSnakeAngles <= 180)
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W))
            {
                curSnakeAngles -= Normal_Rotate;
                canTurn = false;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                curSnakeAngles += Normal_Rotate;
                canTurn = false;
            }
        if (curSnakeAngles > 180 && curSnakeAngles <= 270)
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S))
            {
                curSnakeAngles -= Normal_Rotate;
                canTurn = false;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W))
            {
                curSnakeAngles += Normal_Rotate;
                canTurn = false;
            }
        if (curSnakeAngles > 270 && curSnakeAngles < 360)
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S))
            {
                curSnakeAngles -= Normal_Rotate;
                canTurn = false;
            }
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W))
            {
                curSnakeAngles += Normal_Rotate;
                canTurn = false;
            }
        transform.localEulerAngles = new Vector3(0, 0, curSnakeAngles);
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
        for (int i = curSnakeLength - 1; i > 0; i--)
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

    void OnTriggerEnter2D(Collider2D coll)
    {
        //与食物交互
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
        //与身体交互
        if (coll.gameObject.tag == "Body")
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