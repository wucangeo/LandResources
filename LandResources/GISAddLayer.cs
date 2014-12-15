using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesRaster;

namespace LandResources
{
    public class GISAddLayer
    {
        //添加RasterCatalog图层
        public static void AddRasterCatalog2Map(AxMapControl _axMapControl, IWorkspace _Workspace, string rasterCatalogName)
        {
            try
            {
                IRasterCatalog rasterCatalog = GISDatabaseManager.OpenRasterCatalog(_Workspace, rasterCatalogName); //打开GDB中的栅格目录
                if (rasterCatalog == null)
                {
                    return;
                }
                //if (!rasterCatalog.IsRasterDataset)
                //{
                //    return;
                //}

                IGdbRasterCatalogLayer catalogLayer = new GdbRasterCatalogLayerClass();
                catalogLayer.Setup(rasterCatalog as ITable);
                //catalogLayer.Renderers = 
                _axMapControl.Map.AddLayer(catalogLayer as ILayer);
                _axMapControl.ActiveView.Refresh();
            }
            catch
            {
            }
        }

        //添加RasterDataset图层
        public static void AddRasterDataset2Map(AxMapControl _axMapControl, IWorkspace _Workspace, string rasterDatasetName)
        {
            try
            {
                IRasterDataset rasterDataset = GISDatabaseManager.OpenRasterDatasetFromWorkspace(_Workspace, rasterDatasetName); //打开GDB中的栅格dataset
                if (rasterDataset == null)
                {
                    return;
                }

                IRasterLayer rasterLayer = new RasterLayerClass();
                rasterLayer.CreateFromDataset(rasterDataset);
                if (rasterLayer == null)
                {
                    MessageBox.Show("添加图层失败。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                _axMapControl.Map.AddLayer(rasterLayer as ILayer);
                _axMapControl.ActiveView.Refresh();
            }
            catch
            {
            }
        }

        //添加要素图层
        public static void AddFeatureFeatureClass2Map(AxMapControl _axMapControl, IWorkspace _Workspace, string FeatureName)
        {
            try
            {
                IFeatureWorkspace pFeatureWorkspace = _Workspace as IFeatureWorkspace;
                IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(FeatureName);
                if (pFeatureClass == null)
                {
                    return;
                }
                IFeatureLayer featureLayer = new FeatureLayerClass();
                featureLayer.FeatureClass = pFeatureClass;
                featureLayer.Name = FeatureName;
                _axMapControl.Map.AddLayer(featureLayer as ILayer);
                _axMapControl.ActiveView.Refresh();
            }
            catch
            {
            }
        }
    }
}
