using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Views.Pages.Dialogs;

public partial class PrinterSelectionDialog : UserControl
{
    public PrinterSelectionDialog()
    {
        InitializeComponent();
    }

    // Method for getting the selected printers from listbox
    public List<PrinterModel> GetSelectedPrinters()
    {
        return PrintersListBox.SelectedItems?.Cast<PrinterModel>().ToList() ?? new List<PrinterModel>();
    }
}