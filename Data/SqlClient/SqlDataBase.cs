using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Borealis.Data.SqlClient {
    public class SqlDataBase : DataSet {
        public Dictionary<string, SqlDataAdapter> Adapters { get; set; }
        public ConflictOption Conflict { get; set; }
        public string HostName { get; set; }
        public string DatabaseName { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public static string TrustedConnectionFormat = "Server={0};Database={1};Trusted_Connection=true;";
        public static string ConnectionFormat = "Server={0};Database={1};User Id={2};Password={3};";
        
        public SqlDataBase(string hostName, string databaseName, string userId = "", string password = "")
            : base(databaseName) {

            // Creates a dictionary for registering table's adapter to update one by one in the reflect method.
            Adapters = new Dictionary<string, SqlDataAdapter>();

            // Automatically set the conflict option to compare row version,
            // This option will find the first Timestamp (the data type not column name) in a column of a row.
            // Then compare it to the Timestamp of the new changes, if it is updated then the row will update.
            Conflict = ConflictOption.CompareRowVersion;

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

        public void LoadTable(string tableName) {
            SqlDataAdapter adapter = new SqlDataAdapter(string.Format("SELECT * FROM {0};", tableName), CreateNewConnection());
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
            SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(Adapters[tableName]);
            cmdBuilder.ConflictOption = Conflict;
            Adapters[tableName].Update(this, tableName);
        }
    }
}
