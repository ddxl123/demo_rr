using System.Runtime.InteropServices;
using LiteDB;
using Tool;
using System.ComponentModel;

public class FormDragDropListInOrder : Form
{
    private List<List<Single>> groupSingleList = new List<List<Single>>();

    private Point dragStartPoint;

    public FlowLayoutPanel mainFlowLayoutPanel = new FlowLayoutPanel();

    public FlowLayoutPanel bodyFlowLayoutPanel = Global.bodyFlowLayoutPanelInOrder;

    public FlowLayoutPanel allEventFlowLayoutPanel = new FlowLayoutPanel();

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
        allEventFlowLayoutPanel.BorderStyle = BorderStyle.None;
        allEventFlowLayoutPanel.AutoScroll = true;
        allEventFlowLayoutPanel.Size = new Size(K.GET_DEFAULT_WIDTH(), K.GET_DEFAULT_HEIGHT() * 2 / 3);

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

        mainFlowLayoutPanel.Controls.Add(startOrCancelControl());
        mainFlowLayoutPanel.Controls.Add(bodyFlowLayoutPanel);
        this.Controls.Add(mainFlowLayoutPanel);
    }


    private Control addLoopEventButtonControl()
    {
        Button addButton = new Button();
        addButton.Text = "➕ 增加循环";
        addButton.Scale(new SizeF(2, 2));
        addButton.MouseClick += (s, e) =>
        {
            var maxSingleResult = LiteDBSingleton.Instance.GetCollection<Single>(Single.TABLE_NAME)
            .Find(LiteDBSingleton.Instance.eventModeQuery(EventMode.InOrder)).OrderByDescending((e) => e.Priority1).FirstOrDefault();

            if (maxSingleResult == null)
            {
                LiteDBSingleton.Instance.insertOrModifySingleEntity(new Single(0, 0, null, null, EventMode.InOrder) { Id = null });
            }
            else
            {
                LiteDBSingleton.Instance.insertOrModifySingleEntity(new Single(maxSingleResult.Priority1 + 1, 0, null, null, EventMode.InOrder) { Id = null });
            }
            Level1EventItems();
        };

        return addButton;
    }

    private void Level1EventItems()
    {
        // 重新向数据库中获取。
        var result = LiteDBSingleton.Instance.GetCollection<Single>(Single.TABLE_NAME).Find(LiteDBSingleton.Instance.eventModeQuery(EventMode.InOrder));
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

            // groupEventFlowLayoutPanel.AutoSize = true;
            groupEventFlowLayoutPanel.WrapContents = false;
            groupEventFlowLayoutPanel.AutoScroll = true;
            groupEventFlowLayoutPanel.VerticalScroll.Visible = true;
            groupEventFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            groupEventFlowLayoutPanel.BorderStyle = BorderStyle.None;
            groupEventFlowLayoutPanel.Width = K.GET_DEFAULT_WIDTH() - 100;
            groupEventFlowLayoutPanel.Height = 200;

            var row1FlowLayoutPanel = new FlowLayoutPanel();
            row1FlowLayoutPanel.AutoSize = true;
            row1FlowLayoutPanel.FlowDirection = FlowDirection.LeftToRight;

            var row2FlowLayoutPanel = new FlowLayoutPanel();
            row2FlowLayoutPanel.AutoSize = true;
            row2FlowLayoutPanel.FlowDirection = FlowDirection.LeftToRight;

            var p = new Label();
            p.Text = $"顺序：{groupSingle.First().Priority1}";

            var singlesColumnFlowLayoutPanel = new FlowLayoutPanel();
            singlesColumnFlowLayoutPanel.AutoSize = true;
            singlesColumnFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            singlesColumnFlowLayoutPanel.VerticalScroll.Visible = true;
            singlesColumnFlowLayoutPanel.AutoScroll = true;
            singlesColumnFlowLayoutPanel.BorderStyle = BorderStyle.FixedSingle;

            // 第一行 row
            var addLoopButton = new Button();
            addLoopButton.AutoSize = true;
            addLoopButton.Text = "➕ 添加截图";
            addLoopButton.MouseClick += (s, e) =>
            {
                var newSingle = new Single(groupSingle.First().Priority1, groupSingle.OrderByDescending(o => o.Priority2).First().Priority2 + 1, null, null, EventMode.InOrder);
                LiteDBSingleton.Instance.insertOrModifySingleEntity(newSingle);
                Level2EventItems(singlesColumnFlowLayoutPanel, groupSingle);
            };
            var text = new Label() { Text = "循环次数：" };
            var timesButton = new TextBox();

            // 第二行 row
            var upButton = new Button();
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
                    LiteDBSingleton.Instance.insertOrModifySingleEntity(c);
                });
                groupSingleList[currentIndex].ForEach(c =>
                {
                    c.Priority1 = lastP;
                    LiteDBSingleton.Instance.insertOrModifySingleEntity(c);
                });
                Level1EventItems();
            };
            var downButton = new Button();
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
                    LiteDBSingleton.Instance.insertOrModifySingleEntity(c);
                });
                groupSingleList[currentIndex].ForEach(c =>
                {
                    c.Priority1 = nextP;
                    LiteDBSingleton.Instance.insertOrModifySingleEntity(c);
                });
                Level1EventItems();
            };
            var removeButton = new Button();
            removeButton.Text = "移除";
            removeButton.ForeColor = Color.Red;
            removeButton.MouseClick += (s, ev) =>
            {
                groupSingle.ForEach(si =>
                {
                    LiteDBSingleton.Instance.GetCollection<Single>(Single.TABLE_NAME).Delete(si.Id);
                });
                Level1EventItems();
            };

            // singlesFlowLayoutPanel.Size = new Size(K.GET_DEFAULT_WIDTH(),200);
            Level2EventItems(singlesColumnFlowLayoutPanel, groupSingle);

            row1FlowLayoutPanel.Controls.Add(p);
            row1FlowLayoutPanel.Controls.Add(addLoopButton);
            row1FlowLayoutPanel.Controls.Add(text);
            row1FlowLayoutPanel.Controls.Add(timesButton);
            row1FlowLayoutPanel.Controls.Add(upButton);
            row1FlowLayoutPanel.Controls.Add(downButton);
            row1FlowLayoutPanel.Controls.Add(removeButton);

            row2FlowLayoutPanel.Controls.Add(singlesColumnFlowLayoutPanel);

            groupEventFlowLayoutPanel.Controls.Add(row1FlowLayoutPanel);
            groupEventFlowLayoutPanel.Controls.Add(row2FlowLayoutPanel);

            allEventFlowLayoutPanel.Controls.Add(groupEventFlowLayoutPanel);
            allEventFlowLayoutPanel.Controls.Add(new Panel() { Height = 20 });
        });
    }

    private void Level2EventItems(FlowLayoutPanel singlesColumnFlowLayoutPanel, List<Single> singles)
    {

        var q = Query.And(LiteDBSingleton.Instance.eventModeQuery(EventMode.InOrder), Query.EQ("Priority1", singles.First().Priority1));
        var result = LiteDBSingleton.Instance.GetCollection<Single>(Single.TABLE_NAME).Find(q).OrderByDescending(e => e.Priority2).ToList();
        singlesColumnFlowLayoutPanel.Controls.Clear();
        singles.Clear();
        singles.AddRange(result);

        singles.ForEach((single) =>
        {
            var subOrderLabel = new Label();
            subOrderLabel.Text = $"子顺序：{single.Priority2}";

            var rowFlowLayoutPanel = new FlowLayoutPanel();
            rowFlowLayoutPanel.AutoSize = true;
            rowFlowLayoutPanel.FlowDirection = FlowDirection.LeftToRight;

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
                LiteDBSingleton.Instance.insertOrModifySingleEntity(singles[currentIndex]);
                LiteDBSingleton.Instance.insertOrModifySingleEntity(singles[lastIndex]);
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
                LiteDBSingleton.Instance.insertOrModifySingleEntity(singles[currentIndex]);
                LiteDBSingleton.Instance.insertOrModifySingleEntity(singles[nextIndex]);
                Level2EventItems(singlesColumnFlowLayoutPanel, singles);
            };
            var removeButton = new Button();
            removeButton.Text = "移除";
            removeButton.ForeColor = Color.Red;
            removeButton.MouseClick += (s, e) =>
            {
                LiteDBSingleton.Instance.GetCollection<Single>(Single.TABLE_NAME).Delete(new BsonValue(single.Id));
                Level2EventItems(singlesColumnFlowLayoutPanel, singles);
            };

            leftButtonFlowLayoutPanel.Controls.Add(upButton);
            leftButtonFlowLayoutPanel.Controls.Add(downButton);
            leftButtonFlowLayoutPanel.Controls.Add(removeButton);

            rowFlowLayoutPanel.Controls.Add(leftButtonFlowLayoutPanel);
            rowFlowLayoutPanel.Controls.Add(pictureBoxControl(single));
            rowFlowLayoutPanel.Controls.Add(paramControl(single));

            singlesColumnFlowLayoutPanel.Controls.Add(subOrderLabel);
            singlesColumnFlowLayoutPanel.Controls.Add(rowFlowLayoutPanel);
            singlesColumnFlowLayoutPanel.Controls.Add(ControlTool.separator());
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

    private Control startOrCancelControl()
    {
        Button button = Global.buttonInOrder;

        if (Global.startingEventMode == EventMode.InOrder)
        {
            button.Text = "暂停";
        }
        else
        {
            button.Text = "启动";
        }
        button.Click += (s, e) =>
        {
            if (Global.startingEventMode == EventMode.InOrder)
            {
                Global.startingEventMode = EventMode.None;
                button.Text = "启动";
                Global.timer.Stop();
                bodyFlowLayoutPanel.Visible = true;
                Console.WriteLine("暂停成功");
            }
            else
            {
                Global.startingEventMode = EventMode.InOrder;
                button.Text = "暂停";
                LiteDBSingleton.Instance.setLoopTime(LiteDBSingleton.Instance.GetValueByKey(K.LOOP_EVENT_TIME_CYCLE_IN_ORDER), EventMode.InOrder);
                Global.timer.Start();
                bodyFlowLayoutPanel.Visible = false;
                Console.WriteLine("启动成功");

                if (button == Global.buttonSimultaneously)
                {
                    Global.buttonInOrder.Text = "启动";
                    Global.bodyFlowLayoutPanelSimultaneously.Visible = true;
                }
                else if (button == Global.buttonInOrder)
                {
                    Global.buttonSimultaneously.Text = "启动";
                    Global.bodyFlowLayoutPanelInOrder.Visible = true;
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
        var paramPanel = new FlowLayoutPanel();
        paramPanel.FlowDirection = FlowDirection.TopDown;
        paramPanel.AutoSize = true;

        var similarityThresholdRowPanel = new FlowLayoutPanel();
        similarityThresholdRowPanel.FlowDirection = FlowDirection.LeftToRight;
        similarityThresholdRowPanel.AutoSize = true;
        var similarityThresholdLabel = new Label();
        similarityThresholdLabel.AutoSize = true;
        similarityThresholdLabel.Text = "相似阈值：";
        var similarityThreshold = new TextBox();
        similarityThreshold.AutoSize = true;
        similarityThreshold.Text = item.SimilarityThreshold.ToString();
        similarityThreshold.Leave += (s, e) =>
        {
            double v;
            bool isTry = double.TryParse(similarityThreshold.Text, out v);
            if (v < 0)
            {
                v = 0;
            }
            else if (v > 1)
            {
                v = 1.0;
            }
            similarityThreshold.Text = v.ToString();
            item.SimilarityThreshold = v;
            LiteDBSingleton.Instance.insertOrModifySingleEntity(item);
        };
        ToolTip toolTip1 = new ToolTip();
        toolTip1.IsBalloon = true;
        toolTip1.ShowAlways = true;

        toolTip1.SetToolTip(similarityThresholdRowPanel, "范围：0~1");
        similarityThresholdRowPanel.Controls.Add(similarityThresholdLabel);
        similarityThresholdRowPanel.Controls.Add(similarityThreshold);



        var positionBlockTypeRowPanel = new FlowLayoutPanel();
        positionBlockTypeRowPanel.FlowDirection = FlowDirection.LeftToRight;
        positionBlockTypeRowPanel.AutoSize = true;
        var positionBlockTypeLabel = new Label();
        positionBlockTypeLabel.AutoSize = true;
        positionBlockTypeLabel.Text = "识别区位：";
        var positionBlockTypeBox = new ComboBox();
        positionBlockTypeBox.AutoSize = true;
        positionBlockTypeBox.Width = 300;
        positionBlockTypeBox.DropDownWidth = 300;
        positionBlockTypeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        positionBlockTypeBox.MouseWheel += (s, e) => { ((HandledMouseEventArgs)e).Handled = true; };
        foreach (var t in Tool.Method.GetEnumMembers<PositionBlockType>())
        {
            positionBlockTypeBox.Items.Add(Tool.Method.GetEnumDescription(t));
        }
        positionBlockTypeBox.SelectedIndex = Tool.Method.GetEnumIndex<PositionBlockType>(item.PositionBlockType);
        positionBlockTypeBox.SelectedIndexChanged += (s, e) =>
        {
            item.PositionBlockType = Tool.Method.GetEnumMembers<PositionBlockType>()[positionBlockTypeBox.SelectedIndex];
            LiteDBSingleton.Instance.insertOrModifySingleEntity(item);
        };

        positionBlockTypeRowPanel.Controls.Add(positionBlockTypeLabel);
        positionBlockTypeRowPanel.Controls.Add(positionBlockTypeBox);

        paramPanel.Controls.Add(similarityThresholdRowPanel);
        paramPanel.Controls.Add(positionBlockTypeRowPanel);
        paramPanel.Controls.Add(explainControl(item));

        return paramPanel;
    }


    private Control mainExplainControl()
    {
        var addEventPanel = new FlowLayoutPanel();
        addEventPanel.FlowDirection = FlowDirection.TopDown;
        addEventPanel.AutoSize = true;
        var text = new Label();
        text.AutoSize = true;
        text.Text = "会按照顺序依次进行点击操作";

        var loopTimePanel = new FlowLayoutPanel();
        loopTimePanel.FlowDirection = FlowDirection.LeftToRight;
        loopTimePanel.AutoSize = true;
        var timeLabel1 = new Label();
        timeLabel1.Text = "每";
        timeLabel1.AutoSize = true;
        var timeLabel2 = new Label();
        timeLabel2.Text = "秒触发一次循环事件";
        timeLabel2.AutoSize = true;
        var timeBox = new TextBox();

        timeBox.Text = LiteDBSingleton.Instance.GetValueByKey(K.LOOP_EVENT_TIME_CYCLE_IN_ORDER);
        LiteDBSingleton.Instance.setLoopTime(LiteDBSingleton.Instance.GetValueByKey(K.LOOP_EVENT_TIME_CYCLE_IN_ORDER), EventMode.InOrder);
        timeBox.Leave += (s, e) =>
        {
            timeBox.Text = LiteDBSingleton.Instance.setLoopTime(timeBox.Text, EventMode.InOrder).ToString();
        };
        loopTimePanel.Controls.Add(timeLabel1);
        loopTimePanel.Controls.Add(timeBox);
        loopTimePanel.Controls.Add(timeLabel2);

        addEventPanel.Controls.Add(text);
        addEventPanel.Controls.Add(loopTimePanel);
        return addEventPanel;
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

    // 截图
    private Control pictureBoxControl(Single item)
    {
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
            item.ImagePath = $"{K.ASSETS_SINGLE}/{Guid.NewGuid().ToString()}.png";
            // 截图
            new ScreenRegionSelector(item.ImagePath).ShowDialog();

            // 显示全部窗体
            foreach (Form item in Application.OpenForms)
            {
                item.Visible = true;
            }

            LiteDBSingleton.Instance.insertOrModifySingleEntity(item);
            pictureBox.Image = Image.FromFile(item.ImagePath);
        };
        return pictureBox;
    }

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
            LiteDBSingleton.Instance.insertOrModifySingleEntity(item);
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
            LiteDBSingleton.Instance.GetCollection<Single>(Single.TABLE_NAME).Delete(item.Id);
            // ReShowItems();
        };
        return removeButton;
    }


}
