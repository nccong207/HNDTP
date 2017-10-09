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

        public KiemtraTKDT()
        {
            info = new InfoCustomData(IDataType.MasterDetailDt);
        }
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
            if (data.CurMasterIndex < 0)
                return;

            drMaster = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;

            checkTKDT();
        }

        private void checkTKDT()
        {
            DateTime ngayct = DateTime.Parse(drMaster["NgayCT", DataRowVersion.Current].ToString());
            string pk = data.DrTableMaster["Pk"].ToString();
            DataRow[] drDetail = data.DsData.Tables[1].Select(string.Format("{0} = '{1}'", pk, drMaster[pk]));

            foreach (DataRow row in drDetail)
            {
                if (string.IsNullOrEmpty(row["TkCp"].ToString()))
                {
                    int thang = Convert.ToInt32(row["SoKy"].ToString());
                    if (!string.IsNullOrEmpty(row["TGPB"].ToString()))
                    {
                        DateTime ngayPb = DateTime.Parse(row["TGPB"].ToString());
                        if ((thang > 1) || (ngayPb.Year >= ngayct.Year && ngayPb.Month > ngayct.Month))
                            showMsg();
                    }
                    else
                    {
                        if (thang > 1 ) showMsg();
                    }
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
