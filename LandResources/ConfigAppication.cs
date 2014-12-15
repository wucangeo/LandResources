using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//custom
using System.Windows.Forms; //MessageBoxa
using System.Drawing;       //Image,Bitmap
using System.IO;            //File,Directory
using System.Collections;   //HashMap
using System.Data;          //Datatable

using DevExpress.Utils;
using DevExpress.XtraBars;  
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraNavBar;
using DevExpress.XtraTreeList;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;   //AccessMDB


namespace LandResources {
    public class ConfigAppication
    {
        /// <summary>
        /// 取得本系统数据配置路径
        /// </summary>
        /// <returns>数据全路径</returns>
        public static string getAppDataPath()
        {
            string dataItemPath = "";  //配置文件路径
            try
            {
                //软件的注册表项是否存在
                bool appRegExist = ConfigRegister.IsAppRegeditExit();
                if (!appRegExist)
                {
                    ConfigRegister.WriteAppReg();
                }

                //判断软件配置文件的注册表项是否存在
                bool configRegExist = ConfigRegister.IsRegeditExit();
                if (configRegExist)
                {
                    dataItemPath = ConfigRegister.GetConfigReg();
                }
                else
                {
                    if (MessageBox.Show("请为本软件指定配置文件夹，方便下次使用。", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.OK)
                    {
                        FolderBrowserDialog fbd = new FolderBrowserDialog();
                        fbd.Description = "指定配置文件夹";
                        fbd.RootFolder = Environment.SpecialFolder.Desktop;
                        fbd.SelectedPath = Application.StartupPath;
                        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            dataItemPath = fbd.SelectedPath;
                        }
                    }
                }
                setAppConfigPath(dataItemPath); //设置数据目录
            }
            catch
            {
            }
            return dataItemPath;
        }

        /// <summary>
        /// 设置系统数据目录，并写入注册表
        /// </summary>
        /// <param name="dataItemPath">系统数据路径</param>
        /// <returns>注册表是否写入成功</returns>

        public static bool setAppConfigPath(string dataItemPath)
        {
            bool isSucc = true;
            try
            {
                //判断指定配置文件是否存在
                if (Directory.Exists(dataItemPath))
                {
                    ConfigRegister.WriteConfigReg(dataItemPath);   //写入
                }
                else
                {
                    if (MessageBox.Show("本软件配置文件夹不存在，是否重新指定？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                    {
                        FolderBrowserDialog fbd = new FolderBrowserDialog();
                        fbd.Description = "指定配置文件夹";
                        fbd.RootFolder = Environment.SpecialFolder.Desktop;
                        fbd.SelectedPath = Application.StartupPath;
                        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            dataItemPath = fbd.SelectedPath;
                            ConfigRegister.WriteConfigReg(dataItemPath);   //写入
                        }
                    }
                }
            }
            catch
            {
                isSucc = false;
            }
            return isSucc;

        }

        /// <summary>
        /// 获取系统配置图片
        /// </summary>
        /// <param name="imagesFolder">系统配置文件夹</param>
        /// <returns>图片集</returns>
        public static ImageCollection setImageCollection(string imagesFolder, out List<string> imageNameList)
        {
            ImageCollection ic = new ImageCollection();
            ic.ImageSize = new Size(100, 75);
            imageNameList = new List<string>();
            try
            {
                if (!Directory.Exists(imagesFolder))
                {
                    MessageBox.Show("系统配置路径没有设置，请重新设置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }
                DirectoryInfo di = new DirectoryInfo(imagesFolder);
                FileInfo[] fileList = di.GetFiles();
                for (int index = 0; index < fileList.GetLength(0); index++)
                {
                    FileInfo fi = fileList[index];
                    if (fi is FileInfo)
                    {
                        if (fi.Extension == ".png" || fi.Extension == ".jpg")
                        {
                            string name = Path.GetFileNameWithoutExtension(fi.Name);    //取得文件名
                            Bitmap bm = new Bitmap(fi.FullName);
                            Image image = Image.FromFile(fi.FullName);   //取得图片
                            imageNameList.Add(name);    //加入imageNameList
                            ic.AddImage(image, index.ToString());    //加入collection
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return ic;
        }

        /// <summary>
        /// 取得数据目录列表
        /// </summary>
        /// <param name="dataFolderPath">数据总目录</param>
        /// <returns>目录list</returns>
        public static List<string> getDataDirectoryList(string dataFolderPath, out List<string> dataFolderNameList)
        {
            dataFolderNameList = new List<string>();
            List<string> dataList = new List<string>();
            try
            {
                if (!Directory.Exists(dataFolderPath))
                {
                    MessageBox.Show("系统配置路径没有设置，请重新设置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return dataList;
                }
                DirectoryInfo diDataPath = new DirectoryInfo(dataFolderPath);
                DirectoryInfo[] subDiList = diDataPath.GetDirectories();
                foreach (DirectoryInfo di in subDiList)
                {
                    string diName = di.Name;
                    string numStr = diName.Substring(0, 2);
                    int num = -1;
                    if (int.TryParse(numStr, out num))
                    {
                        dataList.Add(di.FullName);          //添加路径
                        dataFolderNameList.Add(di.Name);    //添加目录名称list
                    }
                }
            }
            catch
            {
                return dataList;
            }
            return dataList;
        }

        /// <summary>
        /// 为主页中的数据目录创建浏览项
        /// </summary>
        /// <param name="image">图片</param>
        /// <param name="index">索引</param>
        /// <param name="caption">标题</param>
        /// <param name="hint">tips指示</param>
        /// <returns>GalleryItem</returns>
        public static GalleryItem CreateGalleryItem(Image image, int index, string caption, string hint)
        {
            GalleryItem item = new GalleryItem();
            item.HoverImageIndex = item.ImageIndex = index;
            item.Caption = caption.Substring(2, caption.Length - 2);
            //item.Image = image;
            //item.HoverImage = image;
            //item.Tag = info;
            item.Hint = hint;
            return item;
        }

        public static void AlterDataGalleryItemCaption(RibbonGalleryBarItem gallery, List<string> nameList, bool isExpand)
        {
            try
            {
                for (int index = 0; index < gallery.Gallery.Groups[0].Items.Count; index++)
                {
                    if (index >= nameList.Count)
                    {
                        break;  //如果galleryItem比namelist多，就退出
                    }
                    GalleryItem gi = gallery.Gallery.Groups[0].Items[index];
                    string name = nameList[index];
                    DirectoryInfo di = new DirectoryInfo(name);
                    if (di.Exists)  //如果目录存在，则用目录名称
                    {
                        name = di.Name;
                    }
                    gi.Caption = name;  //修改标签名称
                    if (name.Length > 10)
                    {
                        gi.Caption = name.Substring(0, 10) + ".."; //如果名称过长，则处理长度
                    }
                    if (!isExpand)
                    {
                        gi.Caption = name.Substring(2, name.Length - 2);
                    }
                }
            }
            catch
            {
            }
        }

        public static List<Metadata> getAllDataSpecifyFolder(string path, Hashtable group)
        {
            List<Metadata> fileList = new List<Metadata>();
            try
            {
                List<Metadata> list = new List<Metadata>();
                FileSystemInfo fsInfo = new DirectoryInfo(path);

                ListFiles(fsInfo, list, group, path);
                //排序
                fileList = (from c in list
                            orderby c.name ascending
                            select c).ToList<Metadata>();
            }
            catch
            {
            }
            return fileList;
        }

        //遍历文件
        public static void ListFiles(FileSystemInfo info, List<Metadata> list, Hashtable group, string rootPath)
        {
            try
            {
                if (!info.Exists)
                    return;
                DirectoryInfo dir = info as DirectoryInfo;
                //不是目录
                if (dir == null)
                    return;
                FileSystemInfo[] files = dir.GetFileSystemInfos();
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i] as FileInfo;
                    //是文件
                    if (file != null)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file.FullName);  //文件名                          
                        string extension = file.Extension.ToLower();      //拓展名，包含“.”
                        string fullPath = file.FullName;    //全路径
                        string relativePath = fullPath.Substring(rootPath.Length, fullPath.Length - rootPath.Length); //取得相对路径

                        string groupName = (string)group[extension];    //获取分组名
                        if (groupName == "hide")
                        {
                            continue;   //如果为隐藏的数据，则继续下一次遍历
                        }
                        else if (groupName == null || groupName == "")
                        {
                            groupName = (string)group[".else"]; //如果格式未识别，则标识为未知格式
                        }

                        if (fileName.Length < 4)
                        {
                            string[] dirs = relativePath.Split('\\');
                            string lastDirName = "";
                            if (dirs.Length > 2)
                            {
                                lastDirName = dirs[dirs.Length - 2];
                                fileName = lastDirName + "\\" + fileName;
                            }
                        }


                        Metadata md = new Metadata();
                        md.name = fileName;             //文件名  
                        md.path = file.FullName;        //文件路径         
                        md.extention = extension;       //拓展名
                        md.group = groupName;           //分组名
                        md.relativePath = relativePath; //相对路径
                        list.Add(md);

                    }
                    //对于子目录，进行递归调用
                    else
                    {
                        ListFiles(files[i], list, group, rootPath);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 初始化数据分组
        /// </summary>
        /// <returns>数据分组hashTable</returns>
        public static Hashtable initFileGroup()
        {
            Hashtable ht = new Hashtable();
            try
            {
                string documentGroup = "document";   //文档数据
                string spatialGroup = "spatial";     //空间数据
                string imageGroup = "image";         //图片数据
                string dbGroup = "database";         //数据库数据
                string elseGroup = "else";           //其他类型数据
                string hideGroup = "hide";           //隐藏数据

                //分组名称
                ht.Add(documentGroup, "文档数据");
                ht.Add(spatialGroup, "空间数据");
                ht.Add(imageGroup, "图片数据");
                ht.Add(dbGroup, "关系数据集");
                ht.Add(elseGroup, "其他类型");
                ht.Add(hideGroup, "隐藏数据");

                //文档：
                ht.Add(".doc", documentGroup);
                ht.Add(".docx", documentGroup);
                ht.Add(".xls", documentGroup);
                ht.Add(".xlsx", documentGroup);
                ht.Add(".pdf", documentGroup);

                //图片：
                ht.Add(".jpg", imageGroup);
                ht.Add(".bmp", imageGroup);
                ht.Add(".png", imageGroup);
                ht.Add(".gif", imageGroup);

                //空间数据：
                ht.Add(".mxd", spatialGroup);   //esri地图
                ht.Add(".shp", spatialGroup);   //矢量
                ht.Add(".tif", spatialGroup);   //栅格
                ht.Add(".tiff", spatialGroup);  //栅格 
                ht.Add(".dwg", spatialGroup);  //CAD dwg

                //数据库：
                ht.Add(".mdb", dbGroup);    //空间数据库

                //其他未识别的格式
                ht.Add(".else", elseGroup);
                //不需要展示的：
                ht.Add(".xml", hideGroup);  //shp文件的记录
                ht.Add(".shx", hideGroup);  //图形信息
                ht.Add(".sbx", hideGroup);  //图形信息
                ht.Add(".sbn", hideGroup);  //图形信息
                ht.Add(".prj", hideGroup);  //投影
                ht.Add(".dbf", hideGroup);  //数据表文件
                ht.Add(".db", hideGroup);   //Thumb图片预览索引文件
            }
            catch
            {
            }

            return ht;
        }

        //创建NavBarControl中的NavBarItem
        public static NavBarItem createNavBarItem(Metadata md)
        {
            NavBarItem nbi = new NavBarItem(md.name);
            try
            {
                //图标
                Icon icon = Icon.ExtractAssociatedIcon(md.path);
                Image img = (Image)(new Bitmap(icon.ToBitmap() as Image, new Size(16, 16)));
                nbi.SmallImage = img;
                //nbi.Caption = md.name;
                nbi.Hint = md.relativePath;
                nbi.Tag = md.path;
            }
            catch
            {
            }
            finally
            {
                GC.Collect();
            }

            return nbi;
        }

        //创建GalleryControl中的GalleryItem
        public static GalleryItem createGalleryItem(Metadata md)
        {
            GalleryItem gi = null;
            try
            {
                //图标
                Icon icon = Icon.ExtractAssociatedIcon(md.path);
                Image img = (Image)(new Bitmap(icon.ToBitmap() as Image, new Size(64, 64)));
                //名称
                string giCaption = md.name;
                if (giCaption.Length > 8)
                {
                    giCaption = ".." + md.name.Substring(md.name.Length - 8, 8);
                }
                gi = new GalleryItem(img, giCaption, "");
                gi.Hint = md.relativePath;
                gi.Tag = md.path;
            }
            catch
            {
                Image img = global::LandResources.Properties.Resources.icon_else_64;
                gi = new GalleryItem(img, md.name, "");
                gi.Hint = "请在左侧查看更多";
                gi.Tag = "more";
            }
            return gi;
        }
        //重载函数，返回更多图标
        public static GalleryItem createGalleryItem(Metadata md, bool isTenMore)
        {
            GalleryItem gi = null;
            try
            {
                if (isTenMore)
                {
                    Image img = global::LandResources.Properties.Resources.home_more_64;
                    gi = new GalleryItem(img, "更多...", "");
                }
            }
            catch
            {
                Image img = global::LandResources.Properties.Resources.icon_else_64;
                gi = new GalleryItem(img, "更多...", "");
            }
            return gi;
        }
        //由路径查询到图片list的索引值
        public static int findIndexOfImageList(string path, List<Metadata> list)
        {
            int index = 0;
            try
            {
                index = list.FindIndex(
                    delegate(Metadata md)
                    {
                        return md.group == "image" && md.path.Equals(path);
                    });
            }
            catch
            {
            }
            return index;
        }

        //由图片生成生成缩略图
        public static Image getThumbImage(Image imageFrom, Size sizeTo)
        {
            int widthTo = sizeTo.Width;
            int heightTo = sizeTo.Height;
            int drawWidth = heightTo * imageFrom.Width / imageFrom.Height;
            int drawHeight = heightTo;
            int xStart = (widthTo - drawWidth) / 2;
            //处理输出图片大小

            // 创建画布
            Bitmap bmp = new Bitmap(widthTo, heightTo);
            Image imageTo = (Image)bmp;
            Graphics g = Graphics.FromImage(bmp);
            try
            {
                //设置高质量插值法
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                //设置高质量,低速度呈现平滑程度
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                //清空画布并以透明背景色填充
                g.Clear(System.Drawing.Color.Transparent);
                //在指定位置并且按指定大小绘制原图片的指定部分
                g.DrawImage(imageFrom, new Rectangle(xStart, 0, drawWidth, drawHeight),
                new Rectangle(0, 0, imageFrom.Width, imageFrom.Height),
                GraphicsUnit.Pixel);
                imageTo = (Image)bmp.Clone();
            }
            catch
            {
            }
            finally
            {
                //显示释放资源
                bmp.Dispose();
                g.Dispose();
                imageFrom.Dispose();
                GC.Collect();
            }
            return imageTo;
        }

        //打开空间数据
        public static void OpenSpatialData(FileInfo file, AxMapControl _mapControl)
        {
            try
            {
                _mapControl.ClearLayers();   //清空已显示的地图
                _mapControl.Map.ClearLayers();   //清空
                string extension = file.Extension.ToLower();
                if (extension == ".mxd")
                {
                    _mapControl.LoadMxFile(file.FullName);

                }
                else if (extension == ".shp")
                {
                    _mapControl.AddShapeFile(file.DirectoryName, file.Name);
                }
                else if (extension == ".tif" || extension == ".tiff")
                {
                    IWorkspaceFactory pWorkspaceFactory = new RasterWorkspaceFactoryClass();
                    IRasterWorkspace pRasterWorkspace = (IRasterWorkspace)pWorkspaceFactory.OpenFromFile(file.DirectoryName, 0);
                    IRasterDataset pRasterDataset = (IRasterDataset)pRasterWorkspace.OpenRasterDataset(file.Name);
                    IRasterLayer pRasterLayer = new RasterLayerClass();
                    pRasterLayer.CreateFromDataset(pRasterDataset);
                    _mapControl.Map.AddLayer(pRasterLayer);
                    _mapControl.ActiveView.Refresh();
                }
                else if (extension == ".dwg")
                {
                    IWorkspaceFactory pWorkspaceFactory = new CadWorkspaceFactoryClass();
                    IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(file.DirectoryName, 0);
                    //--定义一个CAD画图空间，并把上边打开的工作空间赋给它
                    ICadDrawingWorkspace pCadDrawingWorkspace = pWorkspace as ICadDrawingWorkspace;
                    //--定义一个CAD的画图数据集，并且打开上边指定的工作空间中一幅CAD图
                    //--然后赋值给CAD数据集
                    ICadDrawingDataset pCadDrawingDataset = pCadDrawingWorkspace.OpenCadDrawingDataset(file.Name);
                    //--通过ICadLayer类，把上边得到的CAD数据局赋值给ICadLayer类对象的
                    //--CadDrawingDataset属性
                    ICadLayer pCadLayer = new CadLayerClass();
                    pCadLayer.CadDrawingDataset = pCadDrawingDataset;
                    //--利用MapControl加载CAD层
                    _mapControl.Map.AddLayer(pCadLayer);
                }
            }
            catch
            {
            }
            finally
            {
                _mapControl.ActiveView.Refresh();
            }

        }

        //打开MDB数据库
        public static void OpenMDBFile(IWorkspace workspace, TreeList treeList)
        {
            try
            {

                //取得数据
                treeList.KeyFieldName = "KeyFieldName";
                treeList.ParentFieldName = "ParentFieldName";
                DataTable dataTable = GISServices.CreateTreeListTable(workspace);
                if (dataTable.Rows.Count == 0)
                {
                    return;
                }
                treeList.DataSource = dataTable;
                treeList.ExpandAll();
            }
            catch
            {
            }
        }

        //---------------------------文件操作------------------------
        //NavBar中的文件拷贝
        public static bool CopyFileTo(string orgPath, string destDir)
        {
            bool isCopySucc = true;
            try
            {
                if (!File.Exists(orgPath))
                {
                    MessageBox.Show("文件" + orgPath + "不存在，无法添加到数据库。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return isCopySucc = false;
                }
                //检查是否存在目的目录
                if (!Directory.Exists(destDir))
                {
                    MessageBox.Show("数据库目标目录" + orgPath + "不存在，无法添加到数据库。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return isCopySucc = false;
                }
                //先来移动文件
                string orgFileName = Path.GetFileName(orgPath);
                string destFilePath = Path.Combine(destDir, orgFileName);   //准备入库的目标文件路径
                if(File.Exists(destFilePath))
                {
                    if(MessageBox.Show("数据库目标目录中已存在文件'" + orgFileName + "'，要替换吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        File.Copy(orgPath, destFilePath, true); //复制文件
                    }
                    else
                    {
                        return isCopySucc = false;
                    }
                }
                else
                {
                    File.Copy(orgPath, destFilePath, false); //复制文件
                }
                
            }
            catch
            {
                isCopySucc = false;
            }
            return isCopySucc;
        }

        //删除文件
        public static bool DeleteFile(string path)
        {
            bool isDeleteSucc = true;
            try
            {
                File.Delete(path);
            }
            catch
            {
                isDeleteSucc = false;
            }
            return isDeleteSucc;
        }
    }
}
