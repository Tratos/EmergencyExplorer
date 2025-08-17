using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;



namespace EmergencyExplorer
{
    public partial class Form1 : Form
    {
        public string Paths;
        public string ExportPaths;

        private List<string> fullPathsVFF = new List<string>();
        private List<string> fullPathsPAL = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void pfadWählenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog()
            {
                Description = "Game Ordner wählen", ShowNewFolderButton = false
            })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Paths = dlg.SelectedPath;
                    LoadPalFiles(Paths);
                    LoadVFFFiles(Paths);
                }
            }
        }

        private void LoadPalFiles(string rootPath)
        {
            cmbPalettes.BeginUpdate();
            try
            {
                cmbPalettes.Items.Clear();
                fullPathsPAL.Clear();

                var files = Directory.EnumerateFiles(rootPath, "*.pal", SearchOption.AllDirectories)
                                     .OrderBy(f => f, StringComparer.CurrentCultureIgnoreCase);

                foreach (var file in files)
                {
                    fullPathsPAL.Add(file);
                    cmbPalettes.Items.Add(Path.GetFileName(file));
                }

                if (cmbPalettes.Items.Count > 0)
                    cmbPalettes.SelectedIndex = 0; 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Laden der Palette-Dateien: " + ex.Message);
            }
            finally
            {
                cmbPalettes.EndUpdate();
            }
        }

        private string GetSelectedPalettePath()
        {
            int idx = cmbPalettes.SelectedIndex;
            if (idx >= 0 && idx < fullPathsPAL.Count)
                return fullPathsPAL[idx];
            return null;
        }

        private void LoadVFFFiles(string rootPath)
        {
            listBox1.BeginUpdate();
            try
            {
                listBox1.Items.Clear();
                fullPathsVFF.Clear();

                // Alle .vff Dateien rekursiv suchen
                var files = Directory.EnumerateFiles(rootPath, "*.vff", SearchOption.AllDirectories)
                                     .OrderBy(f => f, StringComparer.CurrentCultureIgnoreCase);

                foreach (var file in files)
                {
                    fullPathsVFF.Add(file); // Pfad merken
                    listBox1.Items.Add(Path.GetFileName(file)); // nur Name in ListBox
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Durchsuchen: " + ex.Message, "Fehler",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                listBox1.EndUpdate();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBox1.SelectedIndex;
            if (idx >= 0 && idx < fullPathsVFF.Count)
            {
                string path = fullPathsVFF[idx];
                StatusLabel.Text = path;

                VFFClass.LoadVFF(path, GetSelectedPalettePath(),"",false);
                using (var bmpTemp = new Bitmap(VFFClass.VFFPreview))
                {
                    pictureBox1.Image?.Dispose(); 
                    pictureBox1.Image = new Bitmap(bmpTemp); 
                }


            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ExportPaths))
            {
                using (var dlg = new FolderBrowserDialog()
                {
                    Description = "Export Ordner wählen",
                    ShowNewFolderButton = false
                })
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        ExportPaths = dlg.SelectedPath;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            int idx = listBox1.SelectedIndex;
            if (idx >= 0 && idx < fullPathsVFF.Count)
            {
                string path = fullPathsVFF[idx];
                VFFClass.LoadVFF(path, GetSelectedPalettePath(), ExportPaths, true);
                VFFClass.DumpPalette(GetSelectedPalettePath(), ExportPaths);
            }
        }
    }
}
