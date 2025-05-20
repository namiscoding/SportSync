using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
// using Microsoft.AspNetCore.Http; // Không cần nữa
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportSync.Business.Interfaces;
using SportSync.Business.Dtos; // Thêm using cho ImageInputDto
using System;
using System.IO;
using System.Threading.Tasks;
using SportSync.Business.Settings;

namespace SportSync.Business.Services
{
    public class CloudinaryImageUploadService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryImageUploadService> _logger;

        public CloudinaryImageUploadService(
            IOptions<CloudinarySettings> cloudinaryConfig,
            ILogger<CloudinaryImageUploadService> logger)
        {
            _logger = logger;

            if (cloudinaryConfig == null || cloudinaryConfig.Value == null ||
                string.IsNullOrEmpty(cloudinaryConfig.Value.CloudName) ||
                string.IsNullOrEmpty(cloudinaryConfig.Value.ApiKey) ||
                string.IsNullOrEmpty(cloudinaryConfig.Value.ApiSecret))
            {
                _logger.LogError("Cloudinary settings are not configured properly.");
                throw new ArgumentNullException(nameof(cloudinaryConfig), "Cloudinary settings are missing or incomplete.");
            }

            Account account = new Account(
                cloudinaryConfig.Value.CloudName,
                cloudinaryConfig.Value.ApiKey,
                cloudinaryConfig.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<Interfaces.ImageUploadResult> UploadImageAsync(ImageInputDto imageInput, string folderName)
        {
            if (imageInput == null || imageInput.Content == null || imageInput.Content.Length == 0 || string.IsNullOrEmpty(imageInput.FileName))
            {
                _logger.LogWarning("UploadImageAsync: Invalid image input provided.");
                return new Interfaces.ImageUploadResult { Success = false, ErrorMessage = "Dữ liệu ảnh không hợp lệ." };
            }

            _logger.LogInformation("Attempting to upload image {FileName} to Cloudinary folder {FolderName}.", imageInput.FileName, folderName);

            try
            {
                // Đảm bảo stream ở vị trí đầu nếu nó đã được đọc trước đó
                if (imageInput.Content.CanSeek)
                {
                    imageInput.Content.Seek(0, SeekOrigin.Begin);
                }

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(imageInput.FileName, imageInput.Content),
                    Folder = folderName,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
                    return new Interfaces.ImageUploadResult { Success = false, ErrorMessage = uploadResult.Error.Message };
                }

                _logger.LogInformation("Image uploaded successfully to Cloudinary. PublicId: {PublicId}, Url: {Url}", uploadResult.PublicId, uploadResult.SecureUrl.ToString());
                return new Interfaces.ImageUploadResult
                {
                    Success = true,
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.SecureUrl.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Cloudinary upload for file {FileName}.", imageInput.FileName);
                return new Interfaces.ImageUploadResult { Success = false, ErrorMessage = "Đã xảy ra lỗi trong quá trình tải ảnh lên: " + ex.Message };
            }
            finally
            {
                // Quan trọng: Dispose stream nếu nó được tạo ra và quản lý bởi DTO hoặc service này.
                // Nếu stream đến từ IFormFile.OpenReadStream(), controller nên chịu trách nhiệm dispose nó.
                // imageInput.Content?.Dispose(); // Cân nhắc kỹ về việc ai sở hữu stream.
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            // ... (giữ nguyên logic DeleteImageAsync) ...
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("DeleteImageAsync: PublicId is null or empty.");
                return false;
            }

            _logger.LogInformation("Attempting to delete image with PublicId {PublicId} from Cloudinary.", publicId);
            var deletionParams = new DeletionParams(publicId);
            try
            {
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);
                if (deletionResult.Result?.ToLower() == "ok" || deletionResult.Result?.ToLower() == "not found")
                {
                    _logger.LogInformation("Image deletion from Cloudinary successful or image not found for PublicId {PublicId}. Result: {Result}", publicId, deletionResult.Result);
                    return true;
                }
                else
                {
                    _logger.LogError("Cloudinary image deletion failed for PublicId {PublicId}. Result: {Result}, Error: {Error}", publicId, deletionResult.Result, deletionResult.Error?.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during Cloudinary image deletion for PublicId {PublicId}.", publicId);
                return false;
            }
        }
    }
}
