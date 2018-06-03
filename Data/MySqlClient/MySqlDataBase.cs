using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace Borealis.Data.MySqlClient {
    public class MySqlDataBase : DataSet {
        public string HostName { get; set; }
        public string DatabaseName { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public static string ConnectionFormat = "Server={0};Database={1};Uid={2};Pwd={3};";

        public MySqlDataBase(string hostName, string databaseName, string userId = "root", string password = "")
            : base(databaseName) {

            HostName = hostName;
            DatabaseName = databaseName;
            UserId = userId;
            Password = password;
        }

        public MySqlConnection CreateNewConnection() {
            return new MySqlConnection(string.Format(ConnectionFormat, HostName, DatabaseName, UserId, Password));
        }

        public void LoadTable(string tableName) {
            MySqlDataAdapter adapter = new MySqlDataAdapter(string.Format("SELECT * FROM {0};", tableName), CreateNewConnection());
            adapter.FillSchema(this, SchemaType.Source);
            adapter.Fill(this, tableName);
        }

        public void Insert(
            string tableName,
            NameValueCollection columns) {

            DataRow newRow = Tables[tableName].NewRow();
            foreach (string columnName in columns.Keys) newRow[columnName] = columns[columnName];
            Tables[tableName].Rows.Add(newRow);
        }

        public DataRow[] Select(
            string tableName,
            string condition = "") {
            
            return Tables[tableName].Select(condition);
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
            MySqlDataAdapter adapter = new MySqlDataAdapter(string.Format("SELECT * FROM {0};", tableName), CreateNewConnection());
            new MySqlCommandBuilder(adapter);
            adapter.Update(this, tableName);
        }
    }
}
