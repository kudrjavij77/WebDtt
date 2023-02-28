using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace WebDtt.Reports
{
    public partial class Attedance : DevExpress.XtraReports.UI.XtraReport
    {
        public Attedance()
        {
            InitializeComponent();
        }

        private void xrTableCell5_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {

        }
    }
}
