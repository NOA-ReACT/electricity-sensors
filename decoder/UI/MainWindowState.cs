using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOAReact.ElectricitySensorDecoder.UI;

public class MainWindowState : INotifyPropertyChanged
{
    /// <summary>
    /// Path to currently selected file
    /// </summary>
    string? selectedPath = null;

    /// <summary>
    /// Whether the "Open" button should be enabled
    /// </summary>
    public bool OpenEnabled { get => selectedPath == null; }
    
    /// <summary>
    /// Whether the "Close" button should be enabled
    /// </summary>
    public bool CloseEnabled { get => selectedPath != null; }

    /// <summary>
    /// Path to currently selected file
    /// </summary>
    public string? SelectedPath
    {
        get => selectedPath;
        set
        {
            selectedPath = value;
            OnPropertyChanged(nameof(SelectedPath));
            OnPropertyChanged(nameof(OpenEnabled));
            OnPropertyChanged(nameof(CloseEnabled));
        }
    }

    /// <summary>
    /// Data point values (X-axis) for the plot, each variable corresponds to one list
    /// </summary>
    public Dictionary<string, List<double>> DataPoints = new();

    /// <summary>
    /// Index of last packet in DataPoints array.
    /// Used so we know only to append new points.
    /// </summary>
    public int LastPacketIndex = 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string info)
    {
        Console.WriteLine(info);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
}
