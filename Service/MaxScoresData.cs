using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class MaxScoresData
    {
        public static decimal GetTotalMaxScore()
        {
            return MaxScores.Sum(q => q.TotalScore);
        }

        public static readonly List<QuestionScore> MaxScores = new()
    {
        new QuestionScore
        {
            Qcode = "Q1",
            SubScores = new Dictionary<string, decimal> { { "Login", 1.0m } }
        },
        new QuestionScore
        {
            Qcode = "Q2",
            SubScores = new Dictionary<string, decimal>
            {
                { "List All", 0.25m },
                { "List All 2", 0.25m },
                { "Paging", 1.0m }
            }
        },
        new QuestionScore
        {
            Qcode = "Q3",
            SubScores = new Dictionary<string, decimal>
            {
                { "Add OK", 1.0m },
                { "Display Top", 0.25m },
                { "Validation Combobox", 0.25m },
                { "Validation Required", 0.25m },
                { "Validation - Characters Length", 0.25m },
                { "Validation - No Special Characters", 0.5m }
            }
        },
        new QuestionScore
        {
            Qcode = "Q4",
            SubScores = new Dictionary<string, decimal>
            {
                { "Update OK", 1.0m },
                { "Update Validation", 1.0m }
            }
        },
        new QuestionScore
        {
            Qcode = "Q5",
            SubScores = new Dictionary<string, decimal>
            {
                { "Test 1", 0.5m },
                { "Test 2", 0.5m },
                { "Test 3", 0.5m }
            }
        },
        new QuestionScore
        {
            Qcode = "Q6",
            SubScores = new Dictionary<string, decimal>
            {
                { "Delete with SignalR", 1.5m }
            }
        }
    };
    }
}
