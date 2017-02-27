using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using System.Windows.Forms;

namespace XLHoanQTK
{
    public class XLHoanQTK : ICControl
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;

        public void AddEvent()
        {
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(XLHoanQTK_ColumnChanged);
        }

        void XLHoanQTK_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (drMaster == null)
                drMaster = e.Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;

            if (e.Column.ColumnName.ToUpper() == "HOVAY")
            {
                if (e.Row[e.Column.ColumnName] == DBNull.Value)
                {
                    e.Row["TTienBB"] = 0;
                    e.Row["TTienTN"] = 0;
                    e.Row["TLBB"] = 0;
                    e.Row["TLTN"] = 0;
                    e.Row["DuAn"] = DBNull.Value;
                    return;
                }

                string sql = string.Format(@" DECLARE @HoVay uniqueidentifier
                                SET @HoVay = '{0}'

                                SELECT  hv.DTID, da.MTID, da.LSTKBB, da.LSTKTN
                                        , SUM(ISNULL(tp.TienBB,0)) TienBB, SUM(ISNULL(tp.TienTN,0)) TienTN
                                FROM    DTHoVay AS hv 
		                                LEFT OUTER JOIN MTTHUPHI AS tp ON hv.DTID = tp.HoVay
		                                INNER JOIN MTDuAn AS da ON hv.MTID = da.MTID
                                WHERE	HoVay = @HoVay
                                GROUP BY hv.DTID, da.MTID, da.LSTKBB, da.LSTKTN", e.Row[e.Column.ColumnName]);
                using (DataTable dt = db.GetDataTable(sql))
                {
                    if (dt.Rows.Count > 0)
                    {
                        e.Row["TTienBB"] = dt.Rows[0]["TienBB"];
                        e.Row["TTienTN"] = dt.Rows[0]["TienTN"];
                        e.Row["TLBB"] = dt.Rows[0]["LSTKBB"];
                        e.Row["TLTN"] = dt.Rows[0]["LSTKTN"];
                        e.Row["DuAn"] = dt.Rows[0]["MTID"];
                    }
                    else
                    {
                        e.Row["TTienBB"] = 0;
                        e.Row["TTienTN"] = 0;
                        e.Row["TLBB"] = 0;
                        e.Row["TLTN"] = 0;
                        e.Row["DuAn"] = DBNull.Value;
                    }
                }
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

    }
}
