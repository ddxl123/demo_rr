using System.Numerics;
using System.Timers;
using LiteDB;
using Tool;
public class TimerElapser
{

    // inOder 随时暂停、继续
    public static EventWaitHandle inOrderEwh = new EventWaitHandle(false, EventResetMode.ManualReset);

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
            if (trigger(single) != null)
            {
                break;
            }

        }
    }


    // 以 [single] 为进入点，执行循环全部循环序号，直到达到要求循环次数
    public static async Task<InOrderLoopStatus> startInOrder(Single single)
    {
        int times = int.Parse(Db.Instance.GetSth<int>(Db.IN_ORDER_LOOP_EVENT_TIMES, 1));
        for (int i = 0; i < times; i++)
        {
            if (Global.startingEventMode != EventMode.InOrder)
            {
                return InOrderLoopStatus.complete;
            }
            InOrderLoopStatus inOrderLoopStatus = await InOrderPriority1LoopARound(single, false);
            // InOrderLoopStatus inOrderLoopStatus = await InOrderPriority1LoopARound(single, i == 0);
            if (Global.startingEventMode != EventMode.InOrder)
            {
                return InOrderLoopStatus.complete;
            }
            if (inOrderLoopStatus == InOrderLoopStatus.complete)
            {
                // 继续当前循环的下一轮。
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.timeout)
            {
                return inOrderLoopStatus;
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.jump)
            {
                return inOrderLoopStatus;
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.jump_fail)
            {
                return inOrderLoopStatus;
            }
            else
            {
                throw new Exception($"未处理 {inOrderLoopStatus}");
            }
            if (Global.startingEventMode != EventMode.InOrder)
            {
                return InOrderLoopStatus.complete;
            }
            await Task.Delay(int.Parse(Db.Instance.GetSth<int>(Db.IN_ORDER_LOOP_EVENT_TIMES, 1)) * 1000);
            if (Global.startingEventMode != EventMode.InOrder)
            {
                return InOrderLoopStatus.complete;
            }
        }
        MessageBox.Show("循环检测模式已结束");
        return InOrderLoopStatus.complete;
    }
    // 以 [single] 为进入点，执行一轮全部循环序号
    public static async Task<InOrderLoopStatus> InOrderPriority1LoopARound(Single single, bool isEnterPoint)
    {
        BsonExpression q1 = Db.Instance.eventModeQuery(EventMode.InOrder);
        BsonExpression q2 = Query.LTE("Priority1", single.Priority1);
        List<List<Single>> singless = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
        .Find(isEnterPoint ? Query.And(q1, q2) : q1)
        .OrderByDescending(e => e.Priority1)
        .ThenByDescending(e => e.Priority2)
        .GroupBy(e => e.Priority1)
        .Select(e => e.ToList())
        .ToList();
        for (int i = 0; i < singless.Count; i++)
        {
            List<Single> singles = singless[i];
            if (Global.startingEventMode != EventMode.InOrder)
            {
                return InOrderLoopStatus.complete;
            }
            InOrderLoopStatus inOrderLoopStatus = await InOrderPriority2Loop(singles.Contains(single) ? single : singles.First());
            if (Global.startingEventMode != EventMode.InOrder)
            {
                return InOrderLoopStatus.complete;
            }
            if (inOrderLoopStatus == InOrderLoopStatus.complete)
            {
                // 继续当前循环的下一轮。
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.timeout)
            {
                return inOrderLoopStatus;
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.jump)
            {
                return inOrderLoopStatus;
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.jump_fail)
            {
                return inOrderLoopStatus;
            }
            else
            {
                throw new Exception($"未处理 {inOrderLoopStatus}");
            }
        }
        return InOrderLoopStatus.complete;
    }


    // 以 [single] 为进入点，执行循环当前循环序号，直到达到要求循环次数
    public static async Task<InOrderLoopStatus> InOrderPriority2Loop(Single single)
    {
        for (int i = 0; i < single.Priority1LoopTimes!; i++)
        {
            if (Global.startingEventMode != EventMode.InOrder)
            {
                return InOrderLoopStatus.complete;
            }
            // InOrderLoopStatus inOrderLoopStatus = await InOrderPriority2ARound(single, i == 0);
            InOrderLoopStatus inOrderLoopStatus = await InOrderPriority2ARound(single, false);
            if (Global.startingEventMode != EventMode.InOrder)
            {
                return InOrderLoopStatus.complete;
            }
            if (inOrderLoopStatus == InOrderLoopStatus.complete)
            {
                // 继续当前循环的下一轮。
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.timeout)
            {
                // 当前循环序号循环结束。此处啥也不干，因为 InOrderPriority2ARound 已经处理，显示超时弹窗。
                return inOrderLoopStatus;
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.jump)
            {
                // 此当前循环序号循环结束。此处啥也不干，因为 InOrderPriority2ARound 已经处理，跳转到新地方了。
                return inOrderLoopStatus;
            }
            else if (inOrderLoopStatus == InOrderLoopStatus.jump_fail)
            {
                // 此当前循环序号循环结束。此处啥也不干，因为 InOrderPriority2ARound 已经处理，显示跳转失败弹窗。
                return inOrderLoopStatus;
            }
            else
            {
                throw new Exception($"未处理 {inOrderLoopStatus}");
            }
        }
        return InOrderLoopStatus.complete;
    }

    // 以 [single] 为进入点，开始执行一轮当前循环序号
    public static async Task<InOrderLoopStatus> InOrderPriority2ARound(Single single, bool isEnterPoint)
    {
        BsonExpression q1 = Db.Instance.eventModeQuery(EventMode.InOrder);
        BsonExpression q2 = Db.Instance.Priority1Query((int)single.Priority1!);
        BsonExpression q3 = Query.LTE("Priority2", new BsonValue((int)single.Priority2!));
        List<Single> singles = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
        .Find(isEnterPoint ? Query.And(q1, q2, q3) : Query.And(q1, q2)).OrderByDescending(e => e.Priority2).ToList();

        for (int i = 0; i < singles.Count; i++)
        {
            Single s = singles[i];
            int timeoutSecond = 0;
            while (true)
            {
                if (Global.startingEventMode != EventMode.InOrder)
                {
                    return InOrderLoopStatus.complete;
                }
                await Task.Delay((int)s.Priority2CheckSecond! * 1000);
                if (Global.startingEventMode != EventMode.InOrder)
                {
                    return InOrderLoopStatus.complete;
                }
                timeoutSecond += (int)s.Priority2CheckSecond!;
                if (timeoutSecond > s.Priority2TimeoutSecond!)
                {

                    return InOrderLoopStatus.timeout;
                    // 有 bug
                    // if (s.Priority2ToWhere == null)
                    // {
                    //     MessageBox.Show($"循环序号：{s.Priority1}，截图序号：{s.Priority2}，检测时间累计超过 {s.Priority2TimeoutSecond} 秒");
                    //     return InOrderLoopStatus.timeout;
                    // }
                    // else
                    // {
                    //     Single? whereSingle = Db.Instance.GetCollection<Single>(Single.TABLE_NAME).FindById(s.Priority2ToWhere!);
                    //     if (whereSingle == null)
                    //     {
                    //         MessageBox.Show($"循环序号：{s.Priority1}，截图序号：{s.Priority2}，检测时间累计超过 {s.Priority2TimeoutSecond} 秒，跳转的序号不存在");
                    //         return InOrderLoopStatus.jump_fail;
                    //     }
                    //     else
                    //     {
                    //         if (Global.startingEventMode != EventMode.InOrder)
                    //         {
                    //             return InOrderLoopStatus.complete;
                    //         }
                    //         await startInOrder(whereSingle!);
                    //         if (Global.startingEventMode != EventMode.InOrder)
                    //         {
                    //             return InOrderLoopStatus.complete;
                    //         }
                    //         return InOrderLoopStatus.jump;
                    //     }
                    // }
                }
                var hasTrigger = trigger(s);
                if (hasTrigger != null)
                {
                    for (int ie = 0; ie < s.EventTriggerTimes - 1; ie++)
                    {
                        if (Global.startingEventMode != EventMode.InOrder)
                        {
                            return InOrderLoopStatus.complete;
                        }
                        await Task.Delay((int)s.EventMillisencondsOnce!);
                        if (Global.startingEventMode != EventMode.InOrder)
                        {
                            return InOrderLoopStatus.complete;
                        }
                        trigger(s);
                    }
                    break;
                }
            }
        }

        return InOrderLoopStatus.complete;
    }

    // 返回触发的single和rectangle，若为 null，则未识别到。
    public static (Single, Rectangle)? trigger(Single s)
    {

        Rectangle? resultRect = GetSingleRectangle(s);
        if (resultRect != null)
        {
            Tool.Clicker.Handle((EventKey)s.EventKey!, resultRect!.Value);
            Console.WriteLine($"识别并触发成功:\n区域:{resultRect.Value}\n序号:循环序号{s.Priority1}-截图序号{s.Priority2}\n");
            return ((Single, Rectangle)?)(s, resultRect!);
        }
        return null;
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
