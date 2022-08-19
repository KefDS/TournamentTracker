using System.ComponentModel;
using System.Windows.Forms;
using TrackerLibrary;
using System.Linq;

namespace TrackerUI
{
    public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
    {
        private BindingList<TeamModel> availableTeams = new(GlobalConfig.Connection.GetTeam_All());
        private BindingList<TeamModel> selectedTeams = new();

        private BindingList<PrizeModel> selectedPrizes = new();

        public CreateTournamentForm()
        {
            InitializeComponent();
            InitializeLists();
        }

        private void InitializeLists()
        {
            selectTeamDropDown.DataSource = availableTeams;
            selectTeamDropDown.DisplayMember = nameof(TeamModel.TeamName);

            tournamentTeamsListBox.DataSource = selectedTeams;
            tournamentTeamsListBox.DisplayMember = nameof(TeamModel.TeamName);

            prizesListBox.DataSource = selectedPrizes;
            prizesListBox.DisplayMember = nameof(PrizeModel.PlaceName);
        }

        private void addTeamButton_Click(object sender, System.EventArgs e)
        {
            if (selectTeamDropDown.SelectedItem != null)
            {
                TeamModel p = (TeamModel) selectTeamDropDown.SelectedItem;

                availableTeams.Remove(p);
                selectedTeams.Add(p);
            }
        }

        private void deleteSelectedTournamentPlayerButton_Click(object sender, System.EventArgs e)
        {
            if(tournamentTeamsListBox.SelectedItem != null)
            {
                TeamModel p = (TeamModel)tournamentTeamsListBox.SelectedItem;

                selectedTeams.Remove(p);
                availableTeams.Add(p);
            }
        }

        private void createPrizeButton_Click(object sender, System.EventArgs e)
        {
            CreatePrizeForm frm = new(this);
            frm.Show();
        }

        public void PrizeComplete(PrizeModel prize)
        {
            selectedPrizes.Add(prize);
        }

        public void TeamComplete(TeamModel model)
        {
            selectedTeams.Add(model);
        }

        private void createNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CreateTeamForm frm = new(this);
            frm.Show();
        }

        private void removeSelectedPrizeButton_Click(object sender, System.EventArgs e)
        {
            PrizeModel p = (PrizeModel)prizesListBox.SelectedItem;

            if(p != null)
            {
                selectedPrizes.Remove(p);
            }
        }

        private void createTournamentButton_Click(object sender, System.EventArgs e)
        {
            bool feeAcceptable = decimal.TryParse(entryFeeValue.Text, out decimal fee);

            if(!feeAcceptable)
            {
                MessageBox.Show("You need to enter a valid Entry Fee",
                    "Invalid Fee",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return;
            }

            TournamentModel tm = new();
            tm.TournamentName = tournamentNameValue.Text;
            tm.EntryFee = fee;

            tm.Prizes = selectedPrizes.ToList();
            tm.EnteredTeams = selectedTeams.ToList();

            TournamentLogic.CreateRounds(tm);
            GlobalConfig.Connection.CreateTournament(tm);
            tm.AlertUsersToNewRound();

            TournamentViewerForm frm = new(tm);
            frm.Show();
            Close();
        }
    }
}
