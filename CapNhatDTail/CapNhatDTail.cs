using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;

namespace CapNhatDTail
{
    public class CapNhatDTail : ICData
    {
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;

        #region ICData Members

        public DataCustomData Data
        {
            set { data = value ; }
        }

        public void ExecuteAfter()
        {
            // Cập nhật tiền vay còn lại
            CapNhatNo();
        }

        public void ExecuteBefore()
        {
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            
            using (DataTable dt = db.GetDataTable(@"SELECT * FROM DMNV 
                                                    WHERE   MaNV IN('THUHOIVON','THUPHI','TPQHTM')"))
            {
                if (drMaster.RowState == DataRowState.Added)
                {
                    DataRow row;
                    foreach (DataRow _row in dt.Rows)
                    {
                        // Tiền vốn vay
                        if (_row["MaNV"].ToString().ToUpper() == "THUHOIVON")
                        {
                            row = data.DsData.Tables[1].NewRow();
                            row["MTTNID"] = drMaster["MTTNID"];
                            row["MaNV"] = "THUHOIVON";
                            row["TaiKhoan"] = _row["TKDU1"];
                            row["SoTien"] = drMaster["TienNop"];
                            row["GhiChu"] = "Thu tiền vốn vay";
                            data.DsData.Tables[1].Rows.Add(row);
                        }
                        // Tiền phí trong hạn
                        if (_row["MaNV"].ToString().ToUpper() == "THUPHI")
                        {
                            if ((decimal)drMaster["PsTTH"] > 0)
                            {
                                row = data.DsData.Tables[1].NewRow();
                                row["MTTNID"] = drMaster["MTTNID"];
                                row["MaNV"] = "THUPHI";
                                row["TaiKhoan"] = _row["TKDU1"];
                                row["GhiChu"] = "Tiền phí trong hạn";
                                row["SoTien"] = drMaster["PsTTH"];
                                data.DsData.Tables[1].Rows.Add(row);
                            }
                        }
                        // Tiền phí quá hạn
                        if (_row["MaNV"].ToString().ToUpper() == "TPQHTM")
                        {
                            if ((decimal)drMaster["PsPhiQH"] > 0)
                            {
                                row = data.DsData.Tables[1].NewRow();
                                row["MTTNID"] = drMaster["MTTNID"];
                                row["MaNV"] = "THUPHI";
                                row["TaiKhoan"] = _row["TKDU1"];
                                row["GhiChu"] = "Tiền phí quá hạn";
                                row["SoTien"] = drMaster["PsPhiQH"];
                                data.DsData.Tables[1].Rows.Add(row);
                            }
                        }
                    }
                }

                if (drMaster.RowState == DataRowState.Modified)
                {
                    foreach (DataRow row in data.DsData.Tables[1].Select(string.Format("MTTNID = '{0}'", drMaster["MTTNID"])))
                    {
                        // Tiền vốn vay
                        switch (row["MaNV"].ToString().ToUpper())
                        {
                            case "THUHOIVON":
                                row["SoTien"] = drMaster["TienNop"];
                                row["GhiChu"] = "Thu tiền vốn vay";
                                break;
                            case "THUPHI":
                                if ((decimal)drMaster["TienPhi"] > 0)
                                {
                                    row["SoTien"] = drMaster["PsTTH"];
                                    row["GhiChu"] = "Tiền phí trong hạn";
                                }
                                break;
                            case "TPQHTM":
                                if ((decimal)drMaster["TienPhi"] > 0)
                                {
                                    row["SoTien"] = drMaster["PsPhiQH"];
                                    row["GhiChu"] = "Tiền phí quá hạn";
                                }
                                break;
                        }
                    }
                }
            }
        }

        public InfoCustomData Info
        {
            get { return info ; }
        }

        void CapNhatNo()
        {
            // Cập nhật nợ của hộ vay
            if (data.CurMasterIndex < 0)
                return;
            
            string MTID = "", HoVayID = "";
            DataRow drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            using (DataView dvMaster = new DataView(data.DsData.Tables[0]))
            {
                dvMaster.RowStateFilter = DataViewRowState.Deleted;
                if (dvMaster.Count > 0)
                {
                    // Khi xóa phiếu thu
                    //MaNV = dvMaster[0]["MaNV"].ToString();
                    MTID = dvMaster[0]["MTTNID"].ToString();
                    HoVayID = dvMaster[0]["HoVay"].ToString();
                }
                else
                {
                    // Khi thêm - sửa phiếu thu
                    //MaNV = drMaster["MaNV"].ToString();
                    MTID = drMaster["MTTNID"].ToString();
                    HoVayID = drMaster["HoVay"].ToString();
                }

                if (string.IsNullOrEmpty(HoVayID))// Nghiệp vụ là thu nợ vay vốn mới làm
                    return;
                // Cập nhật tiền nợ vay vốn của hộ dân
                data.DbData.EndMultiTrans();//cần phải kết thúc transaction của phiếu thu trước
                
                string sqldk = string.Format(@" DECLARE @DaNop DECIMAL(20,6)
                                                SET @DaNop = ISNULL((SELECT	ISNULL(SUM(TienNop),0)
					                                                FROM	MTThuNo
					                                                WHERE	HoVay = '{0}'),0)
                                                UPDATE	DTHoVay SET
                                                        TVDaNop = @DaNop
                                                        ,TVConLai = DuocVay - @DaNop
                                                WHERE	DTID = '{0}'", HoVayID);
                db.UpdateByNonQuery(sqldk);
            }
        }

        #endregion
    }
}
