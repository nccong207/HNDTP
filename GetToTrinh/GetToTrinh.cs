using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTDatabase;
using CDTLib;
using Plugins;
using DevExpress.XtraLayout;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace GetToTrinh
{
    public class GetToTrinh :ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        LayoutControl lc;
        DataRow drMaster;
        DataView dvDetail;
        TextEdit soQD;
        MemoEdit SoTT;
        public DataTable dt = new DataTable();
        Database db = Database.NewDataDatabase();
        GridView grDetail;
        public string MaBP;
        public int THVay;

        #region ICControl Members

        public void AddEvent()
        {
            lc = data.FrmMain.Controls.Find("lcMain",true) [0] as LayoutControl;
            SimpleButton btn = new SimpleButton();
            btn.Name = "btnGetTT";
            btn.Text = "Chọn tờ trình";
            LayoutControlItem lci = lc.AddItem("",btn);
            lci.Name = "lcibtn";
            btn.Click += new EventHandler(btn_Click);

            // B/sung
            SimpleButton btnDA = new SimpleButton();
            btnDA.Name = "btnGetDA";
            btnDA.Text = "Chọn dự án";
            LayoutControlItem lciDA = lc.AddItem("", btnDA);
            lciDA.Name = "lcibtnDA";
            btnDA.Click += new EventHandler(btnDA_Click);

            soQD = data.FrmMain.Controls.Find("SoQD",true) [0] as TextEdit;
            SoTT = data.FrmMain.Controls.Find("TheoTT",true) [0] as MemoEdit;
            grDetail =( data.FrmMain.Controls.Find("gcMain",true)[0] as GridControl).MainView as GridView;
        }

        void btnDA_Click(object sender, EventArgs e)
        {
            DataSet ds = (data.BsMain.DataSource) as DataSet;
            if (ds == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            // Dự án đã giải ngân
            if ((bool)drMaster["IsGiaiNgan"] == true)
            {
                XtraMessageBox.Show("Bạn không được sửa/xóa dữ liệu khi dự án đã giải ngân", Config.GetValue("PackageName").ToString());
                return;
            }

            if (drMaster["CQNQD"].ToString() == "")
            {
                XtraMessageBox.Show("Bạn phải chọn CQ nhận quyết định", Config.GetValue("PackageName").ToString());
                return;
            }
            MaBP = drMaster["CQNQD"].ToString();
            THVay = (int)drMaster["THVay"];
            if (soQD.Properties.ReadOnly)
            {
                XtraMessageBox.Show("Thao tác không hợp lệ, vui lòng chuyển qua chế độ chỉnh sửa hoặc thêm mới");
                return;
            }

            frmDuAn da = new frmDuAn(drMaster["BoPhan"].ToString(), drMaster["CQNQD"].ToString(), THVay);
            if (da.ShowDialog() == DialogResult.OK)
            {
                using (DataView dv = new DataView(da.dtDuAn))
                {
                    dv.RowFilter = "Chon = 1";
                    if (dv.Count == 0)
                        return;
                    // Xóa dự án đã add trước đó
                    for (int i = grDetail.RowCount; i >= 0; i--)
                    {
                        grDetail.DeleteRow(i);
                    }

                    // Add dự án vào quyết định
                    foreach (DataRowView drv in dv)
                    {
                        grDetail.AddNewRow();
                        grDetail.SetFocusedRowCellValue(grDetail.Columns["MTID"], drv.Row["MTID"]);
                        grDetail.UpdateCurrentRow();
                    }
                }
            }
        }

        void btn_Click(object sender, EventArgs e)
        {
            DataSet ds = (data.BsMain.DataSource) as DataSet;
            if (ds == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            // Dự án đã giải ngân
            if ((bool)drMaster["IsGiaiNgan"] == true)
            {
                XtraMessageBox.Show("Bạn không được sửa/xóa dữ liệu khi dự án đã giải ngân", Config.GetValue("PackageName").ToString());
                return;
            }

            if (drMaster["CQNQD"].ToString() == "")
            {
                XtraMessageBox.Show("Bạn phải chọn CQ nhận quyết định",Config.GetValue("PackageName").ToString());
                return;
            }
            MaBP = drMaster["CQNQD"].ToString();
            THVay = (int)drMaster["THVay"];
            if (soQD.Properties.ReadOnly)
            {
                XtraMessageBox.Show("Thao tác không hợp lệ, vui lòng chuyển qua chế độ chỉnh sửa hoặc thêm mới");
                return;
            }

            // Chọn tờ trình
            FrmDSTT frm = new FrmDSTT();
            frm.MaBP = MaBP;
            frm.THVay = THVay;
            frm.ShowDialog();
            if (frm.SoTT.ToString() == "")
            {
                XtraMessageBox.Show("Bạn đã hủy bỏ thao tác chọn tờ trình", Config.GetValue("PackageName").ToString());
                return;
            }
            DataTable dt = frm.dt;
            if (dt.Rows.Count == 0) return;

            // Bị đuối ....
            if (SoTT.EditValue.ToString() != "")
            {
                if (XtraMessageBox.Show("Bạn có muốn xóa dữ liệu để tạo dữ liệu mới", Config.GetValue("PackageName").ToString()) == DialogResult.Yes)
                {
                    for (int i = grDetail.DataRowCount - 1; i >= 0; i--)
                    {
                        grDetail.DeleteRow(i);
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        grDetail.AddNewRow();
                        grDetail.SetFocusedRowCellValue(grDetail.Columns["MTID"], row["MTID"]);
                        grDetail.SetFocusedRowCellValue(grDetail.Columns["MTTTID"], row["MTTTID"]);// B/s ID to trinh vao DTQD 30/08/13
                        grDetail.UpdateCurrentRow();
                        string s = grDetail.GetFocusedRowCellValue(grDetail.Columns["SoTT"]).ToString();
                    }
                }
                else
                    return;
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    grDetail.AddNewRow();
                    grDetail.SetFocusedRowCellValue(grDetail.Columns["MTID"], row["MTID"]);
                    grDetail.SetFocusedRowCellValue(grDetail.Columns["MTTTID"], row["MTTTID"]);// B/s ID to trinh vao DTQD 30/08/13
                    grDetail.UpdateCurrentRow();
                    //string s = grDetail.GetFocusedRowCellValue(grDetail.Columns["SoTT"]).ToString();
                }
            }
           
            SoTT.EditValue = frm.SoTT;
        }

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        #endregion
    }
}
