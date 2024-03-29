using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress.XtraEditors;
using System.Windows.Forms;

namespace CapNhatGiaiNgan
{
    public class CapNhatGiaiNgan : ICData 
    {
        private InfoCustomData info;
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;
        public CapNhatGiaiNgan()
        {
            info = new InfoCustomData(IDataType.MasterDetailDt);
        }

        #region ICData Members

        public DataCustomData Data
        {
            set { data = value; }
        }

        public void ExecuteAfter()
        {
            drMaster = data.DsDataCopy.Tables[0].Rows[data.CurMasterIndex];
            // Cập nhật thông tin của dự án khi check xác nhận hoàn thành
            string isGN = "";
            if (drMaster.RowState != DataRowState.Deleted && (bool)drMaster["XacNhan"])
                isGN = "1";
            if (drMaster.RowState == DataRowState.Modified && (bool)drMaster["XacNhan", DataRowVersion.Original] && !(bool)drMaster["XacNhan"])
                isGN = "0";
            if (drMaster.RowState == DataRowState.Deleted && (bool)drMaster["XacNhan", DataRowVersion.Original])
                isGN = "0";
            if (isGN != "")
                Update(isGN);
        }

        public void ExecuteBefore()
        {
            CheckState();
        }
        void CheckState()
        {
            // B/sung cho TH da set gia tri false
            info.Result = true;

            if (data.CurMasterIndex < 0)
                return;
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState != DataRowState.Deleted && (bool)drMaster["XacNhan"]
                && (drMaster["TGiaiNgan"] == DBNull.Value || Convert.ToDecimal(drMaster["TGiaiNgan"]) == 0))
            {
                XtraMessageBox.Show("Cần nhập số tiền giải ngân trước khi đánh dấu giải ngân", Config.GetValue("PackageName").ToString());
                info.Result = false;
                return;
            }

            if ((drMaster.RowState == DataRowState.Modified || drMaster.RowState == DataRowState.Deleted)
                &&(bool)drMaster["XacNhan", DataRowVersion.Original])
            {
                //kiem tra cac du an giai ngan da co thu phi hay chua?
                using (DataView dvDT = new DataView(data.DsData.Tables[1]))
                {
                    string duans = "";
                    dvDT.RowStateFilter = DataViewRowState.CurrentRows | DataViewRowState.Deleted;
                    dvDT.RowFilter = "MTID = '" + drMaster["MTID", DataRowVersion.Original] + "'";
                    foreach (DataRowView drv in dvDT)
                        if (!duans.Contains(drv["DuAn"].ToString()))
                            duans += "'" + drv["DuAn"].ToString() + "',";
                    duans = duans.Remove(duans.Length - 1);
                    if (duans != "")
                    {
                        DataTable dtVP = db.GetDataTable("select MTDAID from MTVonPhi where MTDAID in (" + duans + ")");
                        if (dtVP.Rows.Count > 0)
                        {
                            XtraMessageBox.Show("Bạn không được sửa/xóa dữ liệu khi dự án đã thu phí (vốn)!", Config.GetValue("PackageName").ToString());
                            info.Result = false;
                            return;
                        }

                    }
                }
            }
        }

        void Update(string isGN)
        {
            data.DbData.EndMultiTrans();
            string sql = "";
            // Cập nhật tình trạng đã giải ngân cho DTHoVay,MTDuAn,MTQD,MTTTrinh
            if (data.CurMasterIndex < 0)
                return;
            drMaster = data.DsDataCopy.Tables[0].Rows[data.CurMasterIndex];
            var mtqdid = drMaster["SoQD"].ToString();
            // update DTHoVay
            sql += string.Format(@" UPDATE DTHoVay SET isGiaiNgan = {3} ,NgayNN = '{0}' ,NgayTN = '{1}'
                                    WHERE DTID IN (SELECT HoVay FROM DTNhanNo WHERE MTID = '{2}' AND PsVay >0)",
                                    drMaster["NgayNN"], drMaster["THTra"], drMaster["MTID"].ToString(), isGN);
            // update MTDuAn
            sql += string.Format(@" UPDATE  MTDuAn SET 
                                            NgayNN = '{0}',SoNNo = '{1}',THTra = '{2}',DaiDien = '{3}'
                                            ,CMNDDD = '{4}',NgayCap = {5},NoiCap = {6}
                                            ,CVDaiDien = N'{7}',SoHieuTK = N'{8}',NganHang = N'{9}',GhiChu3 = N'{10}', GiaiNgan = {12} 
                                    WHERE   MTID IN ( SELECT MTID FROM DTQD WHERE MTQDID ='{11}')"
                                    , drMaster["NgayNN"], drMaster["SoNNo"], drMaster["THTra"], drMaster["DaiDien"], drMaster["SoCMND"]
                                    , drMaster["NgayCap"] != DBNull.Value ? string.Format("'{0}'", drMaster["NgayCap"]) : "NULL"
                                    , drMaster["NoiCap"] != DBNull.Value ? drMaster["NoiCap"] : "NULL"
                                    , drMaster["CVDaiDien"], drMaster["SoHieuTK"], drMaster["NganHang"], drMaster["GhiChu"], mtqdid, isGN);
            // update quyết định
            sql += string.Format(@" UPDATE MTQD SET isGiaiNgan = {1} 
                                    WHERE MTQDID ='{0}'",mtqdid, isGN);
            // update tờ trình
            sql += string.Format(@" update tt
                                    set tt.isGiaiNgan = {1}
                                    from MTTTrinh tt 
                                    inner join DTQD dt on tt.MTTTID = dt.MTTTID
                                    where dt.MTQDID = '{0}'", mtqdid, isGN);
            db.UpdateByNonQuery(sql);
        }

        public InfoCustomData Info
        {
            get { return info; }
        }

        #endregion
    }
}
