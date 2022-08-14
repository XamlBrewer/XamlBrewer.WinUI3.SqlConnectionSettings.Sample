using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
            builder.IntegratedSecurity = true;
            builder.TrustServerCertificate = true;
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
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged()
        {
            // Broadcast all properties.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private List<string> AuthenticationProtocols => new() {
            "Windows Authentication",
            "SQL Server Authentication",
            "Azure Active Directory - Universal with MFA",
            "Azure Active Directory - Password",
            "Azure Active Directory - Integrated"
        };

        private async void DatabaseComboBox_DropDownOpened(object sender, object e)
        {
            if (string.IsNullOrWhiteSpace(Server))
            {
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
            catch (Exception ex)
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
    }
}
