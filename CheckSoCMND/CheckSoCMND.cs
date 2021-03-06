using Plugins;
using DevExpress.XtraEditors;

namespace CheckSoCMND
{
    public class CheckSoCMND : ICControl 
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.SingleDt);
        private DataCustomFormControl data;
        TextEdit SoCMND;

        #region ICControl Members

        public void AddEvent()
        {
            //SoCMND = data.FrmMain.Controls.Find("SoCMND",true)[0] as TextEdit;
            //if (SoCMND != null)
            //{
            //    SoCMND.Properties.MaxLength = 9;
            //}
            ////SoCMND.Validating += new System.ComponentModel.CancelEventHandler(SoCMND_Validating);
            //SoCMND.KeyPress += new KeyPressEventHandler(SoCMND_KeyPress);
        }

        //void SoCMND_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
        //    {
        //        e.Handled = true;
        //    }
        //}

        //void SoCMND_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //     TextEdit cmnd = sender as TextEdit;
        //    if (cmnd.Properties.ReadOnly || cmnd.EditValue.ToString() == "")
        //        return;
        //    string str = cmnd.EditValue.ToString();
        //    if (str.Length != 9)
        //    {
        //        e.Cancel = true;
        //        //cmnd.EditValue = "";
        //        cmnd.Focus();
        //        XtraMessageBox.Show("Số CMND không đúng định dạng \n Không được vượt quá 9 hoặc ít hơn 9 ký tự",Config.GetValue("PackageName").ToString(),MessageBoxButtons.OK,MessageBoxIcon.Warning);
        //        return;
        //    }
        //}

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
