using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using CDTLib;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System.Data;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;

namespace TrichLap
{
    public class TrichLap : ICControl
    {
        private InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl _data;
        private DateTime dt1;
        private DateTime dt2;

        #region ICControl Members

        public void AddEvent()
        {
            if (!_data.DrTable.Table.Columns.Contains("ExtraSql"))
                return;
            string es = _data.DrTable["ExtraSql"].ToString();
            if (es == "")
                return;
            bool tlcq = es.Contains("MaNV = 'TLCQ'");           //xử lý khác nhau đối với quỹ DPRR và các quỹ khác
            LayoutControl lcMain = _data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            SimpleButton btnTrichLap = new SimpleButton();
            btnTrichLap.Text = tlcq ? "Trích lập các quỹ" : "Trích lập quỹ dự phòng";
            btnTrichLap.Name = "btnTrichLap";
            lcMain.Controls.Add(btnTrichLap);
            LayoutControlItem lci = lcMain.AddItem();
            lci.Name = "cusTrichLap";
            lci.Control = btnTrichLap;
            if (tlcq)
                btnTrichLap.Click += new EventHandler(btnTLCQ_Click);
            else
                btnTrichLap.Click += new EventHandler(btnTrichLap_Click);
        }

        void btnTrichLap_Click(object sender, EventArgs e)
        {
            if (_data.BsMain.Current == null)
                return;
            DataRow drCur = (_data.BsMain.Current as DataRowView).Row;
            if (drCur.RowState != DataRowState.Added)
            {
                XtraMessageBox.Show("Vui lòng trích lập khi lập mới chứng từ", Config.GetValue("PackageName").ToString());
                return;
            }
            if (drCur["NgayCT"].ToString() == "")
            {
                XtraMessageBox.Show("Vui lòng nhập ngày chứng từ để trích lập", Config.GetValue("PackageName").ToString());
                return;
            }

            drCur["MaNV"] = "TLQDP";

            LayQuy(drCur);

            Database db = Database.NewDataDatabase();

            string[] pra = new string[] { "@tk", "@ngayct", "@dk", "@soduno", "@soduco" };
            object[] value = new object[] { "121", dt2, "MaBP = '" + Config.GetValue("MaCN") + "'", 0, 0 };
            SqlDbType[] type = new SqlDbType[] {SqlDbType.VarChar, SqlDbType.SmallDateTime, SqlDbType.VarChar, SqlDbType.Decimal, SqlDbType.Decimal};
            ParameterDirection[] dir = new ParameterDirection[] {ParameterDirection.Input, ParameterDirection.Input, ParameterDirection.Input, ParameterDirection.Output, ParameterDirection.Output};

            object[] values = db.GetValueByStore("Sodutaikhoanthuong", pra, value, type, dir);

            object[] value2 = new object[] { "127", dt2, "MaBP = '" + Config.GetValue("MaCN") + "'", 0, 0 };
            object[] values2 = db.GetValueByStore("Sodutaikhoanthuong", pra, value2, type, dir);
            
            if ((values[0] == null || values[0].ToString() == "") && (values2[0] == null || values2[0].ToString() == ""))
            {
                XtraMessageBox.Show("Chưa có số dư của tài khoản cho vay trong quý này", Config.GetValue("PackageName").ToString());
                return;
            }
            if (Decimal.Parse(values[0].ToString()) == 0 && Decimal.Parse(values2[0].ToString()) == 0)
            {
                XtraMessageBox.Show("Số dư tài khoản cho vay đang bằng 0, nên không thể trích lập", Config.GetValue("PackageName").ToString());
                return;
            }
            decimal sotien = Decimal.Parse(values[0].ToString()) + Decimal.Parse(values2[0].ToString());
            XtraMessageBox.Show("Số dư tài khoản cho vay (121 và 127) = " + sotien.ToString("###,###,###,###"));

            decimal qdprr = Math.Round(3 * 75 * sotien / 100000, 0);
            GridView gv = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gv.SelectAll();
            gv.DeleteSelectedRows();
            gv.AddNewRow();
            gv.SetFocusedRowCellValue(gv.Columns["Ps"], qdprr);
            //gv.SetFocusedRowCellValue(gv.Columns["TkNo"], "63297");
            //gv.SetFocusedRowCellValue(gv.Columns["TkCo"], "4314");
            gv.SetFocusedRowCellValue(gv.Columns["DienGiaiCT"], "Trích lập quỹ dự phòng rủi ro");
            gv.UpdateCurrentRow();
        }

        private void LayQuy(DataRow drCur)
        {
            DateTime dt = DateTime.Parse(drCur["NgayCT"].ToString());
            int m = dt.Month;
            int y = dt.Year;
            int q;
            if (m <= 3)
                q = 1;
            else
            {
                if (m <= 6)
                    q = 4;
                else
                {
                    if (m <= 9)
                        q = 7;
                    else
                        q = 10;
                }
            }
            dt1 = new DateTime(y, q, 1);
            dt2 = dt1.AddMonths(3).AddDays(-1);
        }

        void btnTLCQ_Click(object sender, EventArgs e)
        {
            if (_data.BsMain.Current == null)
                return;
            DataRow drCur = (_data.BsMain.Current as DataRowView).Row;
            if (drCur.RowState != DataRowState.Added)
            {
                XtraMessageBox.Show("Vui lòng trích lập khi lập mới chứng từ", Config.GetValue("PackageName").ToString());
                return;
            }
            if (drCur["NgayCT"].ToString() == "")
            {
                XtraMessageBox.Show("Vui lòng nhập ngày chứng từ để trích lập", Config.GetValue("PackageName").ToString());
                return;
            }

            drCur["MaNV"] = "TLCQ";

            string mabp = Config.GetValue("MaCN").ToString();
            LayQuy(drCur);

            Database db = Database.NewDataDatabase();
            object o421 = db.GetValue(string.Format("select sum(psco - psno) from bltk where tk = '421' and MaBP = '{0}' and NgayCT between '{1}' and '{2}'", mabp, dt1, dt2));
            if (o421 == null || o421.ToString() == "")
            {
                XtraMessageBox.Show("Chưa có phát sinh của tài khoản 421 trong quý này", Config.GetValue("PackageName").ToString());
                return;
            }
            if (Decimal.Parse(o421.ToString()) == 0)
            {
                XtraMessageBox.Show("Số dư tài khoản 421 đang bằng 0, nên không thể trích lập", Config.GetValue("PackageName").ToString());
                return;
            }
            decimal sotien = Decimal.Parse(o421.ToString());
            XtraMessageBox.Show("Số dư tài khoản 421 = " + sotien.ToString("###,###,###,###"));
            decimal qdtpt = Math.Round(20 * sotien / 100, 0);
            GridView gv = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gv.SelectAll();
            gv.DeleteSelectedRows();
            gv.AddNewRow();
            gv.SetFocusedRowCellValue(gv.Columns["Ps"], qdtpt);
            gv.SetFocusedRowCellValue(gv.Columns["TkNo"], "421");
            gv.SetFocusedRowCellValue(gv.Columns["TkCo"], "4313");
            gv.SetFocusedRowCellValue(gv.Columns["DienGiaiCT"], "Trích lập quỹ đầu tư phát triển");
            gv.UpdateCurrentRow();

            decimal luong = 0;
            object o63211 = db.GetValue(string.Format("select sum(psno) from bltk where tk = '63211' and MaBP = '{0}' and NgayCT between '{1}' and '{2}'", mabp, dt1, dt2));
            if (o63211 == null || o63211.ToString() == "" || decimal.Parse(o63211.ToString()) == 0)
                XtraMessageBox.Show("Không có phát sinh tài khoản lương 63211, sẽ không trích lập lương", Config.GetValue("PackageName").ToString());
            else
            {
                luong = Math.Round(decimal.Parse(o63211.ToString()) / 4, 0);

                //khen thưởng
                var khenthuong = luong / 3;

                gv.AddNewRow();
                gv.SetFocusedRowCellValue(gv.Columns["Ps"], khenthuong);
                gv.SetFocusedRowCellValue(gv.Columns["TkNo"], "421");
                gv.SetFocusedRowCellValue(gv.Columns["TkCo"], "4311");
                gv.SetFocusedRowCellValue(gv.Columns["DienGiaiCT"], "Trích lập quỹ khen thưởng");
                gv.UpdateCurrentRow();

                //phúc lợi
                var phucloi = 2*luong / 3;

                gv.AddNewRow();
                gv.SetFocusedRowCellValue(gv.Columns["Ps"], phucloi);
                gv.SetFocusedRowCellValue(gv.Columns["TkNo"], "421");
                gv.SetFocusedRowCellValue(gv.Columns["TkCo"], "4311");
                gv.SetFocusedRowCellValue(gv.Columns["DienGiaiCT"], "Trích lập quỹ phúc lợi");
                gv.UpdateCurrentRow();

            }

            decimal von = sotien - qdtpt - luong;
            gv.AddNewRow();
            gv.SetFocusedRowCellValue(gv.Columns["Ps"], von);
            gv.SetFocusedRowCellValue(gv.Columns["TkNo"], "421");
            gv.SetFocusedRowCellValue(gv.Columns["TkCo"], "43122");
            gv.SetFocusedRowCellValue(gv.Columns["DienGiaiCT"], "Trích bổ sung vào vốn hoạt động để cho vay");
            gv.UpdateCurrentRow();

        }

        public DataCustomFormControl Data
        {
            set { _data = value; }
        }

        public InfoCustomControl Info
        {
            get { return _info; }
        }

        #endregion
    }
}
