
using System.Numerics;
using System.Timers;
using Tool;

public class HomeForm : Form
{


    public HomeForm()
    {
        WindowHideIcon();
        Global.init(this);


        Text = "自动点击工具";
        Width = K.GET_DEFAULT_WIDTH() + 100;
        Height = K.GET_DEFAULT_HEIGHT();


        TabControl tabControl = new TabControl();
        tabControl.Dock = DockStyle.Fill;

        TabPage tabPage1 = new TabPage("同时检测模式");
        TabPage tabPage2 = new TabPage("顺序检测模式");

        Global.formDragDropListSimultaneously.FormBorderStyle = FormBorderStyle.None;
        Global.formDragDropListSimultaneously.TopLevel = false;
        Global.formDragDropListSimultaneously.Show();

        Global.formDragDropListInOrder.FormBorderStyle = FormBorderStyle.None;
        Global.formDragDropListInOrder.TopLevel = false;
        Global.formDragDropListInOrder.Show();

        tabPage1.Controls.Add(Global.formDragDropListSimultaneously);
        tabPage2.Controls.Add(Global.formDragDropListInOrder);
        tabPage2.Dock = DockStyle.Fill;
        tabControl.TabPages.Add(tabPage1);
        tabControl.TabPages.Add(tabPage2);
        tabControl.Dock = DockStyle.Fill;

        this.Controls.Add(tabControl);
    }



    // 最小化托盘
    private void WindowHideIcon()
    {
        NotifyIcon notifyIcon = new NotifyIcon();

        // 创建NotifyIcon对象
        notifyIcon.Text = "应用程序名称";
        notifyIcon.Icon = new Icon(K.ICON);
        notifyIcon.Visible = true;

        // 订阅双击事件，用于还原窗口
        notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        notifyIcon.ContextMenuStrip = new ContextMenuStrip();
        notifyIcon.ContextMenuStrip.Items.Add("退出", null, (s, e) => { Application.Exit(); });

        // 初始时展示窗口
        this.WindowState = FormWindowState.Normal;
        this.Show();
    }



    // 双击托盘图标
    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        // 显示或隐藏窗体
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
        else
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }
    }

    // 窗口关闭事件处理方法
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            base.OnFormClosing(e);
        }
    }

}