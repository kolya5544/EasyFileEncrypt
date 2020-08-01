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

namespace SimpleFileEncrypt
{
    public partial class ActionList : Form
    {
        public ActionList()
        {
            InitializeComponent();
        }
        public static byte[] key = Encoding.UTF8.GetBytes("LoremIpsumDolorSitAmet0000000000");
        public static string selection = "";
        public static bool Switch = false;

        private void button1_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                key = GenerateKey();
                byte[] Base64 = Encoding.UTF8.GetBytes(Convert.ToBase64String(key));
                string hexString = ByteArrayToString(Base64);
                string final = Convert.ToBase64String(Encoding.UTF8.GetBytes(hexString));
                var bbb = MessageBox.Show("Secret key was generated. Make sure to save it! You cannot decrypt files without this file.", "ATTENTION!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (bbb == DialogResult.No) return;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Key container. (*.keyc)|*.keyc";
                sfd.ShowDialog();
                File.WriteAllBytes(sfd.FileName, Encoding.UTF8.GetBytes(final));
                MessageBox.Show("Saved!", "Saved!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            var a = MessageBox.Show("Originals of files will be removed! Press YES if you want to continue, and NO to close the program.", "ATTENTION!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (a == DialogResult.No)
            {
                Environment.Exit(0);
            }
            MessageBox.Show("Started encryption... It may take some time. Please, don't shutdown your computer. Press OK.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (Switch)
            {
                List<string> files = Directory.EnumerateFiles(selection, "*", SearchOption.AllDirectories).ToList();
                foreach (string fname in files)
                {
                    if (fname.EndsWith(".keyc")) continue;
                    EncryptFile(fname, key);
                }
            } else
            {
                EncryptFile(selection, key);
            }
            MessageBox.Show("Done!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        public static void EncryptFile(string fname,byte[] key)
        {
            FileInfo fi = new FileInfo(fname);
            long len = fi.Length;
            byte[] lenBytes = BitConverter.GetBytes(len);
            using (FileStream fs = new FileStream(fname, FileMode.Open))
            {
                using (FileStream fw = new FileStream(fname + ".IKEN", FileMode.Create, FileAccess.Write))
                {
                    fw.Write(Form1.SIGNATURE, 0, Form1.SIGNATURE.Length);
                    fw.Write(lenBytes, 0, lenBytes.Length);
                    while (true)
                    {
                        byte[] buffer = new byte[512];
                        int tbf = 0;
                        try
                        {
                            tbf = fs.Read(buffer, 0, 512);
                        }
                        catch
                        {
                            break;
                        }
                        if (tbf != 0)
                        {
                            byte[] newByte = Encrypt(buffer, key);
                            fw.Write(newByte, 0, 512);
                        }
                        else
                        {
                            break;
                        }
                    }
                    fw.Flush();
                }
            }
            File.Delete(fname);
        }
        private static byte[] Encrypt(byte[] toEncrypt, byte[] key)
        {
            AesCryptoServiceProvider aes = CreateProvider(key);
            List<byte> K = new List<byte>();
            K = key.ToList();
            while (K.Count < 32)
            {
                K.Add(0x00);
            }
            key = K.ToArray();
            ICryptoTransform cTransform = aes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
            aes.Clear();
            return resultArray;
        }
        private static byte[] Decrypt(byte[] toDecrypt, byte[] key)
        {
            AesCryptoServiceProvider aes = CreateProvider(key);
            List<byte> K = new List<byte>();
            K = key.ToList();
            while (K.Count < 32)
            {
                K.Add(0x00);
            }
            key = K.ToArray();
            ICryptoTransform cTransform = aes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);
            aes.Clear();
            return resultArray;
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public static byte[] GenerateKey()
        {
            var rng = new RandomGenerator();
            List<byte> keyBytes = new List<byte>();
            for (int i = 0; i<32; i++)
            {
                keyBytes.Add((byte)rng.Next(0, 256));
            }
            return keyBytes.ToArray();
        }
        public static AesCryptoServiceProvider CreateProvider(byte[] key)
        {
            return new AesCryptoServiceProvider
            {
                KeySize = 256,
                BlockSize = 128,
                Key = key,
                Padding = PaddingMode.None,
                Mode = CipherMode.ECB
            };
        }

        private void ActionList_Shown(object sender, EventArgs e)
        {
            key = Encoding.UTF8.GetBytes("LoremIpsumDolorSitAmet0000000000");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                var bbb = MessageBox.Show("Please, select your key container containing the key.", "Key", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (bbb == DialogResult.No) return;
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Key container. (*.keyc)|*.keyc";
                ofd.ShowDialog();
                string b = File.ReadAllText(ofd.FileName);
                byte[] aaa = Convert.FromBase64String(b);
                string hexString = Encoding.UTF8.GetString(aaa);
                string b64 = Encoding.UTF8.GetString(StringToByteArray(hexString));
                key = Convert.FromBase64String(b64);
            }
            var a = MessageBox.Show("Encrypted files will be removed! Press YES if you want to continue, and NO to close the program.", "ATTENTION!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (a == DialogResult.No)
            {
                Environment.Exit(0);
            }
            MessageBox.Show("Started decryption... It may take some time. Please, don't shutdown your computer. Press OK.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (Switch)
            {
                List<string> files = Directory.EnumerateFiles(selection, "*", SearchOption.AllDirectories).ToList();
                foreach (string fname in files)
                {
                    
                    DecryptFile(fname, key);
                }
            }
            else
            {
                DecryptFile(selection, key);
            }
            MessageBox.Show("Done!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        public static void DecryptFile(string file, byte[] key)
        {
            if (!file.EndsWith(".IKEN")) return;
            int Length = 0;
            using (var fr = File.Open(file, FileMode.Open, FileAccess.Read))
            {
                using (var fw = File.Open(file.Substring(0, file.Length - 5), FileMode.Create, FileAccess.Write))
                {
                    byte[] siqBytes = new byte[Form1.SIGNATURE.Length];
                    fr.Read(siqBytes, 0, siqBytes.Length);
                    byte[] lenBytes = new byte[8];
                    fr.Read(lenBytes, 0, 8);
                    Length = (int)BitConverter.ToInt64(lenBytes, 0);
                    while (true)
                    {
                        byte[] buffer = new byte[512];
                        int tbf = 0;
                        try
                        {
                            tbf = fr.Read(buffer, 0, 512);
                        }
                        catch
                        {
                            break;
                        }
                        if (tbf != 0)
                        {
                            byte[] newByte = Decrypt(buffer, key);
                            fw.Write(newByte, 0, 512);
                        }
                        else
                        {
                            break;
                        }
                    }
                    fw.Flush();
                }

            }
            byte[] b = File.ReadAllBytes(file.Substring(0, file.Length - 5));
            File.WriteAllBytes(file.Substring(0, file.Length - 5), b.ToList().GetRange(0, Length).ToArray());
            File.Delete(file);
        }

        public static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
    public class RandomGenerator
    {
        readonly RNGCryptoServiceProvider csp;

        public RandomGenerator()
        {
            csp = new RNGCryptoServiceProvider();
        }

        public int Next(int minValue, int maxExclusiveValue)
        {
            if (minValue >= maxExclusiveValue)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            long diff = (long)maxExclusiveValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        private uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            byte[] buffer = new byte[bytesNumber];
            csp.GetBytes(buffer);
            return buffer;
        }
    }
}
