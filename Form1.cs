using DifficultyOptimizer.src;
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

namespace DifficultyOptimizer
{
    public partial class DifficultyOptimizerForm : Form
    {
        private string RootFolder { get; set; }

        private string CurrentDirectory { get; set; }

        private Optimizer Optimizer;

        private OpenFileDialog FileBrowser = new OpenFileDialog();

        private FolderBrowserDialog FolderBrowser = new FolderBrowserDialog();

        private string RemoveRoot(string file) => file.Replace(RootFolder, ".");

        private string AddRoot(string file) => RootFolder + file.Substring(1);

        private void DeleteRow(int row) => DataGrid.Rows.RemoveAt(row);

        private bool ValidateRow(int row) => File.Exists(AddRoot(GetMapFilePath(row)));

        public DifficultyOptimizerForm()
        {
            Optimizer = new Optimizer();
            CurrentDirectory = Directory.GetCurrentDirectory();
            SetRootFolder(CurrentDirectory);
            FileBrowser.Multiselect = true;
            FileBrowser.Filter = "Map File|*.osu;*.qua";
            InitializeComponent();
            PrintToOutput($"Please set your maps directory. \nCurrent directory: {CurrentDirectory}");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ButtonOptimize_Click(object sender, EventArgs e)
        {

        }

        private void ButtonSetDirectory_Click(object sender, EventArgs e)
        {
            if (FolderBrowser.ShowDialog() != DialogResult.OK)
            {
                ErrorToOutput("Error setting the root directory for maps.");
                return;
            }

            PrintToOutput($"Relative Root directory for maps has been set to: {FolderBrowser.SelectedPath}");
            SetRootFolder(FolderBrowser.SelectedPath);
        }

        private void SetRootFolder(string root)
        {
            RootFolder = FolderBrowser.SelectedPath;
            FileBrowser.InitialDirectory = RootFolder;
        }

        private void ClearOutput() => TextBoxOutput.Clear();

        private void PrintToOutput(string output)
        {
            TextBoxOutput.AppendText($"{output}\n");
            TextBoxOutput.ScrollToCaret();
        }

        private void ErrorToOutput(string output)
        {
            TextBoxOutput.SelectionColor = Color.Red;
            TextBoxOutput.AppendText($"{output}\n");
            TextBoxOutput.ScrollToCaret();
        }

        private void ButtonImportMap_Click(object sender, EventArgs e)
        {
            if (FileBrowser.ShowDialog() != DialogResult.OK || !FileBrowser.CheckFileExists)
            {
                ErrorToOutput("Error importing selected map.");
                return;
            }

            foreach (var file in FileBrowser.FileNames)
            {
                ImportMapData(file);
            }
        }

        private void ImportMapData(string file, float diff = 1f, float weight = 0.5f)
        {
            try
            {
                Optimizer.AddMapData(file, diff, weight);

                var row = (DataGridViewRow)DataGrid.Rows[0].Clone();
                row.Cells[0].Value = true;
                row.Cells[1].Value = RemoveRoot(file);
                row.Cells[2].Value = diff;
                row.Cells[3].Value = weight;
                DataGrid.Rows.Add(row);
                PrintToOutput($"Map Imported: {file}");
            }
            catch
            {
                ErrorToOutput($"Error importing selected map. Make sure you have the proper root directory set. Map: {file}");
            }
        }

        private bool ValidateData()
        {
            for (var i = 0; i < DataGrid.Rows.Count - 1; i++)
            {
                var valid = ValidateRow(i);

                if (valid)
                    continue;

                ErrorToOutput($"Invalid file path: {GetMapFilePath(i)}");
                DeleteRow(i);
                i--;
            }



            return DataGrid.Rows.Count > 1;
        }

        private string GetMapFilePath(int row) => DataGrid.Rows[row].Cells[1].Value.ToString();

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!(e.ColumnIndex == 2 || e.ColumnIndex == 3))
                return;

            float i;

            if (float.TryParse(Convert.ToString(DataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value), out i))
                return;

            DataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = i.ToString();
        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ButtonImportData_Click(object sender, EventArgs e)
        {
            var browser = new OpenFileDialog()
            {
                Filter = "Json File|*.json;",
                InitialDirectory = CurrentDirectory
            };

            if (browser.ShowDialog() != DialogResult.OK || !browser.CheckPathExists)
            {
                ErrorToOutput("Json import cancelled.");
                return;
            }

            try
            {
                var file = File.ReadAllText(browser.FileName);

                PrintToOutput(file);
                ClearTable();

                var data = JsonConvert.DeserializeObject<JsonData>(file);

                foreach (var map in data.Dataset)
                {
                    ImportMapData(map.FilePath, map.TargetDifficulty, map.Weight);
                }

                var valid = ValidateData();

                if (!valid)
                {
                    ErrorToOutput($"Dataset is invalid. Make sure you have set the correct map directory. Saved directory: {data.RootDirectory}");
                    return;
                }

                PrintToOutput("Import succeeded.");
            }
            catch
            {
                ErrorToOutput("Import Failed. Failed to deserialize file.");
            }
        }

        private void ClearTable()
        {
            DataGrid.Rows.Clear();
        }

        private void ButtonExportData_Click(object sender, EventArgs e)
        {
            var valid = ValidateData();

            if (!valid)
            {
                ErrorToOutput("Export failed. Invalid dataset.");
                return;
            }

            var data = new List<MapData>();

            try
            {
                for (var row = 0; row < DataGrid.Rows.Count - 1; row++)
                    data.Add(ConvertRowToData(row));
            }
            catch
            {
                ErrorToOutput("Export failed. Unable to parse the data.");
                return;
            }

            try
            {
                var toSerialize = new JsonData()
                {
                    RootDirectory = RootFolder,
                    Dataset = data
                };

                var json = JsonConvert.SerializeObject(toSerialize); // Json..Serialize(toSerialize);
                var dir = CurrentDirectory;

                File.WriteAllText($"{CurrentDirectory}\\DifficultyData.json", json);

                PrintToOutput($"Json has been successfully created in: {CurrentDirectory}");
            }
            catch
            {
                ErrorToOutput("Export failed. Failed to write data to disk.");
            }
        }

        private MapData ConvertRowToData(int row)
        {
            var file = GetMapFilePath(row);
            var difficulty = float.Parse(DataGrid.Rows[row].Cells[2].Value.ToString());
            var weight = float.Parse(DataGrid.Rows[row].Cells[2].Value.ToString());
            return new MapData(file, difficulty, weight);
        }
    }
}
