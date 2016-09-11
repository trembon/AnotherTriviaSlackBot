using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Entities
{
    [Table("questions")]
    public class Question
    {
        [Key]
        [Column("text")]
        public string Text { get; set; }

        [Column("answer")]
        public string Answer { get; set; }

        [Column("hint")]
        public string Hint { get; set; }
    }
}
