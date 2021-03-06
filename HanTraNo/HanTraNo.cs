using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;

namespace HanTraNo
{
    public class HanTraNo : ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;
        SpinEdit THvay;
        DateEdit NgayNN;
        GridLookUpEdit grQD;
        GridView grDetail;

        #region ICControl Members

        public void AddEvent()
        {
            THvay = data.FrmMain.Controls.Find("THVay",true) [0] as SpinEdit;
            NgayNN = data.FrmMain.Controls.Find("NgayNN",true) [0] as DateEdit;
            grQD = data.FrmMain.Controls.Find("SoQD",true) [0] as GridLookUpEdit;
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            grDetail = (data.FrmMain.Controls.Find("gcMain",true) [0] as GridControl).MainView as GridView ;        
            BsMain_DataSourceChanged(data.BsMain,new EventArgs ());
        }
         
        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            if(data.BsMain.Current != null)
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(HanTraNo_ColumnChanged);
            ds.Tables[1].ColumnChanged +=new DataColumnChangeEventHandler(DetailHanTraNo_ColumnChanged);
        }

        void HanTraNo_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
            drMaster = e.Row;
            if (e.Column.ColumnName.ToUpper().Equals("XACNHAN"))
            {
                if ((bool)e.Row["XacNhan"])
                {
                    XtraMessageBox.Show("Khi xác nhận đã giải ngân, bạn không thể sửa/xóa thông tin của Giấy nhận nợ/giải ngân", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (e.Column.ColumnName.ToUpper().Equals("NGAYNN") || e.Column.ColumnName.ToUpper().Equals("THVAY"))
            {
                if (drMaster["THVay"] != DBNull.Value)
                {
                    e.Row["THTra"] = DateTime.Parse(e.Row["NgayNN"].ToString()).AddMonths(int.Parse(e.Row["THVay"].ToString()));
                    e.Row.EndEdit();
                }
            }
            if (e.Column.ColumnName.ToUpper().Equals("SOQD"))
            {
                if (e.Row["SoQD"].ToString() != "")
                {
                    string sql = string.Format(@"SELECT DTID,NguonVon,m.LaiSuat
                                                FROM    DTHoVay d INNER JOIN MTDuAn m ON d.MTID = m.MTID
                                                WHERE   m.MTID IN( SELECT MTID FROM DTQD WHERE MTQDID = '{0}')
                                                        AND IsHoTro = 1", e.Row["SoQD"]);
                    using (DataTable dt = db.GetDataTable(sql))
                    {
                        if (dt.Rows.Count == 0)
                            return;
                        // Bổ sung: Tự set tỷ lệ lãi suất
                        e.Row["PhiVay"] = dt.Rows[0]["LaiSuat"];
                        // Xóa detail trước khi add mới
                        for (int i = grDetail.DataRowCount - 1; i >= 0; i--)
                            grDetail.DeleteRow(i);

                        // Add detail row
                        foreach (DataRow row in dt.Rows)
                        {
                            grDetail.AddNewRow();
                            grDetail.SetFocusedRowCellValue(grDetail.Columns["HoVay"], row["DTID"]);
                            grDetail.SetFocusedRowCellValue(grDetail.Columns["NguonVon"], row["NguonVon"]);
                            grDetail.UpdateCurrentRow();
                        }
                    }
                }
                else
                {
                    for (int i = grDetail.DataRowCount - 1; i >= 0; i--)
                        grDetail.DeleteRow(i);
                }
            }
        }
        
        void DetailHanTraNo_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
            if (e.Column.ColumnName.ToUpper().Equals("PSVAY"))
            {
                if ((decimal)e.Row["PsVay"] > (decimal)e.Row["DuocVay"])
                {
                    e.Row["PsVay"] = e.Row["DuocVay"];
                }
            }
        }

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get {return info; }
        }

        #endregion
    }
}
