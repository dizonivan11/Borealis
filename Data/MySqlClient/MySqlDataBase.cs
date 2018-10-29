using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace Borealis.Data.MySqlClient {
    public class MySqlDataBase : DataSet {
        public Dictionary<string, MySqlDataAdapter> Adapters { get; set; }
        public ConflictOption Conflict { get; set; }
        public string HostName { get; set; }
        public string DatabaseName { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public static string ConnectionFormat = "Server={0};Database={1};Uid={2};Pwd={3};";

        public MySqlDataBase(string hostName, string databaseName, string userId = "root", string password = "")
            : base(databaseName) {

            // Creates a dictionary for registering table's adapter to update one by one in the reflect method.
            Adapters = new Dictionary<string, MySqlDataAdapter>();

            // Automatically set the conflict option to compare row version,
            // This option will find the first Timestamp (the data type not column name) in a column of a row.
            // Then compare it to the Timestamp of the new changes, if it is updated then the row will update.
            Conflict = ConflictOption.CompareRowVersion;

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
            Adapters.Add(tableName, adapter);
            adapter.FillSchema(this, SchemaType.Source);
            adapter.Fill(this, tableName);
        }

        public void Insert(
            string tableName,
            NameValueCollection columns) {

            if (!Adapters.ContainsKey(tableName)) return;
            DataRow newRow = Tables[tableName].NewRow();
            foreach (string columnName in columns.Keys) newRow[columnName] = columns[columnName];
            Tables[tableName].Rows.Add(newRow);
        }

        public DataRow[] Select(
            string tableName,
            string condition = "") {

            if (!Adapters.ContainsKey(tableName)) return null;
            return Tables[tableName].Select(condition);
        }

        public void Update(
            string tableName,
            NameValueCollection columns,
            string condition = "") {

            if (!Adapters.ContainsKey(tableName)) return;
            DataRow[] selectedRows = Tables[tableName].Select(condition);
            for (int i = 0; i < selectedRows.Length; i++) {
                int rowIndex = Tables[tableName].Rows.IndexOf(selectedRows[i]);
                foreach (string columnName in columns.Keys)
                    Tables[tableName].Rows[rowIndex][columnName] = columns[columnName];
            }
        }

        public void Delete(
            string tableName,
            string condition = "") {

            if (!Adapters.ContainsKey(tableName)) return;
            DataRow[] selectedRows = Tables[tableName].Select(condition);
            for (int i = 0; i < selectedRows.Length; i++)
                selectedRows[i].Delete();
        }

        public void Reflect(string tableName) {
            if (!Adapters.ContainsKey(tableName)) return;
            MySqlCommandBuilder cmdBuilder = new MySqlCommandBuilder(Adapters[tableName]);
            cmdBuilder.ConflictOption = Conflict;
            Adapters[tableName].Update(this, tableName);
        }
    }
}
