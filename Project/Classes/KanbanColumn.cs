using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Classes
{
    public class KanbanColumn
    {
        //Поле наименование обязательно для заполнения.
        public int Id { get; set; }
        public string TitleStatus { get; set; }
        public int ProjectId { get; set; }

        public KanbanColumn(int Id, string TitleStatus, int ProjectId)
        {
            this.Id = Id;
            this.TitleStatus = TitleStatus;
            this.ProjectId = ProjectId;


        }
    }
}

