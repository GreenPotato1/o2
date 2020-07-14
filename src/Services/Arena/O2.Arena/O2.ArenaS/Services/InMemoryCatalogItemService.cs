using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using O2.ArenaS.Data;

namespace O2.ArenaS.Services
{
    public class InMemoryCatalogItemService: ICatalogItemService
    {
        private static readonly Random RandomGenerator = new Random();
        private readonly List<CatalogItem> _certificates = new List<CatalogItem>();
        private int _currentCertificate = 0;

        public InMemoryCatalogItemService()
        {
           
        }
        // public static string GetDLLFolder()
        // {
        //     string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        //     UriBuilder uri = new UriBuilder(codeBase);
        //     string path = Uri.UnescapeDataString(uri.Path);
        //     return Path.GetDirectoryName(path);
        // }
        public async Task GetData()
        {
            await Task.Delay(100);
            _certificates.Add(new CatalogItem()
            {
                Id = 1,
                Category = "Test"
            });
            //         new CatalogItem()
            //         {
            //             Id = id,
            //             ShortNumber = certDto.ShortNumber,
            //             Serial = certDto.Serial,
            //             Number = certDto.Number,
            //             Lastname = certDto.Lastname,
            //             Firstname = certDto.Firstname,
            //             Middlename = certDto.Middlename,
            //             Education = certDto.Education,
            //             DateOfCert = certDto.DateOfCert,
            //             Visible = certDto.Visible,
            //             Lock = certDto.Lock
            //         }
            //         );
            // var path = GetDLLFolder()+ "/Services/Import_DB_PFR.json";
            //
            // var str = System.IO.File.ReadAllText(path);
            // var certificationForListDto = JsonConvert.DeserializeObject<List<CatalogItem>>(str);
            // var id = 0;
            // foreach (var certDto in certificationForListDto)
            // {
            //     _certificates.Add(
            //         new CatalogItem()
            //         {
            //             Id = id,
            //             ShortNumber = certDto.ShortNumber,
            //             Serial = certDto.Serial,
            //             Number = certDto.Number,
            //             Lastname = certDto.Lastname,
            //             Firstname = certDto.Firstname,
            //             Middlename = certDto.Middlename,
            //             Education = certDto.Education,
            //             DateOfCert = certDto.DateOfCert,
            //             Visible = certDto.Visible,
            //             Lock = certDto.Lock
            //         }
            //         );
            //     ++id;
            // }
        }
        public Task<IReadOnlyCollection<CatalogItem>> GetAllAsync(CancellationToken ct)
        {
            GetData().GetAwaiter().GetResult();
            return Task.FromResult<IReadOnlyCollection<CatalogItem>>(_certificates.AsReadOnly());
        }
        
        public async Task<CatalogItem> GetByIdAsync(int id,CancellationToken ct)
        {            
            GetData().GetAwaiter().GetResult();
            await Task.Delay(1000,ct);
            var extResult1Task = CallExternalServiceAsync(ct);
            var extResult2Task = CallExternalServiceAsync(ct);
            await Task.WhenAll(extResult1Task,extResult2Task);
            return _certificates.SingleOrDefault(x=>x.Id==id);
        }

        public Task<CatalogItem> UpdateAsync(CatalogItem certificate,CancellationToken ct)
        {
            GetData().GetAwaiter().GetResult();
            var toUpdate = _certificates.SingleOrDefault(x => x.Id == certificate.Id);
            if (toUpdate ==null)
            {
                return null;
            }
            
            toUpdate.Category = certificate.Category;
            return Task.FromResult(toUpdate);
        }

        public Task<CatalogItem> AddAsync(CatalogItem catalogItem,CancellationToken ct)
        {
            GetData().GetAwaiter().GetResult();
            catalogItem.Id = ++_currentCertificate;
            _certificates.Add(catalogItem);
            return Task.FromResult(catalogItem);
        }

        public async Task RemoveAsync(int id, CancellationToken ct)
        {
            GetData().GetAwaiter().GetResult();
            await Task.Delay(1000,ct);
            var toDelete = _certificates.SingleOrDefault(x => x.Id == id);
            _certificates.Remove(toDelete);
        }

        private static async Task<int> CallExternalServiceAsync(CancellationToken ct)
        {
            await Task.Delay(1000, ct);
            return RandomGenerator.Next();
        }
    }
}
