using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Borealis.Data.SqlClient
{
    public class SqlDataManager
    {
        public string HostName { get; set; }
        public string DatabaseName { get; set; }
        public static string ConnectionFormat = "Server={0};Database={1};Trusted_connection=true;";

        public SqlDataManager(string hostName, string databaseName) {
            HostName = hostName;
            DatabaseName = databaseName;
        }

        public SqlConnection CreateNewConnection() {
            return new SqlConnection(string.Format(ConnectionFormat, HostName, DatabaseName));
        }

        public string ParameterName(string columnName) {
            for (int i = 0; i < columnName.Length; i++)
                if (!char.IsLetterOrDigit(columnName[i]))
                    columnName = columnName.Remove(i, 1);

            return string.Format("@{0}", columnName);
        }

        public void Insert(
            string tableName,
            Dictionary<string, object> row) {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0}(", tableName);
            bool firstColumn = true;
            foreach (string columnName in row.Keys) {
                if (!firstColumn) sb.Append(", ");
                sb.AppendFormat("[{0}]",columnName);
                firstColumn = false;
            }
            sb.AppendFormat(") VALUES(", tableName);
            bool firstValue = true;
            foreach (string columnName in row.Keys) {
                if (!firstValue) sb.Append(", ");
                sb.Append(ParameterName(columnName));
                firstValue = false;
            }
            sb.Append(");");
            SqlCommand cmd = new SqlCommand() {
                CommandText = sb.ToString(),
                Connection = CreateNewConnection()
            };
            foreach (string columnName in row.Keys)
                cmd.Parameters.AddWithValue(ParameterName(columnName), row[columnName]);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public List<Dictionary<string, object>> Select(
            string tableName,
            string condition = "",
            Dictionary<string, object> parameters = null,
            params string[] columnsToSelect) {

            List<Dictionary<string, object>> selectedRows = new List<Dictionary<string, object>>();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            if (columnsToSelect.Length > 0) {
                for (int i = 0; i < columnsToSelect.Length; i++) {
                    if (i > 0) sb.Append(", ");
                    sb.AppendFormat("[{0}]", columnsToSelect[i]);
                }
            } else sb.Append("*");
            sb.AppendFormat(" FROM {0}", tableName);
            if (condition != string.Empty) sb.AppendFormat(" WHERE {0}", condition);
            sb.Append(";");
            SqlCommand cmd = new SqlCommand() {
                CommandText = sb.ToString(),
                Connection = CreateNewConnection()
            };
            if (parameters != null)
                foreach (string parameterName in parameters.Keys)
                    cmd.Parameters.AddWithValue(parameterName, parameters[parameterName]);
            cmd.Connection.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) {
                Dictionary<string, object> row = new Dictionary<string, object>();
                for (int i = 0; i < dr.FieldCount; i++) row.Add(dr.GetName(i), dr[i]);
                selectedRows.Add(row);
            }
            cmd.Connection.Close();
            return selectedRows;
        }

        public void Update(
            string tableName,
            Dictionary<string, object> row,
            string condition = "",
            Dictionary<string, object> parameters = null) {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("UPDATE {0} SET ", tableName);
            bool firstColumn = true;
            foreach (string columnName in row.Keys) {
                if (!firstColumn) sb.Append(", ");
                sb.AppendFormat("[{0}]={1}", columnName, ParameterName(columnName));
                firstColumn = false;
            }
            if (condition != string.Empty) sb.AppendFormat(" WHERE {0}", condition);
            sb.Append(";");
            SqlCommand cmd = new SqlCommand() {
                CommandText = sb.ToString(),
                Connection = CreateNewConnection()
            };
            foreach (string columnName in row.Keys)
                cmd.Parameters.AddWithValue(ParameterName(columnName), row[columnName]);
            if (parameters != null)
                foreach (string parameterName in parameters.Keys)
                    cmd.Parameters.AddWithValue(parameterName, parameters[parameterName]);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        public void Delete(
            string tableName,
            string condition = "",
            Dictionary<string, object> parameters = null) {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("DELETE FROM {0}", tableName);
            if (condition != string.Empty) sb.AppendFormat(" WHERE {0}", condition);
            sb.Append(";");
            SqlCommand cmd = new SqlCommand() {
                CommandText = sb.ToString(),
                Connection = CreateNewConnection()
            };
            if (parameters != null)
                foreach (string parameterName in parameters.Keys)
                    cmd.Parameters.AddWithValue(parameterName, parameters[parameterName]);
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }
    }
}
