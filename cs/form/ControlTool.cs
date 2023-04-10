using Tool;

public class ControlTool
{
    // 分隔符
    public static Control separator()
    {
        Panel separator = new Panel();
        separator.Dock = DockStyle.Fill;
        separator.Height = 2;
        separator.BackColor = Color.FromArgb(50, Color.Gray);
        return separator;
    }

    // 空图片填充
    public static void emptyImage(PictureBox pictureBox)
    {
        if (pictureBox.Image != null)
        {
            pictureBox.Image.Dispose();
        }
        Bitmap bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
        Graphics.FromImage(bmp).FillRectangle(Brushes.LightGray, 0, 0, pictureBox.Width, pictureBox.Height);
        pictureBox.Image = bmp;
    }
}