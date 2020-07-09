using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using O2.Black.Toolkit.Core;
using O2.Certificate.API.DTOs.O2C;
using O2.Certificate.API.Helper;

namespace O2.Certificate.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiVersion}/apps/helper")]
    public class HelperController: ControllerBase
    {
        #region ctor

        public HelperController()
        {
            
        }

        #endregion
        
        #region Methods V1_0

        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("CertificateNumberToUnixTime")]
        [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public async Task<long> GetUnixTime_V1_0(string certificateNumber)

        {
            var trueSerial = certificateNumber.Contains("A");
            if (trueSerial)
                certificateNumber = certificateNumber.Substring(1);

            var dateTime = HelperCertificate.GetDateCert(certificateNumber);
            var result = dateTime.ConvertToUnixTime();
            return result;
        }
        
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("migrate")]
        [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public IActionResult Migrate_V1_0(string password)

        {
            if (password != "#89_DangerSnake?") return StatusCode(500);
            HelperDBContext.Context.Database.Migrate();
            return Ok();

        }
        
        [AllowAnonymous]
        [MapToApiVersion("1.0")]
        [HttpPost("recreate")]
        [ProducesResponseType(200, Type = typeof(O2CCertificateForReturnDto))]
        public IActionResult ReCreate_V1_0(string password)
        {
            if (password != "#89_DangerSnake?") return StatusCode(500);
           HelperDBContext.Context.Database.EnsureDeleted();
           HelperDBContext.Context.Database.EnsureCreated();
            return Ok();
        }
        #endregion
    }
}