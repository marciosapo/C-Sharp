using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Login
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private static string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
        private static string dbPath = Path.Combine(dbFolder, "database.db");
        private static string connectionString = $"Data Source={dbPath};Version=3;";

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Open the connection
                    Console.WriteLine("Connection successful!");

                    // SQL query with parameterized username to prevent SQL injection
                    string query = "SELECT * FROM login_logs";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        dataGridView1.Rows.Clear(); 
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            // Check if we have any rows (i.e., the username exists)
                            while (reader.Read())
                            {
                                // Retrieve the stored password (hashed)
                                DateTime data_login = (DateTime)reader["data_login"];
                                string hora_login = reader["hora_login"].ToString().Trim();  // Trim to remove spaces
                                string tipo_login = reader["login_tipo"].ToString().Trim();  // Trim to remove spaces
                                string formattedDate = data_login.ToString("yyyy-MM-dd");
                                dataGridView1.Rows.Add(formattedDate, hora_login, tipo_login);


                            }
                            reader.Close();
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedValue = comboBox1.SelectedItem.ToString();
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            string dbPath = Path.Combine(dbFolder, "database.db");
            string connectionString = $"Data Source={dbPath};Version=3;";
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Open the connection
                    Console.WriteLine("Connection successful!");

                    // SQL query with parameterized username to prevent SQL injection
                    string query;
                    if(selectedValue != "Todos")
                    {
                        query = "SELECT * FROM login_logs WHERE login_tipo = @tipo";
                    }else
                    {
                        query = "SELECT * FROM login_logs";
                    }

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        dataGridView1.Rows.Clear();
                        cmd.Parameters.AddWithValue("@tipo", selectedValue);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            // Check if we have any rows (i.e., the username exists)
                            while (reader.Read())
                            {
                                // Retrieve the stored password (hashed)
                                DateTime data_login = (DateTime)reader["data_login"];
                                string hora_login = reader["hora_login"].ToString().Trim();  // Trim to remove spaces
                                string tipo_login = reader["login_tipo"].ToString().Trim();  // Trim to remove spaces
                                string formattedDate = data_login.ToString("yyyy-MM-dd");
                                dataGridView1.Rows.Add(formattedDate, hora_login, tipo_login);


                            }
                            reader.Close();
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
    }
}
