using CRDB_Farmers.DTos;
using CRDB_Farmers.Helper;
using ExcelDataReader.Log;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace CRDB_Farmers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region FilesPaths
        public string ExcelFilePath { get; set; }
        public string EmbossingFilePath { get; set; }
        public string FarmerImagesPath { get; set; }
        public string QrCodeImagesPath { get; set; }

        List<ExcelDto> ExcelFileData = new List<ExcelDto>();

        #endregion

        #region intializeWindow
        public MainWindow()
        {

            InitializeComponent();
        }
        #endregion

        #region ChooseExcelFile
        private void ChooseExcelFile_Click(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;
            SuccessLabel.Visibility = Visibility.Collapsed;
            LoadingBar.Visibility = Visibility.Collapsed;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Csv File",
                Filter = "Excel Files|*.csv;*.csv",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ExcelFilePath = openFileDialog.FileName;
                SelectedFileNameTextBlock.Text = ExcelFilePath;



            }
        }
        #endregion

        #region ChooseEmbossingFile
        private void ChooseEmbossingFile_Click(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;
            SuccessLabel.Visibility = Visibility.Collapsed;
            LoadingBar.Visibility = Visibility.Collapsed;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Text File",
                Filter = "Text Files|*.txt;*.inp;*.cif;*.dat;*.log;*.ini|All Files|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                EmbossingFilePath = openFileDialog.FileName;
                SelectedCifFileNameTextBlock.Text = EmbossingFilePath;



            }
        }

        #endregion

       
        #region ProcessData

        
        private async void ProcessFiles_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            DataComparisonResult _comparisonResult = new DataComparisonResult();
            List<EmbossingDto> ListEmbosing = new List<EmbossingDto>();

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Choose a folder to save output images";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                    return;

                processButton.IsEnabled = false;
                string savePath = dialog.SelectedPath;

                // 1️⃣ Read Excel
                ExcelFileData = Excel.ReadExcelAsList<ExcelDto>(ExcelFilePath);
                if (ExcelFileData.Count == 0)
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                    ErrorLabel.Content = "There isn't any data in excel file !";
                    ErrorLabel.Visibility = Visibility.Visible;
                    return;
                }

                // 2️⃣ Create folders for images
                FarmerImagesPath = Folder.CreateFolder(savePath, "FarmerImages");
                QrCodeImagesPath = Folder.CreateFolder(savePath, "QrCodeImages");

                // 3️⃣ UI feedback
                ErrorLabel.Visibility = Visibility.Collapsed;
                SuccessLabel.Visibility = Visibility.Collapsed;
                LoadingBar.Visibility = Visibility.Visible;

                #region Handle Embossing File
                try
                {
                    var EmbossingFileRecords = EmbossingFile.GetEmbossingFileData(EmbossingFilePath);
                    if (EmbossingFileRecords.Length == 0)
                    {
                        LoadingBar.Visibility = Visibility.Collapsed;
                        ErrorLabel.Content = "There isn't any data in Embossing file !";
                        ErrorLabel.Visibility = Visibility.Visible;
                        return;
                    }

                    ListEmbosing = EmbossingFile.Parse(EmbossingFileRecords, EmbossingFile.FileRecordsDataInBytes, ref _comparisonResult);
                }
                catch (Exception ex)
                {
                    errorMessage += $" {ex.Message}\n";
                }
                #endregion

                #region Comparison (Common + Failed)
                try
                {
                    ProcessFilesData.ProcessFiles(ListEmbosing, ExcelFileData, ref _comparisonResult);
                }
                catch (Exception ex)
                {
                    errorMessage += $" {ex.Message}\n";
                    LoadingBar.Visibility = Visibility.Collapsed;
                    ErrorLabel.Content = errorMessage;
                    ErrorLabel.Visibility = Visibility.Visible;
                    return;
                }
                #endregion

                #region Create MDB File (handle insert failures)
                if (_comparisonResult.CommonData.Count > 0)
                {
                    MDBFile.CreateMDBFile(_comparisonResult.CommonData, savePath, "FarmersPatch", ref errorMessage);
                    // أي failed insert أثناء MDB تم إضافته تلقائيًا داخل _comparisonResult
                }
                else
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                    ErrorLabel.Content = "There isn't any matched data between two files";
                    ErrorLabel.Visibility = Visibility.Visible;
                    return;
                }
                #endregion

                #region Parallel Image Processing (after MDB)
                var excelDict = ExcelFileData
                    .GroupBy(c => c.AccountNumber)
                    .ToDictionary(g => g.Key, g => g.First());

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                await Task.Run(() =>
                {
                    Parallel.ForEach(_comparisonResult.CommonData, parallelOptions, item =>
                    {
                        if (!excelDict.TryGetValue(item.AccountNumber, out var excel))
                            return;

                        // Farmer Image
                        if (!string.IsNullOrEmpty(excel.FarmerImage))
                        {
                            try
                            {
                                image.ForceConvertBase64ToPng(
                                    excel.FarmerImage,
                                    FarmerImagesPath,
                                    item.AccountNumber,
                                    ref errorMessage);
                            }
                            catch (Exception ex)
                            {
                                lock (_comparisonResult)
                                {
                                    _comparisonResult.AddExcelRecord(item.AccountNumber, $"Farmer image error: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            lock (_comparisonResult)
                            {
                                _comparisonResult.AddExcelRecord(item.AccountNumber, "There isn't farmer image");
                            }
                        }

                        // QR Code
                        if (!string.IsNullOrEmpty(excel.QRCode))
                        {
                            try
                            {
                                image.ForceConvertBase64ToPng(
                                    excel.QRCode,
                                    QrCodeImagesPath,
                                    item.AccountNumber,
                                    ref errorMessage);
                            }
                            catch (Exception ex)
                            {
                                lock (_comparisonResult)
                                {
                                    _comparisonResult.AddExcelRecord(item.AccountNumber, $"QR code error: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            lock (_comparisonResult)
                            {
                                _comparisonResult.AddExcelRecord(item.AccountNumber, "There isn't QR code image");
                            }
                        }
                    });
                });
                #endregion

                #region Create Excel Failed Report (all sources)
                var combinedFailedRecords = new List<failedRecords>();
                combinedFailedRecords.AddRange(_comparisonResult.OnlyInOrFailedFromEmbossingFile);
                combinedFailedRecords.AddRange(_comparisonResult.OnlyInOrFailedFromExcelFile);

                if (combinedFailedRecords.Count > 0)
                {
                    Excel.ExportListToExcel(combinedFailedRecords, savePath, "FailedData", ref errorMessage);
                }
                #endregion

                LoadingBar.Visibility = Visibility.Collapsed;
                processButton.IsEnabled = true;
                SuccessLabel.Content = $"Operation completed successfully ({_comparisonResult.CommonData.Count} records succeeded)!";
                SuccessLabel.Visibility = Visibility.Visible;
            }
        }

        #endregion
        

        #region Code-Behind for Moving Window
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
