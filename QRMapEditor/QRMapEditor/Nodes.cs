using System.Collections.Generic;

namespace QRMapEditor
{
    class Nodes
    {
        public int X { set; get; }              //真实坐标，大写
        public int x { set; get; }              //用于画布显示，小写
        public int Y { set; get; }              //Y坐标
        public int y { set; get; }
        public int QR { set; get; }             //二维标识
        public int ID { set; get; }             //标识
        public bool IsPosable { set; get; } = false;     //是否可用
        public bool Dockable { set; get; } = false;     //是否可停靠

        //站点角度
        public List<int> Pose { set; get; } = new List<int>();

        //邻接站点链表
        public List<Nodes> Siblings { set; get; } = new List<Nodes>();

        public Role ROLE;
        public enum Role
        {
            Normal = 0,
            Site = 1,
            Charger = 2,
        }


    }
}
