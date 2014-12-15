using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LandResources
{
    public class Metadata
    {
        public string name;         //文件名称
        public string path;         //文件全路径
        public string relativePath; //相对路径
        public string extention;    //拓展名，用于显示图标
        public string group;        //分组名称，用于navigation的分组
    }

    public class FileGroup
    {
        public string name;
        public List<string> list;
    }
}
