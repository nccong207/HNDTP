using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress.XtraEditors;

namespace ThuHoiVon
{
    public class ThuHoiVon :ICControl  
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;
        //GridLookUpEdit gluHoVay;

        #region ICControl Members

        public void AddEvent()
        {
            //gluHoVay = data.FrmMain.Controls.Find("HoVay",true)[0] as GridLookUpEdit;
            //gluHoVay.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluHoVay_CloseUp);
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain,new EventArgs());    
        }

//        void gluHoVay_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
//        {
//            if (drMaster.RowState == DataRowState.Deleted)
//                return;
//            if (e.CloseMode == PopupCloseMode.Normal)
//            {
//                DataTable dt = new DataTable();
//                string sql = string.Format(@" SELECT MTID,TVConLai,TVDaNop,DuocVay,NguoiVay,LaiSuat,m.HoTen 
//                                              FROM  DTHoVay d 
//                                                    INNER JOIN DMHoDan m ON d.NguoiVay = m.ID 
//                                              WHERE DTID = '{0}'", drMaster["HoVay"].ToString());
//                dt = db.GetDataTable(sql);
//                if (dt.Rows.Count > 0)
//                {
//                    drMaster["DuAn"] = dt.Rows[0]["MTID"].ToString();
//                    drMaster["HoVayID"] = dt.Rows[0]["NguoiVay"].ToString();
//                    drMaster["HoTen"] = dt.Rows[0]["HoTen"].ToString();
//                    drMaster["PhiVay"] = decimal.Parse(dt.Rows[0]["LaiSuat"].ToString());
//                    drMaster["STVay"] = decimal.Parse(dt.Rows[0]["DuocVay"].ToString());
//                    drMaster["STDaNop"] = decimal.Parse(dt.Rows[0]["TVDaNop"].ToString());
//                    drMaster["STConLai"] = decimal.Parse(dt.Rows[0]["TVConLai"].ToString());
//                }
//            }
//        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            if (data.BsMain.Current != null)
              drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(ThuHoiVon_ColumnChanged);
        }

        // Tính Phí quá hạn theo tháng
        // Phí quá hạn theo ngày (theo phương pháp tích số dư) - 1 năm: 360 ngày - tháng 30 ngày
        // Tính lại tiền hoàn nếu có
        void ThuHoiVon_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted) return;
            if (drMaster == null) drMaster = e.Row;
            string sql = string.Empty;

            if (e.Column.ColumnName.ToUpper().Equals("HOVAY"))
            {
                // Set thông tin vay vốn của hộ vay
                sql = string.Format(@" SELECT   d.MTID, d.TVConLai, d.TVDaNop, d.DuocVay, d.NguoiVay, hd.HoTen
                                                , da.LaiSuat, da.MucLSQH, da.TLQTK, da.LSTKBB, da.LSTKTN
                                        FROM    DTHoVay AS d 
		                                        INNER JOIN DMHoDan AS hd ON d.NguoiVay = hd.ID 
		                                        INNER JOIN MTDuAn AS da ON d.MTID = da.MTID 
                                        WHERE   d.DTID = '{0}'", e.Row["HoVay"]);
                using (DataTable dt = db.GetDataTable(sql))
                {
                    if (dt.Rows.Count > 0)
                    {
                        e.Row["DuAn"] = dt.Rows[0]["MTID"];
                        e.Row["HoTen"] = dt.Rows[0]["HoTen"];
                        e.Row["PhiVay"] = dt.Rows[0]["LaiSuat"];
                        e.Row["PhiQH"] = dt.Rows[0]["MucLSQH"];
                        e.Row["STVay"] = dt.Rows[0]["DuocVay"];
                        e.Row["STDaNop"] = dt.Rows[0]["TVDaNop"];
                        e.Row["STConLai"] = dt.Rows[0]["TVConLai"];
                        // Đặt ID hộ vay ở cuối để đảm bảo các giá trị trên đã có
                        e.Row["HoVayID"] = dt.Rows[0]["NguoiVay"].ToString();
                    }
                }
                e.Row.EndEdit();
            }

            // Tính tiền phí, tiền hoàn
            if (e.Column.ColumnName.ToUpper().Equals("HOVAYID") || e.Column.ColumnName.ToUpper().Equals("NGAYTHU"))
            {
                if (e.Row["NgayThu"] == DBNull.Value || e.Row["HoVay"] == DBNull.Value) 
                    return;
                // Có 2 Trường hợp: Trả vốn trước hạn và trả vốn quá hạn
                sql = string.Format(@"EXEC sp_SoThang_ThuHoiVon '{0}','{1}'", e.Row["HoVay"], e.Row["NgayThu"]);
                using (DataTable dtp = db.GetDataTable(sql))
                {
                    if (dtp.Rows.Count > 0)
                    {
                        // Trả vốn trong hạn
                        if ((bool)dtp.Rows[0]["QuaHan"] == false)
                        {
                            if ((DateTime)e.Row["NgayThu"] >= (DateTime)dtp.Rows[0]["NgayTPCuoi"])
                            {
                                e.Row["STTH"] = MonthDiff((DateTime)e.Row["NgayThu"], (DateTime)dtp.Rows[0]["NgayTPCuoi"], true);
                                e.Row["STQH"] = 0;
                                e.Row["SoNgayQH"] = 0;
                                // Hoàn phí
                                e.Row["STHP"] = 0;
                            }
                            else
                            {
                                // Hoàn lại phí (nếu có)
                                e.Row["STTH"] = 0;
                                e.Row["STQH"] = 0;
                                e.Row["SoNgayQH"] = 0;
                                // Tính lại số tháng phải hoàn phí
                                e.Row["STHP"] = MonthDiff((DateTime)dtp.Rows[0]["NgayTPCuoi"], (DateTime)e.Row["NgayThu"], false);
                            }
                        }
                        else // Trả vốn quá hạn
                        {
                            e.Row["STTH"] = dtp.Rows[0]["STTrongHan"];
                            e.Row["STQH"] = dtp.Rows[0]["STQuanHan"];
                            e.Row["SoNgayQH"] = dtp.Rows[0]["SNQuanHan"];
                            // Hoàn phí
                            e.Row["STHP"] = 0;
                        }
                    }
                    else
                    {
                        e.Row["STTH"] = 0;
                        e.Row["STQH"] = 0;
                        e.Row["SoNgayQH"] = 0;
                        e.Row["TienPhi"] = 0;
                        // Hoàn phí
                        e.Row["STHP"] = 0;
                    }
                    //e.Row.EndEdit();
                }
            }

            if (e.Column.ColumnName.ToUpper().Equals("TIENNOP"))
            {
                if (e.Row["TienNop"] != DBNull.Value && e.Row["TienPhi"] != DBNull.Value)
                {
                    e.Row["TongTien"] = (decimal)e.Row["TienPhi"] + (decimal)e.Row["TienNop"];
                }
            }
        }

        public int MonthDiff(DateTime d1, DateTime d2, bool abs)
        {
            //Điều kiện: d1 >= d2
            //abs: true - Lấy theo khoảng cách
            //abs: false - Lấy theo tròn tháng
            if (abs == true)
                return (d1.Year - d2.Year) * 12 + d1.Month - d2.Month;
            else
            {
                double dMon = Math.Round(d1.Subtract(d2).Days / (365.25 / 12), 2);
                //Math.Sign(dMon) : Lấy phần nguyên
                double dPS = dMon - Math.Sign(dMon);
                if (dMon >= 0.95)
                    dPS = 1;
                else dPS = 0;
                return Convert.ToInt16(Math.Sign(dMon) + dPS);
            }
        }

        public DataCustomFormControl Data
        {
            set { data = value ; }
        }

        public InfoCustomControl Info
        {
            get { return info ; }
        }

        #endregion
    }
}
