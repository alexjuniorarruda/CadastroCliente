using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Correios;

namespace DataBase
{
    public class ClienteDb
    {
        public int Id { get; set; }
        public DateTime DataCadastro { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Logradouro { get; set; }
        public string NumeroCasa { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Cep { get; set; }
        public string TelefoneFixo { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }

        ConnDb conn = new ConnDb();
        MySqlDataAdapter da;
        MySqlDataReader dr;

        public ClienteDb() { }

        public ClienteDb(string nome, string cpf, DateTime dataNascimento, string logradouro, string numeroCasa, string bairro, string cidade, string estado, string cep, string telefoneFixo, string celular, string email)
        {
            this.Nome = nome;
            this.Cpf = cpf;
            this.DataNascimento = dataNascimento;
            this.Logradouro = logradouro;
            this.NumeroCasa = numeroCasa;
            this.Bairro = bairro;
            this.Cidade = cidade;
            this.Estado = estado;
            this.TelefoneFixo = telefoneFixo;
            this.Celular = celular;
            this.Email = email;
        }

        public string[] RetornaCep(string cep)
        {
            string[] list = new string[2];

            if (!string.IsNullOrWhiteSpace(cep))
            {
                using (var service = new CorreiosApi())
                {
                    try
                    {
                        var r = service.consultaCEP(cep);
                        list[0] = r.uf;
                        list[1] = r.cidade;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Cep inválido", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return list;
        }

        public void LimparControles(Control control)
        {
            foreach (Control cont in control.Controls)
            {
                if (cont is TextBox)
                    (cont as TextBox).Text = "";
                if (cont is MaskedTextBox)
                    (cont as MaskedTextBox).Text = "";
                if (cont is DateTimePicker)
                {
                    ((DateTimePicker)cont).MinDate = new DateTime(1900, 1, 1);
                    ((DateTimePicker)cont).MaxDate = new DateTime(2100, 1, 1);
                    ((DateTimePicker)cont).Value = DateTime.Now.Date < ((DateTimePicker)cont).MinDate ? ((DateTimePicker)cont).MinDate : DateTime.Now.Date > ((DateTimePicker)cont).MaxDate ? ((DateTimePicker)cont).MaxDate : DateTime.Now.Date;
                    if (((DateTimePicker)cont).ShowCheckBox)
                        ((DateTimePicker)cont).Checked = false;
                }

                this.LimparControles(cont);
            }
        }

        public void ImpedeNumerosTextBox(KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !(e.KeyChar == (char)Keys.Back) && !(e.KeyChar == (char)Keys.Space))
                e.Handled = true;
        }

        public DateTime GetDataAtual()
        { // Pega a data atual direto do banco por motivo de segurança.

            DateTime dt = new DateTime();
            try
            {
                using (var cmd = conn.DbConnection().CreateCommand())
                {
                    cmd.CommandText = "SELECT DATE_FORMAT(now(), '%d/%m/%Y %H:%i:%s') AS data_atual FROM dual";
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        dt = Convert.ToDateTime(dr["data_atual"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atenção!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            return dt;
        }

        public bool GetCpf(string cpf)
        {
            bool bRetorno = false;
            try
            {
                using (var cmd = conn.DbConnection().CreateCommand())
                {
                    cmd.CommandText = $"SELECT COUNT(*) AS retorno FROM cadcliente WHERE cpf = '{cpf}';";
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        bRetorno = Convert.ToBoolean(dr["retorno"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atenção!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            return bRetorno;      
        }

        public void GetConsultaGrid(DataGridView dgv, DataTable dt)
        {
            dt.Clear();

            try
            {
                using (var cmd = conn.DbConnection().CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                                            id AS 'Id',
                                            DATE_FORMAT(data_cadastro, '%d/%m/%Y') AS 'Data do Cadastro',
                                            nome_completo AS 'Nome',
                                            cpf AS 'CPF',
                                            DATE_FORMAT(data_nascimento, '%d/%m/%Y') AS 'Data de Nascimento',
                                            CONCAT(logradouro, ', ',numero,', ',bairro) AS 'Endereço',
                                            cidade AS 'Cidade',
                                            estado AS 'Estado',
                                            cep AS 'CEP',
                                            telefone_fixo AS 'Telefone Fixo',
                                            celular AS 'Celular',
                                            email AS 'E-mail'
                                        FROM
                                            cadCliente
                                        ORDER BY id DESC";
                    da = new MySqlDataAdapter(cmd.CommandText, conn.DbConnection());
                    da.Fill(dt);
                    dgv.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atenção!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Salvar(ClienteDb cliente)
        {

            try
            {
                using (var cmd = conn.DbConnection().CreateCommand())
                {

                    if (cliente.Id == 0)
                    {
                        cmd.CommandText = @"INSERT INTO cadCliente 
                                                        (data_cadastro, nome_completo, cpf, 
                                                        data_nascimento, logradouro, numero, 
                                                        bairro, estado, cidade, cep,
                                                        telefone_fixo, celular, email) 
                                                        VALUES 
                                                        (@data_cadastro, @nome_completo, @cpf,
		                                                @data_nascimento, @logradouro, @numero, 
                                                        @bairro, @estado, @cidade, @cep, 
                                                        @telefone_fixo, @celular, @email)";

                        cmd.Parameters.AddWithValue("@data_cadastro", cliente.DataCadastro);
                    }
                    else
                    {
                        cmd.CommandText = @"UPDATE cadcliente SET 
	                                            nome_completo = @nome_completo, 
                                                cpf = @cpf, 
                                                data_nascimento = @data_nascimento,
                                                logradouro = @logradouro,
                                                numero = @numero,
                                                bairro = @bairro,
                                                estado = @estado,
                                                cidade = @cidade,                                                
                                                cep = @cep,
                                                telefone_fixo = @telefone_fixo,
                                                celular = @celular,
                                                email = @email
                                            WHERE id = @id";

                        cmd.Parameters.AddWithValue("@id", cliente.Id);
                    }

                    cmd.Parameters.AddWithValue("@nome_completo", cliente.Nome);
                    cmd.Parameters.AddWithValue("@cpf", cliente.Cpf);
                    cmd.Parameters.AddWithValue("@data_nascimento", cliente.DataNascimento);
                    cmd.Parameters.AddWithValue("@logradouro", cliente.Logradouro);
                    cmd.Parameters.AddWithValue("@numero", cliente.NumeroCasa);
                    cmd.Parameters.AddWithValue("@bairro", cliente.Bairro);
                    cmd.Parameters.AddWithValue("@estado", cliente.Estado);
                    cmd.Parameters.AddWithValue("@cidade", cliente.Cidade);                    
                    cmd.Parameters.AddWithValue("@cep", cliente.Cep);
                    cmd.Parameters.AddWithValue("@telefone_fixo", cliente.TelefoneFixo);
                    cmd.Parameters.AddWithValue("@celular", cliente.Celular);
                    cmd.Parameters.AddWithValue("@email", cliente.Email);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Registro salvo com sucesso!", "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atenção!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public void Excluir(string id)
        {
            try
            {
                using (var cmd = conn.DbConnection().CreateCommand())
                {
                    cmd.CommandText = $"DELETE FROM cadcliente WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Registro deletado com Sucesso!", "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atenção!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
