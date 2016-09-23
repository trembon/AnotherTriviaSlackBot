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
        private const char REPLACEMENT_CHAR = '⋆';

        [Key]
        [Column("id")]
        public string ID { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("answer")]
        public string Answer { get; set; }

        [Column("category")]
        public string Category { get; set; }

        public string GenerateHint()
        {
            if (String.IsNullOrWhiteSpace(this.Answer))
                return String.Empty;

            StringBuilder hint = new StringBuilder();
            Random rand = new Random();

            int nonSpaceCharacters = this.Answer.Where(c => Char.IsLetterOrDigit(c)).Count();
            if (nonSpaceCharacters <= 3)
            {
                for (int i = 0; i < this.Answer.Length; i++)
                {
                    if (Char.IsLetterOrDigit(this.Answer[i]))
                    {
                        hint.Append(REPLACEMENT_CHAR);
                    }
                    else
                    {
                        hint.Append(this.Answer[i]);
                    }
                }
            }
            else
            {
                int toReplace = nonSpaceCharacters / 3 * 2;

                int[] positionsToChange = Enumerable.Range(0, this.Answer.Length).Where(x => Char.IsLetterOrDigit(this.Answer[x])).OrderBy(x => rand.Next()).Take(toReplace).ToArray();
                for (int i = 0; i < this.Answer.Length; i++)
                {
                    if (positionsToChange.Contains(i))
                    {
                        hint.Append(REPLACEMENT_CHAR);
                    }
                    else
                    {
                        hint.Append(this.Answer[i]);
                    }
                }
            }

            return hint.ToString();
        }
    }
}
