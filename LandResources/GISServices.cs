using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//diy 
using System.Windows.Forms;
using System.Data;      //DataTable
using System.IO;        //FileInfo

using DevExpress.XtraVerticalGrid;  //PropertyGridControl
using DevExpress.XtraEditors.Repository;   //RepositoryItem
using DevExpress.XtraVerticalGrid.Rows;     //MultiEditorRow
using DevExpress.XtraTreeList.Nodes;        //TreeListNode

using ESRI.ArcGIS.Controls;     //mapControl
using ESRI.ArcGIS.Carto;        //Identify 
using ESRI.ArcGIS.Geometry;     //Point
using ESRI.ArcGIS.esriSystem;   //IArray
using ESRI.ArcGIS.Geodatabase;  //IFeature
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DataSourcesGDB;   //AccessMDB


namespace LandResources
{
    public class GISServices
    {
        public static void IdentifyTool(AxMapControl _mapControl, IMapControlEvents2_OnMouseDownEvent e,GISIdentifyFrm identifyFrm,System.Drawing.Point mousePosition)
        {
            try
            {
                if(_mapControl.Map.LayerCount == 0)
                {
                    return;
                }
                IIdentify pIdentify = _mapControl.Map.get_Layer(0) as IIdentify; //通过图层获取 IIdentify 实例
                IPoint pPoint = new ESRI.ArcGIS.Geometry.Point(); //新建点来选择
                IArray pIDArray;
                IIdentifyObj pIdObj;
                pPoint.PutCoords(e.mapX, e.mapY);      //定义点
                int delta = 500;
                IEnvelope envelope = new EnvelopeClass();
                envelope.XMin = e.mapX - delta;
                envelope.XMax = e.mapX + delta;
                envelope.YMin = e.mapY - delta;
                envelope.YMax = e.mapY + delta;
                IGeometry geo = envelope as IGeometry;
                IZAware zAware = geo as IZAware;
                zAware.ZAware = true;

                pIDArray = pIdentify.Identify(geo);       //通过点获取数组，用点一般只能选择一个元素
                if (pIDArray != null)
                {
                    //取得要素
                    pIdObj = pIDArray.get_Element(0) as IIdentifyObj;       //取得要素                
                    pIdObj.Flash(_mapControl.ActiveView.ScreenDisplay);     //闪烁效果
                    IRowIdentifyObject rowIdentify = pIdObj as IRowIdentifyObject;
                    IFeature feature = rowIdentify.Row as IFeature;
                    IFields fields = feature.Fields;

                    //取得控件
                    PropertyGridControl propertyGrid = identifyFrm.Controls[0] as PropertyGridControl;                    
                    propertyGrid.Rows.Clear();  //清除上次显示
                    CategoryRow rowCategory = new CategoryRow("图层名称："+pIdObj.Layer.Name);
                    propertyGrid.Rows.Add(rowCategory);

                    for (int index = 0; index < fields.FieldCount; index++)
                    {
                        //获取Feature的属性数据
                        IField field = fields.get_Field(index);
                        string fieldName = field.AliasName;
                        string value = Convert.ToString(feature.get_Value(index));
                        //添加属性行
                        EditorRow rowEdit = new EditorRow(fieldName);
                        rowEdit.Properties.UnboundType = DevExpress.Data.UnboundColumnType.String;
                        rowEdit.Properties.Caption = fieldName;
                        //给这个属性赋值
                        RepositoryItemTextEdit textItem = new RepositoryItemTextEdit();
                        textItem.ReadOnly = true;
                        textItem.NullText = value;
                        rowEdit.Properties.RowEdit = textItem;

                        rowCategory.ChildRows.Add(rowEdit);
                    } 
                    //修改界面大小
                    int frmHeight =  42 + fields.FieldCount * 19;
                    if(frmHeight>400)
                    {
                        frmHeight = 400;    //最大高度为400
                    }
                    identifyFrm.Size = new System.Drawing.Size(identifyFrm.Size.Width,frmHeight);
                    identifyFrm.Location = mousePosition;
                }
                else
                {
                    //MessageBox.Show("Nothing!");
                }
            }
            catch
            {
            }
        }

        //输出当前地图为图片
        public static string ExportImage(IActiveView pActiveView)
        {
            try
            {
                SaveFileDialog pSaveFileDialog = new SaveFileDialog();
                pSaveFileDialog.Filter = "JPEG(*.jpg)|*.jpg|AI(*.ai)|*.ai|BMP(*.BMP)|*.bmp|EMF(*.emf)|*.emf|GIF(*.gif)|*.gif|PDF(*.pdf)|*.pdf|PNG(*.png)|*.png|EPS(*.eps)|*.eps|SVG(*.svg)|*.svg|TIFF(*.tif)|*.tif";
                pSaveFileDialog.Title = "输出地图";
                pSaveFileDialog.RestoreDirectory = true;
                pSaveFileDialog.FilterIndex = 1;
                if (pSaveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return null;
                }
                string FileName = pSaveFileDialog.FileName;
                IExport pExporter = null;
                switch (pSaveFileDialog.FilterIndex)
                {
                    case 1:
                        pExporter = new ExportJPEGClass();
                        break;
                    case 2:
                        pExporter = new ExportBMPClass();
                        break;
                    case 3:
                        pExporter = new ExportEMFClass();
                        break;
                    case 4:
                        pExporter = new ExportGIFClass();
                        break;
                    case 5:
                        pExporter = new ExportAIClass();
                        break;
                    case 6:
                        pExporter = new ExportPDFClass();
                        break;
                    case 7:
                        pExporter = new ExportPNGClass();
                        break;
                    case 8:
                        pExporter = new ExportPSClass();
                        break;
                    case 9:
                        pExporter = new ExportSVGClass();
                        break;
                    case 10:
                        pExporter = new ExportTIFFClass();
                        break;
                    default:
                        MessageBox.Show("输出格式错误");
                        return null;
                }
                IEnvelope pEnvelope = new EnvelopeClass();
                ITrackCancel pTrackCancel = new CancelTrackerClass();
                tagRECT ptagRECT;
                ptagRECT = pActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame();

                int pResolution = (int)(pActiveView.ScreenDisplay.DisplayTransformation.Resolution);

                pEnvelope.PutCoords(ptagRECT.left, ptagRECT.bottom, ptagRECT.right, ptagRECT.top);
                pExporter.Resolution = pResolution;
                pExporter.ExportFileName = FileName;
                pExporter.PixelBounds = pEnvelope;
                pActiveView.Output(pExporter.StartExporting(), pResolution, ref ptagRECT, pActiveView.Extent, pTrackCancel);
                pExporter.FinishExporting();
                //释放资源
                pSaveFileDialog.Dispose();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pExporter);
                return FileName;
            }
            catch
            {
                return null;

            }
        }

        //由路径取得MDB的workspace
        public static IWorkspace getMDBWorkspaceFromPath(string mdbPath)
        {
            IWorkspace workspace = null;
            try
            {
                FileInfo fi = new FileInfo(mdbPath);
                if (!fi.Exists)
                {
                    return null;
                }
                IWorkspaceFactory workspaceFactory = new AccessWorkspaceFactoryClass();
                workspace = workspaceFactory.OpenFromFile(mdbPath, 0);
            }
            catch
            {
            }
            return workspace;
        }

        //由workspace得到所有数据的DataTable
        public static DataTable CreateTreeListTable(IWorkspace workspace)
        {
            //定制表头------------------------------------------------------------
            DataTable dt = new DataTable();
            try
            {
                DataColumn dcOID = new DataColumn("KeyFieldName", Type.GetType("System.Int32"));
                DataColumn dcParentOID = new DataColumn("ParentFieldName", Type.GetType("System.Int32"));
                DataColumn dcNodeName = new DataColumn("NodeName", Type.GetType("System.String"));
                DataColumn dcNodeCode = new DataColumn("NodeCode", Type.GetType("System.String"));
                dt.Columns.Add(dcOID);
                dt.Columns.Add(dcParentOID);
                dt.Columns.Add(dcNodeName);
                dt.Columns.Add(dcNodeCode);


                int parentCode = 1;     //所有数据目录的父级编码

                //遍历workspace数据-------------------------------------------------
                //遍历FeatureDataset数据----------------------
                IEnumDataset pEnumFeatureDataset = workspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);
                pEnumFeatureDataset.Reset();
                IDataset pFeaturerDataset = pEnumFeatureDataset.Next();
                while (pFeaturerDataset != null)
                {
                    if (pFeaturerDataset is IFeatureDataset)
                    {
                        IFeatureDataset pFeatureDataset = pFeaturerDataset as IFeatureDataset;
                        IFeatureClassContainer m_FeatureClassContainer = (IFeatureClassContainer)pFeatureDataset;
                        IEnumFeatureClass m_EnumFC = m_FeatureClassContainer.Classes;
                        IFeatureClass pFeatureClass = null;

                        //添加父级栅格目录节点
                        DataRow drFeaturerDataset = dt.NewRow();
                        drFeaturerDataset["KeyFieldName"] = parentCode;
                        drFeaturerDataset["ParentFieldName"] = DBNull.Value;
                        drFeaturerDataset["NodeName"] = pFeatureDataset.Name;
                        drFeaturerDataset["NodeCode"] = "FeaturerDataset";
                        dt.Rows.Add(drFeaturerDataset);

                        int parentCodeFeatureDataset = parentCode + 1;
                        while ((pFeatureClass = m_EnumFC.Next()) != null)
                        {
                            //添加节点
                            string featureType = "vector";
                            if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                            {
                                featureType = "FeaturePoint";
                            }
                            else if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                            {
                                featureType = "FeaturePolyline";
                            }
                            else if (pFeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                            {
                                featureType = "FeaturePolygon";
                            }
                            //添加子级栅格目录节点
                            DataRow drFeature = dt.NewRow();
                            drFeature["KeyFieldName"] = parentCodeFeatureDataset++;
                            drFeature["ParentFieldName"] = parentCode;
                            drFeature["NodeName"] = pFeatureClass.AliasName;
                            drFeature["NodeCode"] = featureType;
                            dt.Rows.Add(drFeature);
                        }
                        parentCode = parentCodeFeatureDataset;
                    }
                    pFeaturerDataset = pEnumFeatureDataset.Next();
                }

                //遍历RasterCatalog数据--------------------
                IEnumDataset pEnumRasterCatalog = workspace.get_Datasets(esriDatasetType.esriDTRasterCatalog);
                pEnumRasterCatalog.Reset();
                IDataset pDataset = pEnumRasterCatalog.Next();
                while (pDataset != null)
                {
                    if (pDataset is IRasterCatalog)
                    {
                        IRasterCatalog pRasterCatalog = pDataset as IRasterCatalog;
                        IFeatureClass pFeatureClass = (IFeatureClass)pRasterCatalog;
                        IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
                        IFeature pFeature = null;
                        //添加父级栅格目录节点
                        DataRow drRasterCatalog = dt.NewRow();
                        drRasterCatalog["KeyFieldName"] = parentCode;
                        drRasterCatalog["ParentFieldName"] = DBNull.Value;
                        drRasterCatalog["NodeName"] = pFeatureClass.AliasName;
                        drRasterCatalog["NodeCode"] = "RasterCatalog";
                        dt.Rows.Add(drRasterCatalog);

                        int parentCodeRasterCatalog = parentCode + 1;

                        while ((pFeature = pFeatureCursor.NextFeature()) != null)
                        {
                            IFields pFields = pFeature.Fields;
                            int nameIndex = pFields.FindField("Name");
                            string imageName = "";
                            if (nameIndex >= 0)
                            {
                                imageName = (string)pFeature.get_Value(nameIndex);
                            }
                            else
                            {
                                imageName = (string)pFeature.get_Value(0);
                            }
                            //添加子级栅格目录节点
                            DataRow drRaster = dt.NewRow();
                            drRaster["KeyFieldName"] = parentCodeRasterCatalog++;
                            drRaster["ParentFieldName"] = parentCode;
                            drRaster["NodeName"] = imageName;
                            drRaster["NodeCode"] = "RasterImage";
                            dt.Rows.Add(drRaster);
                        }
                        parentCode = parentCodeRasterCatalog;
                    }
                    pDataset = pEnumRasterCatalog.Next();
                }

                //遍历RasterDataset数据--------------------
                IEnumDataset pEnumRasterDataset = workspace.get_Datasets(esriDatasetType.esriDTRasterDataset);
                pEnumRasterDataset.Reset();
                IDataset pDatasetRaster = pEnumRasterDataset.Next();
                while (pDatasetRaster != null)
                {
                    if (pDatasetRaster is IRasterDataset)
                    {
                        IRasterDataset rasterDataset = pDatasetRaster as IRasterDataset;
                        //添加节点
                        string rasterDatasetName = rasterDataset.CompleteName;
                        rasterDatasetName = rasterDatasetName.Substring(0, rasterDatasetName.IndexOf('.'));

                        //添加父级栅格目录节点
                        DataRow drRasterDataset = dt.NewRow();
                        drRasterDataset["KeyFieldName"] = parentCode++;
                        drRasterDataset["ParentFieldName"] = DBNull.Value;
                        drRasterDataset["NodeName"] = rasterDatasetName;
                        drRasterDataset["NodeCode"] = "RasterDataset";
                        dt.Rows.Add(drRasterDataset);
                    }
                    pDataset = pEnumRasterDataset.Next();
                }
            }
            catch
            {
            }
            return dt;
        }

        public static void AddData2Map(AxMapControl _axMapControl, TreeListNode node, IWorkspace _Workspace)
        {
            try
            {

                //先验条件
                if (_axMapControl == null || node == null || _Workspace == null)
                {
                    return;
                }
                //获取名称
                string LayerName = (string)node["NodeName"];   //图层名称
                string LayerType = (string)node["NodeCode"];  //图层类型

                switch (LayerType)
                {
                    case "RasterCatalog":
                        GISAddLayer.AddRasterCatalog2Map(_axMapControl, _Workspace, LayerName); //添加图层
                        break;
                    case "RasterImage":
                        MessageBox.Show("暂不支持直接添加RasterImage，你可以添加RasterCatalog。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case "RasterDataset":
                        GISAddLayer.AddRasterDataset2Map(_axMapControl, _Workspace, LayerName); //添加图层
                        break;
                    case "FeaturerDataset":
                        MessageBox.Show("暂不支持直接添加要素集，你可以分别添加其中的FeatureClass。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case "FeaturePoint":
                    case "FeaturePolyline":
                    case "FeaturePolygon":
                        GISAddLayer.AddFeatureFeatureClass2Map(_axMapControl, _Workspace, LayerName); //添加图层
                        break;
                }
            }
            catch
            {
            }
        }

    }
}
