public class FlowLayoutPanelCustom : FlowLayoutPanel
{
    private bool isResizing = false;
    private Point dragStart;
    private Size startSize;
    private Rectangle dragRect;
    private bool isDragging = false;

    public FlowLayoutPanelCustom()
    {
        this.ResizeRedraw = true;
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        this.Cursor = Cursors.SizeNWSE;
        this.MouseDown += FlowLayoutPanelCustom_MouseDown;
        this.MouseUp += FlowLayoutPanelCustom_MouseUp;
        this.MouseMove += FlowLayoutPanelCustom_MouseMove;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (isDragging)
        {
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor,
                dragRect.Right - 16, dragRect.Bottom - 16, 16, 16);
        }
        else
        {
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor,
                this.ClientSize.Width - 16, this.ClientSize.Height - 16, 16, 16);
        }
    }

    private void FlowLayoutPanelCustom_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left &&
            this.ClientRectangle.Contains(e.Location) &&
            e.Location.X >= this.ClientSize.Width - 16 &&
            e.Location.Y >= this.ClientSize.Height - 16)
        {
            isResizing = true;
            dragStart = e.Location;
            startSize = this.Size;
            this.Capture = true;
        }
    }

    private void FlowLayoutPanelCustom_MouseUp(object sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            isDragging = false;
            this.Size = dragRect.Size;
            this.PerformLayout();
            this.Invalidate();
        }
        else if (isResizing)
        {
            isResizing = false;
            this.Capture = false;
            this.Invalidate();
        }
    }

    private void FlowLayoutPanelCustom_MouseMove(object sender, MouseEventArgs e)
    {
        if (isResizing)
        {
            int newWidth = startSize.Width + e.X - dragStart.X;
            int newHeight = startSize.Height + e.Y - dragStart.Y;
            if (newWidth < this.MinimumSize.Width)
                newWidth = this.MinimumSize.Width;
            if (newHeight < this.MinimumSize.Height)
                newHeight = this.MinimumSize.Height;
            this.Size = new Size(newWidth, newHeight);
            this.Invalidate();
        }
        else if (!isDragging &&
            e.Location.X >= this.ClientSize.Width - 16 &&
            e.Location.Y >= this.ClientSize.Height - 16)
        {
            this.Cursor = Cursors.SizeNWSE;
        }
        else
        {
            this.Cursor = Cursors.Default;
        }

        if (e.Button == MouseButtons.Left && isResizing)
        {
            dragRect = new Rectangle(dragStart, new Size(e.X - dragStart.X, e.Y - dragStart.Y));
            isDragging = true;
            this.Invalidate();
        }
    }
}