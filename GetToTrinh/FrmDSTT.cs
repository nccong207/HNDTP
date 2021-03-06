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
    public partial class FrmDSTT : DevExpress.XtraEditors.XtraForm
    {
        Database db = Database.NewDataDatabase();
        public DataTable dt = new DataTable();
        public string SoTT = "";
        public string MaBP;
        public int THVay;

        public FrmDSTT()
        {
            InitializeComponent();
           
        }

        private void FrmDSTT_Load(object sender, EventArgs e)
        {
            string sql = string.Format(@"SELECT	CAST(0 as BIT) ColChon,* 
                                        FROM	MTTTrinh t
                                        WHERE	isTrinhDuyet = 1 AND t.THVay = {0} AND BoPhan = '{2}'
		                                        AND MTTTID NOT IN ( SELECT MTTTID FROM DTQD WHERE MTTTID IS NOT NULL)
		                                        AND (NguonVon = '{1}' OR NguonVon IN (SELECT ID
											                                        FROM	DMXaPhuong 
											                                        WHERE	BoPhan = '{2}' ))"
                                        , THVay, Config.GetValue("MaCN").ToString(), MaBP);
            gcTT.DataSource = db.GetDataTable(sql);
            //gcTT.DataSource = dt;
            gvTT.BestFitColumns();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string MTTTID = "";
            using (DataTable dt1 = gcTT.DataSource as DataTable)
            {
                using (DataView dv = new DataView(dt1))
                {
                    dv.RowFilter = "ColChon = 1";
                    
                    for (int i = 0; i < dv.Count; i++)
                    {
                        DataRow dr = dv[i].Row;
                        if ((bool)dr["Colchon"])
                        {
                            MTTTID += string.Format("'{0}',", dr["MTTTID"]);
                            SoTT += dr["SoTTrinh"].ToString()+ ";";
                        }
                    }
                    
                    if (dv.Count == 0)
                    {
                        XtraMessageBox.Show("Vui lòng chọn tờ trình", Config.GetValue("PackageName").ToString());
                        return;
                    }
                }
            }
            MTTTID = MTTTID.Length > 0 ? MTTTID.Substring(0, MTTTID.Length - 1) : "";
            if (!string.IsNullOrEmpty(MTTTID))
            {
                string _sql = string.Format("SELECT * FROM DTTTrinh WHERE MTTTID IN ({0})", MTTTID);
                dt = db.GetDataTable(_sql);
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}