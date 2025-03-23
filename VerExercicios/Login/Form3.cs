using MySql.Data.MySqlClient;
using MailKit.Net.Smtp; // Ensure this is included at the top of your file
using MimeKit;          // For creating the email message
using System;
using System.Net.Mail;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Org.BouncyCastle.Crypto;

namespace Login
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string Username = textBox1.Text.Trim();
            Boolean result = getValidation(Username);
            if (result == false)
            {
                MessageBox.Show($"Username not found: {Username}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Boolean getValidation(string Username)
        {
            Boolean result = false;
            string connectionString = "Server=localhost;Database=login;User ID=root;Password=Marcio158333;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Open the connection
                    Console.WriteLine("Connection successful!");

                    // SQL query with parameterized username to prevent SQL injection
                    string query = "SELECT userpass, email FROM login WHERE username = @username";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", Username);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Check if we have any rows (i.e., the username exists)
                            while (reader.Read())
                            {

                                string userpass = reader["userpass"].ToString().Trim();

                                string email = reader["email"].ToString().Trim();  // Trim to remove spaces
                                MessageBox.Show($"Username found: {Username}\nPassword: {userpass}\nEmail: {email}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            }
                            reader.Close();
                            conn.Close();
                            result = true;
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            return result;
        }
    }
}
