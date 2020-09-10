using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CS322_PZ_MilanRadeljic2865
{
    public partial class Form1 : Form
    {

        string connectionString;
        SqlConnection connection;
        public Form1()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["CS322_PZ_MilanRadeljic2865.Properties.Settings.LokalConnectionString"].ToString();
            populateTableList();
            listTables.SelectedIndex = 0;
            populateItems();
            populateOrder(1);
        }


        private void populateTableList()
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("Select MAX(id) from sitting_table", connection);
                object result = cmd.ExecuteScalar();
                int tableCount = Convert.ToInt32(result);
                listTables.Items.Clear();
                for (int i = 0; i < tableCount; i++)
                {
                    listTables.Items.Add(i + 1);

                }
            }

        }



        private void populateOrder(int tableID)
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT mi.name Item, mi.price Price, SUM(o.quantity) Quantity, SUM(o.quantity) * mi.price Total FROM orderr o INNER JOIN menu_item mi ON mi.id = o.menu_item_id WHERE o.table_id = @tableID GROUP BY o.table_id, mi.name, mi.price", connection);
                //SqlCommand cmd = new SqlCommand("SELECT o.id order_id, mi.name item_name, mi.price item_price, SUM(omi.quantity) item_quantity, SUM(omi.quantity) * mi.price item_value FROM orderr o INNER JOIN orderr_menu_item omi ON omi.orderr_id = o.id INNER JOIN menu_item mi ON mi.id = omi.menu_item_id WHERE o.sitting_table_id = @tableID GROUP BY o.id, o.sitting_table_id, omi.id, mi.name, mi.price ORDER BY o.id, o.sitting_table_id, mi.name, mi.price", connection);
                cmd.Parameters.AddWithValue("@tableID", tableID);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    var tb = new DataTable();
                    tb.Load(dr);
                    dataGridOrder.DataSource = tb;
                }

            }

        }

        private void populateItems()
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM menu_item", connection);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    var tb = new DataTable();
                    tb.Load(dr);
                    listItems.DisplayMember = "name";
                    listItems.ValueMember = "id";
                    listItems.DataSource = tb;
                    dataGridItems.DataSource = tb;
                }
            }

        }
        private void populateListReceipts()
        {

            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                   SqlCommand cmd1 = new SqlCommand("SELECT id FROM receipt", connection);
                using (SqlDataReader dr1 = cmd1.ExecuteReader())
                {
                    var tb1 = new DataTable();
                    tb1.Load(dr1);
                    listReceipts.DisplayMember = "id";
                    listReceipts.ValueMember = "id";
                    listReceipts.DataSource = tb1;
                }
            }

        }

        private void populateReceipts()
        {

            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT mi.name Item, mi.price Price, SUM(o.quantity) Quantity, SUM(o.quantity) * mi.price Total FROM orderr o INNER JOIN menu_item mi ON mi.id = o.menu_item_id WHERE o.receipt_id = @receiptID GROUP BY o.table_id, mi.name, mi.price", connection);
                cmd.Parameters.AddWithValue("@receiptID", listReceipts.SelectedValue);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    var tb = new DataTable();
                    tb.Load(dr);
                    dataGridReceipts.DataSource = tb;
                }
            }

        }

        private void TotalPriceForSittingTable(int tableID)
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT SUM(Total) from (SELECT SUM(o.quantity) * mi.price Total FROM orderr o INNER JOIN menu_item mi ON mi.id = o.menu_item_id WHERE o.table_id = @tableID GROUP BY mi.price) as Total_Value", connection);
                cmd.Parameters.AddWithValue("@tableID", tableID);
                object result = cmd.ExecuteScalar();
                totalValueTxt.Text = Convert.ToString(result);
            }

        }

        private void plusBtn_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("Insert into sitting_table(id) values (@tableID)", connection);
                int tableCount = listTables.Items.Count;
                cmd.Parameters.AddWithValue("@tableID", tableCount+1);
                cmd.ExecuteNonQuery();
                populateTableList();
            }

        }

        private void minusBtn_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                int tableCount = listTables.Items.Count;
                if (tableCount > 1)
                {
                    SqlCommand cmd1 = new SqlCommand("Select * from orderr where table_id = @tableID", connection);
                    cmd1.Parameters.AddWithValue("@tableID", tableCount);
                    object result = cmd1.ExecuteScalar();
                    if (result == null)
                    {
                        SqlCommand cmd = new SqlCommand("Delete from sitting_table where id = @tableID", connection);
                        cmd.Parameters.AddWithValue("@tableID", tableCount);
                        cmd.ExecuteNonQuery();
                        populateTableList();
                    }
                    else
                    {
                        MessageBox.Show("There is an active order for the table", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void addItemOrder(int itemID, int tableID, int amount)
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO orderr (table_id, menu_item_id, quantity) VALUES (@tableID, @itemID, @amount); ", connection);
                cmd.Parameters.AddWithValue("@tableID", tableID);
                cmd.Parameters.AddWithValue("@itemID", itemID);
                cmd.Parameters.AddWithValue("@amount", amount);
                try
                {
                    cmd.ExecuteNonQuery();

                }
                catch
                {

                }
            }

        }

        private void addItemBtn_Click(object sender, EventArgs e)
        {

            addItemOrder(Convert.ToInt32(listItems.SelectedValue.ToString()), Convert.ToInt32(listTables.SelectedIndex + 1), Convert.ToInt32(amountNumeric.Value));
            amountNumeric.Value = 1;
            populateOrder(listTables.SelectedIndex + 1);
            TotalPriceForSittingTable(listTables.SelectedIndex + 1);
        }

        private void checkoutBtn_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection)
            {
                if (totalValueTxt.Text != "")
                {
                    //pravi se racun
                    SqlCommand cmd = new SqlCommand("insert into receipt(total_value) values(@totalValue)", connection);
                    cmd.Parameters.AddWithValue("@totalValue", Convert.ToDecimal(totalValueTxt.Text));
                    cmd.ExecuteNonQuery();

                    //izvlaci se ID napravljenog racuna
                    int receiptID;
                    SqlCommand cmd2 = new SqlCommand("Select max(id) from receipt", connection);
                    object result = cmd2.ExecuteScalar();
                    receiptID = Convert.ToInt32(result);

                    //menja se Foreign Key vrednost na orderu za ID racuna i sitting table se stavlja na 0

                    SqlCommand cmd3 = new SqlCommand("UPDATE orderr SET table_id = 0, receipt_id = @receiptID WHERE table_id = @tableID;", connection);
                    cmd3.Parameters.AddWithValue("@receiptID", receiptID);
                    cmd3.Parameters.AddWithValue("@tableID", listTables.SelectedIndex + 1);

                    cmd3.ExecuteNonQuery();

                    MessageBox.Show("Total price: " + totalValueTxt.Text, "Receipt ID: " + receiptID);

                    totalValueTxt.Text = "";
                    populateListReceipts();
                    populateReceipts();
                    populateOrder(listTables.SelectedIndex + 1);
                }


            }
        }
        private void showReceiptValue()
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("Select total_value from receipt where id = @receiptID", connection);
                cmd.Parameters.AddWithValue("@receiptID", listReceipts.SelectedValue);
                object result = cmd.ExecuteScalar();
                receiptValue.Text = Convert.ToString(result);
            }
        }
        private void listReceipts_SelectedIndexChanged(object sender, EventArgs e)
        {
           populateReceipts();
           showReceiptValue();
        }

        private void listTables_SelectedIndexChanged(object sender, EventArgs e)
        {

            TotalPriceForSittingTable(listTables.SelectedIndex + 1);
            populateOrder(listTables.SelectedIndex + 1);
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM orderr where table_id = @tableID", connection);
                cmd.Parameters.AddWithValue("@tableID", listTables.SelectedIndex+1);
                try
                {
                    cmd.ExecuteNonQuery();
                    populateOrder(listTables.SelectedIndex + 1);

                }
                catch
                {

                }
            }

        }

        private void createItemBtn_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                if (newItemNameTxt.Text == "")
                {
                    MessageBox.Show("Insert name", "Incorrect data", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                else
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO menu_item (name, price) VALUES (@name, @price)", connection);
                    cmd.Parameters.AddWithValue("@name", newItemNameTxt.Text.ToString());
                    cmd.Parameters.AddWithValue("@price", Convert.ToDouble(priceNewNum.Value));
                    try
                    {
                        newItemNameTxt.Text = "";
                        priceNewNum.Value = 1;
                        cmd.ExecuteNonQuery();
                        populateItems();

                    }
                    catch
                    {

                    }
                }
            }
        }

        private void deleteItemBtn_Click(object sender, EventArgs e)
        {

            connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection)
            {
                SqlCommand cmd = new SqlCommand("Delete from orderr where menu_item_id = @idValue; Delete from menu_item where id = @idValue", connection);
                cmd.Parameters.AddWithValue("@idValue", Convert.ToInt32(idDelNum.Value));
                try
                {
                    cmd.ExecuteNonQuery();
                    idDelNum.Value = 1;
                    populateItems();

                }
                catch
                {

                }
            }
        }

        private void updateItemBtn_Click(object sender, EventArgs e)
        {

            connection = new SqlConnection(connectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("update menu_item set name = @newName, price = @newPrice where id=@idValue", connection);
                cmd.Parameters.AddWithValue("@newName", updateItemTxt.Text.ToString());
                cmd.Parameters.AddWithValue("@newPrice", priceUpdateNum.Value);
                cmd.Parameters.AddWithValue("@idValue", Convert.ToInt32(idUpdateNum.Value));
                try
                {
                    cmd.ExecuteNonQuery();
                    idUpdateNum.Value = 1;
                    priceUpdateNum.Value = 1;
                    populateItems();
                }
                catch
                {

                }
            }
        }

        private void ItemGridCellClick(object sender, DataGridViewCellEventArgs e)
        {
            try{
                if (dataGridItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    int idItem = Convert.ToInt32(dataGridItems.Rows[e.RowIndex].Cells[0].Value.ToString());
                    SqlCommand cmd = new SqlCommand("select * from menu_item where id = @idValue", connection);
                    cmd.Parameters.AddWithValue("@idValue", idItem);
                    try{
                        using (SqlDataReader dr = cmd.ExecuteReader()){
                            var tb = new DataTable();
                            tb.Load(dr);
                            string name = Convert.ToString(tb.Rows[0][1]);
                            decimal price = Convert.ToDecimal(tb.Rows[0][2]);
                            //promena update
                            updateItemTxt.Text = name;
                            priceUpdateNum.Value = price;
                            idUpdateNum.Value = Convert.ToDecimal(idItem);
                            //promena del
                            idDelNum.Value = Convert.ToDecimal(idItem);
                        }
                    }
                    catch (System.IndexOutOfRangeException ex)
                    {

                    }
                    connection.Close();
                }

            }
            catch (System.ArgumentOutOfRangeException ex)
            {

            }
            catch (System.IndexOutOfRangeException ex)
            {

            }
        }

        private void removeBtn_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            using (connection)
            {
                SqlCommand cmd = new SqlCommand("Delete from orderr where receipt_id = @idValue; Delete from receipt where id = @idValue", connection);
                cmd.Parameters.AddWithValue("@idValue", listReceipts.SelectedValue);
                try
                {
                    cmd.ExecuteNonQuery();
                    populateReceipts();
                    populateListReceipts();

                }
                catch
                {

                }
            }

        }
    }
}
