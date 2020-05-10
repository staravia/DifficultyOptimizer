using Accord.Math.Convergence;
using Accord.Math.Optimization;
using DifficultyOptimizer.src;
using Newtonsoft.Json;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Parsers;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DifficultyOptimizer
{
    public partial class DifficultyOptimizerForm : Form
    {
        /// <summary>
        /// The root folder of our reference maps.
        /// </summary>
        private string RootFolder { get; set; }

        /// <summary>
        /// The directory of this application.
        /// </summary>
        private string CurrentDirectory { get; }

        /// <summary>
        /// Used to import files.
        /// </summary>
        private OpenFileDialog FileBrowser = new OpenFileDialog();

        /// <summary>
        /// Used to reference the current directory.
        /// </summary>
        private FolderBrowserDialog FolderBrowser = new FolderBrowserDialog();

        /// <summary>
        /// 
        /// </summary>
        private StrainConstantsKeys Constants { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        private List<MapData> MapData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource TokenSource { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private NelderMead Solver { get; }
        
        private string RemoveRoot(string file) => file.Replace(RootFolder, ".");

        private string AddRoot(string file) => RootFolder + file.Substring(1);

        private void DeleteRow(int row) => DataGrid.Rows.RemoveAt(row);

        private bool ValidateRow(int row) => File.Exists(AddRoot(GetMapFilePath(row)));
        private event EventHandler<string> OptimizeStepComplete;

        public DifficultyOptimizerForm()
        {
            CurrentDirectory = Directory.GetCurrentDirectory();
            FileBrowser.Multiselect = true;
            FileBrowser.Filter = "Map File|*.osu;*.qua";
            SetRootFolder(CurrentDirectory);
            InitializeComponent();

            CheckForLocalJson();

            Constants = new StrainConstantsKeys();
            foreach (var constant in Constants.ConstantVariables)
                ImportConstantData(constant.Name, constant.Value);
            
            VariableGrid.AllowUserToAddRows = false;
            VariableGrid.AllowUserToDeleteRows = false;
            OptimizeStepComplete += OnOptimizeStepComplete;
            
            // Create Nelder Mead Solver
            Solver = new NelderMead(Constants.ConstantVariables.Count, GetOptimizedValue)
            {
                Convergence = new GeneralConvergence(Constants.ConstantVariables.Count)
                {
                    Evaluations = 0, 
                    MaximumEvaluations = 100000
                }
            };
        }

        private void CheckForLocalJson()
        {
            var path = $"{CurrentDirectory}\\DifficultyData.json";

            PrintToOutput($"Searching for local DifficultyData.json file: {path}");

            if (File.Exists(path))
            {
                try
                {

                    var file = File.ReadAllText(path);
                    var data = JsonConvert.DeserializeObject<JsonData>(file);

                    if (!Directory.Exists(data.RootDirectory))
                    {
                        ErrorToOutput($"Local DifficultyData.json found, but Directory does not exist: {data.RootDirectory}");
                        PrintToOutput($"If you are importing this data from another machine, please set the maps directory.");
                        return;
                    }

                    SetRootFolder(data.RootDirectory);
                    ImportMaps(data);
                }
                catch
                {
                    ErrorToOutput($"Failed to parse local DifficultyData.json file");
                }
                return;
            }

            PrintToOutput($"DifficultyData.json does not exist. Please set your maps directory before importing maps or datasets. \nCurrent directory: {CurrentDirectory}");
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

        private double GetInputConstant(int index)
        {
            float val;

            if (float.TryParse(Convert.ToString(VariableGrid.Rows[index].Cells[2].Value), out val))
                return val;

            return 0;
        }

        private void ButtonOptimize_Click(object sender, EventArgs e)
        {
            // Todo: Find a way to cancel the optimization while it is running
            if (TokenSource != null)
            {
                TokenSource.Cancel();
                return;
            }

            HandleOptimization();
        }

        private async Task HandleOptimization()
        {
            // Clear the output so it doesn't get clogged up.
            ClearOutput();
            
            // Update MapData List
            TokenSource = new CancellationTokenSource();
            MapData = ParseMapData(true);
            UpdateSolver();

            if (MapData.Count == 0)
            {
                ErrorToOutput("Failed to parse map data.");
                return;
            }
            
            // Initialize Input
            var input = new double[Constants.ConstantVariables.Count];

            for (var i = 0; i < Constants.ConstantVariables.Count; i++)
                input[i] = GetInputConstant(i);
            
            // Create Stopwatch
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Solve
            try
            {
                Action<Task> action = delegate { HandleOptimizeCompleted(stopwatch); };
                var task = Task.Run(() => Solver.Minimize(input), TokenSource.Token)
                    .ContinueWith(action, TokenSource.Token);

                await task;

                TokenSource.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                PrintToOutput("Optimization Cancelled.");
                TokenSource.Dispose();
            }
            finally
            {
                TokenSource = null;
            }
        }

        /// <summary>
        /// </summary>
        private void UpdateSolver()
        {
            // Update Cancellation Token
            Solver.Token = TokenSource.Token;
            
            // Set Lower bounds
            for (var i = 0; i < Solver.LowerBounds.Length; i++)
            {
                float val;

                if (float.TryParse(Convert.ToString(VariableGrid.Rows[i].Cells[5].Value), out val))
                    Solver.LowerBounds[i] = val;
            }
            
            // Set Upper bounds
            for (var i = 0; i < Solver.LowerBounds.Length; i++)
            {
                float val;

                if (float.TryParse(Convert.ToString(VariableGrid.Rows[i].Cells[4].Value), out val))
                    Solver.UpperBounds[i] = val;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private double GetOptimizedValue(double[] input)
        {
            if (TokenSource.IsCancellationRequested)
                return 0;
            
            float[] inputf = Array.ConvertAll(input, x => (float)x);
            double total = 0;
            double weight = 0;
            
            Constants = new StrainConstantsKeys(inputf);
            
            // Solve Every Map's Difficulty
            foreach (var map in MapData)
            {
                var diff = map.Map.SolveDifficulty(ModIdentifier.None, Constants).OverallDifficulty;
                var delta = Math.Pow(10f * (diff - map.TargetDifficulty), 2);

                total += delta * map.Weight;
                weight += map.Weight;
            }

            var value = total / weight;

            OptimizeStepComplete?.Invoke(this, $"Current f(x) = {value}");

            return value;
        }

        /// <summary>
        /// </summary>
        /// <param name="stopwatch"></param>
        private void HandleOptimizeCompleted(Stopwatch stopwatch)
        {
            TokenSource.Dispose();
            TokenSource = null;
            stopwatch.Stop();
            PrintToOutput($"Done! Took {stopwatch.Elapsed.TotalSeconds} seconds to compute!");

            for (var i = 0; i < Solver.Solution.Length; i++)
                UpdateConstantData(i, (float)Solver.Solution[i]);

            for (var i = 0; i < MapData.Count; i++)
                UpdaateMapOutput(i, MapData[i].Map.SolveDifficulty(ModIdentifier.None, Constants).OverallDifficulty);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="output"></param>
        private void OnOptimizeStepComplete(object sender, string output)
        {
            PrintToOutput(output);
            ProgressBar.Value = (int)(100 * Solver.Convergence.Evaluations / (float)Solver.Convergence.MaximumEvaluations);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSetDirectory_Click(object sender, EventArgs e)
        {
            if (FolderBrowser.ShowDialog() != DialogResult.OK)
            {
                ErrorToOutput("Cancelled Root Directory setup");
                return;
            }

            PrintToOutput($"Relative Root directory for maps has been set to: {FolderBrowser.SelectedPath}");
            SetRootFolder(FolderBrowser.SelectedPath);
        }

        /// <summary>
        /// </summary>
        /// <param name="root"></param>
        private void SetRootFolder(string root)
        {
            RootFolder = root;
            FileBrowser.InitialDirectory = RootFolder;
        }

        /// <summary>
        /// </summary>
        private void ClearOutput() => TextBoxOutput.Clear();

        /// <summary>
        /// </summary>
        /// <param name="output"></param>
        private void PrintToOutput(string output)
        {
            TextBoxOutput.AppendText($"{output}\n");
            TextBoxOutput.ScrollToCaret();
        }

        /// <summary>
        /// </summary>
        /// <param name="output"></param>
        private void ErrorToOutput(string output)
        {
            TextBoxOutput.SelectionColor = Color.Red;
            TextBoxOutput.AppendText($"{output}\n");
            TextBoxOutput.ScrollToCaret();
        }
        
        private void ImportMapData(string file, float diff = 1f, float weight = 0.5f)
        {
            try
            {
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
        
        private void UpdaateMapOutput(int index, float diff) => DataGrid.Rows[index].Cells[4].Value = diff;

        private void ImportConstantData(string name, float value, bool optimize = true, float max = 1000f, float min = 0)
        {
            var row = (DataGridViewRow)VariableGrid.Rows[0].Clone();
            row.Cells[1].Value = name;
            row.Cells[2].Value = value;
            row.Cells[0].Value = optimize;
            row.Cells[4].Value = max;
            row.Cells[5].Value = min;
            VariableGrid.Rows.Add(row);
        }

        private void ButtonImportMap_Click(object sender, EventArgs e)
        {
            if (FileBrowser.ShowDialog() != DialogResult.OK || !FileBrowser.CheckFileExists)
            {
                ErrorToOutput("Maps Import Cancelled.");
                return;
            }

            foreach (var file in FileBrowser.FileNames)
            {
                ImportMapData(file);
            }
        }


        private void UpdateConstantData(int index, float output) =>
            VariableGrid.Rows[index].Cells[3].Value = output;

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
                ClearTable();

                var data = JsonConvert.DeserializeObject<JsonData>(file);
                ImportMaps(data);
            }
            catch
            {
                ErrorToOutput("Import Failed. Failed to deserialize file.");
            }
        }

        private void ImportMaps(JsonData data)
        {
            foreach (var map in data.Dataset)
                ImportMapData(map.FilePath, map.TargetDifficulty, map.Weight);

            var valid = ValidateData();

            if (!valid)
            {
                ErrorToOutput("Import Failed. Failed to find a single file path. Make sure that the maps directory is set.");
                PrintToOutput($"Current Directory has been changed to: {CurrentDirectory}");
                return;
            }

            PrintToOutput("Import succeeded.");
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

            var data = ParseMapData();

            if (data.Count == 0)
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

        private MapData ConvertRowToData(int row, bool parseMap = false)
        {
            var file = parseMap ? AddRoot(GetMapFilePath(row)) : GetMapFilePath(row);
            var difficulty = float.Parse(DataGrid.Rows[row].Cells[2].Value.ToString());
            var weight = float.Parse(DataGrid.Rows[row].Cells[3].Value.ToString());
            Qua map = null;

            if (parseMap)
            {
                try
                {
                    if (file.EndsWith(".qua"))
                        map = Qua.Parse(file);
                    else if (file.EndsWith(".osu"))
                        map = new OsuBeatmap(file).ToQua();

                    PrintToOutput($"Map successfully parsed: {file}");
                }
                catch (Exception e)
                {
                    ErrorToOutput($"Error parsing map: {e.Message}");
                }
            }

            return new MapData(file, difficulty, weight, map);
        }

        private List<MapData> ParseMapData(bool parseMap = false)
        {
            var data = new List<MapData>();

            for (var row = 0; row < DataGrid.Rows.Count - 1; row++)
            {
                try
                {
                    data.Add(ConvertRowToData(row, parseMap));
                }
                catch
                {
                    // Ignored
                }
            }

            return data;
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void VariableGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ButtonCalculate_Click(object sender, EventArgs e)
        {
            //asd
        }
    }
}
