using BookShop.Context;
using BookShop.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShop.API.Controllers
{
    /// <summary>
    /// Controller for managing book operations in the BookShop API.
    /// Provides endpoints for retrieving and creating books.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the BookController class.
        /// </summary>
        /// <param name="context">The application database context for accessing book data.</param>
        public BookController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all books from the database.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// Returns an IEnumerable of Book objects if successful.
        /// Returns a 200 OK response with the list of all books.
        /// </returns>
        /// <remarks>
        /// This endpoint does not require authentication.
        /// Example: GET /book
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksAsync()
        {
            return await _context.Books.Include(book => book.Author).ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific book by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the book to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// Returns the Book object with the specified ID if found.
        /// Returns a 200 OK response with the book details if successful.
        /// Returns a 404 Not Found response if no book with the specified ID exists.
        /// </returns>
        /// <remarks>
        /// This endpoint does not require authentication.
        /// Example: GET /book/1
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookByIdAsync(int id)
        {
            var book = await _context.Books.Include(book => book.Author).FirstOrDefaultAsync(book => book.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            return book;
        }

        /// <summary>
        /// Creates a new book and adds it to the database.
        /// </summary>
        /// <param name="book">The Book object containing the details of the book to be added.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// Returns the newly created Book object with the generated ID.
        /// Returns a 200 OK response with the saved book details if successful.
        /// Returns a 400 Bad Request response if the book object is invalid.
        /// </returns>
        /// <remarks>
        /// This endpoint requires authentication via Bearer token in the Authorization header.
        /// The book object must contain:
        /// - Title (string, required): The title of the book
        /// - Author (object, required): The book's author
        /// - PublishedYear (integer, required): The publication year
        /// - Price (decimal, required): The price of the book
        /// - ISBN (string, required): The ISBN
        /// Example: POST /book with a complete book JSON object
        /// 
        /// Required Header:
        /// Authorization: Bearer {token}
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return Ok(book);
        }

    }
}
