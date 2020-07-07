using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using O2.Black.Toolkit.Core;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using O2.Certificate.API.DTOs.O2C;
using O2.Certificate.API.Helper;
using O2.Certificate.Data.Models.O2C;
using O2.Certificate.Repositories.Helper;
using O2.Certificate.Repositories.Interfaces;
using QRCoder;


namespace O2.Certificate.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    
    [Route("api/v{v:apiVersion}/apps/certificates")]
    public class CertificatesController : ControllerBase
    {
        #region Fields

        private readonly ICertificateBaseRepository<O2CCertificate> _certificatesBaseRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        #endregion


        #region Ctors

        public CertificatesController(
            ICertificateBaseRepository<O2CCertificate> certificatesBaseRepository,
            IConfiguration config,
            IMapper mapper)
        {
            _certificatesBaseRepository = certificatesBaseRepository;
            _config = config;
            _mapper = mapper;
        }

        #endregion


        #region Methods V1_0

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("add_update")]
        [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public async Task<IActionResult> AddUpdate_V1_0(O2CCertificateForCreateDto o2CCertificationForCreateDto,
            ApiVersion apiVersion)
        {
            try
            {
                var createCertificate = MappingCertificate(o2CCertificationForCreateDto);

                var createEntity = await _certificatesBaseRepository.AddOrUpdateAsync(createCertificate);

                if (createEntity == null)
                    return StatusCode(500);

                var certificateToReturn = _mapper.Map<O2CCertificateForReturnDto>(createEntity);
                return CreatedAtAction(nameof(Get_V1_0),
                    new {id = certificateToReturn.Id, actualInfo = false, v = apiVersion.ToString()},
                    certificateToReturn);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private O2CCertificate MappingCertificate(O2CCertificateForCreateDto o2CCertificationForCreateDto)
        {
            var list = new List<O2CCertificateLocation>();
            var locationList = _mapper.Map<List<O2CLocation>>(o2CCertificationForCreateDto.Locations);
            var photos = _mapper.Map<List<O2CPhoto>>(o2CCertificationForCreateDto.Photos);

            var contacts = _mapper.Map<List<O2CContact>>(o2CCertificationForCreateDto.Contacts);

            list.AddRange(locationList
                .Select(item => new O2CCertificateLocation() {O2CLocation = item}));

            var createCertificate = new O2CCertificate()
            {
                Id = o2CCertificationForCreateDto.Id,

                Serial = o2CCertificationForCreateDto.Serial,
                ShortNumber = o2CCertificationForCreateDto.ShortNumber,
                Number = o2CCertificationForCreateDto.Number,

                Firstname = o2CCertificationForCreateDto.Firstname,
                Lastname = o2CCertificationForCreateDto.Lastname,
                Middlename = o2CCertificationForCreateDto.Middlename,

                Education = o2CCertificationForCreateDto.Education,
                Visible = o2CCertificationForCreateDto.Visible,
                Lock = o2CCertificationForCreateDto.Lock,
                DateOfCert = o2CCertificationForCreateDto.DateOfCert,

                Locations = list,
                Photos = photos,
                Contacts = contacts
            };
            return createCertificate;
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public async Task<IActionResult> Get_V1_0(Guid id, ApiVersion apiVersion)
        {
            var certificate = await _certificatesBaseRepository.GetAsync(id);
            var eventsToReturn = _mapper.Map<O2CCertificateForReturnDto>(certificate);
            return Ok(eventsToReturn);
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<O2CCertificateForListDto>))]
        public async Task<IActionResult> GetAll_V1_0([FromQuery] CertificateParam certificateParam,
            ApiVersion apiVersion,
            bool actualInfo = false)
        {
            var certifications = await _certificatesBaseRepository.GetAllAsync(certificateParam, actualInfo);
            var eventsToReturn = _mapper.Map<List<O2CCertificateForListDto>>(certifications);
            Response.AddPagination(certifications.CurrentPage, certifications.PageSize,
                certifications.TotalCount, certifications.TotalPages);
            return Ok(eventsToReturn);
        }

        // [AllowAnonymous]
        // [MapToApiVersion("1.0")]
        // [HttpGet]
        // [ProducesResponseType(200, Type = typeof(List<O2CCertificateForListDto>))]
        // public async Task<IActionResult> GetAll_V1_0(ApiVersion apiVersion,
        //     bool actualInfo = false)
        // {
        //     var certifications = await _certificatesBaseRepository.GetAllAsync(actualInfo);
        //     var eventsToReturn = _mapper.Map<List<O2CCertificateForListDto>>(certifications);
        //     return Ok(eventsToReturn);
        // }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("delete/{id}")]
        [ProducesResponseType(200, Type = typeof(O2CCertificateForListDto))]
        public async Task<IActionResult> Delete_V1_0(Guid id, ApiVersion apiVersion)
        {
            var certificate = await _certificatesBaseRepository.DeleteAsync(id);
            var eventsToReturn = _mapper.Map<O2CCertificateForListDto>(certificate);
            return Ok(eventsToReturn);
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("import")]
        [ProducesResponseType(200, Type = typeof(List<O2CCertificateForListDto>))]
        public async Task<IActionResult> Import_V1_0([FromForm] O2CCertificateImportDto o2CCertificateImportDto)
        {
            var path = o2CCertificateImportDto.File.FileName;
            await using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await o2CCertificateImportDto.File.CopyToAsync(fileStream);
            }

            var str = System.IO.File.ReadAllText(path);
            var certificationForListDto = JsonConvert.DeserializeObject<List<O2CCertificateForCreateDto>>(str);

            var listCertificates = new List<O2CCertificate>();
            foreach (var item in certificationForListDto)
            {
                var createCertificate = MappingCertificate(item);
                listCertificates.Add(createCertificate);
            }
            // var list = _mapper.Map<List<O2CCertificate>>(certificationForListDto);
            
            // import photos
            PhotoHelper.LoadCertificates(listCertificates);
            
            var certifications =
                await _certificatesBaseRepository.AddRangeAsync(listCertificates, o2CCertificateImportDto.CleanData);
            var certificatesReturn = _mapper.Map<List<O2CCertificateForListDto>>(certifications);
            return Ok(certificatesReturn);
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("load_photo")]
        public async Task<IActionResult> LoadPhoto_V1_0()
        {
            return Ok();
        }

        // This method is for converting bitmap into a byte array
        private static byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }


        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("delete_photo/{id}")]
        public IActionResult DeletePhoto_V1_0(Guid id)
        {
            return Ok();
        }

        #endregion

        public static Bitmap ByteArrayToImage(byte[] source)
        {
            using (var ms = new MemoryStream(source))
            {
                return new Bitmap(ms);
            }
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("generate_terapia/{id}")]
        public async Task<IActionResult> Generate_terapia_V1_0(Guid id, long uniDate)
        {
            var certifications = await _certificatesBaseRepository.GetAllAsync();
            var certification = certifications.SingleOrDefault(x => x.Id == Guid.Parse(id.ToString()));
            Bitmap bitmapClear;
             // foreach (var certification in certifications)
             // {
                var image = Image.FromFile("Files/template-hypnos.png");

                var cert = new Bitmap(image);


                bitmapClear = new Bitmap(cert.Width, cert.Height);
                using (Graphics graphics = Graphics.FromImage(bitmapClear))
                {
                    graphics.DrawImage(cert, 0, 0, cert.Width, cert.Height);

                    var fio = certification.Lastname + " " + certification.Firstname +
                              " " + (string.IsNullOrEmpty(certification.Middlename)
                                  ? " "
                                  : " " + certification.Middlename); //certification.FIO.Replace(" ", "\r\n");
                    ImageHelper.AddedText(fio, cert, graphics, "Arial", 95, Brushes.Black, 1265, StringAlignment.Center,
                        StringAlignment.Center);

                    var date = uniDate.ConvertToDateTime();
                    ImageHelper.AddedTextX(date.ToString("dd.MM.yyyy"), cert, graphics, "Arial", 68, Brushes.Black,
                        2090,
                        2539, StringAlignment.Center, StringAlignment.Center);
                    ////added serial_number
                    //ImageHelper.AddedText(certification.SerialNumber, cert, graphics, "Arial", 45, Brushes.Red, 2727, StringAlignment.Center, StringAlignment.Near);

                    //graphics.DrawImage(qrCodeImage, 1070, 2901, qrCodeImage.Width, qrCodeImage.Height);
                    graphics.Save();
                    // bitmapClear.Save(fio + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            // }

             var bitmapBytes = ImageHelper.BitmapToBytes(bitmapClear); //Convert bitmap into a byte array

             return File(bitmapBytes, "image/jpeg");
                // return Ok();
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("generate/{id}")]
        public async Task<IActionResult> Generate_V1_0(Guid id)
        {
            var certifications = await _certificatesBaseRepository.GetAllAsync();
            var certification = certifications.SingleOrDefault(x => x.Id == Guid.Parse(id.ToString()));
            // var certification = new O2CCertificate()
            // {
            //     Serial = "А",
            //     Number = "A0086061820",
            //     Firstname = "Анна",
            //     Lastname = "Янушкевич",
            //     Middlename = "Леонидовна",
            //     DateOfCert = new DateTime(2020, 02, 28).ConvertToUnixTime()
            // };

            var content = " Центр Гипноза Антона Маркова" +
                          " https://antonmarkov.com/obuchenie/baza-sertifikatov-pfr/" +
                          " Сертификат " + certification.Serial + certification.Number +
                          "; Дата сертификации: " + certification.DateOfCert.Value.ConvertToDateTime();


            var image = Image.FromFile("Files/pft_template_cert.png");
            var cert = new Bitmap(image);

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(
                " Центр Гипноза Антона Маркова" + " https://antonmarkov.com/obuchenie/baza-sertifikatov-pfr/" +
                " Сертификат " + certification.Serial + certification.Number +
                "; Дата сертификации: " + certification.DateOfCert.Value.ConvertToDateTime(),
                QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            var qrCodeImage = qrCode.GetGraphic(5);


            var bitmapClear = new Bitmap(cert.Width, cert.Height);
            using (var graphics = Graphics.FromImage(bitmapClear))
            {
                graphics.DrawImage(cert, 0, 0, cert.Width, cert.Height);

                var fio = certification.Lastname + " " + certification.Firstname +
                          " " + (string.IsNullOrEmpty(certification.Middlename)
                              ? ""
                              : "\n" + certification.Middlename); //certification.FIO.Replace(" ", "\r\n");
                var fullNumber = certification.Serial + certification.Number;
                ImageHelper.AddedText(fio, cert, graphics, "Arial", 95, Brushes.Black, 1550,
                    StringAlignment.Center, StringAlignment.Center);

                //added serial_number
                ImageHelper.AddedText(fullNumber, cert, graphics, "Arial", 45, Brushes.Red, 2727,
                    StringAlignment.Center, StringAlignment.Near);

                graphics.DrawImage(qrCodeImage, 1070, 2901, qrCodeImage.Width, qrCodeImage.Height);
                graphics.Save();
            }

            var bitmapBytes = ImageHelper.BitmapToBytes(bitmapClear); //Convert bitmap into a byte array

            return File(bitmapBytes, "image/jpeg");
        }

        /// <summary>
        /// Get Certificates with new Number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [MapToApiVersion("1.1")]
        [HttpGet("generate/{id}")]
        public async Task<IActionResult> Generate_V1_1(Guid id)
        {
            var certifications = await _certificatesBaseRepository.GetAllAsync();
            var certification = certifications.SingleOrDefault(x => x.Id == Guid.Parse(id.ToString()));
            var content = " Центр Гипноза Антона Маркова" +
                          " https://antonmarkov.com/obuchenie/baza-sertifikatov-pfr/" +
                          " Сертификат " + certification.Serial + certification.Number +
                          "; Дата сертификации: " + certification.DateOfCert;


            var image = Image.FromFile("Files/pft_template_cert.png");
            var cert = new Bitmap(image);

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(
                " Центр Гипноза Антона Маркова" + " https://antonmarkov.com/obuchenie/baza-sertifikatov-pfr/" +
                " Сертификат " + certification.Serial + certification.Number +
                "; Дата сертификации: " + certification.DateOfCert,
                QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            var qrCodeImage = qrCode.GetGraphic(5);


            var bitmapClear = new Bitmap(cert.Width, cert.Height);
            using (var graphics = Graphics.FromImage(bitmapClear))
            {
                graphics.DrawImage(cert, 0, 0, cert.Width, cert.Height);

                var fio = certification.Lastname + " " + certification.Firstname +
                          " " + (string.IsNullOrEmpty(certification.Middlename) ? "" : "\n" + certification.Middlename);
                var fullNumber = certification.Serial + certification.Number;
                ImageHelper.AddedText(fio, cert, graphics, "Arial", 95, Brushes.Black, 1550,
                    StringAlignment.Center, StringAlignment.Center);

                //added serial_number
                ImageHelper.AddedText(fullNumber, cert, graphics, "Arial", 45, Brushes.Red, 2727,
                    StringAlignment.Center, StringAlignment.Near);

                graphics.DrawImage(qrCodeImage, 1070, 2901, qrCodeImage.Width, qrCodeImage.Height);
                graphics.Save();
            }

            var bitmapBytes = ImageHelper.BitmapToBytes(bitmapClear); //Convert bitmap into a byte array

            return File(bitmapBytes, "image/jpeg");
        }
    }
}