using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleFileEncrypt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static string selection = "";
        public static bool Switch = false; //false is file, true is folder;

        private void button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog newd = new CommonOpenFileDialog();
            newd.IsFolderPicker = true;
            var b = newd.ShowDialog();
            //MessageBox.Show(newd.FileName);
            if (b == CommonFileDialogResult.Ok)
            {
                selection = newd.FileName;
                Switch = true;
                UpdateUILoad();
                ProcessSelection();
            }
        }

        public static byte[] SIGNATURE = { 0x11, 0x55, 0x44, 0x05, 0x11 };
        public static int AmountOfFilesEncrypted = 0;
        private void ProcessSelection()
        {
            if (Switch)
            {
                List<string> files = Directory.EnumerateFiles(selection, "*", SearchOption.AllDirectories).ToList();
                AmountOfFilesEncrypted = 0;
                foreach (string fname in files)
                {
                    using (FileStream fs = new FileStream(fname, FileMode.Open))
                    {
                        byte[] CheckBuffer = new byte[5];
                        fs.Read(CheckBuffer, 0, CheckBuffer.Length);
                        if (BACompare(SIGNATURE, CheckBuffer))
                        {
                            AmountOfFilesEncrypted++;
                        }
                    }
                }
                label2.Text = "Found " + AmountOfFilesEncrypted + " encrypted files! Tested "+files.Count+" files in total.";
                label2.Visible = true;
            }
            button5.Enabled = true;
        }
        public static bool BACompare(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }
            if ((a1 != null) && (a2 != null))
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private void UpdateUILoad()
        {
            button1.Enabled = false;
            button3.Enabled = false;
            label1.Text = "Selected: "+selection;
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            Form dia = new ActionList();
            ActionList.selection = selection;
            ActionList.Switch = Switch;
            dia.ShowDialog();
            button1.Enabled = true;
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog newd = new CommonOpenFileDialog();
            newd.IsFolderPicker = false;
            var b = newd.ShowDialog();
            //MessageBox.Show(newd.FileName);
            if (b == CommonFileDialogResult.Ok)
            {
                selection = newd.FileName;
                Switch = false;
                UpdateUILoad();
                ProcessSelection();
            }
        }
    }
}
