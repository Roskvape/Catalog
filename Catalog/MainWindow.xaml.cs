using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace BindingEnums
{
    public enum WeaponType
    {
        Club,
        Sword,
        Axe,
        Polearm,
        Bow
    }
}

namespace Catalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string dbReloadCommand = "SELECT* FROM weapons";
        private string dbInsertCommand = "INSERT INTO `weapons` (`weapon_id`, `weapon_name`, `weapon_type`, `weapon_damage`, `weapon_range`) VALUES (NULL, ";
        private string dbRemoveCommand = "DELETE FROM `weapons` WHERE `weapons`.`weapon_id` = ";
        private string dbConnection = "SERVER=localhost;DATABASE=items_database;UID=root;PASSWORD=;";

        public MainWindow()
        {
            InitializeComponent();
            ReloadTable();
        }

        private static readonly Regex _isIntRegex = new Regex("[^0-9]+"); //Integers only
        private static bool IsInt(string text)
        {
            return !_isIntRegex.IsMatch(text);
        }
        private void IntOnlyInputs_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsInt(e.Text);
        }

        private void PasteOnlyInts(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsInt(text)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private void ReloadTable()
        {
            SendCommandToDatabase(dbReloadCommand);
        }

        private void SendCommandToDatabase(string firstInput, params string[] additionalInput)
        {
            DataTable dataTable = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(dbConnection))
            {
                MySqlCommand firstCommand = new MySqlCommand(firstInput, connection);

                connection.Open();

                if (firstCommand.CommandText == dbReloadCommand)
                {
                    dataTable.Load(firstCommand.ExecuteReader());
                }
                else
                {
                    firstCommand.ExecuteNonQuery();
                }

                if (additionalInput != null)
                {
                    foreach (string command in additionalInput)
                    {
                        MySqlCommand nextCommand = new MySqlCommand(command, connection);

                        if (nextCommand.CommandText == dbReloadCommand)
                        {
                            dataTable.Load(nextCommand.ExecuteReader());
                        }
                        else
                        {
                            nextCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            dataTableGrid.DataContext = dataTable;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string command = $"'{txtName.Text}', '{cbType.Text}', '{txtDamage.Text}', '{txtRange.Text}')";
            SendCommandToDatabase(dbInsertCommand + command, dbReloadCommand);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            SendCommandToDatabase(dbRemoveCommand + txtID.Text, dbReloadCommand);
        }
    }
}
