using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using System.Data;
using DevExpress.XtraTab;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;

namespace TabVisible
{
    public class TabVisible : ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        XtraTabControl tbMain;
        GridLookUpEdit grdTT;
        DataRow drMaster;
        LayoutControl lc;
        //SpinEdit TongHoDN;
        //CalcEdit TvonDA;
        //CalcEdit TongVonDN;
        //int TSoHDN;
        //decimal TVonDN;
        TextEdit ghichu;
        GridView gvDetail;
        //CheckEdit GiaiNgan;
        GridLookUpEdit NguonVon;
        private string TTDA = "", TTCDA = "", TPTD = "", TTPD = "", TTNN = "";

        #region ICControl Members

        public void AddEvent()
        {
            grdTT = data.FrmMain.Controls.Find("TinhTrang",true)[0] as GridLookUpEdit;
            grdTT.Popup += new EventHandler(grdTT_Popup);
            lc = data.FrmMain.Controls.Find("LcMain", true)[0] as LayoutControl;     
            tbMain = data.FrmMain.Controls.Find("tcMain",true)[0] as XtraTabControl;
            //TongHoDN = data.FrmMain.Controls.Find("TSoHDN",true)[0] as SpinEdit;
            //TongVonDN = data.FrmMain.Controls.Find("TVonDNV",true)[0] as CalcEdit;
            //TvonDA = data.FrmMain.Controls.Find("TVonDA", true)[0] as CalcEdit;
            NguonVon = data.FrmMain.Controls.Find("NguonVon_TenBP",true) [0] as GridLookUpEdit;
            NguonVon.Popup += new EventHandler(NguonVon_Popup);
            ghichu = data.FrmMain.Controls.Find("GhiChu",true)[0] as TextEdit;
            gvDetail = (data.FrmMain.Controls.Find("DTHoVay", true)[0] as GridControl).MainView as GridView;
            ghichu.PropertiesChanged += new EventHandler(ghichu_PropertiesChanged);
            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
            if (data.BsMain.Current == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
        }
       
        void NguonVon_Popup(object sender, EventArgs e)
        {
            GridLookUpEdit glu = sender as GridLookUpEdit;
            GridView gv = glu.Properties.View as GridView;
            gv.ClearColumnsFilter();
            drMaster = (data.BsMain.Current as DataRowView).Row;
            gv.ActiveFilterString = "MaBP = '" + drMaster["BoPhan"].ToString() + "' OR MaBP = 'TP' OR BPme = '" + drMaster["BoPhan"].ToString() + "'";
        }

        void ghichu_PropertiesChanged(object sender, EventArgs e)
        {
            if (TTDA == "" || TTCDA == "" || TPTD == "" || TTPD == "" || TTNN == "")
                findGroup();
            if (ghichu.Properties.ReadOnly == true)
            {                 
                LayoutControlGroup lcg2 = lc.Items.FindByName(TPTD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg2.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
            }

            if (ghichu.Properties.ReadOnly == false)
            {
                if (drMaster["TinhTrang"].ToString() == "2" || drMaster["TinhTrang"].ToString() == "3")
                {
                    foreach (GridColumn col in  gvDetail.Columns)
                    {
                        if (col.Name == "clIsHoTro" || col.Name == "clDuocVay")
                        col.OptionsColumn.ReadOnly = false;
                        else
                        col.OptionsColumn.ReadOnly = true;
                    }
                }
                else
                {
                    foreach (GridColumn col in gvDetail.Columns)
                    {
                        col.OptionsColumn.ReadOnly = true;
                    }
                }
                
                #region    // Dự án đang lập
                if (drMaster["TinhTrang"].ToString() == "1")
                {
                    //Group Phê duyệt
                    LayoutControlGroup lcg2 = lc.Items.FindByName(TTPD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg2.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    // Group Thẩm định
                    LayoutControlGroup lcg4 = lc.Items.FindByName(TPTD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg4.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                }
                #endregion

                #region   // Dự án chờ thẩm định ,đã thẩm định

                if (drMaster["TinhTrang"].ToString() == "2" || drMaster["TinhTrang"].ToString() == "3")
                {

                    LayoutControlGroup lcg21 = lc.Items.FindByName(TPTD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg21.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = false;
                        }
                    }
                    //Group Thông tin dự án 
                    LayoutControlGroup lcg2 = lc.Items.FindByName(TTDA) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg2.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            if ((lci.Control as BaseEdit).Name == "TinhTrang")
                                continue;
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    //Group Thông tin chủ dự án
                    LayoutControlGroup lcg6 = lc.Items.FindByName(TTCDA) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg6.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                           
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    //Group Phê duyệt
                    LayoutControlGroup lcg7 = lc.Items.FindByName(TTPD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg7.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                }
                #endregion

                #region // Chờ duyệt
                if (drMaster["TinhTrang"].ToString() == "4")
                {

                    //Group Thông tin dự án 
                    LayoutControlGroup lcg2 = lc.Items.FindByName(TTDA) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg2.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            if ((lci.Control as BaseEdit).Name == "TinhTrang")
                                continue;
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    //Group Thông tin chủ dự án
                    LayoutControlGroup lcg8 = lc.Items.FindByName(TTCDA) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg8.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    // phê duyệt
                    LayoutControlGroup lcg4 = lc.Items.FindByName(TTPD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg4.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    // Group Thẩm định
                    LayoutControlGroup lcg5 = lc.Items.FindByName(TPTD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg5.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                }
                #endregion

                #region // Đã duyệt

                if (drMaster["TinhTrang"].ToString() == "5")
                {
                    //Group Thông tin dự án 
                    LayoutControlGroup lcg2 = lc.Items.FindByName(TTDA) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg2.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    //Group Thông tin chủ dự án
                    LayoutControlGroup lcg9 = lc.Items.FindByName(TTCDA) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg9.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    //Group Phê duyệt
                    LayoutControlGroup lcg10 = lc.Items.FindByName(TTPD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg10.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            if ((lci.Control as BaseEdit).Name == "SoQD" || (lci.Control as BaseEdit).Name == "NgayQD")
                                continue;
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                    // Group Thẩm định
                    LayoutControlGroup lcg3 = lc.Items.FindByName(TPTD) as LayoutControlGroup;
                    foreach (LayoutControlItem lci in lcg3.Items)
                    {
                        if ((lci.Control as BaseEdit) != null)
                        {
                            (lci.Control as BaseEdit).Properties.ReadOnly = true;
                        }
                    }
                }
                #endregion
            }
        }
        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(TabVisible_ColumnChanged);
            ds.Tables[2].ColumnChanged += new DataColumnChangeEventHandler(DTHoDan_ColumnChanged);
            ds.Tables[2].RowDeleted += new DataRowChangeEventHandler(TabVisible_RowDeleted);
        }

        void TabVisible_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null) return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            DataView dv = new DataView(ds.Tables[2]);
            dv.RowFilter = "MTID = '" + drMaster["MTID"].ToString() + "'";
            // Tính lại tổng cộng trong row master
            if (dv.Count == 0)
            {
                drMaster["TSoHo"] = 0;
                drMaster["TSoLD"] = 0;
                drMaster["TSoLDTH"] = 0;
                drMaster["TSoHDN"] = 0;
                drMaster["TVonCo"] = 0;
                drMaster["TVonVay"] = 0;
                drMaster["TVonDNV"] = 0;
                return;
            }
            else
            {
                using (DataTable dt = dv.ToTable())
                {
                    drMaster["TSoHo"] = dt.Compute("Count(IsHoTro)", "");               // Tổng hộ tham gia
                    drMaster["TSoLD"] = dt.Compute("Sum(SoLD)", "");                    // Tổng số lao động
                    drMaster["TSoLDTH"] = dt.Compute("Sum(SoLDTH)", "");                // Tổng số lao động thu hút
                    drMaster["TSoHDN"] = dt.Compute("Count(IsHoTro)", "IsHoTro = 1");   // Tổng số hộ được duyệt

                    drMaster["TVonCo"] = dt.Compute("Sum(VonTuCo)", "");                 // Tổng số vốn hiện có
                    drMaster["TVonVay"] = dt.Compute("Sum(CanVay)", "");                // Tổng số vốn xin vay
                    drMaster["TVonDNV"] = dt.Compute("Sum(DuocVay)", "IsHoTro = 1");    // Tổng số vốn được duyệt
                }
            }
        }

        void TabVisible_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
        }

        void DTHoDan_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null) return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            DataView dv = new DataView(ds.Tables[2]);
            dv.RowFilter = "MTID = '" + drMaster["MTID"].ToString() + "'";
            if (dv.Count == 0)
            {
                drMaster["TSoHo"] = 0;
                drMaster["TSoLD"] = 0;
                drMaster["TSoLDTH"] = 0;
                drMaster["TSoHDN"] = 0;
                drMaster["TVonCo"] = 0;
                drMaster["TVonVay"] = 0;
                drMaster["TVonDNV"] = 0;
                return;
            }

            if (e.Column.ColumnName.ToUpper().Equals("ISHOTRO"))
            {
                if ((bool)e.Row["IsHoTro"])
                    e.Row["DuocVay"] = e.Row["CanVay"];
                else
                    e.Row["DuocVay"] = 0;

                // Tính lại tổng cộng trong row master
                using (DataTable dt = dv.ToTable())
                {
                    drMaster["TSoHo"] = dt.Compute("Count(IsHoTro)", "");               // Tổng hộ tham gia
                    drMaster["TSoLD"] = dt.Compute("Sum(SoLD)", "");                    // Tổng số lao động
                    drMaster["TSoLDTH"] = dt.Compute("Sum(SoLDTH)", "");                // Tổng số lao động thu hút
                    drMaster["TSoHDN"] = dt.Compute("Count(IsHoTro)", "IsHoTro = 1");   // Tổng số hộ được duyệt

                    drMaster["TVonCo"] = dt.Compute("Sum(VonTuCo)","");                 // Tổng số vốn hiện có
                    drMaster["TVonVay"] = dt.Compute("Sum(CanVay)", "");                // Tổng số vốn xin vay
                    drMaster["TVonDNV"] = dt.Compute("Sum(DuocVay)", "IsHoTro = 1");    // Tổng số vốn được duyệt
                }
                e.Row.EndEdit();
            }

            if (e.Column.ColumnName.ToUpper().Equals("DUOCVAY"))
            {
                using (DataTable dt = dv.ToTable())
                {
                    drMaster["TVonDNV"] = dt.Compute("Sum(DuocVay)", "IsHoTro = 1");    // Tổng số vốn được duyệt
                }
            }
        }

        void grdTT_Popup(object sender, EventArgs e)
        {
            // ở mỗi tình trạng của dự án chỉ được change tới hoặc lui 1 mức.
            GridLookUpEdit grd = sender as GridLookUpEdit;
            GridView gv = grd.Properties.View as GridView;
            gv.ClearColumnsFilter();
            GridView gvTT = grd.Properties.View as GridView;
            gvTT.ClearColumnsFilter();
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster["TinhTrang"].ToString() == "1")
            {
                gvTT.ActiveFilterString = " Ma LIKE '[1-2]' ";
            }
            if (drMaster["TinhTrang"].ToString() == "2")
            {
                gvTT.ActiveFilterString = " Ma LIKE '[1-23]' ";
            }
            if (drMaster["TinhTrang"].ToString() == "3")
            {
                gvTT.ActiveFilterString = "  Ma LIKE '[2-34]' ";
            }
            if (drMaster["TinhTrang"].ToString() == "4")
            {
                gvTT.ActiveFilterString = "  Ma LIKE '[3-45]' ";
            }
            if (drMaster["TinhTrang"].ToString() == "5")
            {
                gvTT.ActiveFilterString = "  Ma LIKE '[4-5]' ";
            }
        }

        void findGroup()
        {
            //Tìm name của layout
            for (int i = 0; i < lc.Items.Count; i++)
            {
                if (lc.Items[i].TypeName == "LayoutGroup")
                {
                    switch (lc.Items[i].Text.Trim())
                    {
                        case "Thông tin dự án":
                            TTDA = lc.Items[i].Name;
                            break;
                        case "Thông tin chủ dự án":
                            TTCDA = lc.Items[i].Name;
                            break;
                        case "Thành phần thẩm định":
                            TPTD = lc.Items[i].Name;
                            break;
                        case "Thông tin phê duyệt":
                            TTPD = lc.Items[i].Name;
                            break;
                        case "Thông tin nhận nợ":
                            TTNN = lc.Items[i].Name;
                            break;
                    }
                }
            }
        }

        void FrmMain_Shown(object sender, EventArgs e)
        {
            if (TTDA == "" || TTCDA == "" || TPTD == "" || TTPD == "" || TTNN == "")
                findGroup();
            if (tbMain == null)
                return;
            tbMain.TabPages[0].PageVisible = false;
        }

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        #endregion
    }
}
