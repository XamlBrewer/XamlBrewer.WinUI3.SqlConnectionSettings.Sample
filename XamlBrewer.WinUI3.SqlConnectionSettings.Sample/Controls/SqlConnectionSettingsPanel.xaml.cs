using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XamlBrewer.WinUI3.Controls
{
    public sealed partial class SqlConnectionSettingsPanel : UserControl, INotifyPropertyChanged
    {
        private bool isFetchingDatabases;
        private bool isConnecting;

        private List<string> databases = new();

        private readonly SqlConnectionStringBuilder builder = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public SqlConnectionSettingsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy getting the list of databases.
        /// </summary>
        public bool IsFetchingDatabases
        {
            get { return isFetchingDatabases; }
            set
            {
                isFetchingDatabases = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy connecting.
        /// </summary>
        public bool IsConnecting
        {
            get { return isConnecting; }
            set
            {
                isConnecting = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current server name.
        /// </summary>
        public string Server
        {
            get { return builder.DataSource; }
            set
            {
                builder.DataSource = value;
                Databases = new List<string>(); // Clear the database list.
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public string UserId
        {
            get { return builder.UserID; }
            set
            {
                builder.UserID = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        public string Password
        {
            get { return builder.Password; }
            set
            {
                builder.Password = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the list of databases.
        /// </summary>
        public List<string> Databases
        {
            get { return databases; }
            set
            {
                databases = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current database.
        /// </summary>
        public string Database
        {
            get { return builder.InitialCatalog; }
            set
            {
                builder.InitialCatalog = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current connection string.
        /// </summary>
        public string ConnectionString
        {
            get { return builder.ConnectionString; }
            set
            {
                builder.ConnectionString = value;
                cboAuthenticationProtocols.SelectedIndex = (int)builder.Authentication;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged()
        {
            // Broadcast all properties.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private IEnumerable<string> AuthenticationProtocols
        {
            get
            {
                foreach (string str in Enum.GetNames(typeof(SqlAuthenticationMethod)))
                {
                    yield return str.SplitCamelCase();
                }
            }
        }

        private async void DatabaseComboBox_DropDownOpened(object sender, object e)
        {
            await RefreshDatabases();
        }

        private async Task RefreshDatabases()
        {
            if (string.IsNullOrWhiteSpace(Server))
            {
                return;
            }

            if (Databases.Any())
            {
                // Already fetched.
                return;
            }

            IsFetchingDatabases = true;

            var databases = new List<string>();

            try
            {
                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.OpenAsync();

                    using var command = connection.CreateCommand();
                    command.CommandText = "SELECT [name] FROM sys.databases ORDER BY [name]";

                    using SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        databases.Add(reader.GetString(0));
                    }
                }

                Databases = databases;
            }
            catch (Exception)
            {
                Databases = new List<string>();
            }
            finally
            {
                IsFetchingDatabases = false;
            }
        }

        private void Database_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any())
            {
                Database = e.AddedItems.First().ToString();
            }
        }

        private async void ConnectionButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            IsConnecting = true;
            try
            {
                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    await connection.OpenAsync();
                }

                RedIcon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                GreenIcon.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
                GreenIcon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                RedIcon.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        private void ConnectionFlyout_Opened(object sender, object e)
        {
            RedIcon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            GreenIcon.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        private void Authentication_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            builder.Authentication = (SqlAuthenticationMethod)(sender as ComboBox).SelectedIndex;

            // You may not set both !!!
            // builder.IntegratedSecurity = builder.Authentication == SqlAuthenticationMethod.ActiveDirectoryIntegrated;

            // Process UserId and Password boxes.
            // Based on the descriptions and sample connection strings here:
            // https://docs.microsoft.com/en-us/sql/connect/ado-net/sql/azure-active-directory-authentication?view=sql-server-ver16
            // Not all scenarios could be tested.
            switch (builder.Authentication)
            {
                case SqlAuthenticationMethod.ActiveDirectoryIntegrated:
                    ActivateUserId(); // User Id is optional.
                    DeactivatePassword();
                    break;
                case SqlAuthenticationMethod.ActiveDirectoryInteractive:
                    ActivateUserId(); // User Id is optional.
                    DeactivatePassword(); // Password provided via authentication prompt.
                    break;
                case SqlAuthenticationMethod.ActiveDirectoryPassword:
                    ActivateUserId();
                    ActivatePassword();
                    break;
                case SqlAuthenticationMethod.ActiveDirectoryServicePrincipal:
                    ActivateUserId();
                    ActivatePassword();
                    break;
                case SqlAuthenticationMethod.ActiveDirectoryDeviceCodeFlow:
                    DeactivateUserId();
                    DeactivatePassword();
                    break;
                case SqlAuthenticationMethod.ActiveDirectoryManagedIdentity:
                    ActivateUserId();
                    DeactivatePassword();
                    break;
                case SqlAuthenticationMethod.ActiveDirectoryMSI:
                    ActivateUserId();
                    DeactivatePassword();
                    break;
                case SqlAuthenticationMethod.ActiveDirectoryDefault:
                    DeactivateUserId();
                    DeactivatePassword();
                    break;
                case SqlAuthenticationMethod.NotSpecified:
                    DeactivateUserId();
                    DeactivatePassword();
                    break;
                case SqlAuthenticationMethod.SqlPassword:
                    ActivateUserId();
                    ActivatePassword();
                    break;
            }
        }

        private void ActivateUserId()
        {
            crdUserId.IsEnabled = true;
        }

        private void DeactivateUserId()
        {
            UserId = string.Empty;
            builder.Remove(nameof(UserId));
            crdUserId.IsEnabled = false;
        }

        private void ActivatePassword()
        {
            crdPassword.IsEnabled = true;
        }

        private void DeactivatePassword()
        {
            Password = string.Empty;
            builder.Remove(nameof(Password));
            crdPassword.IsEnabled = false;
        }
    }

    internal static class StringExtensions
    {
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }
    }
}
