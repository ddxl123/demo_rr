using System.Numerics;
using System.Timers;
using System.Windows;
using Tool;

public static class Global
{

    public static FlowLayoutPanel bodyFlowLayoutPanelSimultaneously = new FlowLayoutPanel();

    public static FlowLayoutPanel bodyFlowLayoutPanelInOrder = new FlowLayoutPanel();

    public static Button buttonSimultaneously = new Button();
    public static Button buttonInOrder = new Button();
    public static EventMode startingEventMode = EventMode.None;

    public static System.Timers.Timer simultaneouslyTimer = new System.Timers.Timer();

    // 当前执行到哪了
    // public static Single? inOrderTimerSingle = null;


    public static FormDragDropListSimultaneously formDragDropListSimultaneously = new FormDragDropListSimultaneously();

    public static FormDragDropListInOrder formDragDropListInOrder = new FormDragDropListInOrder();



    static Global()
    {
        buttonSimultaneously.Text = "";
    }
    public static void init(HomeForm homeForm)
    {
        deleteSingleOnClose(homeForm);

        // 触发内容只赋值一次
        Global.simultaneouslyTimer.AutoReset = true;
        Global.simultaneouslyTimer.Elapsed += TimerElapser.simultaneouslyTimerElapsed;
    }

    // 退出应用时删除不需要的文件
    private static void deleteSingleOnClose(HomeForm homeForm)
    {
        homeForm.FormClosing += (s, e) =>
        {
            string folderPath = K.ASSETS_SINGLE;
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                var result = Db.Instance.GetCollection<Single>(Single.TABLE_NAME).FindAll();
                if (!result.Select((e) => { return e.ImagePath; }).Contains(Path.GetFileName(file)))
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"已删除文件: {file}");
                    }
                    catch (System.Exception)
                    {
                        Console.WriteLine($"文件已被锁定: {file}");
                    }
                }
            }
            Console.WriteLine("closed");
        };
    }





}