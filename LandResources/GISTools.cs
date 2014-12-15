using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;

namespace LandResources
{
    public class GISTools
    {
        /// <summary>
        /// 设置当前工具为空
        /// </summary>
        /// <param name="axMapC"></param>
        public static void setNull(ESRI.ArcGIS.Controls.AxMapControl axMapC)
        {
            IMapControl2 mapControl = (IMapControl2)axMapC.Object;
            mapControl.CurrentTool = null;
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        public static void AddData(ESRI.ArcGIS.Controls.AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsAddDataCommandClass();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 放大
        /// </summary>
        public static void ZoomIn(ESRI.ArcGIS.Controls.AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapZoomInToolClass();
            pCommand.OnCreate(AxMapC.Object);
            AxMapC.CurrentTool = (ITool)pCommand;
        }
        /// <summary>
        /// 缩小
        /// </summary>
        public static void ZoomOut(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapZoomOutToolClass();
            pCommand.OnCreate(AxMapC.Object);
            AxMapC.CurrentTool = (ITool)pCommand;
        }
        /// <summary>
        /// 查询要素信息
        /// </summary>
        public static void IdentifyTool(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapIdentifyToolClass();
            pCommand.OnCreate(AxMapC.Object);
            AxMapC.CurrentTool = (ITool)pCommand;
        }
        /// <summary>
        /// 移动
        /// </summary>
        public static void Pan(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapPanToolClass();
            pCommand.OnCreate(AxMapC.Object);
            AxMapC.CurrentTool = (ITool)pCommand;
        }
        /// <summary>
        /// 比例放大
        /// </summary>
        public static void ZoomInFix(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapZoomInFixedCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 比例缩小
        /// </summary>
        public static void ZoomOutFix(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapZoomOutFixedCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 左移
        /// </summary>
        public static void PanLeft(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapLeftCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 右移
        /// </summary>
        public static void PanRight(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapRightCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 上移
        /// </summary>
        public static void PanUp(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapUpCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 下移
        /// </summary>
        public static void PanDown(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapDownCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 上移场景
        /// </summary>
        public static void LastExtentBack(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapZoomToLastExtentBackCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 下移场景
        /// </summary>
        public static void LastExtentForward(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsMapZoomToLastExtentForwardCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 全屏显示
        /// </summary>
        public static void FullExtend(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ESRI.ArcGIS.Controls.ControlsMapFullExtentCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 选择
        /// </summary>
        public static void SelectFeature(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsSelectFeaturesToolClass();
            pCommand.OnCreate(AxMapC.Object);
            AxMapC.CurrentTool = (ITool)pCommand;
        }
        /// <summary>
        /// 反选
        /// </summary>
        public static void SwitchSelection(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsSwitchSelectionCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 放大到选择对象
        /// </summary>
        public static void ZoomToFeatures(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsZoomToSelectedCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 清除选择
        /// </summary>
        public static void ClearSelect(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsClearSelectionCommand();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /// <summary>
        /// 选择要素
        /// </summary>
        public static void SelectByGraphic(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsSelectToolClass();
            pCommand.OnCreate(AxMapC.Object);
            AxMapC.CurrentTool = (ITool)pCommand;
        }
        /// <summary>
        /// 全选
        /// </summary>
        public static void SelectAll(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsSelectAllCommandClass();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        public static void SaveMxdFile(AxMapControl AxMapC)
        {
            ICommand pCommand;
            pCommand = new ControlsSaveAsDocCommandClass();
            pCommand.OnCreate(AxMapC.Object);
            pCommand.OnClick();
        }
        /*
        /// <summary>
        /// 属性查询
        /// </summary>
        public void Identify(AxMapControl AxMapC, frmMain form)
        {
            IdentityService service = new IdentityService();
            service.FeatureIdentity(form, AxMapC);
        }
        /// <summary>
        /// 量测
        /// </summary>
        public void Measure(AxMapControl AxMapC, frmMain form)
        {
            IdentityService service = new IdentityService();
            service.Measure(form, AxMapC);
        }
         */
        /// <summary>
        /// 刷新
        /// </summary>
        public void barRefresh()
        {
            //IGraphicsContainer pDeleteElements = define.define.AxMapC.ActiveView.FocusMap as IGraphicsContainer;
            //pDeleteElements.DeleteAllElements();
            //define.define.AxMapC.ActiveView.Refresh();
        }

        /// <summary>
        /// 查询定位
        /// </summary>
        //public void FindAndLocate()
        //{
        //    Tools.frmFindAndLocate sf = new Tools.frmFindAndLocate();
        //    sf.ShowDialog();
        //}

        /// <summary>
        /// 根据项目地图的不同，自定义全图工具
        /// </summary>
        /// <param name="axMapControl"></param>
        public static void FullExtentBySelfDefine(AxMapControl axMapControl)
        {
            IEnvelope evp = new EnvelopeClass();
            evp.XMax = 138.197944;
            evp.XMin = 70.335832;
            evp.YMax = 14.680281;
            evp.YMin = 57.216853;
            axMapControl.ActiveView.Extent = evp;
            axMapControl.ActiveView.Refresh();
        }
    }
}
