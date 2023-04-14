using LiteDB;
using Tool;

public class GroupEventForm2 : Form
{
    public Single? copiedSingle;
    public GroupEventForm2(List<Single> groupSingle)
    {

        Width = K.GET_DEFAULT_WIDTH() + 100;
        Height = 700;

        var column = new FlowLayoutPanel();
        column.FlowDirection = FlowDirection.TopDown;
        column.AutoSize = true;

        var row2FlowLayoutPanel = new FlowLayoutPanel();
        row2FlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
        row2FlowLayoutPanel.WrapContents = false;
        row2FlowLayoutPanel.AutoScroll = true;
        row2FlowLayoutPanel.VerticalScroll.Visible = true;
        row2FlowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;
        row2FlowLayoutPanel.Width = K.GET_DEFAULT_WIDTH();
        row2FlowLayoutPanel.Height = 600;



        var singlesColumnFlowLayoutPanel = new FlowLayoutPanel();
        singlesColumnFlowLayoutPanel.AutoSize = true;
        singlesColumnFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;


        var addLoopButton = new Button();
        addLoopButton.AutoSize = true;
        addLoopButton.Scale(new SizeF(1.5f, 1.5f));
        addLoopButton.Text = "➕ 增加截图";
        addLoopButton.MouseClick += (s, e) =>
        {
            var newSingle = new Single(
                    imagePath: null,
                    explain: null,
                    similarityThreshold: 0.7,
                    positionBlockType: PositionBlockType.left_top,
                    eventMode: EventMode.InOrder,
                    eventMillisencondsOnce: 0,
                    eventKey: EventKey.mouseLeftClick,
                    eventTriggerTimes: 1,
                    priority1: groupSingle.First().Priority1,
                    priority1LoopTimes: 1,
                    priority2ToNextAfterSecond: 0,
                    priority2: groupSingle.OrderByDescending(o => o.Priority2).First().Priority2! + 1,
                    priority2CheckSecond: 1,
                    priority2TimeoutSecond: 99999,
                    priority2toWhere: null
                );
            Db.Instance.insertOrModifySingleEntity(newSingle);
            Level2EventItems(singlesColumnFlowLayoutPanel, groupSingle);
        };
        Level2EventItems(singlesColumnFlowLayoutPanel, groupSingle);
        row2FlowLayoutPanel.Controls.Add(singlesColumnFlowLayoutPanel);
        column.Controls.Add(addLoopButton);
        column.Controls.Add(row2FlowLayoutPanel);
        this.Controls.Add(column);
    }

    private void Level2EventItems(FlowLayoutPanel singlesColumnFlowLayoutPanel, List<Single> singles)
    {

        var q = Query.And(Db.Instance.eventModeQuery(EventMode.InOrder), Query.EQ("Priority1", singles.First().Priority1));
        var result = Db.Instance.GetCollection<Single>(Single.TABLE_NAME).Find(q).OrderByDescending(e => e.Priority2).ToList();
        singlesColumnFlowLayoutPanel.Controls.Clear();
        singles.Clear();
        singles.AddRange(result);

        singles.ForEach((single) =>
        {
            var row1FlowLayoutPanel = new FlowLayoutPanel();
            row1FlowLayoutPanel.AutoSize = true;
            row1FlowLayoutPanel.FlowDirection = FlowDirection.LeftToRight;
            var subOrderLabel = new Label();
            subOrderLabel.AutoSize = true;
            subOrderLabel.Text = $"截图序号：{single.Priority2}";
            // var row1_enter_button = new Button();
            // row1_enter_button.AutoSize = true;
            // row1_enter_button.Text = "从这个截图序号启动";
            // row1_enter_button.MouseClick += async (s, e) =>
            // {
            //     await startOrCancel(single);
            // };

            // row1FlowLayoutPanel.Controls.Add(row1_enter_button);

            var row2FlowLayoutPanel = new FlowLayoutPanel();
            row2FlowLayoutPanel.AutoSize = true;
            row2FlowLayoutPanel.FlowDirection = FlowDirection.LeftToRight;

            // 左侧按钮 column
            var leftButtonFlowLayoutPanel = new FlowLayoutPanel();
            leftButtonFlowLayoutPanel.AutoSize = true;
            leftButtonFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;

            // 左侧按钮
            var upButton = new Button();
            upButton.Text = "上移";
            upButton.MouseClick += (s, e) =>
            {
                var currentIndex = singles.IndexOf(single);
                var lastIndex = currentIndex - 1;
                if (currentIndex == 0)
                {
                    return;
                }
                var currentP = singles[currentIndex].Priority2;
                var lastP = singles[lastIndex].Priority2;
                singles[currentIndex].Priority2 = lastP;
                singles[lastIndex].Priority2 = currentP;
                Db.Instance.insertOrModifySingleEntity(singles[currentIndex]);
                Db.Instance.insertOrModifySingleEntity(singles[lastIndex]);
                Level2EventItems(singlesColumnFlowLayoutPanel, singles);
            };
            var downButton = new Button();
            downButton.Text = "下移";
            downButton.MouseClick += (s, e) =>
            {
                var currentIndex = singles.IndexOf(single);
                var nextIndex = currentIndex + 1;
                if (currentIndex == singles.Count - 1)
                {
                    return;
                }
                var currentP = singles[currentIndex].Priority2;
                var nextP = singles[nextIndex].Priority2;
                singles[currentIndex].Priority2 = nextP;
                singles[nextIndex].Priority2 = currentP;
                Db.Instance.insertOrModifySingleEntity(singles[currentIndex]);
                Db.Instance.insertOrModifySingleEntity(singles[nextIndex]);
                Level2EventItems(singlesColumnFlowLayoutPanel, singles);
            };
            var removeButton = new Button();
            removeButton.Text = "移除";
            removeButton.ForeColor = Color.Red;
            removeButton.MouseClick += (s, e) =>
            {
                Db.Instance.GetCollection<Single>(Single.TABLE_NAME).Delete(new BsonValue(single.Id));
                Level2EventItems(singlesColumnFlowLayoutPanel, singles);
            };

            row1FlowLayoutPanel.Controls.Add(subOrderLabel);

            leftButtonFlowLayoutPanel.Controls.Add(upButton);
            leftButtonFlowLayoutPanel.Controls.Add(downButton);
            leftButtonFlowLayoutPanel.Controls.Add(removeButton);

            row2FlowLayoutPanel.Controls.Add(leftButtonFlowLayoutPanel);
            row2FlowLayoutPanel.Controls.Add(pictureBoxControl(single));
            row2FlowLayoutPanel.Controls.Add(paramControl(single));

            singlesColumnFlowLayoutPanel.Controls.Add(row1FlowLayoutPanel);
            singlesColumnFlowLayoutPanel.Controls.Add(row2FlowLayoutPanel);
            singlesColumnFlowLayoutPanel.Controls.Add(ControlTool.separator());
        });


    }

    // 截图
    private Control pictureBoxControl(Single item)
    {
        var column = new FlowLayoutPanel();
        column.Width = K.GET_DEFAULT_WIDTH();
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
                if (item is HomeForm || item == this)
                {
                    item.Visible = false;
                }
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
                if (item is HomeForm || item == this)
                {
                    item.Visible = true;
                }
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
                  $"已识别到并触发成功！\n区域:{result!.Value.Item2}\n循环序号:{result!.Value.Item1.Priority1}\n截图序号:{result!.Value.Item1.Priority2}");
            pictureBox.Visible = true;
        };

        column.Controls.Add(pictureBox);
        column.Controls.Add(testButton);
        return column;
    }
    // 参数
    private Control paramControl(Single item)
    {
        var columnFlowLayoutPanel = new FlowLayoutPanel();
        columnFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
        columnFlowLayoutPanel.AutoSize = true;


        var row0 = new FlowLayoutPanel();
        row0.AutoSize = true;
        row0.FlowDirection = FlowDirection.LeftToRight;
        var row0_1 = new Label() { Text = "点击左侧框进行截图，可再点击进行修改。", Parent = row0, AutoSize = true };

        // 每？秒在屏幕上检测一次与该截图相似度为？的地方，若检测到多处，则取用？作为触发位置。
        var row1 = new FlowLayoutPanel();
        row1.AutoSize = true;
        row1.FlowDirection = FlowDirection.LeftToRight;
        var row1_1 = new Label() { Text = "每", Parent = row1, AutoSize = true };
        var row1_2_Priority2CheckSecond = new TextBox() { Parent = row1, AutoSize = true, Width = 50 };
        row1_2_Priority2CheckSecond.Text = Db.Instance.GetSingleSth<int>(
            single: item,
            oldValueRead: oS => oS.Priority2CheckSecond,
            newValueRead: nS => nS?.Priority2CheckSecond,
            newValueToOldValue: (oS, n) => oS.Priority2CheckSecond = n,
            min: 1,
            max: null
        );
        row1_2_Priority2CheckSecond.LostFocus += (s, e) =>
        {
            row1_2_Priority2CheckSecond.Text = Db.Instance.SetSingleSth<int>(
                single: item,
                oldValueRead: oS => oS.Priority2CheckSecond,
                newValue: row1_2_Priority2CheckSecond.Text,
                newValueToOldValue: (oS, n) => oS.Priority2CheckSecond = n,
                1,
            max: null
            );
        };
        var row1_3 = new Label() { Text = "秒在屏幕上检测一次与该截图相似度为", Parent = row1, AutoSize = true };
        var row1_4_SimilarityThreshold = new TextBox() { Parent = row1, AutoSize = true, Width = 50 };
        row1_4_SimilarityThreshold.Text = Db.Instance.GetSingleSth<double>(
            single: item,
            oldValueRead: oS => oS.SimilarityThreshold,
            newValueRead: nS => nS?.SimilarityThreshold,
            newValueToOldValue: (oS, n) => oS.SimilarityThreshold = n,
            min: 0,
            max: 1
        );
        row1_4_SimilarityThreshold.LostFocus += (s, e) =>
        {
            row1_4_SimilarityThreshold.Text = Db.Instance.SetSingleSth<double>(
                single: item,
                oldValueRead: oS => oS.SimilarityThreshold,
                newValue: row1_4_SimilarityThreshold.Text,
                newValueToOldValue: (oS, n) => oS.SimilarityThreshold = n,
                0,
                max: 1
            );
        };

        var row1_5 = new Label() { Text = "的地方，若检测到多处，则取用", Parent = row1, AutoSize = true };
        var row1_6_PositionBlockType = new ComboBox() { Parent = row1 };
        row1_6_PositionBlockType.AutoSize = true;
        row1_6_PositionBlockType.Width = 200;
        row1_6_PositionBlockType.DropDownWidth = 400;
        row1_6_PositionBlockType.DropDownStyle = ComboBoxStyle.DropDownList;
        row1_6_PositionBlockType.MouseWheel += (s, e) => { ((HandledMouseEventArgs)e).Handled = true; };
        foreach (var t in Enum.GetValues(typeof(PositionBlockType)))
        {
            row1_6_PositionBlockType.Items.Add(Tool.Method.GetEnumDescription((PositionBlockType)t));
        }
        row1_6_PositionBlockType.SelectedIndex = int.Parse(Db.Instance.GetSingleSth<int>(
            single: item,
            oldValueRead: oS => (int?)oS.PositionBlockType,
            newValueRead: nS => (int?)nS?.PositionBlockType,
            newValueToOldValue: (oS, n) => oS.PositionBlockType = (PositionBlockType)n,
            min: 0,
            max: Enum.GetNames(typeof(PositionBlockType)).Length
            ));
        row1_6_PositionBlockType.SelectedIndexChanged += (s, e) =>
        {
            item.PositionBlockType = (PositionBlockType)int.Parse(Db.Instance.SetSingleSth<int>(
                single: item,
                oldValueRead: oS => (int?)oS.PositionBlockType,
                newValue: row1_6_PositionBlockType.SelectedIndex.ToString(),
                newValueToOldValue: (oS, n) => oS.PositionBlockType = (PositionBlockType)n,
                min: 0,
                max: Enum.GetNames(typeof(PositionBlockType)).Length
            ));
        };
        var row1_7 = new Label() { Text = "作为触发位置。", Parent = row1, AutoSize = true };

        // 如果检测成功，则对该区域每？毫秒进行一次？操作，连续？次，之后继续下一个循环。
        var row2 = new FlowLayoutPanel();
        row2.AutoSize = true;
        row2.FlowDirection = FlowDirection.LeftToRight;
        var row2_1 = new Label() { Text = "如果检测成功，则对该区域每", Parent = row2, AutoSize = true };
        var row2_1_1_EventMillisencondsOnce = new TextBox() { Parent = row2, Width = 50 };
        row2_1_1_EventMillisencondsOnce.Text = Db.Instance.GetSingleSth<int>(
            single: item,
            oldValueRead: oS => (int?)oS.EventMillisencondsOnce,
            newValueRead: nS => (int?)nS?.EventMillisencondsOnce,
            newValueToOldValue: (oS, n) => oS.EventMillisencondsOnce = n,
            min: 0,
            max: null
        );
        row2_1_1_EventMillisencondsOnce.LostFocus += (s, e) =>
        {
            row2_1_1_EventMillisencondsOnce.Text = Db.Instance.SetSingleSth<int>(
                single: item,
                oldValueRead: oS => oS.EventMillisencondsOnce,
                newValue: row2_1_1_EventMillisencondsOnce.Text,
                newValueToOldValue: (oS, n) => oS.EventMillisencondsOnce = n,
                min: 0,
                max: null
            );
        };
        var row2_1_2 = new Label() { Text = "毫秒进行一次", Parent = row2, AutoSize = true };
        var row2_2_EventKey = new ComboBox()
        {
            Parent = row2,
            AutoSize = true,
            Width = 100,
            DropDownWidth = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
        };
        row2_2_EventKey.MouseWheel += (s, e) => { ((HandledMouseEventArgs)e).Handled = true; };
        foreach (var i in Enum.GetValues(typeof(EventKey)))
        {
            row2_2_EventKey.Items.Add(Tool.Method.GetEnumDescription((EventKey)i));
        }
        row2_2_EventKey.SelectedIndex = int.Parse(Db.Instance.GetSingleSth<int>(
            single: item,
            oldValueRead: oS => (int?)oS.EventKey,
            newValueRead: nS => (int?)nS?.EventKey,
            newValueToOldValue: (oS, n) => oS.EventKey = (EventKey)n,
            min: 0,
            max: Enum.GetNames(typeof(EventKey)).Length
            ));
        row2_2_EventKey.SelectedIndexChanged += (s, e) =>
        {
            item.EventKey = (EventKey)int.Parse(Db.Instance.SetSingleSth<int>(
                single: item,
                oldValueRead: oS => (int?)oS.EventKey,
                newValue: row2_2_EventKey.SelectedIndex.ToString(),
                newValueToOldValue: (oS, n) => oS.EventKey = (EventKey)n,
                min: 0,
                max: Enum.GetNames(typeof(EventKey)).Length
            ));
        };

        var row2_2_1 = new Label() { Text = "操作，连续", Parent = row2, AutoSize = true };
        var row2_2_2_EventTriggerTimes = new TextBox() { Parent = row2, Width = 50 };
        row2_2_2_EventTriggerTimes.Text = Db.Instance.GetSingleSth<int>(
            single: item,
            oldValueRead: oS => (int?)oS.EventTriggerTimes,
            newValueRead: nS => (int?)nS?.EventTriggerTimes,
            newValueToOldValue: (oS, n) => oS.EventTriggerTimes = n,
            min: 1,
            max: null
        );
        row2_2_2_EventTriggerTimes.LostFocus += (s, e) =>
        {
            row2_2_2_EventTriggerTimes.Text = Db.Instance.SetSingleSth<int>(
                single: item,
                oldValueRead: oS => oS.EventTriggerTimes,
                newValue: row2_2_2_EventTriggerTimes.Text,
                newValueToOldValue: (oS, n) => oS.EventTriggerTimes = n,
                min: 1,
                max: null
            );
        };
        var row2_3 = new Label() { Text = "次，之后继续下一个循环。", Parent = row2, AutoSize = true };

        // 如果检测时间累计超过？秒仍然未检测到相似的地方，则跳转到主循环序号为？子循环序号为？的地方进行检测。
        var row3 = new FlowLayoutPanel();
        row3.AutoSize = true;
        row3.FlowDirection = FlowDirection.LeftToRight;
        var row3_1 = new Label() { Text = "如果检测时间累计超过", Parent = row3, AutoSize = true };
        var row3_2_Priority2TimeoutSecond = new TextBox() { Parent = row3, AutoSize = true, Width = 50 };
        row3_2_Priority2TimeoutSecond.Text = Db.Instance.GetSingleSth<int>(
            single: item,
            oldValueRead: oS => oS.Priority2TimeoutSecond,
            newValueRead: nS => nS?.Priority2TimeoutSecond,
            newValueToOldValue: (oS, n) => oS.Priority2TimeoutSecond = n,
            min: 0,
            max: null
        );
        row3_2_Priority2TimeoutSecond.LostFocus += (s, e) =>
        {
            row3_2_Priority2TimeoutSecond.Text = Db.Instance.SetSingleSth<int>(
                single: item,
                oldValueRead: oS => oS.Priority2TimeoutSecond,
                newValue: row3_2_Priority2TimeoutSecond.Text,
                newValueToOldValue: (oS, n) => oS.Priority2TimeoutSecond = n,
                min: 0,
                max: null
            );
        };
        var row3_3 = new Label() { Text = "秒仍然未检测到相似的地方，则停止。", Parent = row3, AutoSize = true };
        // var row3_3 = new Label() { Text = "秒仍然未检测到相似的地方，则跳转到 【循环序号为", Parent = row3, AutoSize = true };

        // int? defaultPriority1 = null;
        // int? defaultPriority2 = null;

        // Single? result = Db.Instance.GetCollection<Single>(Single.TABLE_NAME).FindById(new BsonValue(item.Priority2ToWhere ?? ObjectId.Empty));
        // defaultPriority1 = result?.Priority1;
        // defaultPriority2 = result?.Priority2;
        // row3_4.Text = defaultPriority1?.ToString();
        // row3_6.Text = defaultPriority2?.ToString();
        // int? parse(TextBox textBox)
        // {
        //     int? final;
        //     int finalNotNull;
        //     bool isSuccess = int.TryParse(textBox.Text, out finalNotNull);
        //     if (!isSuccess)
        //     {
        //         finalNotNull = -1;
        //     }
        //     if (finalNotNull < 0)
        //     {
        //         finalNotNull = -1;
        //     }
        //     if (finalNotNull == -1)
        //     {
        //         final = null;
        //     }
        //     else
        //     {
        //         final = finalNotNull;
        //     }
        //     return final;
        // }
        // void lostFocus()
        // {
        //     int? vp1 = parse(row3_4);
        //     int? vp2 = parse(row3_6);

        //     var query = from innerList in groupSingleList
        //                 from currentSingle in innerList
        //                 where currentSingle.Priority1 == vp1 && currentSingle.Priority2 == vp2
        //                 select new
        //                 {
        //                     currentSingle
        //                 };
        //     Single? toWhere = query.FirstOrDefault()?.currentSingle;
        //     item.Priority2ToWhere = toWhere?.Id;
        //     Db.Instance.insertOrModifySingleEntity(item);

        //     row3_4.Text = vp1?.ToString() ?? "";
        //     row3_6.Text = vp2?.ToString() ?? "";
        // }
        // row3_4.LostFocus += (s, e) =>
        // {
        //     lostFocus();
        // };

        // row3_6.LostFocus += (s, e) =>
        // {
        //     lostFocus();
        // };


        var row4 = new FlowLayoutPanel();
        row4.AutoSize = true;
        row4.FlowDirection = FlowDirection.LeftToRight;
        var copyData = new Button() { Parent = row4, AutoSize = true, Text = "复制当前截图序号数据（不复制截图）" };
        copyData.MouseClick += (s, e) =>
        {
            copiedSingle = item;
        };

        var paste = new Button() { Parent = row4, AutoSize = true, Text = "粘贴数据" };
        paste.MouseClick += (s, e) =>
        {
            if (copiedSingle != null)
            {
                row2_2_EventKey.SelectedIndex = (int)copiedSingle.EventKey!;
                row1_6_PositionBlockType.SelectedIndex = (int)copiedSingle.PositionBlockType!;
                row2_1_1_EventMillisencondsOnce.Text = copiedSingle.EventMillisencondsOnce?.ToString();
                row2_2_2_EventTriggerTimes.Text = copiedSingle.EventTriggerTimes?.ToString();
                row1_2_Priority2CheckSecond.Text = copiedSingle.Priority2CheckSecond?.ToString();
                row3_2_Priority2TimeoutSecond.Text = copiedSingle.Priority2TimeoutSecond?.ToString();
                // item.Priority2ToWhere = copiedSingle.Priority2ToWhere;
                Db.Instance.insertOrModifySingleEntity(item);
                // void UpdateAllControls(Control control)
                // {
                //     foreach (Control c in control.Controls)
                //     {
                //         // 更新当前控件
                //         Console.WriteLine("zzzzzzzz");
                //         c.PerformLayout();

                //         // 如果当前控件还有子控件，则递归更新子控件
                //         if (c.Controls.Count > 0)
                //         {
                //             UpdateAllControls(c);
                //         }
                //     }
                // }
                // UpdateAllControls(columnFlowLayoutPanel);
            }
        };


        columnFlowLayoutPanel.Controls.Add(row0);
        columnFlowLayoutPanel.Controls.Add(row1);
        columnFlowLayoutPanel.Controls.Add(row2);
        columnFlowLayoutPanel.Controls.Add(row3);
        columnFlowLayoutPanel.Controls.Add(row4);
        // columnFlowLayoutPanel.Controls.Add(row2FlowLayoutPanel);

        return columnFlowLayoutPanel;
    }

}