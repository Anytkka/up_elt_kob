using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project.Classes
{
    public class Project
    {
        //Поля наименование и публичность обязательны для заполнения.
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }

        public Project(int Id, string Name, string Description, bool IsPublic)
        {
            this.Id = Id;
            this.Name = Name;
            this.Description = Description;
            this.IsPublic = IsPublic;
        }

    }
}
