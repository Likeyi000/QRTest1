using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace QRMapEditor
{
    class InputMap
    {
        public void InputM(MapFile file)
        {
            file.is_Open = false;
            inputData(file);
        }
        private void inputData(MapFile file)
        {
            file.MapRoads.Clear();
            file.MapNodes.Clear();
            file.DirPolygon.Clear();
            file.NodeRect.Clear();
            file.is_Open = true;
            file.XmlDoc = XDocument.Load(file.NewPath);
            if (file.XmlDoc.Elements("map").Elements("model").Elements("nodes").Descendants("node").Count() > 0 &&
                file.XmlDoc.Element("map").Element("model").Element("roads").Descendants("road").Count() > 0)
            {
                XElement rootEle = file.XmlDoc.Element("map").Element("model").Element("nodes");
                XElement roadEle = file.XmlDoc.Element("map").Element("model").Element("roads");
                file.M = int.Parse(rootEle.Attribute("m").Value);
                file.N = int.Parse(rootEle.Attribute("n").Value);
                file.Unit = int.Parse(rootEle.Attribute("unit").Value);

                foreach (XElement ele in rootEle.Elements("node"))
                {
                    List<Nodes> nodlis = new List<Nodes>();         //邻居节点链表
                    List<int> poselist = new List<int>();           //Pose链表
                    Nodes node = new Nodes
                    {
                        ID = int.Parse(ele.Attribute("id").Value),
                        X = int.Parse(ele.Attribute("x").Value),
                        Y = int.Parse(ele.Attribute("y").Value),
                        x = (int.Parse(ele.Attribute("x").Value) - 1) * file.ScaX,
                        y = 800 - (int.Parse(ele.Attribute("y").Value) - 1) * file.ScaY,
                        QR = int.Parse(ele.Attribute("qr").Value),
                        ROLE = (Nodes.Role)System.Enum.Parse(typeof(Nodes.Role), ele.Attribute("role").Value),
                        IsPosable = bool.Parse(ele.Attribute("enabled").Value),
                        Dockable = bool.Parse(ele.Attribute("dockable").Value),
                    };

                    foreach (XElement childele in ele.Elements("sibling"))
                    {
                        Nodes nod = new Nodes
                        {
                            ID = int.Parse(childele.Attribute("id").Value),
                            X = int.Parse(childele.Attribute("x").Value),
                            Y = int.Parse(childele.Attribute("y").Value),
                            x = (int.Parse(childele.Attribute("x").Value) - 1) * file.ScaX,
                            y = 800 - (int.Parse(childele.Attribute("y").Value) - 1) * file.ScaY
                        };
                        poselist = Array.ConvertAll<string, int>(Regex.Split(childele.Attribute("pose").Value.ToString(),
        ",", RegexOptions.IgnoreCase), int.Parse).ToList();
                        nod.Pose = poselist;
                        nodlis.Add(nod);
                    }
                    node.Siblings = nodlis;
                    file.MapNodes.Add(node.ID, node);               //导入的站点数据
                }

                foreach (XElement ele in roadEle.Elements("road"))      //获取路径信息
                {
                    Roads road = new Roads
                    {
                        id = int.Parse(ele.Attribute("id").Value),
                        node1 = new Nodes
                        {
                            X = file.MapNodes[int.Parse(ele.Attribute("node1").Value)].X,
                            Y = file.MapNodes[int.Parse(ele.Attribute("node1").Value)].Y,
                            x = file.MapNodes[int.Parse(ele.Attribute("node1").Value)].x,
                            y = file.MapNodes[int.Parse(ele.Attribute("node1").Value)].y,
                            ID = file.MapNodes[int.Parse(ele.Attribute("node1").Value)].ID,
                            ROLE = file.MapNodes[int.Parse(ele.Attribute("node1").Value)].ROLE,
                            IsPosable = file.MapNodes[int.Parse(ele.Attribute("node1").Value)].IsPosable,
                        },
                        node2 = new Nodes
                        {
                            X = file.MapNodes[int.Parse(ele.Attribute("node2").Value)].X,
                            Y = file.MapNodes[int.Parse(ele.Attribute("node2").Value)].Y,
                            x = file.MapNodes[int.Parse(ele.Attribute("node2").Value)].x,
                            y = file.MapNodes[int.Parse(ele.Attribute("node2").Value)].y,
                            ID = file.MapNodes[int.Parse(ele.Attribute("node2").Value)].ID,
                            ROLE = file.MapNodes[int.Parse(ele.Attribute("node2").Value)].ROLE,
                            IsPosable = file.MapNodes[int.Parse(ele.Attribute("node2").Value)].IsPosable,
                        },
                        Direc = (Roads.Direction)int.Parse(ele.Attribute("direc").Value)
                    };
                    file.MapRoads.Add(road.id, road);
                }
            }
            else
            {
                MessageBox.Show("文件中不存在站点或路径信息，请核查。");
            }
        }
    }
}
