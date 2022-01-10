using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using CDTDatabase;
using CDTLib;
using Plugins;

using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using System.Windows.Forms;

namespace ChonCQCapVon
{
    public class ChonCQCapVon: ICControl
    {
        DataCustomFormControl data;
        InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        Database db = Database.NewDataDatabase();
        DataRow drMtDuAn;
        DataTable dtNguonVon;
        GridLookUpEdit glueXaPhuong;
        GridLookUpEdit glueThuocNguon;

        public DataCustomFormControl Data {
            set { data = value; }
        }
        public InfoCustomControl Info {
            get { return info; }
        }
        public void AddEvent()
        {
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
            
        }
        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds != null)
                ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(ChonCQCapVon_ColumnChanged);
        }
        List<string> lst2 = null;
        void ChonCQCapVon_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            try
            {
                drMtDuAn = (data.BsMain.Current as DataRowView).Row;
                List<string> lst = new List<string>(new string[] { "PhuongXa", "NguonVon", "NgayLap" });
                if (drMtDuAn.RowState != DataRowState.Added && drMtDuAn.RowState != DataRowState.Modified
                    && drMtDuAn.RowState != DataRowState.Unchanged)
                    return;
                string maNguon = string.Empty;
                if (lst.Contains(e.Column.ColumnName))
                {
                    if (e.Row["PhuongXa"] != null && !string.IsNullOrEmpty(e.Row["PhuongXa"].ToString())
                        && e.Row["NguonVon"] != null && !string.IsNullOrEmpty(e.Row["NguonVon"].ToString()))
                    {
                        CalcLaiSuat(e.Row["NguonVon"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message.ToString());
            }
            
        }

        void CalcLaiSuat(string maBP)
        {
            dtNguonVon = db.GetDataTable(string.Format("select top 1 n.* from DMNguon n inner join wNguonVon w on n.MaNguon = w.Cap where w.MaBP = '{0}' and n.NgayAD <= '{1}' ORDER BY n.NgayAD desc",
                maBP, drMtDuAn["NgayLap"]));
            lst2 = new List<string>(new string[] { "LaiSuat", "TrichHQ", "TrichXP", "MucLSQH" });
            if (dtNguonVon.Rows.Count == 0)
            {
                XtraMessageBox.Show(string.Format("Không tìm thấy thiết lập lãi suất của nguồn vốn {0} tại thời điểm {1:dd/MM/yyyy}",
                    maBP, drMtDuAn["NgayLap"]), Config.GetValue("PackageName").ToString());
                foreach (string s in lst2)
                    drMtDuAn[s] = DBNull.Value;
                drMtDuAn["NguonID"] = DBNull.Value;
                drMtDuAn.EndEdit();
                return;
            }
            foreach (string s in lst2)
                drMtDuAn[s] = dtNguonVon.Rows[0][s];
            drMtDuAn["NguonID"] = dtNguonVon.Rows[0]["MaNguon"];
            drMtDuAn.EndEdit();
        }
    }
}
