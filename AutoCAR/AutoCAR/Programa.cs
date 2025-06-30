using AutoCar;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace AutoCAR
{
    public partial class Programa: Form
    {
        MySqlConnection con;
        MySqlCommand cmd;
        MySqlDataAdapter da;
        MySqlDataReader dr;
        DataTable du;
        DataTable dl;
        private int currentIndex = -1;
        Boolean IdChanged = false;
        Boolean DadosChanged = false;
        Boolean inAdd = false;
        int IdBefore = 0;
        public Programa()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            string minhaCon = ConfigurationManager.ConnectionStrings["minhaConnectionApp"].ToString();
            con = new MySqlConnection(minhaCon);
        }
        private void btn_Sair_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void Programa_Load(object sender, EventArgs e)
        {
            lbl_autoCar.Text = "AutoCar / " + Variaveis.username;
            lbl_user.Text = Variaveis.username;
            lbl_nivel.Text = Variaveis.nivel;
            if(Variaveis.nivel == "user")
            {
                tabControl.TabPages.Remove(tabVendedores);
                tabControl.TabPages.Remove(tabVendas);
                tabControl.TabPages.Remove(tabClientes);
                tabControl.TabPages.Remove(tabUsers);
            }
            ResetCurrentIndex();
            VerCarros();
            pictureBox1.BackgroundImage = Properties.Resources.carros6;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetCurrentIndex();

            switch (tabControl.SelectedTab.Name)
            {
                case "tabUsers":
                    clearAll();
                    VerUtilizadores();
                    verLogs();
                    pictureBox1.BackgroundImage = Properties.Resources.carros3;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;

                case "tabCarros":
                    clearAll();
                    VerCarros();
                    pictureBox1.BackgroundImage = Properties.Resources.carros6;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;

                case "tabVendedores":
                    clearAll();
                    VerVendedores();
                    pictureBox1.BackgroundImage = Properties.Resources.carros2;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;

                case "tabClientes":
                    clearAll();
                    VerClientes();
                    pictureBox1.BackgroundImage = Properties.Resources.carros4;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;

                case "tabVendas":
                    clearAll();
                    getClientes();
                    getModelos();
                    getVendedores();
                    verVendas();
                    pictureBox1.BackgroundImage = Properties.Resources.carros5;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    string dataFormatada = dateTimePicker1.Value.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("pt-PT"));
                    dateTimePicker1.CustomFormat = "'" + dataFormatada + "'";
                    break;
            }
        }
        //------------------------------------------------------------------------------------//
        //---------------------------Tab Gestão de Utilizadores-------------------------------//
        private void btn_Consultar_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = consultar(txt_id.Text);
            if (id != -1)
            {
                MostrarRegistroById(id);
                IdChanged = false;
                DadosChanged = false;
            }
            else
            {
                if (IdChanged)
                {
                    txt_id.Text = IdBefore.ToString();
                    IdChanged = false;
                    DadosChanged = false;
                }
            }
        }
        private void btn_Next_Click(object sender, EventArgs e)
        {
            if (Next(du.Rows.Count))
            {
                MostrarRegistro(currentIndex);
            }
        }
        private void btn_Previous_Click(object sender, EventArgs e)
        {
            if (Previous())
            {
                MostrarRegistro(currentIndex);
            }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }
        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
        }
        private void btn_Gravar_Click(object sender, EventArgs e)
        {
            if (!isAdd("id"))
            {
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("Não podes adicionar pois alteraste o ID", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                IdChanged = false;
                if (IdBefore != -1)
                {
                    txt_id.Text = IdBefore.ToString();
                }
                else
                {
                    txt_id.Text = "";
                }
                return;
            }
            if (String.IsNullOrEmpty(txt_nome.Text))
            {
                MessageBox.Show($"Erro: Username não pode estar em branco!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (ck_Nivel.SelectedIndex == -1)
            {
                MessageBox.Show($"Erro: Nivel não pode estar em branco!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (ck_Nivel.SelectedItem == null || (ck_Nivel.SelectedItem.ToString() != "admin" && ck_Nivel.SelectedItem.ToString() != "user"))
            {
                MessageBox.Show("Erro: O nível só pode ser 'Admin' ou 'User'!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                con.Open();
                cmd = new MySqlCommand("insert into login(username, pass, nivel) values(@user, @pass, @nivel);", con);
                cmd.Parameters.AddWithValue("user", txt_nome.Text);
                cmd.Parameters.AddWithValue("pass", txt_pass.Text);
                cmd.Parameters.AddWithValue("nivel", ck_Nivel.SelectedItem);
                dr = cmd.ExecuteReader();
                MessageBox.Show("Dados inseridos com sucesso!", "Dados Inseridos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                con.Close();
                VerUtilizadores();
                inAdd = false;
                CloseAllCancels();
            }
            catch (MySqlException w)
            {
                MessageBox.Show($"Falha a ligar a base de dados: {w}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", w.ToString());
                verLogs();
            }
            catch (Exception w)
            {
                MessageBox.Show($"Erro: {w}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", w.ToString());
                verLogs();
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void btn_Update_Click(object sender, EventArgs e)
        {
            if(inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (currentIndex == -1)
            {
                MessageBox.Show("Não existe nenhum Registo!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("O Id foi alterado não pode atualizar sem fazer consulta pois ID não pode ser alterado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!DadosChanged)
            {
                MessageBox.Show("Não foi feita qualquer alteração!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (String.IsNullOrEmpty(txt_nome.Text))
            {
                MessageBox.Show($"Erro: Username não pode estar em branco!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (ck_Nivel.SelectedIndex == -1)
            {
                MessageBox.Show($"Erro: Nivel não pode estar em branco!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (ck_Nivel.SelectedItem == null || (ck_Nivel.SelectedItem.ToString() != "admin" && ck_Nivel.SelectedItem.ToString() != "user"))
            {
                MessageBox.Show("Erro: O nível só pode ser 'Admin' ou 'User'!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                con.Open();
                cmd = new MySqlCommand("update login set username = @user, pass = @pass, nivel = @nivel where id = @id;", con);
                cmd.Parameters.AddWithValue("id", int.Parse(txt_id.Text));
                cmd.Parameters.AddWithValue("user", txt_nome.Text);
                cmd.Parameters.AddWithValue("pass", txt_pass.Text);
                cmd.Parameters.AddWithValue("nivel", ck_Nivel.SelectedItem);
                dr = cmd.ExecuteReader();
                MessageBox.Show("Dados Atualizados com sucesso!", "Dados Atualizados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                con.Close();
                VerUtilizadores();
            }
            catch (MySqlException w)
            {
                MessageBox.Show($"Falha a ligar a base de dados: {w}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", w.ToString());
                verLogs();
            }
            catch (Exception w)
            {
                MessageBox.Show($"Erro: {w}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", w.ToString());
                verLogs();
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        //--------------------------------------------------------------------//
        //---------------------------Tab Carros-------------------------------//
        private void btn_Previous_Carros_Click(object sender, EventArgs e)
        {
            if (Previous())
            {
                MostrarRegistroCarro(currentIndex);
            }
        }
        private void btn_Next_Carros_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (Next(du.Rows.Count))
            {
                MostrarRegistroCarro(currentIndex);
            }
        }
        private void btn_carregar_imagem_carro_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (file.ShowDialog() == DialogResult.OK)
            {
                pic_carro.Image = Image.FromFile(file.FileName);
                pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                DadosChanged = true;
            }
        }
        private void btn_Consultar_Carro_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = consultar(txt_id_carro.Text);
            if (id != -1)
            {
                MostrarRegistroCarroById(id);
                IdChanged = false;
                DadosChanged = false;
            }
            else
            {
                if (IdChanged)
                {
                    txt_id_carro.Text = IdBefore.ToString();
                    IdChanged = false;
                    DadosChanged = false;
                }
            }
        }
        private void btn_Gravar_Carro_Click(object sender, EventArgs e)
        {
            if (!isAdd("id_carro"))
            {
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("Não podes adicionar pois alteraste o ID", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                IdChanged = false;
                if (IdBefore != -1)
                {
                    txt_id_carro.Text = IdBefore.ToString();
                }
                else
                {
                    txt_id_carro.Text = "";
                }
                return;
            }
            if (string.IsNullOrEmpty(txt_narca.Text) || string.IsNullOrEmpty(txt_modelo.Text) ||
        string.IsNullOrEmpty(txt_cilindrada.Text) || string.IsNullOrEmpty(txt_potencia.Text) ||
        string.IsNullOrEmpty(txt_preco.Text) || ck_Combustivel.SelectedIndex == -1)
            {
                MessageBox.Show("Preciso preencher todos os campos e selecionar uma imagem!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(!int.TryParse(txt_cilindrada.Text, out int cilindrada))
            {
                MessageBox.Show("Cilindrada precisa ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txt_potencia.Text, out int potencia))
            {
                MessageBox.Show("Potência precisa ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!decimal.TryParse(txt_preco.Text, out decimal preco))
            {
                MessageBox.Show("Preço precisa ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                con.Open();
                byte[] imagemBytes = Functions.ImageToByteArray(pic_carro.Image);
                cmd = new MySqlCommand(@"INSERT INTO carros (marca, modelo, cilindrada, potencia, tipo_combustivel, imagem, preco) 
                                 VALUES (@marca, @modelo, @cilindrada, @potencia, @tipo_combustivel, @imagem, @preco)", con);
                cmd.Parameters.AddWithValue("@marca", txt_narca.Text);
                cmd.Parameters.AddWithValue("@modelo", txt_modelo.Text);
                cmd.Parameters.AddWithValue("@cilindrada", cilindrada);
                cmd.Parameters.AddWithValue("@potencia", potencia);
                cmd.Parameters.AddWithValue("@tipo_combustivel", ck_Combustivel.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@imagem", imagemBytes);
                cmd.Parameters.AddWithValue("@preco", preco);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Carro inserido com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logs.UpdateLog("Info", "Carro inserido com sucesso!");
                con.Close();
                currentIndex = 0;
                VerCarros();
                inAdd = false;
                CloseAllCancels();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inserir carro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void btn_Atualizar_Carro_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (currentIndex == -1)
            {
                MessageBox.Show("Não existe nenhum Registo!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("O Id foi alterado não pode atualizar sem fazer consulta pois ID não pode ser alterado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(!DadosChanged)
            {
                MessageBox.Show("Não foi feita qualquer alteração!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (string.IsNullOrEmpty(txt_id_carro.Text) || string.IsNullOrEmpty(txt_narca.Text) || string.IsNullOrEmpty(txt_modelo.Text) ||
        string.IsNullOrEmpty(txt_cilindrada.Text) || string.IsNullOrEmpty(txt_potencia.Text) ||
        string.IsNullOrEmpty(txt_preco.Text) || ck_Combustivel.SelectedIndex == -1)
            {
                MessageBox.Show("Preciso preencher todos os campos e selecionar uma imagem!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txt_id_carro.Text, out int id))
            {
                MessageBox.Show("ID tem de ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txt_cilindrada.Text, out int cilindrada))
            {
                MessageBox.Show("Cilindrada precisa ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txt_potencia.Text, out int potencia))
            {
                MessageBox.Show("Potência precisa ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!decimal.TryParse(txt_preco.Text, out decimal preco))
            {
                MessageBox.Show("Preço precisa ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                con.Open();
                byte[] imagemBytes = Functions.ImageToByteArray(pic_carro.Image);
                cmd = new MySqlCommand(@"update carros set marca = @marca, modelo = @modelo, cilindrada = @cilindrada, potencia = @potencia, tipo_combustivel = @tipo_combustivel, imagem = @imagem, preco = @preco where id_carro = @id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@marca", txt_narca.Text);
                cmd.Parameters.AddWithValue("@modelo", txt_modelo.Text);
                cmd.Parameters.AddWithValue("@cilindrada", cilindrada);
                cmd.Parameters.AddWithValue("@potencia", potencia);
                cmd.Parameters.AddWithValue("@tipo_combustivel", ck_Combustivel.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@imagem", imagemBytes);
                cmd.Parameters.AddWithValue("@preco", preco);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Carro Atualizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logs.UpdateLog("Info", "Carro atualizado com sucesso!");
                con.Close();
                currentIndex = 0;
                VerCarros();
                IdChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar carro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        //----------------------------------------------------------------------//
        //---------------------------Tab Vendedores-------------------------------//
        private void btn_vendedor_carregar_imagem_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (file.ShowDialog() == DialogResult.OK)
            {
                pic_vendedor.Image = Image.FromFile(file.FileName);
                pic_vendedor.SizeMode = PictureBoxSizeMode.StretchImage;
                DadosChanged = true;
            }
        }

        private void btn_Previous_Vendedor_Click(object sender, EventArgs e)
        {
            if (Previous())
            {
                MostrarRegistroVendedor(currentIndex);
            }
        }

        private void btn_Next_Vendedor_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (Next(du.Rows.Count))
            {
                MostrarRegistroVendedor(currentIndex);
            }
        }
        private void btn_Atualizar_Vendedor_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (currentIndex == -1 && currentIndex == 0)
            {
                MessageBox.Show("Não existe nenhum Registo!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("O Id foi alterado não pode atualizar sem fazer consulta pois ID não pode ser alterado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!DadosChanged)
            {
                MessageBox.Show("Não foi feita qualquer alteração!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!txt_vendedor_email.Text.Contains("@"))
            {
                MessageBox.Show("Email ínvalido pois não tem o @", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txt_vendedor_nome.Text) || string.IsNullOrEmpty(txt_vendedor_tlm.Text) ||
        string.IsNullOrEmpty(txt_vendedor_email.Text))
            {
                MessageBox.Show("Preciso preencher todos os campos e selecionar uma imagem!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txt_id_vendedor.Text, out int id))
            {
                MessageBox.Show("ID tem de ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txt_vendedor_tlm.Text, out int tlm))
            {
                MessageBox.Show("Telefone precisa ser números!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txt_vendedor_tlm.Text.Length != 9)
            {
                MessageBox.Show("Telefone precisa ter 9 digitos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                con.Open();
                int idVendedor = id;
                byte[] imagemBytes = Functions.ImageToByteArray(pic_vendedor.Image);
                cmd = new MySqlCommand(@"update vendedores set nome = @nome, telefone = @telefone, email = @email, imagem = @imagem where id_vendedor = @id;", con);
                cmd.Parameters.AddWithValue("@nome", txt_vendedor_nome.Text);
                cmd.Parameters.AddWithValue("@telefone", tlm.ToString());
                cmd.Parameters.AddWithValue("@email", txt_vendedor_email.Text);
                cmd.Parameters.AddWithValue("@imagem", imagemBytes);
                cmd.Parameters.AddWithValue("@id", idVendedor);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Vendedor atualizado!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logs.UpdateLog("Info", "Vendedor atualizado com sucesso!");
                con.Close();
                currentIndex = 0;
                VerVendedores();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar vendedor: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void btn_Gravar_Vendedor_Click(object sender, EventArgs e)
        {
            if (!isAdd("id_vendedor"))
            {
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("Não podes adicionar pois alteraste o ID", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                IdChanged = false;
                if (IdBefore != -1)
                {
                    txt_id_vendedor.Text = IdBefore.ToString();
                }
                else
                {
                    txt_id_vendedor.Text = "";
                }
                return;
            }
            if (!txt_vendedor_email.Text.Contains("@"))
            {
                MessageBox.Show("Email ínvalido pois não tem o @", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txt_vendedor_nome.Text) || string.IsNullOrEmpty(txt_vendedor_tlm.Text) ||
        string.IsNullOrEmpty(txt_vendedor_email.Text))
            {
                MessageBox.Show("Preciso preencher todos os campos e selecionar uma imagem!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(!int.TryParse(txt_vendedor_tlm.Text, out int tlm))
            {
                MessageBox.Show("Telefone precisa ser números!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txt_vendedor_tlm.Text.Length != 9)
            {
                MessageBox.Show("Telefone precisa ter 9 digitos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                con.Open();
                byte[] imagemBytes = Functions.ImageToByteArray(pic_vendedor.Image);
                cmd = new MySqlCommand(@"INSERT INTO vendedores (nome, telefone, email, imagem) VALUES (@nome, @telefone, @email, @imagem);", con);
                cmd.Parameters.AddWithValue("@nome", txt_vendedor_nome.Text);
                cmd.Parameters.AddWithValue("@telefone", tlm.ToString());
                cmd.Parameters.AddWithValue("@email", txt_vendedor_email.Text);
                cmd.Parameters.AddWithValue("@imagem", imagemBytes);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Vendedor inserido!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logs.UpdateLog("Info", "Vendedor inserido com sucesso!");
                con.Close();
                currentIndex = 0;
                VerVendedores();
                inAdd = false;
                CloseAllCancels();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inserir vendedor: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void btn_Consultar_Vendedor_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = consultar(txt_id_vendedor.Text);
            if (id != -1)
            {
                MostrarRegistroVendedorById(id);
                IdChanged = false;
                DadosChanged = false;
            }
            else
            {
                if (IdChanged)
                {
                    txt_id_vendedor.Text = IdBefore.ToString();
                    IdChanged = false;
                    DadosChanged = false;
                }
            }
        }
        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            dataGridView3.ClearSelection();
        }
        //----------------------------------------------------------------------//
        //---------------------------Tab Clientes-------------------------------//
        private void btn_Previous_Cliente_Click(object sender, EventArgs e)
        {
            if (Previous())
            {
                MostrarRegistroCliente(currentIndex);
            }
        }
        private void btn_Next_Cliente_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (Next(du.Rows.Count))
            {
                MostrarRegistroCliente(currentIndex);
            }
        }
        private void btn_Gravar_Cliente_Click(object sender, EventArgs e)
        {
            if (!isAdd("id_cliente"))
            {
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("Não podes adicionar pois alteraste o ID", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                IdChanged = false;
                if (IdBefore != -1)
                {
                    txt_id_cliente.Text = IdBefore.ToString();
                }
                else
                {
                    txt_id_cliente.Text = "";
                }
                return;
            }
            if (string.IsNullOrEmpty(txt_cliente_nome.Text) || string.IsNullOrEmpty(txt_cliente_email.Text) ||
        string.IsNullOrEmpty(txt_cliente_endereco.Text) || string.IsNullOrEmpty(txt_cliente_tlm.Text))
            {
                MessageBox.Show("Preciso preencher todos os campos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txt_cliente_tlm.Text, out int tlm))
            {
                MessageBox.Show("Telefone precisa ser números!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!txt_cliente_email.Text.Contains("@"))
            {
                MessageBox.Show("Email ínvalido pois não tem o @", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txt_cliente_tlm.Text.Length != 9)
            {
                MessageBox.Show("Telefone precisa ter 9 digitos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                con.Open();
                cmd = new MySqlCommand(@"INSERT INTO clientes (nome, telefone, email, endereco) VALUES (@nome, @telefone, @email, @endereco);", con);
                cmd.Parameters.AddWithValue("@nome", txt_cliente_nome.Text);
                cmd.Parameters.AddWithValue("@telefone", tlm.ToString());
                cmd.Parameters.AddWithValue("@email", txt_cliente_email.Text);
                cmd.Parameters.AddWithValue("@endereco", txt_cliente_endereco.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Cliente inserido!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logs.UpdateLog("Info", "Cliente inserido com sucesso!");
                con.Close();
                currentIndex = 0;
                VerClientes();
                inAdd = false;
                CloseAllCancels();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inserir Cliente: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void btn_Atualizar_Cliente_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (currentIndex == -1)
            {
                MessageBox.Show("Não existe nenhum Registo!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (IdChanged)
            {
                MessageBox.Show("O Id foi alterado não pode atualizar sem fazer consulta pois ID não pode ser alterado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!DadosChanged)
            {
                MessageBox.Show("Não foi feita qualquer alteração!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!txt_cliente_email.Text.Contains("@"))
            {
                MessageBox.Show("Email ínvalido pois não tem o @", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(txt_cliente_nome.Text) || string.IsNullOrEmpty(txt_cliente_tlm.Text) ||
        string.IsNullOrEmpty(txt_cliente_endereco.Text) || string.IsNullOrEmpty(txt_cliente_email.Text))
            {
                MessageBox.Show("Preciso preencher todos os campos e selecionar uma imagem!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txt_cliente_tlm.Text, out int tlm))
            {
                MessageBox.Show("Telefone precisa ser números!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(txt_cliente_tlm.Text.Length != 9) {
                MessageBox.Show("Telefone precisa ter 9 digitos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                con.Open();
                cmd = new MySqlCommand(@"update clientes set nome = @nome, telefone = @telefone, email = @email, endereco = @endereco where id_cliente = @id;", con);
                cmd.Parameters.AddWithValue("@nome", txt_cliente_nome.Text);
                cmd.Parameters.AddWithValue("@telefone", tlm.ToString());
                cmd.Parameters.AddWithValue("@email", txt_cliente_email.Text);
                cmd.Parameters.AddWithValue("@endereco", txt_cliente_endereco.Text);
                cmd.Parameters.AddWithValue("@id", int.Parse(du.Rows[currentIndex]["id_cliente"].ToString()));
                cmd.ExecuteNonQuery();
                MessageBox.Show("Cliente atualizado!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logs.UpdateLog("Info", "Cliente atualizado com sucesso!");
                con.Close();
                currentIndex = 0;
                VerClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar Cliente: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

        }
        private void btn_Consultar_Cliente_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = consultar(txt_id_cliente.Text);
            if (id != -1)
            {
                MostrarRegistroClienteById(id);
                IdChanged = false;
                DadosChanged = false;
            }
            else
            {
                if (IdChanged)
                {
                    txt_id_cliente.Text = IdBefore.ToString();
                    IdChanged = false;
                    DadosChanged = false;
                }
            }
        }
        //----------------------------------------------------------------------//
        //-----------------------------Tab Vendas-------------------------------//
        private void btn_Previous_Venda_Click(object sender, EventArgs e)
        {
            if (Previous())
            {
                MostrarVenda(currentIndex);
            }
        }
        private void btn_Next_Venda_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (Next(du.Rows.Count))
            {
                MostrarVenda(currentIndex);
            }
        }
        private void btn_Gravar_Venda_Click(object sender, EventArgs e)
        {
            if (!isAdd("id_venda"))
            {
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("Não podes adicionar pois alteraste o ID", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                IdChanged = false;
                if (IdBefore != -1)
                {
                    txt_id_vendas.Text = IdBefore.ToString();
                }
                else
                {
                    txt_id_vendas.Text = "";
                }
                return;
            }
            if (ck_Cliente.SelectedIndex == -1 || ck_Modelo.SelectedIndex == -1 || ck_Vendedor.SelectedIndex == -1)
            {
                MessageBox.Show("Precisas selecionar Vendedor/Modelo/Client!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                int idModelo = getModeloId(ck_Modelo.SelectedItem.ToString());
                string nomeVendedor = ck_Vendedor.SelectedItem.ToString();
                string nomeCliente = ck_Cliente.SelectedItem.ToString();
                string modeloCarro = ck_Modelo.SelectedItem.ToString();
                decimal precoVenda;
                if (!decimal.TryParse(txt_venda_preco.Text, out precoVenda))
                {
                    MessageBox.Show("Preço inválido!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string dataVenda = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                string horaVenda = dateTimePicker1.Value.ToString("HH:mm:ss");
                if (string.IsNullOrEmpty(modeloCarro) || idModelo == -1)
                {
                    MessageBox.Show("Não existe num carro desse modelo disponivel!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                con.Open();
                cmd = new MySqlCommand(@"INSERT INTO vendas (nome_cliente, nome_vendedor, modelo_carro, data_venda, hora_venda, preco_venda) VALUES (@nome_cliente, @nome_vendedor, @modelo_carro, @data_venda, @hora_venda, @preco_venda);", con);
                cmd.Parameters.AddWithValue("@nome_cliente", nomeCliente);
                cmd.Parameters.AddWithValue("@nome_vendedor", nomeVendedor);
                cmd.Parameters.AddWithValue("@modelo_carro", modeloCarro);
                cmd.Parameters.AddWithValue("@preco_venda", precoVenda);
                cmd.Parameters.AddWithValue("@data_venda", dataVenda);
                cmd.Parameters.AddWithValue("@hora_venda", horaVenda);
                cmd.ExecuteNonQuery();
                byte[] imagemBytes = null;
                cmd = new MySqlCommand("SELECT imagem FROM carros WHERE id_carro = @id_carro", con);
                cmd.Parameters.AddWithValue("@id_carro", idModelo);
                dr = cmd.ExecuteReader();
                if (dr.Read() && !dr.IsDBNull(0))
                {
                    imagemBytes = (byte[])dr["imagem"];
                }
                dr.Close();
                //cmd = new MySqlCommand(@"update carros set vendido = @vendido where id_carro = @id_carro", con);
                cmd = new MySqlCommand(@"INSERT into carros_vendidos (id_carro, marca, modelo, cilindrada, potencia, tipo_combustivel, imagem, preco) SELECT id_carro, marca, modelo, cilindrada, potencia, tipo_combustivel, @imagem, preco FROM carros where id_carro = @id_carro;", con);
                cmd.Parameters.AddWithValue("@id_carro", idModelo);
                cmd.Parameters.AddWithValue("@imagem", imagemBytes ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(@"DELETE FROM carros where id_carro = @id_carro;", con);
                cmd.Parameters.AddWithValue("@id_carro", idModelo);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Venda inserida!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logs.UpdateLog("Info", "Venda inserida com sucesso!");
                con.Close();
                currentIndex = 0;
                getModelos();
                verVendas();
                inAdd = false;
                CloseAllCancels();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inserir Venda: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void btn_Atualizar_Venda_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (currentIndex == -1)
            {
                MessageBox.Show("Não existe nenhum Registo!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (IdChanged)
            {
                MessageBox.Show("O Id foi alterado não pode atualizar sem fazer consulta pois ID não pode ser alterado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!DadosChanged)
            {
                MessageBox.Show("Não foi feita qualquer alteração!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (ck_Cliente.SelectedIndex == -1 || ck_Modelo.SelectedIndex == -1 || ck_Vendedor.SelectedIndex == -1)
            {
                MessageBox.Show("Precisas selecionar Vendedor/Modelo/Client!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                int idModelo = getModeloId(ck_Modelo.SelectedItem.ToString());
                string nomeVendedor = ck_Vendedor.SelectedItem.ToString();
                string nomeCliente = ck_Cliente.SelectedItem.ToString();
                string modeloCarro = ck_Modelo.SelectedItem.ToString();
                decimal precoVenda;
                int id_venda = -1;
                if (!decimal.TryParse(txt_venda_preco.Text, out precoVenda))
                {
                    MessageBox.Show("Preço inválido!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string dataVenda = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                string horaVenda = dateTimePicker1.Value.ToString("HH:mm:ss");
                if (string.IsNullOrEmpty(modeloCarro) || idModelo == -1)
                {
                    MessageBox.Show("Não existe num carro desse modelo disponivel já foi vendido", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                con.Open();
                cmd = new MySqlCommand(@"SELECT id_venda, modelo_carro FROM vendas WHERE nome_cliente = @nome_cliente ORDER BY data_venda DESC LIMIT 1;", con);
                cmd.Parameters.AddWithValue("@nome_cliente", nomeCliente);
                dr = cmd.ExecuteReader();
                string modeloAnterior = null;
                if (dr.Read())
                {
                    modeloAnterior = dr["modelo_carro"].ToString();
                    id_venda = int.Parse(dr["id_venda"].ToString());
                    dr.Close();
                }
                else
                {
                    dr.Close();
                }
                if (modeloAnterior != null && modeloAnterior != modeloCarro)
                {
                    cmd = new MySqlCommand(@"SELECT id_carro, marca, modelo, cilindrada, potencia, tipo_combustivel, imagem, preco FROM carros_vendidos WHERE modelo = @modelo_carro LIMIT 1;", con);
                    cmd.Parameters.AddWithValue("@modelo_carro", modeloAnterior);
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        int idCarroRestaurado = int.Parse(dr["id_carro"].ToString());
                        string marca = dr["marca"].ToString();
                        string cilindrada = dr["cilindrada"].ToString();
                        string potencia = dr["potencia"].ToString();
                        string tipoCombustivel = dr["tipo_combustivel"].ToString();
                        byte[] imagem = dr["imagem"] as byte[];
                        decimal preco = Convert.ToDecimal(dr["preco"]);
                        dr.Close();
                        cmd = new MySqlCommand(@"INSERT INTO carros (marca, modelo, cilindrada, potencia, tipo_combustivel, imagem, preco) 
                                    VALUES (@marca, @modelo, @cilindrada, @potencia, @tipo_combustivel, @imagem, @preco);", con);
                        cmd.Parameters.AddWithValue("@marca", marca);
                        cmd.Parameters.AddWithValue("@modelo", modeloAnterior);
                        cmd.Parameters.AddWithValue("@cilindrada", cilindrada);
                        cmd.Parameters.AddWithValue("@potencia", potencia);
                        cmd.Parameters.AddWithValue("@tipo_combustivel", tipoCombustivel);
                        cmd.Parameters.AddWithValue("@imagem", imagem ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@preco", preco);
                        cmd.ExecuteNonQuery();
                        dr.Close();
                        cmd = new MySqlCommand(@"DELETE FROM carros_vendidos WHERE id_carro = @id_carro;", con);
                        cmd.Parameters.AddWithValue("@id_carro", idCarroRestaurado);
                        cmd.ExecuteNonQuery();
                        dr.Close();
                    }
                    else
                    {
                        dr.Close();
                    }
                }
                cmd = new MySqlCommand(@"INSERT INTO vendas (nome_cliente, nome_vendedor, modelo_carro, data_venda, hora_venda, preco_venda) 
                            VALUES (@nome_cliente, @nome_vendedor, @modelo_carro, @data_venda, @hora_venda, @preco_venda);", con);
                cmd.Parameters.AddWithValue("@nome_cliente", nomeCliente);
                cmd.Parameters.AddWithValue("@nome_vendedor", nomeVendedor);
                cmd.Parameters.AddWithValue("@modelo_carro", modeloCarro);
                cmd.Parameters.AddWithValue("@preco_venda", precoVenda);
                cmd.Parameters.AddWithValue("@data_venda", dataVenda);
                cmd.Parameters.AddWithValue("@hora_venda", horaVenda);
                cmd.ExecuteNonQuery();
                byte[] imagemBytes = null;
                cmd = new MySqlCommand("SELECT imagem FROM carros WHERE id_carro = @id_carro", con);
                cmd.Parameters.AddWithValue("@id_carro", idModelo);
                dr = cmd.ExecuteReader();
                if (dr.Read() && !dr.IsDBNull(0))
                {
                    imagemBytes = (byte[])dr["imagem"];
                    dr.Close();
                }
                else
                {
                    dr.Close();
                }
                Console.WriteLine("TEST: " + imagemBytes);
                cmd = new MySqlCommand(@"INSERT INTO carros_vendidos (id_carro, marca, modelo, cilindrada, potencia, tipo_combustivel, imagem, preco) 
                            SELECT id_carro, marca, modelo, cilindrada, potencia, tipo_combustivel, @imagem, preco 
                            FROM carros WHERE id_carro = @id_carro;", con);
                cmd.Parameters.AddWithValue("@id_carro", idModelo);
                cmd.Parameters.AddWithValue("@imagem", imagemBytes ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(@"DELETE FROM carros WHERE id_carro = @id_carro;", con);
                cmd.Parameters.AddWithValue("@id_carro", idModelo);
                cmd.ExecuteNonQuery();
                cmd = new MySqlCommand(@"DELETE FROM vendas WHERE id_venda = @id_venda", con); 
                cmd.Parameters.AddWithValue("@id_venda", id_venda);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Venda atualizada!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Logs.UpdateLog("Info", "Venda atualizada com sucesso!");
                con.Close();
                currentIndex = 0;
                getModelos();
                verVendas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inserir Venda: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logs.UpdateLog("Erro", ex.ToString());
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
        private void btn_Consultar_Venda_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int id = consultar(txt_id_vendas.Text);
            if (id != -1)
            {
                MostraVendaById(id);
                IdChanged = false;
                DadosChanged = false;
            }
            else
            {
                if (IdChanged)
                {
                    txt_id_vendas.Text = IdBefore.ToString();
                    IdChanged = false;
                    DadosChanged = false;
                }
            }
        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            string dataFormatada = dateTimePicker1.Value.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("pt-PT"));
            dateTimePicker1.CustomFormat = "'" + dataFormatada + "'";
            DadosChanged = true;
        }
        private void ck_Modelo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ck_Modelo.SelectedItem.ToString()))
            {
                txt_venda_preco.Text = getPrecoByName(ck_Modelo.SelectedItem.ToString()).ToString();
                DadosChanged = true;
            }
        }
        //--------------------------------Para baixo estão os metodos--------------------------//
        //-------------------------------------------------------------------------------------//
        private void VerUtilizadores()
        {
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"Select * from login;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                dataGridView1.DataSource = du;
                con.Close();
                if (du.Rows.Count > 0)
                {
                    txt_id.Text = dataGridView1.CurrentRow.Cells["ID"].Value.ToString();
                    txt_nome.Text = dataGridView1.CurrentRow.Cells["username"].Value.ToString();
                    txt_pass.Text = dataGridView1.CurrentRow.Cells["pass"].Value.ToString();
                    ck_Nivel.SelectedItem = dataGridView1.CurrentRow.Cells["nivel"].Value.ToString();
                    dataGridView1.Columns["ID"].Visible = false;
                    dataGridView1.ClearSelection();
                    if (int.TryParse(txt_id.Text.Trim(), out int id))
                    {
                        IdBefore = id;
                    }
                    else
                    {
                        MessageBox.Show($"ID inválido. Por favor, insira apenas números. {id}");
                    }
                }
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
        private void verLogs() {
            if (tabControl.SelectedTab != tabUsers) return;
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"Select timestamp_log, tipo, msg from logs;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                dl = new DataTable();
                da.Fill(dl);
                dataGridView2.DataSource = dl;
                con.Close();
                if (dl.Rows.Count > 0)
                {
                    currentIndex = 0;
                    MostrarRegistro(currentIndex);
                }
                dataGridView2.ClearSelection();
            }
            catch (Exception w)
            {
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
        private void VerCarros()
        {
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = @"
                SELECT id_carro, marca, modelo, cilindrada, potencia, tipo_combustivel, preco, imagem, vendido 
                FROM carros
                UNION 
                SELECT id_carro, marca, modelo, cilindrada, potencia, tipo_combustivel, preco, imagem, vendido 
                FROM carros_vendidos 
                ORDER BY id_carro ASC;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                if (du.Rows.Count > 0)
                {
                    txt_id_carro.Text = du.Rows[0]["id_carro"].ToString();
                    txt_narca.Text = du.Rows[0]["marca"].ToString();
                    txt_modelo.Text = du.Rows[0]["modelo"].ToString();
                    txt_cilindrada.Text = du.Rows[0]["cilindrada"].ToString();
                    txt_potencia.Text = du.Rows[0]["potencia"].ToString();
                    ck_Combustivel.SelectedItem = du.Rows[0]["tipo_combustivel"].ToString();
                    txt_preco.Text = du.Rows[0]["preco"].ToString();
                    if (int.TryParse(txt_id_carro.Text.Trim(), out int id))
                    {
                        IdBefore = id;
                    }
                    else
                    {
                        MessageBox.Show("ID inválido. Por favor, insira apenas números.");
                    }
                    if (!Convert.IsDBNull(du.Rows[0]["imagem"]))
                    {
                        try
                        {
                            byte[] imagemBytes = (byte[])du.Rows[0]["imagem"];
                            if (imagemBytes.Length > 0)
                            {
                                if (imagemBytes.Length > 0)
                                {
                                    Functions.loadImage(pic_carro, imagemBytes);
                                }
                            }
                            else
                            {
                                pic_carro.Image = Properties.Resources.sem_foto;
                                pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erro ao carregar imagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            pic_carro.Image = Properties.Resources.sem_foto;
                            pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    else
                    {
                        pic_carro.Image = Properties.Resources.sem_foto;
                        pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    if ("Sim".Equals(du.Rows[0]["vendido"].ToString()))
                    {
                        lbl_Vendido.Visible = true;
                    }
                    else
                    {
                        lbl_Vendido.Visible = false;
                    }
                    IdChanged = false;
                }
                else
                {
                    currentIndex = -1;
                }
                lbl_Encontrados.Text = du.Rows.Count.ToString();
                currentIndex = 0;
                DadosChanged = false;
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
        private void VerVendedores()
        {
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"SELECT * FROM vendedores;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                if (du.Rows.Count > 0)
                {
                    txt_id_vendedor.Text = du.Rows[0]["id_vendedor"].ToString();
                    txt_vendedor_nome.Text = du.Rows[0]["nome"].ToString();
                    txt_vendedor_email.Text = du.Rows[0]["email"].ToString();
                    txt_vendedor_tlm.Text = du.Rows[0]["telefone"].ToString();
                    if (du.Rows[0]["imagem"] != DBNull.Value)
                    {
                        try
                        {
                            byte[] imagemBytes = (byte[])du.Rows[0]["imagem"];
                            if (imagemBytes.Length > 0)
                            {
                                if (imagemBytes.Length > 0)
                                {
                                    Functions.loadImage(pic_vendedor, imagemBytes);
                                }
                            }
                            else
                            {
                                pic_carro.Image = Properties.Resources.sem_foto;
                                pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erro ao carregar imagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            pic_carro.Image = Properties.Resources.sem_foto;
                        }
                    }
                    else
                    {
                        pic_vendedor.Image = Properties.Resources.sem_foto;
                        pic_vendedor.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    currentIndex = 0; 
                    MostrarRegistroVendedor(currentIndex);
                    if (int.TryParse(txt_id_vendedor.Text.Trim(), out int id))
                    {
                        IdBefore = id;
                    }
                    else
                    {
                        MessageBox.Show("ID inválido. Por favor, insira apenas números.");
                    }
                    DadosChanged = false;
                }
                else
                {
                    pic_vendedor.Image = Properties.Resources.sem_foto;
                    pic_vendedor.SizeMode = PictureBoxSizeMode.StretchImage;
                    currentIndex = -1;

                }
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
        private void VerClientes()
        {
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"SELECT * FROM clientes;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                if (du.Rows.Count > 0)
                {
                    txt_id_cliente.Text = du.Rows[0]["id_cliente"].ToString();
                    txt_cliente_nome.Text = du.Rows[0]["nome"].ToString();
                    txt_cliente_endereco.Text = du.Rows[0]["endereco"].ToString();
                    txt_cliente_email.Text = du.Rows[0]["email"].ToString();
                    txt_cliente_tlm.Text = du.Rows[0]["telefone"].ToString();
                    currentIndex = 0;
                    MostrarRegistroCliente(currentIndex);
                    if (int.TryParse(txt_id_cliente.Text.Trim(), out int id))
                    {
                        IdBefore = id;
                    }
                    else
                    {
                        MessageBox.Show("ID inválido. Por favor, insira apenas números.");
                    }
                    DadosChanged = false;
                }
                else
                {
                    currentIndex = -1;
                }
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
        //---------------------------------------------------------------------------//
        //------------------------------UTILIZADORES---------------------------------//
        //---------------------------------------------------------------------------//
        private void MostrarRegistro(int index)
        {
            if (du.Rows.Count > 0 && index >= 0 && index < du.Rows.Count)
            {
                txt_id.Text = du.Rows[index]["ID"].ToString();
                txt_nome.Text = du.Rows[index]["username"].ToString();
                txt_pass.Text = du.Rows[index]["pass"].ToString();
                ck_Nivel.SelectedItem = du.Rows[index]["nivel"].ToString();
                currentIndex = index;
                IdBefore = index;
                IdChanged = false;
                DadosChanged = false;
            }
        }
        private void MostrarRegistroById(int index)
        {
            Boolean found = false;
            for (int i = 0; i < du.Rows.Count; i++)
            {
                if (int.Parse(du.Rows[i]["ID"].ToString()) == index)
                {
                    txt_id.Text = du.Rows[i]["ID"].ToString();
                    txt_nome.Text = du.Rows[i]["username"].ToString();
                    txt_pass.Text = du.Rows[i]["pass"].ToString();
                    ck_Nivel.SelectedItem = du.Rows[i]["nivel"].ToString();
                    currentIndex = i;
                    IdBefore = int.Parse(txt_id.Text.ToString());
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                txt_id.Text = IdBefore.ToString();
                MessageBox.Show($"Não foi encontrado nenhum Registo com o Id {index}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            IdChanged = false;
            DadosChanged = false;
        }
        //---------------------------------------------------------------------------//
        //----------------------------------CARROS-----------------------------------//
        //---------------------------------------------------------------------------//
        private void MostrarRegistroCarro(int index)
        {
            if (du.Rows.Count > 0 && index >= 0 && index < du.Rows.Count)
            {
                txt_id_carro.Text = du.Rows[index]["id_carro"].ToString();
                txt_narca.Text = du.Rows[index]["marca"].ToString();
                txt_modelo.Text = du.Rows[index]["modelo"].ToString();
                txt_cilindrada.Text = du.Rows[index]["cilindrada"].ToString();
                txt_potencia.Text = du.Rows[index]["potencia"].ToString();
                ck_Combustivel.SelectedItem = du.Rows[index]["tipo_combustivel"].ToString();
                txt_preco.Text = du.Rows[index]["preco"].ToString();
                checkVendido(du.Rows[index]["vendido"].ToString());
                if (!Convert.IsDBNull(du.Rows[index]["imagem"]))
                {
                    try
                    {
                        byte[] imagemBytes = (byte[])du.Rows[index]["imagem"];
                        if (imagemBytes.Length > 0)
                        {
                            if (imagemBytes.Length > 0)
                            {
                                Functions.loadImage(pic_carro, imagemBytes);
                            }
                        }
                        else
                        {
                            pic_carro.Image = Properties.Resources.sem_foto;
                            pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar imagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        pic_carro.Image = Properties.Resources.sem_foto;
                        pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    pic_carro.Image = Properties.Resources.sem_foto;
                    pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                currentIndex = index;
                IdBefore = index;
                IdChanged = false;
                DadosChanged = false;
            }
        }
        private void MostrarRegistroCarroById(int index)
        {
            Boolean found = false;
            for (int i = 0; i < du.Rows.Count; i++)
            {
                if (int.Parse(du.Rows[i]["id_carro"].ToString()) == index)
                {
                    txt_id_carro.Text = du.Rows[i]["id_carro"].ToString();
                    txt_narca.Text = du.Rows[i]["marca"].ToString();
                    txt_modelo.Text = du.Rows[i]["modelo"].ToString();
                    txt_cilindrada.Text = du.Rows[i]["cilindrada"].ToString();
                    txt_potencia.Text = du.Rows[i]["potencia"].ToString();
                    ck_Combustivel.SelectedItem = du.Rows[i]["tipo_combustivel"].ToString();
                    txt_preco.Text = du.Rows[i]["preco"].ToString();
                    IdBefore = int.Parse(txt_id_carro.Text.ToString());
                    if (du.Rows[i]["imagem"] != null)
                    {
                        try
                        {
                            byte[] imagemBytes = (byte[])du.Rows[i]["imagem"];
                            if (imagemBytes.Length > 0)
                            {
                                if (imagemBytes.Length > 0)
                                {
                                    Functions.loadImage(pic_carro, imagemBytes);
                                }
                            }
                            else
                            {
                                pic_carro.Image = Properties.Resources.sem_foto;
                                pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erro ao carregar imagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            pic_carro.Image = Properties.Resources.sem_foto;
                        }
                    }
                    else
                    {
                        pic_carro.Image = Properties.Resources.sem_foto;
                        pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    checkVendido(du.Rows[i]["vendido"].ToString());
                    currentIndex = i;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                txt_id_carro.Text = IdBefore.ToString();
                MessageBox.Show($"Não foi encontrado nenhum Registo com o Id {index}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            IdChanged = false;
            DadosChanged = false;
        }
        //---------------------------------------------------------------------------//
        //--------------------------------VENDEDORES---------------------------------//
        //---------------------------------------------------------------------------//
        private void MostrarRegistroVendedor(int index)
        {
            if (du.Rows.Count > 0 && index >= 0 && index < du.Rows.Count)
            {
                txt_id_vendedor.Text = du.Rows[index]["id_vendedor"].ToString();
                txt_vendedor_nome.Text = du.Rows[index]["nome"].ToString();
                txt_vendedor_email.Text = du.Rows[index]["email"].ToString();
                txt_vendedor_tlm.Text = du.Rows[index]["telefone"].ToString();
                if (!Convert.IsDBNull(du.Rows[index]["imagem"]))
                {
                    try
                    {
                        byte[] imagemBytes = (byte[])du.Rows[index]["imagem"];
                        if (imagemBytes.Length > 0)
                        {
                            if (imagemBytes.Length > 0)
                            {
                                Functions.loadImage(pic_vendedor, imagemBytes);
                            }
                        }
                        else
                        {
                            pic_vendedor.Image = Properties.Resources.sem_foto;
                            pic_vendedor.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar imagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        pic_carro.Image = Properties.Resources.sem_foto;
                        pic_carro.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    pic_vendedor.Image = Properties.Resources.sem_foto;
                    pic_vendedor.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                currentIndex = index;
                verVendasByNome(txt_vendedor_nome.Text);
                IdBefore = index;
                IdChanged = false;
                DadosChanged = false;
            }
        }
        private void MostrarRegistroVendedorById(int index)
        {
            Boolean found = false;
            for(int i = 0; i < du.Rows.Count; i++) {
                {
                    if (int.Parse(du.Rows[i]["id_vendedor"].ToString()) == index)
                    {
                        txt_id_vendedor.Text = du.Rows[i]["id_vendedor"].ToString();
                        txt_vendedor_nome.Text = du.Rows[i]["nome"].ToString();
                        txt_vendedor_email.Text = du.Rows[i]["email"].ToString();
                        txt_vendedor_tlm.Text = du.Rows[i]["telefone"].ToString();
                        IdBefore = int.Parse(txt_id_vendedor.Text.ToString());
                        if (!Convert.IsDBNull(du.Rows[i]["imagem"]))
                        {
                            try
                            {
                                byte[] imagemBytes = (byte[])du.Rows[i]["imagem"];
                                if (imagemBytes.Length > 0)
                                {
                                    if (imagemBytes.Length > 0)
                                    {
                                        Functions.loadImage(pic_vendedor, imagemBytes);
                                    }
                                }
                                else
                                {
                                    pic_vendedor.Image = Properties.Resources.sem_foto;
                                    pic_vendedor.SizeMode = PictureBoxSizeMode.StretchImage;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Erro ao carregar imagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                pic_vendedor.Image = Properties.Resources.sem_foto;
                                pic_vendedor.SizeMode = PictureBoxSizeMode.StretchImage;
                            }
                        }
                        else
                        {
                            pic_vendedor.Image = Properties.Resources.sem_foto;
                            pic_vendedor.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        currentIndex = i;
                        verVendasByNome(txt_vendedor_nome.Text);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    txt_id_vendedor.Text = IdBefore.ToString();
                    MessageBox.Show($"Não foi encontrado nenhum Registo com o Id {index}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                IdChanged = false;
                DadosChanged = false;
            }
        }
        //---------------------------------------------------------------------------//
        //---------------------------------CLIENTES----------------------------------//
        //---------------------------------------------------------------------------//
        private void MostrarRegistroCliente(int index)
        {
            if (du.Rows.Count > 0 && index >= 0 && index < du.Rows.Count)
            {
                txt_id_cliente.Text = du.Rows[index]["id_cliente"].ToString();
                txt_cliente_nome.Text = du.Rows[index]["nome"].ToString();
                txt_cliente_endereco.Text = du.Rows[index]["endereco"].ToString();
                txt_cliente_email.Text = du.Rows[index]["email"].ToString();
                txt_cliente_tlm.Text = du.Rows[index]["telefone"].ToString();
                currentIndex = index;
                IdBefore = index;
                IdChanged = false;
                DadosChanged = false;
            }
        }
        private void MostrarRegistroClienteById(int index)
        {
            Boolean found = false;
            for (int i = 0; i < du.Rows.Count; i++)
            {
                if (int.Parse(du.Rows[i]["id_cliente"].ToString()) == index)
                {
                    txt_id_cliente.Text = du.Rows[i]["id_cliente"].ToString();
                    txt_cliente_nome.Text = du.Rows[i]["nome"].ToString();
                    txt_cliente_endereco.Text = du.Rows[i]["endereco"].ToString();
                    txt_cliente_email.Text = du.Rows[i]["email"].ToString();
                    txt_cliente_tlm.Text = du.Rows[i]["telefone"].ToString();
                    currentIndex = i;
                    IdBefore = int.Parse(txt_id_cliente.Text.ToString());
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                txt_id_cliente.Text = IdBefore.ToString();
                MessageBox.Show($"Não foi encontrado nenhum Registo com o Id {index}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            IdChanged = false;
            DadosChanged = false;
        }
        //---------------------------------------------------------------------------//
        //----------------------------------VENDAS-----------------------------------//
        //---------------------------------------------------------------------------//
        private void verVendas()
        {
            if (tabControl.SelectedTab != tabVendas) return;
            try
            {
                con.Open();
                cmd = new MySqlCommand(@"
                SELECT 
                    id_venda, 
                    nome_cliente, 
                    nome_vendedor,  
                    modelo_carro, 
                    data_venda, 
                    hora_venda, 
                    preco_venda
                FROM vendas;", con);
                da = new MySqlDataAdapter(cmd);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                if (du.Rows.Count > 0)
                {
                    currentIndex = 0;
                    MostrarVenda(currentIndex);
                }
                else
                {
                    currentIndex = -1;
                }
            }
            catch (Exception w)
            {
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
        private void MostrarVenda(int index)
        {
            if (du.Rows.Count > 0 && index >= 0 && index < du.Rows.Count)
            {
                txt_id_vendas.Text = du.Rows[index]["id_venda"].ToString();
                ck_Modelo.SelectedItem = du.Rows[index]["modelo_carro"].ToString();
                ck_Vendedor.SelectedItem = du.Rows[index]["nome_vendedor"].ToString();
                ck_Cliente.SelectedItem = du.Rows[index]["nome_cliente"].ToString();
                txt_venda_preco.Text = du.Rows[index]["preco_venda"].ToString();
                DateTime dataVenda = Convert.ToDateTime(du.Rows[index]["data_venda"]);
                TimeSpan horaVenda = TimeSpan.Parse(du.Rows[index]["hora_venda"].ToString());
                DateTime dataHoraCompleta = dataVenda.Add(horaVenda);
                dateTimePicker1.Value = dataHoraCompleta;
                currentIndex = index;
                IdBefore = index;
                IdChanged = false;
                DadosChanged = false;
            }
        }
        private void MostraVendaById(int index)
        {
            Boolean found = false;
            for (int i = 0; i < du.Rows.Count; i++)
            {
                if (int.Parse(du.Rows[i]["id_venda"].ToString()) == index)
                {
                    txt_id_vendas.Text = du.Rows[i]["id_venda"].ToString();
                    ck_Modelo.SelectedItem = du.Rows[i]["modelo_carro"].ToString();
                    ck_Vendedor.SelectedItem = du.Rows[i]["nome_vendedor"].ToString();
                    ck_Cliente.SelectedItem = du.Rows[i]["nome_cliente"].ToString();
                    txt_venda_preco.Text = du.Rows[i]["preco_venda"].ToString();
                    DateTime dataVenda = Convert.ToDateTime(du.Rows[i]["data_venda"]);
                    TimeSpan horaVenda = TimeSpan.Parse(du.Rows[i]["hora_venda"].ToString());
                    DateTime dataHoraCompleta = dataVenda.Add(horaVenda);
                    dateTimePicker1.Value = dataHoraCompleta;
                    currentIndex = i;
                    IdBefore = int.Parse(txt_id_vendas.Text.ToString());
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                txt_id_vendas.Text = IdBefore.ToString();
                MessageBox.Show($"Não foi encontrado nenhum Registo com o Id {index}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            IdChanged = false;
            DadosChanged = false;
        }
        private void verVendasByNome(string nome)
        {
            if (tabControl.SelectedTab != tabVendedores) return;
            try
            {
                con.Open();
                cmd = new MySqlCommand(@"
                SELECT  
                    nome_cliente, 
                    nome_vendedor, 
                    modelo_carro, 
                    data_venda, 
                    hora_venda, 
                    preco_venda
                FROM vendas
                WHERE nome_vendedor = @nome_vendedor;", con);
                cmd.Parameters.AddWithValue("@nome_vendedor", nome);
                da = new MySqlDataAdapter(cmd);
                dl = new DataTable();
                da.Fill(dl);
                dataGridView3.DataSource = dl;
                con.Close();
                dataGridView3.ClearSelection();
            }
            catch (Exception w)
            {
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
        private int getPrecoByName(string Modelo)
        {
            int preco = -1;
            try
            {
                con.Open();
                cmd = new MySqlCommand("SELECT preco FROM carros WHERE modelo = @modelo", con);
                cmd.Parameters.AddWithValue("@modelo", Modelo);

                dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    preco = (int)dr.GetDecimal(0);
                }

                dr.Close();
                if (preco == -1)
                {
                    cmd = new MySqlCommand("SELECT preco FROM carros_vendidos WHERE modelo = @modelo", con);
                    cmd.Parameters.AddWithValue("@modelo", Modelo);

                    dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        preco = (int)dr.GetDecimal(0);
                    }

                    dr.Close();
                }
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
            return preco;
        }
        private int getModeloId(String modelo)
        {
            int id = -1;
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"Select id_carro, modelo, vendido from carros;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                if (du.Rows.Count > 0)
                {
                    for (int i = 0; i < du.Rows.Count; i++)
                    {
                        if (modelo.Equals(du.Rows[i]["modelo"].ToString()) && "Não".Equals(du.Rows[i]["vendido"].ToString()))
                        {
                            id = int.Parse(du.Rows[i]["id_carro"].ToString());
                            break;
                        }
                    }
                }
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
            return id;
        }
        

        //---------------------------------------------------------------------------//
        //----------------------------LOAD COMBOBOXES--------------------------------//
        //---------------------------------------------------------------------------//
        private void getClientes()
        {
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"Select id_cliente, nome from clientes;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                ck_Cliente.Items.Clear();
                if (du.Rows.Count > 0)
                {
                    for (int i = 0; i < du.Rows.Count; i++)
                    {
                        ck_Cliente.Items.Add(du.Rows[i]["nome"].ToString());
                    }
                }
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
        private void getModelos()
        {
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"Select id_carro, modelo, preco, vendido from carros UNION Select id_carro, modelo, preco, vendido from carros_vendidos;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                ck_Modelo.Items.Clear();
                if (du.Rows.Count > 0)
                {
                    for (int i = 0; i < du.Rows.Count; i++)
                    {
                        if (!ck_Modelo.Items.Contains(du.Rows[i]["modelo"].ToString()))
                        {
                            ck_Modelo.Items.Add(du.Rows[i]["modelo"].ToString());
                        }
                    }
                }
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
        private void getVendedores()
        {
            try
            {
                con.Open();
                cmd = new MySqlCommand();
                cmd.CommandText = $"Select id_vendedor, nome from vendedores;";
                da = new MySqlDataAdapter(cmd.CommandText, con);
                du = new DataTable();
                da.Fill(du);
                con.Close();
                ck_Vendedor.Items.Clear();
                if (du.Rows.Count > 0)
                {
                    for (int i = 0; i < du.Rows.Count; i++)
                    {
                        ck_Vendedor.Items.Add(du.Rows[i]["nome"].ToString());
                    }
                }
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
        
        private void ResetCurrentIndex()
        {
            currentIndex = -1;
        }
        private void checkVendido(String vendido)
        {
            if ("Sim".Equals(vendido))
            {
                lbl_Vendido.Visible = true;
            }
            else
            {
                lbl_Vendido.Visible = false;
            }
        }
        private Boolean Previous()
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (currentIndex == -1)
            {
                MessageBox.Show("Não existe registros", "Registos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (currentIndex > 0)
            {
                currentIndex--;
            }
            else
            {
                MessageBox.Show("Não existe mais registos para trás", "Registos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }
        private Boolean Next(int count)
        {
            if (inAdd)
            {
                MessageBox.Show("Estás adicionar novo registo precisas cancelar antes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (currentIndex == -1)
            {
                MessageBox.Show("Não existe registros", "Registos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (currentIndex < count - 1)
            {
                currentIndex++;
            }
            else
            {
                MessageBox.Show("Não existe mais registos para a frente", "Registos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }
        private int consultar(string campo)
        {
            if (String.IsNullOrEmpty(campo))
            {
                MessageBox.Show("Para pesquisar um registro precisas escolher o ID!", "Registro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            if (!int.TryParse(campo, out int id))
            {
                MessageBox.Show("Id tem de ser um número!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            return id;
        }

        private void txt_id_carro_TextChanged(object sender, EventArgs e)
        {
            if (!inAdd)
            {
                IdChanged = true;
            }
        }

        private void txt_id_vendedor_TextChanged(object sender, EventArgs e)
        {
            if (!inAdd)
            {
                IdChanged = true;
            }
        }

        private void txt_id_cliente_TextChanged(object sender, EventArgs e)
        {
            if (!inAdd)
            {
                IdChanged = true;
            }
        }

        private void txt_id_vendas_TextChanged(object sender, EventArgs e)
        {
            if (!inAdd)
            {
                IdChanged = true;
            }
        }

        private void txt_id_TextChanged(object sender, EventArgs e)
        {
            if (!inAdd)
            {
                IdChanged = true;
            }
        }

        private void txt_id_Enter(object sender, EventArgs e)
        {
            if (!IdChanged)
            {
                int TempId = 0;
                if (int.TryParse(txt_id.Text, out TempId))
                {
                    IdBefore = TempId;
                }
            }
        }

        private void txt_id_vendedor_Enter(object sender, EventArgs e)
        {
            if (!IdChanged)
            {
                int TempId = 0;
                if (int.TryParse(txt_id_vendedor.Text, out TempId))
                {
                    IdBefore = TempId;
                }
            }
        }

        private void txt_id_cliente_Enter(object sender, EventArgs e)
        {
            if (!IdChanged)
            {
                int TempId = 0;
                if (int.TryParse(txt_id_cliente.Text, out TempId))
                {
                    IdBefore = TempId;
                }
            }
        }

        private void txt_id_vendas_Enter(object sender, EventArgs e)
        {
            if (!IdChanged)
            {
                int TempId = 0;
                if (int.TryParse(txt_id_vendas.Text, out TempId))
                {
                    IdBefore = TempId;
                }
            }
        }

        private void ck_Cliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void ck_Vendedor_SelectedIndexChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_nome_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_pass_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void ck_Nivel_SelectedIndexChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_cliente_nome_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_cliente_endereco_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_cliente_email_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_cliente_tlm_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_vendedor_nome_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_vendedor_email_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_vendedor_tlm_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_narca_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_modelo_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_cilindrada_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_potencia_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void ck_Combustivel_SelectedIndexChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void txt_preco_TextChanged(object sender, EventArgs e)
        {
            DadosChanged = true;
        }

        private void btn_cancel_vendedor_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                if (IdBefore != -1 && IdBefore != 0)
                {
                    txt_id_vendedor.Text = IdBefore.ToString();
                    MostrarRegistroVendedorById(IdBefore);
                }
                else
                {
                    txt_id_vendedor.Text = "";
                }
                CloseAllCancels();
                inAdd = false;
            }
        }

        private void btn_cancel_carro_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                if (IdBefore != -1 && IdBefore != 0)
                {
                    txt_id_carro.Text = IdBefore.ToString();
                    MostrarRegistroCarroById(IdBefore);
                }
                else
                {
                    txt_id_carro.Text = "";
                }
                CloseAllCancels();
                inAdd = false;
            }
        }

        private void btn_cancel_vendas_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                if (IdBefore != -1 && IdBefore != 0)
                {
                    txt_id_vendas.Text = IdBefore.ToString();
                    MostraVendaById(IdBefore);
                }
                else
                {
                    txt_id_vendas.Text = "";
                }
                CloseAllCancels();
                inAdd = false;
            }
        }

        private void btn_cancel_cliente_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                if (IdBefore != -1 && IdBefore != 0)
                {
                    txt_id_cliente.Text = IdBefore.ToString();
                    MostrarRegistroClienteById(IdBefore);
                }
                else
                {
                    txt_id_cliente.Text = "";
                }
                CloseAllCancels();
                inAdd = false;
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            if (inAdd)
            {
                if (IdBefore != -1 && IdBefore != 0)
                {
                    txt_id.Text = IdBefore.ToString();
                    MostrarRegistroById(IdBefore);
                }
                else
                {
                    txt_id.Text = "";
                }
                CloseAllCancels();
                inAdd = false;
            }
        }
        private void LimparCampos(int valor)
        {
            if (!inAdd)
            {
                inAdd = true;
                IdBefore = valor;
                txt_id_carro.Text = "";
                txt_narca.Text = "";
                txt_modelo.Text = "";
                txt_cilindrada.Text = "";
                txt_potencia.Text = "";
                ck_Combustivel.SelectedIndex = 0;
                txt_preco.Text = "";
                txt_id.Text = "";
                txt_nome.Text = "";
                txt_pass.Text = "";
                ck_Nivel.SelectedItem = 0;
                txt_id_vendedor.Text = "";
                txt_vendedor_nome.Text = "";
                txt_vendedor_email.Text = "";
                txt_vendedor_tlm.Text = "";
                txt_id_cliente.Text = "";
                txt_cliente_nome.Text = "";
                txt_cliente_endereco.Text = "";
                txt_cliente_email.Text = "";
                txt_cliente_tlm.Text = "";
                txt_id_vendas.Text = "";
                ck_Modelo.SelectedItem = 0;
                ck_Vendedor.SelectedItem = 0;
                ck_Cliente.SelectedItem = 0;
                txt_venda_preco.Text = "";
                lbl_Vendido.Visible = false;
                pic_carro.Image = Properties.Resources.sem_foto;
                pic_vendedor.Image = Properties.Resources.sem_foto;
                OpenAllCancels();
            }
        }
        private void CloseAllCancels()
        {
            btn_cancel.Visible = false;
            btn_cancel_carro.Visible = false;
            btn_cancel_vendas.Visible = false;
            btn_cancel_vendedor.Visible = false;
            btn_cancel_cliente.Visible = false;
        }
        private void OpenAllCancels()
        {
            btn_cancel.Visible = true;
            btn_cancel_carro.Visible = true;
            btn_cancel_vendas.Visible = true;
            btn_cancel_vendedor.Visible = true;
            btn_cancel_cliente.Visible = true;
        }
        private void clearAll()
        {
            currentIndex = -1;
            IdBefore = -1;
            inAdd = false;
            IdChanged = false;
            DadosChanged = false;
            CloseAllCancels();
        }
        private Boolean isAdd(string field)
        {
            if (!inAdd)
            {
                int campo = -1;
                if (!IdChanged)
                {
                    campo = currentIndex;
                }
                else
                {
                    campo = IdBefore;
                }
                if (campo != -1 && !String.IsNullOrEmpty(du.Rows[campo][field].ToString()))
                {
                    LimparCampos(int.Parse(du.Rows[campo][field].ToString()));
                    if(field == "id_venda")
                    {
                        ck_Modelo.SelectedIndex = 0;
                        dateTimePicker1.Format = DateTimePickerFormat.Custom;
                        dateTimePicker1.CustomFormat = "dd 'de' MMMM 'de' yyyy";
                        dateTimePicker1.Value = DateTime.Now;
                    }
                }
                else
                {
                    LimparCampos(0);
                    if (field == "id_venda")
                    {
                        ck_Modelo.SelectedIndex = 0;
                        dateTimePicker1.Format = DateTimePickerFormat.Custom;
                        dateTimePicker1.CustomFormat = "dd 'de' MMMM 'de' yyyy";
                        dateTimePicker1.Value = DateTime.Now;
                    }
                }
                return false;
            }
            return true;
        }
    }
}