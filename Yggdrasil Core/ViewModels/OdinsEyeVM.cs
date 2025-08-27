using System.ComponentModel;
using System.Windows.Media;
using Yggdrasil_Core.Utils;

namespace Yggdrasil_Core.ViewModels
{
    public class OdinsEyeVM : INotifyPropertyChanged
    {
        private double boxSize = 30;
        private double boxThickness = 2;
        private double boxWidth = 1;
        private double boxOpacity = 1;
        private Color boxColor = Colors.Black;
        private bool showPlayer;
        private bool showFriend;
        private bool showParty;
        private bool showGuild;
        private bool showMonsters;
        private bool showBoss;
        private bool showMinions;
        private bool showItem;
        private bool showNPC;
        private bool showLine;
        private bool showID;
        private bool showName;
        private bool showCoordinate;
        private bool showHPBar;
        private bool showSPBar;
        private bool showLordKnight;
        private bool showHighPriest;
        private bool showHighWizard;
        private bool showWhitesmith;
        private bool showSniper;
        private bool showAssassinCross;
        private bool showPaladin;
        private bool showChampion;
        private bool showProfessor;
        private bool showStalker;
        private bool showMinstrel;
        private bool showSuperNovice;
        private bool showBiochemist;
        private bool showNovice;
        private bool showCloaked;
        private bool showHidden;

        public event PropertyChangedEventHandler PropertyChanged;

        public double BoxSize
        {
            get => boxSize;
            set { boxSize = value; OnPropChanged(nameof(BoxSize)); }
        }

        public double BoxThickness
        {
            get => boxThickness;
            set { boxThickness = value; OnPropChanged(nameof(BoxThickness)); }
        }

        public double BoxWidth
        {
            get => boxWidth;
            set { boxWidth = value; OnPropChanged(nameof(BoxWidth)); }
        }

        public double BoxOpacity
        {
            get => boxOpacity;
            set { boxOpacity = value; OnPropChanged(nameof(BoxOpacity)); }
        }

        public Color BoxColor
        {
            get => boxColor;
            set { boxColor = value; OnPropChanged(nameof(BoxColor)); }
        }

        public bool ShowPlayer
        {
            get => showPlayer;
            set { showPlayer = value; OnPropChanged(nameof(ShowPlayer)); }
        }

        public bool ShowFriend
        {
            get => showFriend;
            set { showFriend = value; OnPropChanged(nameof(ShowFriend)); }
        }

        public bool ShowParty
        {
            get => showParty;
            set { showParty = value; OnPropChanged(nameof(ShowParty)); }
        }

        public bool ShowGuild
        {
            get => showGuild;
            set { showGuild = value; OnPropChanged(nameof(ShowGuild)); }
        }

        public bool ShowMonsters
        {
            get => showMonsters;
            set { showMonsters = value; OnPropChanged(nameof(ShowMonsters)); }
        }

        public bool ShowBoss
        {
            get => showBoss;
            set { showBoss = value; OnPropChanged(nameof(ShowBoss)); }
        }

        public bool ShowMinions
        {
            get => showMinions;
            set { showMinions = value; OnPropChanged(nameof(ShowMinions)); }
        }

        public bool ShowItem
        {
            get => showItem;
            set { showItem = value; OnPropChanged(nameof(ShowItem)); }
        }

        public bool ShowNPC
        {
            get => showNPC;
            set { showNPC = value; OnPropChanged(nameof(ShowNPC)); }
        }

        public bool ShowLine
        {
            get => showLine;
            set { showLine = value; OnPropChanged(nameof(ShowLine)); }
        }

        public bool ShowID
        {
            get => showID;
            set { showID = value; OnPropChanged(nameof(ShowID)); }
        }

        public bool ShowName
        {
            get => showName;
            set { showName = value; OnPropChanged(nameof(ShowName)); }
        }

        public bool ShowCoordinate
        {
            get => showCoordinate;
            set { showCoordinate = value; OnPropChanged(nameof(ShowCoordinate)); }
        }

        public bool ShowHPBar
        {
            get => showHPBar;
            set { showHPBar = value; OnPropChanged(nameof(ShowHPBar)); }
        }

        public bool ShowSPBar
        {
            get => showSPBar;
            set { showSPBar = value; OnPropChanged(nameof(ShowSPBar)); }
        }

        public bool ShowLordKnight
        {
            get => showLordKnight;
            set { showLordKnight = value; OnPropChanged(nameof(ShowLordKnight)); }
        }

        public bool ShowHighPriest
        {
            get => showHighPriest;
            set { showHighPriest = value; OnPropChanged(nameof(ShowHighPriest)); }
        }

        public bool ShowHighWizard
        {
            get => showHighWizard;
            set { showHighWizard = value; OnPropChanged(nameof(ShowHighWizard)); }
        }

        public bool ShowWhitesmith
        {
            get => showWhitesmith;
            set { showWhitesmith = value; OnPropChanged(nameof(ShowWhitesmith)); }
        }

        public bool ShowSniper
        {
            get => showSniper;
            set { showSniper = value; OnPropChanged(nameof(ShowSniper)); }
        }

        public bool ShowAssassinCross
        {
            get => showAssassinCross;
            set { showAssassinCross = value; OnPropChanged(nameof(ShowAssassinCross)); }
        }

        public bool ShowPaladin
        {
            get => showPaladin;
            set { showPaladin = value; OnPropChanged(nameof(ShowPaladin)); }
        }

        public bool ShowChampion
        {
            get => showChampion;
            set { showChampion = value; OnPropChanged(nameof(ShowChampion)); }
        }

        public bool ShowProfessor
        {
            get => showProfessor;
            set { showProfessor = value; OnPropChanged(nameof(ShowProfessor)); }
        }

        public bool ShowStalker
        {
            get => showStalker;
            set { showStalker = value; OnPropChanged(nameof(ShowStalker)); }
        }

        public bool ShowMinstrel
        {
            get => showMinstrel;
            set { showMinstrel = value; OnPropChanged(nameof(ShowMinstrel)); }
        }

        public bool ShowSuperNovice
        {
            get => showSuperNovice;
            set { showSuperNovice = value; OnPropChanged(nameof(ShowSuperNovice)); }
        }

        public bool ShowBiochemist
        {
            get => showBiochemist;
            set { showBiochemist = value; OnPropChanged(nameof(ShowBiochemist)); }
        }

        public bool ShowNovice
        {
            get => showNovice;
            set { showNovice = value; OnPropChanged(nameof(ShowNovice)); }
        }

        public bool ShowCloaked
        {
            get => showCloaked;
            set { showCloaked = value; OnPropChanged(nameof(ShowCloaked)); }
        }

        public bool ShowHidden
        {
            get => showHidden;
            set { showHidden = value; OnPropChanged(nameof(ShowHidden)); }
        }

        private void OnPropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}