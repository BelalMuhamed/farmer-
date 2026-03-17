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
            #region GetDataFromFiles
            using (var dialog = new FolderBrowserDialog())
            {
                #region Choose Path to save 
                dialog.Description = "Choose a folder to save output images";
                dialog.ShowNewFolderButton = true;

                var result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return; 
                }
                processButton.IsEnabled=false;
                string savePath = dialog.SelectedPath;
                #endregion

                #region read excel file data 
                ExcelFileData = Excel.ReadExcelAsList<ExcelDto>(ExcelFilePath);
                #endregion

                #region handle if no data in excel file 
                if (ExcelFileData.Count == 0)
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                    errorMessage = "There isn't any data in excel file !";
                    ErrorLabel.Content = errorMessage;
                    ErrorLabel.Visibility = Visibility.Visible;
                    return;
                }
                #endregion

                #region create folders
                FarmerImagesPath = Folder.CreateFolder(savePath, "FarmerImages");
                QrCodeImagesPath = Folder.CreateFolder(savePath, "QrCodeImages");
                #endregion

                #region controls
                ErrorLabel.Visibility = Visibility.Collapsed;
                SuccessLabel.Visibility = Visibility.Collapsed;
                LoadingBar.Visibility = Visibility.Visible;
                #endregion

                #region Handle farmer images  
                await Task.Run(() =>
                {
                    for (int i = 0; i < ExcelFileData.Count; i++)
                    {
                         if(string.IsNullOrEmpty(ExcelFileData[i].FarmerImage))
                         {
                            _comparisonResult.AddExcelRecord(ExcelFileData[i].AccountNumber, "There isn't farmer image ");
                            continue;

                        }
                        try
                        {
                            image.ForceConvertBase64ToPng(ExcelFileData[i].FarmerImage, FarmerImagesPath, ExcelFileData[i].AccountNumber, ref errorMessage);

                        }
                        catch (Exception ex)
                        {
                            _comparisonResult.AddExcelRecord(ExcelFileData[i].AccountNumber, $"Cann't Proccess farmer  image because {ex.Message}");
                            continue;
                        }


                    }
                });
                #endregion

                #region Handle QrCodeImage
                await Task.Run(() =>
                {
                    for (int i = 0; i < ExcelFileData.Count; i++)
                    {
                        if (string.IsNullOrEmpty(ExcelFileData[i].FarmerImage))
                        {
                            _comparisonResult.AddExcelRecord(ExcelFileData[i].AccountNumber, "There isn't farmer image ");
                            continue;


                        }

                        try
                        {
                            image.ForceConvertBase64ToPng(ExcelFileData[i].QRCode, QrCodeImagesPath, ExcelFileData[i].AccountNumber, ref errorMessage);

                        }
                        catch (Exception ex)
                        {
                            _comparisonResult.AddExcelRecord(ExcelFileData[i].AccountNumber, $"Cann't Proccess Qr code image because {ex.Message}");
                            continue;
                        }

                    }
                });
                #endregion

                #region Handle Embossing File
                await Task.Run(() =>
                {

                    try
                    {
                        var EmbossingFileRecords = EmbossingFile.GetEmbossingFileData(EmbossingFilePath);
                        if(EmbossingFileRecords.Length ==0)
                        {
                            LoadingBar.Visibility = Visibility.Collapsed;
                            errorMessage = "There isn't any data in Embossing file !";
                            ErrorLabel.Content = errorMessage;
                            ErrorLabel.Visibility = Visibility.Visible;

                            return;
                        }

                         ListEmbosing = EmbossingFile.Parse(EmbossingFileRecords, EmbossingFile.FileRecordsDataInBytes ,ref _comparisonResult);
                       
                    }
                    catch (Exception ex)
                    {
                        errorMessage += $" {ex.Message}\n";
                    }

                });
                #endregion

                #region comparisonFileData


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



                #region create Report Missed Data 

                var combinedFailedRecords = new List<failedRecords>();

                if (_comparisonResult.OnlyInOrFailedFromEmbossingFile.Count > 0)
                {
                    combinedFailedRecords.AddRange(_comparisonResult.OnlyInOrFailedFromEmbossingFile);
                }

                if (_comparisonResult.OnlyInOrFailedFromExcelFile.Count > 0)
                {
                    combinedFailedRecords.AddRange(_comparisonResult.OnlyInOrFailedFromExcelFile);
                }

                if (combinedFailedRecords.Count > 0)
                {
                    Excel.ExportListToExcel(combinedFailedRecords, savePath, "FailedData", ref errorMessage);
                }
                #endregion

                #region CraeteMDPFile
                if (_comparisonResult.CommonData.Count > 0)
                {
                    MDBFile.CreateMDBFile(_comparisonResult.CommonData, savePath, "FarmersPatch", ref errorMessage);
                }
                else
                {
                    errorMessage = "there isn't any matched data between two files ";
                    LoadingBar.Visibility = Visibility.Collapsed;
                    ErrorLabel.Content = errorMessage;
                    ErrorLabel.Visibility = Visibility.Visible;
                    return;
                }
                #endregion

                LoadingBar.Visibility = Visibility.Collapsed;
                processButton.IsEnabled = true;
                SuccessLabel.Content = $"Operation completed successfully({_comparisonResult.CommonData.Count} records  are succeeded)!";

                SuccessLabel.Visibility = Visibility.Visible;
                
            }
            #endregion

          
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
