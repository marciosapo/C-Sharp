using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login
{
    public partial class Setup: Form
    {
        public Setup()
        {
            InitializeComponent();
        }

        private void Setup_Load(object sender, EventArgs e)
        {
            txt_host.Text = DatabaseConnector.getDataBase("host");
            txt_port.Text = DatabaseConnector.getDataBase("host_port");
            txt_user.Text = DatabaseConnector.getDataBase("host_user");
            txt_pass.Text = DatabaseConnector.getDataBase("host_pass");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DatabaseConnector.saveDataBase(txt_host.Text, txt_port.Text, txt_user.Text, txt_pass.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(DatabaseConnector.testConnection(txt_host.Text, txt_port.Text, txt_user.Text, txt_pass.Text), "Teste de Ligação");
        }
    }
}
