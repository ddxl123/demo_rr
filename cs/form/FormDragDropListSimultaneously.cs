using System.Runtime.InteropServices;
using LiteDB;
using Tool;
using System.ComponentModel;

public class FormDragDropListSimultaneously : Form
{
    private List<Single> singleList = new List<Single>();

    private Point dragStartPoint;

    public FlowLayoutPanel mainFlowLayoutPanel = new FlowLayoutPanel();

    public FlowLayoutPanel bodyFlowLayoutPanel = Global.bodyFlowLayoutPanelSimultaneously;

    public FlowLayoutPanel eventFlowLayoutPanel = new FlowLayoutPanel();

    public FormDragDropListSimultaneously()
    {
        this.AutoSize = true;
        mainFlowLayoutPanel.AutoSize = true;
        mainFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;

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

        bodyFlowLayoutPanel.Controls.Add(addEventButtonControl());
        bodyFlowLayoutPanel.Controls.Add(eventFlowLayoutPanel);
        mainFlowLayoutPanel.Controls.Add(startOrCancelControl());
        mainFlowLayoutPanel.Controls.Add(bodyFlowLayoutPanel);
        this.Controls.Add(mainFlowLayoutPanel);
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
            // 创建一个包含两个子控件的元素
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
                Global.timer.Stop();
                bodyFlowLayoutPanel.Visible = true;
                Console.WriteLine("暂停成功");
            }
            else
            {
                Global.startingEventMode = EventMode.Simultaneously;
                button.Text = "暂停";
                Db.Instance.setLoopTime(Db.Instance.GetValueByKey(Db.LOOP_EVENT_TIME_CYCLE_SIMULTANEOUSLY), EventMode.Simultaneously);
                Global.timer.Start();
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
                    Global.bodyFlowLayoutPanelSimultaneously.Visible = true;
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
            Db.Instance.insertOrModifySingleEntity(item);
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
            Db.Instance.insertOrModifySingleEntity(item);
        };

        positionBlockTypeRowPanel.Controls.Add(positionBlockTypeLabel);
        positionBlockTypeRowPanel.Controls.Add(positionBlockTypeBox);

        paramPanel.Controls.Add(similarityThresholdRowPanel);
        paramPanel.Controls.Add(positionBlockTypeRowPanel);
        paramPanel.Controls.Add(explainControl(item));

        return paramPanel;
    }

    // 添加截图按钮
    private Control addEventButtonControl()
    {
        var addEventPanel = new FlowLayoutPanel();
        addEventPanel.FlowDirection = FlowDirection.TopDown;
        addEventPanel.AutoSize = true;
        Button addButton = new Button();
        addButton.Text = "➕ 增加截图";
        addButton.Scale(new SizeF(2, 2));
        addButton.MouseClick += (s, e) =>
        {
            var maxSingleResult = Db.Instance.GetCollection<Single>(Single.TABLE_NAME)
            .Find(Db.Instance.eventModeQuery(EventMode.Simultaneously)).OrderByDescending((e) => e.Priority1).FirstOrDefault();
            if (maxSingleResult == null)
            {
                Db.Instance.insertOrModifySingleEntity(new Single(0, null, null, null, EventMode.Simultaneously) { Id = null });
            }
            else
            {
                Db.Instance.insertOrModifySingleEntity(new Single(maxSingleResult.Priority1 + 1, null, null, null, EventMode.Simultaneously) { Id = null });
            }
            ReShowItems();
        };

        var text = new Label();
        text.AutoSize = true;
        text.Text = "每次触发循环事件，都会在屏幕上检测下面列表中的全部截图，并会触发其中的一个点击事件一次。"
        + "\n会按照优先级从高到低进行检测，优先级高的将被触发点击，后面的截图不会继续触发点击。";

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

        timeBox.Text = Db.Instance.GetValueByKey(Db.LOOP_EVENT_TIME_CYCLE_SIMULTANEOUSLY);
        Db.Instance.setLoopTime(Db.Instance.GetValueByKey(Db.LOOP_EVENT_TIME_CYCLE_SIMULTANEOUSLY), EventMode.Simultaneously);
        timeBox.Leave += (s, e) =>
        {
            timeBox.Text = Db.Instance.setLoopTime(timeBox.Text, EventMode.Simultaneously).ToString();
        };
        loopTimePanel.Controls.Add(timeLabel1);
        loopTimePanel.Controls.Add(timeBox);
        loopTimePanel.Controls.Add(timeLabel2);

        addEventPanel.Controls.Add(text);
        addEventPanel.Controls.Add(loopTimePanel);
        addEventPanel.Controls.Add(addButton);
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
                int tempPriority = singleList[current].Priority1;
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
                int tempPriority = singleList[current].Priority1;
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

            Db.Instance.insertOrModifySingleEntity(item);
            ReShowItems();
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
            ReShowItems();
        };
        return removeButton;
    }
}
