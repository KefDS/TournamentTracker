using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TrackerLibrary;

namespace TrackerUI
{
    public partial class TournamentViewerForm : Form
    {
        private TournamentModel tournament;
        private readonly BindingList<int> rounds = new();
        private readonly BindingList<MatchupModel> selectedMatchups = new();

        public TournamentViewerForm(TournamentModel tournament)
        {
            InitializeComponent();
            this.tournament = tournament;

            tournament.OnTournamentComplete += Tournament_OnTournamentComplete;

            LoadFormData();
            LoadRound();
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private void Tournament_OnTournamentComplete(object sender, DateTime e)
        {
            this.Close();
        }

        private void LoadFormData()
        {
            tournamentName.Text = tournament.TournamentName;

            roundDropDown.DataSource = rounds;
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = nameof(MatchupModel.DisplayName);
        }

        private void LoadRound()
        {
            rounds.Clear();
            for (int i = 1; i <= tournament.Rounds.Count; i++)
                rounds.Add(i);
        }

        private void LoadMatchups(int currentRound)
        {
            List<MatchupModel> currentMatchups = tournament.Rounds.Find(roundList => roundList.First().MatchupRound == currentRound);

            selectedMatchups.Clear();
            if (unplayedOnlyCheckbox.Checked) currentMatchups = currentMatchups.FindAll(m => m.Winner is null);
            currentMatchups.ForEach(m => selectedMatchups.Add(m));

            LoadMatchup();
            DisplayMatchupInfo();
        }

        private void DisplayMatchupInfo()
        {
            bool isVisible = selectedMatchups.Count > 0;

            List<Control> controls = new() { teamOneName, teamTwoName, teamOneScoreLabel, teamOneScoreValue, teamTwoScoreLabel, teamTwoScoreValue, vsLabel, scoreButton };
            controls.ForEach(c => c.Visible = isVisible);
        }

        private void LoadMatchup()
        {
            MatchupModel m = (MatchupModel)matchupListBox.SelectedItem;
            if (m == null) return;

            teamOneScoreValue.ReadOnly = false;
            teamTwoScoreValue.ReadOnly = false;

            // First team
            if (m.Entries[0].TeamCompeting != null)
            {
                teamOneName.Text = m.Entries[0].TeamCompeting.TeamName;
                teamOneScoreValue.Text = m.Entries[0].Score.ToString();
            }
            else
            {
                teamOneName.Text = "Not yet set";
                teamOneScoreValue.Text = string.Empty;
                teamOneScoreValue.ReadOnly = true;
            }

            // Second team
            if (m.Entries.Count > 1)
            {
                // Second team
                if (m.Entries[1].TeamCompeting != null)
                {
                    teamTwoName.Text = m.Entries[1].TeamCompeting.TeamName;
                    teamTwoScoreValue.Text = m.Entries[1].Score.ToString();
                }
                else
                {
                    teamTwoName.Text = "Not yet set";
                    teamTwoScoreValue.Text = string.Empty;
                    teamTwoScoreValue.ReadOnly = true;
                }
            }
            else
            {
                teamTwoName.Text = "<Bye>";
                teamTwoScoreValue.Text = "N/A";
                teamTwoScoreValue.ReadOnly = true;
            }
        }

        private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchup();
        }

        private void unplayedOnlyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private void scoreButton_Click(object sender, EventArgs e)
        {
            string errorMessage = ValidateData();
            if (errorMessage.Length > 0)
            {
                MessageBox.Show($"Input Error: {errorMessage}");
                return;
            }

            MatchupModel m = (MatchupModel)matchupListBox.SelectedItem;

            double teamOneScore = 0;
            double teamTwoScore = 0;

            // First team
            if (m.Entries[0].TeamCompeting is not null)
            {
                if (double.TryParse(teamOneScoreValue.Text, out teamOneScore))
                {
                    m.Entries[0].Score = teamOneScore;
                }
                else
                {
                    MessageBox.Show($"Please enter a valid score for {m.Entries[0].TeamCompeting.TeamName}");
                    return;
                }
            }

            if (m.Entries.Count > 1)
            {
                // Sencond team
                if (m.Entries[1].TeamCompeting is not null)
                {
                    if (double.TryParse(teamTwoScoreValue.Text, out teamTwoScore))
                    {
                        m.Entries[1].Score = teamTwoScore;
                    }
                    else
                    {
                        MessageBox.Show($"Please enter a valid score for {m.Entries[1].TeamCompeting.TeamName}");
                        return;
                    }
                }
            }

            try
            {
                TournamentLogic.UpdateTournamentResults(tournament);
            }
            catch (Exception ex)
            {

                MessageBox.Show($"The application had the following error: {ex.Message}");
            }

            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        private string ValidateData()
        {
            string output = String.Empty;

            bool scoreOneValid = double.TryParse(teamOneScoreValue.Text, out double teamOneScore);
            bool scoreTwoValid = double.TryParse(teamTwoScoreValue.Text, out double teamTwoScore);

            if (!scoreOneValid)
            {
                output = "The score One value is not a valid number";
            }
            else if (!scoreTwoValid)
            {
                output = "The score Two value is not a valid number";
            }
            else if (teamOneScore == 0 && teamTwoScore == 0)
            {
                output = "You did not enter a score for either team";
            }
            else if (teamOneScore == teamTwoScore)
            {
                output = "We do not allow ties in this application";
            }

            return output;
        }
    }
}
