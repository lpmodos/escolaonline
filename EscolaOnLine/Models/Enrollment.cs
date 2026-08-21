using EscolaOnLine.Enums;
using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Models
{
    public class Enrollment
    {
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        [Required]
        public Status Status { get; set; }
        public DateTime DataMatricula { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        public virtual Course  Course { get; set; }
        public virtual Student Student { get; set; }
    }
}
