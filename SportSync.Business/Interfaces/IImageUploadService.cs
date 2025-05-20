// using Microsoft.AspNetCore.Http; // Không cần IFormFile nữa
using SportSync.Business.Dtos; // Thêm using cho ImageInputDto
using System.IO; // Thêm using cho Stream
using System.Threading.Tasks;

namespace SportSync.Business.Interfaces
{
    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string? PublicId { get; set; }
        public string? Url { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IImageUploadService
    {
        // Thay đổi từ IFormFile sang ImageInputDto hoặc các tham số riêng lẻ
        Task<ImageUploadResult> UploadImageAsync(ImageInputDto imageInput, string folderName);
        // Hoặc Task<ImageUploadResult> UploadImageAsync(Stream imageStream, string fileName, string contentType, string folderName);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
