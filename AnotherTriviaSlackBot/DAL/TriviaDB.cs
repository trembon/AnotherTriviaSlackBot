using AnotherTriviaSlackBot.Entities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.DAL
{
    public static class TriviaDB
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static object dbQuestionLock = new object();
        private static object dbDataLock = new object();

        /// <summary>
        /// Gets random questions in a specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static List<Question> GetRandomQuestions(string category, int amount)
        {
            try
            {
                // fetch all bad questions from the data context
                List<string> badQuestionIds = new List<string>(0);
                lock (dbDataLock)
                {
                    using(var context = new DataDBContext())
                    {
                        badQuestionIds = context.BadQuestions.Select(bq => "'" + bq.QuestionID + "'").ToList();
                    }
                }

                // fetch the specified amount of random questions, filtered by all bad questions, from the question context
                lock (dbQuestionLock)
                {
                    using (var context = new QuestionDBContext())
                    {
                        return context.Database.SqlQuery<Question>($"SELECT * FROM questions WHERE id NOT IN ({String.Join(",", badQuestionIds)}) AND category = '{category}' ORDER BY RANDOM() LIMIT {amount}").ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, $"Failed to get {amount} random questions from category '{category}'.");
                return new List<Question>();
            }
        }

        private static int? cacheGetQuestionCount = null;
        /// <summary>
        /// Gets the existing number of questions in the database.
        /// </summary>
        /// <returns></returns>
        public static int GetQuestionCount()
        {
            try
            {
                lock (dbQuestionLock)
                {
                    // check if the value is cached
                    if (cacheGetQuestionCount != null)
                        return cacheGetQuestionCount.Value;

                    // else fetch it from the question context
                    using (var context = new QuestionDBContext())
                    {
                        cacheGetQuestionCount = context.Questions.Count();
                        return cacheGetQuestionCount.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get question count.");
                return -1;
            }
        }

        private static Dictionary<string, int> cacheGetCategories = null;
        /// <summary>
        /// Gets the existing categories with the number of questions in each.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, int> GetCategories()
        {
            try
            {
                lock (dbQuestionLock)
                {
                    // check if result is cached
                    if (cacheGetCategories != null)
                        return cacheGetCategories;

                    // else fetch it from the question context
                    using (var context = new QuestionDBContext())
                    {
                        cacheGetCategories = context.Questions.GroupBy(q => q.Category).ToDictionary(k => k.Key, v => v.Count());
                        return cacheGetCategories;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get categories.");
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Updates the player stats after a trivia round.
        /// </summary>
        /// <param name="latestAddition">The latest addition.</param>
        public static void UpdatePlayerStats(Dictionary<string, int> latestAddition)
        {
            if (latestAddition.Count == 0)
                return;

            try
            {
                lock (dbDataLock)
                {
                    using (var context = new DataDBContext())
                    {
                        // fetch all previous participant user
                        var users = context.UserStats.ToList();

                        // loop through all users that scored
                        foreach (var scoredUser in latestAddition)
                        {
                            // find the user in the database, if it does not exist, add it
                            var user = users.FirstOrDefault(u => u.UserID == scoredUser.Key);
                            if (user == null)
                            {
                                user = new UserStats { UserID = scoredUser.Key, Score = 0 };
                                context.UserStats.Add(user);
                            }

                            // update the score on the user
                            user.Score += latestAddition[user.UserID];
                        }

                        // save all changes
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get update user stats.");
            }
        }

        /// <summary>
        /// Gets the top five users.
        /// </summary>
        /// <returns></returns>
        public static List<UserStats> GetTopFiveUsers()
        {
            try
            {
                lock (dbDataLock)
                {
                    using (var context = new DataDBContext())
                    {
                        return context
                            .UserStats
                            .OrderByDescending(u => u.Score)
                            .Take(5)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to get top five user stats.");
                return new List<UserStats>();
            }
        }

        /// <summary>
        /// Sets the specified question as bad.
        /// </summary>
        /// <param name="questionId">The question identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public static void SetQuestionAsBad(string questionId, string userId)
        {
            try
            {
                lock (dbDataLock)
                {
                    using (var context = new DataDBContext())
                    {
                        var badQuestion = new BadQuestion { QuestionID = questionId, UserID = userId };
                        context.BadQuestions.Add(badQuestion);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, $"Failed to set question '{questionId}' (marked by '{userId}').");
            }
        }
    }
}
