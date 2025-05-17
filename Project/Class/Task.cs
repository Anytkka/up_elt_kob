using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Class
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; } 
        public DateTime DueDate { get; set; } 
        public int Status { get; set; } 
        public int ProjectId { get; set; } 

        public Task(int id, string name, string description, DateTime dueDate, int status, int projectId)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.DueDate = dueDate;
            this.Status = status;
            this.ProjectId = projectId;
        }
    }
}

 
