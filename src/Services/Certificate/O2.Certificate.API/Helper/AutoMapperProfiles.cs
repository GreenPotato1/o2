using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using O2.Certificate.API.DTOs;
using O2.Certificate.API.DTOs.O2C;
using O2.Certificate.API.DTOs.O2Ev;
using O2.Certificate.Data.Models.O2C;
using O2.Certificate.Data.Models.O2Ev;

namespace O2.Certificate.API.Helper
{
    public class AutoMapperProfiles : Profile
    {
        private static string ConverterToPhotos<TSourceMember>(IEnumerable<O2CPhoto> srcPhotos)
        {
            var result = srcPhotos.FirstOrDefault(p => p.IsMain)?.Url;
            return string.IsNullOrEmpty(result) ? HelperDefaulter.UrlCertificates : result;
        }

        private static string ConverterToLocations<T>(IEnumerable<O2CCertificateLocation> srcLocations)
        {
            var o2CCertificateLocations = srcLocations as O2CCertificateLocation[] ?? srcLocations.ToArray();

            if (!o2CCertificateLocations.Any())
                return string.Empty;
            var result = o2CCertificateLocations.Aggregate(string.Empty, (current,
                    item) => current + (item.O2CLocation.Country + " - " + item.O2CLocation.Region));

            return string.IsNullOrEmpty(result) ? "" : result;
        }

        private static string ConverterToContacts<TSourceMember>(IEnumerable<O2CContact> srcContacts)
        {
            var o2CContacts = srcContacts as O2CContact[] ?? srcContacts.ToArray();

            if (!o2CContacts.Any())
                return string.Empty;

            var result = string.Empty;
            foreach (var contact in o2CContacts)
                result = result + TemplateHelper.Convert(contact.Key, contact.Value);
            ;

            return string.IsNullOrEmpty(result) ? "" : result;
        }

        public AutoMapperProfiles()
        {
            Mapper.Reset();

            #region Certificates

            CreateMap<O2CCertificate, O2CCertificateForCreateDto>();
            CreateMap<O2CCertificateForCreateDto, O2CCertificate>();
            // .ForMember(dto => dto.Locations, opt
            //     => opt.MapFrom(x => x.Locations.Select(y => y.O2CLocation).ToList()));

            // .ForMember(dest=>dest.Id, opt =>
            // {
            //     opt.MapFrom(src=>src.Id);
            // })
            // .ForMember(dest=>dest.Firstname, opt =>
            // {
            //     opt.MapFrom(src=>src.Firstname);
            // })
            // .ForMember(dest=>dest.Lastname, opt =>
            // {
            //     opt.MapFrom(src=>src.Lastname);
            // })
            // .ForMember(dest=>dest.Middlename, opt =>
            // {
            //     opt.MapFrom(src=>src.Middlename);
            // })
            // ;

            CreateMap<O2CPhoto, PhotoForReturnDto>();
            CreateMap<PhotoForReturnDto, O2CPhoto>();

            CreateMap<O2CLocation, O2CLocationDto>();
            CreateMap<O2CLocationDto, O2CLocation>();

            CreateMap<O2CContact, O2CContactDto>();
            CreateMap<O2CContactDto, O2CContact>();

            CreateMap<O2CCertificate, O2CCertificateForListDto>()
                .ForMember(
                    dest => dest.PhotoUrl,
                    opt =>
                    {
                        opt.MapFrom(mapExpression: src =>
                            ConverterToPhotos<string>(src.Photos)
                        );
                    })
                .ForMember(
                    dest => dest.AllContacts,
                    opt =>
                    {
                        opt.MapFrom(mapExpression: src =>
                            ConverterToContacts<string>(src.Contacts)
                        );
                    })
                .ForMember(
                    dest => dest.AllLocations,
                    opt =>
                    {
                        opt.MapFrom(mapExpression: src =>
                            ConverterToLocations<string>(src.Locations)
                        );
                    });

            CreateMap<O2CCertificateForListDto, O2CCertificate>();

            CreateMap<O2CCertificate, O2CCertificateForReturnDto>()
                .ForMember(
                    dest => dest.PhotoUrl,
                    opt =>
                    {
                        opt.MapFrom(mapExpression: src =>
                            ConverterToPhotos<string>(src.Photos)
                        );
                    })
                .ForMember(
                    dest => dest.AllContacts,
                    opt =>
                    {
                        opt.MapFrom(mapExpression: src =>
                            ConverterToContacts<string>(src.Contacts)
                        );
                    })
                .ForMember(
                    dest => dest.AllLocations,
                    opt =>
                    {
                        opt.MapFrom(mapExpression: src =>
                            ConverterToLocations<string>(src.Locations)
                        );
                    });

            CreateMap<O2CCertificateForReturnDto, O2CCertificate>();

            #endregion

            #region Events

            CreateMap<O2EvEvent, O2EvEventReturnDto>()
                .ForMember(dest => dest.PhotoUrl,
                    opt => { opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url); });


            CreateMap<O2EvEvent, O2EvEventForCreateDto>();


            CreateMap<O2EvEventForCreateDto, O2EvEvent>();

            CreateMap<O2EvEvent, O2EvEventForListDto>()
                .ForMember(dest => dest.PhotoUrl,
                    opt => { opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url); });
            CreateMap<O2EvEventForListDto, O2EvEvent>();

            #endregion

            #region Mapping Meta of Location

            CreateMap<O2EvMeta, O2EvMetaDto>();
            CreateMap<O2EvMetaDto, O2EvMeta>();

            #endregion
        }
    }

    internal class TemplateHelper
    {
        public static string Convert(string contactKey, string contactValue)
        {
            switch (@contactKey)
            {
                case ("phone"):
                {
                    return "<noindex>" +
                           "Телефон: " +
                           "<a rel=\"nofollow\" href=\"tel:+" + contactValue + "\"> +" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                case ("vk"):
                {
                    return "<noindex>" +
                           "Вконтакте: " +
                           "<a rel=\"nofollow\" href=\"" + contactValue + "\">" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                case ("fb"):
                {
                    return "<noindex>" +
                           "Facebook: " +
                           "<a rel=\"nofollow\" href=\"" + contactValue + "\">" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                case ("site"):
                {
                    return "<noindex>" +
                           "сайт: " +
                           "<a rel=\"nofollow\" href=\"" + contactValue + "\">" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                case ("whatsapp"):
                {
                    return "<noindex>" +
                           "whatsapp: " +
                           "<a rel=\"nofollow\" href=\"" + contactValue + "\"> +" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                case ("viber"):
                {
                    return "<noindex>" +
                           "viber: " +
                           "<a rel=\"nofollow\" href=\"" + contactValue + "\"> +" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                case ("telegram"):
                {
                    return "<noindex>" +
                           "telegram: " +
                           "<a rel=\"nofollow\" href=\"" + contactValue + "\"> +" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                case ("instagram"):
                {
                    return "<noindex>" +
                           "instagram: " +
                           "<a rel=\"nofollow\" href=\"" + contactValue + "\">" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                case ("email"):
                {
                    return "<noindex>" +
                           "email: " +
                           "<a rel=\"nofollow\" href=\"" + contactValue + "\">" + contactValue + "</a>" +
                           "<noindex><br>";
                    break;
                }
                default:
                    throw new Exception("Not found template for the contact "+contactKey+" = "+contactValue);
                    break;
            }
        }
    }
}