using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static Mysqlx.Expect.Open.Types.Condition.Types;

namespace Login
{
    public static class DatabaseConnector
    {
        public static string testConnection(string host, string port, string user, string pass)
        {
            string result = "Falha a conectar!";
            string connectionString = "";
            if (!string.IsNullOrEmpty(user) && user != "")
            {
                connectionString = $"Server={host};Port={int.Parse(port)};User Id={user};Password={pass};";
        }
            else
            {
                connectionString = $"Server={host};Port={int.Parse(port)};";
            }
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    result = "Ligação efetuada com sucesso!";
                }
                catch (Exception e)
                {
                    result = "Falha a conectar!";
                }
            }
            return result;
        }
        public static void saveDataBase(string host, string port, string user, string pass)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            // Check if the key exists and update it, or add a new key-value pair
            if (config.AppSettings.Settings["host"] != null) config.AppSettings.Settings["host"].Value = host;
            else config.AppSettings.Settings.Add("host", host);
            if (config.AppSettings.Settings["host_port"] != null) config.AppSettings.Settings["host_port"].Value = port;
            else config.AppSettings.Settings.Add("host_port", port);
            if (config.AppSettings.Settings["host_user"] != null) config.AppSettings.Settings["host_user"].Value = user;
            else config.AppSettings.Settings.Add("host_user", user);
            if (config.AppSettings.Settings["host_pass"] != null) config.AppSettings.Settings["host_pass"].Value = pass;
            else config.AppSettings.Settings.Add("host_pass", pass);
            // Save the changes and refresh the section
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            MessageBox.Show("Configuração atualizada!", "Setup");
        }
        public static string getDataBase(string type)
        {
            string istype = ConfigurationManager.AppSettings[type];
            return istype;
        }
    }
}