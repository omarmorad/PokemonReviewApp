using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repository;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : Controller
    {
        private readonly IPokemonRepository _pokemonrepository;
        public readonly IReviewRepository _reviewRepository;
        public readonly IOwnerRepository _ownerRepository;
        private readonly IMapper _mapper;

        public PokemonController(IPokemonRepository pokemonrepository 
            , IMapper mapper , 
            IOwnerRepository ownerRepository,
            IReviewRepository reviewRepository)
        {
            _pokemonrepository = pokemonrepository; 
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public IActionResult GetPokemons()
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(_pokemonrepository.GetPokemons());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(pokemons);
        }
        [HttpGet("{pokeId}")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(int pokeId) 
        {
            if(!_pokemonrepository.PokemonExists(pokeId))
                return NotFound();

            var pokemon = _mapper.Map<PokemonDto>(_pokemonrepository.GetPokemon(pokeId));
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemon);
        }
        [HttpGet("{pokeId}/rating")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonRating( int pokeId)
        {
            if(!_pokemonrepository.PokemonExists(pokeId))
                return NotFound();
            var rating = _pokemonrepository.GetPokemonRatings(pokeId);
            if(!ModelState.IsValid)
                return BadRequest();
            return Ok(rating);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreatePokemon([FromQuery] int ownerId, [FromQuery] int catId, [FromBody] PokemonDto pokemonCreate)
        {
            if (pokemonCreate == null)
                return BadRequest(ModelState);

            var pokemons = _pokemonrepository.GetPokemons().
                Where(c=> c.Name.Trim().ToUpper() == pokemonCreate.Name.TrimEnd().ToUpper()).FirstOrDefault();

            if (pokemons != null)
            {
                ModelState.AddModelError("", "Owner already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var pokemonMap = _mapper.Map<Pokemon>(pokemonCreate);


            if (!_pokemonrepository.CreatePokemon(ownerId, catId, pokemonMap))
            {
                ModelState.AddModelError("", "Something went wrong while savin");
                return StatusCode(500, ModelState);
            }

            return Ok("Successfully created");
        }

        [HttpPut("{pokeId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdatePokemon( int pokeId, [FromQuery] int ownerId,
        [FromQuery] int catId,[FromBody] PokemonDto updatedpokemon)
        {
            if (updatedpokemon == null)
                return BadRequest(ModelState);

            if (pokeId != updatedpokemon.Id)
                return BadRequest(ModelState);

            if (!_pokemonrepository.PokemonExists(pokeId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var pokemonMap = _mapper.Map<Pokemon>(updatedpokemon);

            if (!_pokemonrepository.UpdatePokemon(ownerId , catId,pokemonMap))
            {
                ModelState.AddModelError("", "Something went wrong updating Pokemon");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
        [HttpDelete("{pokeId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeletePokemon(int pokeId)
        {
            if (!_pokemonrepository.PokemonExists(pokeId))
            {
                return NotFound();
            }

            //var reviewsToDelete = _reviewRepository.GetReviewsOfAPokemon(pokeId);
            var pokemonToDelete = _pokemonrepository.GetPokemon(pokeId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //if (!_reviewRepository.DeleteReviews(reviewsToDelete.ToList()))
            //{
            //    ModelState.AddModelError("", "Something went wrong when deleting reviews");
            //}

            if (!_pokemonrepository.DeletePokemon(pokemonToDelete))
            {
                ModelState.AddModelError("", "Something went wrong deleting owner");
            }

            return NoContent();
        }



    }
}
;
