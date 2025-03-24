using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Configuration;

namespace Login
{
    public partial class Form4 : Form
    {
        private string currentUser;
        private Boolean setCombo = false;
        private Form mainWindow;

        public Form4(string currentUser, Form window)
        {
            InitializeComponent();
            this.currentUser = currentUser;
            this.mainWindow = window;
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            label2.Text = currentUser;
            comboBox1.SelectedIndex = 0;
            pictureBox1.BackgroundImage = BDS.Properties.Resources.folder;
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox2.BackgroundImage = BDS.Properties.Resources.folder1;
            pictureBox2.BackgroundImageLayout = ImageLayout.Stretch;
            button1.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button3.BackgroundImage = BDS.Properties.Resources.run;
            button3.BackgroundImageLayout = ImageLayout.Stretch;
            button3.FlatAppearance.MouseOverBackColor = Color.Transparent;
            pictureBox3.BackgroundImage = BDS.Properties.Resources.login;
            pictureBox3.BackgroundImageLayout = ImageLayout.Stretch;
            button4.BackgroundImage = BDS.Properties.Resources.database;
            button4.BackgroundImageLayout = ImageLayout.Stretch;
            button4.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button5.BackgroundImage = BDS.Properties.Resources.newFile;
            button5.BackgroundImageLayout = ImageLayout.Stretch;
            button5.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button6.BackgroundImage = BDS.Properties.Resources.databaseSetup;
            button6.BackgroundImageLayout = ImageLayout.Stretch;
            button6.FlatAppearance.MouseOverBackColor = Color.Transparent;
            lbl_ficheiros.Text = "Ficheiros: 0";
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!setCombo)
            {
                setCombo = true;
            }
            else
            {
                if (comboBox1.SelectedItem.ToString() == "Logout")
                {
                    Console.WriteLine("Window: " + mainWindow);

                    if (mainWindow != null && mainWindow is Form1)
                    {
                        Form1 mainForm = (Form1)mainWindow;
                        mainForm.clearTextBoxes();
                        mainForm.Show();
                    }

                    this.Hide();
                }
                else if (comboBox1.SelectedItem.ToString() == "Fechar")
                {
                    System.Environment.Exit(0);
                }
                else if (comboBox1.SelectedItem.ToString() == "Check Logs")
                {
                    Form2 form2 = new Form2();
                    form2.Show();
                }
                else
                {
                    return;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    refreshFiles(folderDialog.SelectedPath);
                }
            }
        }

        private void refreshFiles(string folder)
        {
            lblFolder.Text = folder;
            string[] files = Directory.GetFiles(lblFolder.Text);
            listBox1.Items.Clear();
            listBox1.Items.AddRange(files.Select(Path.GetFileName).ToArray());
            lbl_ficheiros.Text = $"Ficheiros: {files.Length}";
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                textBox1.Text = "";
                string selectedFileName = listBox1.SelectedItem.ToString();
                string fullFilePath = Path.Combine(lblFolder.Text, selectedFileName);

                if (File.Exists(fullFilePath))
                {
                    textBox1.Lines = File.ReadLines(fullFilePath).ToArray();
                }
                else
                {
                    MessageBox.Show("Ficheiro não encontrado.");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedFileName = listBox1.SelectedItem.ToString();
                string saveFilePath = Path.Combine(lblFolder.Text, selectedFileName);

                try
                {
                    File.WriteAllText(saveFilePath, textBox1.Text);
                    MessageBox.Show("Ficheiro gravado com sucesso: " + saveFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro a gravar: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Nenhum ficheiro selecionado");
            }
        }
        public static string RemoveComments(string sqlScript)
        {
            string patternSingleLine = @"(--.*?$)";
            sqlScript = Regex.Replace(sqlScript, patternSingleLine, string.Empty, RegexOptions.Multiline);
            string patternMultiLine = @"/\*.*?\*/";
            sqlScript = Regex.Replace(sqlScript, patternMultiLine, string.Empty, RegexOptions.Singleline);

            return sqlScript;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                textBox2.Text = $"❌ Não existe nada para correr!";
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["minhaConnectionApp"].ToString()
            .Replace("hostApp", DatabaseConnector.getDataBase("host"))
            .Replace("appPort", DatabaseConnector.getDataBase("host_port"))
            .Replace("userApp", DatabaseConnector.getDataBase("host_user"))
            .Replace("passApp", DatabaseConnector.getDataBase("host_pass"));
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("", conn))
                        {
                            string sqlScript = textBox1.Text;
                            string cleanedScript = RemoveComments(sqlScript);
                            string[] sqlCommands = cleanedScript.Split(';');

                            foreach (string sql in sqlCommands)
                            {
                                if (!string.IsNullOrWhiteSpace(sql))
                                {
                                    cmd.CommandText = sql;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        stopwatch.Stop();
                        textBox2.Text = $"✅ Script executado com sucesso!";
                    }
                    catch (MySqlException ex)
                    {
                        // Handle MySQL-specific errors
                        stopwatch.Stop();
                        textBox2.Text = $"❌ Erro ao executar o script: {ex.Message}"; // Failure message with error details
                    }
                    catch (Exception ex)
                    {
                        // Handle any other errors
                        stopwatch.Stop();
                        textBox2.Text = $"❌ Ocorreu um erro: {ex.Message}"; // General error message
                    }
                }
            }
            catch (Exception ex)
            {
                textBox2.Text = $"❌ Error: {ex.Message}, Script executado com erro";
            }
            stopwatch.Stop();
            textBox2.Text += $"\n⏳Time: {stopwatch.ElapsedMilliseconds} ms";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Formdatabase formdatabase = new Formdatabase();
            formdatabase.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"Valor de lblFolder.Text: '{lblFolder.Text}'");
            
            if (string.IsNullOrWhiteSpace(lblFolder.Text))
            {
                MessageBox.Show("Não podes criar ficheiro sem escolheres a pasta primeiro!", "ERRO");
                return;
            }
            string name = Prompt.ShowDialog("Enter file name:", "File Name Input");
            if (!string.IsNullOrEmpty(name))
            {
                string fileName = $"{name}.sql";
                string filePath = Path.Combine(lblFolder.Text, fileName);
                if (File.Exists(filePath))
                {
                    MessageBox.Show($"Ficheiro com o nome {name} já existe!", "ERRO");
                }
                else
                {
                    // If not, create an empty file (or do any other operation)
                    File.Create(filePath).Close();
                    MessageBox.Show($"Ficheiro '{name}.sql' criado com sucesso!", "Sucesso");
                    refreshFiles(lblFolder.Text);
                }
            }
            else
            {
                MessageBox.Show("Não escolheste nenhum nome!", "Warning");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Setup formSetup = new Setup();
            formSetup.Show();
        }
    }
}
