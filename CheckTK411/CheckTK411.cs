using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;

namespace CheckTK411
{
    public class CheckTK411:ICData
    {
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);
        private DataCustomData data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;

        public DataCustomData Data
        {
            set { data = value; }
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
            //tạm thời chưa ràng buộc, nếu sau này ràng buộc thì mở ra
            //if (Config.GetValue("MaCN") != null && Config.GetValue("MaCN").ToString() != "TP")
            //    CheckDuAn();
            //if (info.Result == true)
                CheckNguonVon();
            if (info.Result == true)    //neu dap ung dieu kien nguon von thi moi kiem tra tiep
                CheckSoHo();
        }

        private void CheckDuAn()
        {
            List<string> lstTable = new List<string>(new string[] { "MT11", "MT12", "MT15", "MT16", "MT45", "MT51" });
            if (!lstTable.Contains(data.DrTableMaster["TableName"].ToString()))
                return;
            // Ktra chon du an
            // TKNo: MT11, 15
            // TKCo: MT12, 16
            // TKNo, TKCo: DT51
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            DataView dv = new DataView(data.DsData.Tables[1]);
            if (drMaster.RowState == DataRowState.Added)
                dv.RowStateFilter = DataViewRowState.Added;
            else
            {
                string pk = data.DrTableMaster["Pk"].ToString();
                string pkValue = drMaster[pk].ToString();
                dv.RowFilter = pk + " = '" + pkValue + "'";
            }
            string tb = data.DrTableMaster["TableName"].ToString();
            //TH1: TK gom o master va detail
            if (tb != "MT51" && tb != "MT45")
            {
                string TK = "TKNo", TKDU = "TKCo";
                if (tb == "MT12" || tb == "MT16")
                {
                    TK = "TKCo"; TKDU = "TKNo";
                }

                foreach (DataRowView dr in dv)
                {
                    string tkno = drMaster[TK].ToString();
                    string tkco = dr[TKDU].ToString();
                    if (dr["MTDAID"] == DBNull.Value
                        && (tkno.StartsWith("121") || tkco.StartsWith("121")
                            || tkno.StartsWith("3112") || tkco.StartsWith("3112")
                            || tkno.StartsWith("5111") || tkco.StartsWith("5111")))
                    {
                        XtraMessageBox.Show("Vui lòng chọn dự án", Config.GetValue("PackageName").ToString());
                        info.Result = false;
                        return;
                    }
                }
            }
            else
            {
                //TH2: TK nam o detail detail
                foreach (DataRowView dr in dv)
                {
                    string tkno = dr["TKNo"].ToString();
                    string tkco = dr["TKCo"].ToString();
                    if (dr["MTDAID"] == DBNull.Value
                        && (tkno.StartsWith("121") || tkco.StartsWith("121")
                            || tkno.StartsWith("3112") || tkco.StartsWith("3112")
                            || tkno.StartsWith("5111") || tkco.StartsWith("5111")))
                    {
                        XtraMessageBox.Show("Vui lòng chọn dự án", Config.GetValue("PackageName").ToString());
                        info.Result = false;
                        return;
                    }
                }
            }
            info.Result = true;
        }

        private void CheckNguonVon()
        {
            List<string> lstTable = new List<string>(new string[] { "MT11", "MT12", "MT15", "MT16", "MT45", "MT51" });
            if (!lstTable.Contains(data.DrTableMaster["TableName"].ToString()))
                return;
            // Ktra nguon von vay
            // TKNo: MT11, 15
            // TKCo: MT12, 16
            // TKNo, TKCo: DT51
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            DataView dv = new DataView(data.DsData.Tables[1]);
            if (drMaster.RowState == DataRowState.Added)
                dv.RowStateFilter = DataViewRowState.Added;
            else
            {
                string pk = data.DrTableMaster["Pk"].ToString();
                string pkValue = drMaster[pk].ToString();
                dv.RowFilter = pk + " = '" + pkValue + "'";
            }
            string tb = data.DrTableMaster["TableName"].ToString();
            //TH1: TK gom o master va detail
            if (tb != "MT51" && tb != "MT45")
            {
                string TK = "TKNo", TKDU = "TKCo";
                if (tb == "MT12" || tb == "MT16")
                {
                    TK = "TKCo"; TKDU = "TKNo";
                }

                foreach (DataRowView dr in dv)
                {
                    string tkno = drMaster[TK].ToString();
                    string tkco = dr[TKDU].ToString();
                    if (dr["NguonVon"] == DBNull.Value
                        && (tkno.StartsWith("411") || tkco.StartsWith("411")
                            || tkno.StartsWith("3112") || tkco.StartsWith("3112")
                            || tkno.StartsWith("5111") || tkco.StartsWith("5111")))
                    {
                        XtraMessageBox.Show("Vui lòng chọn nguồn vốn", Config.GetValue("PackageName").ToString());
                        info.Result = false;
                        return;
                    }
                }
            }
            else
            {
                //TH2: TK nam o detail detail
                foreach (DataRowView dr in dv)
                {
                    string tkno = dr["TKNo"].ToString();
                    string tkco = dr["TKCo"].ToString();
                    if (dr["NguonVon"] == DBNull.Value
                        && (tkno.StartsWith("411") || tkco.StartsWith("411")
                            || tkno.StartsWith("3112") || tkco.StartsWith("3112")
                            || tkno.StartsWith("5111") || tkco.StartsWith("5111")))
                    {
                        XtraMessageBox.Show("Vui lòng chọn nguồn vốn", Config.GetValue("PackageName").ToString());
                        info.Result = false;
                        return;
                    }
                }
            }
            info.Result = true;
        }

        private void CheckSoHo()
        {
            List<string> lstTable = new List<string>(new string[] { "MT11", "MT15", "MT45" });
            if (!lstTable.Contains(data.DrTableMaster["TableName"].ToString()))
                return;
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            //SoHo nam o detail 
            DataView dv = new DataView(data.DsData.Tables[1]);
            if (drMaster.RowState == DataRowState.Added)
                dv.RowStateFilter = DataViewRowState.Added;
            else
            {
                string pk = data.DrTableMaster["Pk"].ToString();
                string pkValue = drMaster[pk].ToString();
                dv.RowFilter = pk + " = '" + pkValue + "'";
            }
            foreach (DataRowView dr in dv)
            {
                if (dr["SoHo"] == DBNull.Value
                    && (Regex.IsMatch(dr["TKCo"].ToString(), "3112*") || Regex.IsMatch(dr["TKCo"].ToString(), "5111*")))
                {
                    XtraMessageBox.Show("Vui lòng nhập số hộ", Config.GetValue("PackageName").ToString());
                    info.Result = false;
                    return;
                }
            }
            info.Result = true;
        }
    }
}
