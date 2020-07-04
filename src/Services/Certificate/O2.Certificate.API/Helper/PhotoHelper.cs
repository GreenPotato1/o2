using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using O2.Black.Toolkit.Core;
using O2.Business.Data.Models.O2C;

namespace O2.Business.API.Helper
{
    public static class PhotoHelper
    {
        public static async Task LoadCertificates(IEnumerable<O2CCertificate> list)
        {
            foreach (var item in list)
            {
                var filename = item.Serial + item.Number;
                var pathPhoto = "Files/PFR_Photos/" + filename+".jpg";
                if (!File.Exists(pathPhoto))
                    continue;

                var photo = new O2CPhoto
                {
                    FileName = filename.ToUpper().ToString() + '_' + DateTime.Now.ConvertToUnixTime() +
                               Path.GetExtension(pathPhoto).ToLower()
                };
                using (var stream = new FileStream(pathPhoto, FileMode.Open, FileAccess.Read))
                {
                    photo.Url = AzureBlobHelper.UploadFileToStorage(stream,
                        fileName: photo.FileName,
                        TypeTable.Certificates).GetAwaiter().GetResult();
                    photo.IsMain = true;
                    item.Photos.Add(photo);
                }
            }
        }
    }
}