
public enum ScreenRegionSelectorResult
{
    ok,
    // 截图尺寸太小
    tooSmall,
    // 取消
    cancel,
}
// 调用 new ScreenRegionSelector().ShowDialog() 即可。
public class ScreenRegionSelector : Form
{

    private Point startPoint;
    private Rectangle selectionRect;
    private bool isDragging;
    private string? imagePath;

    private Size? minSize;

    public ScreenRegionSelectorResult screenRegionSelectorResult = ScreenRegionSelectorResult.cancel;

    public ScreenRegionSelector(string imagePath, Size? minSize)
    {
        this.imagePath = imagePath;
        this.FormBorderStyle = FormBorderStyle.None;
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.Black;
        this.Opacity = 0.3;
        this.DoubleBuffered = true;
        this.minSize = minSize;

        this.KeyDown += new KeyEventHandler(EscapeKeyExample_KeyDown);
    }

    private void EscapeKeyExample_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Close();
        }
    }


    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        // 开始拖拽选择框
        startPoint = e.Location;
        selectionRect = new Rectangle(startPoint, Size.Empty);
        isDragging = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // 拖拽选择框
        if (isDragging)
        {
            selectionRect.Location = new Point(
                Math.Min(e.X, startPoint.X),
                Math.Min(e.Y, startPoint.Y));
            selectionRect.Size = new Size(
                Math.Abs(e.X - startPoint.X),
                Math.Abs(e.Y - startPoint.Y));
            this.Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        // 停止拖拽选择框
        isDragging = false;

        // 关闭选择框
        this.Dispose();
        if (minSize != null)
        {
            if (selectionRect.Size.Width < minSize.Value.Width && selectionRect.Size.Height < minSize.Value.Height)
            {
                screenRegionSelectorResult = ScreenRegionSelectorResult.tooSmall;
                MessageBox.Show($"截图尺寸太小！");
                return;
            }

        }
        Tool.Capturer.CaptureFullScreen(imagePath!, selectionRect);
        screenRegionSelectorResult = ScreenRegionSelectorResult.ok;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // 绘制选择框
        if (isDragging)
        {
            using (Pen pen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawRectangle(pen, selectionRect);
            }
        }
    }
}