using System;
using System.ComponentModel.DataAnnotations;
using Syncfusion.iOS.DataForm;

namespace CustomizeLabelEditorLayout
{

    public class ContactInfo
    {
        #region Fields

        private int? contactNumber;
        private string address;      
        private string name;
        public string image;
        #endregion

        #region Constructor

        public ContactInfo()
        {

        }

        #endregion

        [DisplayOptions(RowSpan = 5, ShowLabel = false)]
        public string Image
        {
            get { return this.image; }
            set { this.image = value; }
        }

        [DisplayOptions(RowSpan = 2)]
        public string Name
        {
            get { return name; }
            set { this.name = value; }
        }

        public string Address
        {
            get { return this.address; }
            set { this.address = value; }
        }

        [Display(Name = "Contact Number")]
        public int? ContactNumber
        {
            get { return this.contactNumber; }
            set { this.contactNumber = value; }
        }
    }
}
