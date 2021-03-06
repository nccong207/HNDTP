using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraLayout;
using DevExpress.XtraGrid.Views.Grid;
using System.Windows.Forms;

namespace GetDAToTrinh
{
    public class GetDAToTrinh : ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();

        DataRow drMaster;
        LayoutControl lcMain;
        GridView grDA;
        SpinEdit soho;
        SpinEdit sold;
        CalcEdit vonvay;
        TextEdit SoTT;
        GridLookUpEdit grNV;

        #region ICControl Members

        public void AddEvent()
        {
             lcMain = data.FrmMain.Controls.Find("lcMain",true) [0] as LayoutControl;
            SimpleButton btnGetDA = new SimpleButton();
            btnGetDA.Name = "btnGetDA";
            btnGetDA.Text = " Lấy dự án ";
            LayoutControlItem lci = lcMain.AddItem("",btnGetDA);
            lci.Name = "btnGet";
            btnGetDA.Click += new EventHandler(btnGetDA_Click);
            grDA = (data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            SoTT = data.FrmMain.Controls.Find("SoTTrinh",true) [0] as TextEdit;
            soho = data.FrmMain.Controls.Find("TSoHo",true)[0] as SpinEdit ;
            sold = data.FrmMain.Controls.Find("TSoLDTH", true)[0] as SpinEdit;
            vonvay = data.FrmMain.Controls.Find("TVonVay", true)[0] as CalcEdit;
            grNV = data.FrmMain.Controls.Find("NguonVon_TenBP",true) [0] as GridLookUpEdit;
            grNV.Popup += new EventHandler(grNV_Popup);
        }

        void grNV_Popup(object sender, EventArgs e)
        {
            GridLookUpEdit gr = sender as GridLookUpEdit;
            GridView gv = gr.Properties.View as GridView;
            gv.ClearColumnsFilter();
            drMaster = (data.BsMain.Current as DataRowView).Row;
            gv.ActiveFilterString = "MaBP = '" + drMaster["BoPhan"].ToString() + "' OR MaBP = 'TP' OR BPme = '" + drMaster["BoPhan"].ToString() + "'";
        }

        void btnGetDA_Click(object sender, EventArgs e)
        {

            if (SoTT.Properties.ReadOnly)
            {
                XtraMessageBox.Show("Vui lòng chuyển sang chế độ chỉnh sửa hoặc thêm mới!",
                  Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK);
                return;
            }
            //Nếu grid detail chưa có số liệu, lọc trong danh mục dự án các dự án đã thẩm định //
             //của quận huyện đó và chưa có tờ trình để add vào grid.
            //- Nếu grid detail đã có số liệu, kiểm tra rowstate của drCurrentMaster:
            //    + Nếu là Added, thì hỏi có muốn xóa dữ liệu trong grid detail để tạo lại ko,
                   //nếu nhấn có thì thực hiện theo yêu cầu
            //    + Nếu ko phải là Added, thì thông báo thao tác không hợp lệ và return
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;

            drMaster = (data.BsMain.Current as DataRowView).Row;
            string sql = string.Format(@"SELECT * FROM MTDuAn n
                               WHERE TinhTrang = 3 AND BoPhan = '{0}'AND NguonVon = '{1}' AND n.THVay = {2} AND MTID NOT IN ( SELECT m.MTID FROM MTDuAn m 
                                                  INNER JOIN DTTTrinh d ON m.MTID = d.MTID
                                                  INNER JOIN MTTTrinh mt ON d.MTTTID = mt.MTTTID WHERE isTrinhDuyet = 1
                                                 )", drMaster["BoPhan"].ToString(), drMaster["NguonVon"].ToString(),int.Parse(drMaster["THVay"].ToString()));
            DataTable dt = new DataTable();
            dt = db.GetDataTable(sql);

            if (grDA.DataRowCount == 0)
            {
                if (dt.Rows.Count == 0)
                    return;

                AddNewData(dt.Rows);
            }
            else
            {
               
                if (drMaster.RowState == DataRowState.Added)
                {
                    DialogResult result = XtraMessageBox.Show("Bạn có muốn xóa dữ liệu danh sách dự án đã có ", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {

                        for (int i = grDA.DataRowCount -1; i >=0; i--)
                        {
                            grDA.DeleteRow(i);
                        }

                        AddNewData(dt.Rows);
                        //foreach (DataRow row in dt.Rows)
                        //{
                        //    grDA.AddNewRow();
                        //    grDA.SetFocusedRowCellValue(grDA.Columns["MTID"], row["MTID"]);
                        //    grDA.UpdateCurrentRow();
                        //}
                    }
                    else
                        return;
                }
                else
                {
                    XtraMessageBox.Show("Thao tác không hợp lệ ", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            
        }

        private void AddNewData(DataRowCollection rows)
        {
            DataTable dtDetail = (data.BsMain.DataSource as DataSet).Tables[1];
            string mtbgid = drMaster["MTTTID"].ToString();
            int i = 0;
            foreach (DataRow row in rows)
            {
                var datarow = dtDetail.NewRow();

                datarow["MTTTID"] = mtbgid;
                datarow["MTID"] = row["MTID"];
                datarow["NgayLap"] = row["NgayLap"];
                datarow["PhuongXa"] = row["PhuongXa"];
                datarow["THVay"] = row["THVay"];
                datarow["THTH"] = row["THTH"];
                datarow["NguonVon"] = row["NguonVon"];
                datarow["TSoLDTH"] = row["TSoLDTH"];
                datarow["TSoHo"] = row["TSoHDN"];
                datarow["TVonCo"] = row["TVonCo"];
                datarow["TVonVay"] = row["TVonDNV"];
                datarow["TVonDA"] = row["TVonDA"];

                if (datarow.RowState == DataRowState.Detached)
                {
                    dtDetail.Rows.Add(datarow);
                }

                grDA.SetRowCellValue(i, "MTID", row["MTID"]);
                i++;
            }

            grDA.RefreshData();
        }

        public DataCustomFormControl Data
        {
            set {data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        #endregion
    }
}
