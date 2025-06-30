using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Data;

namespace AutoCAR
{
    internal static class Functions
    {
        //---------------------------------------------------------------------------//
        //----------------------------------IMAGEMS----------------------------------//
        //---------------------------------------------------------------------------//
        public static byte[] ImageToByteArray(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bmp = new Bitmap(img))
                {
                    bmp.Save(ms, ImageFormat.Png);
                }
                return ms.ToArray();
            }
        }
        public static void loadImage(PictureBox image, byte[] imagemBytes)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream(imagemBytes);
                image.Image = Image.FromStream(ms);
                image.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar a imagem: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                }
            }
        }
        public static void RunSQLScript(string scriptFilePath, MySqlConnection con)
        {
            try
            {
                string script = File.ReadAllText(scriptFilePath);

                // Reopen the connection to execute the script
                con.Open();

                // Execute the script (you can split it into individual commands if necessary)
                using (MySqlCommand cmd = new MySqlCommand(script, con))
                {
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Script SQL executado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // If script execution fails, log the error
                MessageBox.Show($"Erro ao executar o script SQL: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro ao Executar Script", ex.ToString());
            }
            finally
            {
                // Ensure the connection is closed
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
    }
}
