using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;

namespace KiemTraLoaiKH
{
    public class KiemTraLoaiKH: ICData
    {
        private InfoCustomData _info = new InfoCustomData(IDataType.Single);
        private DataCustomData _data;
        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            
        }

        public void ExecuteBefore()
        {
            //kiem tra
            DataView dv = new DataView(_data.DsData.Tables[0]);
            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
            foreach (DataRowView drv in dv)
            {
                if (drv["MaKH"].ToString() == string.Empty)
                    continue;
                _info.Result = Boolean.Parse(drv["IsKH"].ToString()) || Boolean.Parse(drv["IsNCC"].ToString()) ||
                    Boolean.Parse(drv["IsNV"].ToString());
                if (!_info.Result)
                {
                    XtraMessageBox.Show("Vui lòng chọn loại đối tượng cho " + drv["TenKH"].ToString() + ": là khách hàng/nhà cung cấp/nhân viên!");
                    return;
                }
            }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
        public KiemTraLoaiKH()
        {
            _info = new InfoCustomData(IDataType.Single);
        }

    }
}
