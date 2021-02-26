using System.Collections.Generic;

namespace QRMapEditor
{
    class Roads
    {
        public Nodes node1 { set; get; } = new Nodes();        //路径站点1
        public Nodes node2 { set; get; } = new Nodes();        //路径站点2

        public int id { set; get; }     //路径id（唯一标识）
        public Direction Direc; 

        public enum Direction
        {
            Disable = 0,        //默认阻塞
            Single1 = 1,        //单向1(node1->node2)
            Single2 = 2,        //单向2(node2->node1)
            Both = 3,           //双向
        }

    }
}
