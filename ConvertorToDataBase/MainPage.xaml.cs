using ConvertorToDataBase.Enums;
using ConvertorToDataBase.Exceptions;
using ConvertorToDataBase.Interfaces;
using ConvertorToDataBase.Models;
using ConvertorToDataBase.Modules;
using ConvertorToDataBase.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using MySql.Data.MySqlClient;
using Npgsql;
using Npgsql.PostgresTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZstdSharp;

namespace ConvertorToDataBase
{
    public partial class MainPage : ContentPage
    {
        const string entryTableID = "EntryTable_";
        const string pickerDataTypeID = "PickerDataType_";
        const string pickerKeySetID = "PickerKeySet_";
        const string entryDateFormatID = "EntryDateFormat_";
        const string entryDefaultValueID = "EntryDefaultValue_";
        const string entryColNameID = "EntryColName_";
        const string entryDataTypeLengthID = "EntryDataTypeLength_";

        private string connectionStringFile;
        private DataBaseType dataBaseType = DataBaseType.MYSQL;
        private string databaseName;
        private IDatabaseManager  _dbManager;
        List<string> dataTypes;
        List<(string sheetName,List<FileColumn> fileColumns)> Tables;
        List<ColumnOptionsViewModel> columnOptions = new List<ColumnOptionsViewModel>
        {
            new ColumnOptionsViewModel{ ColumnOption = ColumnOption.NONE , Name =  nameof(ColumnOption.NONE)},
            new ColumnOptionsViewModel{ ColumnOption = ColumnOption.DEFAULT , Name =  nameof(ColumnOption.DEFAULT)}, 
            new ColumnOptionsViewModel{ ColumnOption = ColumnOption.UNIQUE , Name =  nameof(ColumnOption.UNIQUE)}
        };


        public MainPage()
        {
            InitializeComponent();
        }

        [Obsolete]
        public void BuildTableLayout(string tableName , List<FileColumn> columns)
        {
            Frame frame = new Frame 
            { 
                HasShadow = true,
                CornerRadius = 5,
                BackgroundColor = Colors.Transparent,
                Padding = 5
            };

            StackLayout TableStackLayout = new StackLayout();

            StackLayout stackLayoutTableName = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Padding = 5,
                Spacing = 5
            };

            Label labelTable = new Label
            {
                Text = "Tabel Name: ",
                TextColor = Colors.Black,
                FontSize = 15,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Start
            };

            Entry entryTable = new Entry
            {
                ClassId = $"{entryTableID}{tableName}",
                Text = tableName,
                PlaceholderColor = Colors.DimGray,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = 150,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center
            };

            stackLayoutTableName.Children.Add(labelTable);
            stackLayoutTableName.Children.Add(entryTable);

            TableStackLayout.Children.Add(stackLayoutTableName);

            for (int i=0; i < columns.Count; i++)
            {
                string colName = columns[i].Name;

                StackLayout stackLayoutItem = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Padding = 5,
                    Spacing = 5
                };

                Label labelName = new Label
                {
                    Text = "Column :",
                    TextColor = Colors.Black,
                    FontSize = 15,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };

                Entry entry_ColumnName = new Entry
                {
                    ClassId = $"{entryColNameID}{tableName}{colName}",
                    Text = colName,
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 150,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalTextAlignment = TextAlignment.Center,
                };

                Picker picker_DataType = new Picker
                {
                    ClassId = $"{pickerDataTypeID}{tableName}{colName}",
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 200,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 15
                };

                picker_DataType.ItemsSource = dataTypes;

                var foundDataType = dataTypes.FirstOrDefault(_ => _ == columns[i].SuggestDataType.ToUpper());

                if (foundDataType is null)
                {
                    foundDataType = dataTypes.FirstOrDefault(_ => _ == nameof(MysqlDataType.TEXT));
                }

                picker_DataType.SelectedItem = foundDataType;
                picker_DataType.SelectedIndexChanged += Picker_DataType_SelectedIndexChanged;

                bool isVariableLengthBinaryDataType = DataTypeHelper.IsVariableLengthBinaryDataType(foundDataType);

                Entry entry_DataTypeLength = new Entry
                {
                    ClassId = $"{entryDataTypeLengthID}{tableName}{colName}",
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 70,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalTextAlignment = TextAlignment.Center,
                    IsVisible = isVariableLengthBinaryDataType,
                    Text = isVariableLengthBinaryDataType ? "50" : string.Empty,
                    Placeholder = "Length",
                };

                bool isDatabaseDateTimeType = (DataTypeHelper.IsDatabaseDateTimeType(foundDataType));

                Entry entry_DateFormat = new Entry
                {
                    ClassId = $"{entryDateFormatID}{tableName}{colName}",
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 150,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalTextAlignment = TextAlignment.Center,
                    IsVisible = isDatabaseDateTimeType,
                    Text = isDatabaseDateTimeType ? "dd/MM/yyyy" : string.Empty,
                    Placeholder = "Format",
                };

                Picker picker_KeySet = new Picker
                {

                    ClassId = $"{pickerKeySetID}{tableName}{colName}",
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 200,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 15
                };

                picker_KeySet.ItemDisplayBinding = new Binding("Name");
                picker_KeySet.ItemsSource = columnOptions;
                picker_KeySet.SelectedIndex = 0;
                picker_KeySet.SelectedIndexChanged += Picker_KeySet_SelectedIndexChanged;

                Entry entry_DefaultValue = new Entry
                {
                    ClassId = $"{entryDefaultValueID}{tableName}{colName}",
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 70,
                    VerticalOptions =  LayoutOptions.CenterAndExpand,
                    HorizontalTextAlignment = TextAlignment.Center,
                    IsVisible = false,
                    Placeholder = "Value"
                };

                stackLayoutItem.Children.Add(labelName);
                stackLayoutItem.Children.Add(entry_ColumnName);
                stackLayoutItem.Children.Add(picker_DataType);
                stackLayoutItem.Children.Add(entry_DataTypeLength);
                stackLayoutItem.Children.Add(entry_DateFormat);
                stackLayoutItem.Children.Add(picker_KeySet);
                stackLayoutItem.Children.Add(entry_DefaultValue);

                TableStackLayout.Children.Add(stackLayoutItem);
            }

            frame.Content = TableStackLayout;

            MainStackLayout.Children.Add(frame);
        }

        private void Picker_KeySet_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sender is null || sender is not Picker picker || picker.SelectedItem is not ColumnOptionsViewModel columnOptions)
                return;

            string? dynamicID = picker.ClassId.Replace($"{pickerKeySetID}", "");

            string classID = $"{entryDefaultValueID}{dynamicID}";

            Entry? foundEntryDataTypeLength = findEntryByClassID(classID);

            if (foundEntryDataTypeLength is null)
                return;

            foundEntryDataTypeLength.Text = string.Empty;

            if (columnOptions.ColumnOption == ColumnOption.DEFAULT)
            {
                foundEntryDataTypeLength.IsVisible = true;
            }
            else
            {
                foundEntryDataTypeLength.IsVisible = false;
            }
        }

        private void Picker_DataType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (sender is null || sender is not Picker picker || picker.SelectedItem is not string dataType)
                return;

            string? dynamicID = picker.ClassId.Replace($"{pickerDataTypeID}", "");

            string dataTypeLength_ClassID = $"{entryDataTypeLengthID}{dynamicID}";
            string dateFormat_ClassID = $"{entryDateFormatID}{dynamicID}";

            Entry? foundEntryDataTypeLength = findEntryByClassID(dataTypeLength_ClassID);
            Entry? foundEntryDateFormat = findEntryByClassID(dateFormat_ClassID);

            if (foundEntryDataTypeLength is null || foundEntryDateFormat is null)
                return;

            foundEntryDateFormat.Text = string.Empty;
            foundEntryDataTypeLength.Text = string.Empty;

            if (DataTypeHelper.IsDatabaseDateTimeType(dataType))
            {
                foundEntryDateFormat.IsVisible = true;
            }
            else
            {
                foundEntryDateFormat.IsVisible = false;
            }

            if (DataTypeHelper.IsVariableLengthBinaryDataType(dataType))
            {
                foundEntryDataTypeLength.IsVisible = true;
            }
            else
            {
                foundEntryDataTypeLength.IsVisible = false;
            }
        }

        private Entry? findEntryByClassID(string classID)
        {
            foreach (var child in MainStackLayout.Children)
            {
                if (child is not Frame frame || frame.Content is not StackLayout tableStackLayout)
                    return null;

                foreach (var tableChild in tableStackLayout.Children)
                {
                    if (tableChild is StackLayout stackLayout)
                    {
                        foreach (var stackChild in stackLayout.Children)
                        {
                            if (stackChild is Entry entry && entry.ClassId == classID)
                            {
                                return entry;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private Picker? findPickerByClassID(string classID)
        {
            foreach (var child in MainStackLayout.Children)
            {
                if (child is not Frame frame || frame.Content is not StackLayout tableStackLayout)
                    return null;

                foreach (var tableChild in tableStackLayout.Children)
                {
                    if (tableChild is StackLayout stackLayout)
                    {
                        foreach (var stackChild in stackLayout.Children)
                        {
                            if (stackChild is Picker picker && picker.ClassId == classID)
                            {
                                return picker;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                activityIndicator.IsVisible = true;

                // If 'connectionsStackLayout.IsVisible == true', it indicates that the database connection is not configured yet
                if (connectionsStackLayout.IsVisible)
                {
                    connectionsStackLayout.IsVisible = false;

                    await EstablishDatabaseConnection();

                    Tables = ExtractTablesFromExcel();

                    for (int i = 0; i < Tables.Count; i++)
                        BuildTableLayout(Tables[i].Item1, Tables[i].Item2);
                }
                else
                {
                    await extractTableDataFromControlsAndNavigate();
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");

                connectionsStackLayout.IsVisible = true;
                MainStackLayout.Children.Clear();
            }
            finally
            {
                activityIndicator.IsVisible = false;
            }
        }

        private async Task EstablishDatabaseConnection()
        {
            databaseName = DatabaseEntry.Text;
            string userName = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string port = PortEntry.Text;
            string hostName = HostNameEntry.Text;

            dataTypes = (dataBaseType == DataBaseType.MYSQL) ? Enum.GetNames(typeof(MysqlDataType)).ToList() : (dataBaseType == DataBaseType.POSTGRESQL) ? 
                Enum.GetNames(typeof(NpgsqlDataType)).ToList() : Enum.GetNames(typeof(SqlServerDataType)).ToList();

            string connString = (dataBaseType == DataBaseType.MYSQL) ? $"server={hostName};uid={userName};password={password};port={port};database={databaseName}" : 
                $"Host={hostName};Port={port};Username={userName};Password={password};Database={databaseName};";

            _dbManager = (dataBaseType == DataBaseType.SQLSERVER) ? new SQLServerDataBaseManager(new SqlConnection(connString)) :
                        (dataBaseType == DataBaseType.MYSQL) ? new MysqlDataBaseManager(new MySqlConnection(connString)) :
                        new PostgresDataBaseManager(new NpgsqlConnection(connString));

            bool canConnect = await _dbManager.TestConnection();

            if (!canConnect)
                throw new DatabaseConnectionException(databaseName);
        }

        private List<(string, List<FileColumn>)> ExtractTablesFromExcel()
        {
            List<(string, List<FileColumn>)> tables = new List<(string, List<FileColumn>)>();

            // Provide the path to Excel file (.xlsx)
            string filePath = FileNamePathEntry.Text;

            // Connection string for Excel file using OleDb
            connectionStringFile = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties=\"Excel 12.0 Xml;HDR=Yes;\"";

            using (OleDbConnection connection = new OleDbConnection(connectionStringFile))
            {
                connection.Open();

                // Get the names of the sheets in the Excel file
                var sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (sheets == null)
                {
                    throw new NoSheetsFoundException();
                }

                List<FileColumn> columns = new List<FileColumn>();
                foreach (DataRow sheet in sheets.Rows)
                {
                    string? sheetName = sheet["TABLE_NAME"].ToString();
                   
                    // Select all data from the sheet
                    var query = $"SELECT * FROM [{sheetName}]";
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Assuming the first row contains headers
                        if (dataTable.Rows.Count > 0)
                        {
                            foreach (DataColumn column in dataTable.Columns)
                            {
                                columns.Add(new FileColumn { Name = column.ColumnName  , SuggestDataType = DataTypeMapper.MapToDataBaseDataType(column.DataType,dataBaseType)});
                            }
                        }
                        else
                        {
                            throw new NoDataFoundException(sheetName);
                        }
                    }

                    tables.Add(new(sheetName, columns));
                }

                return tables;
            }
        }

        private async Task extractTableDataFromControlsAndNavigate()
        {
            // List to store extracted table information
            List<(string TableName, List<TableColumn> Columns)> tables = new List<(string, List<TableColumn>)>();

            // Loop through each table in the collection
            for (int i = 0; i < this.Tables.Count; i++)
            {
                // List to store columns of the current table
                List<TableColumn> tableColumns = new List<TableColumn>();

                // Extract sheet name and create class ID for the table entry
                string sheetName = this.Tables[i].sheetName;
                string tableClassID = $"{entryTableID}{sheetName}";

                // Find the entry control for the table name
                Entry? foundEntryTable = findEntryByClassID(tableClassID);

                // If the entry control is not found, exit the loop
                if (foundEntryTable is null)
                    break;

                // Extract the table name from the found entry control
                string tableName = foundEntryTable.Text;

                // Loop through each column in the current table
                foreach (var colName in this.Tables[i].fileColumns.Select(_ => _.Name))
                {
                    // Generate dynamic IDs for various controls related to the current column
                    string dynamicID = $"{sheetName}{colName}";

                    string colEntryClassID = $"{entryColNameID}{dynamicID}";
                    string pickerDataTypeClassID = $"{pickerDataTypeID}{dynamicID}";
                    string entryDataTypeLengthClassID = $"{entryDataTypeLengthID}{dynamicID}";
                    string entryDateFormatClassID = $"{entryDateFormatID}{dynamicID}";
                    string pickerKeySetClassID = $"{pickerKeySetID}{dynamicID}";
                    string entryDefaultValueClassID = $"{entryDefaultValueID}{dynamicID}";

                    // Find controls related to the current column
                    Entry? foundEntryColName = findEntryByClassID(colEntryClassID);
                    Picker? foundPickerDataType = findPickerByClassID(pickerDataTypeClassID);
                    Entry? foundDataTypeLength = findEntryByClassID(entryDataTypeLengthClassID);
                    Entry? foundEntryDateFormat = findEntryByClassID(entryDateFormatClassID);
                    Picker? foundPickerKeySet = findPickerByClassID(pickerKeySetClassID);
                    Entry? foundEntryDefaultValue = findEntryByClassID(entryDefaultValueClassID);

                    // If any of the controls related to the current column is not found, exit the loop
                    if (foundEntryColName is null || foundPickerDataType is null || foundDataTypeLength is null || foundEntryDateFormat is null ||
                        foundPickerKeySet is null || foundEntryDefaultValue is null)
                    {
                        break;
                    }

                    // Extract information from controls and create a TableColumn
                    string NewColName = foundEntryColName.Text;
                    string? selectedDateType = foundPickerDataType.SelectedItem as string;
                    string defaultValue = foundEntryDefaultValue.Text;
                    string dataType = selectedDateType;
                    ColumnOption option = (foundPickerKeySet.SelectedItem as ColumnOptionsViewModel).ColumnOption;
                    string dateformat = foundEntryDateFormat.Text;
                    int length = int.TryParse(foundDataTypeLength.Text, out _) ? int.Parse(foundDataTypeLength.Text) : 0;

                    // Add the TableColumn to the list of columns for the current table
                    tableColumns.Add(new TableColumn { ColumnOption = option, DataType = dataType, DateFormat = dateformat, Length = length, Name = NewColName, DefaultValue = defaultValue });
                }

                // Add the table information (table name and columns) to the list of tables
                tables.Add((tableName, tableColumns));
            }

            // Navigate to the FinalPage with the extracted table information
            await Navigation.PushAsync(new FinalPage(tables, _dbManager, databaseName, connectionStringFile));
        }

        private async void SelectFileBtn_Clicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync();

            if (result != null)
            {
                if (Path.GetExtension(result.FileName)?.ToLower() == ".xlsx")
                {
                    FileNamePathEntry.Text = result.FullPath;
                }
                else
                {
                    await DisplayAlert("Invalid File", "Please select a valid .xlsx file.", "OK");
                }
            }
        }

        private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                var selectedRadioButton = (RadioButton)sender;
                string selectedOption = selectedRadioButton.Content.ToString().ToUpper();

                dataBaseType = (selectedOption == nameof(DataBaseType.MYSQL)) ? DataBaseType.MYSQL : (selectedOption == nameof(DataBaseType.POSTGRESQL)) ? DataBaseType.POSTGRESQL : DataBaseType.SQLSERVER;
            }
        }
    }
}
