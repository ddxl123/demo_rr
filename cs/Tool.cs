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
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        public static void Click(Rectangle rect)
        {
            int x = rect.Left + rect.Width / 2;
            int y = rect.Top + rect.Height / 2;

            SetCursorPos(x, y);

            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, 0);
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

        // 获取枚举全部成员
        public static T[] GetEnumMembers<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }


        public static int GetEnumIndex<T>(Enum value)
        {
            return Array.IndexOf(GetEnumMembers<T>(), value);
        }


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

    }

}
