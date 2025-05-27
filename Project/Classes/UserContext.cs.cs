using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Project.Classes
{
    public class UserContext : User, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _fullName;
        private string _email;
        private string _password;
        private string _biography;
        private string _profileImagePath;

        public UserContext(int Id, string Email, string Password, string FullName, string Biography, string ProfileImagePath = null)
            : base(Id, Email, Password, FullName, Biography, ProfileImagePath)
        {
            _email = Email;
            _password = Password;
            _fullName = FullName;
            _biography = Biography;
            _profileImagePath = ProfileImagePath;
        }

        public new int Id => base.Id;

        public new string Email
        {
            get => _email;
            set
            {
                _email = value;
                base.Email = value;
                OnPropertyChanged();
            }
        }

        public new string Password
        {
            get => _password;
            set
            {
                _password = value;
                base.Password = value;
                OnPropertyChanged();
            }
        }

        public new string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                base.FullName = value;
                OnPropertyChanged();
            }
        }

        public new string Biography
        {
            get => _biography;
            set
            {
                _biography = value;
                base.Biography = value;
                OnPropertyChanged();
            }
        }

        public new string ProfileImagePath
        {
            get => _profileImagePath;
            set
            {
                _profileImagePath = value;
                base.ProfileImagePath = value;
                OnPropertyChanged();
            }
        }

        public static List<UserContext> Get()
        {
            List<UserContext> AllUser = new List<UserContext>();
            string SQL = "SELECT * FROM `user`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader Data = Connection.Query(SQL, connection);
            while (Data.Read())
            {
                AllUser.Add(new UserContext(
                    Data.GetInt32(0),
                    Data.GetString(1),
                    Data.GetString(2),
                    Data.GetString(3),
                    Data.IsDBNull(4) ? null : Data.GetString(4),
                    Data.IsDBNull(5) ? null : Data.GetString(5)
                ));
            }
            Connection.CloseConnection(connection);
            return AllUser;
        }

        public void Add()
        {
            string SQL = "INSERT INTO `user` (`email`, `password`, `fullName`, `biography`, `image`) " +
                         "VALUES (@email, @password, @fullName, @biography, @image)";

            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@email", this.Email);
                cmd.Parameters.AddWithValue("@password", this.Password);
                cmd.Parameters.AddWithValue("@fullName", this.FullName);
                cmd.Parameters.AddWithValue("@biography", this.Biography ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@image", this.ProfileImagePath ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            Connection.CloseConnection(connection);
        }

        public void Update()
        {
            string SQL = "UPDATE `user` " +
                 "SET `email`=@email, " +
                 "`password`=@password, " +
                 "`fullName`=@fullName, " +
                 "`biography`=@biography, " +
                 "`image`=@image " +
                 "WHERE `id`=@id";

            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@id", this.Id);
                cmd.Parameters.AddWithValue("@email", this.Email);
                cmd.Parameters.AddWithValue("@password", this.Password);
                cmd.Parameters.AddWithValue("@fullName", this.FullName);
                cmd.Parameters.AddWithValue("@biography", this.Biography ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@image", this.ProfileImagePath ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            Connection.CloseConnection(connection);
        }

        public void Delete()
        {
            string SQL = "DELETE FROM `user` WHERE `id`=@id";
            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@id", this.Id);
                cmd.ExecuteNonQuery();
            }
            Connection.CloseConnection(connection);
        }

        public override string ToString()
        {
            return FullName ?? "Не указано";
        }
    }
}