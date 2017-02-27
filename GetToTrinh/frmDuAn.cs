using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;
using CDTDatabase;

namespace GetToTrinh
{
    public partial class frmDuAn : DevExpress.XtraEditors.XtraForm
    {
        public DataTable dtDuAn = new DataTable();
        Database db = Database.NewDataDatabase();
        string NguonVon, BoPhan;
        int THVay;

        public frmDuAn(string NVon, string MaBP, int ThoiHan)
        {
            InitializeComponent();
            NguonVon = NVon;
            BoPhan = MaBP;
            THVay = ThoiHan;
        }

        private void frmDuAn_Load(object sender, EventArgs e)
        {
            string sql = string.Format(@"SELECT	CAST(0 AS BIT) Chon, MTID, NgayLap, BoPhan, PhuongXa, ChuDA
		                                        , TenDA, THVay, TSoHo, NguonVon, TSoHo, TSoLD, TVonVay
                                        FROM	MTDuAn m 
                                        WHERE	(NguonVon = '{0}' OR NguonVon IN (SELECT DISTINCT ID FROM DMXaPhuong WHERE BoPhan = '{0}'))
                                                AND BoPhan = '{1}' AND NoTT = 1 AND THVay = {2}
		                                        AND MTID NOT IN (SELECT MTID FROM DTQD)
                                        ORDER BY m.BoPhan, m.NgayLap, m.THVay"
                                        , NguonVon, BoPhan, THVay);
            gcTT.DataSource = db.GetDataTable(sql);
            gvTT.BestFitColumns();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            dtDuAn = gcTT.DataSource as DataTable;
            this.DialogResult = DialogResult.OK;
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < gvTT.RowCount; i++)
            {
                gvTT.SetRowCellValue(i, gvTT.Columns["Chon"], chkAll.Checked);
                gvTT.UpdateCurrentRow();// Fix cho TH không nhận được giá trị dòng đầu tiên
            }
        }

    }
}