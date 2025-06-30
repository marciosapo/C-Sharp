using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
namespace AutoCAR
{
    public static class Logs
    {

        public static void UpdateLog(string tipo, string msg)
        {
            MySqlConnection con;
            MySqlCommand cmd;
            MySqlDataReader dr;
            string minhaCon = ConfigurationManager.ConnectionStrings["minhaConnectionApp"].ToString();
            con = new MySqlConnection(minhaCon);
            try
            {
                con.Open();
                string query = "insert into logs(tipo, msg) values(@tipo, @msg)";
                cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("tipo", tipo);
                cmd.Parameters.AddWithValue("msg", msg);
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    Console.WriteLine($"\nLog tipo {tipo}: {msg}");
                }
                con.Close();
            }
            catch (MySqlException w)
            {
                MessageBox.Show($"Falha a ligar a base de dados: {w}");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
    }
}
