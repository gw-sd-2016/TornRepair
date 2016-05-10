using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TornRepair3
{
    public partial class Welcome : Form
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dia=MessageBox.Show("WARNING!!!WARNING!!!You are about to enter my secret chamber. Please leave now or your licence will be deactivated forever!!!"
                ,"WARNING!!!!!!!",MessageBoxButtons.YesNo);
            if (dia == DialogResult.Yes)
            {
                MessageBox.Show("Achievement unlocked: True Lover of Computer Science!! Now I will show you some really interesting things, hope you will enjoy");
                Form1 f1 = new Form1();
                f1.Show();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TornPieceInput tpi = new TornPieceInput();
            tpi.Show();
        }
    }
}
