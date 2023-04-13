using LiteDB;
using Tool;
using CommunityToolkit.Maui.Alerts;

public class FormDragDropListSimultaneously : Form
{
    private List<Single> singleList = new List<Single>();

    private Point dragStartPoint;


    public FlowLayoutPanel bodyFlowLayoutPanel = new FlowLayoutPanel();

    public FlowLayoutPanel eventFlowLayoutPanel = new FlowLayoutPanel();

    public FormDragDropListSimultaneously()
    {
        this.AutoSize = true;

        bodyFlowLayoutPanel.AutoSize = true;
        bodyFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;

        eventFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
        eventFlowLayoutPanel.WrapContents = false;
        eventFlowLayoutPanel.AutoScroll = true;
        eventFlowLayoutPanel.VerticalScroll.Visible = true;
        eventFlowLayoutPanel.Width = K.GET_DEFAULT_WIDTH() - 100;
        eventFlowLayoutPanel.Height = K.GET_DEFAULT_HEIGHT() * 2 / 3;
        eventFlowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;

        // 显示列表中的元素
        ReShowItems();

        bodyFlowLayoutPanel.Controls.Add(startOrCancelControl());
        bodyFlowLayoutPanel.Controls.Add(headerControl());
        bodyFlowLayoutPanel.Controls.Add(eventFlowLayoutPanel);
        this.Controls.Add(bodyFlowLayoutPanel);
    }

    private void ReShowItems()
    {
        eventFlowLayoutPanel.Controls.Clear();
        singleList.Clear();
        // 重新向数据库中获取。
        var result = Db.Instance.GetCollection<Single>(Single.TABLE_NAME).Find(Db.Instance.eventModeQuery(EventMode.Simultaneously));
        singleList.AddRange(result.OrderByDescending((s) => s.Priority1));

        eventFlowLayoutPanel.Controls.Add(ControlTool.separator());

        foreach (var item in singleList)
        {
            var singlePanel = new FlowLayoutPanel();
            singlePanel.FlowDirection = FlowDirection.LeftToRight;
            singlePanel.AutoSize = true;


            singlePanel.Controls.Add(priorityLabelControl(item));
            singlePanel.Controls.Add(upDownPanelControl(item));
            singlePanel.Controls.Add(pictureBoxControl(item));
            singlePanel.Controls.Add(paramControl(item));
            eventFlowLayoutPanel.Controls.Add(singlePanel);
            eventFlowLayoutPanel.Controls.Add(ControlTool.separator());
        }
    }

    // 启动/暂停按钮
    private Control startOrCancelControl()
    {
        Button button = Global.buttonSimultaneously;

        if (Global.startingEventMode == EventMode.Simultaneously)
        {
            button.Text = "暂停";
        }
        else
        {
            button.Text = "启动";
        }
        button.Click += (s, e) =>
        {
            if (Global.startingEventMode == EventMode.Simultaneously)
            {
                Global.startingEventMode = EventMode.None;
                button.Text = "启动";
                TimerElapser.stopSimultaneouslyTimer();
                bodyFlowLayoutPanel.Visible = true;
                Console.WriteLine("暂停成功");
            }
            else
            {
                Global.startingEventMode = EventMode.Simultaneously;
                button.Text = "暂停";
                TimerElapser.startSimultaneouslyTimer();
                bodyFlowLayoutPanel.Visible = false;
                Console.WriteLine("启动成功");

                if (button == Global.buttonSimultaneously)
                {
                    Global.buttonInOrder.Text = "启动";
                    Global.bodyFlowLayoutPanelInOrder.Visible = true;
                }
                else if (button == Global.buttonInOrder)
                {
                    Global.buttonSimultaneously.Text = "启动";
                    bodyFlowLayoutPanel.Visible = true;
                }
                else
                {
                    throw new Exception($"未处理 {button}");
                }
            }
        };
        return button;
    }

    // 参数
    private Control paramControl(Single item)
    {
        var colomn = new FlowLayoutPanel()
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };
        var row1 = new FlowLayoutPanel()
        {
            Parent = colomn,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        var row2 = new FlowLayoutPanel()
        {
            Parent = colomn,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        var row3 = new FlowLayoutPanel()
        {
            Parent = colomn,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        // 在屏幕上检测一次与该截图相似度为？的地方，若检测到多处，则取用？作为触发位置。
        var row1_1 = new Label() { Parent = row1, AutoSize = true, Text = "在屏幕上检测一次与该截图相似度为" };
        var row1_2 = new TextBox() { Parent = row1, Width = 50 };
        row1_2.Text = Db.Instance.GetSingleSth<double>(
            single: item,
            oldValueRead: oS => oS.SimilarityThreshold,
            newValueRead: nS => nS?.SimilarityThreshold,
            newValueToOldValue: (oS, n) => oS.SimilarityThreshold = n,
            min: 0,
            max: 1
        );
        row1_2.Leave += (s, e) =>
        {
            row1_2.Text = Db.Instance.SetSingleSth<double>(
                single: item,
                oldValueRead: oS => oS.SimilarityThreshold,
                newValue: row1_2.Text,
                newValueToOldValue: (oS, n) => oS.SimilarityThreshold = n,
                min: 0,
                max: 1
            );
        };
        var row1_3 = new Label() { Parent = row1, AutoSize = true, Text = "的地方，若检测到多处，则取用" };
        var row1_4 = new ComboBox()
        {
            Parent = row1,
            Width = 200,
            DropDownWidth = 300,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        row1_4.MouseWheel += (s, e) => { ((HandledMouseEventArgs)e).Handled = true; };
        foreach (var t in Enum.GetValues(typeof(PositionBlockType)))
        {
            row1_4.Items.Add(Tool.Method.GetEnumDescription((PositionBlockType)t));
        }
        row1_4.SelectedIndex = int.Parse(Db.Instance.GetSingleSth<int>(
            single: item,
            oldValueRead: oS => (int?)oS.PositionBlockType,
            newValueRead: nS => (int?)nS?.PositionBlockType,
            newValueToOldValue: (oS, n) => oS.PositionBlockType = (PositionBlockType)n,
            min: 0,
            max: Enum.GetNames(typeof(PositionBlockType)).Length
            ));
        row1_4.SelectedIndexChanged += (s, e) =>
        {
            item.PositionBlockType = (PositionBlockType)int.Parse(Db.Instance.SetSingleSth<int>(
                single: item,
                oldValueRead: oS => (int?)oS.PositionBlockType,
                newValue: row1_4.SelectedIndex.ToString(),
                newValueToOldValue: (oS, n) => oS.PositionBlockType = (PositionBlockType)n,
                min: 0,
                max: Enum.GetNames(typeof(PositionBlockType)).Length
            ));
        };
        var row1_5 = new Label() { Parent = row1, AutoSize = true, Text = "作为触发位置。" };

        // 如果触发该检测，则对其区域进行一次？操作。
        var row2_1 = new Label() { Parent = row2, AutoSize = true, Text = "如果触发该检测，则对其区域进行一次" };
        var row2_2 = new ComboBox()
        {
            Parent = row2,
            Width = 100,
            DropDownWidth = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        row2_2.MouseWheel += (s, e) => { ((HandledMouseEventArgs)e).Handled = true; };
        foreach (var t in Enum.GetValues(typeof(EventKey)))
        {
            row2_2.Items.Add(Tool.Method.GetEnumDescription((EventKey)t));
        }
        row2_2.SelectedIndex = int.Parse(Db.Instance.GetSingleSth<int>(
            single: item,
            oldValueRead: oS => (int?)oS.EventKey,
            newValueRead: nS => (int?)nS?.EventKey,
            newValueToOldValue: (oS, n) => oS.EventKey = (EventKey)n,
            min: 0,
            max: Enum.GetNames(typeof(EventKey)).Length
            ));
        row2_2.SelectedIndexChanged += (s, e) =>
        {
            item.EventKey = (EventKey)int.Parse(Db.Instance.SetSingleSth<int>(
                single: item,
                oldValueRead: oS => (int?)oS.EventKey,
                newValue: row2_2.SelectedIndex.ToString(),
                newValueToOldValue: (oS, n) => oS.EventKey = (EventKey)n,
                min: 0,
                max: Enum.GetNames(typeof(EventKey)).Length
            ));
        };

        return colomn;
    }

    // 添加截图按钮
    private Control headerControl()
    {
        var header = new FlowLayoutPanel();
        header.FlowDirection = FlowDirection.TopDown;
        header.AutoSize = true;

        var row1 = new FlowLayoutPanel();
        row1.FlowDirection = FlowDirection.LeftToRight;
        row1.AutoSize = true;
        // 【同时检测模式】启动后，每？秒都会在屏幕上同时检测下面列表中的全部截图，并会触发序号最高的事件1次。
        var text1 = new Label() { Parent = row1, AutoSize = true, Text = "【同时检测模式】启动后，每" };
        var text2 = new TextBox() { Parent = row1, Width = 50 };
        text2.Text = Db.Instance.GetSth<int>(Db.SIMULTANEOUSLY_LOOP_EVENT_TIME_CYCLE, 1);
        text2.Leave += (s, e) =>
        {
            text2.Text = Db.Instance.SetSth<int>(Db.SIMULTANEOUSLY_LOOP_EVENT_TIME_CYCLE, text2.Text, 1);
        };
        var text3 = new Label() { Parent = row1, AutoSize = true, Text = "秒都会在屏幕上同时检测下面列表中的全部截图，并会触发序号最高的事件1次。" };
        var row2 = new Label() { AutoSize = true, Text = "注意：截图越多，循环时间应该设置的越大，因为截图越多每次同时检测耗时就越长" };

        Button row3 = new Button();
        row3.Text = "➕ 增加截图";
        row3.Scale(new SizeF(2, 2));
        row3.MouseClick += (s, e) =>
        {
            var maxSingleResult = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
            .Find(Db.Instance.eventModeQuery(EventMode.Simultaneously)).OrderByDescending((e) => e.Priority1).FirstOrDefault();
            if (maxSingleResult == null)
            {
                Db.Instance.insertOrModifySingleEntity(
                    new Single(
                        imagePath: null,
                        explain: null,
                        similarityThreshold: 0.7,
                        positionBlockType: PositionBlockType.left_top,
                        eventMode: EventMode.Simultaneously,
                        eventMillisencondsOnce: 0,
                        eventKey: EventKey.mouseLeftClick,
                        eventTriggerTimes: 1,
                        priority1: 0,
                        priority1LoopTimes: 3,
                        priority2ToNextAfterSecond: null,
                        priority2: null,
                        priority2CheckSecond: null,
                        priority2TimeoutSecond: null,
                        priority2toWhere: null
                    )
                    );
            }
            else
            {
                Db.Instance.insertOrModifySingleEntity(new Single(
                        imagePath: null,
                        explain: null,
                        similarityThreshold: 0.7,
                        positionBlockType: PositionBlockType.left_top,
                        eventMode: EventMode.Simultaneously,
                        eventMillisencondsOnce: 0,
                        eventKey: EventKey.mouseLeftClick,
                        eventTriggerTimes: 1,
                        priority1: maxSingleResult.Priority1 + 1,
                        priority1LoopTimes: 3,
                        priority2ToNextAfterSecond: null,
                        priority2: null,
                        priority2CheckSecond: null,
                        priority2TimeoutSecond: null,
                        priority2toWhere: null
                    ));
            }
            ReShowItems();
        };


        header.Controls.Add(row1);
        header.Controls.Add(row2);
        header.Controls.Add(row3);
        return header;
    }

    // 优先级
    private Control priorityLabelControl(Single item)
    {
        var priorityLabel = new Label();
        priorityLabel.Text = item.Priority1.ToString();
        priorityLabel.AutoSize = true;
        return priorityLabel;
    }

    // 上移和下移
    private Control upDownPanelControl(Single item)
    {
        var upDownPanel = new FlowLayoutPanel();
        upDownPanel.FlowDirection = FlowDirection.TopDown;

        Button upButton = new Button();
        Button downButton = new Button();

        upButton.Text = "上移";
        downButton.Text = "下移";

        upButton.MouseClick += (s, e) =>
        {
            int current = singleList.IndexOf(item);
            if (current == 0)
            {
                return;
            }
            else
            {
                int last = current - 1;
                int tempPriority = (int)singleList[current].Priority1!;
                singleList[current].Priority1 = singleList[last].Priority1;
                singleList[last].Priority1 = tempPriority;
                Db.Instance.insertOrModifySingleEntity(singleList[current]);
                Db.Instance.insertOrModifySingleEntity(singleList[last]);
                ReShowItems();
            }
        };

        downButton.MouseClick += (s, e) =>
        {
            int current = singleList.IndexOf(item);
            if (current == singleList.Count - 1)
            {
                return;
            }
            else
            {
                int next = current + 1;
                int tempPriority = (int)singleList[current].Priority1!;
                singleList[current].Priority1 = singleList[next].Priority1;
                singleList[next].Priority1 = tempPriority;
                Db.Instance.insertOrModifySingleEntity(singleList[current]);
                Db.Instance.insertOrModifySingleEntity(singleList[next]);
                ReShowItems();
            }
        };

        upDownPanel.AutoSize = true;
        upDownPanel.Controls.Add(upButton);
        upDownPanel.Controls.Add(downButton);
        upDownPanel.Controls.Add(removeSingleButtonControl(item));
        return upDownPanel;
    }

    // 截选图
    private Control pictureBoxControl(Single item)
    {
        var column = new FlowLayoutPanel();
        column.AutoSize = true;
        column.FlowDirection = FlowDirection.TopDown;

        var pictureBox = new PictureBox();
        ToolTip toolTip1 = new ToolTip();
        toolTip1.SetToolTip(pictureBox, "点击此区域进行截取图片");
        toolTip1.IsBalloon = true;
        toolTip1.ShowAlways = true;

        if (item.ImagePath == null)
        {
            // 假图片
            ControlTool.emptyImage(pictureBox);
        }
        else
        {
            if (File.Exists(item.ImagePath))
            {
                pictureBox.Image = Image.FromFile(item.ImagePath);
            }
            else
            {
                ControlTool.emptyImage(pictureBox);
            }
        }
        pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
        pictureBox.Width = 100;
        pictureBox.Height = 100;
        pictureBox.MouseClick += (s, e) =>
        {
            // 隐藏全部窗体
            foreach (Form item in Application.OpenForms)
            {
                item.Visible = false;
            }

            // 存放
            var savePath = $"{K.ASSETS_SINGLE}/{Guid.NewGuid().ToString()}.png";
            // 截图
            var screenRegionSelector = new ScreenRegionSelector(savePath, new Size(20, 20));
            screenRegionSelector.ShowDialog();
            if (screenRegionSelector.screenRegionSelectorResult == ScreenRegionSelectorResult.ok)
            {
                item.ImagePath = savePath;
                Db.Instance.insertOrModifySingleEntity(item);
                pictureBox.Image = Image.FromFile(item.ImagePath);
            }
            // 显示全部窗体
            foreach (Form item in Application.OpenForms)
            {
                item.Visible = true;
            }
        };

        var testButton = new Button() { AutoSize = true, Text = "识别测试" };
        testButton.MouseClick += async (s, e) =>
        {
            pictureBox.Visible = false;
            await Task.Delay(100);
            var result = TimerElapser.trigger(item);
            var show = MessageBox.Show(result == null ?
                  "未识别到" :
                  $"已识别到并触发成功！\n区域:{result!.Value.Item2}\n序号:{result!.Value.Item1.Priority1}\n");
            pictureBox.Visible = true;
        };

        column.Controls.Add(pictureBox);
        column.Controls.Add(testButton);
        return column;
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
            ReShowItems();
        };
        return removeButton;
    }
}
