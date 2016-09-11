using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Entities
{
    [Table("stats")]
    public class UserStats
    {
        [Key]
        [Column("user_id")]
        public string UserID { get; set; }

        [Column("score")]
        public int Score { get; set; }
    }
}
