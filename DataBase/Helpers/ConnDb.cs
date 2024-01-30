using MySql.Data.MySqlClient;

namespace DataBase
{
    public class ConnDb
    {
        private const string connString = "server=localhost;user id=root;password=root;database=CadastroCliente";

        MySqlConnection connection;

        public MySqlConnection DbConnection()
        {
            connection = new MySqlConnection(connString);
            connection.Open();
            return connection;
        }        
    }
}
