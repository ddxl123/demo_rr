
// using Microsoft.Toolkit.Uwp.Notifications;
// using System.Windows;
// public class Toast
// { // 构造函数，接受一个窗口对象作为参数
//     public Toast(Window window)
//     {
//         // 获取窗口的位置和大小
//         var rect = window.RestoreBounds;
//         var x = rect.X;
//         var y = rect.Y;
//         var width = rect.Width;
//         var height = rect.Height;

//         // 设置toast的位置和大小，使其显示在窗口的下方
//         ToastNotificationManagerCompat.History.DefaultToastPosition = new ToastPosition()
//         {
//             HorizontalAlignment = ToastHorizontalAlignment.Center,
//             VerticalAlignment = ToastVerticalAlignment.After,
//             Offset = new ToastOffset()
//             {
//                 X = x + width / 2,
//                 Y = y + height
//             }
//         };
//     }

//     // 显示一个简单的toast消息，接受一个字符串作为参数
//     public void ShowToast(string message)
//     {
//         // 创建一个toast内容对象，添加文本
//         var toastContent = new ToastContentBuilder()
//             .AddText(message);

//         // 显示toast
//         toastContent.Show();
//     }
// }