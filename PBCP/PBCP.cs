using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System.Data;

namespace PBCP
{
    public class PBCP : ICControl
    {
        private DataCustomFormControl _data;
        private InfoCustomControl _info = new InfoCustomControl(IDataType.MasterDetailDt);
        #region ICControl Members

        public void AddEvent()
        {
            GridView gvDetail = (_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            if (!_data.DrTable.Table.Columns.Contains("ExtraSql")
                || _data.DrTable["ExtraSql"].ToString().Contains("PBCP = 1"))
            {
                gvDetail.Columns["MaKho"].Visible = false;
                gvDetail.Columns["MaVT"].Visible = false;
                gvDetail.Columns["TenVT"].Caption = "Diễn giải";
                gvDetail.Columns["MaDVT"].Visible = false;
                gvDetail.Columns["SoLuong"].Visible = false;
                gvDetail.Columns["SLQuyDoi"].Visible = false;
                gvDetail.Columns["GiaNT"].Visible = false;
                gvDetail.Columns["Gia"].Visible = false;
                gvDetail.Columns["PsNT"].Caption = "Phát sinh nguyên tệ";
                gvDetail.Columns["Ps"].Caption = "Phát sinh";
            }
            else
            {
                gvDetail.Columns["MaKho"].Visible = true;
                gvDetail.Columns["MaVT"].Visible = true;
                gvDetail.Columns["MaDVT"].Visible = true;
                gvDetail.Columns["SoLuong"].Visible = true;
                gvDetail.Columns["SLQuyDoi"].Visible = true;
                gvDetail.Columns["GiaNT"].Visible = true;
                gvDetail.Columns["Gia"].Visible = true;
                gvDetail.Columns["PsNT"].Visible = true;
                gvDetail.Columns["Ps"].Visible = true;
            }

            if (_data.BsMain.DataSource != null)
            {
                DataSet dsData = _data.BsMain.DataSource as DataSet;
                if (dsData == null)
                    return;
                dsData.Tables[0].TableNewRow += new DataTableNewRowEventHandler(PBCP_TableNewRow);
            }
            _data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            if (_data.BsMain.Current != null)
            {
                DataRow dr = (_data.BsMain.Current as DataRowView).Row;
                DataTableNewRowEventArgs e = new DataTableNewRowEventArgs(dr);
                PBCP_TableNewRow((_data.BsMain.DataSource as DataSet).Tables[0], e);
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet dsData = _data.BsMain.DataSource as DataSet;
            if (dsData == null)
                return;
            dsData.Tables[0].TableNewRow += new DataTableNewRowEventHandler(PBCP_TableNewRow);
        }

        void PBCP_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            if (e.Row == null)
                return;
            //if (_data.DrTable.Table.Columns.Contains("ExtraSql")
            //    && _data.DrTable["ExtraSql"].ToString().Contains("PBCP = 1"))
                e.Row["PBCP"] = 1;
            //else
            //    e.Row["PBCP"] = 0;
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
