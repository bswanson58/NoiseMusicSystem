using System;
using Album4Matter.Dto;

namespace Album4Matter.Interfaces {
    public enum InspectionItem {
        Source,
        Artist,
        Album,
        Date
    }

    public class InspectionItemUpdate {
        public  InspectionItem  ItemChanged { get; }
        public  string          Item { get; }

        public InspectionItemUpdate( InspectionItem item, string newValue ) {
            ItemChanged = item;
            Item = newValue;
        }
    }

    public interface IItemInspectionViewModel {
        void    SetInspectionItem( SourceItem item );

        IObservable<InspectionItemUpdate>   InspectionItemChanged { get; }
    }
}
