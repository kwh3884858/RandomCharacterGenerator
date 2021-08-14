using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Shapes;

namespace RandomCharacterGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class CommonTable
        {
            public CommonTable()
            {
                m_rows = new List<CommonRow>();
            }
            public List< CommonRow> m_rows;
        }
        public class CommonRow
        {
            public string Name { get; set; }
        }

        public class CommonRowMap : ClassMap<CommonRow>
        {
            public CommonRowMap()
            {
                Map(m => m.Name).Index(0).Name("name");
            }
        }

        private void ReadCSVFile(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CommonRowMap>();
                var records = csv.GetRecords<CommonRow>();
                m_currentTable = new CommonTable();
                foreach (var record in records)
                {
                    m_currentTable.m_rows.Add(new CommonRow() { Name = record.Name });
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            m_tableDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            m_tableDirectory += m_tableDirectoryName;
            if (!Directory.Exists(m_tableDirectory))
            {
                Directory.CreateDirectory(m_tableDirectory);
            }
            Window_Loaded();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            //Perform actions when SelectedItem changes
            TreeViewItem item = (TreeViewItem) e.NewValue;
            string selectedName = item.Header.ToString();
            if (selectedName.Contains(".csv"))
            {
                selectedName = selectedName.Substring(selectedName.LastIndexOf("\\") + 1);
                selectedName = selectedName.Substring(0, selectedName.LastIndexOf("."));
                MessageBox.Show(selectedName);
                ReadCSVFile(item.Tag.ToString());
                FillfullTable();
            }
        }

        private void NewTable(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Table file (*.csv)|*.csv";
            saveFileDialog.InitialDirectory = m_tableDirectory;
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, null);
        }

        private void AddNewRow(object sender, RoutedEventArgs e)
        {
            m_currentTable.m_rows.Add(new CommonRow());
            FillfullTable();
        }

        private void Window_Loaded()
        {
            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            try
            {
                foreach (string s in Directory.GetDirectories(path))
                {
                    TreeViewItem dummyNode = new TreeViewItem();
                    TreeViewItem item = new TreeViewItem();
                    item.Header = "[Folder] " + s.Substring(s.LastIndexOf("\\") + 1);
                    item.Tag = s;
                    item.FontWeight = FontWeights.Normal;
                    item.Items.Add(dummyNode);
                    item.Expanded += new RoutedEventHandler(folder_Expanded);
                    foldersItem.Items.Add(item);
                }

                foreach (string s in Directory.GetFiles(path))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = s.Substring(s.LastIndexOf("\\") + 1);
                    item.Tag = s;
                    item.FontWeight = FontWeights.Normal;
                    foldersItem.Items.Add(item);
                }
            }
            catch (Exception) { }
        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1)
            {
                item.Items.Clear();
                foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                {
                    TreeViewItem dummyNode = new TreeViewItem();
                    TreeViewItem subitem = new TreeViewItem();
                    subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                    subitem.Tag = s;
                    subitem.FontWeight = FontWeights.Normal;
                    subitem.Items.Add(dummyNode);
                    subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                    item.Items.Add(subitem);
                }

                foreach (string s in Directory.GetFiles(item.Tag.ToString()))
                {
                    TreeViewItem newItem = new TreeViewItem();
                    newItem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                    newItem.Tag = s;
                    newItem.FontWeight = FontWeights.Normal;
                    item.Items.Add(newItem);
                }
            }
        }

        void FillfullTable()
        {
            TableDocument.Blocks.Clear();

            Table table1 = new Table();
            TableDocument.Blocks.Add(table1);

            // Set some global formatting properties for the table.
            table1.CellSpacing = 10;
            table1.Background = Brushes.White;

            // Create and add an empty TableRowGroup to hold the table's Rows.
            TableRowGroup group = new TableRowGroup();
            table1.RowGroups.Add(group);

            // Create 6 columns and add them to the table's Columns collection.
            int index = 0;
            if(m_currentTable == null) return;
            foreach (var row in m_currentTable.m_rows)
            {
                TableRow currentRow = new TableRow();
                group.Rows.Add(currentRow);

                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(index.ToString()))));
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(row.Name))));
                index++;
            }
        }

        private string m_tableDirectoryName = "\\Tables";
        private string m_tableDirectory;
        private CommonTable m_currentTable = null;
    }
}
