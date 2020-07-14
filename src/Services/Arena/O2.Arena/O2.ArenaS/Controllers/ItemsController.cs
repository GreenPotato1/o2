using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2.ArenaS.Data;
using O2.ArenaS.DTOs;
using O2.ArenaS.Mappings;
using O2.ArenaS.Services;

namespace O2.ArenaS.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/items")]
    public class ItemsController : ControllerBase
    {
        private readonly ICatalogItemService _catalogItemService;


        public ItemsController(ICatalogItemService catalogItemService)
        {
            _catalogItemService = catalogItemService;
        }
       

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("add_update")]
        // [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public async Task<IActionResult> AddAsync(ApiVersion apiVersion, CatalogItemViewModel model, CancellationToken ct)
        {
            model.Id = 0; //not needed when we move to MediatR
            var certificate = await _catalogItemService.AddAsync(model.ToServiceModel(), ct);
            return CreatedAtAction(nameof(GetByIdAsync_V1_0),
                    new { id = certificate.Id, actualInfo = false, v = apiVersion.ToString() },
                    certificate);
            //return CreatedAtAction(nameof(GetByIdAsync_V1_0),new { id = certificate.Id , ct =ct }, certificate.ToViewModel());
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("{id}")]
        // [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public async Task<IActionResult> GetByIdAsync_V1_0(ApiVersion apiVersion, int id, CancellationToken ct)
        {
            var catalogItem = await _catalogItemService.GetByIdAsync(id, ct);

            if (catalogItem == null)
            {
                return NotFound();
            }

            return Ok(catalogItem.ToViewModel());

        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet]
        // [ProducesResponseType(200, Type = typeof(List<O2CCertificateForListDto>))]
        public async Task<IActionResult> GetAll_V1_0(ApiVersion apiVersion, CancellationToken ct,bool actualInfo = false)
        {
            var result = await _catalogItemService.GetAllAsync(ct);
            return Ok(result.ToViewModel());
        }

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpGet("delete/{id}")]
        // [ProducesResponseType(200, Type = typeof(O2CCertificateForListDto))]
        public async Task<IActionResult> RemoveAsync(ApiVersion apiVersion, int id, CancellationToken ct)
        {
            await _catalogItemService.RemoveAsync(id, ct);

            return NoContent();
        }
    }
}