using NZWalks.API.Data;
using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _accessor;
        private readonly NZWalksDbContext _dbContext;
        public ImageRepository(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor accessor, NZWalksDbContext dbContext)
        {
            _environment = webHostEnvironment;
            _accessor = accessor;
            _dbContext = dbContext;
        }
        public async Task<Image> Upload(Image image)
        {
            var localFilePath = Path.Combine(_environment.ContentRootPath, "Images", $"{image.FileName}{image.FileExtension}");

            // Upload Image to Local Path
            using var stream = new FileStream(localFilePath, FileMode.Create);
            await image.File.CopyToAsync(stream);

            // https://localhost:1234/images/image.jpg
            var urlFilePath = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}{_accessor.HttpContext.Request.PathBase}/images/{image.FileName}{image.FileExtension}";

            image.FilePath = urlFilePath;

            await _dbContext.Images.AddAsync(image);
            await _dbContext.SaveChangesAsync();

            return image;
        }
    }
}
