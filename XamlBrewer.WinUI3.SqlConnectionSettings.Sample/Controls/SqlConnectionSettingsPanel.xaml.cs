using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace XamlBrewer.WinUI3.Controls
{
    public sealed partial class SqlConnectionSettingsPanel : UserControl, INotifyPropertyChanged
    {
        private bool isBusy = true;

        private string connectionString;

        private List<string> databases = new List<string>();

        private SqlConnectionStringBuilder builder = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public SqlConnectionSettingsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
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
                connectionString = value;
                builder.ConnectionString = connectionString;
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
    }
}
