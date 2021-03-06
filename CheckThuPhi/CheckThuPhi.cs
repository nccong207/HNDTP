using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Windows.Forms;
using System.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid.Views.Grid;

namespace CheckThuPhi
{
    public class CheckThuPhi :ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        DataRow drMaster;
        Database db = Database.NewDataDatabase();
        CalcEdit TienTN;
        LayoutControl lcMain;
        GridLookUpEdit gluHoVay;
        #region ICControl Members

        public void AddEvent()
        {
            lcMain = data.FrmMain.Controls.Find("lcMain",true) [0] as LayoutControl;
            SimpleButton btnCalc = new SimpleButton();
            btnCalc.Name = "btnCalc";
            btnCalc.Text = " Tính tiền phí ";
            LayoutControlItem lci = lcMain.AddItem("",btnCalc);
            lci.Name = "lcibtnCal" ;
            btnCalc.Click += new EventHandler(btnCalc_Click);
            
            TienTN = data.FrmMain.Controls.Find("TienTN",true) [0] as CalcEdit;
            TienTN.EditValueChanged += new EventHandler(TienTN_EditValueChanged);
            gluHoVay = data.FrmMain.Controls.Find("HoVay", true)[0] as GridLookUpEdit;
            gluHoVay.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(gluHoVay_CloseUp);

            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain,new EventArgs());
         
        }

        void gluHoVay_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (drMaster == null) return;
            if (drMaster.RowState == DataRowState.Deleted) return;
            if (e.Value == DBNull.Value) return;
            
            drMaster = (data.BsMain.Current as DataRowView).Row;

            if (drMaster["HoVay"] != DBNull.Value || !string.IsNullOrEmpty(drMaster["HoVay"].ToString()))
            {
                if (e.CloseMode == PopupCloseMode.Normal)
                {
                    //Get value
                    GridLookUpEdit gridHoVay = sender as GridLookUpEdit;
                    DataRow drHoVay = gridHoVay.Properties.View.GetDataRow(gridHoVay.Properties.View.FocusedRowHandle);
                    if (drHoVay == null) return;
                    if (drHoVay["NgayNN"] == DBNull.Value || drHoVay["NgayTN"] == DBNull.Value) return;
                    
                    if (drHoVay["DenNgay"] != DBNull.Value)
                    {
                        //Thu phí lần thứ 2 trở đi
                        //Tính số tháng đã đóng phí
                        int _Month = MonthDiff((DateTime)drHoVay["DenNgay"], (DateTime)drHoVay["NgayNN"], true);
                        drMaster["TuNgay"] = drHoVay["DenNgay"];
                        drMaster["DenNgay"] = ((DateTime)drHoVay["NgayNN"]).AddMonths(_Month + 3);
                        // TH được gian hạn nợ, thời gian sẽ ko đúng theo nguyên tắc 3 tháng 1 lần (tính lại số tháng phải đóng phí)
                        if ((DateTime)drMaster["DenNgay"] > (DateTime)drHoVay["NgayTN"])
                        {
                            int iMonth = MonthDiff((DateTime)drMaster["DenNgay"], (DateTime)drHoVay["NgayTN"], true);
                            drMaster["SoThang"] = (int)drMaster["SoThang"] - iMonth;
                            drMaster["DenNgay"] = ((DateTime)drMaster["DenNgay"]).AddMonths(-1 * iMonth);
                        }
                    }
                    else
                    {
                        // Thu phí lần đầu tiên
                        drMaster["TuNgay"] = (DateTime)drHoVay["NgayNN"];
                        drMaster["DenNgay"] = ((DateTime)drHoVay["NgayNN"]).AddMonths(3);
                    }

                    string sql = string.Format(@"  SELECT   d.MTID,TVConLai,TVDaNop,DuocVay,NguoiVay
                                                        ,d.LaiSuat,m.HoTen,TLQTK,LSTKBB,LSTKTN,DuocVay*TLQTK/100 [TienTKBB]
                                               FROM     DTHoVay d INNER JOIN DMHoDan m ON d.NguoiVay = m.ID 
                                                              INNER JOIN MTDuAn a ON d.MTID = a.MTID  
                                               WHERE DTID = '{0}'", drMaster["HoVay"].ToString());
                    using (DataTable dt = db.GetDataTable(sql))
                    {
                        if (dt.Rows.Count > 0)
                        {
                            drMaster["DuAn"] = dt.Rows[0]["MTID"].ToString();
                            drMaster["TenHoVay"] = dt.Rows[0]["HoTen"].ToString();
                            drMaster["HoTen"] = dt.Rows[0]["NguoiVay"].ToString();
                            drMaster["PhiVay"] = dt.Rows[0]["LaiSuat"];
                            drMaster["SoTienVay"] = dt.Rows[0]["DuocVay"];
                            drMaster["DaThuHoi"] = dt.Rows[0]["TVDaNop"];
                            drMaster["ConLai"] = dt.Rows[0]["TVConLai"];
                            drMaster["PhiVayTK"] = dt.Rows[0]["TLQTK"];
                            // Tiền quỹ tiết kiệm bắt buộc, cố định theo tiền vay
                            if (!string.IsNullOrEmpty(drHoVay["HTVay"].ToString()))
                            {
                                if (drHoVay["HTVay"].ToString().Trim() == "Trả nợ theo quý")
                                    drMaster["TienBB"] = (decimal)dt.Rows[0]["TienTKBB"] * (int)drMaster["SoThang"];
                                else
                                    drMaster["TienBB"] = 0;
                            }
                        }
                        else
                        {
                            drMaster["DuAn"] = DBNull.Value;
                            drMaster["TenHoVay"] = DBNull.Value;
                            drMaster["HoTen"] = DBNull.Value;
                            drMaster["PhiVay"] = 0;
                            drMaster["SoTienVay"] = 0;
                            drMaster["DaThuHoi"] = 0;
                            drMaster["ConLai"] = 0;
                            drMaster["PhiVayTK"] = 0;
                            drMaster["TienBB"] = 0;
                        }
                    }

                    if (drMaster["TongTien"].ToString() != "")
                        drMaster["TongTien"] = DBNull.Value;
                }
            }
        }

        void btnCalc_Click(object sender, EventArgs e)
        {
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (TienTN.Properties.ReadOnly)
            {
                XtraMessageBox.Show("Thao tác không hợp lệ,bạn phải chuyển sang chế độ thêm mới hoặc chỉnh sửa", Config.GetValue("PackageName").ToString());
                return;
            }
            // Tính tiền phí có 2 TH
            // Thu phí trong hạn: 
            // Thu phí quá hạn: 
            if (drMaster["HoVay"].ToString() != "" && drMaster["NgayThu"]!= DBNull.Value)
            {
                string sql = string.Format(@"SELECT NgayTN,isQuaHan,d.HTVay,MucLSQH 
                                                    ,CASE WHEN DATEDIFF(day,NgayTN,'{1}') >0 THEN DATEDIFF(day,NgayTN,'{1}') ELSE 0 END SoNgay
                                             FROM   DTHoVay d
                                                    INNER JOIN MTDuAn a ON d.MTID = a.MTID 
                                             WHERE  DTID = '{0}' ", drMaster["HoVay"], drMaster["NgayThu"]);
                using (DataTable dt = db.GetDataTable(sql))
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (Boolean.Parse(dt.Rows[0]["isQuaHan"].ToString()))
                        {
                            drMaster["PhiQH"] = dt.Rows[0]["MucLSQH"];
                            drMaster["SoNgayQH"] = dt.Rows[0]["songay"];
                        }
                        else
                        {
                            drMaster["PhiQH"] = 100;
                            drMaster["SoNgayQH"] = 0;
                        }
                        if (dt.Rows[0]["HTVay"].ToString().Trim() == "Trả nợ theo quý")// Tính lại tiền quỹ tiết kiệm bắt buộc
                        {
                            drMaster["TienBB"] = (decimal)drMaster["SoTienVay"] * (decimal)drMaster["PhiVayTK"] / 100 * (int)drMaster["SoThang"];
                        }
                        else
                        {
                            drMaster["TienBB"] = 0;
                        }
                    }
                    decimal PhiVay = (decimal)drMaster["ConLai"] * (decimal)drMaster["PhiVay"] / 100;
                    drMaster["TienPhi"] = PhiVay * (decimal)drMaster["PhiQH"] / 100 * (int)drMaster["SoThang"] 
                                        + PhiVay * (decimal)drMaster["PhiQH"] / 100 * (int)drMaster["SoNgayQH"] / 30;
                }
                drMaster.EndEdit();
            }
        }

        void TienTN_EditValueChanged(object sender, EventArgs e)
        {
            CalcEdit calc = sender as CalcEdit;
            if(calc.Properties.ReadOnly)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            if (data.BsMain.Current != null)
                drMaster = (data.BsMain.Current as DataRowView).Row;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(CheckThuPhi_ColumnChanged);
        }

        void CheckThuPhi_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;
            if (e.Column.ColumnName.ToUpper().Equals("NGAYTHU"))
            {
                e.Row["TongTien"] = DBNull.Value;
                e.Row.EndEdit();
            }
            if (e.Column.ColumnName.ToUpper().Equals("HOVAY"))
            {
                e.Row.EndEdit();
            }
        }

        public int MonthDiff(DateTime endDate, DateTime startDate, bool abs)
        {
            //Điều kiện: d1 >= d2
            //abs: true - Lấy theo khoảng cách
            //abs: false - Lấy theo tròn tháng
            if (abs == true)
                return (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
            else
                return Convert.ToInt16(Math.Round(endDate.Subtract(startDate).Days / (365.25 / 12), 0));
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
