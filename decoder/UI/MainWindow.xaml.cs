using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using NOAReact.ElectricitySensorDecoder.Library;
using System.IO;

namespace NOAReact.ElectricitySensorDecoder.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    FileSystemWatcher? watcher = null;

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handler for the "Open" button click. Opens the dialog to select a file.
    /// </summary>
    private void btnOpen_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "GSF Files (*.gsf)|*gsf|All Files|*",
            Title = "Decoder: Open GSF File..."
        };

        if (openFileDialog.ShowDialog() == true)
        {
            SelectFile(openFileDialog.FileName);
        }
    }

    /// <summary>
    /// Handler for the "Close" button click. Disabled the currently active watcher.
    /// </summary>
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        if (state.SelectedPath != null)
        {
            state.SelectedPath = null;
        }
        if (watcher != null)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
        }
    }

    /// <summary>
    /// Upon selection of a new GSF file, prepares the file watcher.
    /// </summary>
    /// <param name="newPath">The new selected GSF file</param>
    private void SelectFile(string newPath)
    {
        state.SelectedPath = newPath;
        state.DataPoints.Clear();
        state.DataPoints["mill_counts"] = new List<double>();
        state.DataPoints["space_counts"] = new List<double>();
        state.LastPacketIndex = 0;

        var inputParent = Directory.GetParent(newPath);
        if (inputParent == null || !inputParent.Exists)
        {
            MessageBox.Show("File's parent directory is not accessible! Cannot continue.");
            state.SelectedPath = null;
            return;
        }

        watcher = new FileSystemWatcher(inputParent.FullName);
        watcher.Filter = Path.GetFileNameWithoutExtension(newPath) + "*";
        watcher.Changed += HandleChangedFile;
        watcher.Created += HandleChangedFile;
        watcher.EnableRaisingEvents = true;

        HandleChangedFile(this, null);
    }

    /// <summary>
    /// Called by the file watcher when the GSF files are updated. We read the data here and create the timeseries for the plot.
    /// </summary>
    private void HandleChangedFile(object sender, FileSystemEventArgs e)
    {
        if (state.SelectedPath == null)
        {
            return;
        }

        var parser = new GSFParser(state.SelectedPath);
        var startTime = parser.GetStartTime();

        var packets = parser.GetXData();

        int i = 0;
        foreach (var packet in packets)
        {
            if (i < state.LastPacketIndex)
            {
                i++;
                continue;
            }

            var message = new XDataMessage(startTime, packet);
            if (message.IsValid)
            {
                state.DataPoints["space_counts"].Add(Convert.ToDouble(message.Variables["space_counts"]));
                state.DataPoints["mill_counts"].Add(Convert.ToDouble(message.Variables["mill_counts"]));
            }
        }

        plotArea.Plot.Clear();
        foreach (var variableName in state.DataPoints.Keys)
        {
            plotArea.Plot.AddSignal(state.DataPoints[variableName].ToArray());
        }
        plotArea.Refresh();
    }

    /// <summary>
    /// Handler for the "to CSV" button
    /// </summary>
    private void btnToCSV_Click(object sender, RoutedEventArgs e)
    {
        if (state.SelectedPath == null)
        {
            return;
        }

        // Open input file
        var parser = new GSFParser(state.SelectedPath);
        var startTime = parser.GetStartTime();
        var formattedTime = startTime.ToString("yyyy-MM-dd_HH-mm-ss");

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*csv|All Files|*",
            Title = "Decoder: Save data as CSV file",
            FileName = formattedTime + ".csv"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            var csvFile = new CSVFile(saveFileDialog.FileName);
            csvFile.WriteHeader();

            var packets = parser.GetXData();
            foreach (var packet in packets)
            {
                var xdata = new XDataMessage(startTime, packet);
                csvFile.WriteXDataMessage(xdata);
            }
        }

        MessageBox.Show("File saved!");
    }
}

