﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Entities
{
    [Table("bad_questions")]
    public class BadQuestion
    {
        [Key]
        [Column("questions_id")]
        public string QuestionID { get; set; }

        [Column("user_id")]
        public string UserID { get; set; }
    }
}
