# Borealis

>	A .NET Library which contains a method for basic CRUD (Create, Retrieve, Update and Delete) Operations. It can be used with SQL.

> An example usage of the library

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Borealis.Data.SqlClient;

namespace BorealisDataTest {
    public partial class Form1 : Form {
        SqlDataManager sqlDM = new SqlDataManager("localhost\\sqlexpress", "db1");

        public Form1() {
            InitializeComponent();
        }

        private void ins_Click(object sender, EventArgs e) {
            Dictionary<string, object> row = new Dictionary<string, object>();
            row.Add("first", fn.Text);
            row.Add("middle", mn.Text);
            row.Add("last", ln.Text);
            row.Add("address", adr.Text);
            sqlDM.Insert("tbl1", row);
            MessageBox.Show("Insert successful");
        }

        private void upt_Click(object sender, EventArgs e) {
            Dictionary<string, object> row = new Dictionary<string, object>();
            if (fn.Text != string.Empty) row.Add("first", fn.Text);
            if (mn.Text != string.Empty) row.Add("middle", mn.Text);
            if (ln.Text != string.Empty) row.Add("last", ln.Text);
            if (adr.Text != string.Empty) row.Add("address", adr.Text);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", id.Text);
            sqlDM.Update("tbl1", row, "id=@id", parameters);
            MessageBox.Show("Update successful");
        }

        private void sel_Click(object sender, EventArgs e) {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", id.Text);
            List<Dictionary<string, object>> selectedRows = sqlDM.Select("tbl1", "id=@id", parameters, "first", "middle", "last", "address");
            if (selectedRows.Count > 0) {
                fn.Text = selectedRows[0]["first"].ToString();
                mn.Text = selectedRows[0]["middle"].ToString();
                ln.Text = selectedRows[0]["last"].ToString();
                adr.Text = selectedRows[0]["address"].ToString();
                MessageBox.Show("Select successful");
            }
            else MessageBox.Show("Select unsuccessful");
        }

        private void del_Click(object sender, EventArgs e) {
            if (id.Text != string.Empty) {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("@id", id.Text);
                sqlDM.Delete("tbl1", "id=@id", parameters);
            }
            else {
                sqlDM.Delete("tbl1");
            }
            MessageBox.Show("Delete successful");
        }
    }
}
```