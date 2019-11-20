﻿using System;

namespace Album4Matter.Dto {
    public class Album4MatterPreferences {
        public  string      SourceDirectory { get; set; }
        public  string      TargetDirectory { get; set; }

        public Album4MatterPreferences() {
            SourceDirectory = String.Empty;
            TargetDirectory = String.Empty;
        }
    }
}