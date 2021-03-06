using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using System.Windows.Forms;


namespace CapNhatQD
{
    public class CapNhatQD : ICData 
    {
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;
        DataView dvDetail;

        #region ICData Members

        public DataCustomData Data
        {
            set { data = value ; }
        }

        public void ExecuteAfter()
        {
            Update();
        }

        public void ExecuteBefore()
        {
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
            {
                if (Boolean.Parse(drMaster["isGiaiNgan", DataRowVersion.Original].ToString()))
                {
                    XtraMessageBox.Show("Bạn không được chỉnh sửa dữ liệu khi dự án đã giải ngân", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    info.Result = false;
                }
            }
            else if (drMaster.RowState == DataRowState.Modified)
            {
                if (Boolean.Parse(drMaster["isGiaiNgan"].ToString()))
                {
                    XtraMessageBox.Show("Bạn không được chỉnh sửa dữ liệu khi dự án đã giải ngân", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    info.Result = false;
                }
            }
        }

        void Update()
        {
            if (data.CurMasterIndex < 0)
                return;
            string sql = "";
            drMaster = data.DsDataCopy.Tables[0].Rows[data.CurMasterIndex];
            dvDetail = new DataView(data.DsDataCopy.Tables[1]);
            dvDetail.RowFilter = "MTQDID = '" + drMaster["MTQDID"].ToString() + "'";
            if (drMaster.RowState == DataRowState.Deleted)
            {
                sql = "";
                dvDetail.RowStateFilter = DataViewRowState.Deleted;
                for (int i = 0; i < dvDetail.Count; i++)
                {
                    sql += string.Format(" UPDATE MTDuAn SET SoQD = null,NgayQD = null,Ghichu2 = null,TinhTrang = 4 WHERE MTID = '{0}'",
                                         dvDetail[i].Row["MTID", DataRowVersion.Original].ToString());
                }
                if (sql != "")
                {
                    data.DbData.UpdateByNonQuery(sql);
                }
            }
            if (drMaster.RowState == DataRowState.Added)
            {
                sql = "";
                for (int i = 0; i < dvDetail.Count; i++)
                {
                    sql += string.Format(" UPDATE MTDuAn SET SoQD = '{0}',NgayQD = '{1}',Ghichu2 = '{2}',TinhTrang = 5 WHERE MTID = '{3}'",
                                          drMaster["SoQD"].ToString(), drMaster["NgayLap"]
                                          , drMaster["GhiChu"].ToString(), dvDetail[i].Row["MTID"].ToString());                    
                }
                data.DbData.UpdateByNonQuery(sql);
            }
            if (drMaster.RowState == DataRowState.Modified || drMaster.RowState == DataRowState.Unchanged)
            {
                sql = "";
                dvDetail.RowFilter = " DTQDID ='" + drMaster["MTQDID"].ToString() + "'";
                dvDetail.RowStateFilter = DataViewRowState.Added;
                for (int i = 0; i < dvDetail.Count; i++)
                {
                    sql += string.Format(" UPDATE MTDuAn SET SoQD = '{0}',NgayQD = '{1}',Ghichu2 = '{2}',TinhTrang = 5 WHERE MTID = '{3}'",
                                          drMaster["SoQD"].ToString(), drMaster["NgayLap"]
                                          , drMaster["GhiChu"].ToString(), dvDetail[i].Row["MTID"].ToString());
                    
                }
                if (sql != "")
                {
                    data.DbData.UpdateByNonQuery(sql);
                }
                dvDetail.RowFilter = "";
                dvDetail.RowStateFilter = DataViewRowState.Deleted;
                sql = "";
                for (int i = 0; i < dvDetail.Count; i++)
                {
                    sql += string.Format(" UPDATE MTDuAn SET SoQD = null,NgayQD = null,Ghichu2 = null,TinhTrang = 4 WHERE MTID = '{0}'",
                                           dvDetail[i].Row["MTID", DataRowVersion.Original].ToString());                    
                }
                if (sql != "")
                {
                    data.DbData.UpdateByNonQuery(sql);
                }
                sql = "";
                dvDetail.RowFilter = "";
                dvDetail.RowStateFilter = DataViewRowState.Unchanged;
                for (int i = 0; i < dvDetail.Count; i++)
                {
                    sql += string.Format(" UPDATE MTDuAn SET SoQD = '{0}',NgayQD = '{1}',Ghichu2 = '{2}',TinhTrang = 5 WHERE MTID = '{3}'",
                                          drMaster["SoQD"].ToString(), drMaster["NgayLap"]
                                          , drMaster["GhiChu"].ToString(), dvDetail[i].Row["MTID"].ToString());

                }
                if (sql != "")
                {
                    data.DbData.UpdateByNonQuery(sql);
                }

            }
            dvDetail.RowFilter = "";
        }

        public InfoCustomData Info
        {
            get { return info ; }
        }

        #endregion
    }
}
