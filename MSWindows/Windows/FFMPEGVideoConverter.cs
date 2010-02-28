﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace Mirosubs.Converter.Windows {
    class FFMPEGVideoConverter : VideoConverter {
        private readonly string args;

        private static Regex durationRegex = new Regex(@"^\s+Duration: (\d\d:\d\d:\d\d.\d\d)");
        private static Regex timeRegex = new Regex(@"time=([^\s]+)\u0020bitrate");
        private static Regex finishedRegex = new Regex(@"^video:");
        private static Regex unknownFormatRegex = new Regex(@"Unknown format$");

        private long lengthMs = -1;
        private string outputFileName;

        internal FFMPEGVideoConverter(string fileName, VideoFormat format) {
            if (format == VideoFormat.G1)
                args = string.Format(
                    "-i \"{0}\" -y -fpre \"{1}\" -aspect 3:2 -s 400x300 -r 23.976 " +
                    "-vcodec libx264 -b 480k -acodec aac -ab 96k -threads 0 " +
                    "\"{2}\"",
                    fileName,
                    Path.Combine(ExecutableDir, 
                        @"ffmpeg-bin\libx264hq.ffpreset"),
                    outputFileName = Path.ChangeExtension(fileName, ".g1.mp4"));
            else if (format == VideoFormat.PSP)
                args = string.Format(
                    "-i \"{0}\" -y -b 300k -s 320x240 -vcodec libxvid -ab 32k " +
                    "-ar 24000 -acodec aac \"{1}\"", fileName,
                    outputFileName = Path.ChangeExtension(fileName, ".psp.mp4"));
            else
                throw new ArgumentException();
        }
        protected override string ConversionExeName {
            get { return @"ffmpeg-bin/ffmpeg.exe"; }
        }
        protected override string ConversionArgs {
            get { return args; }
        }
        public override string OutputFileName {
            get { return outputFileName; }
        }
        protected override void process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e) {
            // really just for entertainment, since ffmpeg 
            // doesn't seem to send anything to stdout.
            Debug.Print("OUTPUT");
            Debug.Print(e.Data);
        }
        protected override void process_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e) {
            Debug.Print("ERROR");
            Debug.Print(e.Data);
            string line = e.Data;
            if (line == null)
                return;
            IssueConvertOutputEvent(line);
            if (lengthMs == -1 && durationRegex.IsMatch(line)) {
                Match m = durationRegex.Match(line);
                lengthMs = (long)TimeSpan.Parse(m.Groups[1].Value).TotalMilliseconds;
            }
            else if (timeRegex.IsMatch(line)) {
                Match m = timeRegex.Match(line);
                string[] components = m.Groups[1].Value.Split(':', '.');
                long ms = 0;
                long[] factors = new long[] { 10, 100, 60, 60 };
                long curFactor = 1;
                for (int i = 0; i < components.Length; i++) {
                    curFactor *= factors[i];
                    ms += Int32.Parse(components[components.Length - 1 - i]) * curFactor;
                }
                IssueConvertProgressEvent((int)(100 * ms / lengthMs));
            }
            else if (finishedRegex.IsMatch(line))
                IssueFinishedEvent();
            else if (unknownFormatRegex.IsMatch(line))
                IssueUnknownFormatEvent();
        }
    }
}