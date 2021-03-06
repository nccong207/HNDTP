using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace CapNhatThuVonPhi
{
    public class CapNhatThuVonPhi : ICData
    {
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();

        public DataCustomData Data
        {
            set { data = value; }
        }

        public InfoCustomData Info
        {
            get { return info; }
        }

        private string LaySoCT(DateTime ngayct)
        {
            string Nam = ngayct.Year.ToString();

            Nam = Nam.Substring(2, 2);
            string maCN = "";
            if (Config.GetValue("MaCN") != null)
                maCN = Config.GetValue("MaCN").ToString();
            // 3. Số phiếu thu tự nhảy. Qua mỗi năm số phiếu nhảy lại 001
            // vd: PT001/13
            // Số CT = [Mã CT] + [Số thứ tự] + "/" + [Năm] + [MaCN]
            string prefix = "PXC";
            string suffix = "/" + Nam + maCN;

            string sql = string.Format(@"SELECT TOP 1 CAST(REPLACE(REPLACE(SoCT,'{0}',''),'{1}','') AS INT) [SoCT]
                                        FROM	MT45
                                        WHERE	SoCT LIKE '{0}%{1}' 
                                                AND ISNUMERIC(REPLACE(REPLACE(SoCT,'{0}',''),'{1}','')) = 1
                                        ORDER BY CAST(REPLACE(REPLACE(SoCT,'{0}',''),'{1}','') AS INT) DESC"
                                        , prefix, suffix);
                         
            string soctNew;
            using (DataTable dt = db.GetDataTable(sql))
            {
                if (dt.Rows.Count > 0)
                {
                    int i = (int)dt.Rows[0]["SoCT"] + 1;
                    soctNew = prefix + i.ToString("D3") + suffix;
                }
                else
                {
                    soctNew = prefix + "001" + suffix;
                }
            }
            return soctNew;
        }

        private void LapPhieuThu(DataRow drMaster, DataTable dtPT)
        {
            DateTime ngayct = DateTime.Parse(drMaster["Ngay"].ToString());
            Database db = Database.NewDataDatabase();
            soct = LaySoCT(ngayct);
            //string tkno = "1111";
            string diengiai = "Thu vốn phí";
            string makh = drMaster["MaHQ"].ToString(); //lay ma huyen quan lam ma khach hang
            string tenkh = db.GetValue("select TenBP from DMBoPhan where MaBP = '" + makh + "'").ToString();
            string mt45id = Guid.NewGuid().ToString();
            object tt = dtPT.Compute("sum(Ps)", "");
            //MT45
            string sqlM45 = string.Format(@"INSERT INTO MT45(MT45ID, MaCT, NgayCT, SoCT, MaKH, TenKH, MaNV, DienGiai, Ttien, MTVPID)
                                                      VALUES('{0}','PXC','{1}','{2}','{3}',N'{4}','PTTTM',N'{5}',REPLACE('{6}',',','.'),'{7}');"
                                            , mt45id, ngayct, soct, makh, tenkh, diengiai, tt, drMaster["MTVPID"]);
            //DT45
            foreach (DataRow dr in dtPT.Rows)
            {
                sqlM45 += string.Format(@"INSERT INTO DT45(MT45ID, DT45ID, Ps, TkNo, TkCo, TkCP, SoKy, SoHo, MaBP, MaVV, NguonVon, MTDAID, TenVT, HoVayID, NguoiVay, TGPB)
                                                    VALUES('{0}','{1}',REPLACE('{2}',',','.'),'{3}','{4}',{5},{6},{7},'{8}','{9}','{10}','{11}',N'{12}','{13}',N'{14}', '{15}');"
                    , mt45id, Guid.NewGuid(), dr["Ps"], dr["TkNo"], dr["TkCo"], dr["TkCP"] == DBNull.Value ? "NULL" : "'" + dr["TkCP"] + "'"
                    , dr["SoKy"], dr["SoHo"], dr["MaBP"], dr["MaVV"], dr["NguonVon"], dr["MTDAID"], dr["DienGiai"], dr["HoVayID"], dr["NguoiVay"], ngayct);
            }

            //Insert BLTK
            sqlM45 += string.Format(@"INSERT INTO BLTK(MaCT, MTID, SoCT, NgayCT, DienGiai, MaKH, TenKH, TK, TKDu, PsNo, PsCo, NhomDK, MTIDDT, MaBP, MaVV, NguonVon, MTDAID, DienGiaiMT, SoHo)
                                        SELECT  m.MaCT, m.MT45ID, m.SoCT, m.NgayCT, d.TenVT, m.MaKH, m.TenKH, d.TkNo, d.TkCo, d.Ps, 0, 'PXC1', d.DT45ID, d.MaBP, d.MaVV, d.NguonVon, d.MTDAID, m.DienGiai, d.SoHo
                                        FROM    DT45 AS d 
                                                INNER JOIN MT45 AS m ON d.MT45ID = m.MT45ID
                                        WHERE	m.MT45ID = '{0}';
                                        INSERT INTO BLTK(MaCT, MTID, SoCT, NgayCT, DienGiai, MaKH, TenKH, TK, TKDu, PsNo, PsCo, NhomDK, MTIDDT, MaBP, MaVV, NguonVon, MTDAID, DienGiaiMT, SoHo)
                                        SELECT  m.MaCT, m.MT45ID, m.SoCT, m.NgayCT, d.TenVT, m.MaKH, m.TenKH, d.TkCo, d.TkNo, 0, d.Ps, 'PXC2', d.DT45ID, d.MaBP, d.MaVV, d.NguonVon, d.MTDAID, m.DienGiai, d.SoHo
                                        FROM    DT45 AS d 
                                                INNER JOIN MT45 AS m ON d.MT45ID = m.MT45ID
                                        WHERE	m.MT45ID = '{0}'", mt45id);
            db.BeginMultiTrans();
            blFlag = db.UpdateByNonQuery(sqlM45);
            if (blFlag)
                db.EndMultiTrans();
            else
            {
                info.Result = false;
                XtraMessageBox.Show("Lỗi tạo chứng từ", Config.GetValue("PackageName").ToString());
            }
        }
        
        private void TaoCT(DataRow drMaster)
        {
            //tao cau truc bang tam
            DataTable dtPT = new DataTable();
            dtPT.Columns.Add("MaBP", typeof(String));
            dtPT.Columns.Add("MaVV", typeof(String));
            dtPT.Columns.Add("NguonVon", typeof(String));
            dtPT.Columns.Add("MTDAID", typeof(Guid));
            dtPT.Columns.Add("SoHo", typeof(Int32));

            dtPT.Columns.Add("Ps", typeof(Decimal));
            dtPT.Columns.Add("DienGiai", typeof(String));
            dtPT.Columns.Add("SoKy", typeof(Int32));
            dtPT.Columns.Add("TkNo", typeof(String));
            dtPT.Columns.Add("TkCo", typeof(String));
            dtPT.Columns.Add("TkCP", typeof(String));
            // Bổ sung 2 field: HoVayID và NguoiVay để hỗ trợ mẫu in thu vốn phí
            dtPT.Columns.Add("HoVayID", typeof(Guid));
            dtPT.Columns.Add("NguoiVay", typeof(String));
            // Lấy thông tin MTDuAn
            string NguonVon = "", MaVV = "";
            int THVay = 0;
            string sql = string.Format(@"SELECT Top 1* FROM MTDuAn WHERE MTID = '{0}'", drMaster["MTDAID"]);
            using (DataTable _dt = db.GetDataTable(sql))
            {
                if (_dt.Rows.Count > 0)
                {
                    NguonVon = _dt.Rows[0]["NguonVon"].ToString();
                    MaVV = _dt.Rows[0]["PhuongXa"].ToString();
                    THVay = (int)_dt.Rows[0]["THVay"];
                }
            }
            //lay so lieu (moi dong trong DTVonPhi la mot dong trong DT45)
            //tkno = 1111; soho = 1; mabp,mavv,nguonvon,mtdaid lay tu drMaster
            DataView dv = new DataView(data.DsData.Tables[1]);
            dv.RowFilter = "MTVPID = '" + drMaster["MTVPID"].ToString() + "'";
            foreach (DataRowView drv in dv)
            {
                //doi voi tien phi trong han: tkco = 31122, tkcp = 5111, soky = sothang
                if ((decimal)drv["TienTHan"] > 0)
                {
                    DataRow dr = dtPT.NewRow();
                    dr["MaBP"] = drMaster["MaHQ"];
                    dr["MTDAID"] = drMaster["MTDAID"];
                    dr["NguonVon"] = NguonVon;
                    dr["MaVV"] = MaVV;
                    dr["SoHo"] = 1;
                    dr["HoVayID"] = drv["DTID"];
                    dr["NguoiVay"] = drv["HoTen"];

                    dr["Ps"] = drv["TienTHan"];
                    dr["DienGiai"] = "Tiền phí trong hạn: " + drv["HoTen"].ToString();
                    dr["SoKy"] = drv["SoThang"];
                    dr["TkNo"] = "1111";
                    dr["TkCo"] = "31122";
                    dr["TkCP"] = "5111";
                    dtPT.Rows.Add(dr);
                }
                //doi voi tien phi qua han: tkco = 31123, tkcp = null, soky = 1
                if ((decimal)drv["TienQHan"] > 0)
                {
                    DataRow dr = dtPT.NewRow();
                    dr["MaBP"] = drMaster["MaHQ"];
                    dr["MTDAID"] = drMaster["MTDAID"];
                    dr["NguonVon"] = NguonVon;
                    dr["MaVV"] = MaVV;
                    dr["SoHo"] = 1;
                    dr["HoVayID"] = drv["DTID"];
                    dr["NguoiVay"] = drv["HoTen"];

                    dr["Ps"] = drv["TienQHan"];
                    dr["DienGiai"] = "Tiền phí quá hạn: " + drv["HoTen"].ToString();
                    dr["SoKy"] = drv["SoThang"];
                    dr["TkNo"] = "1111";
                    dr["TkCo"] = "31123";
                    dr["TkCP"] = DBNull.Value;
                    dtPT.Rows.Add(dr);
                }
                                
                //doi voi tien von trong han (DTVonPhi.isQuaHan = 0): 
                //- neu du an ngan han (<= 12 thang): tkco = 1211, tkcp = null, soky = 1
                //- neu du an trung han (<= 24 thang): tkco = 1212, tkcp = null, soky = 1

                //doi voi tien von qua han (DTVonPhi.isQuaHan = 1):
                //- neu du an ngan han (<= 12 thang): tkco = 1271, tkcp = null, soky = 1
                //- neu du an trung han (<= 24 thang): tkco = 1272, tkcp = null, soky = 1
                if ((decimal)drv["TienVon"] > 0)
                {
                    DataRow dr = dtPT.NewRow();
                    dr["MaBP"] = drMaster["MaHQ"];
                    dr["MTDAID"] = drMaster["MTDAID"];
                    dr["NguonVon"] = NguonVon;
                    dr["MaVV"] = MaVV;
                    dr["SoHo"] = 1;
                    dr["HoVayID"] = drv["DTID"];
                    dr["NguoiVay"] = drv["HoTen"];

                    dr["Ps"] = drv["TienVon"];
                    dr["DienGiai"] = "Thu tiền vốn trong hạn: " + drv["HoTen"].ToString();
                    dr["SoKy"] = 1;
                    dr["TkNo"] = "1111";
                    dr["TkCo"] = !(bool)drv["IsQuaHan"] ? (THVay <= 12 ? "1211" : "1212") : (THVay <= 12 ? "1271" : "1272");
                    dr["TkCP"] = DBNull.Value;
                    dtPT.Rows.Add(dr);
                }
            }

            //lap phieu thu
            LapPhieuThu(drMaster, dtPT);
        }

        public void ExecuteAfter()
        {
            if (data.CurMasterIndex < 0)
                return;
            DataRow drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            string MTID = "";
            if (drMaster.RowState == DataRowState.Added || drMaster.RowState == DataRowState.Modified)
                MTID = drMaster["MTDAID"].ToString();
            if(drMaster.RowState == DataRowState.Deleted)
                MTID = drMaster["MTDAID",DataRowVersion.Original].ToString();
            if (string.IsNullOrEmpty(MTID)) return;
            data.DbData.EndMultiTrans();
            // Cập nhật tiền vốn đã nộp sang DTHoVay
            string sql = string.Format(@"UPDATE	DTHoVay SET
		                                        TVDaNop = ISNULL(w.DaNop,0)
		                                        ,TVConLai = DuocVay - ISNULL(w.DaNop,0)
                                        FROM(	SELECT	d.DTID, SUM(d.TienVon) DaNop
		                                        FROM	MTVonPhi m 
				                                        INNER JOIN DTVonPhi d ON m.MTVPID = d.MTVPID
		                                        WHERE	m.MTDAID = '{0}'
		                                        GROUP BY d.DTID
	                                        ) w INNER JOIN DTHoVay v ON v.DTID = w.DTID", MTID);
            db.UpdateByNonQuery(sql);
        }
        string soct = ""; bool blFlag = true;
        public void ExecuteBefore()
        {
            info.Result = true;
            DataRow drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if ((drMaster.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Modified)
                && drMaster["SoCT",DataRowVersion.Original].ToString() != "")
            {
                XtraMessageBox.Show("Cần xóa phiếu thu trước, bằng cách nhấp vào nút '...' ở số phiếu thu",
                    Config.GetValue("PackageName").ToString());
                info.Result = false;
                return;
            }
            
            //Tao phieu thu ke toan (MT45)
            if ((drMaster.RowState == DataRowState.Added || drMaster.RowState == DataRowState.Modified)
                && drMaster["SoCT"].ToString() == "")
            {
                TaoCT(drMaster);
                drMaster["SoCT"] = blFlag ? soct : string.Empty;
            }

        }
    }
}
