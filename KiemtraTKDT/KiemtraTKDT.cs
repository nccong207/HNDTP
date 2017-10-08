using CDTLib;
using DevExpress.XtraEditors;
using Plugins;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace KiemtraTKDT
{
    public class KiemtraTKDT : ICData
    {

        private InfoCustomData info = new InfoCustomData(IDataType.Single);
        private DataCustomData data;
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
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            if (data.CurMasterIndex < 0)
                return;

            checkTKDT();
        }

        private void checkTKDT()
        {
            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            DateTime ngayct = DateTime.Parse(drMaster["NgayCT", DataRowVersion.Current].ToString());

            string pk = data.DrTableMaster["Pk"].ToString();
            DataRow[] drDetail = data.DsData.Tables[1].Select(string.Format("{0} = '{1}'", pk, drMaster[pk]));

            foreach (DataRow row in drDetail)
            {
                int thang = Convert.ToInt32(drMaster["SoKy"].ToString());
                if (!string.IsNullOrEmpty(drMaster["TGPB", DataRowVersion.Current].ToString()))
                {
                    DateTime ngayPb = DateTime.Parse(drMaster["TGPB", DataRowVersion.Current].ToString());
                    if ((thang <= 1) && (ngayPb.Year <= ngayct.Year && ngayPb.Month <= ngayct.Month))
                        showMsg();
                }
                else
                {
                    if ((thang <= 1))
                        showMsg();
                }
            }
        }

        private void showMsg()
        {
            XtraMessageBox.Show("Bắt buộc nhập tài khoản doanh thu", Config.GetValue("PackageName").ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            info.Result = false;
            return;
        }
    }
}
