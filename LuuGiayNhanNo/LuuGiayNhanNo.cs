using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace LuuGiayNhanNo
{
    public class LuuGiayNhanNo:ICData
    {
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;

        public DataCustomData Data
        {
            set { data = value ; }
        }

        public InfoCustomData Info
        {
            get { return info; }
        }

        public void ExecuteAfter()
        {
        }

        public void ExecuteBefore()
        {
            // B/sung cho TH da set gia tri false
            info.Result = true;

            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            
            if ((bool)drMaster["XacNhan"] && drMaster["NgayGN"] == DBNull.Value)
            {
                XtraMessageBox.Show("Ngày giải ngân không được rỗng", Config.GetValue("PackageName").ToString());
                info.Result = false;
                return;
            }
            // Nguồn vốn thành phố ko cần kiểm tra ràng buộc
            if (drMaster["ThuocNV"] != DBNull.Value && drMaster["ThuocNV"].ToString().ToUpper() != "TP")
            {
                //Fix: Khi nguồn vốn của H/Quận thiếu hiểu thị Y/N cho người dùng chọn
                if (XtraMessageBox.Show("Cho phép giải ngân khi nguồn vốn Huyện/Quận không đủ để giải ngân?"
                    , Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    string sql = string.Format(@"DECLARE @TuNgay DATETIME, @MaBP VARCHAR(16)
                                                SET @TuNgay = '{0}'
                                                SET @MaBP = '{1}'
                                                SELECT	MaBP, NULL BPme, TenBP, CAST(0 AS BIT) IsMoi
		                                                , CASE WHEN MaBP = 'TP' THEN 1 ELSE 2 END Cap
                                                INTO	#BoPhan
                                                FROM	DMBP
                                                WHERE	MaBP = @MaBP OR MaBP = 'TP' OR @MaBP = 'TP'
                                                UNION ALL
                                                SELECT	ID, BoPhan, XaPhuong, IsMoi, 3
                                                FROM	DMXaPhuong
                                                WHERE	BoPhan = @MaBP OR @MaBP = 'TP'
                                                ORDER BY Cap

                                                SELECT	bp.MaBP, bp.TenBP, bp.Cap, bp.BPme, bp.IsMoi, 0 SoDu
                                                INTO	#TempData
                                                FROM	#BoPhan bp
                                                UNION ALL
                                                SELECT	bp.MaBP, bp.TenBP, bp.Cap, bp.BPme, bp.IsMoi, SUM(ob.DuCo) - SUM(ob.DuNo) SoDu
                                                FROM	#BoPhan bp
		                                                INNER JOIN OBTK ob ON bp.MaBP = ob.NguonVon
                                                WHERE	ob.TK like '411%' AND (ob.MaBP = @MaBP OR bp.BPme = @MaBP)
                                                GROUP BY bp.MaBP, bp.TenBP, bp.Cap, bp.BPme, bp.IsMoi
                                                UNION ALL
                                                SELECT	bp.MaBP, bp.TenBP, bp.Cap, bp.BPme, bp.IsMoi, SUM(bl.PsCo) - SUM(bl.PsNo) [SDDK]
                                                FROM	BLTK bl
		                                                INNER JOIN #BoPhan bp ON bp.MaBP = bl.NguonVon
                                                WHERE	bl.TK LIKE '411%' AND NgayCT <= @TuNgay AND (bl.MaBP = @MaBP OR bp.BPme = @MaBP)
                                                GROUP BY bp.MaBP, bp.TenBP, bp.Cap, bp.BPme, bp.IsMoi

                                                SELECT	MaBP, TenBP, Cap, BPme, IsMoi, SUM(SoDu) SoDu
                                                FROM	#TempData
                                                GROUP BY MaBP, TenBP, Cap, BPme, IsMoi
                                                ORDER BY Cap, TenBP

                                                DROP TABLE #TempData
                                                DROP TABLE #BoPhan", drMaster["NgayNN"], drMaster["ThuocNV"]);

                    using (DataTable dtNguonVon = db.GetDataTable(sql))
                    {
                        if (dtNguonVon.Rows.Count <= 0)
                        {
                            XtraMessageBox.Show("Nguồn vốn quỹ HTND không đủ để thực hiện giải ngân", Config.GetValue("PackageName").ToString());
                            info.Result = false;
                            return;
                        }
                        else
                        {
                            // Ktra tính hợp lệ của từng nguồn vốn
                            using (DataTable dtDetail = data.DsData.Tables[1])
                            {
                                // Tổng tiền đã nhập trên giao diện, group theo Nguồn vốn
                                Dictionary<string, decimal> dicSum = new Dictionary<string, decimal>();
                                foreach (DataRow row in dtDetail.Select(string.Format("MTID = '{0}'", drMaster["MTID"])))
                                {
                                    string group = row["NguonVon"].ToString();
                                    decimal rate = (decimal)row["PsVay"];
                                    if (dicSum.ContainsKey(group))
                                        dicSum[group] += rate;
                                    else
                                        dicSum.Add(group, rate);
                                }

                                // Ktra hợp lệ
                                foreach (KeyValuePair<string, decimal> _key in dicSum)
                                {
                                    DataRow[] drs = dtNguonVon.Select(string.Format("MaBP = '{0}'", _key.Key));
                                    if (drs.Length > 0)
                                    {
                                        if (_key.Key == drs[0]["MaBP"].ToString() && _key.Value >= (decimal)drs[0]["SoDu"])
                                        {
                                            XtraMessageBox.Show(string.Format("Nguồn vốn quỹ HTND {0} không đủ để thực hiện giải ngân", drs[0]["TenBP"].ToString())
                                                        , Config.GetValue("PackageName").ToString());
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
