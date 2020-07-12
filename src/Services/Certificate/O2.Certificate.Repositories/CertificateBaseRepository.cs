using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using O2.Black.Toolkit.Core;
using O2.Black.Toolkit.Core.Data;
using O2.Business.Repositories.Core;
using O2.Business.Repositories.Helper;
using O2.Business.Repositories.Interfaces;
using O2.Certificate.Data;
using O2.Certificate.Data.Models.O2C;

namespace O2.Business.Repositories
{
    public class CertificateBaseRepository<TClass> : BaseRepository<TClass>,
        ICertificateBaseRepository<TClass>
        where TClass : O2CCertificate, IEntity
    {
        #region Ctors

        public CertificateBaseRepository(O2BusinessDataContext context) : base(context)
        {
        }

        #endregion

        public override async Task<TClass> GetAsync(Guid id)
        {
            return await DataContext.GetDataSet<TClass>()
                .Include(p => p.Locations)
                .ThenInclude(x => x.O2CLocation)
                .Include(p => p.Contacts)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public override async Task<IEnumerable<TClass>> GetAllAsync()
        {
            var itemType = await DataContext.GetDataSet<TClass>()
                .Include(p => p.Locations)
                .ThenInclude(x => x.O2CLocation)
                .Include(p => p.Contacts)
                .Include(p => p.Photos)
                .OrderBy(x => x.ShortNumber)
                .ToListAsync();

            return itemType;
        }

        public async Task<IEnumerable<TClass>> GetAllAsync(bool showAll)
        {
            if (!showAll)
                return await GetAllAsync();
            var itemType = await DataContext.GetDataSet<TClass>()
                .Include(p => p.Locations)
                .ThenInclude(x => x.O2CLocation)
                .Include(p => p.Contacts)
                .Include(p => p.Photos)
                .OrderBy(x => x.ShortNumber)
                .ToListAsync();

            return itemType.Where(item => item.Visible == true).ToList();
        }


        public override Task<TClass> UpdateAsync(TClass entity)
        {
            NewMethod(entity);

            return base.UpdateAsync(entity);
        }


        public override void CloneForUpdate(TClass sourceEntity, TClass exist)
        {
            exist.Firstname = sourceEntity.Firstname;
            exist.Lastname = sourceEntity.Lastname;
            exist.Middlename = sourceEntity.Middlename;
            exist.Visible = sourceEntity.Visible;
            exist.Number = sourceEntity.Number;
            exist.ShortNumber = sourceEntity.ShortNumber;
            exist.Serial = sourceEntity.Serial;
            exist.DateOfCert = sourceEntity.DateOfCert;
            exist.Education = sourceEntity.Education;

            //ToDO: create Update operation for Constacts, Photos, Locations
        }

        public override async Task<TClass> AddBaseAsync(TClass entity)
        {
            NewMethod(entity);
            // var result = await DataContext.AddAsync(entity);
            // await SaveAllAsync();
            // return result.Entity;
            return await base.AddBaseAsync(entity);
        }

        private void NewMethod(TClass entity)
        {
            // attach locations
            foreach (var location in entity.Locations)
            {
                if (DataContext.O2CLocation != null && location.O2CLocation != null)
                {
                    if (!DataContext.O2CLocation.Any())
                    {
                        location.O2CCertificate = entity;
                        var existLocation = DataContext.O2CLocation.SingleOrDefault(x =>
                            x.Country == location.O2CLocation.Country
                            && x.Region ==
                            location.O2CLocation.Region);
                            
                        if (existLocation!=null)
                        {
                            location.O2CLocation = existLocation;
                            DataContext.Attach(location.O2CLocation);
                            // DataContext.O2CLocation.Add(location.O2CLocation);
                            DataContext.Attach(location);
                            DataContext.O2CCertificateLocation.Add(location);
                        }
                        else
                        {
                            DataContext.Attach(location.O2CLocation);
                            DataContext.O2CLocation.Add(location.O2CLocation);
                            DataContext.Attach(location);
                            DataContext.O2CCertificateLocation.Add(location);
                        }
                    }
                    else
                    {
                        if (DataContext.O2CLocation.FirstOrDefault(x =>
                                x.Id == location.O2CLocation.Id) == null)
                        {
                            location.O2CCertificate = entity;
                           
                            var existLocation = DataContext.O2CLocation.SingleOrDefault(x =>
                                x.Country == location.O2CLocation.Country
                                && x.Region ==
                                location.O2CLocation.Region);
                            
                            if (existLocation!=null)
                            {
                                location.O2CLocation = existLocation;
                                DataContext.Attach(location.O2CLocation);
                                // DataContext.O2CLocation.Add(location.O2CLocation);
                                DataContext.Attach(location);
                                DataContext.O2CCertificateLocation.Add(location);
                            }
                            else
                            {
                                DataContext.Attach(location.O2CLocation);
                                DataContext.O2CLocation.Add(location.O2CLocation);
                                DataContext.Attach(location);
                                DataContext.O2CCertificateLocation.Add(location);
                            }
                        }
                    }
                }
            }

            // attach photos
            foreach (var photo in entity.Photos)
            {
                if (DataContext.O2CPhoto != null && photo != null)
                {
                    if (!DataContext.O2CPhoto.Any())
                    {
                        photo.O2CCertificate = entity;
                        DataContext.Attach(photo);
                        DataContext.O2CPhoto.Add(photo);
                    }
                    else
                    {
                        if (DataContext.O2CPhoto.FirstOrDefault(p => p.Id == photo.Id) == null)
                        {
                            photo.O2CCertificate = entity;
                            DataContext.Attach(photo);
                            DataContext.O2CPhoto.Add(photo);
                        }
                    }
                }
            }

            // attach contacts
            foreach (var contact in entity.Contacts)
            {
                if (!DataContext.O2CContact.Any())
                {
                    contact.O2CCertificate = entity;
                    DataContext.Attach(contact);
                    DataContext.O2CContact.Add(contact);
                }
                else
                {
                    if (DataContext.O2CContact != null && contact != null)
                    {
                        if (DataContext.O2CContact.FirstOrDefault(c => c.Id == contact.Id) == null)
                        {
                            contact.O2CCertificate = entity;
                            DataContext.Attach(contact);
                            DataContext.O2CContact.Add(contact);
                        }
                    }
                }
            }
        }

        public async Task<TClass> LoadPhoto(TClass existEvent, O2CPhoto o2CPhoto)
        {
            DataContext.Attach(o2CPhoto);
            DataContext.O2CPhoto.Add(o2CPhoto);
            return await AddOrUpdateAsync(existEvent);
        }

        public async Task<List<TClass>> AddRangeAsync(List<TClass> listEntities, bool cleanData)
        {
            if (cleanData)
            {
                var all = await GetAllAsync();
                foreach (var getEntity in all)
                {
                    if (getEntity.Photos != null && getEntity.Photos.Any())
                    {
                        foreach (var photo in getEntity.Photos)
                        {
                            AzureBlobHelper.DeletePhoto(photo.FileName, TypeTable.Certificates);
                            DataContext.Entry(photo).State = EntityState.Deleted;
                        }
                    }

                    if (getEntity.Contacts != null && getEntity.Contacts.Any())
                    {
                        foreach (var contact in getEntity.Contacts)
                        {
                            DataContext.Entry(contact).State = EntityState.Deleted;
                        }
                    }

                    if (getEntity.Locations != null && getEntity.Locations.Any())
                    {
                        foreach (var location in getEntity.Locations)
                        {
                            DataContext.Entry(location).State = EntityState.Deleted;
                        }
                    }
                    

                    // DataContext.Entry(getEntity).Collection("Locations").Load();
                    // DataContext.Entry(getEntity).Collection("Photos").Load();
                    // DataContext.Entry(getEntity).Collection("Contacts").Load();
                    DataContext.Entry(getEntity).State = EntityState.Deleted;
                }
            }

            DataContext.SaveChanges();
            return await base.AddRangeAsync(listEntities);
        }

        public async Task<PagedList<TClass>> GetAllAsync(CertificateParam certificateParam, bool actualInfo)
        {
            var certs = await GetAllAsync(actualInfo);
            return PagedList<TClass>.Create(certs.AsQueryable(), certificateParam.PageNumber,
                certificateParam.PageSize);
        }
    }
}