using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2.ArenaS.Data;

namespace O2.ArenaS.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/items")]
    public class ItemsController : ControllerBase
    {
        private readonly ArenaContext _arenaContext;

        public ItemsController(ArenaContext arenaContext)
        {
            _arenaContext = arenaContext;
        }
        public ReadOnlyCollection<Item> Items { get; set; } = new ReadOnlyCollection<Item>(new List<Item>());

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("add_update")]
        // [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public IActionResult Add()
        {
            var createdItem = new Item()
            {
            };
            _arenaContext.Add(createdItem);
            _arenaContext.SaveChanges();
            return Ok(createdItem);
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("{id}")]
        // [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public async Task<IActionResult> Get_V1_0(Guid id, ApiVersion apiVersion)
        {
            return Ok();
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet]
        // [ProducesResponseType(200, Type = typeof(List<O2CCertificateForListDto>))]
        public async Task<IActionResult> GetAll_V1_0(ApiVersion apiVersion, bool actualInfo = false)
        {
            return Ok();
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("delete/{id}")]
        // [ProducesResponseType(200, Type = typeof(O2CCertificateForListDto))]
        public async Task<IActionResult> Delete_V1_0(Guid id, ApiVersion apiVersion)
        {
            return Ok();
        }
    }
}