using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.IO;

namespace FireAndIce.ViewModels
{
    public class PictureViewModel : PropertyChangedBase
    {
        public Uri Name { get; set; }

        public PictureViewModel(string name)
        {
            String path = Path.GetFullPath("Images");
            String pictureFile = "\\";
            String actualFile = path + pictureFile + name + ".png";

            Name = new Uri(actualFile, UriKind.Absolute);
            NotifyOfPropertyChange(() => Name);
        }

        public void PoemTime()
        {
            if (Name.ToString().ToUpper().Contains("SCOT"))
            {
                String path = Path.GetFullPath("Images");
                String pictureFile = "\\";
                String actualFile = path + pictureFile + "Bits and Bytes" + ".png";

                Name = new Uri(actualFile, UriKind.Absolute);
                NotifyOfPropertyChange(() => Name);                
            }
        }
    }
}
