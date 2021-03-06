﻿using Accord.Math.Convergence;
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
using Quaver.API.Maps.Processors.Difficulty.Optimization;

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
        private readonly OpenFileDialog FileBrowser = new OpenFileDialog();

        /// <summary>
        /// Used to reference the current directory.
        /// </summary>
        private readonly FolderBrowserDialog FolderBrowser = new FolderBrowserDialog();

        /// <summary>
        /// Reference to the current constant variables.
        /// </summary>
        private StrainConstantsKeys Constants { get; set; }
        
        /// <summary>
        /// Reference to the current map dataset.
        /// </summary>
        private List<MapData> MapData { get; set; }

        /// <summary>
        /// Used to cancel the optimization task.
        /// </summary>
        private CancellationTokenSource TokenSource { get; set; }

        /// <summary>
        /// Used to optimize the map difficulties.
        /// </summary>
        private NelderMead Solver { get; set; }
        
        /// <summary>
        /// Determines which constants by which constants are supposed to be solved.
        /// </summary>
        private bool[] ActiveConstants { get; }
        
        /// <summary>
        /// Removes the root dir of given path.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string RemoveRoot(string file) => file.Replace(RootFolder, ".");

        /// <summary>
        /// Adds the root dir to the given path.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string AddRoot(string file) => RootFolder + file.Substring(1);

        /// <summary>
        /// Deletes a row at the given index in the maps data grid.
        /// </summary>
        /// <param name="row"></param>
        private void DeleteMapDataRow(int row) => DataGrid.Rows.RemoveAt(row);

        /// <summary>
        /// This will validate the data.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool ValidateMapDataRow(int row) => File.Exists(AddRoot(GetMapFilePath(row)));
        
        /// <summary>
        /// This will check every maps to see if they are valid.
        /// </summary>
        /// <returns></returns>
        private bool ValidateAllMaps()
        {
            for (var i = 0; i < DataGrid.Rows.Count - 1; i++)
            {
                var valid = ValidateMapDataRow(i);

                if (valid)
                    continue;

                ErrorToOutput($"Invalid file path: {GetMapFilePath(i)}");
                DeleteMapDataRow(i);
                i--;
            }

            return DataGrid.Rows.Count > 1;
        }
        
        /// <summary>
        /// Gets the file path of a map from a specific index
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private string GetMapFilePath(int row) => DataGrid.Rows[row].Cells[1].Value.ToString();
        
        /// <summary>
        /// This is invoked during optimization when a step has been completed.
        /// </summary>
        private event EventHandler<string> OptimizeStepComplete;

        /// <summary>
        /// </summary>
        public DifficultyOptimizerForm()
        {
            // Initialize File Browser / current dir
            CurrentDirectory = Directory.GetCurrentDirectory();
            FileBrowser.Multiselect = true;
            FileBrowser.Filter = "Map File|*.osu;*.qua";
            
            // Initialize UI/data
            SetRootFolder(CurrentDirectory);
            InitializeComponent();
            InitializeConstants();
            CheckForLocalMapData();
            OptimizeStepComplete += OnOptimizeStepComplete;
            
            // Initialize ActiveConstants
            ActiveConstants = new bool[Constants.ConstantVariables.Count];
        }

        /// <summary>
        /// This initializes the constant variables in the difficulty solver.
        /// </summary>
        private void InitializeConstants()
        {
            Constants = new StrainConstantsKeys();
            foreach (var constant in Constants.ConstantVariables)
                TryImportConstantData(constant.Name, constant.Value);
            
            VariableGrid.AllowUserToAddRows = false;
            VariableGrid.AllowUserToDeleteRows = false;
        }

        /// <summary>
        /// This will scan the root directory for the DifficultyData.json file
        /// </summary>
        private void CheckForLocalMapData()
        {
            var path = $"{CurrentDirectory}\\DifficultyData.json";
            
            PrintToOutput($"Searching for local DifficultyData.json file: {path}");

            // If there's no DifficultyData.json file in the current path of this program, the user will have to manually set things up.
            if (!File.Exists(path))
            {
                PrintToOutput($"DifficultyData.json does not exist. Please set your maps directory before importing maps or datasets. \nCurrent directory: {CurrentDirectory}");
                return;
            }

            // This will attempt to import all the map data into the current program.
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
                ImportConstants(data);
            }
            catch
            {
                ErrorToOutput($"Failed to parse local DifficultyData.json file");
            }
        }

        /// <summary>
        /// Imports all the constant data that is saved.
        /// </summary>
        /// <param name="data"></param>
        private void ImportConstants(JsonData data)
        {
            try
            {
                if (data.Constants.Length != Constants.ConstantVariables.Count)
                {
                    ErrorToOutput("Total number of variables from DifficultyData.json does not match total constants in the current difficulty solver.");
                    return;
                }

                var dialogResult = MessageBox.Show("Do you want to use your previously saved constants?", "Difficulty Optimizer", MessageBoxButtons.YesNo);

                switch (dialogResult)
                {
                    case DialogResult.Yes:
                        for (var i = 0; i < data.Constants.Length; i++)
                            VariableGrid.Rows[i].Cells[2].Value = data.Constants[i];
                        
                        PrintToOutput("Successfully loaded saved constants. Using saved constants.");
                        return;
                    case DialogResult.No:
                        ErrorToOutput("Using default constants instead of saved constants. When saving, you will overwrite the previous file! Make sure you have a backup if needed.");
                        
                        return;
                }
            }
            catch
            {
                ErrorToOutput("Failed to import saved constants. Using default values.");
            }
        }

        /// <summary>
        /// This will prepare the optimizer.
        /// </summary>
        /// <returns></returns>
        private async Task HandleOptimization()
        {
            // Clear the output so it doesn't get clogged up.
            ClearOutput();
            
            // Update MapData List
            var token = new CancellationTokenSource();
            TokenSource = token;
            MapData = ParseAllMapData(true);
            ButtonOptimize.Text = "Cancel Optimization";
            InitializeSolver();

            // If there's no map data, it's either because it failed to parse or the user hasn't imported them yet.
            if (MapData.Count == 0)
            {
                ErrorToOutput("Failed to parse map data.");
                return;
            }
            
            // Initialize Stopwatch
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Optimize Difficulty via Solver
            try
            {
                Action<Task> action = delegate { HandleOptimizeCompleted(stopwatch); };

                var task = Task.Run(() => Solver.Minimize(ParseConstantsInput(true)), token.Token)
                    .ContinueWith(action, token.Token);

                await task;

                token.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                ErrorToOutput("Optimization Cancelled.");
                UpdateAllConstantOutputs();
                TokenSource = null;
            }
            catch (Exception e)
            {
                ErrorToOutput("Failed to optimize.");
                ErrorToOutput(e.Message);
                ErrorToOutput(e.StackTrace);
            }
            finally
            {
                ButtonOptimize.Text = "Optimize";
            }
        }
        
        /// <summary>
        /// Gets a single input for a specific constant with given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private double GetConstantFromInput(int index)
        {
            if (float.TryParse(Convert.ToString(VariableGrid.Rows[index].Cells[2].Value), out var val))
                return val;

            return 0;
        }

        /// <summary>
        /// Get all the inputted constants in a double array
        /// </summary>
        /// <returns></returns>
        private double[] ParseConstantsInput(bool partial)
        {
            double[] output;

            if (partial)
            {
                var count = 0;
                output = new double[Solver.Capacity];
                
                for (var i = 0; i < ActiveConstants.Length; i++)
                {
                    if (!ActiveConstants[i])
                        continue;

                    output[count] = GetConstantFromInput(i);
                    count++;
                }
            }
            else
            {
                output = new double[Constants.ConstantVariables.Count];
                
                for (var i = 0; i < Constants.ConstantVariables.Count; i++)
                    output[i] = GetConstantFromInput(i);   
            }

            return output;
        }
        
        /// <summary>
        /// Updates all the constants to match a specific input
        /// </summary>
        /// <param name="input"></param>
        private void UpdateConstants(double[] input, bool partial)
        {
            var inputf = new float[Constants.ConstantVariables.Count];
            if (partial)
            {
                var count = 0;
                
                for (var i = 0; i < Constants.ConstantVariables.Count; i++)
                {
                    if (!ActiveConstants[i])
                    {
                        inputf[i] = (float)GetConstantFromInput(i);
                        continue;
                    }

                    inputf[i] = (float)input[count];
                    count++;
                }
            }
            
            else
                inputf = Array.ConvertAll(input, x => (float)x);

            Constants = new StrainConstantsKeys(inputf);
        }

        /// <summary>
        /// Updates the current solver algorithm
        /// </summary>
        private void InitializeSolver()
        {
            // Update ActiveConstants
            var count = 0;
            
            for (var i = 0; i < Constants.ConstantVariables.Count; i++)
            {
                ActiveConstants[i] = (bool) VariableGrid.Rows[i].Cells[0].Value;

                if (ActiveConstants[i])
                    count++;
            }

            // Initialize Solver
            Solver = new NelderMead(count, SolverFX)
            {
                Convergence = new GeneralConvergence(count)
                {
                    Evaluations = 0, 
                    MaximumEvaluations = 200
                },
                
                Token = TokenSource.Token
            };

            // Set step size
            const double scale = 0.15;
            var data = ParseConstantsInput(true);

            for (var i = 0; i < Solver.StepSize.Length; i++)
                Solver.StepSize[i] = data[i] * scale;

            // Set Lower bounds
            count = 0;
            for (var i = 0; i < ActiveConstants.Length; i++)
            {
                if (!ActiveConstants[i])
                    continue;

                if (float.TryParse(Convert.ToString(VariableGrid.Rows[i].Cells[5].Value), out var min))
                    Solver.LowerBounds[count] = min;
                
                if (float.TryParse(Convert.ToString(VariableGrid.Rows[i].Cells[4].Value), out var max))
                    Solver.UpperBounds[count] = max;

                count++;
            }
        }

        /// <summary>
        /// This is used in the optimization method. This function could be cancelled.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private double SolverFX(double[] input)
        {
            if (TokenSource.IsCancellationRequested)
                return 0;

            UpdateConstants(input, true);
            
            return GetCurrentFX(false);
        }

        /// <summary>
        /// Gets the current F(x).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private double GetCurrentFX(bool updateUI)
        {
            double total = 0;
            double weight = 0;
            
            // Solve Every Map's Difficulty
            for (var i = 0; i < MapData.Count; i++)
            {
                try
                {
                    var map = MapData[i];
                    var diff = map.Map.SolveDifficulty(ModIdentifier.None, Constants).OverallDifficulty;
                    var delta = Math.Pow(10f * (diff - map.TargetDifficulty), 2);

                    total += delta * map.Weight;
                    weight += map.Weight;

                    if (updateUI)
                        UpdaateMapDifficultyOutput(i, diff);

                }
                catch (Exception)
                {
                    total += 100000;
                }
            }

            // Get an average for the F(x)
            var value = total / weight;
            
            OptimizeStepComplete?.Invoke(this, $"Current f(x) = {value}");

            return value;
        }

        /// <summary>
        /// This is called once the optimization method is complete
        /// </summary>
        /// <param name="stopwatch"></param>
        private void HandleOptimizeCompleted(Stopwatch stopwatch)
        {
            // Dispose cancellation token + stop stopwatch
            ProgressBar.Value = 100;
            TokenSource = null;
            stopwatch.Stop();

            // Compute for current difficulties + update output
            UpdateAllConstantOutputs();
            PrintCodeToOutput();
            GetCurrentFX(true);
            PrintToOutput($"Done! Took {stopwatch.Elapsed.TotalSeconds} seconds to compute!");
        }

        /// <summary>
        /// This will print stuff to the output that you can copy+paste into the API code
        /// </summary>
        private void PrintCodeToOutput()
        {
            PrintToOutput("-----------------------");
            
            for (var i = 0; i < VariableGrid.Rows.Count; i++)
            {
                var extra = i < VariableGrid.Rows.Count - 1 ? "," : "";
                PrintToOutput($"new ConstantVariable(\"{VariableGrid.Rows[i].Cells[1].Value}\", {VariableGrid.Rows[i].Cells[3].Value}f){extra}");   
            }
            
            PrintToOutput("-----------------------");

        }

        /// <summary>
        /// Updates all the constant outputs in the UI
        /// </summary>
        private void UpdateAllConstantOutputs()
        {
            for (var i = 0; i < Constants.ConstantVariables.Count; i++)
                UpdateConstantOutput(i, Constants.ConstantVariables[i].Value);
        }

        /// <summary>
        /// Computes the difficulty for every map and update the UI.
        /// </summary>
        private void ComputeForDifficulty()
        {
            if (MapData == null)
                MapData = ParseAllMapData(true);

            UpdateConstants(ParseConstantsInput(false), false);
            PrintCodeToOutput();
            GetCurrentFX(true);
        }

        /// <summary>
        /// This is called once a step has been completed during optimization.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="output"></param>
        private void OnOptimizeStepComplete(object sender, string output)
        {
            PrintToOutput(output);
            
            if (Solver != null)
                ProgressBar.Value = (int)(100 * Solver.Convergence.Evaluations / (float)Solver.Convergence.MaximumEvaluations);
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
        
        /// <summary>
        /// Tries to import map data from given file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="diff"></param>
        /// <param name="weight"></param>
        private void TryImportMapData(string file, float diff = 1f, float weight = 0.5f)
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
        
        /// <summary>
        /// Updates the UI to display the output of a map's difficulty.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="diff"></param>
        private void UpdaateMapDifficultyOutput(int index, float diff) => DataGrid.Rows[index].Cells[4].Value = diff;

        /// <summary>
        /// Tries to import constant data.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="optimize"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private void TryImportConstantData(string name, float value, bool optimize = true, float min = 0.05f, float max = -1)
        {
            var row = (DataGridViewRow)VariableGrid.Rows[0].Clone();
            row.Cells[1].Value = name;
            row.Cells[2].Value = value;
            row.Cells[0].Value = optimize;
            row.Cells[4].Value = max < 0 ? value * 2f + 5 : max;
            row.Cells[5].Value = min;
            VariableGrid.Rows.Add(row);
        }
        
        /// <summary>
        /// This will attempt to import map data with given json file.
        /// </summary>
        /// <param name="data"></param>
        private void ImportMaps(JsonData data)
        {
            foreach (var map in data.Dataset)
                TryImportMapData(map.FilePath, map.TargetDifficulty, map.Weight);

            var valid = ValidateAllMaps();

            if (!valid)
            {
                ErrorToOutput("Import Failed. Failed to find a single file path. Make sure that the maps directory is set.");
                PrintToOutput($"Current Directory has been changed to: {CurrentDirectory}");
                return;
            }

            PrintToOutput("Import succeeded.");
        }

        /// <summary>
        /// Tries to export map data as a DifficultyData.json file.
        /// </summary>
        private void TryExportData()
        {
            // Handle case where the dataset is invalid.
            var valid = ValidateAllMaps();
            if (!valid)
            {
                ErrorToOutput("Export failed. Invalid dataset.");
                return;
            }

            // Handle case where data is unable to be parsed.
            var data = ParseAllMapData();
            var constants = ParseConstantsInput(false);
            if (data.Count == 0)
            {
                ErrorToOutput("Export failed. Unable to parse the data.");
                return;
            }

            // Tries to serialize and write the DifficultyData.json file.
            try
            {
                var toSerialize = new JsonData
                {
                    RootDirectory = RootFolder,
                    Constants = constants,
                    Dataset = data
                };
                
                File.WriteAllText($"{CurrentDirectory}\\DifficultyData.json", JsonConvert.SerializeObject(toSerialize));
                PrintToOutput($"DifficultyData.json has been successfully created in: {CurrentDirectory}");
            }
            catch
            {
                ErrorToOutput("Export failed. Failed to write data to disk.");
            }
        }

        /// <summary>
        /// Converts the input of a specific row from the data grid into a Qua file.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="parseMap"></param>
        /// <returns></returns>
        private MapData ParseMapsRowData(int row, bool parseMap = false)
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

        /// <summary>
        /// Parses every maps into a list.
        /// </summary>
        /// <param name="parseMap"></param>
        /// <returns></returns>
        private List<MapData> ParseAllMapData(bool parseMap = false)
        {
            var data = new List<MapData>();

            for (var row = 0; row < DataGrid.Rows.Count - 1; row++)
            {
                try
                {
                    data.Add(ParseMapsRowData(row, parseMap));
                }
                catch
                {
                    // Ignored
                }
            }

            return data;
        }

        /// <summary>
        /// Updates the output of a constant in the UI
        /// </summary>
        /// <param name="index"></param>
        /// <param name="output"></param>
        private void UpdateConstantOutput(int index, float output) => VariableGrid.Rows[index].Cells[3].Value = output;
        
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!(e.ColumnIndex == 2 || e.ColumnIndex == 3))
                return;

            if (float.TryParse(Convert.ToString(DataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value), out var i))
                return;

            DataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = i;
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
                ErrorToOutput("DifficultyData.json import cancelled.");
                return;
            }

            try
            {
                var file = File.ReadAllText(browser.FileName);
                DataGrid.Rows.Clear();

                var data = JsonConvert.DeserializeObject<JsonData>(file);
                ImportMaps(data);
            }
            catch
            {
                ErrorToOutput("Import Failed. Failed to deserialize file.");
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void VariableGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!(e.ColumnIndex == 2 || e.ColumnIndex == 4 || e.ColumnIndex == 5))
                return;

            if (float.TryParse(Convert.ToString(VariableGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value), out var i))
                return;

            VariableGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = i;
        }

        private void ButtonCalculate_Click(object sender, EventArgs e) => ComputeForDifficulty();
        
        private void ButtonOptimize_Click(object sender, EventArgs e)
        {
            if (TokenSource != null)
            {
                TokenSource.Cancel();
                return;
            }

            HandleOptimization();
        }
        
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
        
        private void ButtonImportMap_Click(object sender, EventArgs e)
        {
            if (FileBrowser.ShowDialog() != DialogResult.OK || !FileBrowser.CheckFileExists)
            {
                ErrorToOutput("Maps Import Cancelled.");
                return;
            }

            foreach (var file in FileBrowser.FileNames)
            {
                TryImportMapData(file);
            }
        }
        
        private void ButtonExportData_Click(object sender, EventArgs e) => TryExportData();

        private void ButtonUseValues_Click(object sender, EventArgs e)
        {
            var success = false;
            for (var i = 0; i < Constants.ConstantVariables.Count; i++)
            {
                if (! (bool) VariableGrid.Rows[i].Cells[0].Value)
                    continue;

                try
                {
                    float value;
                    if (!float.TryParse(VariableGrid.Rows[i].Cells[3].Value.ToString(), out value))
                        continue;

                    success = true;
                    VariableGrid.Rows[i].Cells[2].Value = value;
                }
                catch
                {
                    // ignored
                }
            }
            
            if (!success)
                ErrorToOutput("There are currently no optimized values to use as the input.");
        }
    }
}
