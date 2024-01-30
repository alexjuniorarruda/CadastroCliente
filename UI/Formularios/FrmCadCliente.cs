using System;
using System.IO;
using System.Data;
using System.Windows.Forms;
using DataBase;
using CadastroCliente;
using CadastroCliente.DataSetsRelatorios;

namespace UI
{
    public partial class FrmCadCliente : Form
    {
        ClienteDb cliente = new ClienteDb();
        DataTable tabela = new DataTable();

        public FrmCadCliente()
        {
            InitializeComponent();
        }

        private void FrmCadCliente_Load(object sender, EventArgs e)
        {
            cliente.GetConsultaGrid(dgClientes, tabela);
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            string[] textBox = new string[] { txtNome.Text, txtEndereco.Text, txtNumero.Text, txtBairro.Text };

            string sinalObrigatorio = "*";

            foreach (var item in textBox)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    MessageBox.Show($"Todos os campos com {sinalObrigatorio} precisam ser preenchidos!", "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            mtxCPF.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            mtxTelefone.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            mtxCelular.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            mtxCep.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;

            if (string.IsNullOrWhiteSpace(mtxCPF.Text) || string.IsNullOrWhiteSpace(mtxCep.Text))
            {
                MessageBox.Show($"Todos os campos com {sinalObrigatorio} precisam ser preenchidos!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (cliente.GetCpf(mtxCPF.Text))
            {
                MessageBox.Show("CPF já cadastrado!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtEmail.Text != string.Empty)
            {
                if (!txtEmail.Text.Contains("@"))
                {
                    MessageBox.Show("E-mail inválido!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (txtId.Text != string.Empty)
            {
                cliente.Id = Convert.ToInt32(txtId.Text);
            }

            cliente.DataCadastro = cliente.GetDataAtual();
            cliente.Nome = txtNome.Text.Trim().ToUpper();
            cliente.Cpf = mtxCPF.Text.Trim();
            cliente.DataNascimento = Convert.ToDateTime(dtpDataNascimento.Text);
            cliente.Logradouro = txtEndereco.Text.Trim().ToUpper();
            cliente.NumeroCasa = txtNumero.Text.Trim().ToUpper();
            cliente.Bairro = txtBairro.Text.Trim().ToUpper();
            cliente.Estado = txtUf.Text.ToUpper();
            cliente.Cidade = txtCidade.Text.ToUpper();            
            cliente.Cep = mtxCep.Text.Trim();
            cliente.TelefoneFixo = mtxTelefone.Text;
            cliente.Celular = mtxCelular.Text;
            cliente.Email = txtEmail.Text.Trim();

            cliente.Salvar(cliente);
            cliente.LimparControles(this);
            cliente.GetConsultaGrid(dgClientes, tabela);

        }

        private void dgClientes_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string[] endereco = dgClientes.CurrentRow.Cells["Endereço"].Value.ToString().Split(',');

            txtId.Text = dgClientes.CurrentRow.Cells["Id"].Value.ToString();
            txtNome.Text = dgClientes.CurrentRow.Cells["Nome"].Value.ToString();
            dtpDataNascimento.Text = dgClientes.CurrentRow.Cells["Data de Nascimento"].Value.ToString();
            mtxCPF.Text = dgClientes.CurrentRow.Cells["CPF"].Value.ToString();
            mtxTelefone.Text = dgClientes.CurrentRow.Cells["Telefone Fixo"].Value.ToString();
            mtxCelular.Text = dgClientes.CurrentRow.Cells["Celular"].Value.ToString();
            txtEmail.Text = dgClientes.CurrentRow.Cells["E-mail"].Value.ToString();
            txtEndereco.Text = endereco[0].Trim();
            txtNumero.Text = endereco[1].Trim();
            txtBairro.Text = endereco[2].Trim();
            mtxCep.Text = dgClientes.CurrentRow.Cells["CEP"].Value.ToString();
            txtUf.Text = dgClientes.CurrentRow.Cells["Estado"].Value.ToString();
            txtCidade.Text = dgClientes.CurrentRow.Cells["Cidade"].Value.ToString();
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            if (txtId.Text != string.Empty)
            {
                DialogResult dialogResult = MessageBox.Show("Deseja realmente excluir esse registro?", "Mensagem", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    cliente.Excluir(txtId.Text);
                    cliente.GetConsultaGrid(dgClientes, tabela);
                    cliente.LimparControles(this);
                }
            }
            else
            {
                MessageBox.Show("Por favor selecione um registro.", "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void txtFiltro_TextChanged(object sender, EventArgs e)
        {
            tabela.DefaultView.RowFilter = string.Format("[{0}] LIKE '%{1}%'", "Nome", txtFiltro.Text);
        }

        private void txtNome_KeyPress(object sender, KeyPressEventArgs e)
        {
            cliente.ImpedeNumerosTextBox(e);
        }
        
        private void btnGerarExcel_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application xCellApp = new Microsoft.Office.Interop.Excel.Application();

            if (dgClientes.Rows.Count > 0)
            {
                try
                {
                    xCellApp.Application.Workbooks.Add(Type.Missing);

                    for (int i = 1; i < dgClientes.Columns.Count + 1; i++)
                    {
                        xCellApp.Cells[1, i] = dgClientes.Columns[i - 1].HeaderText.ToUpper();
                    }

                    for (int i = 0; i < dgClientes.Rows.Count; i++)
                    {
                        for (int j = 0; j < dgClientes.Columns.Count; j++)
                        {
                            xCellApp.Cells[i + 2, j + 1] = dgClientes.Rows[i].Cells[j].Value.ToString();
                        }
                    }

                    xCellApp.Columns.AutoFit();
                    xCellApp.Visible = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro : " + ex.Message);
                    xCellApp.Quit();
                }
            }

        }

        private void btnGerarTxt_Click(object sender, EventArgs e)
        {
            try
            {
                sfdSalvar = new SaveFileDialog();
                sfdSalvar.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                sfdSalvar.FilterIndex = 2;
                sfdSalvar.RestoreDirectory = true;

                if (sfdSalvar.ShowDialog() == DialogResult.OK)
                {
                    using (TextWriter writer = new StreamWriter(sfdSalvar.FileName))
                    {
                        for (int i = 0; i < dgClientes.Rows.Count; i++)
                        {
                            for (int j = 0; j < dgClientes.Columns.Count; j++)
                            {
                                if (j == dgClientes.Columns.Count - 1)
                                {
                                    writer.Write($"{dgClientes.Rows[i].Cells[j].Value.ToString().ToLower()}; ");
                                }
                                else
                                {
                                    writer.Write($"{dgClientes.Rows[i].Cells[j].Value.ToString().ToLower()}, ");
                                }

                            }
                        }

                        writer.Close();
                        MessageBox.Show("Dados exportados com sucesso!", "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void mtxCep_Leave(object sender, EventArgs e)
        {
            if (mtxCep.Text != string.Empty)
            {
                mtxCep.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
                string[] list = cliente.RetornaCep(mtxCep.Text);
                txtUf.Text = list[0];
                txtCidade.Text = list[1];
            }
            else
            {
                MessageBox.Show("Cep não informado!", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }     
        }

        private void btnGerarPdf_Click(object sender, EventArgs e)
        {
            var dataSet = new DataSetClientes();

            foreach (DataGridViewRow linha in dgClientes.Rows)
            {
                if (!linha.IsNewRow)
                {
                    var novaLinhaDataSet = dataSet.Cliente.NewClienteRow();
                    novaLinhaDataSet.Id = linha.Cells["Id"].Value.ToString();
                    novaLinhaDataSet.Data_Cadastro = linha.Cells["Data do Cadastro"].Value.ToString();
                    novaLinhaDataSet.Nome = linha.Cells["Nome"].Value.ToString();
                    novaLinhaDataSet.CPF = linha.Cells["CPF"].Value.ToString();
                    novaLinhaDataSet.Data_Nascimento = linha.Cells["Data de Nascimento"].Value.ToString();
                    novaLinhaDataSet.Endereco = linha.Cells["Endereço"].Value.ToString();
                    novaLinhaDataSet.Cidade = linha.Cells["Cidade"].Value.ToString();
                    novaLinhaDataSet.Estado = linha.Cells["Estado"].Value.ToString();
                    novaLinhaDataSet.CEP = linha.Cells["CEP"].Value.ToString();
                    novaLinhaDataSet.Telefone_fixo = linha.Cells["Telefone Fixo"].Value.ToString();
                    novaLinhaDataSet.Celular = linha.Cells["Celular"].Value.ToString();
                    novaLinhaDataSet.Email = linha.Cells["E-mail"].Value.ToString();

                    dataSet.Cliente.AddClienteRow(novaLinhaDataSet);
                }
            }

            var frmRelatorio = new FrmRelatorio(dataSet);
            frmRelatorio.Show();

        }
    }
}
