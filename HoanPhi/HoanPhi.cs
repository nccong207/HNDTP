using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress.XtraGrid.Views.Grid;

namespace HoanPhi
{
    public class HoanPhi : ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;
        GridView grKH;

        #region ICControl Members

        public void AddEvent()
        {
            grKH = data.FrmMain.Controls.Find("MaKH",true ) [0] as GridView ;

            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged( data.BsMain,new EventArgs() );
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            if (data.BsMain.Current < 0)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(HoanPhi_ColumnChanged);
        }

        void HoanPhi_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
            // Hoàn phí khi đóng tiền nợ trước hạn 
            String sql = string.Format(@" SELECT DATEDIFF(MONTH,MAX(TuNgay),'{0}') SoThang
                                          FROM DTHoVay d INNER JOIN   MTTHUPHI t ON d.DTID = t.HoVay
                                          WHERE NguoiVay = '{1}'",drMaster["NgayCT"],drMaster["MaKH"].ToString());
            using (DataTable dt = new DataTable())
            {
                dt = db.GetDataTable(sql);
                if (dt.Rows.Count > 0 && drMaster["MaNV"].ToString() == "HOANPHI")
                {
                    if (dt.Rows[0]["SoThang"].ToString() = "0")
                    {
                        // Hoàn tiền phí 2 tháng

                    }
                    else if (dt.Rows[0]["SoThang"].ToString() = "1")
                    {
                        // Hoàn tiền phí 1 tháng
                    }
                    else
                    {
                        // Phí = 0;
                    }
                }
                else if (dt.Rows.Count == 0 && drMaster["MaNV"].ToString() == "HOANPHI")
                {
                    // Chưa đóng tiền phí
                }
            }
            // Hoàn phí khi đóng hết nợ :  
                    // - Quỹ tiết kiệm bắt buộc * Lãi suất
                    // - Quỹ tiết kiệm tự nguyện * Lãi suất

        }

        public DataCustomFormControl Data
        {
            set { data = value ; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }
        #endregion
    }
}
