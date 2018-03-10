using Bunifu.Framework.UI;
using Iteedee.ApkReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApkTools
{
    public partial class Form1 : Form
    {
        string config = "False:False:True:False:False:True:512";
        string dec_path = "";
        string compile_path = "";
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            this.Opacity = 0;
            timer1.Start();
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
            if (!File.Exists("Tools\\config"))
            {
                File.Create("Tools\\config").Close();
                StreamWriter sw = new StreamWriter("Tools\\config");
                sw.Write(config);
                sw.Close();
            }
            else
            {
                StreamReader s = new StreamReader("Tools\\config");
                config = s.ReadToEnd();
                s.Close();
            }
            string[] conf = this.config.Split(':');
            checkBox2.Checked = bool.Parse(conf[0]);
            checkBox3.Checked = bool.Parse(conf[1]);
            checkBox4.Checked = bool.Parse(conf[2]);
            checkBox1.Checked = bool.Parse(conf[3]);
            checkBox5.Checked = bool.Parse(conf[4]);
            rc.Checked = bool.Parse(conf[5]);
            numericUpDown1.Value = int.Parse(conf[6]);
        }
        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            save();
            this.Opacity = 1;
            for (int i = 0; i < 25; i++)
            {
                this.Opacity -= 0.03;
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            Environment.Exit(0);
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 25; i++)
            {
                this.Opacity -= 0.03;
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            WindowState = FormWindowState.Minimized;
            Opacity = 1;
        }

        private void bunifuCards2_Click(object sender, EventArgs e)
        {
            dec_path = "";
            compile_path = "";
            AppName.Text = "";
            PkgName.Text = "";
            VersionCode.Text = "";
            VersionName.Text = "";
            Image.Visible = false;
            FilePath.Text = "";
            bunifuFlatButton1.Activecolor = Color.FromArgb(27, 28, 29);
            bunifuFlatButton1.BackColor = Color.FromArgb(27, 28, 29);
            bunifuFlatButton1.Normalcolor = Color.FromArgb(27, 28, 29);
            textBox1.AppendText("Opening Select File Dialog" + "\n");
            OpenFileDialog f = new OpenFileDialog();
            f.Title = "Select Apk File";
            f.Filter = "Apk files (*.apk)|*.apk";
            if (f.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(f.FileName))
                {
                    FilePath.Text = f.FileName;
                    compile_path = Path.GetDirectoryName(f.FileName) + "\\" + f.SafeFileName.Replace(".apk", "") + "-signed.apk";
                    dec_path = Path.GetDirectoryName(f.FileName) + "\\" + f.SafeFileName.Replace(".apk", "") + "_src";
                    textBox1.AppendText("Selected File: " + f.FileName + "\n");
                    textBox1.AppendText("Please Wait..." + "\n");
                    ReadApkFromPath(f.FileName);
                    textBox1.AppendText("\nFile Added!" + "\n");
                    textBox1.AppendText("Click Decompile Button..." + "\n");
                    bunifuFlatButton1.BackColor = Color.Firebrick;
                    bunifuFlatButton1.Activecolor = Color.Firebrick;
                    bunifuFlatButton1.Normalcolor = Color.Firebrick;
                    bunifuFlatButton1.OnHovercolor = Color.FromArgb(27, 28, 29);
                    if (AppName.Text != "")
                    {
                        Image.Visible = true;
                    }
                }
            }
        }
        bool manifest = false;
        bool resource = false;
        public void ReadApkFromPath(string path)
        {
            manifest = false;
            resource = false;
            byte[] manifestData = null;
            byte[] resourcesData = null;

            using (ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(path)))
            {
                using (var filestream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
                    ICSharpCode.SharpZipLib.Zip.ZipEntry item;
                    while (true)
                    {
                        if (manifest && resource)
                        {
                            break;
                        }
                        item = zip.GetNextEntry();
                        if (item != null)
                        {
                            try
                            {
                                if (item.Name.ToLower() == "androidmanifest.xml")
                                {
                                    manifestData = new byte[50 * 1024];
                                    using (Stream strm = zipfile.GetInputStream(item))
                                    {
                                        strm.Read(manifestData, 0, manifestData.Length);
                                    }
                                    manifest = true;
                                }
                                if (item.Name.ToLower() == "resources.arsc")
                                {
                                    using (Stream strm = zipfile.GetInputStream(item))
                                    {
                                        using (BinaryReader s = new BinaryReader(strm))
                                        {
                                            resourcesData = s.ReadBytes((int)s.BaseStream.Length);
                                        }
                                    }
                                    resource = true;
                                }
                            }
                            catch
                            {
                               // MessageBox.Show("err");
                            }
                        }
                    }
                }
            }

            ApkInfo info = null;
            try
            {
                ApkReader apkReader = new ApkReader();
                info = apkReader.extractInfo(manifestData, resourcesData);
            }
            catch
            {
                textBox1.AppendText("Read androidmanifest.xml Failed!" + "\n");
                return;
            }

            textBox1.AppendText(string.Format("Package Name: {0}", info.packageName) + "\n");
            PkgName.Text = info.packageName;

            textBox1.AppendText(string.Format("Name: {0}", info.label) + "\n");
            AppName.Text = info.label;
            Directory.CreateDirectory("icons");
            ExtractFileAndSave(path, info.iconFileName[0], @"icons\", info.packageName);
            if (File.Exists(@"icons\" + info.packageName + ".png"))
            {
                Image.ImageLocation = @"icons\" + info.packageName + ".png";
            }
            textBox1.AppendText(string.Format("Version Name: {0}", info.versionName) + "\n");
            textBox1.AppendText(string.Format("Version Code: {0}", info.versionCode) + "\n");
            VersionName.Text = info.versionName;
            VersionCode.Text = info.versionCode;
            textBox1.AppendText(string.Format("App Has Icon: {0}", info.hasIcon) + "\n");
            if (info.iconFileName.Count > 0)
                Console.WriteLine(string.Format("App Icon: {0}", info.iconFileName[0]) + "\n");
            textBox1.AppendText(string.Format("Min SDK Version: {0}", info.minSdkVersion) + "\n");
            textBox1.AppendText(string.Format("Target SDK Version: {0}", info.targetSdkVersion) + "\n");

            if (info.Permissions != null && info.Permissions.Count > 0)
            {
                textBox1.AppendText("Permissions:" + "\n");
                info.Permissions.ForEach(f =>
                {
                    textBox1.AppendText(string.Format("   {0}", f) + "\n");
                });
            }
            else
                textBox1.AppendText("No Permissions Found" + "\n");
            textBox1.AppendText(string.Format("Supports Any Density: {0}", info.supportAnyDensity) + "\n");
            textBox1.AppendText(string.Format("Supports Large Screens: {0}", info.supportLargeScreens) + "\n");
            textBox1.AppendText(string.Format("Supports Normal Screens: {0}", info.supportNormalScreens) + "\n");
            textBox1.AppendText(string.Format("Supports Small Screens: {0}", info.supportSmallScreens) + "\n");
            return;
        }
        public void ExtractFileAndSave(string APKFilePath, string fileResourceLocation, string FilePathToSave, string pkg)
        {
            try
            {
                using (ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(APKFilePath)))
                {
                    using (var filestream = new FileStream(APKFilePath, FileMode.Open, FileAccess.Read))
                    {
                        ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
                        ICSharpCode.SharpZipLib.Zip.ZipEntry item;
                        while ((item = zip.GetNextEntry()) != null)
                        {
                            if (item.Name.ToLower() == fileResourceLocation)
                            {
                                string fileLocation = System.IO.Path.Combine(FilePathToSave, pkg + ".png");
                                using (Stream strm = zipfile.GetInputStream(item))
                                using (FileStream output = File.Create(fileLocation))
                                {
                                    try
                                    {
                                        strm.CopyTo(output);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch
            {
                textBox1.AppendText("Extract Icon Failed!" + "\n");
                return;
            }
        }

        
        private void method_15(string argumants)
        {
            try
            {
                yes = false;
                Process cmd = new Process();
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.RedirectStandardError = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.OutputDataReceived += this.OutputDataReceived;
                cmd.ErrorDataReceived += this.ErrorDataReceived;
                cmd.Start();
                cmd.BeginOutputReadLine();
                cmd.BeginErrorReadLine();
                cmd.StandardInput.WriteLine(argumants);
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
                cmd.WaitForExit();
                Exited();
            }
            catch (Exception e){
                textBox1.AppendText(e.ToString() + "\n");
            }
        }
        bool yes = false;
        private void Exited()
        {
            if (!err)
            {
                if (decompiling)
                {
                    if (yes)
                    {
                        textBox1.AppendText("Decompiled Successfully" + "\n");
                        textBox1.AppendText("Decompiled Directory: " + dec_path + "\n");
                        FolderPath.Text = dec_path;
                        compiling = false;
                        decompiling = false;
                    }
                }
                else if (compiling)
                {
                    if (yes)
                    {
                        textBox1.AppendText("Compiled Successfully" + "\n");
                        textBox1.AppendText("Signing ..." + "\n");
                        string argumants = "java -Xmx" + numericUpDown1.Value + "m -jar \"Tools\\apksigner.jar\" sign --key \"Tools\\sign1.pk8\" --cert \"Tools\\sign2.pem\" --out "+'"'+compile_path+'"' + " " + '"' + compile_path + '"';
                        method_15(argumants);
                        MessageBox.Show("Compiled & Signed Apk Path: " + compile_path + "\n");
                        textBox1.AppendText("Compiled & Signed Apk Path: " + compile_path + "\n");
                        compiling = false;
                        decompiling = false;
                    }
                }
                err = false;
            }
        }
        bool decompiling = false;
        bool compiling = false;
        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Process process = sender as Process;
            if (process != null)
            {
                try
                {
                    if (e.Data.Trim() != "")
                    {
                        textBox1.AppendText(e.Data + "\n");
                        err = true;
                        textBox1.ScrollToCaret();
                        textBox1.AppendText("Note: Protected apps can't decompile whit this program" + "\n");
                    }
                }
                catch { }
            }
            
        }
        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Process process = sender as Process;
            if (process != null)
            {
                textBox1.AppendText(e.Data + "\n");
                try
                {
                    if (e.Data.ToString().Contains("Copying original files") || e.Data.ToString().Contains("Copying unknown"))
                    {
                        yes = true;
                    }
                }
                catch { }
                textBox1.ScrollToCaret();
            }
        }

        private void bunifuCustomLabel1_Click(object sender, EventArgs e)
        {
            shadowPanel7.Visible = false;
            shadowPanel1.Visible = true;
            shadowPanel2.Visible = true;
            shadowPanel3.Visible = false;

            bunifuSeparator1.Location = new Point(13, 3);
            bunifuSeparator2.Location = new Point(129, 2);
            bunifuSeparator3.Location = new Point(19, -3);
            textBox2.Location = new Point(1, 0);
            this.bunifuCustomLabel1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(57)))), ((int)(((byte)(57)))));

            this.bunifuCustomLabel2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(117)))), ((int)(((byte)(117)))));

            this.bunifuCustomLabel3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(117)))), ((int)(((byte)(117)))));

        }

        private void bunifuCustomLabel2_Click(object sender, EventArgs e)
        {
            shadowPanel7.Visible = false;
            shadowPanel1.Visible = false;
            shadowPanel2.Visible = false;
            shadowPanel3.Visible = true;
            bunifuSeparator1.Location = new System.Drawing.Point(129, 3);
            bunifuSeparator2.Location = new System.Drawing.Point(245, 2);
            bunifuSeparator3.Location = new System.Drawing.Point(135, -3);
            textBox2.Location = new System.Drawing.Point(116, 0);
            this.bunifuCustomLabel2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(57)))), ((int)(((byte)(57)))));
            this.bunifuCustomLabel1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(117)))), ((int)(((byte)(117)))));
            this.bunifuCustomLabel3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(117)))), ((int)(((byte)(117)))));
        }

        private void bunifuCustomLabel3_Click(object sender, EventArgs e)
        {
            shadowPanel1.Visible = false;
            shadowPanel2.Visible = false;
            shadowPanel3.Visible = false;
            shadowPanel7.Visible = true;
            bunifuSeparator1.Location = new System.Drawing.Point(244, 3);
            bunifuSeparator2.Location = new System.Drawing.Point(360, 2);
            bunifuSeparator3.Location = new System.Drawing.Point(250, -3);
            textBox2.Location = new System.Drawing.Point(231, 0);
            this.bunifuCustomLabel3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(57)))), ((int)(((byte)(57)))));
            this.bunifuCustomLabel2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(117)))), ((int)(((byte)(117)))));
            this.bunifuCustomLabel1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bunifuCustomLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(117)))), ((int)(((byte)(117)))));
        }

        private void bunifuCustomLabel4_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("Opening Select Folder Dialog" + "\n");
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.Description = "Select Decompiled Apk Folder";
            f.ShowNewFolderButton = false;
            if (f.ShowDialog() == DialogResult.OK)
            {
                textBox1.AppendText("Selected Path: " + f.SelectedPath + "\n");

                string[] filePaths = Directory.GetFiles(f.SelectedPath);
                if (String.Join("", filePaths).Contains("apktool.yml"))
                {
                    FolderPath.Text = f.SelectedPath;
                }
                else {
                    FolderPath.Text = "";
                    textBox1.AppendText("Your selected folder is not a decompiled folder of APK" + "\n");
                }

            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            save();
            this.Opacity = 1;
            for (int i = 0; i < 25; i++)
            {
                this.Opacity -= 0.03;
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
        }
        public void save() {
            string current = checkBox2.Checked + ":" + checkBox3.Checked + ":" + checkBox4.Checked + ":" + checkBox1.Checked + ":" + checkBox5.Checked + ":" + rc.Checked + ":" + numericUpDown1.Value;
            if (config.Trim() != current.Trim())
            {
                if (File.Exists("Tools\\config"))
                {
                    File.Delete("Tools\\config");
                }
                if (!File.Exists("Tools\\config"))
                {
                    File.Create("Tools\\config").Close();
                }
                StreamWriter sw = new StreamWriter("Tools\\config");
                sw.Write(current);
                sw.Close();
            }
        }
        bool err = false;
        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            string debug_info = "-b";
            string classes = "-s";
            string resources = "-r";
            if (!checkBox2.Checked) {
                debug_info = "";
            }
            if (!checkBox3.Checked)
            {
                classes = "";
            }
            if (!checkBox4.Checked)
            {
                resources = "";
            }
            if (FilePath.Text != "")
            {
                yes = false;
                decompiling = true;
                compiling = false;
                err = false;
                if (dec_path == "")
                {
                    dec_path = FilePath.Text.Replace(".apk", "") + "_src";
                }
                string argumant = "java -Xmx" + numericUpDown1.Value + "m -jar \"Tools\\apktool_2.3.1.jar\" d " + debug_info + " " + classes + " " + resources + " " + "-f -o \""+dec_path+"\" " + '"' +FilePath.Text + '"';
                method_15(argumant);
            }
        }
        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {
            if (FolderPath.Text != "")
            {
                string skip_changes = "-f";
                string debuggable = "-d";
                if (!checkBox1.Checked)
                {
                    skip_changes = "";
                }
                if (!checkBox5.Checked)
                {
                    debuggable = "";
                }
                compiling = true;
                decompiling = false;
                err = false;
                if (compile_path == "")
                {
                    compile_path = FolderPath.Text.Replace("_src","") + "-signed.apk";
                }
                yes = false;
                string argumants = ("java -Xmx" + numericUpDown1.Value + "m -jar \"Tools\\apktool_2.3.1.jar\" b" + skip_changes + " " + debuggable + " " + "-o \"" + compile_path + "\"" + " "+'"'+FolderPath.Text+'"').Trim();
                method_15(argumants);
            }
        }
        private void bunifuCustomLabel11_Click(object sender, EventArgs e)
        {
            start("HaCkEr_NiCe");
        }

        private void bunifuCustomLabel9_Click(object sender, EventArgs e)
        {
            start("CyberSoldiersST");
        }
        public void start(string str)
        {
            try
            {
                Process.Start("tg://resolve?domain="+str);
            }
            catch
            {
                Process.Start("http://telegram.me/" + str);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            for (int i = 0; i < 25; i++)
            {
                this.Opacity += 0.03;
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            Opacity = 1;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ScrollToCaret();
        }

        private void bunifuFlatButton3_Click(object sender, EventArgs e)
        {
            start("CyberSoldiersST");
        }
        private void FolderPath_Leave(object sender, EventArgs e)
        {
            try
            {
                string[] filePaths = Directory.GetFiles(FolderPath.Text);
                if (!String.Join("", filePaths).Contains("apktool.yml"))
                {
                    FolderPath.Text = "";
                    textBox1.AppendText("Your selected folder is not a decompiled folder of APK" + "\n");
                }
            }
            catch (DirectoryNotFoundException)
            {
                FolderPath.Text = "";
                textBox1.AppendText("Your selected folder is not found" + "\n");
            }
            catch (Exception eee)
            {

            }
        }
        Drag d = new Drag();
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            d.Grab(this);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            d.MoveObject();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            d.Release();
        }
    }
}