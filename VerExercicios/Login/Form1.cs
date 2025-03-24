using System;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.Data.SQLite;
using System.IO;
using System.Drawing;

namespace Login
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }
            string dbPath = Path.Combine(dbFolder, "database.db");
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                Console.WriteLine("Database file created successfully!");
                string connectionString = $"Data Source={dbPath};Version=3;";
                try
                {
                    using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                    {
                        try
                        {
                            conn.Open();
                            MessageBox.Show("Base de dados não encontrada. A Criar...");
                            string sqlFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql");
                            string sqlPath = Path.Combine(sqlFolder, "login.sql");
                            if (ExecuteSqlScript(conn, sqlPath))
                            {
                                MessageBox.Show("Base de dados criada com sucesso!");
                            }
                            else
                            {
                                MessageBox.Show("Falhou a criar a base de dados!");
                            }
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);

                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Permission error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("File error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("General error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static readonly float Version = 0.1f;
        public string currentUser;

        public void clearTextBoxes()
        {
            textBox1.Text = "";
            txt_pass.Text = "";
        }

        private string MD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);  // Convert input to byte array
                byte[] hashBytes = md5.ComputeHash(inputBytes);      // Compute the hash

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));  // Convert each byte to its hexadecimal representation
                }

                return sb.ToString();  // Return the hashed string
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(txt_pass.Text))
            {
                MessageBox.Show("Username ou password não podem estar vazias!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string Username = textBox1.Text.Trim();  // Trim to remove any leading/trailing spaces
            string Password = txt_pass.Text.Trim();
            string hashedPassword = MD5Hash(Password);
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            string dbPath = Path.Combine(dbFolder, "database.db");
            string connectionString = $"Data Source={dbPath};Version=3;";

            // Establish connection
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Open connection only once

                    string query = "SELECT userpass FROM login WHERE username = @username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", Username);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword = reader["userpass"].ToString().Trim();
                                storedPassword = MD5Hash(storedPassword).ToLower();

                                if (storedPassword != hashedPassword.ToLower())
                                {
                                    MessageBox.Show("Password incorreta!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    LogLoginAttempt(conn, "Password incorrecta");
                                    return;
                                }
                                else
                                {
                                    MessageBox.Show("Login bem-sucedido!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    currentUser = Username;
                                    UpdateLastLogin(conn, Username);

                                    Form4 form4 = new Form4(currentUser, this);
                                    form4.Show();
                                    this.Hide();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Username não encontrado!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                LogLoginAttempt(conn, "Failed");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static Boolean ExecuteSqlScript(SQLiteConnection conn, string scriptPath)
        {
            Boolean pass = false;
            string script = File.ReadAllText(scriptPath);
            using (SQLiteCommand cmd = new SQLiteCommand(script, conn))
            {
                cmd.ExecuteNonQuery();
                pass = true;
            }
            return pass;
        }
        private void LogLoginAttempt(SQLiteConnection conn, string status)
        {
            string queryInsert = "INSERT INTO login_logs (data_login, hora_login, login_tipo) VALUES (@data_login, @hora_login, @login_tipo)";
            using (SQLiteCommand cmdInsert = new SQLiteCommand(queryInsert, conn))
            {
                cmdInsert.Parameters.AddWithValue("@data_login", DateTime.Now.ToString("yyyy-MM-dd"));
                cmdInsert.Parameters.AddWithValue("@hora_login", DateTime.Now.ToString("HH:mm:ss"));
                cmdInsert.Parameters.AddWithValue("@login_tipo", status);
                cmdInsert.ExecuteNonQuery();
            }
        }

        private void UpdateLastLogin(SQLiteConnection conn, string username)
        {
            string queryUpdate = "UPDATE login SET lastlogin = @lastlogin WHERE username = @username";
            using (SQLiteCommand cmdUpdate = new SQLiteCommand(queryUpdate, conn))
            {
                cmdUpdate.Parameters.AddWithValue("@lastlogin", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmdUpdate.Parameters.AddWithValue("@username", username);
                cmdUpdate.ExecuteNonQuery();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label3.Text = "Versão: " + Version.ToString();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            picpass.SizeMode = PictureBoxSizeMode.StretchImage;
            picpass.Image = BDS.Properties.Resources.eye;
            button3.BackgroundImage = BDS.Properties.Resources.log;
            button3.BackgroundImageLayout = ImageLayout.Stretch;
            button3.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button3.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button4.BackgroundImage = BDS.Properties.Resources.shutdown;
            button4.BackgroundImageLayout = ImageLayout.Stretch;
            button4.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button4.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button1.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button2.FlatAppearance.MouseOverBackColor = Color.Transparent;
            txt_pass.PasswordChar = '*';
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void picpass_Click(object sender, EventArgs e)
        {
            if (txt_pass.PasswordChar == '*')
            {
                picpass.Image = BDS.Properties.Resources.hide;
                txt_pass.PasswordChar = '\0';
            }
            else
            {
                picpass.Image = BDS.Properties.Resources.eye;
                txt_pass.PasswordChar = '*';
            }
            picpass.Refresh();
        }

        private void button4_MouseHover(object sender, EventArgs e)
        {
            button4.BackgroundImage = BDS.Properties.Resources.shutdown2;
            button4.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            button4.BackgroundImage = BDS.Properties.Resources.shutdown;
            button4.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is Form2)
                {
                    // Form2 is already open, bring it to the front
                    form.BringToFront();
                    return;
                }
            }
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void button1_MouseHover(object sender, EventArgs e)
        {
            button1.ForeColor = Color.Yellow;
            button1.FlatAppearance.MouseOverBackColor = Color.Black; 
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.ForeColor = Color.White;
            button1.FlatAppearance.MouseOverBackColor = Color.Transparent;
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
            button2.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button2.ForeColor = Color.Yellow;
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            button2.ForeColor = Color.White;
        }
    }
}
