﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintekLaserFoam
{
    public class MarlinCore : GrblCore
    {
        public MarlinCore(System.Windows.Forms.Control syncroObject, PreviewForm cbform) : base(syncroObject, cbform)
        {
        }

        public override Firmware Type
        { get { return Firmware.Marlin; } }

        // Send "M114\n" to retrieve position
        // Typical response : "X:10.00 Y:0.00 Z:0.00 E:0.00 Count X:1600 Y:0 Z:0"
        protected override void QueryPosition()
        {
            if (MachineStatus != MacStatus.Run)
                com.Write("M114\n");
        }

        //protected override void ParseF(string p)
        //{
        //    string sfs = p.Substring(2, p.Length - 2);
        //    string[] fs = sfs.Split(",".ToCharArray());
        //    SetFS(ParseFloat(fs[0]), ParseFloat(fs[1]));
        //}

        public override StreamingMode CurrentStreamingMode => StreamingMode.Synchronous;
        public override bool UIShowGrblConfig => false;
        public override bool UIShowUnlockButtons => false;
        public override bool SupportTrueJogging => false;
        internal override void SendUnlockCommand() { } //do nothing (should not be called because UI does not show unlock button)
        protected override void SendBoardResetCommand() { }

        protected override void ParseMachineStatus(string data)
        {
            MacStatus var = MacStatus.Disconnected;

            if (data.Contains("ok"))
                var = MacStatus.Idle;

            //try { var = (MacStatus)Enum.Parse(typeof(MacStatus), data); }
            //catch (Exception ex) { Logger.LogException("ParseMachineStatus", ex); }

            if (InProgram && var == MacStatus.Idle) //bugfix for grbl sending Idle on G4
                var = MacStatus.Run;

            if (var == MacStatus.Hold && !mHoldByUserRequest)
                var = MacStatus.Cooling;

            SetStatus(var);
        }

        public override void RefreshConfig()
        {

        }

        protected override void ManageRealTimeStatus(string rline)
        {
            try
            {
                debugLastStatusDelay.Start();

                // Remove EOL
                rline = rline.Trim(trimarray);

                // Marlin M114 response : 
                // X:10.00 Y:0.00 Z:0.00 E:0.00 Count X:1600 Y:0 Z:0
                // Split by space
                string[] arr = rline.Split(" ".ToCharArray());

                if (arr.Length > 0)
                {
                    // Force update of status 
                    ParseMachineStatus("ok");

                    // Retrieve position from data send by marlin
                    float x = float.Parse(arr[0].Split(":".ToCharArray())[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    float y = float.Parse(arr[1].Split(":".ToCharArray())[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    float z = float.Parse(arr[2].Split(":".ToCharArray())[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    SetMPosition(new GPoint(x, y, z));
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage("ManageRealTimeStatus", "Ex on [{0}] message", rline);
                Logger.LogException("ManageRealTimeStatus", ex);
            }
        }
        protected override void DetectHang()
        {
            if (mTP.LastIssue == DetectedIssue.Unknown && MachineStatus == MacStatus.Run && InProgram)
            {
                // Marlin does not answer to immediate command
                // So we can not rise an issue if there is too many time before last call of ManageRealTimeStatus
                // We rise the issue if the last response from the board was too long

                // Original line :
                //bool noQueryResponse = debugLastMoveDelay.ElapsedTime > TimeSpan.FromTicks(QueryTimer.Period.Ticks * 10) && debugLastStatusDelay.ElapsedTime > TimeSpan.FromSeconds(5);
                // Marlin version :
                bool noQueryResponse = debugLastMoveDelay.ElapsedTime > TimeSpan.FromSeconds(10) && debugLastMoveDelay.ElapsedTime > TimeSpan.FromTicks(QueryTimer.Period.Ticks * 10) && debugLastStatusDelay.ElapsedTime > TimeSpan.FromSeconds(5);

                if (noQueryResponse)
                    SetIssue(DetectedIssue.StopResponding);

            }
        }

        // Return true if message received start with X:
        protected override bool IsRealtimeStatusMessage(string rline)
        {
            return rline.StartsWith("X:");
        }

        // PrintekLaserFoam don't ask status to marlin during code execution because there is no immediate command
        // So PrintekLaserFoam has to force the status at the end of programm execution
        protected override void ForceStatusIdle ()
        {
            SetStatus(MacStatus.Idle);
        }

    }

}
