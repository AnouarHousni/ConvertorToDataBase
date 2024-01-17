using ConvertorToDataBase.Enums;
using ConvertorToDataBase.Exceptions;
using ConvertorToDataBase.Interfaces;
using ConvertorToDataBase.Models;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;

namespace ConvertorToDataBase;

public partial class FinalPage : ContentPage
{
    const string frameID = "frame_";
    const string keyTypeLabelID = "keyTypeLabel_";
    const string keyTypePickerID = "keyTypePicker_";
    const string tablePickerID = "tablePicker_";
    const string columnPickerID = "columnPicker_";
    const string referenceLabelID = "referenceLabel_";
    const string referenceTablePickerID = "referenceTablePicker_";
    const string referenceColumnPickerID = "referenceColumnPicker_";
    
    private int count = 0;
    private string _dataBaseName;
    private List<string> keyTypes = new List<string>
    {
        nameof(KeyType.PRIMARYKEY),
        nameof(KeyType.FOREIGNKEY),
    };

    private readonly List<(string tableName, List<TableColumn> tableColumns )> _Tables;
    private readonly string _connectionStringFile;
    private readonly IDatabaseManager  _dbManager;

    public FinalPage(List<(string, List<TableColumn>)> Tables , IDatabaseManager  dbManager,string dataBaseName,string connectionStringFile)
	{
        _dbManager = dbManager;
        _Tables = Tables;
        _dataBaseName = dataBaseName;
        _connectionStringFile = connectionStringFile;

        InitializeComponent();

        BuildConstraintLayout();

    }

    [Obsolete]
    private void BuildConstraintLayout()
	{
        Frame frame = new Frame
        {
            ClassId = $"{frameID}{count}"
        };

		StackLayout constaintStackLayout = new StackLayout
        {
			Orientation = StackOrientation.Horizontal,
			Spacing = 5
		};

        Label keyTypeLabel = new Label
        {
            ClassId = $"{keyTypeLabelID}{count}",
			Text = "Key Type :",
			VerticalOptions = LayoutOptions.CenterAndExpand,
			FontSize = 15,
			TextColor = Colors.Black,
			HorizontalOptions = LayoutOptions.Start,
        };

		Picker keyTypePicker = new Picker
		{
            ClassId = $"{keyTypePickerID}{count}",
            WidthRequest = 150,
			HorizontalOptions = LayoutOptions.Start,
			HorizontalTextAlignment = TextAlignment.Center,
			FontSize = 15,
			TextColor = Colors.Black
		};

        keyTypePicker.ItemsSource = keyTypes;
        keyTypePicker.SelectedIndex = 0;
        keyTypePicker.SelectedIndexChanged += KeyTypePicker_SelectedIndexChanged;

        Picker tablePicker = new Picker
        {
            ClassId = $"{tablePickerID}{count}",
            WidthRequest = 150,
            HorizontalOptions = LayoutOptions.Start,
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = 15,
            TextColor = Colors.Black
        };

        string selectedTableName = _Tables.FirstOrDefault().Item1;

        tablePicker.ItemsSource = _Tables.Select(_ => _.Item1).ToList();
        tablePicker.SelectedItem = selectedTableName;
        tablePicker.SelectedIndexChanged += TablePicker_SelectedIndexChanged;

        Picker columnPicker = new Picker
        {
            ClassId = $"{columnPickerID}{count}",
            WidthRequest = 150,
            HorizontalOptions = LayoutOptions.Start,
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = 15,
            TextColor = Colors.Black
        };

        columnPicker.ItemsSource = _Tables.FirstOrDefault(_ => _.Item1 == selectedTableName).Item2.Select(_=>_.Name).ToList();
        columnPicker.SelectedIndex = 0;

        Label referencesLabel = new Label
        {
            ClassId = $"{referenceLabelID}{count}",
            Text = "References ",
            VerticalOptions = LayoutOptions.CenterAndExpand,
            FontSize = 15,
            TextColor = Colors.Black,
            HorizontalOptions = LayoutOptions.Start,
            IsVisible = false
        };

        Picker referenceTablePicker = new Picker
        {
            ClassId = $"{referenceTablePickerID}{count}",
            WidthRequest = 150,
            HorizontalOptions = LayoutOptions.Start,
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = 15,
            TextColor = Colors.Black,
            IsVisible = false
        };

        referenceTablePicker.ItemsSource = _Tables.Select(_ => _.Item1).ToList();
        referenceTablePicker.SelectedItem = selectedTableName;
        referenceTablePicker.SelectedIndexChanged += ReferenceTablePicker_SelectedIndexChanged;

        Picker referenceColumnPicker = new Picker
        {
            ClassId = $"{referenceColumnPickerID}{count}",
            WidthRequest = 150,
            HorizontalOptions = LayoutOptions.Start,
            HorizontalTextAlignment = TextAlignment.Center,
            FontSize = 15,
            TextColor = Colors.Black,
            IsVisible = false
        };

        referenceColumnPicker.ItemsSource = _Tables.FirstOrDefault(_ => _.Item1 == selectedTableName).Item2.Select(_ => _.Name).ToList();
        referenceColumnPicker.SelectedIndex = 0;

        constaintStackLayout.Children.Add(keyTypeLabel);
        constaintStackLayout.Children.Add(keyTypePicker);
        constaintStackLayout.Children.Add(tablePicker);
        constaintStackLayout.Children.Add(columnPicker);
        constaintStackLayout.Children.Add(referencesLabel);
        constaintStackLayout.Children.Add(referenceTablePicker);
        constaintStackLayout.Children.Add(referenceColumnPicker);

        frame.Content = constaintStackLayout;

        KeySetStackLayout.Children.Add(frame);

        count++;
    }

    private void ReferenceTablePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender is null || sender is not Picker picker || picker.SelectedItem is not string tableName)
            return;

        int count = int.Parse(picker.ClassId.Replace(referenceTablePickerID, string.Empty));

        string referenceColumnPickerClassID = $"{referenceColumnPickerID}{count}";

        Picker referenceColumnPicker = FindPickerById(referenceColumnPickerClassID);

        if (referenceColumnPicker is null)
            return;

        referenceColumnPicker.ItemsSource = _Tables.FirstOrDefault(_ => _.Item1 == tableName).Item2.Select(_ => _.Name).ToList();
        referenceColumnPicker.SelectedIndex = 0;
    }

    private void TablePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender is null || sender is not Picker picker || picker.SelectedItem is not string tableName)
            return;

        int count = int.Parse(picker.ClassId.Replace(tablePickerID, string.Empty));

        string columnPickerClassID = $"{columnPickerID}{count}";

        Picker columnPicker = FindPickerById(columnPickerClassID);

        if (columnPicker is null)
            return;

        columnPicker.ItemsSource = _Tables.FirstOrDefault(_=>_.Item1 == tableName).Item2.Select(_ => _.Name).ToList();
        columnPicker.SelectedIndex = 0;
    }

    private void KeyTypePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if(sender is null || sender is not Picker picker || picker.SelectedItem is not string keyType)
            return;

        int count = int.Parse(picker.ClassId.Replace(keyTypePickerID, string.Empty));

        string referenceLabelClassID = $"{referenceLabelID}{count}";
        string referenceTablePickerClassID = $"{referenceTablePickerID}{count}";
        string referenceColumnPickerClassID = $"{referenceColumnPickerID}{count}";

        Label referenceLabel = FindLabelById(referenceLabelClassID);
        Picker referenceTablePicker = FindPickerById(referenceTablePickerClassID);
        Picker referenceColumnPicker = FindPickerById(referenceColumnPickerClassID);

        if (referenceLabel is null || referenceTablePicker is null || referenceColumnPicker is null)
            return;

        if(keyType == nameof(KeyType.FOREIGNKEY))
        {
            referenceLabel.IsVisible = true;
            referenceTablePicker.IsVisible = true;
            referenceColumnPicker.IsVisible = true;
        }
        else
        {
            referenceLabel.IsVisible = false;
            referenceTablePicker.IsVisible = false;
            referenceColumnPicker.IsVisible = false;
        }
    }

    private Picker FindPickerById(string pickerId)
    {
        foreach (var child in KeySetStackLayout.Children)
        {
            if (child is Frame frame && frame.Content is StackLayout stackLayout)
            {
                foreach (var element in stackLayout.Children)
                {
                    if (element is Picker picker && picker.ClassId == pickerId)
                    {
                        return picker;
                    }
                }
            }
        }

        return null; // Return null if not found
    }

    private Label FindLabelById(string labelId)
    {
        foreach (var child in KeySetStackLayout.Children)
        {
            if (child is Frame frame && frame.Content is StackLayout stackLayout)
            {
                foreach (var element in stackLayout.Children)
                {
                    if (element is Label label && label.ClassId == labelId)
                    {
                        return label;
                    }
                }
            }
        }

        return null; // Return null if not found
    }

    private void addConstrantBtn_Clicked(object sender, EventArgs e)
    {
        BuildConstraintLayout();
    }

    private void removeConstrantBtn_Clicked(object sender, EventArgs e)
    {
        string frameClassID = $"{frameID}{(count -1)}";

        Frame? foundFrame = FindFrameById(frameClassID);

        if(foundFrame is not null)
        {
            KeySetStackLayout.Children.Remove(foundFrame);
            count--;
        }
    }

    private Frame? FindFrameById(string frameId)
    {
        foreach (var child in KeySetStackLayout.Children)
        {
            if (child is Frame frame && frame.ClassId == frameId)
            {
                return frame;
            }
        }

        return null; // Return null if not found
    }

    private async void sendDataBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            await _dbManager.OpenConnection();

            activityIndicator.IsVisible = true;
            List<PrimaryKeyInfo> tablePrimaryKey = new List<PrimaryKeyInfo>();

            foreach (var table in _Tables)
            {
                try
                {
                    await _dbManager.CreateDatabaseTable(table.tableColumns, table.tableName, _dataBaseName, shouldSkipTableExistenceCheck: false);
                }
                catch (TableAlreadyExistsException ex)
                {
                    bool result = await DisplayAlert("Warning", ex.Message, "Yes", "No");

                    if (result)
                    {
                        await _dbManager.CreateDatabaseTable(table.Item2, table.Item1, _dataBaseName, shouldSkipTableExistenceCheck: true);
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                string keyTypePickerClassID = $"{keyTypePickerID}{i}";
                string referenceTablePickerClassID = $"{referenceTablePickerID}{i}";
                string referenceColumnPickerClassID = $"{referenceColumnPickerID}{i}";
                string columnPickerClassID = $"{columnPickerID}{i}";
                string tablePickerClassID = $"{tablePickerID}{i}";

                Picker keyTypePicker = FindPickerById(keyTypePickerClassID);
                Picker columnPicker = FindPickerById(columnPickerClassID);
                Picker tablePicker = FindPickerById(tablePickerClassID);
                Picker referenceTablePicker = FindPickerById(referenceTablePickerClassID);
                Picker referenceColumnPicker = FindPickerById(referenceColumnPickerClassID);

                if (keyTypePicker is null || columnPicker is null || tablePicker is null || referenceTablePicker is null || referenceColumnPicker is null)
                    break;

                string? keyTypeString = keyTypePicker.SelectedItem as string;
                string? columnName = columnPicker.SelectedItem as string;
                string? tableName = tablePicker.SelectedItem as string;

                if (keyTypeString == nameof(KeyType.FOREIGNKEY))
                {
                    string? refrenceTableName = referenceTablePicker.SelectedItem as string;
                    string? referenceColumnName = referenceColumnPicker.SelectedItem as string;

                    string query = $"ALTER TABLE {tableName} " +
                           $"ADD CONSTRAINT fk_constraint_{tableName}_{refrenceTableName} " +
                           $"FOREIGN KEY({columnName}) " +
                           $"REFERENCES {refrenceTableName}({referenceColumnName}); ";

                    await _dbManager.ExecuteDatabaseQuery(query);
                }
                else
                {
                    tablePrimaryKey.Add(new PrimaryKeyInfo { ColumnName = columnName, TableName = tableName });
                }
            }

            for (int n = 0; n < _Tables.Count; n++)
            {
                string TableName = _Tables[n].Item1;

                int count = tablePrimaryKey.Where(_ => _.TableName == TableName).Count();

                if (count == 0)
                    continue;

                string query = $"ALTER TABLE {TableName} ADD CONSTRAINT PK_C{count}_{TableName} PRIMARY KEY (";

                if (count > 1)
                {

                    List<PrimaryKeyInfo> duplicateTables = tablePrimaryKey.Where(_ => _.TableName == TableName).ToList();

                    for (int i = 0; i < duplicateTables.Count; i++)
                    {
                        query += $"{duplicateTables[i].ColumnName}{((i != duplicateTables.Count - 1) ? "," : "")}";
                    }

                    query += ")";
                }
                else
                {
                    PrimaryKeyInfo? foundTablePrimaryKey = tablePrimaryKey.FirstOrDefault(_ => _.TableName == TableName);

                    if (foundTablePrimaryKey is not null)
                    {
                        query += $"{foundTablePrimaryKey.ColumnName});";
                    }
                }

                await _dbManager.ExecuteDatabaseQuery(query);
            }

            await LoadAllDataFromExcelFileIntoDatabase();

            DisplayAlert("Success", "Data has been sended successfully", "ok");
        }
        catch(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Ok");
        }
        finally
        {
            await _dbManager.CloseConnection();

            activityIndicator.IsVisible = false;
        }
    }

    private async Task LoadAllDataFromExcelFileIntoDatabase()
    {
        using (OleDbConnection connection = new OleDbConnection(_connectionStringFile))
        {
            connection.Open();

            // Get the names of the sheets in the Excel file
            var sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if (sheets == null)
            {
                throw new NoSheetsFoundException();
            }

            int i = 0;
            foreach (DataRow sheet in sheets.Rows)
            {
                string sheetName = sheet["TABLE_NAME"].ToString();

                // Select all data from the sheet
                var query = $"SELECT * FROM [{sheetName}]";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                {
                    string tableName = _Tables[i].tableName;
                    List<TableColumn> tableColumns = _Tables[i].tableColumns;

                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Assuming the first row contains headers
                    if (dataTable.Rows.Count > 0)
                    {
                        await _dbManager.InsertDataIntoDatabase(dataTable, tableColumns, tableName);
                    }
                    else
                    {
                        throw new NoDataFoundException(sheetName);
                    }
                }
                i++;
            }
        }
    }

    class PrimaryKeyInfo
    {
        public string? TableName { get; set; }
        public string? ColumnName { get; set; }
    }
}