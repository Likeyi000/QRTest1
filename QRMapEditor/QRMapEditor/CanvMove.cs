using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QRMapEditor
{
    public class CanvMove
    {
        //移动标志
        bool isMoving = false;
        //保存当前移动的是哪个文本
        Canvas canvas;
        DockPanel canvDock, canvD;
        Point PtoCanv, PtoDock;
        public void CanvM(Canvas canv, DockPanel CanvDock, DockPanel CanvD)
        {
            this.canvas = canv;
            this.canvDock = CanvDock;
            this.canvD = CanvD;
            canvas.Tag = new CanvasTag();
            PtoCanv = canvD.TranslatePoint(new Point(0, canvas.Height), canvDock);
            PtoDock = canvD.TranslatePoint(new Point(0, 0), canvDock);
            Reset(canv);
            SetCanvM();
        }
        private void SetCanvM()
        {
            canvas.Focusable = true;
            canvas.Focus();
            Keyboard.Focus(canvas);
            canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            canvas.MouseMove += Canvas_MouseMove;
        }
        private class CanvasTag
        {
            public CanvasTag()
            {
                TotalTranslate = new TranslateTransform();
                TempTranslate = new TranslateTransform();
            }
            public TranslateTransform TotalTranslate { get; set; }
            public TranslateTransform TempTranslate { get; set; }
            public Point StartMovePosition { get; set; }
        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var canv = sender as Canvas;
            if (canvas != canv)
            {
                canvas = new Canvas();
                canvas = canv;
            }
            (canvas.Tag as CanvasTag).StartMovePosition = e.GetPosition(canvD);
            //(canvas.Tag as CanvasTag).StartMovePosition = Mouse.GetPosition(canvD);
            //(canvas.Tag as CanvasTag).StartMovePosition = canvD.TranslatePoint(new Point(Mouse.GetPosition(canvas).X, Mouse.GetPosition(canvas).Y - canvDock.Height), canvDock);
            isMoving = true;
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (canvas == null)
                return;
            isMoving = false;
            Point endMovePosition = e.GetPosition(canvas);
            //Point endMovePosition = canvas.TranslatePoint(new Point(Mouse.GetPosition(canvas).X, Mouse.GetPosition(canvas).Y - canvD.Height), canvDock);
            (canvas.Tag as CanvasTag).TotalTranslate.X += (endMovePosition.X - (canvas.Tag as CanvasTag).StartMovePosition.X);
            (canvas.Tag as CanvasTag).TotalTranslate.Y += (endMovePosition.Y - (canvas.Tag as CanvasTag).StartMovePosition.Y);
            canvas = null;
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMoving && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentMousePosition = e.GetPosition(canvD);//当前鼠标位置
                Point deltaPt = new Point(0, 0)
                {
                    X = (currentMousePosition.X - (canvas.Tag as CanvasTag).StartMovePosition.X),
                    Y = (currentMousePosition.Y - (canvas.Tag as CanvasTag).StartMovePosition.Y)
                };
                (canvas.Tag as CanvasTag).TempTranslate.X = (canvas.Tag as CanvasTag).TotalTranslate.X + deltaPt.X;
                (canvas.Tag as CanvasTag).TempTranslate.Y = (canvas.Tag as CanvasTag).TotalTranslate.Y + deltaPt.Y;
                TransformGroup tfGroup = new TransformGroup();
                tfGroup.Children.Add((canvas.Tag as CanvasTag).TempTranslate);
                canvas.RenderTransform = tfGroup;
            }
        }
        public void Reset(Canvas can)
        {
            if ((can.Tag as CanvasTag).TempTranslate.X != 0 ||
                (can.Tag as CanvasTag).TempTranslate.Y != -1 * (canvD.Height - canvDock.Height + 10))
            {
                (can.Tag as CanvasTag).TotalTranslate.X = 0;
                (can.Tag as CanvasTag).TotalTranslate.Y = -1 * (canvD.Height - canvDock.Height);
                (can.Tag as CanvasTag).TempTranslate.X = 0;
                (can.Tag as CanvasTag).TempTranslate.Y = -1 * (canvD.Height - canvDock.Height + 10);
                TransformGroup tfGroup = new TransformGroup();
                tfGroup.Children.Add((can.Tag as CanvasTag).TempTranslate);
                can.RenderTransform = tfGroup;
            }
        }
        public void CML(Canvas can)
        {
            if (PtoCanv.X + can.Width <= 0)
            {
                MessageBox.Show("向左向移动极限");
            }
            else
            {
                (can.Tag as CanvasTag).TempTranslate.X = (can.Tag as CanvasTag).TempTranslate.X - 60;
                PtoCanv.X -= 60;
            }
        }
        public void CMR(Canvas can)
        {
            if (PtoCanv.X >= canvDock.Width)
            {
                MessageBox.Show("向右向移动极限");
            }
            else
            {
                (can.Tag as CanvasTag).TempTranslate.X = (can.Tag as CanvasTag).TempTranslate.X + 60;
                PtoCanv.X += 60;
            }
        }
        public void CMU(Canvas can)
        {
            if (PtoCanv.Y <= PtoDock.Y)
            {
                MessageBox.Show("向上向移动极限");
            }
            else
            {
                (can.Tag as CanvasTag).TempTranslate.Y = (can.Tag as CanvasTag).TempTranslate.Y - 50;
                PtoCanv.Y -= 50;
            }
        }
        public void CMD(Canvas can)
        {
            if (PtoCanv.Y - can.Height >= PtoDock.Y + canvDock.Height)
            {
                MessageBox.Show("向下向移动极限");
            }
            else
            {
                (can.Tag as CanvasTag).TempTranslate.Y = (can.Tag as CanvasTag).TempTranslate.Y + 50;
                PtoCanv.Y += 50;
            }
        }
    }
}
