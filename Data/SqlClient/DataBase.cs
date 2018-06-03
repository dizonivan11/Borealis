using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Borealis.Data.SqlClient {
    public class DataBase : DataSet {
        public string HostName { get; set; }
        public string DatabaseName { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public static string TrustedConnectionFormat = "Server={0};Database={1};Trusted_Connection=true;";
        public static string ConnectionFormat = "Server={0};Database={1};User Id={2};Password={3};";
        
        public DataBase(string hostName, string databaseName, string userId = "", string password = "") {
            HostName = hostName;
            DatabaseName = databaseName;
            UserId = userId;
            Password = password;
        }

        public SqlConnection CreateNewConnection() {
            if (UserId == string.Empty || Password == string.Empty)
                return new SqlConnection(string.Format(TrustedConnectionFormat, HostName, DatabaseName));
            else
                return new SqlConnection(string.Format(ConnectionFormat, HostName, DatabaseName, UserId, Password));
        }

        public void Insert(
            string tableName,
            NameValueCollection columns) {

            DataRow newRow = Tables[tableName].NewRow();
            foreach (string columnName in columns.Keys) newRow[columnName] = columns[columnName];
            Tables[tableName].Rows.Add(newRow);
        }

        public List<NameValueCollection> Select(
            string tableName,
            string condition = "") {

            List<NameValueCollection> rows = new List<NameValueCollection>();
            DataRow[] selectedRows = Tables[tableName].Select(condition);
            return rows;
        }

        public void Update(
            string tableName,
            NameValueCollection columns,
            string condition = "") {

            DataRow[] selectedRows = Tables[tableName].Select(condition);
            for (int i = 0; i < selectedRows.Length; i++)
                foreach (string columnName in columns.Keys) selectedRows[i][columnName] = columns[columnName];
        }

        public void Delete(
            string tableName,
            string condition = "") {
            
            DataRow[] selectedRows = Tables[tableName].Select(condition);
            for (int i = 0; i < selectedRows.Length; i++)
                selectedRows[i].Delete();
        }

        public void Reflect(string tableName) {
            SqlDataAdapter adapter = new SqlDataAdapter(string.Format("SELECT * FROM {0};", tableName), CreateNewConnection());
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
        }
    }
}
