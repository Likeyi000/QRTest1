using System.IO;
using System.Xml.Linq;

namespace QRMapEditor
{
    class NewMap
    {
        private readonly int QR = 1;
        public void NewFile(MapFile file)
        {
            file.MapNodes.Clear();
            file.MapRoads.Clear();
            file.DirPolygon.Clear();
            file.NodeRect.Clear();
            File.Copy(file.OldPath, file.NewPath);        //拷贝模板文件作为初始文件
            GetConfigData(file);
            GetModelData(file);
        }
        //获取地图文件数据M，N
        private void GetModelData(MapFile file)
        {
            file.XmlDoc = XDocument.Load(file.NewPath);
            XElement rootEle = file.XmlDoc.Element("map").Element("model").Element("nodes");
        }
        //获取配置文件原点
        private void GetConfigData(MapFile file)
        {
            file.ConDoc = XDocument.Load(file.ConPath);
            XElement rootEle = file.ConDoc.Element("map").Element("model").Element("nodes");
            file.Unit = int.Parse(rootEle.Attribute("unit").Value);
            file.M = int.Parse(rootEle.Attribute("m").Value);
            file.N = int.Parse(rootEle.Attribute("n").Value);
            file.NewQR = QR;
        }
    }
}
