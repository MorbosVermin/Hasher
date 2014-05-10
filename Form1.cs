using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hasher
{
    public partial class Form1 : Form
    {
        private class Request
        {
            public bool File { get; set; }
            public string Value { get; set; }
            public CryptoProvider Provider;

            public Request(bool isFile, string value, CryptoProvider provider)
            {
                File = isFile;
                Value = value;
                Provider = provider;
            }
        }

        private class CryptoProvider
        {
            public string Name { get; set; }
            public HashAlgorithm Algorithm { get; set; }

            public CryptoProvider(string name, HashAlgorithm alg)
            {
                Algorithm = alg;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private BindingList<CryptoProvider> providers;

        private bool Wait
        {
            get { return Application.UseWaitCursor; }
            set
            {
                Application.UseWaitCursor = value;
                if (value)
                {
                    Application.DoEvents();
                    button2.Enabled = false;
                    pictureBox1.Image = Properties.Resources.fingerprint_zoom;
                }
                else
                {
                    button2.Enabled = true;
                }
            }
        }

        public Form1()
        {
            InitializeComponent();

            providers = new BindingList<CryptoProvider>();
            providers.Add(new CryptoProvider("MD5", new MD5CryptoServiceProvider()));
            providers.Add(new CryptoProvider("SHA-1", new SHA1CryptoServiceProvider()));
            providers.Add(new CryptoProvider("SHA-256", new SHA256CryptoServiceProvider()));
            providers.Add(new CryptoProvider("SHA-512", new SHA512CryptoServiceProvider()));

            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        }

        void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Wait = false;
            if (e.Result is string)
            {
                pictureBox1.Image = Properties.Resources.fingerprint_checkmark;
                textBox3.Text = (string)e.Result;
            }
            else
            {
                pictureBox1.Image = Properties.Resources.fingerprint_close;
                textBox3.Text = "";
            }
        }

        void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DialogResult r = openFileDialog1.ShowDialog(this);
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            radioButton2.Checked = (!radioButton1.Checked);
            button1.Enabled = radioButton1.Checked;
            textBox2.Enabled = (!radioButton1.Checked);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.Checked = (!radioButton2.Checked);
            textBox2.Enabled = true;
            button1.Enabled = radioButton1.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = providers;
            comboBox1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Wait = true;
            backgroundWorker1.RunWorkerAsync(new Request(radioButton1.Checked, ((radioButton1.Checked) ? textBox1.Text : textBox2.Text), (CryptoProvider)comboBox1.SelectedItem));
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Request request = (Request)e.Argument;
            e.Result = false;
            byte[] data = (request.File) ? File.ReadAllBytes(request.Value) : Encoding.UTF8.GetBytes(request.Value);
            byte[] hash = request.Provider.Algorithm.ComputeHash(data);
            StringBuilder str = new StringBuilder();
            foreach (byte b in hash)
            {
                str.Append(String.Format("{0,2:X2}", b));
            }

            e.Result = str.ToString();
        }
    }
}
