using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel tournament)
        {
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(tournament.EnteredTeams);
            int rounds = FindNumberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds, randomizedTeams.Count);

            tournament.Rounds.Add(CreateFirstRound(byes, randomizedTeams));
            CreateOtherRounds(tournament, rounds);
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            int startingRound = model.CheckCurrentRound();
            List<MatchupModel> toScore = new();
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    if (rm.Entries.Any(x => x.Score != 0) || rm.Entries.Count == 1)
                    {
                        toScore.Add(rm);
                    }
                }
            }

            MarkWinnerInMatchups(toScore);
            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
            int endingRound = model.CheckCurrentRound();

            if (endingRound > startingRound)
            {
                AlertUsersToNewRound(model);
            }

        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();
            List<MatchupModel> currentRound = model.Rounds.Find(x => x.First().MatchupRound == currentRoundNumber);

            foreach (MatchupModel matchup in currentRound)
            {
                foreach (MatchupEntryModel me in matchup.Entries)
                {
                    if (me.TeamCompeting is not null)
                    {
                        foreach (PersonModel p in me.TeamCompeting.TeamMembers)
                        {
                            AlertPersonToNewRound(p, me.TeamCompeting.TeamName, matchup.Entries.Where(x => x.TeamCompeting != me.TeamCompeting).FirstOrDefault());
                        }
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel p, string teamName, MatchupEntryModel competitor)
        {
            if (p.EmailAddress.Length == 0) return;

            string to = p.EmailAddress;
            StringBuilder body = new();

            string subject;
            if (competitor is not null)
            {
                subject = $"You have a new matchup with {competitor.TeamCompeting.TeamName}";

                body.AppendLine("<h1>You have a new machup</h1>");
                body.Append("<strong>Competitor: </strong>");
                body.Append(competitor.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine();
                body.AppendLine("Have a great time!");
                body.AppendLine("~ Tournament Tracker");
            }
            else
            {
                subject = $"You have a bye week this round";

                body.AppendLine("Enjoy your round off!");
                body.AppendLine("~Tournament Tracker");
            }

            EmailLogic.SendEmail(to, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;
            foreach (List<MatchupModel> round in model.Rounds)
            {
                if (round.All(x => x.Winner is not null))
                {
                    output++;
                }
                else
                {
                    return output;
                }
            }

            // Tournament is complete
            CompleteTournament(model);

            return output - 1;
        }

        private static void CompleteTournament(TournamentModel model)
        {
            GlobalConfig.Connection.CompleteTournament(model);

            MatchupModel lastMatchup = model.Rounds.Last().First();
            TeamModel winners = lastMatchup.Winner;
            TeamModel runnerUp = lastMatchup.Entries.Find(x => x.TeamCompeting != winners).TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerUpPrize = 0;

            if (model.Prizes.Count > 0)
            {
                decimal totalIncome = model.EnteredTeams.Count * model.EntryFee;

                PrizeModel firstPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                PrizeModel secondPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();
                if (firstPlacePrize is not null)
                {
                    winnerPrize = firstPlacePrize.CalculatePrizePayout(totalIncome);
                }

                if (secondPlacePrize is not null)
                {
                    runnerUpPrize = secondPlacePrize.CalculatePrizePayout(totalIncome);
                }
            }

            // Send Email to all tournament
            StringBuilder body = new();
            string subject;


            subject = $"In {model.TournamentName}, {winners.TeamName} has won!";

            body.AppendLine("<h1>We have a WINNER!</h1>");
            body.Append("<p>Congratulations to our winner on a great tournament.</p>");
            body.Append("<br />");

            if (winnerPrize > 0)
            {
                body.AppendLine($"<p>{winners.TeamName} will receive ${winnerPrize}");
            }

            if (runnerUpPrize > 0)
            {
                body.AppendLine($"<p>{runnerUp.TeamName} will receive ${runnerUpPrize}");
            }

            body.AppendLine("<p>Thanks for a great tournament everyone!</p>");
            body.AppendLine("~ Tournament Tracker");

            List<string> bcc = new();
            foreach (TeamModel t in model.EnteredTeams)
            {
                foreach (PersonModel p in t.TeamMembers)
                {
                    if (p.EmailAddress.Length > 0)
                    {
                        bcc.Add(p.EmailAddress); 
                    }
                }
            }

            EmailLogic.SendEmail(bcc, new List<string>(), subject, body.ToString());
            model.CompleteTournament();
        }

        private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output;
            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output = Decimal.Multiply(totalIncome, Convert.ToDecimal((prize.PrizePercentage / 100)));
            }

            return output;
        }

        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            Dictionary<int, MatchupModel> modelsDic = models.ToDictionary(item => item.Id, item => item);

            foreach (List<MatchupModel> round in tournament.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    foreach (MatchupEntryModel me in rm.Entries)
                    {
                        if (me.ParentMatchup is not null && modelsDic.ContainsKey(me.ParentMatchup.Id))
                        {
                            me.TeamCompeting = modelsDic[me.ParentMatchup.Id].Winner;
                            GlobalConfig.Connection.UpdateMatchup(rm);
                        }
                    }
                }
            }
        }

        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            // Greater or lesser
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (MatchupModel matchup in models)
            {
                // Checks for byes
                if (matchup.Entries.Count == 1)
                {
                    matchup.Winner = matchup.Entries[0].TeamCompeting;
                    continue;
                }

                // 0 means false, or low score wins
                if (greaterWins == "0")
                {
                    if (matchup.Entries[0].Score < matchup.Entries[1].Score)
                    {
                        matchup.Winner = matchup.Entries[0].TeamCompeting;
                    }
                    else if (matchup.Entries[1].Score < matchup.Entries[0].Score)
                    {
                        matchup.Winner = matchup.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                }
                else
                {
                    // 1 mean true, or high score wins
                    if (matchup.Entries[0].Score > matchup.Entries[1].Score)
                    {
                        matchup.Winner = matchup.Entries[0].TeamCompeting;
                    }
                    else if (matchup.Entries[1].Score > matchup.Entries[0].Score)
                    {
                        matchup.Winner = matchup.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                }
            }
        }

        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new();
            MatchupModel current = new();

            foreach (TeamModel team in teams)
            {
                current.Entries.Add(new MatchupEntryModel { TeamCompeting = team });
                if (byes > 0 || current.Entries.Count > 1)
                {
                    current.MatchupRound = 1;
                    output.Add(current);
                    byes--;

                    current = new();
                }
            }

            return output;
        }

        private static void CreateOtherRounds(TournamentModel tournament, int numberOfRounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = tournament.Rounds[0];
            List<MatchupModel> currentRound = new();
            MatchupModel currentMatchup = new();

            while (round <= numberOfRounds)
            {
                foreach (MatchupModel match in previousRound)
                {
                    currentMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });
                    if (currentMatchup.Entries.Count > 1)
                    {
                        currentMatchup.MatchupRound = round;
                        currentRound.Add(currentMatchup);
                        currentMatchup = new();
                    }

                }

                tournament.Rounds.Add(currentRound);
                previousRound = currentRound;
                currentRound = new();
                round++;
            }
        }

        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            return (int)Math.Pow(2, rounds) - numberOfTeams;
        }

        private static int FindNumberOfRounds(int teamCount)
        {
            int rounds = 1;
            int val = 2;

            while (val < teamCount)
            {
                rounds++;
                val *= 2;
            }

            return rounds;
        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            return teams.OrderBy(_ => Guid.NewGuid()).ToList();
        }
    }
}
