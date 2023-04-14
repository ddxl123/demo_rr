using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using LiteDB;
using Tool;

class Program
{

    const String FULL_SCREAN_FILE_NAME = "full_screan.png";

    [STAThread]
    static void Main(string[] args)
    {


        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // 创建窗体实例 
        Application.Run(new HomeForm());

        // // 保存图像
        // screenShot.Save(FULL_SCREAN_FILE_NAME, ImageFormat.Png);

        // Console.WriteLine("Screen captured and saved to screenshot.png");

        // var result = ImageMatcher.MatchImage("2.png", FULL_SCREAN_FILE_NAME);

        // // 创建一个Timer对象
        // System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        // timer.Interval = 1000; // 设置定时器间隔时间，单位为毫秒
        // timer.Tick += Timer_Tick; // 订阅定时器触发事件

        // // 启动定时器
        // timer.Start();
        // if (result.Count > 0)
        // {
        //     Console.WriteLine(result.Count);
        //     foreach (var item in result)
        //     {
        //         Console.WriteLine("图像A在图像B中找到。");
        //         Rectangle matchRect = item;
        //         new Clicker(matchRect).Click();
        //         Thread.Sleep(1000);
        //     }

        // }
        // else
        // {
        //     Console.WriteLine("图像A未在图像B中找到。");
        //     ShowNotify();
        // }
    }

    public static void ShowNotify()
    {

        // 创建一个NotifyIcon对象
        NotifyIcon notifyIcon = new NotifyIcon();
        notifyIcon.Icon = SystemIcons.Information;
        notifyIcon.Visible = true;

        // 设置BalloonTip内容和标题
        notifyIcon.BalloonTipText = "无法识别到游戏窗口，请将游戏窗口置于前台！";

        // 显示BalloonTip
        notifyIcon.ShowBalloonTip(3000);
    }

    private static void Timer_Tick(object? sender, EventArgs e)
    {
        // ShowNotify();
        Console.WriteLine("定时器触发了：" + DateTime.Now.ToString());
    }

}