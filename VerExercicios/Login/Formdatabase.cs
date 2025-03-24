using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login
{
    public partial class Formdatabase : Form
    {
        MySqlConnection conn;
        MySqlCommand cmd;
        MySqlDataReader adapter;

        public Formdatabase()
        {
            InitializeComponent();
        }
        private void database_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            button1.BackgroundImage = BDS.Properties.Resources.run;
            button1.BackgroundImageLayout = ImageLayout.Stretch;
            button1.FlatAppearance.MouseOverBackColor = Color.Transparent;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            string connectionString = ConfigurationManager.ConnectionStrings["minhaConnectionApp"].ToString()
            .Replace("hostApp", DatabaseConnector.getDataBase("host"))
            .Replace("appPort", DatabaseConnector.getDataBase("host_port"))
            .Replace("userApp", DatabaseConnector.getDataBase("host_user"))
            .Replace("passApp", DatabaseConnector.getDataBase("host_pass"));
            if (string.IsNullOrEmpty(DatabaseConnector.getDataBase("host")))
            {
                MessageBox.Show("Não existe nenhuma ligação configurada!", "Database Connection Failed");
                return;
            }
            if (string.IsNullOrEmpty(DatabaseConnector.getDataBase("host_port")))
            {
                connectionString = connectionString.Replace("appPort", "3306");
            }
            try
            {
                using (conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "SHOW DATABASES;";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable databases = new DataTable();
                            adapter.Fill(databases);

                            listBox1.Items.Clear(); // Clear the list box before adding items

                            foreach (DataRow row in databases.Rows)
                            {
                                string databaseName = row[0].ToString();
                                listBox1.Items.Add($"{databaseName}");
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedDatabase = listBox1.SelectedItem.ToString().Trim();
            if (string.IsNullOrEmpty(selectedDatabase))
            {
                Console.WriteLine("No database selected.");
                return;
            }
            string selectedTable = comboBox1.SelectedItem.ToString().Trim();
            if (string.IsNullOrEmpty(selectedTable))
            {
                Console.WriteLine("No table selected.");
                return;
            }
            string connectionString = ConfigurationManager.ConnectionStrings["minhaConnectionApp"].ToString()
            .Replace("hostApp", DatabaseConnector.getDataBase("host"))
            .Replace("appPort", DatabaseConnector.getDataBase("host_port"))
            .Replace("userApp", DatabaseConnector.getDataBase("host_user"))
            .Replace("passApp", DatabaseConnector.getDataBase("host_pass"))
            + $";Database={selectedDatabase};";
            try
            {
                using (conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string selectQuery = $"SELECT * FROM {selectedTable};";
                        using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, conn))
                        using (MySqlDataAdapter selectAdapter = new MySqlDataAdapter(selectCmd))
                        {
                            DataTable dataTable = new DataTable();
                            selectAdapter.Fill(dataTable);

                            // Bind the DataTable to DataGridView
                            dataGridView1.DataSource = dataTable;
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDatabase = listBox1.SelectedItem.ToString().Trim();
            if (string.IsNullOrEmpty(selectedDatabase))
            {
                Console.WriteLine("No database selected.");
                return;
            }
            listBox2.Items.Clear();
            comboBox1.Items.Clear();
            string connectionString = ConfigurationManager.ConnectionStrings["minhaConnectionApp"].ToString()
            .Replace("hostApp", DatabaseConnector.getDataBase("host"))
            .Replace("appPort", DatabaseConnector.getDataBase("host_port"))
            .Replace("userApp", DatabaseConnector.getDataBase("host_user"))
            .Replace("passApp", DatabaseConnector.getDataBase("host_pass"))
            + $";Database={selectedDatabase};";
            try
            {
                using (conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "SHOW TABLES;";
                        using (cmd = new MySqlCommand(query, conn))
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable tables = new DataTable();
                            tables.Load(reader);

                            foreach (DataRow row in tables.Rows)
                            {
                                string tableName = row[0].ToString();
                                listBox2.Items.Add($"----------------------------------------------");
                                listBox2.Items.Add($"Table: {tableName}");
                                comboBox1.Items.Add(tableName);
                                listBox2.Items.Add($"----------------------------------------------");
                                // Get columns for each table
                                string columnQuery = $"DESCRIBE {tableName};";
                                using (MySqlCommand colCmd = new MySqlCommand(columnQuery, conn))
                                using (MySqlDataAdapter colAdapter = new MySqlDataAdapter(colCmd))
                                {
                                    DataTable columns = new DataTable();
                                    colAdapter.Fill(columns);

                                    foreach (DataRow colRow in columns.Rows)
                                    {
                                        listBox2.Items.Add($" - {colRow["Field"]} ({colRow["Type"]})");
                                    }
                                }
                                // Get primary key for each table
                                string primaryKeyQuery = $@"
                SELECT COLUMN_NAME
                FROM information_schema.KEY_COLUMN_USAGE
                WHERE TABLE_NAME = '{tableName}' AND CONSTRAINT_NAME = 'PRIMARY';";

                                using (MySqlCommand pkCmd = new MySqlCommand(primaryKeyQuery, conn))
                                using (MySqlDataAdapter pkAdapter = new MySqlDataAdapter(pkCmd))
                                {
                                    DataTable primaryKeyColumns = new DataTable();
                                    pkAdapter.Fill(primaryKeyColumns);

                                    if (primaryKeyColumns.Rows.Count > 0)
                                    {
                                        listBox2.Items.Add("   Primary Key:");
                                        foreach (DataRow pkRow in primaryKeyColumns.Rows)
                                        {
                                            listBox2.Items.Add($"    - {pkRow["COLUMN_NAME"]}");
                                        }
                                    }
                                }

                                // Get foreign keys for each table
                                string foreignKeyQuery = $@"
                SELECT 
                    kcu.COLUMN_NAME AS 'Column Name', 
                    kcu.REFERENCED_TABLE_NAME AS 'Referenced Table', 
                    kcu.REFERENCED_COLUMN_NAME AS 'Referenced Column'
                FROM 
                    information_schema.KEY_COLUMN_USAGE kcu
                JOIN 
                    information_schema.REFERENTIAL_CONSTRAINTS rc 
                    ON kcu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME
                WHERE 
                    kcu.TABLE_NAME = '{tableName}' AND 
                    kcu.REFERENCED_TABLE_NAME IS NOT NULL;";

                                using (MySqlCommand fkCmd = new MySqlCommand(foreignKeyQuery, conn))
                                using (MySqlDataAdapter fkAdapter = new MySqlDataAdapter(fkCmd))
                                {
                                    DataTable foreignKeys = new DataTable();
                                    fkAdapter.Fill(foreignKeys);

                                    if (foreignKeys.Rows.Count > 0)
                                    {
                                        listBox2.Items.Add("   Foreign Keys:");
                                        foreach (DataRow fkRow in foreignKeys.Rows)
                                        {
                                            string columnName = fkRow["Column Name"].ToString();
                                            string referencedTable = fkRow["Referenced Table"].ToString();
                                            string referencedColumn = fkRow["Referenced Column"].ToString();
                                            listBox2.Items.Add($"    - {columnName} -> {referencedTable}({referencedColumn})");
                                        }
                                    }
                                }

                            }
                        }
                        conn.Close();
                        comboBox1.SelectedIndex = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
