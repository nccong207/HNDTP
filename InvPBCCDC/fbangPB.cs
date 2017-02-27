using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;

namespace InvPBCCDC
{
    public partial class fbangPB : DevExpress.XtraEditors.XtraForm
    {
        public fbangPB(DataTable banggia)
        {
            InitializeComponent();
            gridControl1.DataSource = banggia;
            gridView1.BestFitColumns();
            if (Config.GetValue("Language").ToString() == "1")
                FormFactory.DevLocalizer.Translate(this);
        }
    }
}