// 调用 new ScreenRegionSelector().Show() 即可。
public class ScreenRegionSelector : Form
{

    private Point startPoint;
    private Rectangle selectionRect;
    private bool isDragging;
    private string? imagePath;

    public ScreenRegionSelector(string imagePath)
    {
        this.imagePath = imagePath;
        this.FormBorderStyle = FormBorderStyle.None;
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.Black;
        this.Opacity = 0.3;
        this.DoubleBuffered = true;


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

        Tool.Capturer.CaptureFullScreen(imagePath!, selectionRect);
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