using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;

namespace LandResources
{
    public class GISDatabaseManager
    {

        /// <summary>
        /// 连接到个人数据库MDB
        /// </summary>
        /// <param name="_pGDBName">数据库路径</param>
        /// <returns>IWorkspace</returns>
        public static IWorkspace GetMDBWorkspace(String _pGDBName)
        {
            IWorkspaceFactory pWsFac = new AccessWorkspaceFactoryClass();
            IWorkspace pWs = pWsFac.OpenFromFile(_pGDBName, 0);
            return pWs;
        }

        /// <summary>
        /// 连接到文件数据库GDB
        /// </summary>
        /// <param name="_pGDBName">数据库路径</param>
        /// <returns>IWorkspace</returns>
        public static IWorkspace GetGDBWorkspace(String _pGDBName)
        {
            IWorkspaceFactory pWsFac = new FileGDBWorkspaceFactoryClass();
            IWorkspace pWorkspace = null;
            try
            {
                pWorkspace = pWsFac.OpenFromFile(_pGDBName, 0);
                return pWorkspace;
            }
            catch (IOException e)
            {
                e.StackTrace.ToString();
                return pWorkspace;
            }
        }

        /// <summary>
        /// 遍历GDB数据库
        /// </summary>
        /// <param name="ListDatasetClassDescription"></param>
        /// <param name="workspace"></param>
        public static void listDTAny(String ListDatasetClassDescription, IWorkspace workspace)
        {
            try
            {
                IEnumDataset enumDataset = workspace.get_Datasets(esriDatasetType.esriDTAny);//esriDTFeatureDataset  esriDTAny
                // esriDTFeatureDataset
                IDataset dsName = null;
                dsName = enumDataset.Next();
                //System.out.println("ListDatasetClass:" + ListDatasetClassDescription);
                while (dsName != null)
                {
                    //System.out.println("Name: " + dsName.getName() + "\t\t; Type: " + getDatasetTypeDescription(dsName.getType()));
                    dsName = enumDataset.Next();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 读取在硬盘中的栅格文件
        /// </summary>
        /// <param name="filePath">栅格文件路径</param>
        /// <returns>IRasterDataset</returns>
        public static IRasterDataset OpenRasterDataset(string filePath)
        {
            IRasterDataset rasterDataset = null;
            string FolderName = Path.GetDirectoryName(filePath);   //由文件路径获取所在目录路径
            string FileName = Path.GetFileName(filePath);      //由文件路径获取文件名
            try
            {
                IRasterWorkspace ws = OpenRasterWorkspace(FolderName);
                rasterDataset = ws.OpenRasterDataset(FileName);
                return rasterDataset;
            }
            catch
            {
                return rasterDataset;
            }
        }

        /// <summary>
        /// 打开栅格工作空间
        /// </summary>
        /// <param name="folderPath">栅格文件所在文件夹</param>
        /// <returns>IRasterWorkspace</returns>
        public static IRasterWorkspace OpenRasterWorkspace(string folderPath)
        {
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesRaster.RasterWorkspaceFactory");
            IWorkspaceFactory wsFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            IRasterWorkspace rasterWS = (IRasterWorkspace)wsFactory.OpenFromFile(folderPath, 0);
            return rasterWS;
        }

        /// <summary>
        /// 打开一个RasterCatalog
        /// </summary>
        /// <param name="pWorkspace">工作空间</param>
        /// <param name="catalogName"></param>
        /// <returns></returns>
        public static IRasterCatalog OpenRasterCatalog(IWorkspace pWorkspace, string rasterCatalogName)
        {
            IRasterCatalog rasterCatalog = null;

            try
            {
                if (rasterCatalogName == null || rasterCatalogName == "")
                {
                    return null;
                }

                IRasterWorkspaceEx pRasterWorkspaceEx = (IRasterWorkspaceEx)pWorkspace;
                rasterCatalog = pRasterWorkspaceEx.OpenRasterCatalog(rasterCatalogName);
                ;
                return rasterCatalog;
            }
            catch
            {
                return null;
            }
        }

        public static IRasterDataset OpenRasterDatasetFromWorkspace(IWorkspace pWorkspace, string rasterDatasetName)
        {
            IRasterDataset rasterDataset = null;

            try
            {
                if (rasterDatasetName == null || rasterDatasetName == "")
                {
                    return null;
                }

                IRasterWorkspaceEx pRasterWorkspaceEx = (IRasterWorkspaceEx)pWorkspace;
                rasterDataset = pRasterWorkspaceEx.OpenRasterDataset(rasterDatasetName);
                ;
                return rasterDataset;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从GDB中打开栅格工作空间
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static IRasterWorkspaceEx OpenFileGDBWorkspace(string filePath)
        {
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory wsFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            IRasterWorkspaceEx rasterWS = (IRasterWorkspaceEx)wsFactory.OpenFromFile(filePath, 0);
            return rasterWS;
        }
    }
}
