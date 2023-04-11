using System.Numerics;
using System.Timers;
using LiteDB;
using Tool;

public class TimerElapser
{

    public static void startSimultaneouslyTimer()
    {
        Global.simultaneouslyTimer.Interval = int.Parse(Db.Instance.GetSth<int>(Db.SIMULTANEOUSLY_LOOP_EVENT_TIME_CYCLE, 1)) * 1000;
        Global.simultaneouslyTimer.Start();
    }
    public static void stopSimultaneouslyTimer()
    {
        Global.simultaneouslyTimer.Stop();
    }
    public static void simultaneouslyTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var singles = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
        .Find(Db.Instance.eventModeQuery(EventMode.Simultaneously)).OrderByDescending((e) => e.Priority1);

        foreach (var single in singles)
        {
            Rectangle? resultRect = GetSingleRectangle(single);
            if (resultRect != null)
            {
                Tool.Clicker.Handle((EventKey)single.EventKey!, resultRect!.Value);
                break;
            }
        }
    }


    public static async Task startInOrderTimer(Single? startSingle)
    {
        if (startSingle == null)
        {
            // Priority1是最大，但Priority2不一定是最大
            Single? first = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
            .Find(Db.Instance.eventModeQuery(EventMode.InOrder)).OrderByDescending((e) => e.Priority1).FirstOrDefault();
            if (first == null)
            {
                MessageBox.Show("没有添加循环");
                return;
            }

            // 查询截图序号对应的 Single
            var priority1Query = Query.And(Db.Instance.eventModeQuery(EventMode.InOrder), Db.Instance.Priority1Query((int)first!.Priority1!));
            Single okFirst = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
            .Find(priority1Query).OrderByDescending(e => e.Priority2).FirstOrDefault()!;

            // 循环当前截图序号。
            while (true)
            {
                await Task.Delay((int)okFirst.Priority2CheckSecond! * 1000);

                Rectangle? resultRect = GetSingleRectangle(okFirst);

                // 若检测到，则进行操作
                if (resultRect != null)
                {
                    // 触发事件
                    Tool.Clicker.Handle((EventKey)okFirst.EventKey!, resultRect!.Value);

                    // 查询下一个截图序号对应的 Single
                    var next2Query = Query.And(Db.Instance.eventModeQuery(EventMode.InOrder), Db.Instance.Priority1Query((int)okFirst.Priority1!));
                    var finalNext2Query = Query.And(next2Query, Db.Instance.Priority2Query((int)(okFirst.Priority2! + 1)));
                    Single? next2Single = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
                     .FindOne(finalNext2Query);

                    // 若查询到了则递归
                    if (next2Single != null)
                    {
                        await startInOrderTimer(next2Single!);
                    }
                    // 若未查询到，则查询当前循环序号是否需要进行循环
                    else
                    {
                        // 当前循环序号需要循环多少次
                        for (int i = 0; i < (int)okFirst.Priority1LoopTimes!; i++)
                        {

                        }

                        var next1Query = Query.And(Db.Instance.eventModeQuery(EventMode.InOrder), Db.Instance.Priority1Query((int)okFirst.Priority1! + 1));
                        Single? next1Single = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
                         .Find(next1Query).OrderByDescending(e => e.Priority2).FirstOrDefault();
                        // 单个循环序号继续循环
                        if (next1Single != null)
                        {
                            // Global.inOrderTimerSingle = next1Single!;
                            // startInOrderTimer();
                        }
                        // 单个循环序号不继续循环
                        else
                        {

                        }
                    }
                }
            }

        }

    }

    public static async Task InOrderPriority1Loop(int priority1, int startPriority2)
    {
        BsonExpression q1 = Db.Instance.eventModeQuery(EventMode.InOrder);
        BsonExpression q2 = Db.Instance.Priority1Query(priority1);
        Single single = Db.Instance.GetCollection<Single>(Single.TABLE_NAME).FindOne(Query.And(q1, q2))!;

    }


    // 循环当前循环序号，直到达到要求循环次数
    public static async Task InOrderPriority2Loop(Single single)
    {
        for (int i = 0; i < single.Priority1LoopTimes!; i++)
        {
            Priority1LoopStatus priority1LoopStatus = await InOrderPriority2ARound((int)single.Priority1!, (int)single.Priority2!);
            if (priority1LoopStatus == Priority1LoopStatus.complete)
            {
                // await Task.Delay((int)single.Priority2ToNextAfterSecond! * 1000);
                // 继续当前循环的下一轮。
            }
            else if (priority1LoopStatus == Priority1LoopStatus.timeout)
            {
                // 当前循环序号循环结束。此处啥也不干，因为 InOrderPriority2ARound 已经处理，显示超时弹窗。
                return;
            }
            else if (priority1LoopStatus == Priority1LoopStatus.jump)
            {
                // 此当前循环序号循环结束。处啥也不干，因为 InOrderPriority2ARound 已经处理，跳转到新地方了。
                return;
            }
            else
            {
                throw new Exception($"未处理 {priority1LoopStatus}");
            }
        }
    }

    // 执行一轮当前循环序号
    public static async Task<Priority1LoopStatus> InOrderPriority2ARound(int priority1, int startPriority2)
    {
        BsonExpression q1 = Db.Instance.eventModeQuery(EventMode.InOrder);
        BsonExpression q2 = Db.Instance.Priority1Query(priority1);
        BsonExpression q3 = Query.LTE("Priority2", new BsonValue(startPriority2));
        List<Single> singles = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
        .Find(Query.And(Query.And(q1, q2), q3)).OrderByDescending(e => e.Priority2).ToList();
        for (int i = 0; i < singles.Count; i++)
        {
            Single single = singles[i];
            int timeoutSecond = 0;
            while (true)
            {
                await Task.Delay((int)single.Priority2CheckSecond! * 1000);
                timeoutSecond += (int)single.Priority2CheckSecond!;
                if (timeoutSecond > single.Priority2TimeoutSecond!)
                {
                    if (single.Priority2ToWhere == null)
                    {
                        MessageBox.Show($"循环序号：{single.Priority1}，截图序号：{single.Priority2}，检测时间累计超过 {single.Priority2TimeoutSecond} 秒");
                        return Priority1LoopStatus.timeout;
                    }
                    else
                    {
                        //todo: 重新开始。
                        return Priority1LoopStatus.jump;
                    }
                }

                Rectangle? resultRect = GetSingleRectangle(single);
                if (resultRect != null)
                {
                    Tool.Clicker.Handle((EventKey)single.EventKey!, resultRect!.Value);
                    break;
                }
            }
        }

        return Priority1LoopStatus.complete;
    }


    // 返回触发的点击位置
    public static Rectangle? GetSingleRectangle(Single single)
    {
        string fullScreanImagePath = K.ASSETS_SINGLE + "/full_screan.png";
        Tool.Capturer.CaptureFullScreen(fullScreanImagePath, null);
        Rectangle fullScreanSize = Screen.GetBounds(Point.Empty);
        if (single.ImagePath != null)
        {
            List<Rectangle> rList = Tool.ImageMatcher.MatchImage(single.ImagePath, fullScreanImagePath, single.SimilarityThreshold ?? 0);
            if (rList.Count != 0)
            {
                switch (single.PositionBlockType)
                {
                    case PositionBlockType.left:
                        return rList.OrderBy((e) => e.Left).First();
                    case PositionBlockType.right:
                        return rList.OrderByDescending((e) => e.Right).First();
                    case PositionBlockType.top:
                        return rList.OrderBy((e) => e.Top).First();
                    case PositionBlockType.down:
                        return rList.OrderByDescending((e) => e.Bottom).First();
                    case PositionBlockType.left_top:
                        return rList.OrderBy((e) => new Vector2(e.Left, e.Top).Length()).First();
                    case PositionBlockType.left_down:
                        return rList.OrderBy((e) => new Vector2(e.Left, fullScreanSize.Height - e.Bottom).Length()).First();
                    case PositionBlockType.right_top:
                        return rList.OrderBy((e) => new Vector2(fullScreanSize.Width - e.Right, e.Top).Length()).First();
                    case PositionBlockType.right_down:
                        return rList.OrderBy((e) => new Vector2(fullScreanSize.Width - e.Right, fullScreanSize.Height - e.Bottom).Length()).First();
                    default:
                        throw new Exception($"未知：{single.PositionBlockType}");
                }
            }
        }
        return null;
    }
}