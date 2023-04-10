 public class CustomFlowLayoutPanel : FlowLayoutPanel
    {
        private int _height;
        private int _lastY;
        private bool _mouseDown;

        public CustomFlowLayoutPanel()
        {
            _height = this.Height;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseDown = true;
                _lastY = e.Y;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_mouseDown)
            {
                int newHeight = _height + (e.Y - _lastY);
                if (newHeight > 0)
                {
                    this.Height = newHeight;
                    this.Invalidate();
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _mouseDown = false;
            _height = this.Height;

            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 绘制控件的内容
            e.Graphics.FillRectangle(Brushes.White, this.ClientRectangle);
            e.Graphics.DrawString("Custom FlowLayoutPanel", this.Font, Brushes.Black, 10, 10);
        }
    }