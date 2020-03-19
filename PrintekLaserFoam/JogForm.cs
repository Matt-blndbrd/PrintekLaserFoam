﻿using System;
using System.Windows.Forms;

namespace PrintekLaserFoam
{
	public partial class JogForm : System.Windows.Forms.UserControl
	{
		GrblCore Core;

		public JogForm()
		{
			InitializeComponent();
            SettingsForm.SettingsChanged += SettingsForm_SettingsChanged;
		}

        public void SetCore(GrblCore core)
		{
			Core = core;

			UpdateFMax.Enabled = true;
			UpdateFMax_Tick(null, null);

			TbSpeed.Value = Math.Max(Math.Min((int)Settings.GetObject("Jog Speed", 1000), TbSpeed.Maximum), TbSpeed.Minimum);
            
			TbStep.Value = Convert.ToDecimal(Settings.GetObject("Jog Step", 10M));

			TbSpeed_ValueChanged(null, null); //set tooltip
			TbStep_ValueChanged(null, null); //set tooltip

            Core.JogStateChange += Core_JogStateChange;
            SettingsForm_SettingsChanged(this, null);
        }

        private void SettingsForm_SettingsChanged(object sender, EventArgs e)
        {
            TlpStepControl.Visible = !(bool)Settings.GetObject("Enable Continuous Jog", true);
            TlpZControl.Visible = (bool)Settings.GetObject("Enale Z Jog Control", false);
        }

        private void Core_JogStateChange(bool jog)
        {
            BtnHome.Visible = !jog;
        }

        private void OnJogButtonMouseDown(object sender, MouseEventArgs e)
		{
			Core.BeginJog((sender as DirectionButton).JogDirection);
		}

        private void OnJogButtonMouseUp(object sender, MouseEventArgs e)
        {
            Core.EndJogV11();
        }

        private void OnZJogButtonMouseDown(object sender, MouseEventArgs e)
        {
            Core.EnqueueZJog((sender as DirectionStepButton).JogDirection, (sender as DirectionStepButton).JogStep);
        }

        private void TbSpeed_ValueChanged(object sender, EventArgs e)
		{

			
			TT.SetToolTip(TbSpeed, $"{Strings.SpeedSliderToolTip} {TbSpeed.Value}");
			LblSpeed.Text = String.Format("F{0}", TbSpeed.Value);
			Settings.SetObject("Jog Speed", TbSpeed.Value);
			Core.JogSpeed = TbSpeed.Value;
			needsave = true;
		}

		private void TbStep_ValueChanged(object sender, EventArgs e)
		{
			TT.SetToolTip(TbStep, $"{Strings.StepSliderToolTip} {TbStep.Value}");
			LblStep.Text = TbStep.Value.ToString();
			Settings.SetObject("Jog Step", TbStep.Value);
			Core.JogStep = TbStep.Value;
			needsave = true;
		}

		bool needsave = false;
		private void OnSliderMouseUP(object sender, MouseEventArgs e)
		{
			if (needsave)
			{
				needsave = false;
				Settings.Save();
			}
		}

		int oldVal;
		private void UpdateFMax_Tick(object sender, EventArgs e)
		{
			int curVal = (int)Math.Max(Core.Configuration.MaxRateX, Core.Configuration.MaxRateY);
			if (oldVal != curVal)
			{
				TbSpeed.Value = Math.Min(TbSpeed.Value, curVal);
				TbSpeed.Maximum = curVal;
				TbSpeed.LargeChange = curVal / 10;
				TbSpeed.SmallChange = curVal / 20;
				oldVal = curVal;
			}
		}

        private void TbStep_Scroll(object sender, EventArgs e)
        {

        }
    }

    public class StepBar : System.Windows.Forms.TrackBar
    {
        decimal[] values = { 0.1M, 0.2M, 0.5M, 1, 2, 5, 10, 20, 50, 100, 200 };

        public StepBar()
        {
            Minimum = 0;
            Maximum = values.Length -1;
            SmallChange = LargeChange = 1;
        }

        private int CurIndex { get { return base.Value; } set { base.Value = value; } }

        public new decimal Value
        {
            get
            {
                return values[CurIndex];
            }
            set
            {
                int found = 0;
                for (int index = 0; index < values.Length; index++)
                {
                    if (Math.Abs(value - values[index]) < Math.Abs(value - values[found]))
                        found = index;
                }
                CurIndex = found;
            }
        }

    }


    public class DirectionButton : UserControls.ImageButton
	{
		private GrblCore.JogDirection mDir = GrblCore.JogDirection.N;

		public GrblCore.JogDirection JogDirection
		{
			get { return mDir; }
			set { mDir = value; }
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if (Width != Height)
				Width = Height;

			base.OnSizeChanged(e);
		}
	}

    public class DirectionStepButton : DirectionButton
    {
        private decimal mStep = 1.0M;

        public decimal JogStep
        {
            get { return mStep; }
            set { mStep = value; }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (Width != Height)
                Width = Height;

            base.OnSizeChanged(e);
        }
    }
}
