﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PKHeX
{
    public partial class SuperTrainingEditor : Form
    {
        public SuperTrainingEditor()
        {
            InitializeComponent();
            int vertScrollWidth = SystemInformation.VerticalScrollBarWidth;
            TLP_SuperTrain.Padding = TLP_DistSuperTrain.Padding = new Padding(0, 0, vertScrollWidth, 0);
            
            populateRegimens("SuperTrain", TLP_SuperTrain, reglist);
            populateRegimens("DistSuperTrain", TLP_DistSuperTrain, distlist);

            CB_Bag.Items.Clear();
            CB_Bag.Items.Add("---");
            for (int i = 1; i < Main.trainingbags.Length - 1; i++)
                CB_Bag.Items.Add(Main.trainingbags[i]);

            if (pkm is PK6)
            {
                PK6 pk6 = (PK6)pkm;
                CB_Bag.SelectedIndex = pk6.TrainingBag;
                NUD_BagHits.Value = pk6.TrainingBagHits;
            }
            else
            {
                CB_Bag.Visible = NUD_BagHits.Visible = false;
            }
        }

        private readonly List<RegimenInfo> reglist = new List<RegimenInfo>();
        private readonly List<RegimenInfo> distlist = new List<RegimenInfo>();
        private readonly PKM pkm = Main.pkm.Clone();
        private const string PrefixLabel = "L_";
        private const string PrefixCHK = "CHK_";

        private void B_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void B_Save_Click(object sender, EventArgs e)
        {
            save();
            Close();
        }

        private void populateRegimens(string Type, TableLayoutPanel TLP, List<RegimenInfo> list)
        {
            // Get a list of all Regimen Attregutes in the PKM
            var RegimenNames = ReflectUtil.getPropertiesStartWithPrefix(pkm.GetType(), Type);
            list.AddRange(from RegimenName in RegimenNames
                          let RegimenValue = ReflectUtil.GetValue(pkm, RegimenName)
                          where RegimenValue is bool
                          select new RegimenInfo(RegimenName, (bool) RegimenValue));
            TLP.ColumnCount = 2;
            TLP.RowCount = 0;
            
            // Add Regimens
            foreach (var reg in list)
                addRegimenChoice(reg);

            // Force auto-size
            foreach (RowStyle style in TLP.RowStyles)
                style.SizeType = SizeType.AutoSize;
            foreach (ColumnStyle style in TLP.ColumnStyles)
                style.SizeType = SizeType.AutoSize;
        }
        private void addRegimenChoice(RegimenInfo reg)
        {
            // Get row we add to
            int row = TLP_SuperTrain.RowCount;
            TLP_SuperTrain.RowCount++;

            var label = new Label
            {
                Anchor = AnchorStyles.Left,
                Name = PrefixLabel + reg.Name,
                Text = reg.Name,
                Padding = Padding.Empty,
                AutoSize = true,
            };
            TLP_SuperTrain.Controls.Add(label, 1, row);

            var chk = new CheckBox
            {
                Anchor = AnchorStyles.Right,
                Name = PrefixCHK + reg.Name,
                AutoSize = true,
                Padding = Padding.Empty,
            };
            chk.CheckedChanged += (sender, e) => { reg.CompletedRegimen = chk.Checked; };
            chk.Checked = reg.CompletedRegimen;
            TLP_SuperTrain.Controls.Add(chk, 0, row);
        }

        private void save()
        {
            foreach (var reg in reglist)
                ReflectUtil.SetValue(pkm, reg.Name, reg.CompletedRegimen);
            foreach (var reg in distlist)
                ReflectUtil.SetValue(pkm, reg.Name, reg.CompletedRegimen);

            if (CB_Bag.Visible)
            {
                (pkm as PK6).TrainingBag = CB_Bag.SelectedIndex;
                (pkm as PK6).TrainingBagHits = (int)NUD_BagHits.Value;
            }

            Main.pkm = pkm;
        }
        
        private class RegimenInfo
        {
            public readonly string Name;
            public bool CompletedRegimen;
            public RegimenInfo(string name, bool completedRegimen)
            {
                Name = name;
                CompletedRegimen = completedRegimen;
            }
        }

        private void B_All_Click(object sender, EventArgs e)
        {
            CHK_Secret.Checked = true;
            foreach (var c in TLP_SuperTrain.Controls.OfType<CheckBox>())
                c.Checked = true;
            foreach (var c in TLP_DistSuperTrain.Controls.OfType<CheckBox>())
                c.Checked = true;
        }
        private void B_None_Click(object sender, EventArgs e)
        {
            CHK_Secret.Checked = false;
            foreach (var c in TLP_SuperTrain.Controls.OfType<CheckBox>())
                c.Checked = false;
            foreach (var c in TLP_DistSuperTrain.Controls.OfType<CheckBox>())
                c.Checked = false;
        }
        private void CHK_Secret_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var c in TLP_SuperTrain.Controls.OfType<CheckBox>().Where(chk => Convert.ToInt16(chk.Name[10]) > 4))
            {
                c.Enabled = CHK_Secret.Checked;
                if (!CHK_Secret.Checked)
                    c.Checked = false;
            }
        }
    }
}
