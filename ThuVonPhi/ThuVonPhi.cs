using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace ThuVonPhi
{
    public class ThuVonPhi:ICControl
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        DataRow drMaster;
        Database db = Database.NewDataDatabase();
        LayoutControl lcMain;
        DateEdit dateNgay;
        GridView gv;
        
        #region ICControl Members
        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        public void AddEvent()
        {
            lcMain = data.FrmMain.Controls.Find("lcMain",true) [0] as LayoutControl;
            dateNgay = data.FrmMain.Controls.Find("Ngay", true)[0] as DateEdit;
            gv = (data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;

            SimpleButton btnXuLy = new SimpleButton();
            btnXuLy.Name = "btnOK";
            btnXuLy.Text = "Xử lý";
            LayoutControlItem lci = lcMain.AddItem("",btnXuLy);
            lci.Name = "lcibtnOK" ;
                        
            btnXuLy.Click += new EventHandler(btnXuLy_Click);
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain,new EventArgs());
        }

       

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null) return;
            if (data.BsMain.Current != null)
                drMaster = (data.BsMain.Current as DataRowView).Row;
            //ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(ThuVonPhi_MasterChanged);
            ds.Tables[1].ColumnChanged += new DataColumnChangeEventHandler(ThuVonPhi_DetailChanged);
        }

        void  ThuVonPhi_MasterChanged(object sender, DataColumnChangeEventArgs e)
        {
            
        }

        
        void ThuVonPhi_DetailChanged(object sender, DataColumnChangeEventArgs e)
        {
            // Báo khi thu phí trong hạn ko đúng với tiền còn lại
            if (e.Column.ColumnName.ToUpper() == "TIENTHAN")
            {
                if ((decimal)e.Row["PsTHan"] < (decimal)e.Row["TienTHan"])
                {
                    XtraMessageBox.Show("Số liệu [Thu phí trong hạn] chưa hợp lệ, vui lòng kiểm tra lại", Config.GetValue("PackageName").ToString());
                }
            }
            // Báo khi thu phí trong hạn ko đúng với tiền còn lại
            if (e.Column.ColumnName.ToUpper() == "TIENQHAN")
            {
                if ((decimal)e.Row["PsQHan"] < (decimal)e.Row["TienQHan"])
                {
                    XtraMessageBox.Show("Số liệu [Thu phí quá hạn] chưa hợp lệ, vui lòng kiểm tra lại", Config.GetValue("PackageName").ToString());
                }
            }
            // Báo khi thu vốn không đúng
            if (e.Column.ColumnName.ToUpper() == "TIENVON")
            {
                if ((decimal)e.Row["PsVayCL"] < (decimal)e.Row["TienVon"])
                {
                    XtraMessageBox.Show("Số liệu [Thu tiền vốn] chưa hợp lệ, vui lòng kiểm tra lại", Config.GetValue("PackageName").ToString());
                }
            }
        }

        void btnXuLy_Click(object sender, EventArgs e)
        {
            //DateTime
            if (dateNgay.Properties.ReadOnly)
                return;

            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster["MTDAID"] == DBNull.Value || drMaster["Ngay"] == DBNull.Value)
            {
                XtraMessageBox.Show("Số liệu chưa hợp lệ, vui lòng kiểm tra lại!", Config.GetValue("PackageName").ToString());
                return;
            }

            // Tạm thời bỏ kiểm soát trường hợp này - 11/03/2014
//            string _sql = string.Format(@"  SELECT  MAX(Ngay) NgayCT
//                                            FROM	MTVonPhi
//                                            WHERE	Ngay >= '{0}' AND MTDAID = '{1}'"
//                        , drMaster["Ngay"], drMaster["MTDAID"]);
//            object obj = db.GetValue(_sql);
//            if (obj != null && obj != DBNull.Value)
//            {
//                XtraMessageBox.Show(string.Format("Số liệu chưa hợp lệ!\nNgày thu Vốn/Phí mới nhất {0}"
//                    , string.Format("{0:dd/MM/yyyy}", obj)), Config.GetValue("PackageName").ToString());
//                return;
//            }
            
            string sql = string.Format("EXEC sp_MTVonPhi '{0}','{1}'", drMaster["Ngay"], drMaster["MTDAID"]);
            using (DataTable dtHoVay = db.GetDataTable(sql))
            {
                SetHoVay(dtHoVay);
            }
        }

        void SetHoVay(DataTable dtHoVay)
        { 
            // Xóa grid
            for (int i = gv.DataRowCount; i >= 0; i--)
            {
                gv.DeleteRow(i);
            }

            // Set dữ liệu vào grid
            foreach (DataRow dr in dtHoVay.Rows)
            {
                gv.AddNewRow();

                gv.SetFocusedRowCellValue(gv.Columns["DTID"], dr["DTID"]);
                gv.SetFocusedRowCellValue(gv.Columns["HoTen"], dr["HoTen"]);
                gv.SetFocusedRowCellValue(gv.Columns["PsVay"], dr["DuocVay"]);
                gv.SetFocusedRowCellValue(gv.Columns["PsVayDN"], dr["TVonDN"]);
                gv.SetFocusedRowCellValue(gv.Columns["PsVayCL"], dr["TVonCL"]); // Tiền vốn còn lại
                gv.SetFocusedRowCellValue(gv.Columns["PsTHan"], dr["TPhiTHCL"]);// Phí TH còn lại
                gv.SetFocusedRowCellValue(gv.Columns["PsQHan"], dr["TPhiQHCL"]);// Phí QH còn lại
                gv.SetFocusedRowCellValue(gv.Columns["LaiSuat"], dr["LaiSuat"]);// Đặt ở vị trí sau các cột ps
                // Bo sung: lấy số tháng theo tham số tùy chọn
                int sttp = Config.GetValue("SoThangTP") != null ? Convert.ToInt32(Config.GetValue("SoThangTP").ToString()) : 3;
                
                // Số tháng: mặc định = 3
                int st = (int)dr["STCL"] > sttp ? sttp : (int)dr["STCL"];
                gv.SetFocusedRowCellValue(gv.Columns["SoThang"], st);
                gv.SetFocusedRowCellValue(gv.Columns["TienQHan"], dr["TPhiQHCL"]);

                if (drMaster["HinhThuc"].ToString() == "Trả nợ theo quý"
                    && (DateTime)drMaster["Ngay"] >= ((DateTime)drMaster["NgayGN"]).AddMonths(3))
                {
                    // Nếu 12 tháng => 1,2,3,4 đợt(3 tháng thu vốn 1 lần)
                    decimal tvon = (decimal)dr["DuocVay"] / (int)dr["SoThang"] * 3;
                    gv.SetFocusedRowCellValue(gv.Columns["TienVon"], tvon >= (decimal)dr["TVonCL"] ? tvon : (decimal)dr["TVonCL"]);
                }
                else
                { 
                    // Khi trả vốn trong kỳ thì phải giảm tiền phí xuống = [Tiền vay còn lại] x [Tỷ lệ]
                    if ((DateTime)drMaster["Ngay"] > (DateTime)drMaster["NgayTH"]
                        || (((DateTime)drMaster["Ngay"]).Month == ((DateTime)drMaster["NgayTH"]).Month
                               && ((DateTime)drMaster["Ngay"]).Year == ((DateTime)drMaster["NgayTH"]).Year))
                        gv.SetFocusedRowCellValue(gv.Columns["TienVon"], dr["TVonCL"]);
                }
                gv.UpdateCurrentRow();
            }
            gv.BestFitColumns();
        }

        #endregion

    }
}
