using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using O2.Business.Repositories.Interfaces;
using AutoMapper;
using Newtonsoft.Json;
using O2.Black.Toolkit.Core;
using O2.Business.API.DTOs.O2Ev;
using O2.Business.Data.Models.O2Ev;

namespace O2.Business.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/apps/events")]
    public class EventsController : ControllerBase
    {
        #region Fields

        private readonly IEventBaseRepository<O2EvEvent> _eventsBaseRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        #endregion


        #region Ctors

        public EventsController(
            IEventBaseRepository<O2EvEvent> eventsBaseRepository,
            IConfiguration config,
            IMapper mapper)
        {
            _eventsBaseRepository = eventsBaseRepository;
            _config = config;
            _mapper = mapper;
        }

        #endregion


        #region Methods V1_0

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("add_update")]
        [ProducesResponseType(201, Type = typeof(O2EvEventForCreateDto))]
        public async Task<IActionResult> AddUpdate_V1_0(O2EvEventForCreateDto o2EvEventForCreateDto,
            ApiVersion apiVersion)
        {
            // var createEvent = _mapper.Map<O2EvEvent>(o2EvEventForCreateDto);

            var createEvent = MappingEvent(o2EvEventForCreateDto);

            var o2EvPhoto = await PreparePhoto(createEvent);

            var createEntity = await _eventsBaseRepository.AddOrUpdateAsync(createEvent);
            createEntity = await _eventsBaseRepository.LoadPhoto(createEntity, o2EvPhoto);
            if (createEntity == null)
                return StatusCode(500);

            var eventsToReturn = _mapper.Map<O2EvEventReturnDto>(createEntity);

            return CreatedAtAction(nameof(Get_V1_0),
                new {id = eventsToReturn.Id, v = apiVersion.ToString()},
                eventsToReturn);
        }

        private O2EvEvent MappingEvent(O2EvEventForCreateDto o2EvEventForCreateDto)
        {
            var list = new O2EvMeta();
            // var locationList = _mapper.Map<O2EvMeta>(o2EvEventForCreateDto.Meta);
            list = new O2EvMeta()
            {
                LocationCountry = o2EvEventForCreateDto.Meta.Country,
                LocationRegion = o2EvEventForCreateDto.Meta.Region
            };

            var o2EvEvent = new O2EvEvent()
            {
                Title = o2EvEventForCreateDto.Title,
                ShortDescription = o2EvEventForCreateDto.ShortDescription,
                StartDate = o2EvEventForCreateDto.StartDate,
                EndDate = o2EvEventForCreateDto.EndDate,
                Meta = list
            };
            return o2EvEvent;
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<O2EvEventForListDto>))]
        public async Task<IActionResult> GetAll_V1_0(ApiVersion apiVersion, bool actualInfo = false, bool last = false,
            int countLast=0)
        {
            var events = await _eventsBaseRepository.GetAllAsync(actualInfo, last, countLast);
            var eventsToReturn = _mapper.Map<List<O2EvEventForListDto>>(events);
            return Ok(eventsToReturn);
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(O2EvEventReturnDto))]
        public async Task<IActionResult> Get_V1_0(Guid id, ApiVersion apiVersion)
        {
            var @event = await _eventsBaseRepository.GetAsync(id);
            var eventsToReturn = _mapper.Map<O2EvEventReturnDto>(@event);
            return Ok(eventsToReturn);
        }


        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("delete/{id}")]
        [ProducesResponseType(200, Type = typeof(O2EvEventForListDto))]
        public async Task<IActionResult> Delete_V1_0(Guid id, ApiVersion apiVersion)
        {
            var @event = await _eventsBaseRepository.DeleteAsync(id);
            var eventsToReturn = _mapper.Map<O2EvEventForListDto>(@event);
            return Ok(eventsToReturn);
        }


        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("export")]
        public async Task<IActionResult> Export_V1_0()
        {
            throw new Exception();
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("import")]
        [ProducesResponseType(200, Type = typeof(List<O2EvEventForListDto>))]
        public async Task<IActionResult> Import_V1_0(ApiVersion apiVersion,
            [FromForm] O2EvEventImportDto o2EvEventsImportDto)
        {
            var path = o2EvEventsImportDto.File.FileName;
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                await o2EvEventsImportDto.File.CopyToAsync(fileStream);
            }

            var str = System.IO.File.ReadAllText(path);

            var eventForListDtos = JsonConvert.DeserializeObject<List<O2EvEventForCreateDto>>(str);
            
            
            // var list = _mapper.Map<List<O2EvEvent>>(eventForListDtos);
            var list = eventForListDtos.Select(item => MappingEvent(item)).ToList();

            foreach (var item in list)
            {
                var o2EvPhoto = await PreparePhoto(item);
                item.Photos.Add(o2EvPhoto);
            }
            // for (int i = 0; i < list.Count; i++)
            // {
            //     var o2EvPhoto = await PreparePhoto(list[i]);
            //     list[i].Photos.Add(o2EvPhoto);
            // }

            var events = await _eventsBaseRepository.AddRangeAsync(list, o2EvEventsImportDto.CleanData);
            var eventsToReturn = _mapper.Map<List<O2EvEventForListDto>>(events);

            return Ok(eventsToReturn);
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("load_photo/{id}")]
        public async Task<IActionResult> LoadPhoto_V1_0(
            ApiVersion apiVersion,
            Guid id,
            [FromForm] O2EvEventPhotoDto o2EvEventPhotoDto)
        {
            var existEvent = await _eventsBaseRepository.GetAsync(id);
            if (existEvent == null)
            {
                return BadRequest();
            }

            var o2EvPhoto = await PreparePhoto(existEvent, o2EvEventPhotoDto);
            o2EvPhoto.IsMain = true;

            var updateEvent = await _eventsBaseRepository.LoadPhoto(existEvent, o2EvPhoto);

            if (updateEvent == null)
                return StatusCode(500);

            var eventsToReturn = _mapper.Map<O2EvEventForListDto>(existEvent);
            return CreatedAtAction(nameof(Get_V1_0),
                new {id = eventsToReturn.Id, actualInfo = false, v = apiVersion.ToString()},
                eventsToReturn);
        }

        private static async Task<O2EvPhoto> PreparePhoto(O2EvEvent existEvent,
            O2EvEventPhotoDto o2EvEventPhotoDto = null)
        {
            const string notImage = "not_image.jpg";
            const string path = "Files/" + notImage;

            var o2EvPhoto = new O2EvPhoto();

            //load default photo
            if (o2EvEventPhotoDto == null)
            {
                if (!System.IO.File.Exists(path))
                {
                    throw new Exception("File not found - " + path);
                }

                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    o2EvPhoto.FileName = existEvent.Id.ToString() + '_' + DateTime.Now.ConvertToUnixTime() +
                                         Path.GetExtension(notImage).ToLower();
                    o2EvPhoto.Url = await AzureBlobHelper.UploadFileToStorage(stream,
                        fileName: o2EvPhoto.FileName,
                        TypeTable.Events);
                    o2EvPhoto.IsMain = true;

                    return o2EvPhoto;
                }
            }

            //prepare file
            var file = o2EvEventPhotoDto.File;

            if (file.Length > 0)
            {
                using (Stream stream = file.OpenReadStream())
                {
                    o2EvPhoto.Url = await AzureBlobHelper.UploadFileToStorage(stream,
                        existEvent.Id.ToString() + '_' + DateTime.Now.ConvertToUnixTime() +
                        Path.GetExtension(notImage).ToLower(),
                        TypeTable.Events);
                    return o2EvPhoto;
                }
            }

            throw new Exception("File is empty");
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("delete_photo/{id}")]
        public async Task<IActionResult> DeletePhoto_V1_0()
        {
            return Ok();
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("preview")]
        public async Task<IActionResult> Preview_V1_0()
        {
            return Ok();
        }

        #endregion
    }
}