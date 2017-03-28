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

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Key]
        [Column("id")]
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        [Column("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        /// <value>
        /// The answer.
        /// </value>
        [Column("answer")]
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [Column("category")]
        public string Category { get; set; }

        /// <summary>
        /// Generates a hint for the question.
        /// </summary>
        /// <returns></returns>
        public string GenerateHint()
        {
            // check so the question has an answer
            if (String.IsNullOrWhiteSpace(this.Answer))
                return String.Empty;

            StringBuilder hint = new StringBuilder();
            Random rand = new Random();

            // count all letter or digit characters in the string
            int nonSpaceCharacters = this.Answer.Where(c => char.IsLetterOrDigit(c)).Count();

            // if the letter or digits are 3 or less, replace those characters with a *
            if (nonSpaceCharacters <= 3)
            {
                for (int i = 0; i < this.Answer.Length; i++)
                {
                    if (char.IsLetterOrDigit(this.Answer[i]))
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
                // if there is more then 3 letter or digits, replace 2/3 or the characters
                int toReplace = nonSpaceCharacters / 3 * 2;

                // generate a randomized list of character positions to replace with *
                int[] positionsToChange = Enumerable.Range(0, this.Answer.Length).Where(x => char.IsLetterOrDigit(this.Answer[x])).OrderBy(x => rand.Next()).Take(toReplace).ToArray();
                for (int i = 0; i < this.Answer.Length; i++)
                {
                    // if the position is in the randomized list, replace it
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

            // return the hint string
            return hint.ToString();
        }
    }
}
