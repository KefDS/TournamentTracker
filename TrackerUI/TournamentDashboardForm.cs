using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;

namespace TrackerUI
{
    public partial class TournamentDashboardForm : Form
    {
        List<TournamentModel> tournaments = GlobalConfig.Connection.GetTournament_All();

        public TournamentDashboardForm()
        {
            InitializeComponent();
            WireUpList();
        }

        private void WireUpList()
        {
            loadExisitngTournamentDropDown.DataSource = tournaments;
            loadExisitngTournamentDropDown.DisplayMember = nameof(TournamentModel.TournamentName);
        }

        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            CreateTournamentForm frm = new();
            frm.Show();
        }

        private void loadTournamentButton_Click(object sender, EventArgs e)
        {
            TournamentModel selectedTournament = (TournamentModel) loadExisitngTournamentDropDown.SelectedItem;
            TournamentViewerForm frm = new(selectedTournament);
            frm.Show();
        }
    }
}
