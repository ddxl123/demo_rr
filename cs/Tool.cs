using System.ComponentModel;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Tool
{

    static public class K
    {

        public static string ASSETS = "assets";
        public static string ASSETS_SINGLE = $"{ASSETS}/single";
        public static string ICON = $"{ASSETS}/icon.ico";

        public static int GET_DEFAULT_HEIGHT() => 800;
        public static int GET_DEFAULT_WIDTH() => Screen.GetBounds(Point.Empty).Width * 2 / 3;
    }

    // 截屏
    public class Capturer
    {
        // 截取屏幕区域，并保存图像。
        // 若 [rectangle] 为空，则截取全屏。
        public static void CaptureFullScreen(string imagePath, Rectangle? rectangle)
        {
            Rectangle bounds = rectangle ?? Screen.GetBounds(Point.Empty);
            Console.WriteLine(bounds);
            IntPtr desktopDC = GetDC(IntPtr.Zero);
            IntPtr memoryDC = CreateCompatibleDC(desktopDC);
            IntPtr bitmap = CreateCompatibleBitmap(desktopDC, bounds.Width, bounds.Height);
            IntPtr oldBitmap = SelectObject(memoryDC, bitmap);

            // 拷贝屏幕内容到位图
            BitBlt(memoryDC, 0, 0, bounds.Width, bounds.Height, desktopDC, bounds.X, bounds.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

            SelectObject(memoryDC, oldBitmap);
            DeleteDC(memoryDC);
            ReleaseDC(IntPtr.Zero, desktopDC);



            // 使用 using 释放位图句柄
            using (Bitmap bmp = Bitmap.FromHbitmap(bitmap))
            {
                //保存图像
                bmp.Save(imagePath, ImageFormat.Png);
                Console.WriteLine("Screen captured and saved to " + imagePath);
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hdc);
    }


    // 自动点击
    public class Clicker
    {

        //导入user32.dll中的SendInput函数，用于模拟鼠标输入
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        //导入user32.dll中的SetCursorPos函数，用于设置鼠标光标位置
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        //导入user32.dll中的GetCursorPos函数，用于获取鼠标光标位置
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out Point lpPoint);

        //定义一个结构体，表示输入事件的类型和数据
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public SendInputEventType type; //输入事件的类型，可以是鼠标、键盘或硬件
            public MouseKeybdhardwareInputUnion mkhi; //输入事件的数据，根据类型不同而不同
            internal static int Size //输入事件的大小，用于传递给SendInput函数
            {
                get { return Marshal.SizeOf(typeof(INPUT)); }
            }
        }

        //定义一个联合体，表示鼠标、键盘或硬件输入事件的数据
        [StructLayout(LayoutKind.Explicit)]
        struct MouseKeybdhardwareInputUnion
        {
            [FieldOffset(0)]
            public MouseInputData mi; //鼠标输入事件的数据

            [FieldOffset(0)]
            public KEYBDINPUT ki; //键盘输入事件的数据

            [FieldOffset(0)]
            public HARDWAREINPUT hi; //硬件输入事件的数据
        }

        //定义一个结构体，表示键盘输入事件的数据
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk; //虚拟键码
            public ushort wScan; //扫描码
            public uint dwFlags; //标志位，表示按下或释放等状态
            public uint time; //时间戳，如果为0则系统自动提供
            public IntPtr dwExtraInfo; //额外信息，一般为0
        }

        //定义一个结构体，表示硬件输入事件的数据
        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg; //消息码
            public short wParamL; //低位参数值
            public short wParamH; //高位参数值
        }

        //定义一个结构体，表示鼠标输入事件的数据
        struct MouseInputData
        {
            public int dx; //鼠标相对或绝对水平位置（取决于MOUSEEVENTF_ABSOLUTE标志）
            public int dy; //鼠标相对或绝对垂直位置（取决于MOUSEEVENTF_ABSOLUTE标志）
            public uint mouseData; //鼠标滚轮或X按钮的数据（取决于dwFlags）
            public MouseEventFlags dwFlags; //鼠标事件的标志位，表示移动、点击、滚动等操作
            public uint time; //时间戳，如果为0则系统自动提供
            public IntPtr dwExtraInfo; //额外信息，一般为0
        }

        //定义一个枚举，表示鼠标事件的标志位
        [Flags]
        enum MouseEventFlags : uint
        {
            MOUSEEVENTF_MOVE = 0x0001, //鼠标移动
            MOUSEEVENTF_LEFTDOWN = 0x0002, //鼠标左键按下
            MOUSEEVENTF_LEFTUP = 0x0004, //鼠标左键释放
            MOUSEEVENTF_RIGHTDOWN = 0x0008, //鼠标右键按下
            MOUSEEVENTF_RIGHTUP = 0x0010, //鼠标右键释放
            MOUSEEVENTF_MIDDLEDOWN = 0x0020, //鼠标中键按下
            MOUSEEVENTF_MIDDLEUP = 0x0040, //鼠标中键释放
            MOUSEEVENTF_XDOWN = 0x0080, //鼠标X按钮按下
            MOUSEEVENTF_XUP = 0x0100, //鼠标X按钮释放
            MOUSEEVENTF_WHEEL = 0x0800, //鼠标滚轮滚动
            MOUSEEVENTF_VIRTUALDESK = 0x4000, //使用虚拟桌面的坐标
            MOUSEEVENTF_ABSOLUTE = 0x8000 //使用绝对坐标（相对于屏幕）
        }

        //定义一个枚举，表示输入事件的类型
        enum SendInputEventType : int
        {
            InputMouse, //鼠标输入事件
            InputKeyboard, //键盘输入事件
            InputHardware //硬件输入事件
        }

        //定义一个公共静态方法，用于模拟鼠标左键单击指定区域的中心位置
        public static void ClickLeftMouseButton(Rectangle rect)
        {
            INPUT mouseInput = new INPUT(); //创建一个INPUT结构体实例，用于存储鼠标输入事件的数据
            mouseInput.type = SendInputEventType.InputMouse; //设置输入事件的类型为鼠标输入事件

            mouseInput.mkhi.mi.dx = (rect.X + rect.Width / 2) * 65536 / Screen.PrimaryScreen!.Bounds.Width; ; //设置鼠标水平位置为区域的中心位置（如果使用绝对坐标，需要乘以65535除以屏幕宽度）
            mouseInput.mkhi.mi.dy = (rect.Y + rect.Height / 2) * 65536 / Screen.PrimaryScreen.Bounds.Height;//设置鼠标垂直位置为区域的中心位置（如果使用绝对坐标，需要乘以65535除以屏幕高度）

            mouseInput.mkhi.mi.mouseData = 0; //设置鼠标滚轮或X按钮的数据为0（不使用）

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为移动和绝对坐标

            mouseInput.mkhi.mi.time = 0; //设置时间戳为0，让系统自动提供

            mouseInput.mkhi.mi.dwExtraInfo = IntPtr.Zero; //设置额外信息为0

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个移动鼠标的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为左键按下

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个按下左键的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为左键释放

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个释放左键的输入事件

        }//定义一个公共静态方法，用于模拟鼠标左键双击指定区域的中心位置
        public static void DoubleClickLeftMouseButton(Rectangle rect)
        {
            INPUT mouseInput = new INPUT(); //创建一个INPUT结构体实例，用于存储鼠标输入事件的数据
            mouseInput.type = SendInputEventType.InputMouse; //设置输入事件的类型为鼠标输入事件

            mouseInput.mkhi.mi.dx = (rect.X + rect.Width / 2) * 65536 / Screen.PrimaryScreen!.Bounds.Width; //设置鼠标水平位置为区域的中心位置（如果使用绝对坐标，需要乘以65535除以屏幕宽度）
            mouseInput.mkhi.mi.dy = (rect.Y + rect.Height / 2) * 65536 / Screen.PrimaryScreen.Bounds.Height; //设置鼠标垂直位置为区域的中心位置（如果使用绝对坐标，需要乘以65535除以屏幕高度）

            mouseInput.mkhi.mi.mouseData = 0; //设置鼠标滚轮或X按钮的数据为0（不使用）

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为移动和绝对坐标

            mouseInput.mkhi.mi.time = 0; //设置时间戳为0，让系统自动提供

            mouseInput.mkhi.mi.dwExtraInfo = IntPtr.Zero; //设置额外信息为0

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个移动鼠标的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为左键按下

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个按下左键的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为左键释放

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个释放左键的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为左键按下

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个按下左键的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为左键释放

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个释放左键的输入事件
        }

        //定义一个公共静态方法，用于模拟鼠标左键长按指定区域的中心位置
        public static void HoldLeftMouseButton(Rectangle rect)
        {
            INPUT mouseInput = new INPUT(); //创建一个INPUT结构体实例，用于存储鼠标输入事件的数据
            mouseInput.type = SendInputEventType.InputMouse; //设置输入事件的类型为鼠标输入事件

            mouseInput.mkhi.mi.dx = (rect.X + rect.Width / 2) * 65536 / Screen.PrimaryScreen!.Bounds.Width; //设置鼠标水平位置为区域的中心位置（如果使用绝对坐标，需要乘以65535除以屏幕宽度）
            mouseInput.mkhi.mi.dy = (rect.Y + rect.Height / 2) * 65536 / Screen.PrimaryScreen.Bounds.Height; //设置鼠标垂直位置为区域的中心位置（如果使用绝对坐标，需要乘以65535除以屏幕高度）

            mouseInput.mkhi.mi.mouseData = 0; //设置鼠标滚轮或X按钮的数据为0（不使用）

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为移动和绝对坐标

            mouseInput.mkhi.mi.time = 0; //设置时间戳为0，让系统自动提供

            mouseInput.mkhi.mi.dwExtraInfo = IntPtr.Zero; //设置额外信息为0

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个移动鼠标的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为左键按下

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个按下左键的输入事件

            System.Threading.Thread.Sleep(1000); //让线程暂停一秒，模拟长按的效果

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为左键释放

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个释放左键的输入事件
        }

        //定义一个公共静态方法，用于模拟鼠标右键单击指定区域的中心位置
        public static void ClickRightMouseButton(Rectangle rect)
        {
            INPUT mouseInput = new INPUT(); //创建一个INPUT结构体实例，用于存储鼠标输入事件的数据
            mouseInput.type = SendInputEventType.InputMouse; //设置输入事件的类型为鼠标输入事件

            mouseInput.mkhi.mi.dx = (rect.X + rect.Width / 2) * 65536 / Screen.PrimaryScreen!.Bounds.Width; //设置鼠标水平位置为区域的中心位置（如果使用绝对坐标，需要乘以65535除以屏幕宽度）
            mouseInput.mkhi.mi.dy = (rect.Y + rect.Height / 2) * 65536 / Screen.PrimaryScreen.Bounds.Height; //设置鼠标垂直位置为区域的中心位置（如果使用绝对坐标，需要乘以65535除以屏幕高度）

            mouseInput.mkhi.mi.mouseData = 0; //设置鼠标滚轮或X按钮的数据为0（不使用）

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为移动和绝对坐标

            mouseInput.mkhi.mi.time = 0; //设置时间戳为0，让系统自动提供

            mouseInput.mkhi.mi.dwExtraInfo = IntPtr.Zero; //设置额外信息为0

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个移动鼠标的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTDOWN | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为右键按下

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个按下右键的输入事件

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTUP | MouseEventFlags.MOUSEEVENTF_ABSOLUTE; //设置鼠标事件的标志位为右键释放

            SendInput(1, ref mouseInput, INPUT.Size); //调用SendInput函数，发送一个释放右键的输入事件
        }






        public static void Handle(EventKey eventKey, Rectangle rectangle)
        {
            if (eventKey == EventKey.mouseLeftClick)
            {
                ClickLeftMouseButton(rectangle);
                return;
            }
            if (eventKey == EventKey.mouseLeftDoubleClick)
            {
                DoubleClickLeftMouseButton(rectangle);
                return;
            }
            if (eventKey == EventKey.mouseLeftLongClick)
            {
                HoldLeftMouseButton(rectangle);
                return;
            }
            if (eventKey == EventKey.mouseRightClick)
            {
                ClickRightMouseButton(rectangle);
                return;
            }
            throw new Exception($"未处理事件 {eventKey}");
        }

    }


    // // 判断A图是否存在于B图内的某个区域的类，并获取所在B图内的A图区域坐标，其中图片只需相似即可。无法识别B图放大或缩小。
    // public class ImageMatcher
    // {
    //     // similarityThreshold 参数可根据需要调整，默认值为0.8。如果匹配得分高于此阈值，则认为图像A存在于图像B中。
    //     public static Tuple<bool, Rectangle> MatchImage(string imagePathA, string imagePathB, double similarityThreshold = 0.5)
    //     {
    //         using Mat imageA = CvInvoke.Imread(imagePathA, ImreadModes.Color);
    //         using Mat imageB = CvInvoke.Imread(imagePathB, ImreadModes.Color);

    //         if (imageA.IsEmpty || imageB.IsEmpty)
    //         {
    //             throw new ArgumentException("One or both of the input images are not found or invalid.");
    //         }

    //         using Mat result = new Mat();
    //         CvInvoke.MatchTemplate(imageB, imageA, result, TemplateMatchingType.CcoeffNormed);

    //         double minVal = 0, maxVal = 0;
    //         Point minLoc = new Point(), maxLoc = new Point();
    //         CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

    //         if (maxVal >= similarityThreshold)
    //         {
    //             Rectangle matchRect = new Rectangle(maxLoc, new Size(imageA.Width, imageA.Height));
    //             return Tuple.Create(true, matchRect);
    //         }
    //         else
    //         {
    //             return Tuple.Create(false, new Rectangle());
    //         }
    //     }
    // }
    public class ImageMatcher
    {
        // 匹配两张图片，返回是否匹配成功和匹配区域
        public static List<Rectangle> MatchImage(string imagePathA, string imagePathB, double similarityThreshold = 0.7)
        {
            // 读取图片A和图片B
            using Mat imageA = CvInvoke.Imread(imagePathA, ImreadModes.Color); // 读取图片A
            using Mat imageB = CvInvoke.Imread(imagePathB, ImreadModes.Color); // 读取图片B

            // 判断图片A和图片B是否存在
            if (imageA.IsEmpty || imageB.IsEmpty) // 如果图片A或图片B不存在
            {
                throw new ArgumentException("输入的一张或两张图片不存在或无效。");
            }

            // 创建一个Mat对象result，用于存储匹配结果
            using Mat result = new Mat();
            // 创建一个Mat对象result
            // 使用模板匹配算法，将匹配结果存储在result中
            // 使用相关系数归一化模板匹配算法，将匹配结果存储在result中
            CvInvoke.MatchTemplate(imageB, imageA, result, TemplateMatchingType.CcoeffNormed);
            // 获取result中的最小值、最大值、最小值位置和最大值位置
            double minVal = 0, maxVal = 0; // 定义最小值和最大值
            Point minLoc = new Point(), maxLoc = new Point(); // 定义最小值位置和最大值位置
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc); // 获取result中的最小值、最大值、最小值位置和最大值位置

            List<Rectangle> matchRects = new List<Rectangle>(); // 创建一个List对象matchRects
            while (maxVal >= similarityThreshold) // 如果最大值大于等于相似度阈值
            {
                Rectangle matchRect = new Rectangle(maxLoc, new Size(imageA.Width, imageA.Height)); // 创建一个Rectangle对象matchRect
                matchRects.Add(matchRect); // 将matchRect添加到matchRects中

                // 将已经匹配的区域及周围区域置为0，避免重复匹配
                int margin = 5;
                Rectangle marginRect = new Rectangle(maxLoc.X - margin, maxLoc.Y - margin, imageA.Width + margin * 2, imageA.Height + margin * 2);
                CvInvoke.Rectangle(result, marginRect, new MCvScalar(0), -1);

                // 继续查找下一个匹配区域
                CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc); // 获取result中的最小值、最大值、最小值位置和最大值位置
            }

            return matchRects; // 返回matchRects
        }
    }

    public class Method
    {
        // 获取指定枚举对应的描述
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());
            DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            if (attribute != null)
            {
                return attribute.Description;
            }
            else
            {
                throw new Exception("未获取到类型");
            }
        }

        // 将source复制到target中。
        public static void CopyObjectProperties(object source, object target)
        {
            var sourceProperties = source.GetType().GetProperties();
            var targetProperties = target.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                var targetProperty = targetProperties.FirstOrDefault(p => p.Name == sourceProperty.Name && p.PropertyType == sourceProperty.PropertyType);

                if (targetProperty != null && targetProperty.CanWrite)
                {
                    targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
                }
            }
        }
    }

}
