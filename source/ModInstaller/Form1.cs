using Newtonsoft.Json;
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
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net.Sockets;
using System.Threading;
using System.IO.Compression;
using System.Net;
using System.Net.Http;

namespace ModInstaller
{
    
    public partial class Form1 : Form
    {
        public static Config config;

        public static List<Mod> modlist = new List<Mod>();

        public static string CurrentModSelected = "-1";

        public static string MODDB = "https://raw.githubusercontent.com/kolya5544/HSMODS/master/";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("config.json"))
            {
                string json = File.ReadAllText("config.json");
                config = JsonConvert.DeserializeObject<Config>(json);
            } else
            {
                config = new Config();
                config.GameDir = "C:/";
                config.ModInstalled = "null";
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config));
            }
            try
            {
                LoadEnvironment();
            }
            catch
            {
                
            }
            LoadModList();

            //DEBUG TODO
            /*modlist.Add(new Mod()
            {
                Author = "kolya5544",
                Description = "Very\r\ngood\r\nmod!",
                ID = 1,
                Name = "Good Mod Very"
            });
            foreach (Mod mod in modlist)
            {
                listBox1.Items.Add(mod.Name);
            }*/
        }

        private void LoadModList()
        {
            /*TcpClient client = new TcpClient("95.181.157.203", 6760);
            var ns = client.GetStream();
            var sw = new StreamWriter(ns);
            sw.AutoFlush = true;
            sw.Write("list\r\n");
            var bytess = default(byte[]);
            using (var memstream = new MemoryStream())
            {
                var buffer = new byte[2048];
                var bytesRead = default(int);
                while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                    memstream.Write(buffer, 0, bytesRead);
                bytess = memstream.ToArray();
            }*/
            using (var w = new WebClient())
            {
                string json = w.DownloadString(MODDB+"mods.json");
                modlist = JsonConvert.DeserializeObject<List<Mod>>(json);
                listBox1.Items.Clear();
                foreach (Mod mod in modlist)
                {
                    listBox1.Items.Add(mod.Name);
                }
            }//DEBUG
        }

        private void LoadEnvironment()
        {
            textBox1.Text = config.GameDir;
            if (File.Exists(config.GameDir+ "/Heat_Signature.exe"))
            {
                if (config.ModInstalled != "null")
                {
                    button2.Enabled = false;
                    button3.Enabled = true;
                    button4.Enabled = false;
                    button3.Text = "Uninstall " + config.ModInstalled;
                } else
                {
                    button2.Enabled = true;
                    button4.Enabled = true;
                    button3.Enabled = false;
                }
            } else
            {
                button2.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();
            string sel = dialog.FileName;
            if (File.Exists(sel + "/Heat_Signature.exe"))
            {
                config.GameDir = sel;
                if (File.Exists(sel + "/mod"))
                {
                    config.ModInstalled = File.ReadAllText(sel + "/mod");
                }
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config));
                LoadEnvironment();
            } else
            {
                MessageBox.Show("Location you've chosen doesn't contain Heat Signature executable.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string selected = (string)listBox1.SelectedItem;
                Mod mod = null;
                for (int i = 0; i<modlist.Count; i++)
                {
                    if (modlist[i].Name == selected)
                    {
                        mod = modlist[i]; break;
                    }
                }
                if (mod != null)
                {
                    textBox2.Text = mod.Author;
                    textBox3.Text = mod.Name;
                    textBox4.Text = mod.Description;
                    CurrentModSelected = mod.URL;
                }
            }
            catch
            {

            }
        }

        public void InstallMod(byte[] Mod)
        {
            ModBinary modBinary = DispatchModBinary(Mod);
            List<string> toBackup = new List<string>();
            foreach (FileBinary fb in modBinary.files)
            {
                toBackup.Add(fb.Filename);
            }
            Directory.CreateDirectory(config.GameDir + "/BACKUP");
            List<string> backUp = new List<string>();
            foreach (string s in toBackup)
            {
                if (File.Exists(config.GameDir + s))
                {
                    backUp.Add(s);
                    Directory.CreateDirectory(Path.GetDirectoryName(config.GameDir + "/BACKUP" + s));
                    File.Copy(config.GameDir + s, config.GameDir + "/BACKUP" + s);
                }
            }
            ZipFile.CreateFromDirectory(config.GameDir + "/BACKUP", config.GameDir + "/backup.zip");
            foreach (string s in backUp)
            {
                File.Delete(config.GameDir + "/BACKUP" + s);
            }
            //todo ACTUALLY install mod
            foreach (FileBinary fb in modBinary.files)
            {
                byte[] contents = new byte[] { };
                if (File.Exists(config.GameDir + fb.Filename))
                {
                    contents = File.ReadAllBytes(config.GameDir + fb.Filename);
                }
                else
                {
                    int l = 0;
                    foreach (Block b in fb.blocks)
                    {
                        l += b.Length;
                    }
                    contents = new byte[l];
                }
                foreach (Block b in fb.blocks)
                {
                    Overwrite(contents, b.Offset, b.Data);
                }
                File.WriteAllBytes(config.GameDir + fb.Filename, contents);
            }
            File.WriteAllText(config.GameDir + "/mod", "MOD");
            config.ModInstalled = textBox3.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (CurrentModSelected == "-1")
            {
                MessageBox.Show("You haven't chosen a mod you want to install.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                /*new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    TcpClient client = new TcpClient("95.181.157.203", 6760);
                    var ns = client.GetStream();
                    var sw = new StreamWriter(ns);
                    sw.AutoFlush = true;
                    sw.Write("get "+CurrentModSelected+"\r\n");
                    var bytess = default(byte[]);
                    using (var memstream = new MemoryStream())
                    {
                        var buffer = new byte[2048];
                        var bytesRead = default(int);
                        while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                            memstream.Write(buffer, 0, bytesRead);
                        bytess = memstream.ToArray();
                    }
                    Mod = bytess;
                }).Start();*/
                button2.Enabled = false;
                byte[] mod = null;
                using (var h = new HttpClient())
                {
                    var tsk = h.GetAsync(MODDB+"mods/"+CurrentModSelected);
                    tsk.Wait();
                    var response = tsk.Result;
                    var tsk2 = response.Content.ReadAsByteArrayAsync();
                    tsk2.Wait();
                    mod = tsk2.Result;
                }
                MessageBox.Show("Started an installation of a mod. Please, press OK and wait.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                InstallMod(mod);
                MessageBox.Show("Successfully installed modification! Game can be launched now.", "OK!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                button3.Enabled = true;
                button4.Enabled = false;
                button3.Text = "Uninstall " + textBox3.Text;
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config));

            }
        }

        public void Overwrite(byte[] buffer, int offset, byte[] to_overwrite)
        {
            int location = 0;
            for (int i = offset; i < offset + to_overwrite.Length; i++)
            {
                buffer[i] = to_overwrite[location];
                location++;
            }
        }

        private ModBinary DispatchModBinary(byte[] mod)
        {
            List<byte> listByte = mod.ToList();
            int offset = 0;
            ModBinary m = new ModBinary();
            m.files = new List<FileBinary>();

            while (true)
            {
                FileBinary fb = new FileBinary();
                string filename = ReadString(mod, offset);
                offset += filename.Length + 1;
                fb.Filename = filename;
                if (filename.StartsWith("/") && filename.Contains(".."))
                {
                    throw new Exception("Unsecure mod filenames found.");
                }
                fb.Length = BitConverter.ToInt32(mod, offset);
                offset += 4;
                fb.blocks = new List<Block>();
                byte[] Remainder = listByte.GetRange(offset, fb.Length).ToArray();
                List<byte> listRem = Remainder.ToList();
                int BlockOffset = 0;
                while (true)
                {
                    var block = new Block();
                    block.Offset = BitConverter.ToInt32(Remainder,BlockOffset);
                    BlockOffset += 4;
                    block.Length = BitConverter.ToInt16(Remainder, BlockOffset);
                    BlockOffset += 2;
                    block.Data = listRem.GetRange(BlockOffset, 1024).ToArray();
                    BlockOffset += 1024;
                    fb.blocks.Add(block);
                    if (BlockOffset >= Remainder.Length) break;
                }
                offset += BlockOffset;
                m.files.Add(fb);
                if (offset >= mod.Length) break;
            }
            return m;
        }

        private string ReadString(byte[] mod, int offset)
        {
            List<byte> str = new List<byte>();
            while (true)
            {
                if (mod[offset] == 0x00) return Encoding.UTF8.GetString(str.ToArray());
                str.Add(mod[offset]);
                offset++;
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox5.TextLength == 0)
            {
                listBox1.Items.Clear();
                foreach (Mod mod in modlist)
                {
                    listBox1.Items.Add(mod.Name);
                }
            } else
            {
                listBox1.Items.Clear();
                foreach (Mod mod in modlist)
                {
                    if (mod.Name.ToLower().Contains(textBox5.Text.ToLower()))
                    {
                        listBox1.Items.Add(mod.Name);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // ZipFile.ExtractToDirectory(config.GameDir + "/backup.zip", config.GameDir);
            ZipArchive zipArchive = ZipFile.OpenRead(config.GameDir + "/backup.zip");
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                entry.ExtractToFile(config.GameDir + "/"+entry.Name, true);
            }
            zipArchive.Dispose();
            File.Delete(config.GameDir + "/mod");
            File.Delete(config.GameDir+"/backup.zip");
            MessageBox.Show("Successfully uninstalled!", "Good", MessageBoxButtons.OK, MessageBoxIcon.Information);
            button3.Enabled = false;
            button2.Enabled = true;
            button4.Enabled = true;
            button3.Text = "Uninstall";
            config.ModInstalled = "null";
            File.WriteAllText("config.json", JsonConvert.SerializeObject(config));
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "HS ModMaker files|*.hsmod;*.bin";
            var result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                byte[] mod = File.ReadAllBytes(ofd.FileName);
                InstallMod(mod);
                MessageBox.Show("Successfully installed modification! Game can be launched now.", "OK!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                button3.Enabled = true;
                button4.Enabled = false;
                button2.Enabled = false;
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config));
            }
        }
    }

    public class Mod
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
    }

    public class Config
    {
        public string GameDir { get; set; }
        public string ModInstalled { get; set; }
    }
    public class ModBinary
    {
        public List<FileBinary> files { get; set; }
    }
    public class FileBinary
    {
        public string Filename { get; set; }
        public List<Block> blocks { get; set; }
        public int Length { get; set; }
    }
    public class Block
    {
        public int Offset { get; set; }
        public short Length { get; set; }
        public byte[] Data { get; set; }
    }
}
