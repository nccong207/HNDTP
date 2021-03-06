using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;

namespace GetNoPhiGoc
{
    public class GetNoPhiGoc : ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;

        #region ICControl Members

        public void AddEvent()
        {
            Data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(GetNoPhiGoc_ColumnChanged);
        }

        void GetNoPhiGoc_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
            if (e.Column.ColumnName.ToUpper().Equals("HOVAY"))
            {
                string sql = string.Format(@"SELECT Min(TuNgay) TuNgay, Max(DenNgay) DenNgay 
                                             FROM MTTHUPHI m 
                                                    INNER JOIN DTHoVay d ON m.HoVay = d.DTID 
                                             WHERE HoVay = '{0}'",drMaster["HoVay"].ToString());
                DataTable dt = new DataTable();
                dt = db.GetDataTable(sql);
              //   TuNgay < NgayLap <= DenNgay -> nợ phí = 0
                if (DateTime.Compare(DateTime.Parse(dt.Rows[0]["TuNgay"].ToString()), DateTime.Parse(drMaster["NgayLap"].ToString())) < 0
                    && DateTime.Compare(DateTime.Parse(dt.Rows[0]["DenNgay"].ToString()), DateTime.Parse(drMaster["NgayLap"].ToString())) > 0)
                {
                    e.Row["NoPhiPT"] = 0;
                }
                //   DenNgay < NgayLap  -> nợ phí   
                else if (DateTime.Compare(DateTime.Parse(dt.Rows[0]["DenNgay"].ToString()), DateTime.Parse(drMaster["NgayLap"].ToString())) < 0)
                {
 
                }
            }
        }

        public DataCustomFormControl Data
        {
            set { data = value ; }
        }

        public InfoCustomControl Info
        {
            get { return info ; }
        }

        #endregion
    }
}
