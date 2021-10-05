using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com_ports_communication
{
    public partial class ComForm : Form
    {
        ComPort port;
        public ComForm()
        {
            InitializeComponent();
            port = new ComPort();

            port.SendMessageEvent += SendMessageHandler;
            port.ReceiveMessageEvent += ReceiveMessageHandler;
            port.ErrorEvent += ErrorHandler;

            string[] anwser = port.GetgetAvailablePorts();
            foreach (string a in anwser)
            {
                textBox1.Text += a + ' ';
            }

            bool suc = port.OpenPort(anwser[0], 9600);
        }

        private void SendMessageHandler(string message)
        {
            
        }

        private void ReceiveMessageHandler(string message)
        {
            
        }

        private void ErrorHandler(string message)
        {
            MessageBox.Show(message);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
