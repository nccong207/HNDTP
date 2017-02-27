using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using DevExpress.XtraTab;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;

namespace DuAnVay
{
    public class DuAnVay:ICControl
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;

        GridView gvHoVay;
        LayoutControl lc;
        //XtraTabControl tbMain;
        GridLookUpEdit grdTT;

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        public void AddEvent()
        { 
            //tbMain = data.FrmMain.Controls.Find("tcMain", true)[0] as XtraTabControl;
            lc = data.FrmMain.Controls.Find("LcMain", true)[0] as LayoutControl;
            grdTT = data.FrmMain.Controls.Find("TinhTrang", true)[0] as GridLookUpEdit;
            GridControl gcHoVay = data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl;
            gvHoVay = (gcHoVay.MainView) as GridView;
            gvHoVay.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gvHoVay_CellValueChanged);
            RepositoryItemGridLookUpEdit gluSoCMND = gcHoVay.RepositoryItems["SoCMND"] as RepositoryItemGridLookUpEdit;
            gluSoCMND.QueryCloseUp += new System.ComponentModel.CancelEventHandler(gluSoCMND_QueryCloseUp);
            RepositoryItemGridLookUpEdit gluCMNDTK = gcHoVay.RepositoryItems["CMNDTK"] as RepositoryItemGridLookUpEdit;
            gluCMNDTK.QueryCloseUp += new System.ComponentModel.CancelEventHandler(gluSoCMND_QueryCloseUp);
            //grdTT.Popup += new EventHandler(grdTT_Popup);
            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
        }

        void gvHoVay_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "SOCMND")
                gvHoVay.UpdateCurrentRow();
        }

        void gluSoCMND_QueryCloseUp(object sender, System.ComponentModel.CancelEventArgs e)
        {
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster["NgayLap"].ToString() == "")
            {
                XtraMessageBox.Show("Cần nhập ngày lập dự án để kiểm tra hộ vay hợp lệ", Config.GetValue("PackageName").ToString());
                e.Cancel = true;
                return;
            }
            GridLookUpEdit glu = sender as GridLookUpEdit;
            if (glu.Properties.View.FocusedRowHandle >= 0 && glu.Properties.View.IsDataRow(glu.Properties.View.FocusedRowHandle))
            {
                string socmnd = glu.Properties.View.GetFocusedRowCellValue("SoCMND").ToString();
                string sohk = glu.Properties.View.GetFocusedRowCellValue("SoHoKhau").ToString();
                string ngaylap = drMaster["NgayLap"].ToString();
                if (!CheckNguoiVay(socmnd, sohk, DateTime.Parse(ngaylap)))
                    e.Cancel = true;
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            if (data.BsMain.Current != null)
                drMaster = (data.BsMain.Current as DataRowView).Row;

            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(Master_ColumnChanged);
            ds.Tables[1].ColumnChanged += new DataColumnChangeEventHandler(DTHoDan_ColumnChanged);
        }

        void Master_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;

            if (e.Column.ColumnName.ToUpper() == "TENDA")
            {
                if (e.Row.RowState == DataRowState.Added || e.Row.RowState == DataRowState.Modified)
                {
                    // Add mức phí, lãi suất ...
                    using (DataTable dtLS = db.GetDataTable(string.Format(@"SELECT TOP 1 * FROM DMLS
                                            WHERE	NgayAD <= '{0}'
                                            ORDER BY NgayAD DESC", drMaster["NgayLap"])))
                    {
                        if (dtLS.Rows.Count > 0)
                        {
                            e.Row["LaiSuat"] = dtLS.Rows[0]["MucLS"];
                            e.Row["MucLSQH"] = dtLS.Rows[0]["MucLSQH"];
                            e.Row["TLQTK"] = dtLS.Rows[0]["TLQTK"];
                            e.Row["LSTKBB"] = dtLS.Rows[0]["LSTKBB"];
                            e.Row["LSTKTN"] = dtLS.Rows[0]["LSTKTN"];
                            e.Row.EndEdit();
                        }
                    }
                }
            }
            
            if (e.Column.ColumnName.ToUpper() == "TINHTRANG" && e.ProposedValue != null && e.ProposedValue.ToString() != "")
                CapNhatTinhTrang(e.ProposedValue.ToString());
        }

        private void CapNhatTinhTrang(string value)
        {
            switch (value)
            {
                case "2":
                    //gvHoVay.OptionsBehavior.Editable = true;
                    //gvHoVay.Columns["DuocVay"].OptionsColumn.AllowEdit = false;
                    gvHoVay.Columns["isGiaiNgan"].Visible = false;
                    gvHoVay.Columns["isQuaHan"].Visible = false;
                    break;
                case "3":
                    //gvHoVay.OptionsBehavior.Editable = true;
                    //gvHoVay.Columns["DuocVay"].OptionsColumn.AllowEdit = true;
                    gvHoVay.Columns["isGiaiNgan"].Visible = false;
                    gvHoVay.Columns["isQuaHan"].Visible = false;
                    break;
                case "4":
                    //gvHoVay.OptionsBehavior.Editable = false;
                    gvHoVay.Columns["isGiaiNgan"].Visible = false;
                    gvHoVay.Columns["isQuaHan"].Visible = false;
                    break;
                case "5":
                    //gvHoVay.OptionsBehavior.Editable = false;
                    gvHoVay.Columns["isGiaiNgan"].Visible = true;
                    gvHoVay.Columns["isQuaHan"].Visible = true;
                    break;
            }
        }

        void DTHoDan_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null) return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
            
            // Khi change tien duoc vay check isHoTro
            if (e.Column.ColumnName.ToUpper().Equals("DUOCVAY"))
            {
                e.Row["IsHoTro"] = (decimal)e.Row["DuocVay"] > 0 ? true : false;
            }
            // cap nhat lai suat va tgvay tu master xuong detail
            if ((e.Row.RowState == DataRowState.Added || e.Row.RowState == DataRowState.Detached) 
                && e.Column.ColumnName.ToUpper().Equals("NGUOIVAY"))
            {
                e.Row["LaiSuat"] = drMaster["LaiSuat"];
                e.Row["TGVay"] = drMaster["THVay"];
                e.Row.EndEdit();
            }
        }

        void FrmMain_Shown(object sender, EventArgs e)
        {
            if (data.BsMain.DataSource == null || data.BsMain.Current == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster == null) return;
            
            if (drMaster.RowState == DataRowState.Added)
            {
                // Add mức phí, lãi suất ...
                using (DataTable dtLS = db.GetDataTable(string.Format(@"SELECT TOP 1 * FROM DMLS
                                            WHERE	NgayAD <= '{0}'
                                            ORDER BY NgayAD DESC", drMaster["NgayLap"])))
                {
                    if (dtLS.Rows.Count > 0)
                    {
                        drMaster["LaiSuat"] = dtLS.Rows[0]["MucLS"];
                        drMaster["MucLSQH"] = dtLS.Rows[0]["MucLSQH"];
                        drMaster["TLQTK"] = dtLS.Rows[0]["TLQTK"];
                        drMaster["LSTKBB"] = dtLS.Rows[0]["LSTKBB"];
                        drMaster["LSTKTN"] = dtLS.Rows[0]["LSTKTN"];
                    }
                }
                if (drMaster["TinhTrang"].ToString() != "")
                    CapNhatTinhTrang(drMaster["TinhTrang"].ToString());
            }

            //if (tbMain == null)
            //    return;
            //tbMain.TabPages[0].PageVisible = false;
        }

        // chưa dùng
        //void findGroup()
        //{
        //    //Tìm name của layout
        //    for (int i = 0; i < lc.Items.Count; i++)
        //    {
        //        if (lc.Items[i].TypeName == "LayoutGroup")
        //        {
        //            switch (lc.Items[i].Text.Trim())
        //            {
        //                case "Thông tin dự án":
        //                    TTDA = lc.Items[i].Name;
        //                    break;
        //                case "Thông tin chủ dự án":
        //                    TTCDA = lc.Items[i].Name;
        //                    break;
        //                case "Thành phần thẩm định":
        //                    TPTD = lc.Items[i].Name;
        //                    break;
        //                case "Thông tin phê duyệt":
        //                    TTPD = lc.Items[i].Name;
        //                    break;
        //                case "Thông tin nhận nợ":
        //                    TTNN = lc.Items[i].Name;
        //                    break;
        //            }
        //        }
        //    }
        //}

        //void grdTT_Popup(object sender, EventArgs e)
        //{
        //    // ở mỗi tình trạng của dự án chỉ được change tới hoặc lui 1 mức.
        //    GridLookUpEdit grd = sender as GridLookUpEdit;
        //    GridView gv = grd.Properties.View as GridView;
        //    gv.ClearColumnsFilter();
        //    GridView gvTT = grd.Properties.View as GridView;
        //    gvTT.ClearColumnsFilter();
        //    drMaster = (data.BsMain.Current as DataRowView).Row;
        //    if (drMaster["TinhTrang"].ToString() == "1")
        //    {
        //        gvTT.ActiveFilterString = " Ma LIKE '[1-2]' ";
        //    }
        //    if (drMaster["TinhTrang"].ToString() == "2")
        //    {
        //        gvTT.ActiveFilterString = " Ma LIKE '[1-23]' ";
        //    }
        //    if (drMaster["TinhTrang"].ToString() == "3")
        //    {
        //        gvTT.ActiveFilterString = "  Ma LIKE '[2-34]' ";
        //    }
        //    if (drMaster["TinhTrang"].ToString() == "4")
        //    {
        //        gvTT.ActiveFilterString = "  Ma LIKE '[3-45]' ";
        //    }
        //    if (drMaster["TinhTrang"].ToString() == "5")
        //    {
        //        gvTT.ActiveFilterString = "  Ma LIKE '[4-5]' ";
        //    }
        //}

        // Ktra thông tin Hộ vay - người thừa kế
        bool CheckNguoiVay(string SoCMND, string SoHK, DateTime NgayLap)
        {
            using (DataTable dt = db.GetDataTable(string.Format(@"EXEC sp_CheckNguoiVay '{0}','{1}','{2}'", SoCMND, SoHK, NgayLap)))
            {
                if (dt.Rows.Count == 0) return true;

                XtraMessageBox.Show("Điều kiện vay chưa hợp lệ, vui lòng kiểm tra lại các yếu tố sau:\nĐộ tuổi: Nam từ 18-60 tuổi, Nữ: 18-55 tuổi\nHộ dân đang thuộc dự án vay vốn khác\nHộ dân đang là người thừa kế trong dự án khác\nHộ dân có số hộ khẩu trùng với người vay trong dự án khác", Config.GetValue("PackageName").ToString());
                return false;
            }
        }
    }
}
