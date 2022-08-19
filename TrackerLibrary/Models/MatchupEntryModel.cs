using System;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public class MatchupEntryModel : IIdentifiable
    {
        public int Id { get; set; }

        public int TeamCompetingId { get; set; }

        /// <summary>
        /// Represents one team in the matchup.
        /// </summary>
        public TeamModel TeamCompeting { get; set; }

        /// <summary>
        /// Represts the score for this particular team.
        /// </summary>
        public double Score { get; set; }

        public int ParentMatchupId { get; set; }

        /// <summary>
        /// Represets the matchup that this team came from as the winner.
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }
    }
}   