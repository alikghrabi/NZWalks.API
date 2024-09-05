using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.CustomActionFilters;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;
using System.Text.Json;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly NZWalksDbContext dbContext;
        private readonly IRegionRepository regionRepository;
        private readonly IMapper mapper;
        private readonly ILogger<RegionsController> logger;


        public RegionsController(NZWalksDbContext dbContext, IRegionRepository regionRepository, IMapper mapper, ILogger<RegionsController> logger)
        {
            this.dbContext = dbContext;
            this.regionRepository = regionRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        /*        [HttpGet]
                public IActionResult GetAll()
                {
                    var regions = new List<Region>
                    {
                        new Region
                        {
                            Id = Guid.NewGuid(),
                            Name = "Aukland Region",
                            Code = "AKL",
                            RegionImageUrl = "https://www.pexels.com/photo/sky-tower-in-auckland-831910/"
                        },
                        new Region
                        {
                            Id = Guid.NewGuid(),
                            Name = "Wellington Region",
                            Code = "WLG",
                            RegionImageUrl = "https://www.pexels.com/photo/sky-tower-in-auckland-831910/"
                        },
                    };
                    return Ok(regions);
                }*/

        [HttpGet]
        // [Authorize(Roles = "Reader, Writer")]
        public async Task<IActionResult> GetAll()
        {
            logger.LogInformation("Regions: GetAll action method was invoked.");
            logger.LogWarning("This is a warning log");

            // Get Data From Database - Domain Models
            var regions = await regionRepository.GetAllAsync();

            // Map Domain Models to DTOs
            var regionsDto = mapper.Map<List<RegionDto>>(regions);

            logger.LogInformation($"Regions: Fetched Data:\n{JsonSerializer.Serialize(regions)}");

            // Return DTOs
            return Ok(regionsDto);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Roles = "Reader, Writer")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            // Get Region Domain Model From the Database
            var region = await regionRepository.GetByIdAsync(id);

            if (region == null)
            {
                return BadRequest("No Region was found.");
            }

            // Map/Convert Region Domail Model to DTO
            var regionDto = mapper.Map<RegionDto>(region);

            // Return DTO back to client
            return Ok(regionDto);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Create([FromBody] AddRegionRequestDto addRegionRequestDto)
        {
                // Map or Convert DTO to Domain Model
                var regionDomainModel = mapper.Map<Region>(addRegionRequestDto);

                // Use Domain Model to Create Region
                await regionRepository.CreateAsync(regionDomainModel);
                await dbContext.SaveChangesAsync();

                // Map Domain Model Back to DTO
                var regionDto = mapper.Map<RegionDto>(regionDomainModel);
                return CreatedAtAction(nameof(GetById), new { id = regionDomainModel.Id }, regionDomainModel);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        [ValidateModel]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateRegionRequestDto updateRegionRequestDto)
        {
                // Map DTO to Domain Model
                var regionDomainModel = mapper.Map<Region>(updateRegionRequestDto);
                regionDomainModel = await regionRepository.UpdateAsync(id, regionDomainModel);

                if (regionDomainModel == null)
                {
                    return NotFound();
                }

                // Convert Domain Model to DTO
                var regionDto = mapper.Map<RegionDto>(regionDomainModel);

                return Ok(regionDto);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var regionDomainModel = await regionRepository.DeleteAsync(id);
            if (regionDomainModel == null)
            {
                return NotFound();
            }

            // Map Domain Model Back to DTO
            var regionDto = mapper.Map<RegionDto>(regionDomainModel);

            return Ok(regionDto);
        }
    }
}