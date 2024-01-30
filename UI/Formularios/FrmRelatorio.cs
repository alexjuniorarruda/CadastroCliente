using System.Data;
using System.Windows.Forms;

namespace UI
{
    public partial class FrmRelatorio : Form
    {
        public FrmRelatorio()
        {
            InitializeComponent();
        }

        public FrmRelatorio(DataSet dataSet) : this()
        {
            rpdClientes1.SetDataSource(dataSet);
            crystalReportViewer1.RefreshReport();
        }
    }
}
