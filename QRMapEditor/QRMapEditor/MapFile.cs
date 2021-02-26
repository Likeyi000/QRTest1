using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace QRMapEditor
{
    class MapFile
    {
        //通用数据
        public Dictionary<int, Nodes> MapNodes = new Dictionary<int, Nodes>();
        public Dictionary<int, Roads> MapRoads = new Dictionary<int, Roads>();
        public XDocument XmlDoc = new XDocument();
        public XDocument ConDoc = new XDocument();
        public bool is_Open = false;
        public bool is_Save = false;
        public string NewPath { set; get; }
        public string OldPath { set; get; }
        public string ConPath { set; get; }
        public int M { set; get; }
        public int N { set; get; }
        public int m { set; get; }
        public int n { set; get; }
        public int Unit { set; get; }

        //Scale data
        public int ScaX { set; get; }
        public int ScaY { set; get; }
        //New model
        public int NewQR { set; get; }

        //Other data
        public Dictionary<int, Polygon> DirPolygon = new Dictionary<int, Polygon>();
        public Dictionary<int, Rectangle> NodeRect = new Dictionary<int, Rectangle>();

        //Draw path
        public Dictionary<int, Polyline> myPolyline = new Dictionary<int, Polyline>();
        public List<SolidColorBrush> listColor = new List<SolidColorBrush>()
        {
            Brushes.Aqua,
            Brushes.Aquamarine,
            Brushes.CadetBlue,
            Brushes.CornflowerBlue,
            Brushes.DarkCyan,
            Brushes.DarkOliveGreen,
            Brushes.DarkSlateBlue,
            Brushes.DarkTurquoise,
            Brushes.DeepSkyBlue,
            Brushes.ForestGreen,
            Brushes.LightCyan,
            Brushes.LightGreen,
            Brushes.MediumAquamarine,
            Brushes.MediumPurple,
            Brushes.MintCream,
            Brushes.PaleTurquoise,
            Brushes.Teal
        };
    }
}
