using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;

namespace GetLaiSuat
{
    public class GetLaiSuat : ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.SingleDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();

        DateEdit NgayLap = new DateEdit();
        CalcEdit LaiSuat = new CalcEdit();
        DataRow drMaster;
        GridLookUpEdit grNguonVon;

        #region ICControl Members

        public void AddEvent()
        {
            grNguonVon = data.FrmMain.Controls.Find("NguonVon_TenBP",true) [0] as GridLookUpEdit;
            grNguonVon.Popup += new EventHandler(grNguonVon_Popup);
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain,new EventArgs());
            //if (data.BsMain.Current == null)
            //    return;
            //drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
        }

        void grNguonVon_Popup(object sender, EventArgs e)
        {
            GridLookUpEdit gr = sender as GridLookUpEdit;
            GridView gv = gr.Properties.View as GridView;
            gv.ClearColumnsFilter();
            drMaster = (data.BsMain.Current as DataRowView).Row;
            gv.ActiveFilterString = string.Format("MaBP = '{0}' OR MaBP = 'TP' OR BPme = '{0}'", drMaster["BoPhan"]);
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataTable ds = data.BsMain.DataSource as DataTable;
            if (ds == null) return;
            if (data.BsMain.Current != null)
                drMaster = (data.BsMain.Current as DataRowView).Row;
            ds.ColumnChanged += new DataColumnChangeEventHandler(GetLaiSuat_ColumnChanged);
        }

        void GetLaiSuat_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted)
                return;
            if (e.Column.ColumnName.ToUpper().Equals("NGAYLAP") || e.Column.ColumnName.ToUpper().Equals("TENDA"))
            {
                if (e.Row["NgayLap"] == DBNull.Value) return;
                if (e.Row.RowState != DataRowState.Added) return;// Chỉ set tỉ lệ khi thêm mới
                using (DataTable dt = db.GetDataTable(string.Format("SELECT TOP 1 * FROM DMLS WHERE NgayAD <= '{0}' ORDER BY NgayAD DESC", e.Row["NgayLap"])))
                {
                    if (dt.Rows.Count == 0)
                    {
                        XtraMessageBox.Show("Chưa thiết lập mức lãi suất trong khoảng thời gian này", Config.GetValue("PackageName").ToString());
                    }
                    else
                    {
                        e.Row["LaiSuat"] = dt.Rows[0]["MucLS"];
                        e.Row["MucLSQH"] = dt.Rows[0]["MucLSQH"];
                        e.Row["TLQTK"] = dt.Rows[0]["TLQTK"];
                        e.Row["LSTKBB"] = dt.Rows[0]["LSTKBB"];
                        e.Row["LSTKTN"] = dt.Rows[0]["LSTKTN"];
                    }
                }
                e.Row.EndEdit();
            }
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
