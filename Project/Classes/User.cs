using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project.Classes
{
    public class User
    {
        //Поля почта, ФИО, пароль обязательны для заполнения.
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Biography { get; set; }

        public User(int Id, string Email, string Password, string FullName, string Biography)
        {
            this.Id = Id;
            this.Email = Email;
            this.Password = Password;
            this.FullName = FullName;
            this.Biography = Biography;
        }
    }
}