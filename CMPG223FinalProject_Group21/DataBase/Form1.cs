using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace DataBase
{
    public partial class Form1 : Form
    {
        private readonly string conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Connor\OneDrive\Documents\CMPG223\Project\DataBase\Default.mdf;Integrated Security=True";
        private List<string> itemNames = new List<string>();
        private List<bool> itemEmphasis = new List<bool>();

        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private DataSet dataSet;
        private SqlCommand comm;
        private SqlDataReader datareader;

        private string updateReport = "";
        private string deleteReport = "";

        public Form1()
        {
            InitializeComponent();

            // Initialize the ListBox with 10 items and their emphasis status
            itemNames.AddRange(new string[]
            {
                "Holsters: Carrying devices for handguns, available in various styles and materials.",
                "Cleaning Kits: Tools and solvents for firearm maintenance and cleaning.",
                "Magazines: Detachable firearm components that hold and feed ammunition.",
                "Sights and Optics: Aiming aids such as scopes, red dot sights, and iron sights.",
                "Gun Cases: Protective containers for transporting firearms safely.",
                "Suppressors: Devices that reduce the noise and recoil of a firearm.",
                "Slings: Straps or harnesses for carrying rifles or shotguns comfortably.",
                "Gun Safes: Secure storage units to store firearms safely.",
                "Tactical Lights: Attachable lights for illuminating targets in low-light conditions."
            });
            itemEmphasis.AddRange(new bool[] { true, false, false, false, false, false, false, false, false, false });

            // Populate the ListBox
            UpdateListBox();
        }

        private void UpdateListBox()
        {
            lsbAccDescription.Items.Clear();
            for (int i = 0; i < itemNames.Count; i++)
            {
                // Emphasize items #2 and #3 by prefixing them with '*'
                string displayText = (itemEmphasis[i] && (i == 1 || i == 2)) ? $"* {itemNames[i]}" : itemNames[i];
                lsbAccDescription.Items.Add(displayText);
            }
        }

        


        private void UpdateDataGridView(DataGridView dataGridView, string query, string parameterName, string parameterValue)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(conStr))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(parameterValue))
                        {
                            command.Parameters.AddWithValue(parameterName, parameterValue);
                        }

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnExit_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void btnAmmoUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                updateReport += "\n" + "Ammunition details Changed \n" + "ID: " + AmmoIDTB.Text + "\n" + "Stock: " + txtAmmoStock.Text + "\n" + "Price: " + txtAmmoPrice.Text;

                connection.Open();
                string sql = "UPDATE AMMUNITION SET Stock = " + txtAmmoStock.Text + " , Ammunition_Price = " + txtAmmoPrice.Text +" WHERE Ammunition_ID ="+AmmoIDTB.Text;
                comm = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter();
                adapter.UpdateCommand = comm;
                adapter.UpdateCommand.ExecuteNonQuery();

                sql = "SELECT * FROM AMMUNITION";
                comm = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter();
                adapter.SelectCommand = comm;
                DataSet set = new DataSet();
                adapter.Fill(set, "AMMUNITION");

                dgvAmmunition.DataSource = set;
                dgvAmmunition.DataMember = "AMMUNITION";

                connection.Close();

                MessageBox.Show("The update has been successful");
            }
            catch(Exception)
            {
                MessageBox.Show("There was a error when updating the ammunition database");
            }
        }

        private void btnAccUpdate_Click(object sender, EventArgs e)
        {
            updateReport += "\n" + "Accessory Update:\n" + "Accessory Type: " + AccessoryIDtxt.Text + "\n" + "Accessory stock: " + txtAccStock.Text + "\n" + "Accessory Price: " + txtAccPrice.Text;
            connection.Open();
            string sql = "UPDATE ACCESSORIES SET Stock = " + txtAccStock.Text + ",Item_Cost = " + txtAccPrice.Text+"WHERE Accessory_ID = "+AccessoryIDtxt.Text;
            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();

            adapter.UpdateCommand = comm;
            adapter.UpdateCommand.ExecuteNonQuery();

            sql = "SELECT * FROM ACCESSORIES";
            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = comm;
            DataSet set = new DataSet();
            adapter.Fill(set, "ACCESSORIES");

            dgvAccessories.DataSource = set;
            dgvAccessories.DataMember = "ACCESSORIES";

            connection.Close();
        }

        private void btnAmmoDelete_Click(object sender, EventArgs e)
        {
            deleteReport += "\n" + "Ammunition deleted \n" + "ID: " + AmmoIDTB.Text + "\n" + "Stock: " + txtAmmoStock.Text + "\n" + "  Price: " + txtAmmoPrice.Text;
            connection.Open();
            string sql = "DELETE  FROM AMMUNITION WHERE Ammunition_ID = " + AmmoIDTB.Text;
            comm = new SqlCommand(sql, connection);
            comm.ExecuteNonQuery();

            sql = "SELECT * FROM AMMUNITION";
            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = comm;
            DataSet set = new DataSet();
            adapter.Fill(set, "AMMUNITION");

            dgvAmmunition.DataSource = set;
            dgvAmmunition.DataMember = "AMMUNITION";


            connection.Close();
            MessageBox.Show("The data has been deleted");
        }

        private void btnAccDelete_Click_1(object sender, EventArgs e)
        {
            DeleteItem("Accessories", "Accessory_ID", AccessoryIDtxt.Text, dgvAccessories);
        }

        private void DeleteItem(string tableName, string typeColumnName, string itemType, DataGridView dataGridView)
        {
            if (string.IsNullOrWhiteSpace(itemType))
            {
                MessageBox.Show("Please select an item to delete.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(conStr))
            {
                connection.Open();

                string deleteQuery = $"DELETE FROM {tableName} WHERE {typeColumnName} = @ItemTypeToDelete";
                using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@ItemTypeToDelete", itemType);
                    int rowsAffected = deleteCommand.ExecuteNonQuery();
                    Console.WriteLine($"Deleted {rowsAffected} rows from {tableName} table.");

                    // Refresh the DataGridView after deletion
                    string selectQuery = $"SELECT * FROM {tableName} WHERE {typeColumnName} = @SelectedItemType";
                    UpdateDataGridView(dataGridView, selectQuery, "@SelectedItemType", itemType);
                }

                connection.Close();
            }
        }
        


        // Event handler for updating or inserting customer data
        private void btnCusUpdate_Click(object sender, EventArgs e)
        {
            updateReport += "\n" + "Customer updated: \n" + "Customer Name: " + txtFName.Text + " " + txtLName.Text + "\n" + "Customer address: " + txtCusAdress.Text + "\n" + "Customer email: " + txtCusEmail.Text + "\n" + "Customer Cell: " + txtCSel.Text;

            connection.Open();

            string sql = "UPDATE CUSTOMERS SET Customer_FName ='" + txtFName.Text + "',Customer_LName = '" + txtLName.Text + "',Customer_CellNum = " + txtCSel.Text + ",Customer_Address = '" + txtCusAdress.Text + "',Customer_Email ='" + txtCusEmail.Text + "' WHERE Customer_ID = " + txtCustomerID.Text;

            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();
            adapter.UpdateCommand = comm;
            adapter.UpdateCommand.ExecuteNonQuery();

            sql = "SELECT * FROM CUSTOMERS";
            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = comm;
            DataSet set = new DataSet();
            adapter.Fill(set, "CUSTOMERS");

            dgvCustomers.DataSource = set;
            dgvCustomers.DataMember = "CUSTOMERS";

            connection.Close();
        }




        // Refresh DataGridView for Customers
        private void RefreshCustomerDataGridView()
        {
            string selectQuery = "SELECT * FROM CUSTOMERS";
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dgvCustomers.DataSource = dataTable;
                }
            }
        }

        private void txtFName_TextChanged(object sender, EventArgs e)
        {
            string firstName = txtFName.Text.Trim();
            string lastName = txtLName.Text.Trim();

            try
            {
                using (SqlConnection connection = new SqlConnection(conStr))
                {
                    connection.Open();

                    // Construct the query to filter based on both first and last names
                    string selectQuery = "SELECT * FROM Customers WHERE 1 = 1"; // Start with a true condition

                    if (!string.IsNullOrEmpty(firstName))
                    {
                        selectQuery += " AND Customer_FName LIKE @FirstName";
                    }

                    if (!string.IsNullOrEmpty(lastName))
                    {
                        selectQuery += " AND Customer_LName LIKE @LastName";
                    }

                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        if (!string.IsNullOrEmpty(firstName))
                        {
                            command.Parameters.AddWithValue("@FirstName", $"%{firstName}%");
                        }

                        if (!string.IsNullOrEmpty(lastName))
                        {
                            command.Parameters.AddWithValue("@LastName", $"%{lastName}%");
                        }

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvCustomers.DataSource = dataTable;
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Event handler for txtLName TextChanged event
        private void txtLName_TextChanged(object sender, EventArgs e)
        {
            // Call the same logic as in txtFName_TextChanged
            txtFName_TextChanged(sender, e);
        }
        private void btnCusDelete_Click(object sender, EventArgs e)
        {
            try
            {
                deleteReport += "\n" + "Customer Deleted: \n" + "Customer Name: " + txtFName.Text + " " + txtLName.Text + "\n" + "Customer address: " + txtCusAdress.Text + "\n" + "Customer email: " + txtCusEmail.Text + "\n" + "Customer Cell: " + txtCSel.Text;

                connection.Open();

                string sql = "DELETE FROM Customers WHERE Customer_ID = " + txtCustomerID.Text;
                comm = new SqlCommand(sql, connection);
                //comm.Parameters.AddWithValue("@Customer_ID", LicenseCustomer_Remove.Text);
                comm.ExecuteNonQuery();

                sql = "SELECT * FROM CUSTOMERS";
                comm = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter();
                adapter.SelectCommand = comm;
                DataSet set = new DataSet();
                adapter.Fill(set, "CUSTOMERS");

                dgvCustomers.DataSource = set;
                dgvCustomers.DataMember = "CUSTOMERS";

                connection.Close();

                MessageBox.Show("Deleted!");
            }
            catch(Exception)
            {
                MessageBox.Show("There was a erro when deleting from the tables");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connection = new SqlConnection(conStr);

            connection.Open();
            string sql = "SELECT * FROM AMMUNITION";
            comm = new SqlCommand(sql,connection);
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = comm;
            DataSet set = new DataSet();
            adapter.Fill(set, "AMMUNITION");

            dgvAmmunition.DataSource = set;
            dgvAmmunition.DataMember = "AMMUNITION";

            sql = "SELECT * FROM FIREARMS";
            comm = new SqlCommand(sql, connection);
            adapter.SelectCommand = comm;
            adapter.Fill(set, "FIREARMS");

            dgvFirearms.DataSource = set;
            dgvFirearms.DataMember = "FIREARMS";

            sql = "SELECT * FROM ACCESSORIES";
            comm = new SqlCommand(sql, connection);
            adapter.SelectCommand = comm;
            adapter.Fill(set, "ACCESSORIES");

            dgvAccessories.DataSource = set;
            dgvAccessories.DataMember = "ACCESSORIES";

            sql = "SELECT * FROM CUSTOMERS";
            comm = new SqlCommand(sql, connection);
            adapter.SelectCommand = comm;
            adapter.Fill(set, "CUSTOMERS");

            dgvCustomers.DataSource = set;
            dgvCustomers.DataMember = "CUSTOMERS";

            sql = "SELECT * FROM LICENSE";
            comm = new SqlCommand(sql, connection);
            adapter.SelectCommand = comm;
            adapter.Fill(set, "LICENSES");

            dgvLicense.DataSource = set;
            dgvLicense.DataMember = "LICENSES";



            
            connection.Close();

        }

        private void comboBoxFirearmType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Filter the DataGridView based on the selected firearm type
            string selectedFirearmType = cbFireType.SelectedItem.ToString();
            DataView dv = new DataView(dataSet.Tables["Firearms"]);
            dv.RowFilter = $"FirearmType = '{selectedFirearmType}'";
            dgvFirearms.DataSource = dv;
        }

        private void PopulateComboBox()
        {
            // Add firearm types to the ComboBox
            cbFireType.Items.AddRange(new string[] { "Rifle", "Pistols", "Shotguns", "Revolvers", "Semi-Rifle" });
        }

        private void btnFireUpdate_Click(object sender, EventArgs e)
        {
            updateReport += "\n"+"Firearms Update:\n" + "Type:" + cbFireType.Text + "\n" + "Firearm name: " + txtFireName.Text + "\n" + "Firearm number: " + txtFireNumber.Text + "\n" + "Firearms in stock: " + txtFireStock.Text;
            connection.Open();

            string sql = "UPDATE FIREARMS SET Firearm_Type ='" + cbFireType.Text + "', Stock = " + txtFireStock.Text + ", Firearm_Name = '" + txtFireName.Text + "', Firearm_no = " + txtFireNumber.Text+ "WHERE Firearm_ID ="+txtFireID.Text;
            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();
            
            adapter.UpdateCommand = comm;
            adapter.UpdateCommand.ExecuteNonQuery();

           
             sql = "SELECT * FROM FIREARMS";
            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = comm;
            DataSet set = new DataSet();
            adapter.Fill(set, "FIREARMS");

            dgvFirearms.DataSource = set;
            dgvFirearms.DataMember = "FIREARMS";

            connection.Close();
        }


        private void LoadLicenseTypes()
        {
            try
            {
                // Clear the ComboBox first
                cbLicenseType.Items.Clear();

                // Open the database connection
                connection.Open();

                // Create a SQL command to fetch distinct license types from the database
                string query = "SELECT DISTINCT Firearm_Licenses FROM Licenses";
                comm = new SqlCommand(query, connection);

                // Execute the command and read the results
                datareader = comm.ExecuteReader();

                string output = "";
                while (datareader.Read())
                {
                    output = datareader.GetValue(0).ToString();
                    cbLicenseType.Items.Add(output);
                }

                // Close the database connection
                connection.Close();
            }catch(Exception)
            {
                MessageBox.Show("There was a error when reading in the license types");
            }
        }

        private void cbLicenseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadLicenseData(cbLicenseType.SelectedItem.ToString());
        }

        private void LoadLicenseData(string selectedLicenseType)
        {
            // Open the database connection
            connection.Open();

            // Create a SQL command to select license data based on the selected license type
            string query = "SELECT * FROM Licenses WHERE LicenseType = @LicenseType";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@LicenseType", selectedLicenseType);

            // Create a SqlDataAdapter to fill the DataGridView
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            // Bind the DataTable to the DataGridView
            dgvLicense.DataSource = dt;

            // Close the database connection
            connection.Close();
        }

        private void btnLicanceDelete_Click(object sender, EventArgs e)
        {
            try
            {
                connection.Open();
                deleteReport += "\n" + "License deleted: " + cbLicenseType.Text;


                string sql = "DELETE FROM LICENSE WHERE LICENSE_ID = " + LicenseCustomer_Remove.Text;
                comm = new SqlCommand(sql, connection);
                //comm.Parameters.AddWithValue("@Customer_ID", LicenseCustomer_Remove.Text);
                comm.ExecuteNonQuery();

                sql = "SELECT * FROM LICENSE";
                comm = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter();
                adapter.SelectCommand = comm;
                DataSet set = new DataSet();
                adapter.Fill(set, "LICENSE");

                dgvLicense.DataSource = set;

                dgvLicense.DataMember = "LICENSE";

                connection.Close();

                MessageBox.Show("Deleted!");
            }
            catch(Exception)
            {
                MessageBox.Show("There was a constraint error when deleting the license");
            }




        }

        private void btnFireDelete_Click(object sender, EventArgs e)
        {
            try
            {
                deleteReport += "\n" + "Firearms Deleted:\n" + "Type:" + cbFireType.Text + "\n" + "Firearm name: " + txtFireName.Text + "\n" + "Firearm number: " + txtFireNumber.Text + "\n" + "Firearms in stock: " + txtFireStock.Text;
                connection.Open();
                string sql = "DELETE FROM FIREARMS WHERE Firearm_ID = " + txtFireID.Text;

                comm = new SqlCommand(sql, connection);

                comm.ExecuteNonQuery();
                MessageBox.Show("the firearm has been deleted");

                sql = "SELECT * FROM FIREARMS";
                comm = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter();
                adapter.SelectCommand = comm;
                DataSet set = new DataSet();
                adapter.Fill(set, "FIREARMS");

                dgvFirearms.DataSource = set;
                dgvFirearms.DataMember = "FIREARMS";

                connection.Close();


            }
            catch(Exception)
            {
                MessageBox.Show("There was a error when deleting from the table:");
            }
            
        }

        private void btnAccDelete_Click(object sender, EventArgs e)
        {
            deleteReport += "\n" + "Accessory Update:\n" + "Accessory Type: " + AccessoryIDtxt.Text + "\n" + "Accessory stock: " + txtAccStock.Text + "\n" + "Accessory Price: " + txtAccPrice.Text;

            connection.Open();

            string sql = "DELETE FROM Accessories WHERE Accessory_ID = " + AccessoryIDtxt.Text;
            comm = new SqlCommand(sql, connection);
            //comm.Parameters.AddWithValue("@Customer_ID", LicenseCustomer_Remove.Text);
            comm.ExecuteNonQuery();

            sql = "SELECT * FROM ACCESSORIES";
            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = comm;
            DataSet set = new DataSet();
            adapter.Fill(set, "ACCESSORIES");

            dgvAccessories.DataSource = set;
            dgvAccessories.DataMember = "ACCESSORIES";

            connection.Close();

            MessageBox.Show("Deleted!");

        }



        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {
            checkBox3.Checked = false;
        }

        private void checkBox3_CheckedChanged_1(object sender, EventArgs e)
        {
            checkBox2.Checked = false;
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                lbReportGenerated.Items.Add(updateReport);
            }
            else if (checkBox3.Checked == true)
            {
                lbReportGenerated.Items.Add(deleteReport);
            }
            else
                MessageBox.Show("please select a report to shown");
        }

       

        private void AccessoryIDtxt_TextChanged(object sender, EventArgs e)
        {
            connection.Open();
            string sql = "SELECT * FROM ACCESSORIES WHERE Accessory_ID LIKE " + AccessoryIDtxt.Text;
            comm = new SqlCommand(sql,connection);
            DataSet set = new DataSet();
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = comm;
            adapter.Fill(set,"Accessories");

            dgvAccessories.DataSource = set;
            dgvAccessories.DataMember = "Accessories";

            connection.Close();           


        }

       

        private void cbFireType_SelectedIndexChanged(object sender, EventArgs e)
        {
            connection.Open();

            string sql = "SELECT * FROM FIREARMS WHERE Firearm_Type LIKE '" + cbFireType.Text + "'";
            comm = new SqlCommand(sql, connection);
            adapter = new SqlDataAdapter();
            adapter.SelectCommand = comm;
            DataSet set = new DataSet();

            adapter.Fill(set, "FIREARMS");

            dgvFirearms.DataSource = set;
            dgvFirearms.DataMember = "FIREARMS";

            connection.Close();
        }
    }


}

