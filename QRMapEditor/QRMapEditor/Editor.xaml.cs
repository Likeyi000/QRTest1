using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Concurrent;
using System.Threading;

namespace QRMapEditor
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor
    {
        #region 对象
        MapFile mFile;
        NewMap newM;
        InputMap inputM;
        SaveMap saveM;
        OutputMap outputM;
        Rectangle hightlightEle;
        CanvMove CanvM;
        AGV agv;
        Dictionary<int, Ellipse> AGVEllipse;
        Dictionary<int, TextBox> AGVTextBox;
        List<CheckBox> singleCheck;
        //绘制agv对象
        DispatcherTimer agvTime; //agv更新
        ConcurrentQueue<AGV> agvque;      //agv位置更新队列
        Dictionary<int, AGV> agvs;         //存储agv的字典
        #endregion

        #region 数据元素
        //模板文件路径
        string oldModelPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config" + @"\map_model.xml";
        //新建文件路径
        public string newModelPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Maps\";
        //配置文件路径
        string confPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config" + @"\Config.xml";

        private readonly int NodeEleHeight = 8;
        private readonly int NodeEleWidth = 8;
        private readonly int RoadStrokeThickness = 2;
        private readonly int scaX = 30;
        private readonly int scaY = 33;

        private int inputNodeCount;
        private int inputRoadCount;
        private int clickCount = 0;
        enum Model
        {
            view = 0,       //视图模式
            edit = 1,       //编辑模式
        }
        Model model = Model.edit;
        #endregion

        public Editor()
        {
            InitializeComponent();
            Ini();
        }
        public Editor(int mod = 1)
        {
            InitializeComponent();
            this.model = mod == 1 ? Model.edit : Model.view;
            PropertySpace.Visibility = Visibility.Collapsed;
            canvDock.Width = 1500;
            ToolSpace.Width = 1500;
            Station.Visibility = Visibility.Collapsed;
            ToolSpace.Visibility = Visibility.Collapsed;
            InforSpace.Visibility = Visibility.Collapsed;
            Ini();
        }

        #region Initialize
        private void Ini()
        {
            Canv.MouseEnter += Canv_MoustEnter;
            NewFile.ToolTip = $"{"NewFile"}";
            InFile.ToolTip = $"{"InputFile"}";
            btnOutFile.IsEnabled = false;
            btnOutFile.Opacity = 0.2;
            btnOutFile.ToolTip = $"{"OutPutFile"}";
            btnSaveFile.IsEnabled = false;
            btnSaveFile.Opacity = 0.2;
            btnSaveFile.ToolTip = $"{"SavrFile"}";
            CanvM = new CanvMove();
            inputM = new InputMap();
            mFile = new MapFile()
            {
                ScaX = scaX,
                ScaY = scaY,
                NewPath = newModelPath,
                ConPath = confPath,
            };
            inputNodeCount = 0;
            inputRoadCount = 0;
            agvTime = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.5) };
            AGVEllipse = new Dictionary<int, Ellipse>();
            AGVTextBox = new Dictionary<int, TextBox>();
            agvque = new ConcurrentQueue<AGV>();
            agvs = new Dictionary<int, AGV>();
            agvTime.Tick += (s, eve) =>
            {
                AGV agv;
                while (agvque.TryDequeue(out agv))
                {
                    AGVLocation(agv.ID, agv.x, agv.y, agv.OR);
                }
                //test();
            };
            agvTime.Start();
        }
        #endregion

        #region agv显示
        public void UpdateAGV(int id, int x, int y, int or)
        {
            agvque.Enqueue(new AGV
            {
                ID = id,
                x = x,
                y = y,
                OR = or,
            });
        }
        public void UpdatePath(int id, List<int> lisID)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (!mFile.myPolyline.ContainsKey(id))
                {
                    mFile.myPolyline.Add(id, new Polyline());
                }
                if (mFile.myPolyline[id].Points.Count > 0)
                {
                    mFile.myPolyline[id].Points.Clear();
                }
                for (int i = 0; i < lisID.Count; i++)
                {
                    mFile.myPolyline[id].Points.Add(new Point(mFile.MapNodes[lisID[i]].x,
                        mFile.MapNodes[lisID[i]].y));
                }
                if (Canv.Children.Contains(mFile.myPolyline[id]))
                {
                    Canv.Children.Remove(mFile.myPolyline[id]);
                }
                drawPath(id, mFile.myPolyline[id]);
            }));
            //App.Current.Dispatcher.Invoke((Action)(() =>
            //{
            //    if (!mFile.myPolyline.ContainsKey(id))
            //    {
            //        mFile.myPolyline.Add(id, new Polyline());
            //    }
            //    if (mFile.myPolyline[id].Points.Count > 0)
            //    {
            //        mFile.myPolyline[id].Points.Clear();
            //    }
            //    for (int i = 0; i < lisID.Count; i++)
            //    {
            //        mFile.myPolyline[id].Points.Add(new Point(mFile.MapNodes[lisID[i]].x, mFile.MapNodes[lisID[i]].y));
            //    }
            //    if (Canv.Children.Contains(mFile.myPolyline[id]))
            //    {
            //        Canv.Children.Remove(mFile.myPolyline[id]);
            //    }
            //    drawPath(id, mFile.myPolyline[id]);
            //}));
        }
        public void OpenXml(string path = null)
        {
            if (System.IO.File.Exists(path))
            {
                mFile.NewPath = path;
                inputM.InputM(mFile);
                DrawInputMap(mFile);
                labM.Text = mFile.M.ToString();
                labN.Text = mFile.N.ToString();
                FileName.Text = System.IO.Path.GetFileName(mFile.NewPath).ToString() + "\n" + mFile.NewPath;
                btnOutFile.IsEnabled = true;
                btnOutFile.Opacity = 1;
                btnSaveFile.IsEnabled = true;
                btnSaveFile.Opacity = 01;
                CanvM.CanvM(Canv, canvDock, CanvD);
            }
            else
            {
                string mess = "该文件不存在！";
                throw new System.IO.FileNotFoundException(mess);
            }
        }
        private void AGVLocation(int id, int x, int y, int or)
        {
            if (agvs.ContainsKey(id))
            {
                AGVEllipse[id].Margin = new Thickness((x - 1) * scaX - 6, (int)Canv.Height - (y - 1) * scaY - 6, 0, 0);
                AGVTextBox[id].Margin = new Thickness((x - 1) * scaX - 6, (int)Canv.Height - (y - 1) * scaY - 6, 0, 0);
            }
            else
            {
                agv = new AGV
                {
                    ID = id,
                    X = x,
                    Y = y,
                    x = x,
                    y = y,
                    OR = or,
                };
                agvs.Add(id, agv);
                drawAGV(agv);
            }
        }
        private void drawAGV(AGV agv)
        {
            Ellipse agvEll = new Ellipse
            {
                Tag = agv.ID,
                Width = 12,
                Height = 12,
                StrokeThickness = 6,
                Margin = new Thickness((agv.x - 1) * scaX - 6, (int)Canv.Height - (agv.y - 1) * scaY - 6, 0, 0),
                Stroke = mFile.listColor[new Random().Next(1, 17)]
            };
            TextBox agvTxt = new TextBox
            {
                Tag = agv.ID,
                Width = 10,
                Height = 10,
                FontSize = 6,
                BorderBrush = null,
                Background = null,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                IsReadOnly = true,
                Text = agv.ID.ToString(),
                Margin = new Thickness((agv.x - 1) * scaX - 6, (int)Canv.Height - (agv.y - 1) * scaY - 6, 0, 0),
            };
            Panel.SetZIndex(agvEll, 10);
            Panel.SetZIndex(agvTxt, 11);
            AGVEllipse.Add(agv.ID, agvEll);
            AGVTextBox.Add(agv.ID, agvTxt);
            Canv.Children.Add(agvEll);
            Canv.Children.Add(agvTxt);
        }
        private void drawPath(int id, Polyline polyL)
        {
            polyL.Stroke = AGVEllipse[id].Stroke;
            polyL.StrokeThickness = 2.6;
            polyL.FillRule = FillRule.EvenOdd;
            Panel.SetZIndex(polyL, 2);
            Canv.Children.Add(polyL);
        }
        private void test()
        {
            Random ra = new Random();
            for (int i = 1; i < 25; i++)
            {
                int agvx = ra.Next(1, 40);
                int agvy = ra.Next(1, 20);
                UpdateAGV(i, agvx, agvy, 0);
            }
        }
        #endregion

        #region ClickEvent
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            newM = new NewMap();
            mFile.NewPath = newModelPath + "map" + "_"
                + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString()
            + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString()
            + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".xml";
            mFile.ConPath = confPath;
            mFile.OldPath = oldModelPath;
            inputNodeCount = 0;
            inputRoadCount = 0;

            newM.NewFile(mFile);
            btnOutFile.IsEnabled = true;
            btnOutFile.Opacity = 1;
            btnSaveFile.IsEnabled = true;
            btnSaveFile.Opacity = 1;

            labM.Text = mFile.M.ToString();
            labN.Text = mFile.N.ToString();
            FileName.Text = System.IO.Path.GetFileName(mFile.NewPath).ToString() + "\n" + mFile.NewPath;

            DrawNewMap(mFile);
            CanvM.CanvM(Canv, canvDock, CanvD);
        }
        private void InFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenDia = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Xml Files(*.xml)|*.xml|All Files(*.)|*.*"
            };
            OpenDia.InitialDirectory = mFile.NewPath;
            if (OpenDia.ShowDialog() == System.Windows.Forms.DialogResult.OK && mFile.NewPath != "")
            {
                mFile.NewPath = OpenDia.FileName;       //文件路径
                inputM.InputM(mFile);
                DrawInputMap(mFile);
                labM.Text = mFile.M.ToString();
                labN.Text = mFile.N.ToString();
                FileName.Text = System.IO.Path.GetFileName(mFile.NewPath).ToString() + "\n" + mFile.NewPath;
                btnOutFile.IsEnabled = true;
                btnOutFile.Opacity = 1;
                btnSaveFile.IsEnabled = true;
                btnSaveFile.Opacity = 01;
                CanvM.CanvM(Canv, canvDock, CanvD);
            }
            //new Thread(() =>
            //{
            //    UpdatePath(1, new List<int> { 16005, 15005, 15006, 15007, 15008, 16008 });
            //    UpdatePath(1, new List<int> { 3008, 4008, 5008, 5007 });
            //    UpdatePath(3, new List<int> { 7008, 8008, 8007, 8006, 8005, 7005 });
            //    UpdatePath(4, new List<int> { 28002, 29002, 30002, 30001 });
            //}).Start();
        }
        private void btnNodeOk_Click(object sender, RoutedEventArgs e)
        {
            mFile.MapNodes[int.Parse(NodeID.Text)].ROLE =
                (Nodes.Role)Enum.Parse(typeof(Nodes.Role), combRole.Text);
            mFile.MapNodes[int.Parse(NodeID.Text)].Dockable = bool.Parse(combDockable.Text);
            mFile.MapNodes[int.Parse(NodeID.Text)].QR = int.Parse(NodeQR.Text);
            mFile.NodeRect[int.Parse(NodeID.Text)].Stroke = Brushes.WhiteSmoke;
            if (mFile.MapNodes[int.Parse(NodeID.Text)].IsPosable)
            {
                if (mFile.MapNodes[int.Parse(NodeID.Text)].Dockable)
                {
                    if (mFile.MapNodes[int.Parse(NodeID.Text)].ROLE == Nodes.Role.Site)
                    {
                        mFile.NodeRect[int.Parse(NodeID.Text)].Stroke = Brushes.Orange;
                    }
                    else if (mFile.MapNodes[int.Parse(NodeID.Text)].ROLE == Nodes.Role.Normal)
                    {
                        mFile.NodeRect[int.Parse(NodeID.Text)].Stroke = Brushes.LightSeaGreen;
                    }
                }
                else
                {
                    mFile.NodeRect[int.Parse(NodeID.Text)].Stroke = Brushes.Khaki;
                }
            }
            for (int i = 0; i < NodeSiblingDock.Children.Count; i++)
            {
                CheckBox cb = (CheckBox)NodeSiblingDock.Children[i];
                if (cb.IsChecked == true)
                {
                    int id = int.Parse(NodeID.Text);
                    mFile.MapNodes[id].Siblings[i].Pose.Clear();
                    for (int j = 0; j < RoleDock.Children.Count; j++)
                    {
                        CheckBox c = (CheckBox)RoleDock.Children[j];
                        if (c.IsChecked == true)
                        {
                            mFile.MapNodes[id].Siblings[i].Pose.Add(int.Parse(System.Text.RegularExpressions.Regex.Replace(c.Content.ToString(), @"[^0-9]+", "")));
                        }
                    }
                }
            }
        }
        //路径信息（暂时不用）
        //private void btnRoadOk_Click(object sender, RoutedEventArgs e)
        //{
        //    for (int i = 0; i < RoadSiblingDock.Children.Count; i++)
        //    {
        //        CheckBox cb = (CheckBox)RoadSiblingDock.Children[i];
        //        if (cb.IsChecked == true)
        //        {
        //            int tag = int.Parse(System.Text.RegularExpressions.Regex.Replace(cb.Content.ToString(), @"[^0-9]+", ""));
        //            for (int j = 0; j < RNRoleDock.Children.Count; j++)
        //            {
        //                CheckBox c = (CheckBox)RNRoleDock.Children[j];
        //                if (c.IsChecked == true)
        //                {
        //                    mFile.editorNodes[tag].Pose.Add(int.Parse(System.Text.RegularExpressions.Regex.Replace(c.Content.ToString(), @"[^0-9]+", "")));
        //                }
        //            }
        //        }
        //    }
        //}
        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            saveM = new SaveMap();
            mFile.m = int.Parse(labM.Text);
            mFile.n = int.Parse(labN.Text);
            ExtenorReduce(mFile);
            saveM.saveConf(mFile);
        }
        private void Output_Click(object sender, RoutedEventArgs e)
        {
            outputM = new OutputMap();
            mFile.m = int.Parse(labM.Text);
            mFile.n = int.Parse(labN.Text);
            outputM.SaveFile(mFile);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            saveM = new SaveMap();
            mFile.m = int.Parse(labM.Text);
            mFile.n = int.Parse(labN.Text);
            saveM.SaveFile(mFile);
        }
        #endregion

        #region MouseEvent
        private void Station_MouseEnter(object sender, MouseEventArgs e)
        {
            Station.Opacity = 0.9;
        }
        private void Station_MouseLeave(object sender, MouseEventArgs e)
        {
            Station.Opacity = 0.2;
        }
        private void Node_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (singleCheck.Count > 0)
            {
                singleCheck.Clear();
                cbPose0.IsChecked = false;
                cbPose90.IsChecked = false;
                cbPose180.IsChecked = false;
                cbPose270.IsChecked = false;
            }
            Rectangle rec = (Rectangle)sender;
            int tag = (int)rec.Tag;

            clickCount++;
            DispatcherTimer clickTime = new DispatcherTimer();
            clickTime.Interval = new TimeSpan(0, 0, 0, 0, 500);
            clickTime.Tick += (s, e1) =>
            {
                clickTime.IsEnabled = false;
                clickCount = 0;
            };
            clickTime.IsEnabled = true;
            if (0 == clickCount % 2 && model == Model.edit)         //是否双击事件
            {
                clickTime.IsEnabled = false;
                clickCount = 0;
                mFile.MapNodes[tag].IsPosable = !mFile.MapNodes[tag].IsPosable;   //双击更改元素启用状态
                inputNodeCount += mFile.MapNodes[tag].IsPosable ? 1 : -1;
                if (mFile.MapNodes[tag].IsPosable)
                {
                    if (mFile.MapNodes[tag].Dockable)
                    {
                        if (mFile.MapNodes[tag].ROLE == Nodes.Role.Site)
                        {
                            rec.Stroke = Brushes.Orange;
                        }
                        else if (mFile.MapNodes[tag].ROLE == Nodes.Role.Normal)
                        {
                            rec.Stroke = Brushes.LightSeaGreen;
                        }
                    }
                    else
                    {
                        rec.Stroke = Brushes.Khaki;
                    }
                }
                else
                {
                    rec.Stroke = Brushes.WhiteSmoke;
                }
            }
            NodeCount.Content = inputNodeCount.ToString();
            if (NodeSiblingDock.Children.Count > 0)
            {
                NodeSiblingDock.Children.Clear();
            }
            ShowNodeInfo(mFile.MapNodes[tag]);
        }
        private void ShowNodeInfo(Nodes node)
        {
            if (Canv.Children.Contains(hightlightEle))
            {
                Canv.Children.Remove(hightlightEle);
            }
            HightLightEle(node);
            NodeID.Text = node.ID.ToString();
            NodeX.Text = node.X.ToString();
            NodeY.Text = node.Y.ToString();
            NodeQR.Text = node.QR.ToString();
            combRole.Text = node.ROLE.ToString();
            NodeStatus.Text = node.IsPosable ? "Enabled" : "Disabled";
            combDockable.Text = node.Dockable.ToString();
            for (int j = 0; j < node.Siblings.Count; j++)
            {
                CheckBox cb = new CheckBox
                {
                    Name = "Node" + node.Siblings[j].ID.ToString() + "CB",
                    Content = "Node" + node.Siblings[j].ID.ToString(),
                    Margin = new Thickness(0, 10, 0, 0),
                };
                cb.Checked += Cb_Checked;
                cb.SetValue(DockPanel.DockProperty, Dock.Top);
                NodeSiblingDock.Children.Add(cb);
                singleCheck.Add(cb);
            }
            RealLocation.Content = (node.X * mFile.Unit).ToString() + " , " + (node.Y * mFile.Unit).ToString();
        }
        private void Cb_Checked(object sender, EventArgs e)
        {
            foreach (CheckBox cb in singleCheck)
            {
                if (cb != (CheckBox)sender)
                {
                    cb.IsChecked = false;
                }
            }
            CheckBox cB = (CheckBox)sender;
            int id = int.Parse(NodeID.Text);

            if (cB.IsChecked == true)
            {
                cbPose0.IsChecked = mFile.MapNodes[id].Siblings[NodeSiblingDock.Children.IndexOf(cB)].Pose.Contains(0);
                cbPose90.IsChecked = mFile.MapNodes[id].Siblings[NodeSiblingDock.Children.IndexOf(cB)].Pose.Contains(90);
                cbPose180.IsChecked = mFile.MapNodes[id].Siblings[NodeSiblingDock.Children.IndexOf(cB)].Pose.Contains(180);
                cbPose270.IsChecked = mFile.MapNodes[id].Siblings[NodeSiblingDock.Children.IndexOf(cB)].Pose.Contains(270);
            }
        }
        private void Node_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;
            int tag = (int)rect.Tag;
            Point p = Canv.PointToScreen(new Point(mFile.MapNodes[tag].x, mFile.MapNodes[tag].y));
            System.Drawing.Point point = new System.Drawing.Point((int)p.X, (int)p.Y);
            System.Windows.Forms.Cursor.Position = point;
        }
        private void Road_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Path pa = (Path)sender;
            int tag = (int)pa.Tag;
            clickCount++;
            DispatcherTimer clickTime = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 500)
            };
            clickTime.Tick += (s, e1) =>
            {
                clickTime.IsEnabled = false;
                clickCount = 0;
            };
            clickTime.IsEnabled = true;
            if (0 == clickCount % 2 && model == Model.edit)         //双击事件
            {
                clickTime.IsEnabled = false;
                clickCount = 0;
                if (Roads.Direction.Disable == mFile.MapRoads[tag].Direc)          //循环切换路劲状态
                {
                    mFile.MapRoads[tag].Direc = Roads.Direction.Single1;
                    inputRoadCount++;
                }
                else if (Roads.Direction.Single1 == mFile.MapRoads[tag].Direc)
                {
                    mFile.MapRoads[tag].Direc = Roads.Direction.Single2;
                }
                else if (Roads.Direction.Single2 == mFile.MapRoads[tag].Direc)
                {
                    mFile.MapRoads[tag].Direc = Roads.Direction.Both;
                }
                else
                {
                    mFile.MapRoads[tag].Direc = Roads.Direction.Disable;
                    inputRoadCount--;
                }
                pa.Stroke = mFile.MapRoads[tag].Direc == Roads.Direction.Disable ?  //改变颜色
                    Brushes.WhiteSmoke : Brushes.PaleTurquoise;
                RoadCount.Content = inputRoadCount.ToString();
                if (mFile.DirPolygon.ContainsKey(tag))
                {
                    Canv.Children.Remove(mFile.DirPolygon[tag]);
                }
                drawDirec(mFile.MapRoads[tag]);
            }
            //if (RoadSiblingDock.Children.Count > 0)
            //{
            //    RoadSiblingDock.Children.Clear();       //清空邻居点数据信息
            //}
            ShowRoadInfo(mFile.MapRoads[tag]);
        }
        private void ShowRoadInfo(Roads road)
        {
            if (Canv.Children.Contains(hightlightEle))
            {
                Canv.Children.Remove(hightlightEle);
            }
            //路径站点
            CheckBox cb1 = new CheckBox
            {
                Content = "Node" + road.node1.ID.ToString(),
                Margin = new Thickness(0, 10, 0, 0)
            };
            cb1.SetValue(DockPanel.DockProperty, Dock.Top);
            CheckBox cb2 = new CheckBox
            {
                Content = "Node" + road.node2.ID.ToString(),
                Margin = new Thickness(0, 10, 0, 0)
            };
            cb2.SetValue(DockPanel.DockProperty, Dock.Top);
            //RoadSiblingDock.Children.Add(cb1);
            //RoadSiblingDock.Children.Add(cb2);
            //drawDirec(mFile.editorRoads[tag]);
            //RoadStatus.Text = road.Direc.ToString();
        }
        private void Road_MouseEnter(object sender, MouseEventArgs e)
        {
            Path pa = (Path)sender;
            int tag = (int)pa.Tag;
            int px = (mFile.MapRoads[tag].node1.x + mFile.MapRoads[tag].node2.x) / 2;
            int py = (mFile.MapRoads[tag].node1.y + mFile.MapRoads[tag].node2.y) / 2;
            Point p = Canv.PointToScreen(new Point(px, py));
            System.Drawing.Point point = new System.Drawing.Point((int)p.X, (int)p.Y);
            System.Windows.Forms.Cursor.Position = point;
        }
        private void Canv_MoustEnter(object sender, MouseEventArgs e)
        {
            Canv.Focusable = true;
            Canv.Focus();
            e.Handled = true;
        }
        #endregion

        #region 绘制地图
        private void DrawNewMap(MapFile NFile)
        {
            if (Canv.Children.Count > 0)
            {
                Canv.Children.Clear();
                NFile.MapNodes.Clear();
                NFile.MapRoads.Clear();
            }
            //新建模板清屏
            if (Canv.Children.Count > 0)
            {
                Canv.Children.Clear();
            }
            //绘制新模板站点
            for (int i = 0; i < NFile.N; i++)
            {
                for (int j = 0; j < NFile.M; j++)
                {
                    Nodes node = new Nodes
                    {
                        X = j + 1,      //真实坐标
                        Y = i + 1,
                        x = j * scaX,    //显示坐标
                        y = (int)Canv.Height - i * scaY,
                        QR = NFile.NewQR++,
                        ID = (j + 1) * 1000 + i + 1,
                        IsPosable = true,
                        Pose = new List<int>(),
                        Siblings = new List<Nodes>(),
                        ROLE = Nodes.Role.Normal,
                    };
                    node.Dockable = node.ROLE != Nodes.Role.Normal;
                    NFile.MapNodes.Add(node.ID, node);
                    drawNode(node);
                }
            }
            AddSiblings(NFile);
            //绘制路径
            for (int i = 0; i < NFile.N; i++)
            {
                for (int j = 0; j < NFile.M - 1; j++)     //横向路径
                {
                    Roads road = new Roads
                    {
                        node1 = NFile.MapNodes[(j + 1) * 1000 + i + 1],
                        node2 = NFile.MapNodes[(j + 2) * 1000 + i + 1],
                        Direc = Roads.Direction.Single1,
                    };
                    road.id = 1 * 1000000 + road.node1.X * 1000 + road.node1.Y;
                    NFile.MapRoads.Add(road.id, road);
                    drawRoad(road);
                    drawDirec(road);
                }
            }
            for (int i = 0; i < NFile.M; i++)
            {
                for (int j = 0; j < NFile.N - 1; j++)     //纵向路径
                {
                    Roads road = new Roads
                    {
                        node1 = NFile.MapNodes[(i + 1) * 1000 + j + 1],
                        node2 = NFile.MapNodes[(i + 1) * 1000 + j + 2],
                        Direc = Roads.Direction.Single2
                    };
                    road.id = 2 * 1000000 + road.node1.X * 1000 + road.node1.Y;
                    NFile.MapRoads.Add(road.id, road);
                    drawRoad(road);
                    drawDirec(road);
                }
            }
        }
        private void AddSiblings(MapFile NFile)
        {
            //获取站点邻居站点
            for (int i = 0; i < NFile.N; i++)           //外层循环
            {
                for (int j = 0; j < NFile.M; j++)
                {
                    for (int k = 0; k < NFile.N; k++)   //内层循环
                    {
                        for (int l = 0; l < NFile.M; l++)
                        {
                            //水平相邻
                            if (1 == Math.Abs(NFile.MapNodes[(j + 1) * 1000 + i + 1].X - NFile.MapNodes[(l + 1) * 1000 + k + 1].X)
                                && NFile.MapNodes[(j + 1) * 1000 + i + 1].Y == NFile.MapNodes[(l + 1) * 1000 + k + 1].Y)
                            {
                                NFile.MapNodes[(j + 1) * 1000 + i + 1].Siblings.Add(new Nodes()
                                {
                                    ID = NFile.MapNodes[(l + 1) * 1000 + k + 1].ID,
                                    X = NFile.MapNodes[(l + 1) * 1000 + k + 1].X,
                                    Y = NFile.MapNodes[(l + 1) * 1000 + k + 1].Y,
                                    Pose = new List<int>(new int[] { 0, 180 }),
                                });
                            }
                            //竖直相邻
                            else if (1 == Math.Abs(NFile.MapNodes[(j + 1) * 1000 + i + 1].Y - NFile.MapNodes[(l + 1) * 1000 + k + 1].Y)
                        && NFile.MapNodes[(j + 1) * 1000 + i + 1].X == NFile.MapNodes[(l + 1) * 1000 + k + 1].X)
                            {
                                NFile.MapNodes[(j + 1) * 1000 + i + 1].Siblings.Add(new Nodes()
                                {
                                    ID = NFile.MapNodes[(l + 1) * 1000 + k + 1].ID,
                                    X = NFile.MapNodes[(l + 1) * 1000 + k + 1].X,
                                    Y = NFile.MapNodes[(l + 1) * 1000 + k + 1].Y,
                                    Pose = new List<int>(new int[] { 90, 270 }),
                                });
                            }
                        }
                    }
                }
            }
        }
        private void DrawInputMap(MapFile IFile)
        {
            if (IFile.is_Open)
            {
                Canv.Children.Clear();
            }
            //站点绘制
            for (int i = 0; i < IFile.N; i++)
            {
                for (int j = 0; j < IFile.M; j++)
                {
                    drawNode(IFile.MapNodes[(j + 1) * 1000 + i + 1]);
                    inputNodeCount += IFile.MapNodes[(j + 1) * 1000 + i + 1].IsPosable ? 1 : -1;
                }
            }
            //绘制路径
            for (int i = 0; i < IFile.N; i++)
            {
                for (int j = 0; j < IFile.M - 1; j++)     //横向路径
                {
                    if (IFile.MapRoads[1000000 + IFile.MapNodes[(j + 1) * 1000 + i + 1].X * 1000
                        + IFile.MapNodes[(j + 1) * 1000 + i + 1].Y].Direc != Roads.Direction.Disable)
                    {
                        drawDirec(IFile.MapRoads[1000000 + IFile.MapNodes[(j + 1) * 1000 + i + 1].X * 1000
                        + IFile.MapNodes[(j + 1) * 1000 + i + 1].Y]);
                        inputRoadCount++;
                    }
                    drawRoad(IFile.MapRoads[1000000 + IFile.MapNodes[(j + 1) * 1000 + i + 1].X * 1000
                        + IFile.MapNodes[(j + 1) * 1000 + i + 1].Y]);
                }
            }
            for (int i = 0; i < IFile.M; i++)
            {
                for (int j = 0; j < IFile.N - 1; j++)     //纵向路径
                {
                    if (IFile.MapRoads[2000000 + IFile.MapNodes[(i + 1) * 1000 + j + 1].X * 1000
                        + IFile.MapNodes[(i + 1) * 1000 + j + 1].Y].Direc != Roads.Direction.Disable)
                    {
                        drawDirec(IFile.MapRoads[2000000 + IFile.MapNodes[(i + 1) * 1000 + j + 1].X * 1000
                        + IFile.MapNodes[(i + 1) * 1000 + j + 1].Y]);
                        inputRoadCount++;
                    }
                    drawRoad(IFile.MapRoads[2000000 + IFile.MapNodes[(i + 1) * 1000 + j + 1].X * 1000
                        + IFile.MapNodes[(i + 1) * 1000 + j + 1].Y]);
                }
            }
            NodeCount.Content = inputNodeCount.ToString();
            RoadCount.Content = inputRoadCount.ToString();
        }
        private void drawNode(Nodes node)
        {
            Rectangle myNode = new Rectangle
            {
                Tag = node.ID,
                Height = NodeEleHeight,
                Width = NodeEleWidth,
                StrokeThickness = NodeEleHeight / 2
            };
            mFile.NodeRect.Add(node.ID, myNode);
            Color color = Colors.WhiteSmoke;
            if (node.IsPosable)
            {
                if (node.Dockable)
                {
                    if (node.ROLE == Nodes.Role.Site)
                    {
                        color = Colors.LightSalmon;
                    }
                    else if (node.ROLE == Nodes.Role.Normal)
                    {
                        color = Colors.Goldenrod;
                    }
                }
                else
                {
                    color = Colors.Moccasin;
                }
            }
            myNode.Margin = new Thickness((int)node.x - NodeEleWidth / 2,
                (int)node.y - NodeEleHeight / 2, 0, 0);
            myNode.Stroke = new SolidColorBrush(color);
            singleCheck = new List<CheckBox>();
            myNode.MouseLeftButtonDown += Node_MouseLeftButtonDown;
            myNode.MouseEnter += Node_MouseEnter;
            Panel.SetZIndex(myNode, 2);
            myNode.ToolTip = $"{"ID:" + node.ID + "\n" + "X:" + node.X + "\n"}" +
                $"{"Y:" + node.Y + "\n" + "QR:" + node.QR + "\n" + "Sta:" + node.IsPosable}";
            Canv.Children.Add(myNode);
        }
        private void drawRoad(Roads road)
        {
            PathFigure myPathfigure = new PathFigure();
            PathGeometry myPathGeometry = new PathGeometry();
            Path myRoad = new Path();
            myPathfigure.StartPoint = new Point(road.node1.x, road.node1.y);
            myRoad.Tag = road.id;
            Color color = road.Direc == Roads.Direction.Disable ? Colors.WhiteSmoke : Colors.PaleTurquoise;
            myPathfigure.Segments.Add(
                new LineSegment(
                    new Point(road.node2.x, road.node2.y),
                    true));
            myPathGeometry.Figures.Add(myPathfigure);
            myRoad.Stroke = new SolidColorBrush(color);
            myRoad.StrokeThickness = RoadStrokeThickness;
            myRoad.Data = myPathGeometry;
            myRoad.MouseLeftButtonDown += Road_MouseLeftButtonDown;
            myRoad.MouseEnter += Road_MouseEnter;
            myRoad.ToolTip = $"{road.id}";
            Canv.Children.Add(myRoad);
            Panel.SetZIndex(myRoad, 1);
        }
        private void HightLightEle(Nodes node)
        {
            hightlightEle = new Rectangle
            {
                Height = 10,
                Width = 10,
                StrokeThickness = 0.6,
                Stroke = Brushes.Red,
                Margin = new Thickness(node.x - 5, node.y - 5, 0, 0),
            };
            Panel.SetZIndex(hightlightEle, 20);
            Canv.Children.Add(hightlightEle);
        }
        private void drawDirec(Roads roads)
        {
            int x1, y1, x2, y2, x3, y3;
            int x, y;
            int direcW = 3;
            int direcS = 4, direcE = 6;
            x = (roads.node1.x + roads.node2.x) / 2;        //中点
            y = (roads.node1.y + roads.node2.y) / 2;
            if (roads.node1.X < roads.node2.X && roads.node1.Y == roads.node2.Y)
            {
                if (roads.Direc == Roads.Direction.Single1)
                {
                    x1 = x - direcS;
                    y1 = y + direcW;
                    x2 = x - direcS;
                    y2 = y - direcW;
                    x3 = x + direcE;
                    y3 = y;
                    Direc(x1, y1, x2, y2, x3, y3, roads);
                }
                else if (roads.Direc == Roads.Direction.Single2)
                {
                    x1 = x + direcS;
                    y1 = y + direcW;
                    x2 = x + direcS;
                    y2 = y - direcW;
                    x3 = x - direcE;
                    y3 = y;
                    Direc(x1, y1, x2, y2, x3, y3, roads);
                }
            }
            else if (roads.node1.X > roads.node2.X && roads.node1.Y == roads.node2.Y)
            {
                if (roads.Direc == Roads.Direction.Single1)
                {
                    x1 = x + direcS;
                    y1 = y + direcW;
                    x2 = x + direcS;
                    y2 = y - direcW;
                    x3 = x - direcE;
                    y3 = y;
                    Direc(x1, y1, x2, y2, x3, y3, roads);
                }
                else if (roads.Direc == Roads.Direction.Single2)
                {
                    x1 = x - direcS;
                    y1 = y + direcW;
                    x2 = x - direcS;
                    y2 = y - direcW;
                    x3 = x + direcE;
                    y3 = y;
                    Direc(x1, y1, x2, y2, x3, y3, roads);
                }
            }
            else if (roads.node1.X == roads.node2.X && roads.node1.Y < roads.node2.Y)
            {
                if (roads.Direc == Roads.Direction.Single1)
                {
                    x1 = x - direcW;
                    y1 = y + direcS;
                    x2 = x + direcW;
                    y2 = y + direcS;
                    x3 = x;
                    y3 = y - direcE;
                    Direc(x1, y1, x2, y2, x3, y3, roads);
                }
                else if (roads.Direc == Roads.Direction.Single2)
                {
                    x1 = x - direcW;
                    y1 = y - direcS;
                    x2 = x + direcW;
                    y2 = y - direcS;
                    x3 = x;
                    y3 = y + direcE;
                    Direc(x1, y1, x2, y2, x3, y3, roads);
                }
            }
            else if (roads.node1.X == roads.node2.X && roads.node1.Y > roads.node2.Y)
            {
                if (roads.Direc == Roads.Direction.Single1)
                {
                    x1 = x - direcW;
                    y1 = y + direcS;
                    x2 = x + direcW;
                    y2 = y + direcS;
                    x3 = x;
                    y3 = y - direcE;
                    Direc(x1, y1, x2, y2, x3, y3, roads);
                }
                else if (roads.Direc == Roads.Direction.Single2)
                {
                    x1 = x - direcW;
                    y1 = y - direcS;
                    x2 = x + direcW;
                    y2 = y - direcS;
                    x3 = x;
                    y3 = y + direcE;
                    Direc(x1, y1, x2, y2, x3, y3, roads);
                }
            }
        }
        private void Direc(int x1, int y1, int x2, int y2, int x3, int y3, Roads roads)
        {
            if (mFile.DirPolygon.ContainsKey(roads.id))
            {
                Canv.Children.Remove(mFile.DirPolygon[roads.id]);
                mFile.DirPolygon.Remove(roads.id);
            }
            Polygon myPolygon = new Polygon
            {
                Tag = roads.id,
                Stroke = Brushes.Green,
                StrokeThickness = 0.2
            };
            Point Point1 = new Point(x1, y1);
            Point Point2 = new Point(x2, y2);
            Point Point3 = new Point(x3, y3);       //箭头点
            PointCollection myPointCollection = new PointCollection
            {
                Point1,
                Point2,
                Point3
            };
            myPolygon.Points = myPointCollection;
            mFile.DirPolygon.Add(roads.id, myPolygon);
            Panel.SetZIndex(myPolygon, 6);
            Canv.Children.Add(myPolygon);
        }
        private void ExtenorReduce(MapFile ERfile)
        {
            if (ERfile.M < ERfile.m || ERfile.N < ERfile.n)
            {
                Extend(ERfile);
            }
            else if (ERfile.M > ERfile.m || ERfile.N > ERfile.n)
            {
                Reduce(ERfile);
            }
        }
        private void Extend(MapFile ERfile)
        {
            //扩充node
            AddNode(0, ERfile.N, ERfile.M, ERfile.m, ERfile);
            AddNode(ERfile.N, ERfile.n, 0, ERfile.M, ERfile);
            AddNode(ERfile.N, ERfile.n, ERfile.M, ERfile.m, ERfile);
            //扩充邻居点
            AddSib(0, ERfile.N, ERfile.M, ERfile.m, 0, ERfile.N, ERfile.M - 1, ERfile.m, ERfile);
            AddSib(ERfile.N, ERfile.n, 0, ERfile.M, ERfile.N - 1, ERfile.n, 0, ERfile.M, ERfile);
            AddSib(ERfile.N, ERfile.n, ERfile.M, ERfile.m, ERfile.N - 1, ERfile.n, ERfile.M - 1, ERfile.m, ERfile);
            //扩充路径            
            AddRoad(0, ERfile.N, ERfile.M - 1, ERfile.m - 1, ERfile.M, ERfile.m, 0, ERfile.N - 1, ERfile);
            AddRoad(ERfile.N, ERfile.n, 0, ERfile.M - 1, 0, ERfile.M, ERfile.N - 1, ERfile.n - 1, ERfile);
            AddRoad(ERfile.N, ERfile.n, ERfile.M - 1, ERfile.m - 1, ERfile.M, ERfile.m, ERfile.N - 1, ERfile.n - 1, ERfile);
        }
        private void Reduce(MapFile ERfile)
        {

        }
        private void AddNode(int s1, int e1, int s2, int e2, MapFile ERfile)
        {
            for (int i = s1; i < e1; i++)       //M+1->m  0->N
            {
                for (int j = s2; j < e2; j++)
                {
                    Nodes node = new Nodes
                    {
                        X = j + 1,      //真实坐标
                        Y = i + 1,
                        x = j * scaX,    //显示坐标
                        y = (int)Canv.Height - i * scaY,
                        QR = ERfile.NewQR++,
                        ID = (j + 1) * 1000 + i + 1,
                        IsPosable = true,
                        Pose = new List<int>(),
                        Siblings = new List<Nodes>(),
                        ROLE = Nodes.Role.Normal,
                    };
                    node.Dockable = node.ROLE != Nodes.Role.Normal;
                    ERfile.MapNodes.Add(node.ID, node);
                    drawNode(node);
                }
            }
        }
        private void AddSib(int s1, int e1, int s2, int e2, int s3, int e3, int s4, int e4, MapFile ERFile)
        {
            //获取站点邻居站点
            for (int i = s1; i < e1; i++)           //外层循环
            {
                for (int j = s2; j < e2; j++)
                {
                    for (int k = s3; k < e3; k++)   //内层循环
                    {
                        for (int l = s4; l < e4; l++)
                        {
                            //水平相邻
                            if (1 == Math.Abs(ERFile.MapNodes[(j + 1) * 1000 + i + 1].X - ERFile.MapNodes[(l + 1) * 1000 + k + 1].X)
                                && ERFile.MapNodes[(j + 1) * 1000 + i + 1].Y == ERFile.MapNodes[(l + 1) * 1000 + k + 1].Y)
                            {
                                ERFile.MapNodes[(j + 1) * 1000 + i + 1].Siblings.Add(new Nodes()
                                {
                                    ID = ERFile.MapNodes[(l + 1) * 1000 + k + 1].ID,
                                    X = ERFile.MapNodes[(l + 1) * 1000 + k + 1].X,
                                    Y = ERFile.MapNodes[(l + 1) * 1000 + k + 1].Y,
                                    Pose = new List<int>(new int[] { 0, 180 }),
                                });
                            }
                            //竖直相邻
                            else if (1 == Math.Abs(ERFile.MapNodes[(j + 1) * 1000 + i + 1].Y - ERFile.MapNodes[(l + 1) * 1000 + k + 1].Y)
                        && ERFile.MapNodes[(j + 1) * 1000 + i + 1].X == ERFile.MapNodes[(l + 1) * 1000 + k + 1].X)
                            {
                                ERFile.MapNodes[(j + 1) * 1000 + i + 1].Siblings.Add(new Nodes()
                                {
                                    ID = ERFile.MapNodes[(l + 1) * 1000 + k + 1].ID,
                                    X = ERFile.MapNodes[(l + 1) * 1000 + k + 1].X,
                                    Y = ERFile.MapNodes[(l + 1) * 1000 + k + 1].Y,
                                    Pose = new List<int>(new int[] { 90, 270 }),
                                });
                            }
                        }
                    }
                }
            }
        }
        private void AddRoad(int s1, int e1, int s2, int e2, int s3, int e3, int s4, int e4, MapFile ERfile)
        {
            for (int i = s1; i < e1; i++)
            {
                for (int j = s2; j < e2; j++)     //横向路径
                {
                    Roads road = new Roads
                    {
                        node1 = ERfile.MapNodes[(j + 1) * 1000 + i + 1],
                        node2 = ERfile.MapNodes[(j + 2) * 1000 + i + 1],
                        Direc = Roads.Direction.Single1,
                    };
                    road.id = 1 * 1000000 + road.node1.X * 1000 + road.node1.Y;
                    ERfile.MapRoads.Add(road.id, road);
                    drawRoad(road);
                    drawDirec(road);
                }
            }
            for (int i = s3; i < e3; i++)
            {
                for (int j = s4; j < e4; j++)     //纵向路径
                {
                    Roads road = new Roads
                    {
                        node1 = ERfile.MapNodes[(i + 1) * 1000 + j + 1],
                        node2 = ERfile.MapNodes[(i + 1) * 1000 + j + 2],
                        Direc = Roads.Direction.Single2
                    };
                    road.id = 2 * 1000000 + road.node1.X * 1000 + road.node1.Y;
                    ERfile.MapRoads.Add(road.id, road);
                    drawRoad(road);
                    drawDirec(road);
                }
            }
        }
        #endregion
        #region 路由
        //Reset canvas's locatioin
        public static RoutedUICommand CanvRS =
            new RoutedUICommand("Canv Move to Left", "CanvRS", typeof(Editor));
        public void CanvRS_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        public void CanvRS_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            CanvM.Reset(Canv);
        }
        //Canvas move to left
        public static RoutedUICommand CanvML =
            new RoutedUICommand("Canv Move to Left", "CanvML", typeof(Editor));
        public void CanvML_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        public void CanvML_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            CanvM.CML(Canv);
        }
        //Canvas move to right
        public static RoutedUICommand CanvMR =
            new RoutedUICommand("Canv Move to right", "CanvMR", typeof(Editor));
        private void CanvMR_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void CanvMR_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            CanvM.CMR(Canv);
        }
        //Canvas move to up
        public static RoutedUICommand CanvMU =
            new RoutedUICommand("Canv move to up", "CanvMU", typeof(Editor));
        private void CanvMU_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void CanvMU_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            CanvM.CMU(Canv);
        }
        //Canvas move to down
        public static RoutedUICommand CanvMD =
            new RoutedUICommand("Canv move to up", "CanvMD", typeof(Editor));
        private void CanvMD_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void CanvMD_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            CanvM.CMD(Canv);
        }
        #endregion
    }
}
