using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//cumstom system
using System.IO;                //文件读写
using Microsoft.Win32;          //注册表操作
using System.Collections;       //HashTable
using DevExpress.XtraBars.Ribbon;
using DevExpress.Utils;
using DevExpress.XtraNavBar;        //导航栏
using DevExpress.XtraTab;           //tabpage的操作
using DevExpress.XtraEditors.Controls;  //ImageSlider
using DevExpress.XtraTreeList;      //TreeList
using DevExpress.XtraTreeList.Nodes;        //TreeListNode

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;

namespace LandResources
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        //数据目录
        private string dataFolderPath = "";     //数据目录路径
        private List<string> dataFolderPathList = new List<string>();   //数据目录路径集合
        private List<string> dataFolderNotPathList = new List<string>();   //数据目录的名称，不包含路径
        //图片目录
        private string configFolderPath = "";   //系统配置文件夹路径        
        private List<string> imageNameList = null;  //配置图片的名称list
        //各种格式数据的list
        private List<Metadata> fileNavigationList = new List<Metadata>(); //当前导航栏里面的所有文件
        private List<Metadata> imageDataList = new List<Metadata>();        //本组中需要显示的所有图片list
        private Hashtable htFileGroup = new Hashtable();      //数据分组依据
        private string documentGroup = "document";   //文档数据
        private string spatialGroup = "spatial";     //空间数据
        private string imageGroup = "image";         //图片数据
        private string dbGroup = "database";         //数据库数据
        private string elseGroup = "else";           //其他类型数据
        //与navBarControl相关
        private NavBarGroup navGroup_Document;      //文档数据
        private NavBarGroup navGroup_Spatial;       //空间数据
        private NavBarGroup navGroup_Image;         //图片数据
        private NavBarGroup navGroup_MDB;           //空间数据库
        private NavBarGroup navGroup_Else;           //其他数据
        private NavBarItemLink focusedNavBarLink;   //当前右键点击的link
        //与HomePage相关
        private GalleryItemGroup galleryGroup_Documnet; //文档分组
        private GalleryItemGroup galleryGroup_Spatial;  //空间数据
        private GalleryItemGroup galleryGroup_Image;    //图片分组
        //private GalleryItemGroup galleryGroup_MDB;      //MDB数据库
        private GalleryItemGroup galleryGroup_Else;     //其他数据类型分组

        //与图片tab相关
        private int curImageIndex = 0;  //当前在GalleryControl中显示图片的index值，相对于imageDataList        
        private List<Metadata> imageListInGallery = new List<Metadata>();   //在GalleryControl中显示的图片metadataList
        private int checkedGalleryItemIndex = 0;    //当前GalleryControl中选中的为第几个图片
        private bool hasPictureEditUpdated = false; //标识pictureEdit是否已经更改完

        //与地图tab相关
        private GISIdentifyFrm identifyFrm = null;  //要素属性框
        private AxMapControl axMapControlCurrent;   //当前活动的MapControl控件，用以区分工具栏
        
        //与MDB的CatalogTab相关
        private IWorkspace workspace;
        private TreeListNode focusedCatalogNode = null;
        
        public MainForm()
        {
            InitializeComponent();
        }

        //主界面加载事件 
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                //启动界面
                StartFrm sf = new StartFrm(this);
                sf.ShowDialog();

                //汉化界面
                HanhuaUserInterface();

                dataFolderPath = ConfigAppication.getAppDataPath();
                configFolderPath = Application.StartupPath + "\\config";
                System.Console.WriteLine("配置文件路径：" + dataFolderPath);
                this.imageCollection_DataGallery.ImageSize = new Size(120, 90);

                //初始化数据分组
                htFileGroup = ConfigAppication.initFileGroup();

                //配置系统的数据
                ReadDataToConfigApp();

                //隐藏所有tabPages
                HideAllRibbonPageCategory();    //先隐藏所有pageCategory

                //初始化时加载界面数据文件图标
                if (dataFolderNotPathList.Count > 0 && dataFolderNotPathList.Count > 0)
                {
                    string path = dataFolderPathList[0];
                    string captionName = dataFolderNotPathList[0];
                    this.tab_Home.Text = "主页 - " + captionName;
                    this.galleryControl_Image.Gallery.Groups[0].Items.Clear();  //清空图片预览框
                    FillNavigationBarAndHomeGallery(path);
                }
            }
            catch
            {
            }
        }

        //汉化界面
        private void HanhuaUserInterface()
        {
            try
            {
                //DevExpress.XtraGrid.Localization.GridResLocalizer.Active = new Dxper.LocalizationCHS.Win.XtraGridCHS();
                //DevExpress.XtraEditors.Controls.Localizer.Active = new Dxper.LocalizationCHS.Win.XtraEditorsCHS();
                //DevExpress.XtraCharts.Localization.ChartResLocalizer.Active = new Dxper.LocalizationCHS.Win.XtraChartsCHS();
                //DevExpress.XtraBars.Localization.BarLocalizer.Active = new Dxper.LocalizationCHS.Win.XtraBarsCHS();
                //DevExpress.XtraLayout.Localization.LayoutLocalizer.Active = new Dxper.LocalizationCHS.Win.XtraLayoutCHS();
                //DevExpress.XtraPrinting.Localization.PreviewLocalizer.Active = new Dxper.LocalizationCHS.Win.XtraPrintingCHS();
                //DevExpress.XtraTreeList.Localization.TreeListResLocalizer.Active = new Dxper.LocalizationCHS.Win.XtraTreeListCHS();
                //DevExpress.Office.Localization.OfficeResLocalizer.Active = new Dxper.LocalizationCHS.Win.OfficeCHS();
                //DevExpress.XtraSpreadsheet.Localization.XtraSpreadsheetLocalizer.Active = new Dxper.LocalizationCHS.Win.XtraSpreadsheetCHS();
            }
            catch
            {
            }
        }
        
        //---------------------------------------------界面事件--------------------------------------------------
        
        //设置系统数据目录 
        private void barBtn_DataFolderConfig_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "指定配置文件夹";
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                fbd.SelectedPath = Application.StartupPath;
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dataFolderPath = fbd.SelectedPath;
                    bool isSucc = ConfigAppication.setAppConfigPath(dataFolderPath);
                    if (isSucc)
                    {
                        MessageBox.Show("设置系统数据目录成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
            }
        }

        //重读数据，刷新界面
        private void barButton_RereadData_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ReadDataToConfigApp();
        }

        //打开数据所在目录
        private void barButton_OpenDataFolder_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (Directory.Exists(dataFolderPath))
                {
                    System.Diagnostics.Process.Start("Explorer.exe", dataFolderPath);
                }
                else
                {
                    if (MessageBox.Show("系统没有找到数据目录，请重新指定。", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
                    {
                        FolderBrowserDialog fbd = new FolderBrowserDialog();
                        fbd.Description = "指定配置文件夹";
                        fbd.RootFolder = Environment.SpecialFolder.Desktop;
                        fbd.SelectedPath = Application.StartupPath;
                        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            dataFolderPath = fbd.SelectedPath;
                            bool isSucc = ConfigAppication.setAppConfigPath(dataFolderPath);
                            if (isSucc)
                            {
                                MessageBox.Show("设置系统数据目录成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        //打开配置文件夹
        private void barButton_OpenConfigFolder_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //判断目录是否存在
                if (!Directory.Exists(configFolderPath))
                {
                    Directory.CreateDirectory(configFolderPath);    //不存在则创建
                    MessageBox.Show("请将配置文件放置在此文件夹中。", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                }
                System.Diagnostics.Process.Start("Explorer.exe", configFolderPath);
            }
            catch
            {
            }
        }

        //初始化Gallery
        private void galleryBar_DataItems_Gallery_InitDropDownGallery(object sender, InplaceGalleryEventArgs e)
        {
            try
            {
                e.PopupGallery.AllowHoverImages = false;
                e.PopupGallery.ImageSize = new Size(120, 90);
                e.PopupGallery.ShowItemText = true;
                e.PopupGallery.ItemImageLocation = Locations.Top;
                e.PopupGallery.SynchWithInRibbonGallery = true;

                ConfigAppication.AlterDataGalleryItemCaption(this.galleryBar_DataItems, dataFolderNotPathList, true);
                //this.galleryBar_DataItems.Gallery.RefreshGallery();
                //this.galleryBar_DataItems.Refresh();
                //this.Refresh();
                this.galleryBar_DataItems.BeginUpdate();
            }
            catch
            {
            }
        }

        //Gallery关闭事件
        private void galleryBar_DataItems_Gallery_PopupClose(object sender, InplaceGalleryEventArgs e)
        {
            try
            {
                ConfigAppication.AlterDataGalleryItemCaption(this.galleryBar_DataItems, imageNameList, false);
                this.galleryBar_DataItems.Gallery.RefreshGallery();
            }
            catch
            {
            }
        }

        //Gallery中的item被点击事件
        private void galleryBar_DataItems_Gallery_ItemClick(object sender, GalleryItemClickEventArgs e)
        {
            try
            {
                string path = e.Item.Hint;
                string captionName = e.Item.Caption;
                this.tab_Home.Text = "主页 - " + captionName;
                this.galleryControl_Image.Gallery.Groups[0].Items.Clear();  //清空图片预览框
                FillNavigationBarAndHomeGallery(path);
            }
            catch
            {
            }
        }


        //TabPage切换事件
        private void xtraTabControl_Home_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {
            try
            {
                XtraTabPage xTab = e.Page;
                if (xTab == null)
                {
                    return;
                }
                HideAllRibbonPageCategory();    //隐藏所有工具栏
                if (xTab.Name == this.tab_doc.Name)
                {
                    this.ribbonPageCategory_doc.Visible = true;     //文档时数据
                    this.ribbonControl1.SelectedPage = this.homeRibbonPage1;
                }
                else if (xTab.Name == this.tab_xls.Name)
                {
                    this.ribbonPageCategory_xls.Visible = true;     //表格数据
                    this.ribbonControl1.SelectedPage = this.homeRibbonPage2;
                }
                else if (xTab.Name == this.tab_image.Name)
                {

                }
                else if (xTab.Name == this.tab_ArcMap.Name)
                {
                    this.ribbonPageCategory_map.Visible = true;     //空间数据
                    this.ribbonControl1.SelectedPage = this.ribbonPage_Map;
                    axMapControlCurrent = this.axMapControl1;
                    GISTools.Pan(this.axMapControlCurrent);
                }
                else if (xTab.Name == this.tab_ArcCatalog.Name)
                {
                    this.ribbonPageCategory_map.Visible = true;     //空间数据
                    this.ribbonControl1.SelectedPage = this.ribbonPage_Map;
                    axMapControlCurrent = this.axMapControl_DB;
                    GISTools.Pan(this.axMapControlCurrent);
                }
            }
            catch
            {
            }
        }

        //点击tab的关闭按钮，隐藏tab
        private void xtraTabControl_Home_CloseButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (this.xtraTabControl_Home.SelectedTabPage.Text == this.tab_Home.Text)
                {
                    return; //如果是关闭主页，则返回
                }
                this.xtraTabControl_Home.SelectedTabPage.PageVisible = false;
            }
            catch
            {
            }
        }
        
        //导航栏点击事件
        private void navBarControlHome_LinkClicked(object sender, NavBarLinkEventArgs e)
        {
            try
            {
                //获取点击文件的路径
                string path = (string)e.Link.Item.Tag;

                //打开文件
                OpenClickedFile(path);
            }
            catch
            {
            }
        }

        //导航栏右键点击事件
        private void navBarControlHome_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                NavBarControl navBar = sender as NavBarControl;
                if (e.Button == MouseButtons.Right && ModifierKeys == Keys.None && navBar.State == NavBarState.Normal)
                {
                    Point pt = navBar.PointToClient(MousePosition);
                    NavBarHitInfo info = navBar.CalcHitInfo(pt);
                    focusedCatalogNode = null;

                    if (info.InLink)
                    {
                        focusedNavBarLink = info.Link;
                        if (focusedNavBarLink == null)
                        {
                            return;
                        }
                        this.contextMenu_NavBar.Show(MousePosition.X, MousePosition.Y);
                    }
                }
            }
            catch
            {
            }
        }
        //在navBar上的右键菜单事件
        private void contextMenu_NavBar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                ToolStripItem tsi = e.ClickedItem;
                string path = (string)focusedNavBarLink.Item.Tag;
                if (!File.Exists(path))
                {
                    return;
                }
                switch (tsi.Name)
                {
                    case "tsmi_NavBar_OpenFile":    //打开文件                    
                        OpenClickedFile(path);
                        break;
                    case "tsmi_NavBar_OpenDir":     //打开文件所在目录
                        string dir = Path.GetDirectoryName(path);
                        System.Diagnostics.Process.Start("explorer.exe", dir);//打开C:\windows文件夹
                        break;
                    case "tsmi_NavBar_Delete":  //删除文件
                        if (MessageBox.Show("确定要删除该文件吗：" + path, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (ConfigAppication.DeleteFile(path))
                            {
                                MessageBox.Show("文件删除成功。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                focusedNavBarLink.Group.ItemLinks.Remove(focusedNavBarLink);
                                this.navBarControlHome.Refresh();
                            }
                            else
                            {
                                MessageBox.Show("文件删除失败，请检查后重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }

                        break;
                    case "tsmi_NavBar_AddFile": //添加文件到此目录
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString();
                        ofd.Filter = "所有文件(*.*)|*.*";
                        ofd.Title = "请选择文件";
                        ofd.Multiselect = true;
                        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string[] selectedFiles = ofd.FileNames;
                            string destDir = Path.GetDirectoryName(path);
                            bool isCopySucc = true;
                            foreach (string file in selectedFiles)
                            {
                                if (!ConfigAppication.CopyFileTo(file, destDir))
                                {
                                    isCopySucc = false;
                                }
                                else
                                {
                                    string fileName = Path.GetFileName(file);
                                    string extension = Path.GetExtension(file);
                                    string groupName = (string)htFileGroup[extension];

                                    Metadata md = new Metadata();
                                    md.name = fileName;
                                    md.path = Path.Combine(destDir, fileName);
                                    md.extention = extension;
                                    md.group = groupName;
                                    md.relativePath = "";

                                    NavBarItem nbi = ConfigAppication.createNavBarItem(md);
                                    focusedNavBarLink.Group.ItemLinks.Add(nbi);
                                    this.navBarControlHome.Refresh();
                                }
                            }
                            if (isCopySucc)
                            {
                                MessageBox.Show("文件成功入库。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("文件未能成功入库，请检查后重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        break;
                    case "tsmi_NavBar_OpenByDefault":   //由系统默认程序打开文件
                        System.Diagnostics.Process.Start(path);
                        break;
                }
            }
            catch
            {
            }
        }

        //图片浏览框尺寸变化事件,在程序启动时会触发
        private void galleryControl_Image_Resize(object sender, EventArgs e)
        {
            try
            {
                int imageWidth = this.galleryControl_Image.Gallery.ImageSize.Width; //取得预设的图片宽度
                int width = this.galleryControl_Image.Size.Width;   //获取控件的宽度
                int imageCount = width / (imageWidth + 25);    //得到需要显示的图片数量
                if (imageCount == 0)
                {
                    this.galleryControl_Image.Gallery.Groups[0].Items.Clear();  //如果图片数量为0，则清空图片
                    return;
                }
                this.galleryControl_Image.Gallery.ColumnCount = imageCount;     //设置需要显示的图片数量
            }
            catch
            {
            }
        }

        //图片浏览框选中图片事件
        private void galleryControlGallery1_ItemCheckedChanged(object sender, GalleryItemEventArgs e)
        {
            hasPictureEditUpdated = !hasPictureEditUpdated;
            GalleryItem gi = e.Item;
            Image showImage = null;
            try
            {
                if (hasPictureEditUpdated)
                {
                    //显示图片
                    if(this.pictureEdit_ImageTab.Image != null)
                    {
                        this.pictureEdit_ImageTab.Image.Dispose();
                    }
                    showImage = ConfigAppication.getThumbImage(Image.FromFile(gi.Hint), this.pictureEdit_ImageTab.Size); //取得缩略图
                    this.pictureEdit_ImageTab.Image = (Image)showImage.Clone();
                    //更改tabPage名称
                    this.tab_image.Text = gi.Caption;
                }
            }
            catch
            {
            }
            finally
            {   
                if(showImage != null)
                {
                    showImage.Dispose();
                    GC.Collect();
                }                
            }
        }

        //HomePage的GalleryControl双击事件
        private void galleryControlGallery1_ItemDoubleClick(object sender, GalleryItemClickEventArgs e)
        {
            try
            {

                //navBarControl切换到相应的group
                string galleryGroupName = e.Item.GalleryGroup.Caption;
                if (this.navGroup_Document != null || this.navGroup_Spatial != null || this.navGroup_Image != null || this.navGroup_MDB != null || this.navGroup_Else != null)
                {
                    if (this.navGroup_Document.Caption.Contains(galleryGroupName))
                    {
                        this.navGroup_Document.Expanded = true;
                    }
                    else if (this.navGroup_Spatial.Caption.Contains(galleryGroupName))
                    {
                        this.navGroup_Spatial.Expanded = true;
                    }
                    else if (this.navGroup_Image.Caption.Contains(galleryGroupName))
                    {
                        this.navGroup_Image.Expanded = true;
                    }
                    else if (this.navGroup_MDB.Caption.Contains(galleryGroupName))
                    {
                        this.navGroup_MDB.Expanded = true;
                    }
                    else if (this.navGroup_Else.Caption.Contains(galleryGroupName))
                    {
                        this.navGroup_Else.Expanded = true;
                    }
                }

                //打开文件
                string path = (string)e.Item.Tag;
                OpenClickedFile(path);
            }
            catch
            {
            }
        }

        //地图工具条按钮事件
        #region //地图工具条按钮事件
        //打开地图mxd
        private void barButton_SpatialOpenMXD_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;    //单选
            ofd.Title = "选择地图文件";
            ofd.Filter = "mxd文件|*.mxd";
            if(dataFolderPath != "")
            {
                ofd.InitialDirectory = dataFolderPath;
            }            
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
            {
                FileInfo fi = new FileInfo(ofd.FileName);
                if(fi.Exists)
                {
                    this.axMapControl1.LoadMxFile(fi.FullName);
                    this.axMapControlCurrent.ActiveView.Refresh();
                }
            }
        }
        //添加图层
        private void barButton_SpatialAddLayer_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISTools.AddData(this.axMapControlCurrent);
        }
        //平移
        private void barButton_SpatialPan_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISTools.Pan(this.axMapControlCurrent);
        }
        //放大
        private void barButton_SpatialZoomIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISTools.ZoomIn(this.axMapControlCurrent);
        }
        //缩小
        private void barButton_SpatialZoomOut_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISTools.ZoomOut(this.axMapControlCurrent);
        }
        //全图
        private void barButton_SpatialFullExtent_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISTools.FullExtend(this.axMapControlCurrent);
        }
        //逐级放大
        private void barButton_SpatialScaleIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISTools.ZoomInFix(this.axMapControlCurrent);
        }
        //逐级缩小
        private void barButton_SpatialScaleOut_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISTools.ZoomOutFix(this.axMapControlCurrent);
        }
        //属性查询
        private void barButton_SpatialIdentify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISTools.setNull(axMapControlCurrent);  //设置工具为空
            //GISTools.IdentifyTool(this.axMapControlCurrent);
            if(this.identifyFrm == null || this.identifyFrm.IsDisposed)
            {
                this.identifyFrm = new GISIdentifyFrm();
                this.identifyFrm.Show();
            }
            this.identifyFrm.Focus();
        }
        #endregion

        //地图点击事件
        private void axMapControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            if (this.identifyFrm != null && !this.identifyFrm.IsDisposed)
            {
                GISServices.IdentifyTool(this.axMapControl1, e, this.identifyFrm,Control.MousePosition);
            }
        }
        //Catalog地图点击事件
        private void axMapControl_DB_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (this.identifyFrm != null && !this.identifyFrm.IsDisposed)
            {
                GISServices.IdentifyTool(this.axMapControl_DB, e, this.identifyFrm,Control.MousePosition);
            }
        }
       
        //Catalog Tab的事件--------------------
        //定制
        private void treeList_MDBCatalog_GetSelectImage(object sender, DevExpress.XtraTreeList.GetSelectImageEventArgs e)
        {
            try
            {
                if (e.Node == null)
                    return;
                TreeListNode node = e.Node;

                string nodeType = (string)node.GetValue("NodeCode");
                switch (nodeType)
                {
                    case "FeaturerDataset":
                        e.NodeImageIndex = 1;
                        break;
                    case "FeaturePoint":
                        e.NodeImageIndex = 2;
                        break;
                    case "FeaturePolyline":
                        e.NodeImageIndex = 3;
                        break;
                    case "FeaturePolygon":
                        e.NodeImageIndex = 4;
                        break;
                    case "RasterCatalog":
                        e.NodeImageIndex = 5;
                        break;
                    case "RasterImage":
                        e.NodeImageIndex = 6;
                        break;
                    case "RasterDataset":
                        e.NodeImageIndex = 7;
                        break;
                    default:
                        e.NodeImageIndex = 0;
                        break;
                }
            }
            catch
            {
            }
        }

        //MDB数据库Catalog的双击事件
        private void treeList_MDBCatalog_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                TreeList tree = sender as TreeList;
                TreeListHitInfo hi = tree.CalcHitInfo(tree.PointToClient(Control.MousePosition));
                if (hi.Node != null)
                {
                    this.axMapControl_DB.Map.ClearLayers();
                    GISServices.AddData2Map(this.axMapControl_DB, hi.Node, workspace);
                }
            }
            catch
            {
            }
        }

        //catalog中的右键点击ContextMenu事件
        private void contextMenu_Catalog_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.Name)
                {
                    case "tsmi_Open":
                        this.axMapControl_DB.Map.ClearLayers();
                        GISServices.AddData2Map(this.axMapControl_DB, focusedCatalogNode, workspace);
                        break;
                    case "tsmi_AddLayer":
                        GISServices.AddData2Map(this.axMapControl_DB, focusedCatalogNode, workspace);
                        break;
                }
            }
            catch
            {
            }
        }

        //catalog中的右键点击事件
        private void treeList_MDBCatalog_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {

                TreeList tree = sender as TreeList;
                if (e.Button == MouseButtons.Right && ModifierKeys == Keys.None && tree.State == TreeListState.Regular)
                {
                    Point pt = tree.PointToClient(MousePosition);
                    TreeListHitInfo info = tree.CalcHitInfo(pt);
                    focusedCatalogNode = null;

                    if (info.HitInfoType == HitInfoType.Cell)
                    {
                        focusedCatalogNode = info.Node;
                        if (focusedCatalogNode == null)
                        {
                            return;
                        }
                        this.contextMenu_Catalog.Show(MousePosition.X, MousePosition.Y);
                    }
                }
            }
            catch
            {
            }
        }
      
        //--------------------------------------------函数----------------------------------------------       
        //读取数据到界面，配置系统的数据
        public void ReadDataToConfigApp()
        {
            try
            {
                this.galleryBar_DataItems.Gallery.Groups[0].Items.Clear();  //清空之前显示

                imageCollection_DataGallery = ConfigAppication.setImageCollection(configFolderPath, out imageNameList);
                dataFolderPathList = ConfigAppication.getDataDirectoryList(dataFolderPath, out dataFolderNotPathList);
                Size sizeGallery = new Size(48, 36);
                Size sizeHover = new Size(100, 75);
                galleryBar_DataItems.Gallery.ImageSize = sizeGallery;
                galleryBar_DataItems.Gallery.HoverImageSize = sizeHover;
                galleryBar_DataItems.Gallery.Images = imageCollection_DataGallery;
                galleryBar_DataItems.Gallery.HoverImages = imageCollection_DataGallery;
                int index = 0;
                foreach (Image image in imageCollection_DataGallery.Images)
                {
                    if (imageNameList.Count >= index && dataFolderPathList.Count >= index)
                    {
                        GalleryItem gItem = ConfigAppication.CreateGalleryItem(image, index, imageNameList[index], dataFolderPathList[index]);
                        galleryBar_DataItems.Gallery.Groups[0].Items.Add(gItem);
                    }
                    else
                    {
                        break;
                    }
                    index++;
                }
            }
            catch
            {
            }
        }

        //配置navigation导航栏
        public void FillNavigationBarAndHomeGallery(string path)
        {
            WaitingFrm wf = new WaitingFrm();
            wf.Show();
            wf.Refresh();
            try
            {
                //NavBarControl
                this.navBarControlHome.Groups.Clear();  //清空所有分组            
                //初始化分组
                this.navGroup_Document = new NavBarGroup("文档数据");
                this.navGroup_Image = new NavBarGroup("图片数据");
                this.navGroup_Spatial = new NavBarGroup("地图数据");
                this.navGroup_MDB = new NavBarGroup("空间数据");
                this.navGroup_Else = new NavBarGroup("其他数据");

                //GalleryControl
                this.galleryControl_HomePage.Gallery.Groups.Clear();    //清空分组
                //初始化分组
                this.galleryGroup_Documnet = new GalleryItemGroup();
                this.galleryGroup_Image = new GalleryItemGroup();
                this.galleryGroup_Spatial = new GalleryItemGroup();
                //this.galleryGroup_MDB = new GalleryItemGroup();       //空间数据库不显示
                this.galleryGroup_Else = new GalleryItemGroup();
                this.galleryGroup_Documnet.Caption = "文档";
                this.galleryGroup_Image.Caption = "图片";
                this.galleryGroup_Spatial.Caption = "地图";
                //this.galleryGroup_MDB.Caption = "空间数据库";          //空间数据库不显示
                this.galleryGroup_Else.Caption = "其他";

                //由路径取得所有文件
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists)
                {
                    MessageBox.Show("没有找到指定项目的数据，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                System.Console.WriteLine("开始获取文件：" + DateTime.Now);
                fileNavigationList = ConfigAppication.getAllDataSpecifyFolder(path, htFileGroup);
                System.Console.WriteLine("结束获取文件并排序：" + DateTime.Now);

                System.Console.WriteLine("开始加入links：" + DateTime.Now);
                foreach (Metadata md in fileNavigationList)
                {
                    NavBarItem nbi = ConfigAppication.createNavBarItem(md);
                    GalleryItem gi = ConfigAppication.createGalleryItem(md);
                    if (md.group == documentGroup)
                    {
                        this.navGroup_Document.ItemLinks.Add(nbi);
                        //添加GalleryHomePage的item
                        if (this.galleryGroup_Documnet.Items.Count < 9)
                        {
                            this.galleryGroup_Documnet.Items.Add(gi);
                        }
                        else if (this.galleryGroup_Documnet.Items.Count == 9)
                        {
                            GalleryItem giTen = ConfigAppication.createGalleryItem(md, true);
                            this.galleryGroup_Documnet.Items.Add(giTen);
                        }
                    }
                    else if (md.group == spatialGroup)
                    {
                        this.navGroup_Spatial.ItemLinks.Add(nbi);
                        //添加GalleryHomePage的item
                        if (this.galleryGroup_Spatial.Items.Count < 9)
                        {
                            this.galleryGroup_Spatial.Items.Add(gi);
                        }
                        else if (this.galleryGroup_Spatial.Items.Count == 9)
                        {
                            GalleryItem giTen = ConfigAppication.createGalleryItem(md, true);
                            this.galleryGroup_Spatial.Items.Add(giTen);
                        }
                    }
                    else if (md.group == imageGroup)
                    {
                        this.navGroup_Image.ItemLinks.Add(nbi);
                        imageDataList.Add(md);  //将所有的图片加入list中，方便显示，减少内存占用
                        //添加GalleryHomePage的item
                        if (this.galleryGroup_Image.Items.Count < 9)
                        {
                            this.galleryGroup_Image.Items.Add(gi);
                        }
                        else if (this.galleryGroup_Image.Items.Count == 9)
                        {
                            GalleryItem giTen = ConfigAppication.createGalleryItem(md, true);
                            this.galleryGroup_Image.Items.Add(giTen);
                        }
                    }
                    else if (md.group == dbGroup)
                    {
                        this.navGroup_MDB.ItemLinks.Add(nbi);
                        //添加GalleryHomePage的item
                        if (this.galleryGroup_Else.Items.Count < 9)
                        {
                            this.galleryGroup_Else.Items.Add(gi);
                        }
                        else if (this.galleryGroup_Else.Items.Count == 9)
                        {
                            GalleryItem giTen = ConfigAppication.createGalleryItem(md, true);
                            this.galleryGroup_Else.Items.Add(giTen);
                        }
                    }
                    //System.Console.WriteLine("正在添加link：" + md.name + DateTime.Now);
                    //else if (md.group == elseGroup) {
                    //    this.navBarGroup_elseData.ItemLinks.Add(nbi);
                    ////}
                }
                System.Console.WriteLine("结束加入links：" + DateTime.Now);

                //开始更新界面navBarControlHome
                this.navBarControlHome.BeginUpdate();
                //添加分组到界面
                bool hasExpanded = false;   //是否有group展开
                if (this.navGroup_Document.ItemLinks.Count > 0)
                {
                    this.navBarControlHome.Groups.Add(this.navGroup_Document);
                    hasExpanded = this.navGroup_Document.Expanded = true;
                }
                if (this.navGroup_Image.ItemLinks.Count > 0)
                {
                    this.navBarControlHome.Groups.Add(this.navGroup_Image);
                    if (!hasExpanded)
                    {
                        hasExpanded = this.navGroup_Image.Expanded = true;
                    }
                }
                if (this.navGroup_Spatial.ItemLinks.Count > 0)
                {
                    this.navBarControlHome.Groups.Add(this.navGroup_Spatial);
                    if (!hasExpanded)
                    {
                        hasExpanded = this.navGroup_Spatial.Expanded = true;
                    }
                }
                if (this.navGroup_MDB.ItemLinks.Count > 0)
                {
                    this.navBarControlHome.Groups.Add(this.navGroup_MDB);
                    if (!hasExpanded)
                    {
                        hasExpanded = this.navGroup_MDB.Expanded = true;
                    }
                }
                if (this.navGroup_Else.ItemLinks.Count > 0)
                {
                    this.navBarControlHome.Groups.Add(this.navGroup_Else);
                    if (!hasExpanded)
                    {
                        hasExpanded = this.navGroup_Else.Expanded = true;
                    }
                }
                this.navBarControlHome.SkinExplorerBarViewScrollStyle = DevExpress.XtraNavBar.SkinExplorerBarViewScrollStyle.ScrollBar;
                this.navBarControlHome.EndUpdate();

                //开始更新GalleryControl_Home的界面
                if (this.galleryGroup_Documnet.Items.Count > 0)
                {
                    this.galleryControl_HomePage.Gallery.Groups.Add(this.galleryGroup_Documnet);
                }
                if (this.galleryGroup_Spatial.Items.Count > 0)
                {
                    this.galleryControl_HomePage.Gallery.Groups.Add(this.galleryGroup_Spatial);
                }
                if (this.galleryGroup_Image.Items.Count > 0)
                {
                    this.galleryControl_HomePage.Gallery.Groups.Add(this.galleryGroup_Image);
                }
                if (this.galleryGroup_Else.Items.Count > 0)
                {
                    this.galleryControl_HomePage.Gallery.Groups.Add(this.galleryGroup_Else);
                }

                this.navBarControlHome.Refresh();   //刷新界面
                this.Refresh();
            }
            catch
            {

            }
            finally
            {
                wf.Dispose();
                wf.CloseFrm();                
                //wf.Close();
            }
        }

        //隐藏所有tabPage对应的工具栏
        public void HideAllRibbonPageCategory()
        {
            this.ribbonPageCategory_doc.Visible = false;    //文档的ribbon
            this.ribbonPageCategory_xls.Visible = false;    //表格数据的ribbon
            this.ribbonPageCategory_map.Visible = false;     //空间数据
            //this.ribbonpageca.Visible = false;    //
        }

        //加载图片
        private void loadImageGallery()
        {
            try
            {
                //清空已经显示的图片
                this.galleryControl_Image.Gallery.Groups[0].Items.Clear();
                imageListInGallery.Clear();      //清空

                //处理显示图片个数
                int columnCount = this.galleryControl_Image.Gallery.ColumnCount;
                if (columnCount == 0 || imageDataList.Count == 0)
                {
                    return; //如果需要显示的数量和拥有的数据为0，则返回
                }
                int showImageIndex = 0; //当前需要显示的图片index
                int eParam = 0;         //如果当前显示的图片index<0,则用这个参数进行调节
                for (int index = 0; index < columnCount; index++)
                {
                    //处理index为负数的问题-------------------
                    showImageIndex = index - columnCount / 2 + curImageIndex + eParam;   //！！【重要】 算法，计算当前需要显示图片的index
                    if (index == 0 && showImageIndex < 0)
                    {
                        eParam = Math.Abs(showImageIndex);
                        showImageIndex += eParam;   //用这个参数进行调节，使得其不为负数                    
                    }
                    if (showImageIndex >= imageDataList.Count)
                    {
                        break;  //如果图片index超过图片数量，则终止
                    }
                    //取得图片资源------------------------------
                    Metadata md = imageDataList[showImageIndex];    //取得文件信息
                    FileInfo fi = new FileInfo(md.path);
                    if (!fi.Exists)
                    {
                        continue;   //如果文件不存在，则继续
                    }
                    string fileGroup = (string)htFileGroup[fi.Extension.ToLower()];  //由文件后缀名取得组别
                    if (fileGroup != imageGroup)
                    {
                        continue;       //如果不是图片，则继续遍历
                    }
                    imageListInGallery.Add(md); //加入list中
                    //加载图片资源，进行显示------------------------------                

                    Image imgGallery = ConfigAppication.getThumbImage(Image.FromFile(fi.FullName), this.galleryControl_Image.Gallery.ImageSize); //取得缩略图
                    GalleryItem gi = new GalleryItem(imgGallery, md.name, "");
                    gi.Hint = md.path;
                    this.galleryControl_Image.Gallery.Groups[0].Items.Add(gi);//GalleryControl控件
                }
                ////设置选中项
                checkedGalleryItemIndex = columnCount / 2 - eParam;    //【重要】算出当前需要选中的item的index
                this.galleryControl_Image.Gallery.SetItemCheck(this.galleryControl_Image.Gallery.Groups[0].Items[checkedGalleryItemIndex], true, true);
            }
            catch
            {
            }
        }

        //点击文件图标，打开文件
        private void OpenClickedFile(string path)
        {
            try
            {
                if (path == "" || path == null)
                {
                    return;
                }
                FileInfo fi = new FileInfo(path);
                //判断文件是否存在
                if (!fi.Exists)
                {
                    MessageBox.Show("文件不存在，请到原始目录中检查。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string extention = fi.Extension.ToLower();    //获取文件的后缀名
                string groupName = (string)htFileGroup[extention];  //获取文件组别
                if (extention == ".doc" || extention == ".docx")
                {
                    this.tab_doc.PageVisible = true;                            //设置相应tabPage可见
                    this.xtraTabControl_Home.SelectedTabPage = this.tab_doc;    //选择相应的tabPage
                    this.richEditControl_doc.LoadDocument(fi.FullName);         //加载文件
                    this.tab_doc.Text = fi.Name;                                //修改tab的名称未文件名
                }
                else if (extention == ".xls" || extention == ".xlsx")
                {
                    this.tab_xls.PageVisible = true;                            //设置相应tabPage可见
                    this.xtraTabControl_Home.SelectedTabPage = this.tab_xls;    //选择相应的tabPage
                    this.spreadsheetControl_xls.LoadDocument(fi.FullName);      //加载文件
                    this.tab_xls.Text = fi.Name;                                //修改tab的名称未文件名
                }
                else if (extention == ".pdf")
                {
                    this.tab_pdf.PageVisible = true;                            //设置相应tabPage可见
                    this.xtraTabControl_Home.SelectedTabPage = this.tab_pdf;    //选择相应的tabPage
                    this.pdfViewer1.LoadDocument(fi.FullName);                  //加载文件
                    this.tab_pdf.Text = fi.Name;                                //修改tab的名称未文件名
                }
                else if (groupName == imageGroup)
                {
                    this.tab_image.PageVisible = true;                          //设置相应tabPage可见
                    this.xtraTabControl_Home.SelectedTabPage = this.tab_image;  //选择相应的tabPage
                    curImageIndex = ConfigAppication.findIndexOfImageList(fi.FullName, imageDataList);  //查询点击图片的索引值
                    loadImageGallery();                                //加载图片到ImageSlider和GalleryControl
                }
                else if (groupName == spatialGroup)
                {
                    this.tab_ArcMap.PageVisible = true;                          //设置相应tabPage可见
                    this.xtraTabControl_Home.SelectedTabPage = this.tab_ArcMap;  //选择相应的tabPage  
                    tab_ArcMap.Text = fi.Name;
                    ConfigAppication.OpenSpatialData(fi, this.axMapControl1);

                }
                else if (groupName == dbGroup)
                {
                    this.tab_ArcCatalog.PageVisible = true;                          //设置相应tabPage可见
                    this.xtraTabControl_Home.SelectedTabPage = this.tab_ArcCatalog;  //选择相应的tabPage  
                    this.tab_ArcCatalog.Text = fi.Name;
                    workspace = GISServices.getMDBWorkspaceFromPath(fi.FullName);
                    if (workspace == null)
                    {
                        MessageBox.Show("无法打开该数据库，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    ConfigAppication.OpenMDBFile(workspace, this.treeList_MDBCatalog);
                }
                else if (extention == elseGroup)
                {
                    System.Diagnostics.Process.Start(fi.FullName);
                }
            }
            catch
            {
            }
        }


        //输出地图
        private void barButton_ExportMapImage_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GISServices.ExportImage(this.axMapControlCurrent.ActiveView);
        }


        //界面定制-------------------------------------------------------------------------

    }
}
