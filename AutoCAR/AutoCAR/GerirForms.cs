using System.Windows.Forms;

namespace AutoCar
{
    internal class GerirForms
    {
        public static void trocarform(Form formAtual, Form novoForm)
        {
            formAtual.Hide();
            novoForm.ShowDialog();
            formAtual.Close();
        }
    }
}
