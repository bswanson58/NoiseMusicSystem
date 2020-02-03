using System;
using System.ComponentModel;

namespace ReusableBits.Mvvm.VersionSpinner {
    public interface IVersionFormatter : INotifyPropertyChanged, IDisposable {
        void            SetVersion( Version version );
        void            StartFormatting();

        VersionLevel    DisplayLevel { get; set; }
        string          VersionString { get; }

        int            Major { get; }
        bool           DisplayMajor { get; }
        
        int            Minor { get; }
        bool           DisplayMinor { get; }
        
        int            Build { get; }
        bool           DisplayBuild { get; }
        
        int            Revision { get; }
        bool           DisplayRevision { get; }
    }
}
