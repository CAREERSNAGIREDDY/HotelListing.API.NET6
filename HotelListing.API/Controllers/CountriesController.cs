using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using HotelListing.API.Models.Country;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Drawing;

namespace HotelListing.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public CountriesController(HotelListingDbContext context, IMapper mapper)
        {
            _context = context;
            this._mapper = mapper;
        }

        // GET: Countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            //By default scafolding start
            //if (_context.Countries == null)
            //{
            //    return NotFound();
            //}
            //  return await _context.Countries.ToListAsync();
            //By default scafolding end

            if (_context.Countries == null)
            {
                return NotFound();
            }
            var countries = await _context.Countries.ToListAsync();
            var records=_mapper.Map<List<GetCountryDto>>(countries);
            //return Ok(countries);
            return Ok(records);
        }

        // GET: Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            if (_context.Countries == null)
            {
                return NotFound();
            }
            //Before Refactor Start
            //var country = await _context.Countries.FindAsync(id);
            //Before Refactor End

            var country = await _context.Countries.Include(q => q.Hotels).FirstOrDefaultAsync(q => q.Id == id);

            if (country == null)
            {
                return NotFound();
            }

            var countryDto = _mapper.Map<CountryDto>(country);

            //return Ok(country);
            return Ok(countryDto);
        }

        // PUT: Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        #region Before Refactor for PUT request
        //public async Task<IActionResult> PutCountry(int id, Country country)
        //{
        //    if (id != country.Id)
        //    {
        //        return BadRequest("Invalid Record Id");
        //    }

        //    _context.Entry(country).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!CountryExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}
        #endregion

        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                return BadRequest("Invalid Record Id");
            }

            //_context.Entry(country).State = EntityState.Modified;
            var country = await _context.Countries.FindAsync(id);
            if(country==null)
            {
                return NotFound();
            }

            //remember the below line for update the record
            _mapper.Map(updateCountryDto, country);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        #region Before Refactor 
        //// POST: Countries
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Country>> PostCountry(Country country)
        //{
        //  if (_context.Countries == null)
        //  {
        //        //return Problem("Entity set 'HotelListingDbContext.Countries'  is null.");
        //        return NotFound();
        //  }
        //    _context.Countries.Add(country);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        //}
        #endregion

        // POST: Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountryDto)
        {
            if (_context.Countries == null)
            {
                //return Problem("Entity set 'HotelListingDbContext.Countries'  is null.");
                return NotFound();
            }
            #region Before Automapper Start
            //var country = new Country 
            //{ 
            //    Name= createCountry.Name,
            //    ShortName=createCountry.ShortName
            //};
            #endregion

            #region After Automapper Start
            var country = _mapper.Map<Country>(createCountryDto);
            #endregion

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: Countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            if (_context.Countries == null)
            {
                return NotFound();
            }
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountryExists(int id)
        {
            return (_context.Countries?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
