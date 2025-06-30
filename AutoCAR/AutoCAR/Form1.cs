using AutoCar;
using AutoCAR;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AutoCAR
{
    public partial class Login: Form
    {
        MySqlConnection con;
        MySqlCommand cmd;
        MySqlDataReader dr;
        MySqlDataAdapter da;
        DataTable du;
        public Login()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            string minhaConTemp = ConfigurationManager.ConnectionStrings["minhaConnectionAppTemp"].ToString();
            con = new MySqlConnection(minhaConTemp);
            try
            {
                con.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Erro: Não foi possivel ligar ao servidor da base de dados a fechar o programa.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.Write(ex.Message);
                Environment.Exit(1);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            con.Close();
            string minhaCon = ConfigurationManager.ConnectionStrings["minhaConnectionApp"].ToString();
            con = new MySqlConnection(minhaCon);
            try
            {
                con.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Erro ao conectar à base de dados. Tentando executar o script SQL.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string scriptPath = Path.Combine(Application.StartupPath, "SQL", "sql.sql");
                Functions.RunSQLScript(scriptPath, new MySqlConnection(minhaConTemp));
                Console.Write(ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            btn_Login.BackColor = Color.FromArgb(40, 168, 241);
            btn_Login.ForeColor = Color.White;
            loadFirstImage();
        }

        private void btn_Sair_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btn_picpass_Click(object sender, EventArgs e)
        {
            if (txt_pass.PasswordChar == '*')
            {
                btn_picpass.BackgroundImage = Properties.Resources.hidden;
                btn_picpass.BackgroundImageLayout = ImageLayout.Stretch;
                txt_pass.PasswordChar = '\0';
            }
            else
            {
                btn_picpass.BackgroundImage = Properties.Resources.eye;
                btn_picpass.BackgroundImageLayout = ImageLayout.Stretch;
                txt_pass.PasswordChar = '*';
            }
            btn_picpass.Refresh();
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_user.Text.Trim()) && string.IsNullOrEmpty(txt_pass.Text.Trim()))
            {
                changeError();
            }
            else
            {
                try
                {
                    con.Open();
                    string query = "SELECT * from login WHERE username = @user AND pass = @pass";
                    cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("user", txt_user.Text.Trim());
                    cmd.Parameters.AddWithValue("pass", txt_pass.Text.Trim());
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        Variaveis.username = txt_user.Text.Trim();
                        Variaveis.nivel = dr.GetString("nivel");
                        int userId = dr.GetInt32("id");
                        dr.Close();
                        UpdateLastLogin(userId);
                        MessageBox.Show($"Bem vindo, {Variaveis.username}!", "AutoCar v1.0", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Logs.UpdateLog("Info", "Login com sucesso! - Utilizador: " + Variaveis.username);
                        GerirForms.trocarform(this, new Programa());
                        con.Close();
                    }
                    else
                    {
                        MessageBox.Show("User ou Pass não corretos!!! Tente novamente.", "AutoCar v1.0", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        changeError();
                        Logs.UpdateLog("Erro", "Tentativa de login falhada com user ou password incorrecta!");
                    }
                }
                catch (MySqlException w)
                {
                    MessageBox.Show($"Falha a ligar a base de dados: {w}");
                    Logs.UpdateLog("Erro", w.ToString());
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
        private void changeError()
        {
            txt_user.Text = "";
            txt_pass.Text = "";
            lbl_error.Text = "Utilizador ou password incorrecta...";
            lbl_error.ForeColor = Color.Red;
        }
        private void UpdateLastLogin(int ID)
        {
            try
            {
                string query = "UPDATE login SET lastlogin = NOW() WHERE id = @id";
                cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("id", ID);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException w)
            {
                MessageBox.Show($"Falha a ligar a base de dados: {w}");
                Logs.UpdateLog("Erro", w.ToString());
            }
        }

        private void txt_user_TextChanged(object sender, EventArgs e)
        {
            lbl_error.Text = "";
        }

        private void txt_pass_TextChanged(object sender, EventArgs e)
        {
            lbl_error.Text = "";
        }
        private void loadFirstImage()
        {
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"Select id_carro, marca, modelo, imagem from carros where id_carro > 0 and id_carro < 7;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                if (du.Rows.Count > 0)
                {
                    for (int i = 0; i < du.Rows.Count; i++)
                    {
                        if (Convert.IsDBNull(du.Rows[i]["imagem"]))
                        {
                            int carId = Convert.ToInt32(du.Rows[i]["id_carro"]);
                            string carMarca = du.Rows[i]["marca"].ToString();
                            string carModelo = du.Rows[i]["modelo"].ToString();
                            if (carId == 1 && carMarca.Equals("BMW") && carModelo.Equals("M3 Competition"))
                            {
                                addImagemCarro(carId, Properties.Resources.BMW);
                            }
                            else if (carId == 2 && carMarca.Equals("Mercedes-Benz") && carModelo.Equals("C 220d"))
                            {
                                addImagemCarro(carId, Properties.Resources.MERCEDES);
                            }
                            else if (carId == 3 && carMarca.Equals("Toyota") && carModelo.Equals("Corolla Hybrid"))
                            {
                                addImagemCarro(carId, Properties.Resources.TOYOTA);
                            }
                            else if (carId == 4 && carMarca.Equals("Tesla") && carModelo.Equals("Model 3 Long Range"))
                            {
                                addImagemCarro(carId, Properties.Resources.TESLA);
                            }
                            else if (carId == 5 && carMarca.Equals("Volkswagen") && carModelo.Equals("Golf 8 GTI"))
                            {
                                addImagemCarro(carId, Properties.Resources.GOLF);
                            }
                            else if (carId == 6 && carMarca.Equals("BMW") && carModelo.Equals("M3 Competition"))
                            {
                                addImagemCarro(carId, Properties.Resources.BMW);
                            }
                        }
                    }
                }
                con.Close();
            }
            catch (MySqlException w)
            {
                MessageBox.Show($"Falha a ligar a base de dados: {w}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", w.ToString());
            }
            catch (Exception w)
            {
                MessageBox.Show($"Erro: {w}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", w.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"Select id_vendedor, imagem from vendedores where id_vendedor = 1;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                if (du.Rows.Count > 0)
                {
                    int vendedorId = Convert.ToInt32(du.Rows[0]["id_vendedor"]);
                    if (vendedorId == 1)
                    {
                        addImagemVendedor(vendedorId, Properties.Resources.Eu);
                    }
                }
                con.Close();
            }
            catch (MySqlException w)
            {
                MessageBox.Show($"Falha a ligar a base de dados: {w}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", w.ToString());
            }
            catch (Exception w)
            {
                MessageBox.Show($"Erro: {w}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", w.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void addImagemCarro(int id, Image carImage)
        {
            try
            {
                con.Open();
                byte[] imagemBytes = Functions.ImageToByteArray(carImage);
                cmd = new MySqlCommand(@"update carros set imagem = @imagem where id_carro = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@imagem", imagemBytes);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar Imagem do carro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void addImagemVendedor(int id, Image VendedorImage)
        {
            try
            {
                con.Open();
                byte[] imagemBytes = Functions.ImageToByteArray(VendedorImage);
                cmd = new MySqlCommand(@"update vendedores set imagem = @imagem where id_vendedor = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@imagem", imagemBytes);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar Imagem do Vendedor: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
    }
}
