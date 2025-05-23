using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Classes
{
    public class Subtask
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; } 
        public DateTime DueDate { get; set; }
        public int TaskId { get; set; }
        public int UserId {  get; set; }
        public int StatusId { get; set; }

        public Subtask(int id, string name, string description, DateTime dueDate, int taskId, int UserId, int statusId)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.DueDate = dueDate;
            this.TaskId = taskId;
            this.UserId = UserId;
            StatusId = statusId;
        }

    }
}
