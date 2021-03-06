using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using System.Data.SqlClient;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace InvPBCCDC
{
    class CalPB
    {
        string _mact = "Z30";
        private DateTime _ngayCt1;
        private DateTime _ngayCt2;
        public Database _dbData = Database.NewDataDatabase();
        public CalPB(int i, string namlv)//i tháng cần tính
        {
            string str = i.ToString() +"/01/"  +  namlv;
            _ngayCt1 = DateTime.Parse(str);
            _ngayCt2 = _ngayCt1.AddMonths(1).AddDays(-1);
        }
        public DataTable dtCCDC;
        private DataTable getData()
        {
            string sql;
            DataTable dt45=new DataTable("dt45");
            sql = "GetPhanboData";
            string[] paranames = new string[] { "@ngayCt1", "@ngayCt2", "@mabp" };
            object[] paraValues = new object[] { _ngayCt1, _ngayCt2, Config.GetValue("MaCN") };
            
            try
            { 
                dt45 = _dbData.GetDataSetByStore(sql,paranames,paraValues);
            }
            catch
            {
                return null;
            }
            return dt45;

  
        }

        public void calculate() 
        {
            
            dtCCDC = getData();
            dtCCDC.DefaultView.RowFilter = "SoKy >= 1 and TkNo like '31122'";
            _dbData.BeginMultiTrans();
            decimal thtk = 0; //tiền hỏng trước kỳ
            decimal tH = 0;//tiền hỏng kỳ này
            decimal tdpb = 0;//tiền đã phân bổ trước kỳ (không tính tiền hỏng)
            decimal tpb = 0;//tiền phân bổ trong kỳ này
            decimal sokydapb = 0;//số kỳ đã phân bổ trước kỳ này
            decimal tpbky = 0; //tiền phân bổ trong 1 kỳ / 1 số lượng
            deleteBt();
            try
            {
                foreach (DataRowView dr in dtCCDC.DefaultView)
                {
                    decimal sl = decimal.Parse(dr["soluong"].ToString());
                    decimal slHtruoc = decimal.Parse(dr["slHTruoc"].ToString());
                    decimal ps = decimal.Parse(dr["ps"].ToString());
                    decimal slh = decimal.Parse(dr["slH"].ToString());
                    
                    thtk = sl > 0 ? slHtruoc * ps / sl : 0;
                    //DateTime drngayct = DateTime.Parse(dr["ngayct"].ToString());
                    DateTime drngayct = DateTime.Parse(dr["TGPB"].ToString());
                    int sothang =_ngayCt1.Month - drngayct.Month  + 12 * (_ngayCt1.Year - drngayct.Year);
                    if (dr["kypb"].ToString() == "" || dr["kypb"].ToString() == "0")
                    {
                        string msg1 = "Chứng từ xuất CCDC số ", msg2 = " có kỳ phân bổ không hợp lệ!\n" +
                            "Vui lòng kiểm tra lại kỳ phân bổ của chứng từ này và thực hiện phân bổ lại!";
                        if (Config.GetValue("Language").ToString() == "1")
                        {
                            msg1 = UIDictionary.Translate(msg1);
                            msg2 = UIDictionary.Translate(msg2);
                        }
                        XtraMessageBox.Show(msg1 + dr["soct"].ToString() + msg2);
                        continue;
                    }
                    int kypb = int.Parse(dr["kypb"].ToString());
                    sokydapb = (sothang-(sothang%kypb))/kypb;
                    if (dr["soky"].ToString() == "" || dr["soky"].ToString() == "0")
                    {
                        string msg1 = "Chứng từ số ", msg2 = " có số kỳ không hợp lệ!\n" +
                            "Vui lòng kiểm tra lại số kỳ của chứng từ này và thực hiện phân bổ lại!";
                        if (Config.GetValue("Language").ToString() == "1")
                        {
                            msg1 = UIDictionary.Translate(msg1);
                            msg2 = UIDictionary.Translate(msg2);
                        }
                        XtraMessageBox.Show(msg1 + dr["soct"].ToString() + msg2);
                        continue;
                    }
                    if (dr["tkcp"].ToString() == "")
                    {
                        string msg1 = "Chứng từ số ", msg2 = " chưa nhập tk doanh thu!\n" +
                            "Vui lòng kiểm tra lại tk doanh thu của chứng từ này và thực hiện phân bổ lại!";
                        if (Config.GetValue("Language").ToString() == "1")
                        {
                            msg1 = UIDictionary.Translate(msg1);
                            msg2 = UIDictionary.Translate(msg2);
                        }
                        XtraMessageBox.Show(msg1 + dr["soct"].ToString() + msg2);
                        continue;
                    }
                    tpbky = sl > 0 ? ps / (sl * decimal.Parse(dr["soky"].ToString())) : ps / (decimal.Parse(dr["soky"].ToString()));
                    tdpb = sl - slHtruoc > 0 ? sokydapb * tpbky * (sl - slHtruoc) : sokydapb * tpbky;
                    if (sl - slHtruoc != 0)
                        tH = slh * (ps - thtk - tdpb) / (sl - slHtruoc);
                    decimal tsl = sl - slHtruoc - slh;
                    tpb = tsl > 0 ? tpbky * tsl : tpbky;
                    dr["pb"] = Math.Round(tH + tpb, 0, MidpointRounding.AwayFromZero);
                    if (tH + tpb > 0)
                        createBt(dr.Row);
                }
                _dbData.EndMultiTrans();
            }
            catch
            {
                
            }
            
        }
        public bool deleteBt()
        {
            string dkbp = " and MaBP = '" + Config.GetValue("MaCN").ToString() + "'";
            string sql = "delete bltk where nhomdk='PBC' and NgayCt=cast('" + _ngayCt2.ToString() + "' as datetime)";
            _dbData.UpdateByNonQuery(sql + dkbp);
            //theo kieu moi
            sql = "delete bltk where mact='" + _mact + "' and NgayCt=cast('" + _ngayCt2.ToString() + "' as datetime)";
            _dbData.UpdateByNonQuery(sql + dkbp);
            return true;
        }
        private bool createBt(DataRow dr)
        {
            string soct = "'PBDT" + _ngayCt2.Month.ToString("00") + "/" + dr.Table.Rows.IndexOf(dr).ToString() + "'";
            string tableName = "bltk";
            List<string> fieldName=new List<string>();
            List<string> Values = new List<string>();
            fieldName.Add("MTID");
            fieldName.Add("MTIDDT");
            fieldName.Add("Nhomdk");
            fieldName.Add("MaCT");
            fieldName.Add("SoCT");
            fieldName.Add("NgayCT");
            fieldName.Add("makh");
            fieldName.Add("DienGiai");
            fieldName.Add("TK");
            fieldName.Add("TKdu");
            fieldName.Add("Psno");
            fieldName.Add("Psco");
            if ( !(dr["MaPhi"] is DBNull))
            {
                fieldName.Add("Maphi");
            }
            if (!(dr["MaVV"] is DBNull))
            {
                fieldName.Add("MaVV");
            }
            if (!(dr["MaBP"] is DBNull))
            {
                fieldName.Add("MaBP");
            }
            Values.Add("convert( uniqueidentifier,'" + dr["mt45id"].ToString() + "')");
            Values.Add("convert( uniqueidentifier,'" + dr["dt45id"].ToString() + "')");
            Values.Add("'PBC'");
            Values.Add("'" + _mact + "'");
            Values.Add(soct);
            Values.Add("cast('" + _ngayCt2.ToString() + "' as datetime)");
            Values.Add("N'" + dr["makh"].ToString() + "'");
            Values.Add("N'Phân bổ phí thu trước - chứng từ " + dr["soct"].ToString()
                + " ngày " + (DateTime.Parse(dr["NgayCT"].ToString())).ToString("dd/MM/yy") + "'");
            Values.Add("'" + dr["Tkno"].ToString() + "'");
            Values.Add("'" + dr["Tkcp"].ToString() + "'");
            Values.Add(dr["pb"].ToString().Replace(",", "."));
            Values.Add("0");
            if (!(dr["MaPhi"] is DBNull))
            {
                Values.Add("'" + dr["MaPhi"].ToString() + "'");
            }
            if (!(dr["MaVV"] is DBNull))
            {
                Values.Add("'" + dr["MaVV"].ToString() + "'");
            }
            if (!(dr["MaBP"] is DBNull))
            {
                Values.Add("'" + dr["MaBP"].ToString() + "'");
            }
            if (! _dbData.insertRow(tableName, fieldName, Values))
            {
                 return false;     
            }
            Values.RemoveRange(0, Values.Count);
            Values.Add("convert( uniqueidentifier,'" + dr["mt45id"].ToString() + "')");
            Values.Add("convert( uniqueidentifier,'" + dr["dt45id"].ToString() + "')");
            Values.Add("'PBC'");
            Values.Add("'" + _mact + "'");
            Values.Add(soct);
            Values.Add("cast('" + _ngayCt2.ToString() + "' as datetime)");
            Values.Add("N'" + dr["makh"].ToString() + "'");
            Values.Add("N'Phân bổ phí thu trước - chứng từ " + dr["soct"].ToString()
                + " ngày " + (DateTime.Parse(dr["NgayCT"].ToString())).ToString("dd/MM/yy") + "'");
            Values.Add("'" + dr["Tkcp"].ToString() + "'");
            Values.Add("'" + dr["Tkno"].ToString() + "'");
            Values.Add("0");
            Values.Add(dr["pb"].ToString().Replace(",", "."));
            if (!(dr["MaPhi"] is DBNull))
            {
                Values.Add("'" + dr["MaPhi"].ToString() + "'");
            }
            if (!(dr["MaVV"] is DBNull))
            {
                Values.Add("'" + dr["MaVV"].ToString() + "'");
            }
            if (!(dr["MaBP"] is DBNull))
            {
                Values.Add("'" + dr["MaBP"].ToString() + "'");
            }
            if (!_dbData.insertRow(tableName, fieldName, Values))
            {
                return false;
            }
            return true;
        }


    }
}
