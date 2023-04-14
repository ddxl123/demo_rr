using LiteDB;
using Tool;

public class FormDragDropListInOrder : Form
{
    private List<List<Single>> groupSingleList = new List<List<Single>>();

    private Point dragStartPoint;

    public FlowLayoutPanel mainFlowLayoutPanel = new FlowLayoutPanel();

    public FlowLayoutPanel bodyFlowLayoutPanel = Global.bodyFlowLayoutPanelInOrder;

    public FlowLayoutPanel allEventFlowLayoutPanel = new FlowLayoutPanel();

    public Button startOrCancelbutton = new Button();


    public FormDragDropListInOrder()
    {
        this.AutoSize = true;
        mainFlowLayoutPanel.AutoSize = true;
        mainFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;

        bodyFlowLayoutPanel.AutoSize = true;
        bodyFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;

        // allEventFlowLayoutPanel.AutoSize = true;
        allEventFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
        allEventFlowLayoutPanel.VerticalScroll.Visible = true;
        allEventFlowLayoutPanel.WrapContents = false;
        allEventFlowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;
        allEventFlowLayoutPanel.AutoScroll = true;
        allEventFlowLayoutPanel.Size = new Size(K.GET_DEFAULT_WIDTH(), K.GET_DEFAULT_HEIGHT() * 2 / 3);
        allEventFlowLayoutPanel.Padding = new Padding() { Left = 10, Top = 10, Bottom = 10 };

        // singleEventFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
        // singleEventFlowLayoutPanel.WrapContents = false;
        // singleEventFlowLayoutPanel.AutoScroll = true;
        // singleEventFlowLayoutPanel.VerticalScroll.Visible = true;
        // singleEventFlowLayoutPanel.Width = K.GET_DEFAULT_WIDTH() - 100;
        // singleEventFlowLayoutPanel.Height = 200;
        // singleEventFlowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;

        // 显示列表中的元素
        // ReShowItems();

        Level1EventItems();

        bodyFlowLayoutPanel.Controls.Add(mainExplainControl());
        bodyFlowLayoutPanel.Controls.Add(addLoopEventButtonControl());
        bodyFlowLayoutPanel.Controls.Add(allEventFlowLayoutPanel);
        // bodyFlowLayoutPanel.Controls.Add(singleEventFlowLayoutPanel);

        mainFlowLayoutPanel.Controls.Add(startOrCancelControl(null));
        mainFlowLayoutPanel.Controls.Add(bodyFlowLayoutPanel);
        this.Controls.Add(mainFlowLayoutPanel);
    }

    private Control startOrCancelControl(Single? startSingle)
    {
        startOrCancelbutton.Scale(new SizeF(1.5f, 1.5f));
        startOrCancelbutton.Text = Global.startingEventMode == EventMode.InOrder ? "停止" : "启动";
        startOrCancelbutton.MouseClick += async (s, e) =>
        {
            await startOrCancel(startSingle);
        };
        return startOrCancelbutton;
    }

    // [startSingle] 为 null 时从第一个开始启动。
    private async Task startOrCancel(Single? startSingle)
    {

        if (Global.startingEventMode == EventMode.InOrder)
        {
            startOrCancelbutton.Text = "启动";
            Global.bodyFlowLayoutPanelInOrder.Visible = true;
            Global.startingEventMode = EventMode.None;
        }
        else
        {
            startOrCancelbutton.Text = "暂停";
            Global.bodyFlowLayoutPanelInOrder.Visible = false;
            Global.startingEventMode = EventMode.InOrder;
            var enterSingle = startSingle ?? Db.Instance.GetCollection<Single>(Single.TABLE_NAME).Find(Db.Instance.eventModeQuery(EventMode.InOrder)).OrderByDescending(e => e.Priority1).First();
            await TimerElapser.startInOrder(enterSingle);
        }
    }



    private Control mainExplainControl()
    {
        var column = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
        };

        var row1 = new FlowLayoutPanel()
        {
            Parent = column,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
        };
        var row2 = new FlowLayoutPanel()
        {
            Parent = column,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
        };
        // 【顺序检测模式】启动后，将按照【循环序号】从大到小的顺序依次执行，每个循环都会按照其【截图序号】从大到小的顺序进行检测。
        // 以下全部循环结束后，等待？秒后，会再次重复以下全部循环，将重复以下全部循环？次。
        var text1 = new Label() { Parent = row1, AutoSize = true, Text = "【顺序检测模式】启动后，将按照【循环序号】从大到小的顺序依次执行，每个循环都会按照其【截图序号】从大到小的顺序进行检测。" };
        var text2 = new Label() { Parent = row2, AutoSize = true, Text = "以下全部循环结束后，等待" };
        var text3 = new TextBox() { Parent = row2, AutoSize = true, Width = 50 };
        text3.Text = Db.Instance.GetSth<int>(Db.IN_ORDER_LOOP_EVENT_WAIT_SECOND, 0);
        text3.LostFocus += (s, e) =>
        {
            text3.Text = Db.Instance.SetSth<int>(Db.IN_ORDER_LOOP_EVENT_WAIT_SECOND, text3.Text, 0);
        };
        var text4 = new Label() { Parent = row2, AutoSize = true, Text = "秒后，会再次重复以下全部循环，将重复以下全部循环" };
        var text5 = new TextBox() { Parent = row2, AutoSize = true, Width = 50 };
        text5.Text = Db.Instance.GetSth<int>(Db.IN_ORDER_LOOP_EVENT_TIMES, 1);
        text5.LostFocus += (s, e) =>
        {
            text5.Text = Db.Instance.SetSth<int>(Db.IN_ORDER_LOOP_EVENT_TIMES, text5.Text, 1);
        };
        var text6 = new Label() { Parent = row2, AutoSize = true, Text = "次（每次完成一次都会减1）。" };

        // var addEventPanel = new FlowLayoutPanel();
        // addEventPanel.FlowDirection = FlowDirection.TopDown;
        // addEventPanel.AutoSize = true;
        // var text = new Label();
        // text.AutoSize = true;
        // text.Text = "会按照顺序依次进行点击操作";

        // var loopTimePanel = new FlowLayoutPanel();
        // loopTimePanel.FlowDirection = FlowDirection.LeftToRight;
        // loopTimePanel.AutoSize = true;
        // var timeLabel1 = new Label();
        // timeLabel1.Text = "每";
        // timeLabel1.AutoSize = true;
        // var timeLabel2 = new Label();
        // timeLabel2.Text = "秒触发一次循环事件";
        // timeLabel2.AutoSize = true;
        // var timeBox = new TextBox();

        // timeBox.Text = Db.Instance.GetValueByKey(Db.LOOP_EVENT_TIME_CYCLE_IN_ORDER);
        // Db.Instance.setLoopTime(Db.Instance.GetValueByKey(Db.LOOP_EVENT_TIME_CYCLE_IN_ORDER), EventMode.InOrder);
        // timeBox.Leave += (s, e) =>
        // {
        //     timeBox.Text = Db.Instance.setLoopTime(timeBox.Text, EventMode.InOrder).ToString();
        // };
        // loopTimePanel.Controls.Add(timeLabel1);
        // loopTimePanel.Controls.Add(timeBox);
        // loopTimePanel.Controls.Add(timeLabel2);

        // addEventPanel.Controls.Add(text);
        // addEventPanel.Controls.Add(loopTimePanel);
        return column;
    }

    private Control addLoopEventButtonControl()
    {
        Button addButton = new Button();
        addButton.Text = "➕ 增加循环";
        addButton.Scale(new SizeF(2, 2));
        addButton.MouseClick += (s, e) =>
        {
            Single? maxSingleResult = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
            .Find(Db.Instance.eventModeQuery(EventMode.InOrder)).OrderByDescending((e) => e.Priority1).FirstOrDefault();

            if (maxSingleResult == null)
            {

                Db.Instance.insertOrModifySingleEntity(new Single(
                        imagePath: null,
                        explain: null,
                        similarityThreshold: 0.7,
                        positionBlockType: PositionBlockType.left_top,
                        eventMode: EventMode.InOrder,
                        eventMillisencondsOnce: 0,
                        eventKey: EventKey.mouseLeftClick,
                        eventTriggerTimes: 1,
                        priority1: 0,
                        priority1LoopTimes: 1,
                        priority2ToNextAfterSecond: 0,
                        priority2: 0,
                        priority2CheckSecond: 1,
                        priority2TimeoutSecond: 999999,
                        priority2toWhere: null
                    ));
            }
            else
            {
                Db.Instance.insertOrModifySingleEntity(new Single(
                        imagePath: null,
                        explain: null,
                        similarityThreshold: 0.7,
                        positionBlockType: PositionBlockType.left_top,
                        eventMode: EventMode.InOrder,
                        eventMillisencondsOnce: 0,
                        eventKey: EventKey.mouseLeftClick,
                        eventTriggerTimes: 1,
                        priority1: (maxSingleResult?.Priority1 ?? -1) + 1,
                        priority1LoopTimes: 1,
                        priority2ToNextAfterSecond: 0,
                        priority2: 0,
                        priority2CheckSecond: 1,
                        priority2TimeoutSecond: 999999,
                        priority2toWhere: null
                    ));
            }
            Level1EventItems();
        };

        return addButton;
    }

    private void Level1EventItems()
    {
        // 重新向数据库中获取。
        var result = Db.Instance.GetCollection<Single>(Single.TABLE_NAME).Find(Db.Instance.eventModeQuery(EventMode.InOrder));
        var group = result.GroupBy(e => e.Priority1).OrderByDescending(g => g.Key).ToList();

        allEventFlowLayoutPanel.Controls.Clear();
        groupSingleList.Clear();
        group.ForEach((e) =>
        {
            groupSingleList.Add(e.ToList());
        });

        groupEventFlowLayoutPanelControl();
    }
    private void groupEventFlowLayoutPanelControl()
    {
        groupSingleList.ForEach((groupSingle) =>
        {
            var groupEventFlowLayoutPanel = new FlowLayoutPanel();
            groupEventFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            groupEventFlowLayoutPanel.AutoSize = true;
            // groupEventFlowLayoutPanel.WrapContents = false;
            // groupEventFlowLayoutPanel.AutoScroll = true;
            // groupEventFlowLayoutPanel.VerticalScroll.Visible = true;
            groupEventFlowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;
            // groupEventFlowLayoutPanel.Width = K.GET_DEFAULT_WIDTH() - 100;
            // groupEventFlowLayoutPanel.Height = 200;


            var row1FlowLayoutPanel = new FlowLayoutPanel();
            row1FlowLayoutPanel.AutoSize = true;
            row1FlowLayoutPanel.FlowDirection = FlowDirection.TopDown;

            var row1_1 = new FlowLayoutPanel() { Parent = row1FlowLayoutPanel };
            row1_1.AutoSize = true;
            row1_1.FlowDirection = FlowDirection.LeftToRight;
            var row1_2 = new FlowLayoutPanel() { Parent = row1FlowLayoutPanel };
            row1_2.AutoSize = true;
            row1_2.FlowDirection = FlowDirection.LeftToRight;
            var row1_3 = new FlowLayoutPanel() { Parent = row1FlowLayoutPanel };
            row1_3.AutoSize = true;
            row1_3.FlowDirection = FlowDirection.LeftToRight;

            var p = new Label() { Parent = row1_1, AutoSize = true };
            p.Text = $"循环序号：{groupSingle.First().Priority1}";

            // 本次循环将按照截图序号从大到小顺序依次进行检测，重复循环？次后，再等待？秒才继续进入下一个循环序号（若设为0秒，则本次循环结束后，将立即进入下一个循环序号）。
            var panel1 = new Label() { Parent = row1_2, AutoSize = true, Text = "本次循环将按照【截图序号】从大到小顺序依次进行检测，重复循环" };
            var panel2 = new TextBox() { Parent = row1_2, AutoSize = true, Width = 50 };
            panel2.Text = Db.Instance.GetSingleSth<int>(
                single: groupSingle.First(),
                oldValueRead: (oS) => oS.Priority1LoopTimes,
                newValueRead: (nS) => nS?.Priority1LoopTimes,
                newValueToOldValue: (oS, n) => { oS.Priority1LoopTimes = n; },
                min: 1,
                max: null
                );
            panel2.LostFocus += (s, e) =>
            {
                groupSingle.ForEach(single =>
                {
                    panel2.Text = Db.Instance.SetSingleSth<int>(single, oS => oS.Priority1LoopTimes, panel2.Text, (oS, n) => oS.Priority1LoopTimes = n, 1, null);
                });
            };
            var panel3 = new Label() { Parent = row1_2, AutoSize = true, Text = "次后，再等待" };
            var panel4 = new TextBox() { Parent = row1_2, AutoSize = true, Width = 50 };
            panel4.Text = Db.Instance.GetSingleSth<int>(
                single: groupSingle.First(),
                oldValueRead: (oS) => oS.Priority2ToNextAfterSecond,
                newValueRead: (nS) => nS?.Priority2ToNextAfterSecond,
                newValueToOldValue: (oS, n) => { oS.Priority2ToNextAfterSecond = n; },
                min: 0,
                max: null
            );
            panel4.LostFocus += (s, e) =>
            {
                groupSingle.ForEach(single =>
                {
                    panel4.Text = Db.Instance.SetSingleSth<int>(
                        single,
                        oS => oS.Priority2ToNextAfterSecond,
                        panel4.Text,
                        (oS, n) => oS.Priority2ToNextAfterSecond = n,
                        0,
                        null
                        );
                });
            };
            var panel5 = new Label() { Parent = row1_2, AutoSize = true, Text = "秒才继续进入下一个循环序号（若设为0秒，则本次循环结束后，将立即进入下一个循环序号）" };

            var upButton = new Button() { Parent = row1_1 };
            upButton.Text = "上移";
            upButton.MouseClick += (e, s) =>
            {
                var currentIndex = groupSingleList.IndexOf(groupSingle);
                var lastIndex = currentIndex - 1;
                if (currentIndex == 0)
                {
                    return;
                }
                var currentP = groupSingleList[currentIndex].First().Priority1;
                var lastP = groupSingleList[lastIndex].First().Priority1;
                groupSingleList[lastIndex].ForEach(c =>
                {
                    c.Priority1 = currentP;
                    Db.Instance.insertOrModifySingleEntity(c);
                });
                groupSingleList[currentIndex].ForEach(c =>
                {
                    c.Priority1 = lastP;
                    Db.Instance.insertOrModifySingleEntity(c);
                });
                Level1EventItems();
            };
            var downButton = new Button() { Parent = row1_1 };
            downButton.Text = "下移";
            downButton.MouseClick += (e, s) =>
            {
                var currentIndex = groupSingleList.IndexOf(groupSingle);
                Console.WriteLine(currentIndex);
                var nextIndex = currentIndex + 1;
                if (currentIndex == groupSingleList.Count - 1)
                {
                    return;
                }
                var currentP = groupSingleList[currentIndex].First().Priority1;
                var nextP = groupSingleList[nextIndex].First().Priority1;
                groupSingleList[nextIndex].ForEach(c =>
                {
                    c.Priority1 = currentP;
                    Db.Instance.insertOrModifySingleEntity(c);
                });
                groupSingleList[currentIndex].ForEach(c =>
                {
                    c.Priority1 = nextP;
                    Db.Instance.insertOrModifySingleEntity(c);
                });
                Level1EventItems();
            };
            var removeButton = new Button() { Parent = row1_1 };
            removeButton.Text = "移除";
            removeButton.ForeColor = Color.Red;
            removeButton.MouseClick += (s, ev) =>
            {
                groupSingle.ForEach(si =>
                {
                    Db.Instance.GetCollection<Single>(Single.TABLE_NAME).Delete(si.Id);
                });
                Level1EventItems();
            };

            var viewGroupSingleButton = new Button() { AutoSize = true, Text = "查看截图" };
            viewGroupSingleButton.MouseClick += (s, e) =>
            {
                new GroupEventForm2(groupSingle).Show();
            };
            groupEventFlowLayoutPanel.Controls.Add(row1FlowLayoutPanel);
            groupEventFlowLayoutPanel.Controls.Add(viewGroupSingleButton);

            allEventFlowLayoutPanel.Controls.Add(groupEventFlowLayoutPanel);
            allEventFlowLayoutPanel.Controls.Add(new Panel() { Height = 40 });
        });
    }


    // 将list中，相同的a1成组，并根据a1大小排序组。
    // private void ReShowItems()
    // {
    //     singleEventFlowLayoutPanel.Controls.Clear();
    //     groupSingleList.Clear();
    //     // 重新向数据库中获取。
    //     var result = LiteDBSingleton.Instance.GetCollection<Single>(Single.TABLE_NAME).Find(LiteDBSingleton.Instance.eventModeQuery(EventMode.InOrder));
    //     groupSingleList.AddRange(result.OrderByDescending((s) => s.Priority1));

    //     foreach (var item in groupSingleList)
    //     {
    //         // 创建一个包含两个子控件的元素
    //         var singlePanel = new FlowLayoutPanel();
    //         singlePanel.FlowDirection = FlowDirection.LeftToRight;
    //         singlePanel.AutoSize = true;

    //         singlePanel.Controls.Add(priorityLabelControl(item));
    //         singlePanel.Controls.Add(upDownPanelControl(item));
    //         singlePanel.Controls.Add(pictureBoxControl(item));
    //         singlePanel.Controls.Add(paramControl(item));
    //         singleEventFlowLayoutPanel.Controls.Add(singlePanel);
    //         singleEventFlowLayoutPanel.Controls.Add(ControlTool.separator());
    //     }
    // }

    // private Control startOrCancelControl()
    // {
    //     Button button = Global.buttonInOrder;

    //     if (Global.startingEventMode == EventMode.InOrder)
    //     {
    //         button.Text = "暂停";
    //     }
    //     else
    //     {
    //         button.Text = "启动";
    //     }
    //     button.Click += (s, e) =>
    //     {
    //         if (Global.startingEventMode == EventMode.InOrder)
    //         {
    //             Global.startingEventMode = EventMode.None;
    //             button.Text = "启动";
    //             Global.simultaneouslyTimer.Stop();
    //             bodyFlowLayoutPanel.Visible = true;
    //             Console.WriteLine("暂停成功");
    //         }
    //         else
    //         {
    //             Global.startingEventMode = EventMode.InOrder;
    //             button.Text = "暂停";
    //             Db.Instance.setLoopTime(Db.Instance.GetValueByKey(Db.LOOP_EVENT_TIME_CYCLE_IN_ORDER), EventMode.InOrder);
    //             Global.simultaneouslyTimer.Start();
    //             bodyFlowLayoutPanel.Visible = false;
    //             Console.WriteLine("启动成功");

    //             if (button == Global.buttonSimultaneously)
    //             {
    //                 Global.buttonInOrder.Text = "启动";
    //                 Global.bodyFlowLayoutPanelSimultaneously.Visible = true;
    //             }
    //             else if (button == Global.buttonInOrder)
    //             {
    //                 Global.buttonSimultaneously.Text = "启动";
    //                 Global.bodyFlowLayoutPanelInOrder.Visible = true;
    //             }
    //             else
    //             {
    //                 throw new Exception($"未处理 {button}");
    //             }
    //         }
    //     };
    //     return button;
    // }


    // 优先级
    private Control priorityLabelControl(Single item)
    {
        var priorityLabel = new Label();
        priorityLabel.Text = item.Priority1.ToString();
        priorityLabel.AutoSize = true;
        return priorityLabel;
    }

    // 上移和下移
    // private Control upDownPanelControl(Single item)
    // {
    //     var upDownPanel = new FlowLayoutPanel();
    //     upDownPanel.FlowDirection = FlowDirection.TopDown;

    //     Button upButton = new Button();
    //     Button downButton = new Button();

    //     upButton.Text = "上移";
    //     downButton.Text = "下移";

    //     upButton.MouseClick += (s, e) =>
    //     {
    //         int current = groupSingleList.IndexOf(item);
    //         if (current == 0)
    //         {
    //             return;
    //         }
    //         else
    //         {
    //             int last = current - 1;
    //             int tempPriority = groupSingleList[current].Priority1;
    //             groupSingleList[current].Priority1 = groupSingleList[last].Priority1;
    //             groupSingleList[last].Priority1 = tempPriority;
    //             LiteDBSingleton.Instance.insertOrModifySingleEntity(groupSingleList[current]);
    //             LiteDBSingleton.Instance.insertOrModifySingleEntity(groupSingleList[last]);
    //             ReShowItems();
    //         }
    //     };

    //     downButton.MouseClick += (s, e) =>
    //     {
    //         int current = groupSingleList.IndexOf(item);
    //         if (current == groupSingleList.Count - 1)
    //         {
    //             return;
    //         }
    //         else
    //         {
    //             int next = current + 1;
    //             int tempPriority = groupSingleList[current].Priority1;
    //             groupSingleList[current].Priority1 = groupSingleList[next].Priority1;
    //             groupSingleList[next].Priority1 = tempPriority;
    //             LiteDBSingleton.Instance.insertOrModifySingleEntity(groupSingleList[current]);
    //             LiteDBSingleton.Instance.insertOrModifySingleEntity(groupSingleList[next]);
    //             ReShowItems();
    //         }
    //     };

    //     upDownPanel.AutoSize = true;
    //     upDownPanel.Controls.Add(upButton);
    //     upDownPanel.Controls.Add(downButton);
    //     upDownPanel.Controls.Add(removeSingleButtonControl(item));
    //     return upDownPanel;
    // }


    // 描述
    private Control explainControl(Single item)
    {
        var explains = new FlowLayoutPanel();
        explains.FlowDirection = FlowDirection.LeftToRight;
        explains.AutoSize = true;
        var text = new Label();
        text.AutoSize = true;
        text.Text = "选区描述：";
        var textBox = new TextBox();
        textBox.AutoSize = true;
        textBox.Text = item.Explain;
        textBox.Leave += (s, e) =>
        {
            item.Explain = textBox.Text;
            Db.Instance.insertOrModifySingleEntity(item);
        };
        explains.Controls.Add(text);
        explains.Controls.Add(textBox);
        return explains;
    }

    // 移除事件按钮
    private Control removeSingleButtonControl(Single item)
    {
        var removeButton = new Button();
        removeButton.Text = "移除";
        removeButton.ForeColor = Color.Red;
        removeButton.MouseClick += (s, e) =>
        {
            Db.Instance.GetCollection<Single>(Single.TABLE_NAME).Delete(item.Id);
            // ReShowItems();
        };
        return removeButton;
    }


}
