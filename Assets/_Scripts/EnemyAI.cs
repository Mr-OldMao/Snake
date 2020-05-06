using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人AI
/// 2020年4月19日 18:40:42
/// 继承 玩家蛇头作为父类，
/// 自由移动、旋转、避障、吃食物
/// </summary>
public class EnemyAI : SnakeHead
{
    //离安全距离
    public int safeDistance = 200;

    //public  float moveHzInit = 0.5f; //敌人移动移动频率moveHZ秒/次 初始值
    private SnakeHead playerHead;

    void Start()
    {
        base.Start();
        playerHead = GameObject.Find("Img_SnakeHead").GetComponent<SnakeHead>();
        gameModel = GameModel.Game;
        CreateBody();
        CreateBody();
    }


    protected override void InitDataByModel()
    {
        base.InitDataByModel();
        //控制敌人移动移动频率  moveHZ秒/次 
        moveHZ = (0.5F - int.Parse(UIManager.GetInstance.txt_Kiss.text) * 0.05f >= 0.1F) ?
        0.5F - int.Parse(UIManager.GetInstance.txt_Kiss.text) * 0.05f : 0.1f;
        //重置初始速度
        startSpeed = moveHZ;
        transform.localPosition = new Vector3(-400, 0, 0);
        bodyParent = GameObject.Find("Canvas_Enemy").GetComponent<Transform>();
    }

    /// <summary>
    /// 重写父类 创建蛇身方法 
    /// </summary>
    public override void CreateBody()
    {
        //实例化 
        Image bodyClone = Instantiate(img_Body, bodyParent, false);
        //换皮肤
        //1生成白色  0生成彩色
        int doubleNum = snakePosList.Count % 2;
        //if (GameManager.curGameSkin == GameSkin.Bule) 
        bodyClone.sprite = doubleNum == 0 ? spr_BodyColor[1] : spr_BodyColor[0];
        snakePosList.Add(bodyClone.GetComponent<RectTransform>());
        //先随机生成在场景外
        bodyClone.transform.localPosition = new Vector3(5000f, 0, 0);
        ////蛇长度+1
        //curSnakeLength++;
    }

    public override void ReStartGame()
    {
        base.ReStartGame();
        InitDataByModel();
    }

    /// <summary>
    /// AI旋转移动：沿轴
    /// 1.规则：不允许往当前方向的后方旋转移动
    /// 2.躲着玩家移动 =>与玩家保持安全距离 => 想要旋转的轴向距离玩家(snakePosList字典坐标)距离 >= 安全距离    
    /// 3.向着食物旋转移动 || 随机自由旋转移动  
    /// </summary>
    protected override void TurnAxis()
    {
        //想要旋转方位 0-up 1-down 2-left 3-right
        CurMovePos nextMovePos = CurMovePos.Down;
        //可以旋转的方位  CurMovePos枚举值
        int[] movePosIndex = new int[4] { -1, -1, -1, -1 };
        //建议旋转的方位 =》向着食物方位
        int[] wantMovePosIndex = new int[2];
        // 找出“可以”、“建议” 旋转的方位
        //当前所处的方向
        if (curMoveOrient == CurMovePos.Up)
        {
            //1.不允许直接往当前方位后方旋转  2.处于安全距离
            for (int i = 0; i < 4; i++)
            {
                if (i != 1 && JudgeSafeDis((CurMovePos)i))
                {
                    movePosIndex[i] = i;
                }
            }
            //3.向食物方向移动
            wantMovePosIndex = WantMoveDis();
        }
        else if (curMoveOrient == CurMovePos.Down)
        {
            //1.不允许直接往当前方位后方旋转  2.处于安全距离
            for (int i = 0; i < 4; i++)
            {
                if (i != 0 && JudgeSafeDis((CurMovePos)i))
                {
                    movePosIndex[i] = i;
                }
            }
            //3.向食物方向移动
            wantMovePosIndex = WantMoveDis();
        }
        else if (curMoveOrient == CurMovePos.Left)
        {
            //1.不允许直接往当前方位后方旋转  2.处于安全距离
            for (int i = 0; i < 4; i++)
            {
                if (i != 3 && JudgeSafeDis((CurMovePos)i))
                {
                    movePosIndex[i] = i;
                }
            }
            //3.向食物方向移动
            wantMovePosIndex = WantMoveDis();
        }
        else if (curMoveOrient == CurMovePos.Right)
        {
            //1.不允许直接往当前方位后方旋转  2.处于安全距离
            for (int i = 0; i < 4; i++)
            {
                if (i != 2 && JudgeSafeDis((CurMovePos)i))
                {
                    movePosIndex[i] = i;
                }
            }
            //3.向食物方向移动
            wantMovePosIndex = WantMoveDis();
        }

        /*选择所要旋转的方位*/
        //Debug.Log("start~~~~~~~~~~~~");
        //两个“建议”的方向在“可以”旋转方向中的个数
        int isWantCount = 0;
        for (int i = 0; i < 4; i++)
        {
            //无效数值
            if (movePosIndex[i] == -1)
                continue;
            //Debug.Log("可以旋转的方向" + movePosIndex[i] + "，希望旋转的方向： " + wantMovePosIndex[0] + "," + wantMovePosIndex[1]);

            //查看“可以”旋转的方向中是否有“建议”旋转的方向
            if (movePosIndex[i] == wantMovePosIndex[0])
            {
                isWantCount++;
                nextMovePos = (CurMovePos)movePosIndex[i];
                //Debug.Log("选择了：" + movePosIndex[i]);
                //break;
            }
            else if (movePosIndex[i] == wantMovePosIndex[1])
            {
                isWantCount++;
                nextMovePos = (CurMovePos)movePosIndex[i];
                //Debug.Log("选择了：" + movePosIndex[i]);
                //break;
            }
        }
        //选择最优转向
        if (isWantCount == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                if (movePosIndex[i] != -1)
                    nextMovePos = (CurMovePos)movePosIndex[i];
            }
        }
        else if (isWantCount == 1)
        {
        }
        else if (isWantCount == 2)
        {
            //两个“建议”的方向均在“可以”旋转方向中 随机选取一个
            nextMovePos = Random.Range(0, 2) == 0 ? (CurMovePos)wantMovePosIndex[0] : (CurMovePos)wantMovePosIndex[1];
            //Debug.Log("两个“建议”的方向均满足 选择了：" + nextMovePos.ToString());
        }
        //Debug.Log("end~~~~~~~~~~~~");
        //开始执行旋转
        switch (nextMovePos)
        {
            case CurMovePos.Up:
                //记录当前的方向
                curMoveOrient = CurMovePos.Up;
                //旋转 
                transform.localEulerAngles = new Vector3(0, 0, 0);
                //静止旋转
                canTurn = false;
                break;
            case CurMovePos.Down:
                curMoveOrient = CurMovePos.Down;
                transform.localEulerAngles = new Vector3(0, 0, 180);
                canTurn = false;
                break;
            case CurMovePos.Left:
                curMoveOrient = CurMovePos.Left;
                transform.localEulerAngles = new Vector3(0, 0, 90);
                canTurn = false;
                break;
            case CurMovePos.Right:
                curMoveOrient = CurMovePos.Right;
                transform.localEulerAngles = new Vector3(0, 0, 270);
                canTurn = false;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 判定：是否与“ 玩家” 和 “边界” 处于安全距离
    /// 检测想要旋转移动的方向是否处于安全距离
    /// 原理参考 原理图：判断安全距离
    /// </summary>
    /// <param name="wantTurnPos">下一步想要旋转的方向</param>
    private bool JudgeSafeDis(CurMovePos wantTurnPos)
    {
        //边界值：
        //int border_up = 500;
        //int border_down = -500;
        //int border_left = -910;
        //int border_right = 910; 
        int border_up = 880;
        int border_down = -880;
        int border_left = -1780;
        int border_right = 1780;
        bool isSafe = false;  //是否安全
        System.Console.WriteLine(snakePosList[0].transform.localPosition);
        switch (wantTurnPos)
        {
            case CurMovePos.Up:
                //判断与“边界”的安全距离
                if (snakePosList[0].transform.localPosition.y >= border_up)
                {
                    isSafe = false;
                    break;
                }
                else
                    isSafe = true;
                //判断与“玩家”的安全距离
                for (int i = 0; i < playerHead.snakePosList.Count; i++)
                {
                    //判断是否有和敌人蛇头 在同一轴向的玩家坐标   （差值小于30等价于在一同轴）    
                    if (Mathf.Abs(playerHead.snakePosList[i].transform.localPosition.x - snakePosList[0].transform.localPosition.x) <= 30)
                    {
                        //判断非轴向上的距离 与 安全距离的关系
                        if (Mathf.Abs(snakePosList[0].transform.localPosition.y -
                            playerHead.snakePosList[i].transform.localPosition.y) >= safeDistance)
                        {
                            Debug.Log("敌人安全");
                            isSafe = true;
                        }
                        else
                        {
                            Debug.Log("敌人危险");
                            isSafe = false;
                            break;
                        }
                        Debug.Log("敌方蛇头所在的位置:" + snakePosList[i].transform.localPosition + ",玩家同轴所在的位置:" + playerHead.snakePosList[i].transform.localPosition);
                    }
                    else
                        isSafe = true;
                }
                break;
            case CurMovePos.Down:
                //判断与“边界”的安全距离
                if (snakePosList[0].transform.localPosition.y <= border_down)
                {
                    isSafe = false;
                    break;
                }
                else
                    isSafe = true;
                //判断与“玩家”的安全距离
                for (int i = 0; i < playerHead.snakePosList.Count; i++)
                {
                    //判断是否有和敌人蛇头 在同一轴向的玩家坐标   （差值小于30等价于在一同轴）    
                    if (Mathf.Abs(playerHead.snakePosList[i].transform.localPosition.x - snakePosList[0].transform.localPosition.x) <= 30)
                    {
                        //判断非轴向上的距离 与 安全距离的关系
                        if (Mathf.Abs(snakePosList[0].transform.localPosition.y -
                            playerHead.snakePosList[i].transform.localPosition.y) >= safeDistance)
                        {
                            //Debug.Log("敌人安全");
                            isSafe = true;
                        }
                        else
                        {
                            //Debug.Log("敌人危险");
                            isSafe = false;
                            break;
                        }
                        //Debug.Log("敌方蛇头所在的位置:" + snakePosList[i].transform.localPosition + ",玩家同轴所在的位置:" + playerHead.snakePosList[i].transform.localPosition);
                    }
                    else
                        isSafe = true;
                }
                break;
            case CurMovePos.Left:
                //判断与“边界”的安全距离
                if (snakePosList[0].transform.localPosition.x <= border_left)
                {
                    isSafe = false;
                    break;
                }
                else
                    isSafe = true;
                //判断与“玩家”的安全距离
                for (int i = 0; i < playerHead.snakePosList.Count; i++)
                {
                    //判断是否有和敌人蛇头 在同一轴向的玩家坐标   （差值小于30等价于在一同轴）    
                    if (Mathf.Abs(playerHead.snakePosList[i].transform.localPosition.y - snakePosList[0].transform.localPosition.y) <= 30)
                    {
                        //判断非轴向上的距离 与 安全距离的关系
                        if (Mathf.Abs(snakePosList[0].transform.localPosition.x -
                            playerHead.snakePosList[i].transform.localPosition.x) >= safeDistance)
                        {
                            Debug.Log("敌人安全");
                            isSafe = true;
                        }
                        else
                        {
                            Debug.Log("敌人危险");
                            isSafe = false;
                            break;
                        }
                        Debug.Log("敌方蛇头所在的位置:" + snakePosList[i].transform.localPosition + ",玩家同轴所在的位置:" + playerHead.snakePosList[i].transform.localPosition);
                    }
                    else
                        isSafe = true;
                }
                break;
            case CurMovePos.Right:
                //判断与“边界”的安全距离
                if (snakePosList[0].transform.localPosition.x >= border_right)
                {
                    isSafe = false;
                    break;
                }
                else
                    isSafe = true;
                //判断与“玩家”的安全距离
                for (int i = 0; i < playerHead.snakePosList.Count; i++)
                {
                    //判断是否有和敌人蛇头 在同一轴向的玩家坐标   （差值小于30等价于在一同轴）    
                    if (Mathf.Abs(playerHead.snakePosList[i].transform.localPosition.y - snakePosList[0].transform.localPosition.y) <= 30)
                    {
                        //判断非轴向上的距离 与 安全距离的关系
                        if (Mathf.Abs(snakePosList[0].transform.localPosition.x -
                            playerHead.snakePosList[i].transform.localPosition.x) >= safeDistance)
                        {
                            Debug.Log("敌人安全");
                            isSafe = true;
                        }
                        else
                        {
                            Debug.Log("敌人危险");
                            isSafe = false;
                            break;
                        }
                        Debug.Log("敌方蛇头所在的位置:" + snakePosList[i].transform.localPosition + ",玩家同轴所在的位置:" + playerHead.snakePosList[i].transform.localPosition);
                    }
                    else
                        isSafe = true;
                }
                break;
            default:
                break;
        }
        return isSafe;
    }

    /// <summary>
    /// 期望移动的方向 =》向着食物的方向
    /// 返回0-up 1-down 2-left 3-right
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private int[] WantMoveDis()
    {
        int[] wantMovePosIndex = new int[2];
        //食物的位置    
        Vector3 foodpos = Vector3.zero;
        try
        {
            //取离敌人最近的一个食物坐标 
            GameObject[] foodArr = GameObject.FindGameObjectsWithTag("Food");
            if (foodArr.Length ==1) foodpos= foodArr[0].transform.localPosition;
            else if (foodArr.Length == 2)  
            {
                foodpos = (Mathf.Abs(transform.localPosition.x - foodArr[0].transform.localPosition.x) + Mathf.Abs(transform.localPosition.y - foodArr[0].transform.localPosition.y))
                    <= (Mathf.Abs(transform.localPosition.x - foodArr[1].transform.localPosition.x) + Mathf.Abs(transform.localPosition.y - foodArr[1].transform.localPosition.y)) ?
                    foodArr[0].transform.localPosition : foodArr[1].transform.localPosition;
            }
        }
        catch (System.Exception)
        {

        }
        //敌人-食物的 差值
        float dif_x = transform.localPosition.x - foodpos.x;
        float dif_y = transform.localPosition.y - foodpos.y;
        //敌人和食物处于（近似处于）一条线上  
        if (Mathf.Abs(dif_x) <= 10 && dif_y >= 0)
        {
            wantMovePosIndex[0] = 2;
            wantMovePosIndex[1] = 2;
        }
        else if (Mathf.Abs(dif_x) <= 10 && dif_y < 0)
        {
            wantMovePosIndex[0] = 3;
            wantMovePosIndex[1] = 3;
        }
        else if (dif_x >= 0 && Mathf.Abs(dif_y) < 10)
        {
            wantMovePosIndex[0] = 1;
            wantMovePosIndex[1] = 1;
        }
        else if (dif_x <= 0 && Mathf.Abs(dif_y) < 10)
        {
            wantMovePosIndex[0] = 0;
            wantMovePosIndex[1] = 0;
        }
        //四个象限
        else if (dif_x > 0 && dif_y > 0)
        {
            wantMovePosIndex[0] = 1;
            wantMovePosIndex[1] = 2;
        }
        else if (dif_x < 0 && dif_y > 0)
        {
            wantMovePosIndex[0] = 1;
            wantMovePosIndex[1] = 3;
        }
        else if (dif_x < 0 && dif_y < 0)
        {
            wantMovePosIndex[0] = 0;
            wantMovePosIndex[1] = 3;
        }
        else if (dif_x > 0 && dif_y < 0)
        {
            wantMovePosIndex[0] = 0;
            wantMovePosIndex[1] = 2;
        }
        return wantMovePosIndex;
    }
}
