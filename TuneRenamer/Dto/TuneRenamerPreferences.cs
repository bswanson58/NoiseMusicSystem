﻿using System;

namespace TuneRenamer.Dto {
    public class TuneRenamerPreferences {
        public  string      SourceDirectory { get; set; }
        public  bool        SkipUnderscoreDirectories {  get; set; }

        public TuneRenamerPreferences() {
            SourceDirectory = String.Empty;
            SkipUnderscoreDirectories = true;
        }
    }
}