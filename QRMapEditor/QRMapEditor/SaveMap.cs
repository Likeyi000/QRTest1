using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace QRMapEditor
{
    class SaveMap
    {
        public void SaveFile(MapFile file)
        {
            saveFile(file);
        }
        public void saveConf(MapFile file)
        {
            saveConfig(file);
        }

        private void saveFile(MapFile file)
        {
            XElement rootEle = file.XmlDoc.Element("map");
            IEnumerable<XElement> targetNodes = from target in rootEle.Descendants("node")
                                                select target;
            targetNodes.Remove();       //清空“node”数据
            targetNodes = from target in rootEle.Descendants("road")
                          select target;
            targetNodes.Remove();       //清空“road”数据

            XElement newNode1, newNode2, newRoad;
            for (int i = 0; i < file.N; i++)
            {
                for (int k = 0; k < file.M; k++)
                {
                    newNode1 = new XElement("node", new XAttribute("id", file.MapNodes[(k + 1) * 1000 + i + 1].ID),
                    new XAttribute("x", file.MapNodes[(k + 1) * 1000 + i + 1].X),
                    new XAttribute("y", file.MapNodes[(k + 1) * 1000 + i + 1].Y),
                    new XAttribute("qr", file.MapNodes[(k + 1) * 1000 + i + 1].QR),
                    new XAttribute("role", file.MapNodes[(k + 1) * 1000 + i + 1].ROLE),
                    new XAttribute("enabled", file.MapNodes[(k + 1) * 1000 + i + 1].IsPosable),
                    new XAttribute("dockable", file.MapNodes[(k + 1) * 1000 + i + 1].Dockable)
                    );
                    for (int j = 0; j < file.MapNodes[(k + 1) * 1000 + i + 1].Siblings.Count; j++)
                    {
                        if (file.MapNodes[(k + 1) * 1000 + i + 1].Siblings.Count > 0)
                        {
                            newNode2 = new XElement("sibling",
                                new XAttribute("id", file.MapNodes[(k + 1) * 1000 + i + 1].Siblings[j].ID),
                            new XAttribute("x", file.MapNodes[(k + 1) * 1000 + i + 1].Siblings[j].X),
                            new XAttribute("y", file.MapNodes[(k + 1) * 1000 + i + 1].Siblings[j].Y),
                            new XAttribute("pose", string.Join(",", file.MapNodes[(k + 1) * 1000 + i + 1].Siblings[j].Pose.ToArray())));     //转为整形数据，并用逗号隔开
                            newNode1.Add(newNode2);
                        }
                    }
                    rootEle.Element("model").Element("nodes").Add(newNode1);        //修改后的数据存储
                }
            }
            //路径信息
            for (int i = 0; i < file.N; i++)
            {
                for (int j = 0; j < file.M - 1; j++)     //横向路径
                {
                    newRoad = new XElement("road", new XAttribute("id", file.MapRoads[1000000 + file.MapNodes[(j + 1) * 1000 + i + 1].X * 1000
                        + file.MapNodes[(j + 1) * 1000 + i + 1].Y].id),
                    new XAttribute("node1", file.MapRoads[1000000 + file.MapNodes[(j + 1) * 1000 + i + 1].X * 1000
                        + file.MapNodes[(j + 1) * 1000 + i + 1].Y].node1.ID),
                    new XAttribute("node2", file.MapRoads[1000000 + file.MapNodes[(j + 1) * 1000 + i + 1].X * 1000
                        + file.MapNodes[(j + 1) * 1000 + i + 1].Y].node2.ID),
                    new XAttribute("direc", (int)file.MapRoads[1000000 + file.MapNodes[(j + 1) * 1000 + i + 1].X * 1000
                        + file.MapNodes[(j + 1) * 1000 + i + 1].Y].Direc));
                    rootEle.Element("model").Element("roads").Add(newRoad);
                }
            }
            for (int i = 0; i < file.M; i++)
            {
                for (int j = 0; j < file.N - 1; j++)     //纵向路径
                {
                    newRoad = new XElement("road", new XAttribute("id", file.MapRoads[2000000 + file.MapNodes[(i + 1) * 1000 + j + 1].X * 1000
                        + file.MapNodes[(i + 1) * 1000 + j + 1].Y].id),
                    new XAttribute("node1", file.MapRoads[2000000 + file.MapNodes[(i + 1) * 1000 + j + 1].X * 1000
                        + file.MapNodes[(i + 1) * 1000 + j + 1].Y].node1.ID),
                    new XAttribute("node2", file.MapRoads[2000000 + file.MapNodes[(i + 1) * 1000 + j + 1].X * 1000
                        + file.MapNodes[(i + 1) * 1000 + j + 1].Y].node2.ID),
                    new XAttribute("direc", (int)file.MapRoads[2000000 + file.MapNodes[(i + 1) * 1000 + j + 1].X * 1000
                        + file.MapNodes[(i + 1) * 1000 + j + 1].Y].Direc));
                rootEle.Element("model").Element("roads").Add(newRoad);
                }
            }

            rootEle.Element("model").Element("nodes").SetAttributeValue("m", file.m);
            rootEle.Element("model").Element("nodes").SetAttributeValue("n", file.n);

            rootEle.Save(file.NewPath);
        }
        private void saveConfig(MapFile file)
        {
            file.ConDoc = XDocument.Load(file.ConPath);
            XElement root = file.ConDoc.Element("map");
            root.Element("model").Element("nodes").SetAttributeValue("m", file.m);
            root.Element("model").Element("nodes").SetAttributeValue("n", file.n);
            root.Save(file.ConPath);
        }
    }
}
