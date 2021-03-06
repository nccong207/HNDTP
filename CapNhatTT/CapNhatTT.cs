using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress;
using DevExpress.XtraEditors;
using System.Windows.Forms;


namespace CapNhatTT
{
    public class CapNhatTT : ICData 
    {

        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;

        #region ICData Members

        public DataCustomData Data
        {
            set {data = value; }
        }

        public void ExecuteAfter()
        {
        }

        public void ExecuteBefore()
        {
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Modified || drMaster.RowState == DataRowState.Deleted)
            {
                if ((bool)drMaster["isGiaiNgan", DataRowVersion.Original])
                {
                    XtraMessageBox.Show("Bạn không được sửa/xóa dữ liệu khi dự án đã giải ngân", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    info.Result = false;
                    return;
                }
            }
            // Set lại giá trị mặc định của info
            info.Result = true;

            // Cập nhật trạng thái của dự án
            Update();
        }

        void Update()
        {
            if (data.CurMasterIndex < 0)
                return;
            drMaster = data.DsDataCopy.Tables[0].Rows[data.CurMasterIndex];
            DataView dvDetail= new DataView(data.DsDataCopy.Tables[1]);
            string sql = "";

            #region Khi thêm mới và trình duyệt = true
            if (drMaster.RowState == DataRowState.Added && (bool)drMaster["isTrinhDuyet"])
            {
                dvDetail.RowFilter = string.Format(@"MTTTID ='{0}'", drMaster["MTTTID"]);
                if (dvDetail.Count == 0) return;

                for (int i = 0; i < dvDetail.Count; i++)
                {
                    sql += string.Format(@" UPDATE MTDuAn Set 
                                            SoTTrinh = '{0}',NgayLapTT = '{1}'
                                            ,GhiChu2 = '{2}',TinhTrang = 4 
                                            WHERE MTID = '{3}' ",
                    drMaster["SoTTrinh"].ToString(), drMaster["NgayLap"].ToString(), drMaster["GhiChu"].ToString(), dvDetail[i].Row["MTID"].ToString());
                }

                info.Result = data.DbData.UpdateByNonQuery(sql);
            }
            #endregion

            #region Khi sửa và trình duyệt = true
            if (drMaster.RowState == DataRowState.Modified || drMaster.RowState == DataRowState.Unchanged)
            {
                sql = "";
                bool flag = false;// Ktra khi detail có thêm - sửa - xóa mới cập nhật tình trạng
                dvDetail.RowFilter = string.Format(@"MTTTID ='{0}'", drMaster["MTTTID"]);
               
                sql = string.Format(@"UPDATE	MTDuAn SET
                                            SoTTrinh = NULL, NgayLapTT = NULL
                                            ,GhiChu2 = NULL, TinhTrang = 3
                                    WHERE	SoTTrinh = N'{0}';", drMaster["SoTTrinh"]);
                
                

                if ((bool)drMaster["isTrinhDuyet"])
                {
                    for (int k = 0; k < dvDetail.Count; k++)
                    {
                        if (dvDetail[k].Row.RowState == DataRowState.Added
                            || dvDetail[k].Row.RowState == DataRowState.Modified || dvDetail[k].Row.RowState == DataRowState.Deleted)
                            flag = true;
                        sql += string.Format("UPDATE MTDuAn Set SoTTrinh = '{0}',NgayLapTT = '{1}',GhiChu2 = '{2}',TinhTrang = 4 WHERE MTID = '{3}';",
                                                drMaster["SoTTrinh"], drMaster["NgayLap"], drMaster["GhiChu"], dvDetail[k].Row["MTID"]);
                    }
                    if ((bool)drMaster["isTrinhDuyet", DataRowVersion.Original] == false)
                        flag = true;// TH khi thêm mới chưa check trình duyệt, khi sửa check trình duyệt
                }

                if (flag && !string.IsNullOrEmpty(sql))
                    info.Result = data.DbData.UpdateByNonQuery(sql);
            }
            #endregion

            #region Khi xóa tờ trình ...
            if (drMaster.RowState == DataRowState.Deleted)
            {
                sql = "";
                dvDetail.RowStateFilter = DataViewRowState.Deleted;
                if (dvDetail.Count == 0) return;

                for (int i = 0; i < dvDetail.Count; i++)
                {
                    sql += string.Format(" UPDATE MTDuAn Set SoTTrinh = null,NgayLapTT = null,GhiChu2 = null,TinhTrang = 3 WHERE MTID = '{0}' ",
                                             dvDetail[i].Row["MTID",DataRowVersion.Original].ToString());
                }
                if (sql != "")
                {
                    data.DbData.UpdateByNonQuery(sql);
                }
            }
            #endregion
        }


        public InfoCustomData Info
        {
            get { return info; }
        }

        #endregion
    }
}
